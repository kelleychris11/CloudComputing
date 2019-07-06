using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

//----------------------------
//Form to register new user
//----------------------------

namespace program5
{
    public partial class Register : System.Web.UI.Page
    {
        public string responseMessage;
        const string TABLE_NAME = "Program5Data";

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        //event handler for submit button. Saves userID and password in DynamoDB
        //starts new session for userID
        protected void submitBtn_Click(object sender, EventArgs e)
        {
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.USWest2
            };

            var client = new AmazonDynamoDBClient(config);

            Amazon.DynamoDBv2.DocumentModel.Table table = null;
            for(int i = 0; i < 9; i++)
            {
                try
                {
                    table = Amazon.DynamoDBv2.DocumentModel.Table.LoadTable(client, TABLE_NAME);
                    break;
                }
                catch(Amazon.DynamoDBv2.AmazonDynamoDBException)
                {
                    if(i == 8)
                    {
                        responseLabel.Text = "connection failure";
                        return;
                    }
                    else
                    {
                        waitTime(i);
                    }
                }
            }

            Document doc = new Document();
            //get userID and password input
            var key = userIDTxt.Text.Trim();
            var pw = pwTxt.Text.Trim();
            if(key == "" || pw == "")
            {
                responseLabel.Text = "Missing input value";
                return;
            }

            for(int i = 0; i < 4; i++)
            {
                try
                {
                    doc = table.GetItem(key);
                    break;
                }
                catch(Amazon.DynamoDBv2.AmazonDynamoDBException)
                {
                    waitTime(i);
                }
            }

            if(doc != null)
            {
                responseLabel.Text = "User ID already exists";
                return;
            }
            doc = new Document();
            //initialize and store user data in DynamoDB
            doc["UserID"] = key;
            doc["Password"] = pw;
            doc["Score"] = 0;
            doc["HighScore"] = 0;
            doc["NumAttempts"] = 0;

            for(int i = 0; i < 9; i++)
            {
                try
                {
                    table.PutItem(doc);
                    break;
                }
                catch(Amazon.DynamoDBv2.AmazonDynamoDBException)
                {
                    if(i == 8)
                    {
                        responseLabel.Text = "unable to save user data";
                        return;
                    }
                    else
                    {
                        waitTime(i);
                    }
                }
            }
            //save UserID for session
            Session["UserID"] = key;
            Response.Redirect("Category.aspx", true);
        }

        //used for repeating http requests. Wait time increases as number of
        //reattempts increases
        public void waitTime(int attemptNum)
        {
            if (attemptNum > 16)
            {
                Thread.Sleep(8000);
            }
            else if (attemptNum > 12)
            {
                Thread.Sleep(4000);
            }
            else if (attemptNum > 7)
            {
                Thread.Sleep(2000);
            }
            else if (attemptNum > 5)
            {
                Thread.Sleep(1000);
            }
            else if (attemptNum > 3)
            {
                Thread.Sleep(500);
            }
            else
            {
                Thread.Sleep(250);
            }
        }
    }
}