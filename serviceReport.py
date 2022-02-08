#-------------------------------------------------------------------------------
# Name:        serviceReport.py
# Purpose:     create a json file for all of arcgis services
#
# Author:      XIAOYI ZHANG
#
# Created:     10/02/2018
# Copyright:   (c) GeoBISL 2018
# Licence:     <your licence>
#-------------------------------------------------------------------------------


import sys,os,socket,getpass
import urllib

import requests
from urllib import request, parse

# Disable warnings
#requests.packages.urllib3.disable_warnings()

import json
#import contextlib
#import csv
import datetime, time, uuid
import configparser
import logger


# log
localPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), ''))
logPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), 'logs'))
logFile = os.path.join(logPath, "searchServicesForArcGISServer.txt")

if (not os.path.exists(logPath)):
    os.makedirs(logPath);

def main():
    host = socket.gethostname()
    userName = getpass.getuser()

    global myLogger
    myLogger = logger.Logger(logFile, initalText="\nStarting Service Report" + " - running on " + host + " by " + userName, overwriteLog=False)
    myLogger.writeLog("\n================================================================================================")

    # settings.config
    settingsFile = os.path.join(localPath, "settings.ini")

    # check settings.ini
    if (not os.path.isfile(settingsFile)):
        myLogger.writeLog("Missing settings.ini file.")
        sys.exit()

    config = configparser.ConfigParser()
    config.read(settingsFile)

    #
    serverName = config.get("SOURCE", "serverName")
    serverName_secured = config.get("SOURCE", "serverName_secured")
    #
    userName = config.get("SOURCE", "adminUser")
    password = config.get("SOURCE", "adminPassword")
    #
    jsonFile_requestsCount  = config.get("OUTPUT", "serviceReport_requestsCount")
    jsonFile_requestsFailed  = config.get("OUTPUT", "serviceReport_requestsFailed")
    jsonFile_requestsCount_7days  = config.get("OUTPUT", "serviceReport_requestsCount_7days")

    myLogger.writeLog("Start...", True)
    myLogger.writeLog("INPUT = " + serverName)
    myLogger.writeLog("INPUT = " + serverName_secured)
    myLogger.writeLog("OUTPUT = " + jsonFile_requestsCount)
    myLogger.writeLog("OUTPUT = " + jsonFile_requestsFailed)
    myLogger.writeLog("OUTPUT = " + jsonFile_requestsCount_7days)

    current_time = time.localtime()

    fromTimeStr = time.strftime('%Y-%m-%d 00:00', current_time)
    toTimeStr = time.strftime('%Y-%m-%d %H:%M', current_time)

    fromTime = int(time.mktime(time.strptime(fromTimeStr, '%Y-%m-%d %H:%M')) * 1000)
    toTime = int(time.mktime(time.strptime(toTimeStr, '%Y-%m-%d %H:%M')) * 1000)

    # 7 days
    day1 = datetime.date.today()
    day2 = datetime.date.today()-datetime.timedelta(1)
    day3 = datetime.date.today()-datetime.timedelta(2)
    day4 = datetime.date.today()-datetime.timedelta(3)
    day5 = datetime.date.today()-datetime.timedelta(4)
    day6 = datetime.date.today()-datetime.timedelta(5)
    day7 = datetime.date.today()-datetime.timedelta(6)

    day1_from = datetime.datetime(day1.year, day1.month, day1.day, 0, 0, 00)
    day1_to = datetime.datetime(day1.year, day1.month, day1.day, 23, 59, 59)
    day2_from = datetime.datetime(day2.year, day2.month, day2.day, 0, 0, 00)
    day2_to = datetime.datetime(day2.year, day2.month, day2.day, 23, 59, 59)
    day3_from = datetime.datetime(day3.year, day3.month, day3.day, 0, 0, 00)
    day3_to = datetime.datetime(day3.year, day3.month, day3.day, 23, 59, 59)
    day4_from = datetime.datetime(day4.year, day4.month, day4.day, 0, 0, 00)
    day4_to = datetime.datetime(day4.year, day4.month, day4.day, 23, 59, 59)
    day5_from = datetime.datetime(day5.year, day5.month, day5.day, 0, 0, 00)
    day5_to = datetime.datetime(day5.year, day5.month, day5.day, 23, 59, 59)
    day6_from = datetime.datetime(day6.year, day6.month, day6.day, 0, 0, 00)
    day6_to = datetime.datetime(day6.year, day6.month, day6.day, 23, 59, 59)
    day7_from = datetime.datetime(day7.year, day7.month, day7.day, 0, 0, 00)
    day7_to = datetime.datetime(day7.year, day7.month, day7.day, 23, 59, 59)

    dateList = []
    dateList.append([day1_from, day1_to])
    dateList.append([day2_from, day2_to])
    dateList.append([day3_from, day3_to])
    dateList.append([day4_from, day4_to])
    dateList.append([day5_from, day5_to])
    dateList.append([day6_from, day6_to])
    dateList.append([day7_from, day7_to])


    # Get token
    token = getToken(userName, password, serverName)

    # Get token - secured
    token_secured = getToken(userName, password, serverName_secured)


    # get service list
    services = getServiceList(serverName, token, "False")



    # get service list - secured
    services_secured = getServiceList(serverName_secured, token_secured, "True")

    # service folder
    folderList = []
    for item in services:
        splitStr = item.split('/')
        folder = splitStr[0]+"/"+splitStr[1]
        if not folder in folderList:
            folderList.append(folder)

    # service folder - secured
    folderList_secured = []
    for item in services_secured:
        splitStr = item.split('/')
        folder = splitStr[0]+"/"+splitStr[1]
        if not folder in folderList:
            folderList_secured.append(folder)

    # today's service repport
    statsCreateReportURL = "{}/admin/usagereports/add?token={}&f=json".format(serverName, token)
    statsCreateReportURL_secured = "{}/admin/usagereports/add?token={}&f=json".format(serverName_secured, token_secured)

    # requestsCount
    jsonData_index = 0
    #
    reportName = uuid.uuid4().hex
    statsDefinition_requestsCount = {'reportname': reportName, 'since': 'CUSTOM',
                       'queries': [{'resourceURIs': services,
                                    'metrics': ['RequestCount']}],
                       'from': fromTime, 'to': toTime,
                       'metadata': {'temp': True,
                                    'tempTimer': int(time.time() * 1000)}}

    json_index_update, jasonData_services_requestsCount = createReport(statsCreateReportURL, statsDefinition_requestsCount, serverName, reportName, token, "False", jsonData_index)

    # secured
    reportName = uuid.uuid4().hex
    statsDefinition_requestsCount_secured = {'reportname': reportName, 'since': 'CUSTOM',
                       'queries': [{'resourceURIs': services_secured,
                                    'metrics': ['RequestCount']}],
                       'from': fromTime, 'to': toTime,
                       'metadata': {'temp': True,
                                    'tempTimer': int(time.time() * 1000)}}

    json_index_update, jasonData_services_requestsCount_secured = createReport(statsCreateReportURL_secured, statsDefinition_requestsCount_secured, serverName_secured, reportName, token_secured, "True", json_index_update)

    # save report
    saveReport(jsonFile_requestsCount, fromTimeStr, toTimeStr, jasonData_services_requestsCount, jasonData_services_requestsCount_secured)

    # requestsFailed
    jsonData_index = 0
    #
    reportName = uuid.uuid4().hex
    statsDefinition_requestsFailed = {'reportname': reportName, 'since': 'CUSTOM',
                       'queries': [{'resourceURIs': services,
                                    'metrics': ['RequestsFailed']}],
                       'from': fromTime, 'to': toTime,
                       'metadata': {'temp': True,
                                    'tempTimer': int(time.time() * 1000)}}

    json_index_update, jasonData_services_requestsFailed = createReport_failed(statsCreateReportURL, statsDefinition_requestsFailed, serverName, reportName, token, "False", jsonData_index)

    # secured
    reportName = uuid.uuid4().hex
    statsDefinition_requestsFailed_secured = {'reportname': reportName, 'since': 'CUSTOM',
                       'queries': [{'resourceURIs': services_secured,
                                    'metrics': ['RequestsFailed']}],
                       'from': fromTime, 'to': toTime,
                       'metadata': {'temp': True,
                                    'tempTimer': int(time.time() * 1000)}}

    json_index_update, jasonData_services_requestsFailed_secured = createReport(statsCreateReportURL_secured, statsDefinition_requestsFailed_secured, serverName_secured, reportName, token_secured, "True", json_index_update)

    # save report
    saveReport(jsonFile_requestsFailed, fromTimeStr, toTimeStr, jasonData_services_requestsFailed, jasonData_services_requestsFailed_secured)

    # 7 days report
    jasonData_services = []
    jsonData_index = 0
    for item in dateList:
        date_from = item[0]
        date_to = item[1]
        #
        date = date_from.strftime('%Y-%m-%d')
        date_fromTimeStr = date_from.strftime('%Y-%m-%d %H:%M')
        date_toTimeStr = date_to.strftime('%Y-%m-%d %H:%M')

        date_fromTime = int(time.mktime(time.strptime(date_fromTimeStr, '%Y-%m-%d %H:%M')) * 1000)
        date_toTime = int(time.mktime(time.strptime(date_toTimeStr, '%Y-%m-%d %H:%M')) * 1000)

        #
        reportName = uuid.uuid4().hex
        statsDefinition_requestsCount_7day = {'reportname': reportName, 'since': 'CUSTOM',
                                         'queries': [{'resourceURIs': folderList,
                                                      'metrics': ['RequestCount']}],
                                         'from': date_fromTime, 'to': date_toTime,
                                         'metadata': {'temp': True,
                                                      'tempTimer': int(time.time() * 1000)}}

        count = createReport_7day(statsCreateReportURL, statsDefinition_requestsCount_7day, serverName, reportName, token)

        # secured
        reportName = uuid.uuid4().hex
        statsDefinition_requestsCount_7day_secured = {'reportname': reportName, 'since': 'CUSTOM',
                                         'queries': [{'resourceURIs': folderList_secured,
                                                      'metrics': ['RequestCount']}],
                                         'from': date_fromTime, 'to': date_toTime,
                                         'metadata': {'temp': True,
                                                      'tempTimer': int(time.time() * 1000)}}

        count_secured = createReport_7day(statsCreateReportURL_secured, statsDefinition_requestsCount_7day_secured, serverName_secured, reportName, token_secured)

        #
        service_count = [date, count+count_secured]
        #
        itemValue = {
            "date": date,
            "count": count
        }
        jsonData_index +=1
        jsonData_service = {
            "id": jsonData_index,
            "text": "service",
            "item": itemValue
        }
        jasonData_services.append(jsonData_service)


    jsonData = {
        "id": 0,
        "children": jasonData_services
    }
    jsonDataStr = str(jsonData).replace("'", '"')
    with open(jsonFile_requestsCount_7days, 'w') as the_file:
        the_file.write(jsonDataStr)


    myLogger.writeLog("End...", True)


def createReport_7day(statsCreateReportURL, statsDefinition, serverName, reportName, token):
    postdata = {'usagereport': json.dumps(statsDefinition)}
    reportResult = postAndLoadJSON(statsCreateReportURL, postdata)

    # Query report
    statsQueryReportURL  = "{}/admin/usagereports/{}/data?token={}&f=json".format(serverName, reportName, token)
    postdata = {'filter': {'machines': '*'}}
    reportData = postAndLoadJSON(statsQueryReportURL, postdata)

    #
    totalCount = 0
    for serviceMetric in reportData['report']['report-data'][0]:
        for count in serviceMetric['data']:
            if count: totalCount += int(count)


    # Cleanup (delete) statistics report
    statsDeleteReportURL = "{}/admin/usagereports/{}/delete?token={}&f=json".format(serverName, reportName, token)
    deleteReportResult = postAndLoadJSON(statsDeleteReportURL, '')

    #
    return totalCount

def createReport_failed(statsCreateReportURL, statsDefinition, serverName, reportName, token, secured, jsonData_index):
    postdata = {'usagereport': json.dumps(statsDefinition)}
    reportResult = postAndLoadJSON(statsCreateReportURL, postdata)
    
    # Query report
    statsQueryReportURL  = "{}/admin/usagereports/{}/data?token={}&f=json".format(serverName, reportName, token)
    postdata = {'filter': {'machines': '*'}}
    reportData = postAndLoadJSON(statsQueryReportURL, postdata)

    #
    jasonData_services = []
    #
    for serviceMetric in reportData['report']['report-data'][0]:
        name = serviceMetric['resourceURI']
        #print(name)
        #
        splitStr = name.split('/')
        name = splitStr[2]
        if splitStr[1] != "Hosted":
            print(serviceMetric['resourceURI'])
        splitStr = name.split('.')
        name = splitStr[0]
        
        #name = splitStr[0].decode('utf-8', 'ignore').encode("utf-8")        
        #
        totalCount = 0
        for count in serviceMetric['data']:
            if count: totalCount += int(count)
        #
        if totalCount > 0:
            #
            jsonData_index +=1
            itemValue = {
                "service": name,
                "requestCount": totalCount,
                "secured": secured
            }
            jsonData_service = {
                "id": jsonData_index,
                "text": "service",
                "item": itemValue
            }
            jasonData_services.append(jsonData_service)


    '''
    itemValue = {
        "fromTime": fromTimeStr,
        "toTime": toTimeStr,
    }
    jsonData = {
        "id": 0,
        "item": itemValue,
        "children": jasonData_services
    }
    jsonDataStr = str(jsonData).replace("'", '"')
    with open(jsonFile, 'w') as the_file:
        the_file.write(jsonDataStr)
    '''

    # Cleanup (delete) statistics report
    statsDeleteReportURL = "{}/admin/usagereports/{}/delete?token={}&f=json".format(serverName, reportName, token)
    deleteReportResult = postAndLoadJSON(statsDeleteReportURL, '')

    #
    return jsonData_index, jasonData_services

def createReport(statsCreateReportURL, statsDefinition, serverName, reportName, token, secured, jsonData_index):
    postdata = {'usagereport': json.dumps(statsDefinition)}
    reportResult = postAndLoadJSON(statsCreateReportURL, postdata)
    
    # Query report
    statsQueryReportURL  = "{}/admin/usagereports/{}/data?token={}&f=json".format(serverName, reportName, token)
    postdata = {'filter': {'machines': '*'}}
    reportData = postAndLoadJSON(statsQueryReportURL, postdata)

    #
    jasonData_services = []
    #
    for serviceMetric in reportData['report']['report-data'][0]:
        name = serviceMetric['resourceURI']
        #
        splitStr = name.split('/')
        name = splitStr[2]
        splitStr = name.split('.')
        name = splitStr[0]
        #name = splitStr[0].decode('utf-8', 'ignore').encode("utf-8")        
        #
        totalCount = 0
        for count in serviceMetric['data']:
            if count: totalCount += int(count)
        #
        if totalCount > 0:
            #
            jsonData_index +=1
            itemValue = {
                "service": name,
                "requestCount": totalCount,
                "secured": secured
            }
            jsonData_service = {
                "id": jsonData_index,
                "text": "service",
                "item": itemValue
            }
            jasonData_services.append(jsonData_service)


    '''
    itemValue = {
        "fromTime": fromTimeStr,
        "toTime": toTimeStr,
    }
    jsonData = {
        "id": 0,
        "item": itemValue,
        "children": jasonData_services
    }
    jsonDataStr = str(jsonData).replace("'", '"')
    with open(jsonFile, 'w') as the_file:
        the_file.write(jsonDataStr)
    '''

    # Cleanup (delete) statistics report
    statsDeleteReportURL = "{}/admin/usagereports/{}/delete?token={}&f=json".format(serverName, reportName, token)
    deleteReportResult = postAndLoadJSON(statsDeleteReportURL, '')

    #
    return jsonData_index, jasonData_services


def saveReport(jsonFile, fromTimeStr, toTimeStr, jasonDataArray, jasonDataArray_secured):
    jasonDataArray_all = []
    for item in jasonDataArray:
        jasonDataArray_all.append(item)
    for item in jasonDataArray_secured:
        jasonDataArray_all.append(item)

    #
    itemValue = {
        "fromTime": fromTimeStr,
        "toTime": toTimeStr,
    }
    jsonData = {
        "id": 0,
        "item": itemValue,
        "children": jasonDataArray_all
    }
    jsonDataStr = str(jsonData).replace("'", '"')
    with open(jsonFile, 'w') as the_file:
        the_file.write(jsonDataStr)

def postAndLoadJSON(url, postData):
    postdata = parse.urlencode(postData).encode()
    req =  request.Request(url, data=postdata) # this will make the method "POST"
    response = request.urlopen(req)
    data = response.read()
    response.close()

    return json.loads(data)
    


def getServiceList(serverName, token, secured):
    url = "{}/admin/services?token={}&f=json".format(serverName, token)

    response = urllib.request.urlopen(url)
    data = response.read()
    root = json.loads(data)
    response.close()

    services = []
    folders = []
    for folder in root['folders']:
        #
        useStatus = True
        #
        if secured == "True":
            if folder == "System":
                useStatus = False
            if folder == "Utilities":
                useStatus = False

        if useStatus:
            folderURL = "/server/admin/services/" + folder
            url = "{}/admin/services/{}?token={}&f=json".format(serverName, folder, token)

            response = urllib.request.urlopen(url)
            data = response.read()
            root = json.loads(data)
            response.close()

            for service in root['services']:
                #services.append("services/{0}.{1}".format(service['serviceName'], service['type']))
                services.append("services/{0}/{1}.{2}".format(folder,service['serviceName'], service['type']))

    return services

def getToken(userName, password, serverName):
    expiration = 60
    url = "{}/admin/generateToken?f=json".format(serverName)
    params = {'f': 'pjson', 'username': userName, 'password': password, 'client': 'requestIP'}

    try:
        r = requests.post(url, data = params, verify=False)
        response = json.loads(r.content)
        tokenString = response['token']
        return tokenString
    except urllib.error.URLError as e:
        print("error: getToken()")
        return ""
    



# Run the script.
if __name__ == '__main__':
    main()