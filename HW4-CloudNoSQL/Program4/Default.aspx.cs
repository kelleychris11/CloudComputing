using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Http;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System.Threading;

//---------------------------------------------------------------
//File: Default.aspx.cs
//Author: Chris Kelley
//Date Created: 11/5/2018
//Last Modified: 11/17/2018
//Purpose: Handle business logic of web page. Retreive data
//from S3 storage, then store data in Dynamo NOSQL Database. 
//User is then able to query items, clear data, or load new data
//---------------------------------------------------------------

namespace Program4
{

    public partial class _Default : Page
    {
        public const string TABLE_NAME = "Program4Data";
        public const string URL = "https://s3-us-west-2.amazonaws.com/css490/input.txt";
        //public const string URL = "https://s3-us-west-2.amazonaws.com/superbucket33/input.txt";
        public const string S3_URL = "https://s3-us-west-2.amazonaws.com/superbucket33/data.txt";
        public string loadURL = "";
        public bool hasResponse = false;
        public string responseMessage = "";
        public bool hasData = false;
        public string message;

        //string containing keys and attributes
        public List<string> items = new List<string>();
        //use to parse data from raw string
        public List<DataItem> dataList = new List<DataItem>();
        //contains data retrieved from query
        public List<DataItem> responseList = new List<DataItem>();


        protected void Page_Load(object sender, EventArgs e)
        {
        }

        //event handler for when loadButton is clicked
        protected void LoadBtn_Click(object sender, EventArgs e)
        {
            string rawData = getData();
            if(rawData == null)
            {
                return;
            }

            parse(rawData);
            pushToDynamo();
            sendToS3(rawData);
            hasData = true;
            loadURL = URL;
            responseMessage = "Data Loaded to DynamoDB & S3 Storage";
        }

        //send data retrieved from S3 storage to another S3 storage
        public void sendToS3(string data)
        {           
            string bucketName = "superbucket33";
            string fileName = "data.txt";
            var client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2);

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = fileName,
                ContentBody = data               
            };

            for(int i = 0; i < 9; i++)
            {
                try
                {
                    var response = client.PutObject(request);
                    break;
                }
                catch(Amazon.DynamoDBv2.AmazonDynamoDBException ade)
                {
                    waitTime(i);
                }
            }
                      
        }

        //evennt handler for clearButton - clears data from database
        protected void ClearBtn_Click(object sender, EventArgs e)
        {           
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.USWest2
            };
            var client = new AmazonDynamoDBClient(config);
            var table = Amazon.DynamoDBv2.DocumentModel.Table.LoadTable(client, TABLE_NAME);
            var request = new ScanRequest{TableName = TABLE_NAME};
            var response = client.Scan(request);
            var result = response.Items;

            //traverse all items in database, delete each item
            foreach (Dictionary<string, AttributeValue> val in result)
            {
                for(int i = 0; i < 9; i++)
                {
                    try
                    {
                        table.DeleteItem(val["LastName"].S, val["FirstName"].S);
                        break;
                    }
                    catch (Amazon.DynamoDBv2.AmazonDynamoDBException ade)
                    {
                        waitTime(i);
                    }
                }
            }
            deleteFromS3();
            responseMessage = "Data cleared from DynamoDB & S3 Storage";
        }

        //delete data from S3 storage
        public void deleteFromS3()
        {
            string bucketName = "superbucket33";
            string fileName = "data.txt";
            var client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2);

            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            for(int i = 0; i < 9; i++)
            {
                try
                {
                    var response = client.DeleteObject(request);
                    break;
                }
                catch(Amazon.DynamoDBv2.AmazonDynamoDBException ade)
                {
                    waitTime(i);
                }
            }
        }

        //used for repeating http requests. Wait time increases has number of
        //reattempts increases
        public void waitTime(int attemptNum)
        {
            if(attemptNum > 16)
            {
                Thread.Sleep(8000);
            }
            else if(attemptNum > 12)
            {
                Thread.Sleep(4000);
            }
            else if(attemptNum > 7)
            {
                Thread.Sleep(2000);
            }
            else if(attemptNum > 5)
            {
                Thread.Sleep(1000);
            }
            else if(attemptNum > 3)
            {
                Thread.Sleep(500);
            }
            else
            {
                Thread.Sleep(250);
            }
        }

        //event handler for submitButton. Program uses data in text field
        //for first name and last name to find item in database
        protected void SubmitBtn_Click(object sender, EventArgs e)
        {
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.USWest2
            };

            var client = new AmazonDynamoDBClient(config);
            var table = Amazon.DynamoDBv2.DocumentModel.Table.LoadTable(client, TABLE_NAME);
            string partKey = LastNameTxt.Text.Trim();
            string sortKey = FirstNameTxt.Text.Trim();

            //reset text fields
            LastNameTxt.Text = "";
            FirstNameTxt.Text = "";

            //check for empty query
            if (partKey== "" && sortKey== "")
            {
                responseMessage = "No Data Entered";
                return;
            }
            //check if both partition key and sort key are used
            if (partKey != "" && sortKey != "")
            {
                if(!searchWithFullKey(partKey, sortKey, table))
                {
                    responseMessage = "Data Not Found";
                }
                else
                {
                    responseMessage = "Result: ";
                }
                return;
            }
           
            string searchValue = "";
            string searchKey = "";
            if(partKey == "")
            {
                searchValue = sortKey;
                searchKey = "FirstName";
            }
            else
            {
                searchValue = partKey;
                searchKey = "LastName";
            }

            if(!searchWithPartialKey(searchKey, searchValue, table))
            {
                responseMessage = "No Data Found";
            }
            else
            {
                responseMessage = "Results: ";
            }
        }

        //Search DynamoDB with partition key and sort key if both are
        //provided by the user
        public bool searchWithFullKey(string partKey, string sortKey, 
            Amazon.DynamoDBv2.DocumentModel.Table table)
        {
            Document doc = new Document();
            DataItem response = new DataItem();

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    doc = table.GetItem(partKey, sortKey);
                    break;
                }
                catch (Amazon.DynamoDBv2.AmazonDynamoDBException)
                {
                    waitTime(i);
                }
            }
 
            if (doc == null)
            {
                return false;
            }
            else
            {
                hasResponse = true;
            }

            response.partKey = doc["LastName"];
            response.sortKey = doc["FirstName"];
            var attributes = doc.GetAttributeNames();
            
            foreach (var attr in attributes)
            {
                if (attr != "LastName" && attr != "FirstName")
                {
                    response.key.Add(attr);
                    response.value.Add(doc[attr]);
                }
            }
            responseList.Add(response);
            return true;
        }

        //Search DynamoDB with a partial key, either partition key or sort key
        //if user only entered one or the other, but not both
        public bool searchWithPartialKey(string searchKey, string searchValue,
            Amazon.DynamoDBv2.DocumentModel.Table table)
        {
            ScanFilter filter = new ScanFilter();
            filter.AddCondition(searchKey, ScanOperator.Equal, searchValue);

            Search search = null;
            for(int i = 0; i < 9; i++)
            {
                try
                {
                    search = table.Scan(filter);
                    break;
                }
                catch(Amazon.DynamoDBv2.AmazonDynamoDBException ade)
                {
                    waitTime(i);
                }
            }
            
            if (search == null || search.Count == 0)
            {
                return false;
            }
            else
            {
                hasResponse = true;
            }

            List<Document> list = new List<Document>();

            for(int i = 0; i < 9; i++)
            {
                try
                {
                    list = search.GetNextSet();
                    break;
                }
                catch (Amazon.DynamoDBv2.AmazonDynamoDBException ade)
                {
                    waitTime(i);
                    if(i == 8) { return false; }
                }
            }

            foreach (var result in list)
            {
                DataItem response = new DataItem();
                response.partKey = result["LastName"];
                response.sortKey = result["FirstName"];

                foreach (var attr in result.GetAttributeNames())
                {
                    if (attr != "LastName" && attr != "FirstName")
                    {
                        response.key.Add(attr);
                        response.value.Add(result[attr]);
                    }
                }
                responseList.Add(response);
            }
            return true;
        }

        //get data from S3 storage.
        public string getData()
        {
            //string message;
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = null;
                for(int i = 0; i < 9; i++)
                {
                    try
                    {
                        response = client.GetAsync(URL).Result;
                        break;
                    }
                    catch(Exception e)
                    {
                        waitTime(i);
                    }
                }
                if(response != null)
                {
                    message = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    responseMessage = "Error connecting to S3 storage, data not loaded";
                    return null;
                }                
            }
            return message;
        }

        //send data to DynamoDB
        public void pushToDynamo()
        {
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.USWest2
            };

            var client = new AmazonDynamoDBClient(config);
            var table = Amazon.DynamoDBv2.DocumentModel.Table.LoadTable(client, TABLE_NAME);

            for(int i = 0; i < dataList.Count; i++)
            {
                var item = new Document();
                item["LastName"] = dataList[i].partKey;
                item["FirstName"] = dataList[i].sortKey;
                for(int j = 0; j < dataList[i].key.Count; j++)
                {
                    item[dataList[i].key[j]] = dataList[i].value[j];
                }
                for(int k = 0; k < 9; k++)
                {
                    try
                    {
                        table.PutItem(item);
                        break;
                    }
                    catch(Amazon.DynamoDBv2.AmazonDynamoDBException ade)
                    {
                        waitTime(k);
                    }
                }                
            }
        }

        //parse response string from S3 into manageable object
        public void parse(string message)
        {
            int curIndex = 0;
            int count = 0;
            string curData = "";
            //parse individual data sets
            while (curIndex < message.Length)
            {
                if (message.IndexOf("\r\n", curIndex) != -1)
                {
                    curData = message.Substring(curIndex, message.IndexOf("\r\n", curIndex) - curIndex) + " ";
                    items.Add(curData);
                    curIndex = message.IndexOf("\r\n", curIndex) + 2;
                }
                else
                {
                    curData = message.Substring(curIndex) + " ";
                    items.Add(curData);
                    curIndex = message.Length;
                }
                count++;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if(items[i] == "" || items[i] == " ")
                {
                    return;
                }
                DataItem curItem = new DataItem();
                curIndex = 0;
                curItem.partKey = items[i].Substring(curIndex, items[i].IndexOf(" ", curIndex) - curIndex).Trim();
                curIndex = items[i].IndexOf(" ", curIndex) + 1;

                while(items[i].Substring(curIndex, 1) == " ")
                {
                    curIndex++;
                }

                curItem.sortKey = items[i].Substring(curIndex, (items[i].IndexOf(" ", curIndex) - curIndex)).Trim();
                curIndex = items[i].IndexOf(" ", curIndex) + 1;

                int j = 0;
                while (items[i].IndexOf("=", curIndex) != -1)
                {
                    string key = items[i].Substring(curIndex, items[i].IndexOf("=", curIndex) - curIndex).Trim();
                    curItem.key.Add(key);
                    curIndex = items[i].IndexOf("=", curIndex) + 1;
                    string value = items[i].Substring(curIndex, items[i].IndexOf(" ", curIndex) - curIndex).Trim();
                    curItem.value.Add(value);
                    curIndex = items[i].IndexOf(" ", curIndex + 1);
                    j++;
                }
                dataList.Add(curItem);
            }
        }

        //Class to temporarily store data from the time the raw data is received
        // until it is sent to the NOSQL database
        public class DataItem
        {
            public string partKey;
            public string sortKey;
            public List<string> key = new List<String>();
            public List<string> value = new List<string>();
        }
    }
}
