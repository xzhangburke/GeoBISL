# -*- coding: utf-8 -*-
"""
get 7 days NCA App users

@author: VHAISAZHANGX0
"""

import os,sys,socket,getpass
import time
import glob
import pandas as pd

import configparser
import logger

# log
localPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), ''))
logPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), 'logs'))
logFile = os.path.join(logPath, "iislogparse_log.txt")
if (not os.path.exists(logPath)):
    os.makedirs(logPath);
    
def main():
    host = socket.gethostname()
    userName = getpass.getuser()

    global myLogger
    myLogger = logger.Logger(logFile, initalText="\nIISLogParse()" + " - running on " + host + " by " + userName, overwriteLog=False)
    myLogger.writeLog("\n===============================================================================================================================")

    # settings.config
    settingsFile = os.path.join(localPath, "settings.ini")

    # check settings.ini
    if (not os.path.isfile(settingsFile)):
        myLogger.writeLog("Missing settings.ini file.")
        sys.exit()

    config = configparser.ConfigParser()
    config.read(settingsFile)

    # input
    IISLogFolder1 = config.get("INPUT", "logFolder1")
    IISLogFolder2 = config.get("INPUT", "logFolder2")

    dayRange_user = config.get("INPUT", "dayRange_user")
    dayRange_summary = config.get("INPUT", "dayRange_summary")

    query_WebService = config.get("INPUT", "query_WebService_2")
    query_restService_site = config.get("INPUT", "query_restService_site")
    query_restService_gravesite = config.get("INPUT", "query_restService_gravesite")

    query_WebService_2 = config.get("INPUT", "query_WebService_2")
    query_restService_site_2 = config.get("INPUT", "query_restService_site_2")
    query_restService_gravesite_2 = config.get("INPUT", "query_restService_gravesite_2")
    
    # output
    csvFile_user = config.get("OUTPUT", "csvFile_user")
    csvFile_site = config.get("OUTPUT", "csvFile_site")
    
    jsonFile_user = config.get("OUTPUT", "jsonFile_user")
    jsonFile_site = config.get("OUTPUT", "jsonFile_site")
    
    csvFile_summary_sites = config.get("OUTPUT", "csvFile_summary_sites")
    csvFile_summary_users  = config.get("OUTPUT", "csvFile_summary_users")
    csvFile_summary_gravesites  = config.get("OUTPUT", "csvFile_summary_gravesites")
    csvFile_summary_numCountByGravesite  = config.get("OUTPUT", "csvFile_summary_numCountByGravesite")
    csvFile_summary_users_unique_7days  = config.get("OUTPUT", "csvFile_summary_users_unique_7days")
    csvFile_summary_users_unique_30days  = config.get("OUTPUT", "csvFile_summary_users_unique_30days")


    jsonFile_summary_sites = config.get("OUTPUT", "jsonFile_summary_sites")
    jsonFile_summary_users = config.get("OUTPUT", "jsonFile_summary_users")
    jsonFile_summary_gravesites = config.get("OUTPUT", "jsonFile_summary_gravesites")
    jsonFile_summary_numCountByGravesite  = config.get("OUTPUT", "jsonFile_summary_numCountByGravesite")


    myLogger.writeLog("Start...", True)

    myLogger.writeLog("IIS Log Folder = " + IISLogFolder1)
    myLogger.writeLog("IIS Log Folder = " + IISLogFolder2)
    myLogger.writeLog("Day Range - user = " + dayRange_user)
    
    #myLogger.writeLog("OUTPUT - user = " + jsonFile_user)
    #myLogger.writeLog("Day Range - summary = " + dayRange_summary)
    #myLogger.writeLog("OUTPUT - summary = " + jsonFile_summary_sites)

        
    # modify to handle all log fields present in your logs
    current_time = time.time()
    
    log_field_names = ['date', 'time', 's-ip', 'cs-method', 'cs-uri-stem', 'cs-uri-query', 's-port', 'cs-username', 'c-ip',
                       'cs(User-Agent)', 'cs(Referer)', 'sc-status', 'sc-substatus', 'sc-win32-status', 'time-taken']

    df = pd.DataFrame()
    df_summary = pd.DataFrame()
    
    print("1")
    # folder 1
    for dirname, dirnames, filenames in os.walk(IISLogFolder1):
        for subdirname in dirnames:
            # skip FTP log directories - we don't want to parse FTP logs
            if subdirname[:3] != "FTP":
                list_of_files = glob.glob(IISLogFolder1 + "\\" + subdirname + "\*.log")
                print("", len(list_of_files))
                
                if len(list_of_files) > 0:
                    for f in list_of_files:
                        creation_time = os.path.getctime(f)
                        # 7 days
                        if (current_time - creation_time) // (24 * 3600) <= float(dayRange_user):
                            try:
                                df_temp = pd.read_csv(f, sep=' ', comment='#', engine='python', names=log_field_names)
                                df_temp["website"] = subdirname
                                df = df.append(df_temp, ignore_index=True)
                            except ValueError:
                                print("7 days", f)
                        # summary 30 days
                        if (current_time - creation_time) // (24 * 3600) <= float(dayRange_summary):
                            try:
                                df_temp_summary = pd.read_csv(f, sep=' ', comment='#', engine='python', names=log_field_names)
                                df_temp_summary["website"] = subdirname
                                df_summary = df_summary.append(df_temp_summary, ignore_index=True)
                            except ValueError:
                                print("30 days", f)
                            
    print("2")
    # folder 2
    for dirname, dirnames, filenames in os.walk(IISLogFolder2):
        for subdirname in dirnames:
            if subdirname[:3] != "FTP":
                list_of_files = glob.glob(IISLogFolder2 + "\\" + subdirname + "\*.log")
                if len(list_of_files) > 0:
                    for f in list_of_files:
                        creation_time = os.path.getctime(f)
                        # 7 days
                        if (current_time - creation_time) // (24 * 3600) <= float(dayRange_user):
                            try:
                                df_temp = pd.read_csv(f, sep=' ', comment='#', engine='python', names=log_field_names)
                                df_temp["website"] = subdirname
                                df = df.append(df_temp, ignore_index=True)
                            except ValueError:
                                print("7 days", f)
                                
                        # summary 30 days
                        if (current_time - creation_time) // (24 * 3600) <= float(dayRange_summary):
                            try:
                                df_temp_summary = pd.read_csv(f, sep=' ', comment='#', engine='python', names=log_field_names)
                                df_temp_summary["website"] = subdirname
                                df_summary = df_summary.append(df_temp_summary, ignore_index=True)
                            except ValueError:
                                print("30 days", f)
                            
    
   
    #
    #
    print("3")

    #df_test = df['cs-uri-stem']
    #df_test.to_csv('D:\etls\IISLogParse\out\data.csv')

    # NCA user - 7 days
    df_user = df[df['cs-uri-stem'].str.contains(query_WebService + '|' + query_WebService_2)]
    parseUserData(df_user, csvFile_user, jsonFile_user)

    print("4")
    
    # NCA site_id query - 7 days
    df_site = df[df['cs-uri-stem'].str.contains(query_restService_site + '|' + query_restService_site_2)]
    df_site_sub = df_site[['date', 'cs-uri-query']]
    parseSiteData(df_site_sub, csvFile_site, jsonFile_site)

    print("5")

    #
    # NCA user - 30days
    df_user_summary = df_summary[df_summary['cs-uri-stem'].str.contains(query_WebService + '|' + query_WebService_2)]
    parseUserData(df_user_summary, csvFile_summary_users, jsonFile_summary_users)

    print("6")
    
    # NCA site_id - 30 days
    df_site_summary = df_summary[df_summary['cs-uri-stem'].str.contains(query_restService_site + '|' + query_restService_site_2)]
    df_site_sub_summary = df_site_summary[['date', 'cs-uri-query']]
    parseSiteData(df_site_sub_summary, csvFile_summary_sites, jsonFile_summary_sites)
    
    print("7")
    
    # NCA gravesites - 30 days
    df_gravesite_summary = df_summary[df_summary['cs-uri-stem'].str.contains(query_restService_gravesite + '|' + query_restService_gravesite_2)]
    df_gravesite_sub_summary = df_gravesite_summary[['date', 'cs-uri-query']]
    parseGravesiteData(df_gravesite_sub_summary, csvFile_summary_gravesites, jsonFile_summary_gravesites)
    parseNumCountByGravesiteData(df_gravesite_sub_summary, csvFile_summary_numCountByGravesite, jsonFile_summary_numCountByGravesite)

    print("8")

    # get unique user request count for 7/30 days    
    dataSummary(csvFile_user, csvFile_summary_users, csvFile_summary_users_unique_7days, csvFile_summary_users_unique_30days)
    

    myLogger.writeLog("End...", True)

def dataSummary(csvFile_users_7days, csvFile_users_30days, csvFile_users_unique_7days, csvFile_users_unique_30days):
    # 7days
    df = pd.read_csv(csvFile_users_7days)
    df_sum = df.groupby(['User'])['Count'].agg('sum').reset_index()
    df_sum.to_csv(csvFile_users_unique_7days, index=False)
    
    # 30days
    df = pd.read_csv(csvFile_users_30days)
    df_sum = df.groupby(['User'])['Count'].agg('sum').reset_index()
    df_sum.to_csv(csvFile_users_unique_30days, index=False)

def parseUserData(df_user, csvFile_user, jsonFile_user):
   
    # group
    g = df_user.groupby(['date', 'cs-username'])
    dataList = []
    for p in g.groups: 
        if p[1] != '-':
            data = [p[0], p[1], len(g.groups[p])]
            dataList.append(data)
    df_new = pd.DataFrame(dataList, columns = ['Date', 'User', 'Count']) 
    df_sort = df_new.sort_values(by=['Date', 'User'], ascending=[False, True])
    
    df_sort.to_csv(csvFile_user, index=False)
    df_sort.to_json(jsonFile_user, orient='records')

    
def parseSiteData(df_data, csvFile_site, jsonFile_site):
    g = df_data.groupby(['date', 'cs-uri-query'])
    dataList = []
    for p in g.groups: 
        if p[1] != '-':
            date = p[0]
            #
            siteid = ""
            x = p[1].upper().split("&")
            if x[0].startswith('WHERE=SITE_ID'):            
                siteid = x[0].replace('%3D', '=').replace('%27', '').replace('WHERE=SITE_ID=', '')
                if len(siteid ) != 3:
                    siteid = ""

            #                
            numcount = len(g.groups[p])

            #
            if siteid != '':                 
                data = [date, siteid, numcount]
                dataList.append(data)
    
    df_new = pd.DataFrame(dataList, columns = ['Date', 'Site_ID', 'Count']) 
    df_sort = df_new.sort_values(by=['Date', 'Site_ID'], ascending=[False, True])

    df_sort.to_csv(csvFile_site, index=False)
    df_sort.to_json(jsonFile_site, orient='records')        

def parseGravesiteData(df_data, csvFile_gravesite, jsonFile_gravesite):
    g = df_data.groupby(['date', 'cs-uri-query'])
    dataList = []
    for p in g.groups: 
        if p[1] != '-':
            date = p[0]
            #
            gravesiteid = ""
            x = p[1].upper().split("&")
            if x[0].startswith('WHERE=GRAVEMARKER_ID'):
                gravesiteid = x[0].replace('%27+AND+ACTIVE%3D%271%27', '').replace('%3D', '=').replace('%27', '').replace('%20AND%20ACTIVE=1', '').replace('WHERE=GRAVEMARKER_ID=', '').replace('\'', '')
                #print(gravesiteid)
                #if len(siteid ) != 3:
                #    siteid = ""

            #                
            numcount = len(g.groups[p])

            #
            if gravesiteid != '':                 
                data = [date, gravesiteid, numcount]
                dataList.append(data)
    
    df_new = pd.DataFrame(dataList, columns = ['Date', 'GRAVEMARKER_ID', 'Count']) 
    df_sort = df_new.sort_values(by=['Date', 'GRAVEMARKER_ID'], ascending=[False, True])

    df_sort.to_csv(csvFile_gravesite, index=False)
    df_sort.to_json(jsonFile_gravesite, orient='records')        

def parseNumCountByGravesiteData(df_data, csvFile_numCountByGravesite, jsonFile_numCountByGravesite):
    g = df_data.groupby(['date'])
    dataList = []
    for p in g.groups:
        date = p
        numcount = len(g.groups[p])

        #
        data = [date, numcount]
        dataList.append(data)
    
    df_new = pd.DataFrame(dataList, columns = ['Date', 'GravesiteCount']) 
    df_sort = df_new.sort_values(by=['Date', 'GravesiteCount'], ascending=[False, True])

    df_sort.to_csv(csvFile_numCountByGravesite, index=False)
    df_sort.to_json(jsonFile_numCountByGravesite, orient='records')        
          
if __name__ == '__main__':
    main()

