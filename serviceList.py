# -*- coding: utf-8 -*-
"""
Find services created by today
Created on Mon Feb 25 12:50:43 2019

@author: VHAISAZHANGX0
"""

import os,sys,socket,getpass
import json

import datetime, time

from arcgis.gis import GIS
import configparser
import logger
import ast

# log
localPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), ''))
logPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), 'logs'))
logFile = os.path.join(logPath, "serviceList_log.txt")
if (not os.path.exists(logPath)):
    os.makedirs(logPath);
    
def main():
    host = socket.gethostname()
    userName = getpass.getuser()

    global myLogger
    myLogger = logger.Logger(logFile, initalText="\nStarting Service List" + " - running on " + host + " by " + userName, overwriteLog=False)
    myLogger.writeLog("\n================================================================================================")

    # settings.config
    settingsFile = os.path.join(localPath, "settings.ini")

    # check settings.ini
    if (not os.path.isfile(settingsFile)):
        myLogger.writeLog("Missing settings.ini file.")
        sys.exit()

    config = configparser.ConfigParser()
    config.read(settingsFile)

    serviceUrl = config.get("INPUT", "serviceUrl")
    serviceFolders = config.get("INPUT", "serviceFolders")
    serviceTitle = config.get("INPUT", "serviceTitle")
    jsonFile_service_inventory = config.get("OUTPUT", "jsonFile_service_inventory")


    myLogger.writeLog("Start...", True)
    myLogger.writeLog("INPUT = " + serviceUrl)
    myLogger.writeLog("OUTPUT_service_inventory = " + jsonFile_service_inventory)


    # connect to thye portal
    gis = GIS(serviceUrl, "arcgis", "arc4GIS1", verify_cert=False) 

    # get services from a folder
    getServiceInventory(gis, jsonFile_service_inventory, serviceFolders)
    
    myLogger.writeLog("End...", True)        

def getServiceInventory(gis, jsonFile, serviceFolders):
    #
    #serviceFolderList = ast.literal_eval(serviceFolders)
    #for serviceFolder in serviceFolderList:
    #    if serviceFolder == "VA":
    #        print(serviceFolder)

    #
    jsonDataList = []
    json_index = 0
    #search_result = gis.content.search(query="", item_type="", max_items=9999)
    search_result = gis.content.advanced_search(query=f"type:'Feature Service'", max_items=999999)

    for item in search_result:
        json_index +=1
        #
        service_id = item.id
        service_owner = item.owner
        service_created = int(item.created)
        service_modified = int(item.modified)
        service_title = item.title
        service_type = item.type

       
        if item.thumbnail != None:
            #print("item.thumbnail", item.thumbnail)
            thumbnailFile = "\\\\vhacdwdwhmul03.vha.med.va.gov\ARCGIS\\arcgisportal\content\\items\\" + service_id + "\\esriinfo\\" + item.thumbnail
            if (not os.path.exists(thumbnailFile)):
                print(service_id, thumbnailFile)
        #
        service_created = datetime.datetime.fromtimestamp(service_created / 1e3).strftime('%Y-%m-%d %H:%M:%S')
        service_modified = datetime.datetime.fromtimestamp(service_modified / 1e3).strftime('%Y-%m-%d %H:%M:%S')

        
        #print(service_created, service_modified)
        jsonItem = {"title": service_title, "type": service_type, "owner": service_owner, "modified": service_modified}
        jsonDataList.append(jsonItem)
        
        #break

    
    # write to json file
    with open(jsonFile, 'w') as outFile:
        outFile.write(json.dumps(jsonDataList))
    
    
if __name__ == '__main__':
    main()

