using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using Amazon.S3;
using System.Threading;

//------------------------------------------------
//Default web page for application: get user ID or 
// allow user to redirect to registration page
//------------------------------------------------

namespace program5
{
    public partial class _Default : Page
    {
        const string TABLE_NAME = "Program5Data";
        //response message outputs on aspx page
        public string responseMessage = "";

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        //Event handler for login button, if credentials are correct, redirect
        //to category selection page
        protected void loginBtn_Click(object sender, EventArgs e)
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
                        responseMessage = "connection failure";
                        return;
                    }
                    else
                    {
                        waitTime(i);
                    }
                }
            }

            Document doc = new Document();
            //get userID
            var key = userIDTxt.Text.Trim();

            if(key == "")
            {
                responseLabel.Text = "No user Id provided";
                return;
            }

            for(int i = 0; i < 9; i++)
            {
                try
                {
                    doc = table.GetItem(key);
                    break;
                }
                catch (Amazon.DynamoDBv2.AmazonDynamoDBException)
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
  
            if (doc == null)
            {
                responseLabel.Text = "UserID not found";
                return;
            }

            string pw = pwTxt.Text.Trim();
            if(pw == doc["Password"])
            {
                Session["UserID"] = key;
                Response.Redirect("Category.aspx", true);
            }
            else
            {
                responseLabel.Text = "Incorrect password, try again";
                return;
            }
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