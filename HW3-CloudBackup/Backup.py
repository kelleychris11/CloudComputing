import boto3
import botocore
import os
from time import sleep

##---------------------------------------------------------------------
# File: backup.py
# Author: Chris Kelley
# Date Created: 10/22/2018
# Last Modified: 10/28/2018
# Purpose: Backup files in the current directory, and 
# subdirectories, to AWS S3 storage bucket. All files
# may be uploaded, or individual files may be selected.
#-----------------------------------------------------------------------  

##------------------------------------------------------
# Backup class
# Purpose: Used to backup files in current directory and
# subdirectories to AWS S3 storage bucket.
class Backup:

    #Constructor
    def __init__(self):
        self.specificFiles = []
        self.foundFiles = []
        self.hasSpecific = False
        self.bucketName = None
        self.folderName = None
        self.s3 = None

    #establish connection with AWS S3 resource
    def getConn(self):
        try:
            self.s3 = boto3.client('s3')
        except (botocore.exceptions.NoCredentialsError, 
                botocore.exceptions.CredentialRetrievalError) as e:
            print("Problem retrieving credentials")
            exit()
    
    #check that bucket either exists. If specified bucket exists
    # attempt to create new bucket. Returns true if bucket is either
    # found or created.
    def checkBucket(self):

        bucketFound = False
        print("checking if bucket exists...")
        for i in range(4):
            try:
                self.s3.head_bucket(Bucket=self.bucketName)
                bucketFound = True
                break
            except (botocore.exceptions.ClientError, 
                    botocore.exceptions.ParamValidationError) as e:
                if i != 3:
                    self.waitTime(i)
                else:
                    print("Bucket not found: ", self.bucketName)

        if bucketFound == False:
            print("Checking if bucket can be created...")
            for i in range(4):
                try:
                    self.s3.create_bucket(Bucket=self.bucketName)
                    bucketFound = True
                    break
                except (botocore.exceptions.ClientError, 
                        botocore.exceptions.ParamValidationError) as e:
                    if i != 3:
                        self.waitTime(i)
                    else:
                        print("Unable to find or create bucket: " + self.bucketName)
                        bucketFound = False
        return bucketFound

    #get input from user for S3 bucket, s3 folder name, and
    #specific files to upload.
    def getInput(self):

        bucketFound = False
        while bucketFound == False:
            print("Enter bucket Name: ")
            self.bucketName = input()
            bucketFound = self.checkBucket()
            
        print("\nEnter specific files to backup (relative path) or press 'Enter'")
        print("to backup all of current directory and subdirectories:")
        response = input().strip()
        while response != "":
            self.hasSpecific = True
            if response[0] == "\\":
                response = response[1:]
            self.specificFiles.append(response)
            print("Any other files? (Press 'Enter' if done):")
            response = input()

        print("\nEnter AWS 'folder' name (Press 'Enter' to use current directory name):")    
        response = input().strip()
        if response == "":
            parentDir = os.getcwd()
            self.folderName = parentDir[parentDir.rfind("\\") + 1:]
        else:
            self.folderName = response
    
    #Used when extablishing http connection, allows multiple
    #attempts with increasing time lapse between attempt.
    def waitTime(self, count):
        if count > 4 and count % 2 == 0:
            print("Attempting Connection...")

        if count < 2:
            sleep(.2)
        elif count < 4:
            sleep(.4)
        elif count < 6:
            sleep(.8)
        elif count < 8:
            sleep(1.6)
        else:
            sleep(3.2)
    
    #If user selected specific files to upload, print any
    #files that were not found on the local system
    def printFilesNotFound(self):
        for i in self.specificFiles:
            fileFound = False
            for j in self.foundFiles:
                if i == j:
                    fileFound = True
                    break
            if fileFound == False:
                print(i + ": Not found on local system")

    #Used to upload specific files, if user chose to select
    #specific files to upload
    def uploadSpecificFiles(self, root, fileList):
 
        for specFile in self.specificFiles:
            fileFound = False
            for file in fileList:
                if file == specFile[specFile.rfind("\\") + 1:]:
                    fileName = root + "\\" + file
                    awsFileName = self.getAwsFileName(fileName)
                    self.uploadFile(fileName, awsFileName)
                    self.foundFiles.append(specFile)
        
    #get fileName that is formatted for AWS call
    def getAwsFileName(self, FileName):
        awsFileName = FileName[2:]
        awsFileName = self.folderName + "\\" + awsFileName
        awsFileName = awsFileName.replace("\\", "/")
        return awsFileName

    #Upload all files in current directory and subdirectories
    def uploadAllFiles(self, root, fileList):
        for file in fileList:
            fileName = root + "\\" + file
            awsFileName = self.getAwsFileName(fileName)
            self.uploadFile(fileName, awsFileName)

    #initialize backup process
    def backupFiles(self):

        #traverse list of files on local system 
        for root, dirList, fileList in os.walk("."):
            if self.hasSpecific:
                self.uploadSpecificFiles(root, fileList)
            else:
                self.uploadAllFiles(root, fileList)

    #Attempt to upload specified file
    def uploadFile(self, fileName, awsFileName):

        if(self.isDuplicate(fileName, awsFileName) == False):
            for i in range(10):
                try:
                    self.s3.upload_file(fileName, self.bucketName, awsFileName)
                    print(fileName[2:] + ":   ***uploaded***")
                    break
                except FileNotFoundError as e:
                    if i != 9:
                        self.waitTime()
                    else:
                        print(fileName + ": not found in local file system")
        else:
            print(fileName[2:] + ":   Not Modified")
   
    #get list of files currently in S3 bucket
    def getAwsBucketFiles(self):

        objectData = []
        for i in range(10):
            try:
                objectData = self.s3.list_objects_v2(Bucket=self.bucketName)
                break
            except botocore.exceptions.ClientError as e:
                if i != 9:
                    self.waitTime(i)
                else:
                    print("Error accessing AWS Bucket data")
        return objectData

    #get last modified date & time of file in S3 bucket
    def getAwsLastModified(self, awsFileName):

        awsTimeModified = 0
        for i in range(10):
            try:
                response = self.s3.head_object(Bucket=self.bucketName, Key=awsFileName)
                awsTimeModified = response['LastModified'].timestamp()
                break
            except botocore.exceptions.ClientError as e:
                if i != 9:
                    self.waitTime(i)
                else:
                    print("Error accessing AWS bucket data")
        return awsTimeModified

    #check to see if a file that exists on the local system
    #has been modified since it was uploaded to the bucket
    def isDuplicate(self, fileName, awsFileName):

        objectData = self.getAwsBucketFiles()
        fileExists = False
        if objectData['KeyCount'] != 0:
            for data in objectData['Contents']:
                if awsFileName == data['Key']:
                    fileExists = True
                    break
        
        if fileExists == True:
            awsLastModified = self.getAwsLastModified(awsFileName)
            localTimeModified = os.path.getmtime(fileName)
            if awsLastModified >= localTimeModified:
                return True
        
        return False
##----------------------------------------------------------
            
backup = Backup()
backup.getConn()
backup.getInput()
backup.backupFiles()
backup.printFilesNotFound()



