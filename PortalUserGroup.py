# -*- coding: utf-8 -*-
"""
Find portal users without group
Created on 12/10/2020

@author: VHAISAZHANGX
"""

import os,sys,socket,getpass
import pandas as pd
from tabulate import tabulate

from arcgis.gis import GIS
import configparser
import logger

#import smtplib

from sqlalchemy import create_engine

classes_path = os.path.abspath(os.path.join(os.path.dirname( __file__ ), 'Classes'))
sys.path.append(classes_path)

from emailUsers import EmailPortalUsers

# log
localPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), ''))
logPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), 'logs'))
logFile = os.path.join(logPath, "PortalUserGroup_log.txt")
if (not os.path.exists(logPath)):
    os.makedirs(logPath);


#
df_portalUsers = None
df_portalGroups = None
workGroupNameList = []
portalGroups = []
addPortalGroupsList = []
removePortalGroupsList = []

# add user list
addUserList_path = "\\\\vhacdwdwhmul03.vha.med.va.gov\\GeoBISL\\SystemV4\\Portal\\userAddList.csv"
removeUserList_path = "\\\\vhacdwdwhmul03.vha.med.va.gov\\GeoBISL\\SystemV4\\Portal\\userRemoveList.csv"

    
def main():
    host = socket.gethostname()
    userName = getpass.getuser()

    global myLogger
    myLogger = logger.Logger(logFile, initalText="\nStarting PortalUserGroup script" + " - running on " + host + " by " + userName, overwriteLog=False)
    myLogger.writeLog("\n================================================================================================")

    # settings.config
    #localPath = sys.path[0]
    settingsFile = os.path.join(localPath, "settings.ini")

    # check settings.ini
    if (not os.path.isfile(settingsFile)):
        myLogger.writeLog("Missing settings.ini file.")
        sys.exit()

    config = configparser.ConfigParser()
    config.read(settingsFile)

    
    #
    portal_url = config.get("INPUT", "portal_url")
    portal_user = config.get("INPUT", "portal_user")
    portal_password = config.get("INPUT", "portal_password")
    
    #
    server_name = config.get("INPUT", "server_name")
    db_name = config.get("INPUT", "db_name")
    table_name = config.get("INPUT", "table_name")


    #
    csvFile_user = config.get("OUTPUT", "csvFile_user")
    csvFile_group = config.get("OUTPUT", "csvFile_group")
    csvFile_workgroupname = config.get("OUTPUT", "csvFile_workgroupname")
    csvFile_addPortalGroup = config.get("OUTPUT", "csvFile_addPortalGroup")
    csvFile_userAddList  = config.get("OUTPUT", "csvFile_userAddList")
    csvFile_removePortalGroup = config.get("OUTPUT", "csvFile_removePortalGroup")
    csvFile_userRemoveList  = config.get("OUTPUT", "csvFile_userRemoveList")
    
    #
    myLogger.writeLog("Start...", True)
    myLogger.writeLog("Portal URL: " + portal_url)
    myLogger.writeLog("OUTPUT = " + csvFile_userAddList)
    myLogger.writeLog("OUTPUT = " + csvFile_userRemoveList)



    #expiration = 60


    # connect to thye portal
    gis = GIS(portal_url, portal_user, portal_password, verify_cert=False)
    
    # retrive CDW workgroup Name from VHACDWDWHSQL43/GIS_DATA_PROCESS/GeoBISL.BaseCamp_Permissions
    getWorkgroupNameList(server_name, db_name, table_name, csvFile_workgroupname)    
    # get portal users without group
    getPortalUserGroupData(gis, csvFile_user, csvFile_group, csvFile_addPortalGroup, csvFile_removePortalGroup, server_name, db_name, table_name)
    
    #add Users to portal Groups from csvFile_addPortalGroup
    addUserToGroup(gis, csvFile_addPortalGroup, csvFile_userAddList, csvFile_removePortalGroup, csvFile_userRemoveList, myLogger)

    # send email
    #sendEmail()
    
    myLogger.writeLog("End...", True)        


def addUserToGroup(gis, csvFile_addPortalGroup, csvFile_userAddList, csvFile_removePortalGroup, csvFile_userRemoveList, myLogger):
 
    numUsersAdded = 0
    usersAdded = []

    numUsersRemoved = 0
    usersRemoved = []
    
    #
    # add
    df = pd.read_csv(csvFile_addPortalGroup)
    
    if len(df) > 0:
         df.to_csv(csvFile_userAddList, mode='a', index=False)
         
    #         
    for index, row in df.iterrows():
        numUsersAdded +=1
        userName = str(row['username'])
        groupName = str(row['portalGroup'])
        
        usersAdded.append([userName, groupName])
        myLogger.writeLog("User Added: " + userName + " Group: " + groupName)        
        
        #
        users = []
        users.append(userName)
        
        #
        group = gis.groups.search('title: ' + groupName, '')
        group[0].add_users(users)
        
        #
        #groupUsers = group[0].get_members()
        #print(groupUsers)        


    #
    # remove
    df = pd.read_csv(csvFile_removePortalGroup)
    
    if len(df) > 0:
         df.to_csv(csvFile_userRemoveList, mode='a', index=False)
         
    #         
    for index, row in df.iterrows():
        numUsersRemoved +=1
        userName = str(row['username'])
        groupName = str(row['portalGroup'])
        
        usersRemoved.append([userName, groupName])
        myLogger.writeLog("User Removed: " + userName + " Group: " + groupName)        
        
        #
        users = []
        users.append(userName)
        #
        group = gis.groups.search('title: ' + groupName, '')
        group[0].remove_users(users)
        
        
    #
    # send email
    sendEmail(numUsersAdded, usersAdded, numUsersRemoved, usersRemoved)   
    
def getPortalUserGroupData(gis, csvFile_user, csvFile_group, csvFile_addPortalGroup, csvFile_removePortalGroup, server_name, db_name, table_name,):

    # list all portal groups
    group_data = [] 
    all_groups = gis.groups.search()    
    for group in all_groups:
       #print("Portal Group", group.title, group.owner, group.access, group.tags, group.description, group.snippet)
       group_data.append([str(group.title), str(group.owner), str(group.access), str(group.tags), str(group.description), str(group.snippet)])
    
    df = pd.DataFrame(group_data, columns = ['title', 'owner', 'access', 'tags', 'description', 'snippet']) 

    # save to csv
    df.to_csv(csvFile_group, index=False)
    
    # get portal groups matched with CDW WorkgroupName List
    for index, row in df.iterrows():
            portalGroupName = str(row['title'])
            if (portalGroupName in workGroupNameList): 
                portalGroups.append(portalGroupName)

    # portal user data
    user_data = [] 
    portal_users = gis.users.search('!esri_ & !admin')
    for user in portal_users:
        user_data.append([str(user.username), str(user.email), str(user.level), str(user.role), str(user.groups)])
        #
        userName = str(user.username)
        userEmail = str(user.email)
        getUserWorkgroupName(gis, userName, userEmail, user.groups, server_name, db_name, table_name)

    # df
    df = pd.DataFrame(user_data, columns = ['username', 'email', 'level', 'role', 'groups']) 
    
    # save to csv
    df.to_csv(csvFile_user, index=False)

    # add portal group for user
    df = pd.DataFrame(addPortalGroupsList, columns = ['username', 'portalGroup']) 
    
    # save to csv
    df.to_csv(csvFile_addPortalGroup, index=False)    
                
    # remove portal group for user
    df = pd.DataFrame(removePortalGroupsList, columns = ['username', 'portalGroup']) 
    
    # save to csv
    df.to_csv(csvFile_removePortalGroup, index=False)    
                

def getUserWorkgroupName(gis, userName, userEmail, groups, server_name, db_name, table_name):
    #user = gis.users.get(userName)

    # add portal group to user
    addGroups = []
    removeGroups = []
    
    userPortalGroups = []
    for grp in groups:
        userPortalGroups.append(str(grp.title))
        #print(str(grp.title))

    #print("===============================")        
    #print(userName, userEmail)
    #print(userPortalGroups)
    
    engine = create_engine('mssql://' + server_name + '/' + db_name + '?trusted_connection=yes&driver=SQL+Server')
   
    with engine.connect() as conn, conn.begin():
        qry = "SELECT DISTINCT WorkgroupName FROM " + db_name + "." + table_name + " Where  AuthorizationType in('CDW_Full', 'CDW_SPatient', 'CDW_Staff') And UPPER(EmailAddress)='" +  userEmail.upper() + "'"
        df = pd.read_sql(qry, engine)
        
        #
        workGroupNameList = []
        for index, row in df.iterrows():
            workGroupName = str(row['WorkgroupName'])
            workGroupNameList.append(workGroupName)
            #
            matched_workGroupName = False
            matched_userPortalGroups = False
            
            #
            if (workGroupName in portalGroups):
               matched_workGroupName = True
            if (workGroupName in userPortalGroups):
               matched_userPortalGroups = True
               
            # add portal group
            if (matched_workGroupName == True and matched_userPortalGroups == False):
                addGroups.append(workGroupName)
                addPortalGroupsList.append([userName, workGroupName])
                
            # removed portal group
            if (matched_workGroupName == False and  matched_userPortalGroups == True):
                removeGroups.append(workGroupName);
                removePortalGroupsList.append([userName, workGroupName])
                
        #
        #print("     User CDW WorkgroupName", workGroupNameList)
        #print("     User Portal Group", userPortalGroups)
        #print("     User Add Portyal Group", addGroups)
        #print("     User remove Portal Group", removeGroups)
    
    
def getWorkgroupNameList(server_name, db_name, table_name, csvFile_workgroupname):

    engine = create_engine('mssql://' + server_name + '/' + db_name + '?trusted_connection=yes&driver=SQL+Server')
    
    with engine.connect() as conn, conn.begin():
        qry = "SELECT Distinct WorkgroupName FROM " + db_name + "." + table_name + "  Where AuthorizationType in('CDW_Full', 'CDW_SPatient', 'CDW_Staff')"
        df = pd.read_sql(qry, engine)

        for index, row in df.iterrows():            
            workGroupNameList.append(str(row['WorkgroupName']))
   
        # save to csv
        df.to_csv(csvFile_workgroupname, index=False)

        #
        del df
        del engine
        
             
def sendEmail(numUsersAdded, usersAdded, numUsersRemoved, usersRemoved):
    
    #print("sendEmail_1")
    portalUrl = "sdev.vamaps.va.gov/portal"
    subjectText = "Assign Portal Users to corresponding portal groups"
    emailMessage = "Portal Url: " + portalUrl + " \n" + \
                   "Number of User Added: " + str(numUsersAdded ) + "\n" + "UserAddList File: " + addUserList_path + "\n" + \
                   "Number of User Removed: " + str(numUsersRemoved ) + "\n" + "UserRemoveList File: " + removeUserList_path + "\n"
                       
    # added
    if numUsersAdded > 0:
        emailMessage += "\n"               
        emailMessage += str(tabulate(usersAdded, headers=['User Added', 'Portal Group']))
    
    # removed
    if numUsersRemoved > 0:
        emailMessage += "\n"               
        emailMessage += str(tabulate(usersRemoved, headers=['User Added', 'Portal Group']))
    
    fromEmail = "vhacdwgisgp1d@va.gov"
    toEmails = ['GeoBISL@va.gov']
    #toEmails = ['xiaoyi.zhang2@va.gov']
    
    
    # sending
    email = EmailPortalUsers(fromEmail, toEmails)
    email.setSubjectText(subjectText)
    email.setBodyText(emailMessage)
    email.sendEmail();    

    #print("sendEmail_2")
        
if __name__ == '__main__':
    main()

