#-------------------------------------------------------------------------------
# Name:        Resource_Tracker.py
# Purpose:     Extract Resource_Tracker data from SharePoint List and geocode
#
# Author:      Xiaoyi Zhang
# Created:     10/06/2021
# Copyright:   (c) GeoBISL 2021
# Python:      Python 3.x required for Requests module
#-------------------------------------------------------------------------------

import sys,os,socket,getpass
import configparser
#import datetime
import logger
import pandas as pd
import numpy as np
import csv
import requests
from sqlalchemy import create_engine, MetaData, Table, select, Boolean, Integer, BigInteger, String, Float, Numeric, DateTime

from office365.runtime.auth.authentication_context import AuthenticationContext
from office365.sharepoint.client_context import ClientContext
#from office365.sharepoint.files.file import File

# log
localPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), ''))
logPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), 'logs'))
logFile = os.path.join(logPath, "resourceTracker_log.txt")
if (not os.path.exists(logPath)):
    os.makedirs(logPath);
    
# csv
csvPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), 'data'))
csvFile = os.path.join(csvPath, "resourceTracker.csv")
csvFile_out = os.path.join(csvPath, "resourceTracker_out.csv")

def main():
    host = socket.gethostname()
    userName = getpass.getuser()

    global myLogger
    myLogger = logger.Logger(logFile, initalText="\nStarting Resource Tracker" + " - running on " + host + " by " + userName, overwriteLog=False)
    myLogger.writeLog("\n================================================================================================")
    myLogger.writeLog("Starting...", True)

    # settings.config
    settingsFile = os.path.join(localPath, "settings.ini")
                            
    # check settings.ini
    if (not os.path.isfile(settingsFile)):
        myLogger.writeLog("Missing settings.ini file.")
        sys.exit()

    config = configparser.ConfigParser()
    config.read(settingsFile)

    sharepoint_url = config.get("INPUT", "sharepoint_url")
    client_id = config.get("INPUT", "client_id")
    client_secret = config.get("INPUT", "client_secret")
    sp_list = config.get("INPUT", "sp_list")
    geocodingService_url = ("INPUT", "geocodingService_url")
    server_name = config.get("INPUT", "server_name")
    db_name = config.get("INPUT", "db_name")
    table_name = config.get("INPUT", "table_name")
    
    status_1 = getShapePointData(sharepoint_url, client_id, client_secret, sp_list)
    if status_1:
        status_2 = geoCoding(geocodingService_url)
        if status_2:
            status_3 = exportToSQLServer(csvFile_out, server_name, db_name, table_name)
            if status_3:
                myLogger.writeLog("Finished " + str(myLogger.getTimeChange()) + " seconds", True)
            else:
                myLogger.writeLog("Error(exportToSQLServer) " + str(myLogger.getTimeChange()) + " seconds", True)
        else:
            myLogger.writeLog("Error(geoCoding) " + str(myLogger.getTimeChange()) + " seconds", True)
    else:
        myLogger.writeLog("Error(getShapePointData) " + str(myLogger.getTimeChange()) + " seconds", True)
        

def geoCoding(geocodingService_url):
    try:
        df = pd.read_csv(csvFile)
        df['OBJECTID'] = -1
        df['Deployed_x'] = -1
        df['Deployed_y'] = -1
        df = df.astype({'OBJECTID': np.integer})
        df = df.astype({'Deployed_x': np.float})
        df = df.astype({'Deployed_y': np.float})
        
        #print(df['Comments'])
        for index, row in df.iterrows():
            street = row['Deployed_Street']
            city = row['Current_Location']
            state = row['Deployed_State']
            zipcode = row['Deployed_Zip']
            
            address = street + "," + city + "," + state + " " + str(zipcode)
            #print(address)
            #address = comments.split(",",1)[1]
            x, y = findAddress(address, geocodingService_url)
            df.at[index,'OBJECTID'] = index
            df.at[index,'Deployed_x'] = x
            df.at[index,'Deployed_y'] = y
    
        df.to_csv(csvFile_out, index=False)        

        return True
    except:
        return False
        
def findAddress(address, geocodingService_url):
    try:
        outSR = "4326"
        geocodingService_url = "https://gis.va.gov/server/rest/services/TOOLS/HSIP_Navteq_2015_Composite/GeocodeServer/findAddressCandidates"
        #print(address)
        address_new = address.strip().replace(' ', '+').replace(',', '%2C+')
        #print(address_new)
        lookup = requests.get(geocodingService_url + "?SingleLine=" + address_new + "&outSR=" + outSR + "&maxLocations=1&f=pjson")
        data = lookup.json()
        x = data['candidates'][0]['location']['x']
        y = data['candidates'][0]['location']['y']
        
        return x, y
    except:
        return -1, -1
    
    
def getShapePointData(sharepoint_url, client_id, client_secret, sp_list):
    try:
        app_settings = {
                'url': sharepoint_url,
                'client_id': client_id,
                'client_secret': client_secret
                }
        
        context_auth = AuthenticationContext(url=app_settings['url'])
        context_auth.acquire_token_for_app(client_id=app_settings['client_id'], client_secret=app_settings['client_secret'])
        
        ctx = ClientContext(app_settings['url'], context_auth)
    
        #
        sp_lists = ctx.web.lists
        s_list = sp_lists.get_by_title(sp_list)
        list_items = s_list.get_items()
        ctx.load(list_items)
        ctx.execute_query()
    
        with open(csvFile, 'w') as f:
            fields = list_items[0].properties.keys()
            w = csv.DictWriter(f, fields)
            w.writeheader()
            for item in list_items:
                w.writerow(item.properties)
            
        #
        df = pd.read_csv(csvFile)
        #cols = df.columns.values.tolist()
    
        df.drop(['FileSystemObjectType', 'Id', 'ServerRedirectedEmbedUri', 'ServerRedirectedEmbedUrl', 'ContentTypeId', 
                    'ID', 'Modified', 'Created', 'AuthorId', 'EditorId', 'OData__UIVersionString', 'Attachments', 'GUID'], axis=1, inplace=True)
    
        df.to_csv(csvFile, index=False)
        
        return True
    except:
        return False        


def exportToSQLServer(csvFile_out, server_name, db_name, table_name):
    try:
        df = pd.read_csv(csvFile_out, encoding = "ISO-8859-1") 
        
        #
        engine = create_engine('mssql://' + server_name + '/' + db_name + '?trusted_connection=yes&driver=SQL+Server')
    
    
        # column type
        columnType = {
                        'OBJECTID':       Integer,
                        'Title':         String(length=100),
                        'ComplianceAssetId':           String(length=100),
                        'Asset_Available': String(length=100),
                        'Request_Made':        String(length=100),
                        'Request_Approved':        String(length=100),
                        'Asset_Departed':         String(length=100),
                        'Asset_Arrived':          String(length=100),
                        'Demob':           String(length=100),
                        'Resource':       String(length=100),
                        'Qty':            String(length=100),
                        'Resource_From':          String(length=500),
                        'Current_Location':        String(length=500),
                        'Status':          String(length=100),
                        'Request_x0020_':          String(length=100),
                        'Demob2':          String(length=100),
                        'Comments':          String(length=100),
                        'LOG_POC':          String(length=100),
                        'LOG_Phone':          String(length=100),
                        'LOG_Email':          String(length=100),
                        'Driver_Name':          String(length=100),
                        'Driver_Cell':          String(length=100),
                        'POC':          String(length=100),
                        'State':          String(length=100),
                        'Deployed_State':          String(length=100),
                        'Home_Street':          String(length=100),
                        'Home_Zip':          String(length=100),
                        'Deployed_Street':          String(length=100),
                        'Deployed_Zip':          String(length=100),
                        'Deployed_x':      Numeric(precision=38,scale=8),
                        'Deployed_y':       Numeric(precision=38,scale=8)
                        }
        
        #
        df.to_sql(name=table_name,con=engine, index=False, if_exists='replace', dtype=columnType)
    
        #
        with engine.connect() as conn, conn.begin():
            sql = "ALTER TABLE " + table_name + " ADD geometry Geometry"
            conn.execute(sql)
    
            #
            sql = "UPDATE "  + table_name + " SET [geometry] = geometry::STGeomFromText('POINT('+convert(varchar(20),Deployed_x)+' '+convert(varchar(20),Deployed_y)+')',4326)"
            conn.execute(sql)
    
            #
            #sql = "CREATE INDEX index_id ON "  + table_name + " (Clinic)"
            #conn.execute(sql)
    
    
            #
            sql = "ALTER TABLE " + table_name + " ALTER COLUMN OBJECTID INT NOT NULL"
            conn.execute(sql)        
    
            #
            sql = "ALTER TABLE " + table_name + " ADD CONSTRAINT PK_OBJECTID_POWERWEATHER PRIMARY KEY CLUSTERED (OBJECTID)"
            conn.execute(sql)        
    
            #
            sql = "CREATE SPATIAL INDEX index_spatial ON " + table_name + " (geometry) USING GEOMETRY_AUTO_GRID WITH (BOUNDING_BOX=(xmin=-164, ymin=17,xmax=-65, ymax=65));"
            conn.execute(sql)        
    
        #
        del df
        del engine
        
        return True
    except:
        return False        

if __name__ == '__main__':
    main()
