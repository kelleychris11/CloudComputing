using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

//-----------------------------------------
//Form to play quiz game
//-----------------------------------------

namespace program5
{
    public partial class PlayGame : System.Web.UI.Page
    {
        public const string TABLE_NAME = "Program5Data";
        protected void Page_Load(object sender, EventArgs e)
        {
            string userID = "";
            try
            {
                userID = (string)Session["UserID"];
            }
            catch (Exception) { Response.Redirect("Default.aspx", true); }

            if(userID == null || userID == "")
            {
                Response.Redirect("Default.aspx", true);
            }

            try
            {
                bool pageSet = (bool)Session["pageSet"];
                if (!pageSet) { setPage(); }
            }
            catch (Exception)
            {
                int questionCount = 0;
                Session["questionCount"] = questionCount;
                setPage();
            }
        }

        //set up questions and answers on page
        public void setPage()
        {
            answerBtn1.Enabled = true;
            answerBtn2.Enabled = true;
            answerBtn3.Enabled = true;
            answerBtn4.Enabled = true;
            submitBtn.Visible = true;

            int curIncorrect = 0;
            try
            {
                curIncorrect = (int)Session["curIncorrect"];
            }
            catch(Exception)
            {
                //Response.Redirect("Category.aspx");
            }

            setIncorrectButtons(curIncorrect);

            Questions questions = null;
            try
            {
                questions = (Questions)Session["questions"];
            }
            catch(Exception)
            {
                Response.Redirect("Category.aspx");
            }
            
            if (questions == null)
            {
                Response.Redirect("Category.aspx", true);
                //Server.Transfer("Category.aspx");
            }
            int questionCount = 0;
            try
            {
                questionCount = (int)Session["questionCount"];
            }catch(Exception)
            {
                //Response.Redirect("Category.aspx");
            }

            Label1.Text = questions.results[questionCount].question;
            var corrAnswer = questions.results[questionCount].correct_answer;

            //place correct answer in list of possible answer buttons
            Random rand = new Random();
            int ansNum = rand.Next(4);
            Session["ansNum"] = ansNum;
            placeAnswer(corrAnswer, ansNum);

            //place incorrect answers
            bool corrAnsPassed = false;
            for (int i = 0; i < 4; i++)
            {
                if (i == ansNum)
                {
                    corrAnsPassed = true;
                    continue;
                }

                if (!corrAnsPassed)
                {
                    placeAnswer(questions.results[questionCount].incorrect_answers[i], i);
                }
                else
                {
                    placeAnswer(questions.results[questionCount].incorrect_answers[i - 1], i);
                }
                bool pageSet = true;
                Session["pageSet"] = pageSet;
            }
        }

        //place correct answer randomly in form
        public void placeAnswer(string answer, int numInList)
        {
            switch(numInList)
            {
                case 0: answerBtn1.Text = answer;
                    break;
                case 1: answerBtn2.Text = answer;
                    break;
                case 2: answerBtn3.Text = answer;
                    break;
                case 3: answerBtn4.Text = answer;
                    break;
                default:
                    break;
            }
        }

        //event handler for submit button
        protected void submitBtn_Click(object sender, EventArgs e)
        {
            string key = "";

            try
            {
                key = (string)Session["UserID"];
            }
            catch(Exception)
            {
                Response.Redirect("Default.aspx");
            }

            if (key == null || key == "")
            {
                Session["pageSet"] = false;
                Response.Redirect("Default.aspx", true);
            }

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

            for(int i = 0; i < 9; i++)
            {
                try
                {
                    doc = table.GetItem(key);
                    break;
                }
                catch(Amazon.DynamoDBv2.AmazonDynamoDBException)
                {
                    if(i == 8)
                    {
                        return;
                    }
                    else
                    {
                        waitTime(i);
                    }                  
                }
            }
            if(doc == null)
            {
                Response.Redirect("Default.aspx");
            }

            int score = Convert.ToInt32(doc["Score"]);
            int highScore = Convert.ToInt32(doc["HighScore"]);

            resultLabel.Visible = true;
            bool result = checkAnswer();
            if(result)
            {
                disableRadioButtons();

                //adjust user score
                score++;
                doc["Score"] = score;
                if(score > highScore)
                {
                    highScore = score;
                    doc["HighScore"] = highScore;
                }

                //print results
                resultLabel.Text = "You got it correct. Good job.";
                nextBtn.Visible = true;
                resultImg.Visible = true;
                submitBtn.Visible = false;
                resultImg.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/goldStar.jpg";

            }
            else
            {
                int numIncorrect = 0;
                try
                {
                    numIncorrect = (int)Session["CurIncorrect"] + 1;
                }
                catch(Exception)
                {
                    Response.Redirect("Category.aspx");
                }
                
                Session["CurIncorrect"] = numIncorrect;
                setIncorrectButtons(numIncorrect);               

                if(numIncorrect == 3)
                {
                    resultLabel.Text = "You got it wrong. GAME OVER!!!!";
                    doc["Score"] = 0;
                    submitBtn.Visible = false;
                    resultImg.Visible = true;
                    resultImg.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/poo.png";
                    restartBtn.Visible = true;
                    disableRadioButtons();
                }
                else
                {
                    resultLabel.Text = "You got it wrong. Bad job.";
                }

            }

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

            scoreLabel.Text = "Current Score: " + Convert.ToString(score);
            highScoreLabel.Text = "High Score: " + Convert.ToString(highScore);
        }

        //disable radio buttons
        public void disableRadioButtons()
        {
            answerBtn1.Enabled = false;
            answerBtn2.Enabled = false;
            answerBtn3.Enabled = false;
            answerBtn4.Enabled = false;
        }

        //check if correct answer is selected
        public bool checkAnswer()
        {
            int ansNum = 0;
            try
            {
                ansNum = (int)Session["ansNum"];
            }
            catch(Exception)
            {
                Response.Redirect("Category.aspx");
            }

            if(answerBtn1.Checked && ansNum == 0 ||
                answerBtn2.Checked && ansNum == 1 ||
                answerBtn3.Checked && ansNum == 2 ||
                answerBtn4.Checked && ansNum == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //next question button event handler
        protected void nextBtn_Click(object sender, EventArgs e)
        {
            resultImg.Visible = false;
            int questionCount = 0;
            try
            {
                questionCount = (int)Session["questionCount"];
            }
            catch (Exception) { }
            
            if(questionCount == 9)
            {
                //get more questions
                getNextBatch();
                questionCount = 0;
                bool pageSet = false;
                Session["pageSet"] = pageSet;
            }
            else
            {
                questionCount++;
            }

            Session["questionCount"] = questionCount;
            resultLabel.Text = "";
            resetButtons();
            nextBtn.Visible = false;
            setPage();
        }

        //uncheck radio buttons
        public void resetButtons()
        {
            answerBtn1.Checked = false;
            answerBtn2.Checked = false;
            answerBtn3.Checked = false;
            answerBtn4.Checked = false;
        }

        //get next batch of questions
        public void getNextBatch()
        {
            string url = "";
            try
            {
                url = (string)Session["url"];
            }
            catch(Exception)
            {
                Response.Redirect("Category.aspx", true);
            }
            if(url == "") { Response.Redirect("Category.aspx", true); }

            using (var client = new HttpClient())
            {
                HttpResponseMessage response = null;

                for(int i = 0; i < 12; i++)
                {
                    try
                    {
                        response = client.GetAsync(url).Result;
                        break;
                    }
                    catch(Exception)
                    {
                        if(i == 11)
                        {
                            Response.Redirect("Default.aspx");
                        }
                        else
                        {
                            waitTime(i);
                        }
                        
                    }
                }

                string message = response.Content.ReadAsStringAsync().Result;
                Questions questions = JsonConvert.DeserializeObject<Questions>(message);
                Session["questions"] = questions;
            }
        }

        //event handler for restart button
        protected void restartBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("Category.aspx", true);
        }

        //set images for incorrect answers
        public void setIncorrectButtons(int numIncorrect)
        {
            if(numIncorrect == 0)
            {
                attempt1Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/greenO.png";
                attempt2Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/greenO.png";
                attempt3Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/greenO.png";

            }
            else if(numIncorrect == 1)
            {
                attempt1Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/redX.png";
                attempt2Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/greenO.png";
                attempt3Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/greenO.png";
            }
            else if(numIncorrect == 2)
            {
                attempt1Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/redX.png";
                attempt2Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/redX.png";
                attempt3Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/greenO.png";
            }
            else
            {
                attempt1Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/redX.png";
                attempt2Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/redX.png";
                attempt3Img.ImageUrl = "http://d1258o4ayshbp3.cloudfront.net/redX.png";
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