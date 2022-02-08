# -*- coding: utf-8 -*-
"""
Convert ShapeFile to TopoJSon

@author: VHAISAZHANGX0
"""

import os,sys,socket,getpass
import time
import glob
import pandas as pd

import topojson as tp
import geopandas as gpd

import configparser
import logger

# log
localPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), ''))
logPath = os.path.abspath(os.path.join(os.path.dirname( __file__ ), 'logs'))
logFile = os.path.join(logPath, "ConvertToTopoJSon_log.txt")
if (not os.path.exists(logPath)):
    os.makedirs(logPath);
    
def main():
    host = socket.gethostname()
    userName = getpass.getuser()

    global myLogger
    myLogger = logger.Logger(logFile, initalText="\nConvertToTopoJSon()" + " - running on " + host + " by " + userName, overwriteLog=False)
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
    inFile = config.get("INPUT", "inFile")
    
    # output
    outFolder = config.get("OUTPUT", "outFolder")


    myLogger.writeLog("Start...", True)

    myLogger.writeLog("inFile = " + inFile)
    myLogger.writeLog("outFolder = " + outFolder)
    
        
    

    myLogger.writeLog("End...", True)

          
if __name__ == '__main__':
    main()

