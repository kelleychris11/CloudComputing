using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Threading;

//----------------------------------------
//Form allows user to select quiz category
//----------------------------------------

namespace program5
{
    public partial class Category : System.Web.UI.Page
    {
        public Catagories categories;
        public Questions questions;
        public const string TABLE_NAME = "Program5Data";

        protected void Page_Load(object sender, EventArgs e)
        {
            string userID = null;
            try
            {
                userID = (string)Session["UserID"];
            }
            catch(Exception)
            {
                Response.Redirect("Default.aspx", true);
            }
            if (userID == null)
            {
                Response.Redirect("Default.aspx", true);
            }

            addAttempt();

            //load drop down with catagories
            string url = "https://opentdb.com/api_category.php";
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                for(int i = 0; i < 12; i++)
                {
                    try
                    {
                        response = client.GetAsync(url).Result;
                        break;
                    }
                    catch(Exception)
                    {
                        waitTime(i);
                    }
                }
            }

            string message = response.Content.ReadAsStringAsync().Result;
            categories = JsonConvert.DeserializeObject<Catagories>(message);

            //manually add 'any catagory' because api does not include it
            catList.Items.Add("Any Catagory");
            for (int i = 0; i < categories.trivia_categories.Length; i++)
            {
                catList.Items.Add(categories.trivia_categories[i].name);
            }
        }

        //event handler for play game button. Starts quiz game.
        protected void playGameBtn_Click(object sender, EventArgs e)
        {
            Session["pageSet"] = false;
            int catIndex = catList.SelectedIndex;
            string url = "https://opentdb.com/api.php?amount=10";

            if(catIndex != 0)
            {
                int catApiNum = categories.trivia_categories[catIndex - 1].id;
                url += "&category=" + catApiNum + "&type=multiple";
            }
            else
            {
                url += "&type=multiple";
            }
            Session["url"] = url;
            var response = new HttpResponseMessage();
            string message = null;
            using (var client = new HttpClient())
            {
                for(int i = 0; i < 12; i++)
                {
                    try
                    {
                        response = client.GetAsync(url).Result;
                        break;
                    }
                    catch(Exception)
                    {
                        waitTime(i);
                    }
                }               
                message = response.Content.ReadAsStringAsync().Result;
            }
            questions = JsonConvert.DeserializeObject<Questions>(message);
            Session["questions"] = questions;
            Session["CurIncorrect"] = 0;
            Response.Redirect("PlayGame.aspx", true);
        }

        //increments total number of attempts user has made at quiz
        public void addAttempt()
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
                    waitTime(i);
                }
            }
            Document doc = new Document();

            string key = null;
            try
            {
                key = (string)Session["UserID"];
            }
            catch(Exception)
            {
                Response.Redirect("Default.aspx");
            }
            if(key == null)
            {
                Response.Redirect("Default.aspx");
            }
            
            for(int i = 0; i < 9; i++)
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

            if(doc == null)
            {
                return;
            }

            int numAttempts = Convert.ToInt32(doc["NumAttempts"]) + 1;
            doc["NumAttempts"] = numAttempts;

            for(int i = 0; i < 9; i++)
            {
                try
                {
                    table.PutItem(doc);
                    break;
                }
                catch(Amazon.DynamoDBv2.AmazonDynamoDBException)
                {
                    waitTime(i);
                }
            }          
        }

        //used for waiting between failed attempts at http request
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



    //get trivia catagories for REST call
    public class Catagories
    {
        public Trivia_Categories[] trivia_categories { get; set; }
    }

    public class Trivia_Categories
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    //Trivia questions and answers
    public class Questions
    {
        public int response_code { get; set; }
        public Result[] results { get; set; }
    }

    public class Result
    {
        public string category { get; set; }
        public string type { get; set; }
        public string difficulty { get; set; }
        public string question { get; set; }
        public string correct_answer { get; set; }
        public string[] incorrect_answers { get; set; }
    }
}