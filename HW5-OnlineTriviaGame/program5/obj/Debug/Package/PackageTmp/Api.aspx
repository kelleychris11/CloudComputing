<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Api.aspx.cs" Inherits="program5.Api" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <h1>Retrieve quiz user data in JSON format</h1>
        <div>
             <strong>Get a list of all users:</strong>
         <font color="red">http://p5api.us-west-2.elasticbeanstalk.com/api/users</font>   
        </div>
        <p>Data returned for each user is:<font color="blue"> UserID, Score, High Score, & Number of Quiz Attempts</font></p>
        <p></p>
        <div><strong>Get data for individual users:</strong> 
            <font color="red">http://p5api.us-west-2.elasticbeanstalk.com/api/users?username={username}</font> </div>
        <p><div><font color="green">Example: retrieve data for user: 'powerplayer'</div> 
            <div>http://p5api.us-west-2.elasticbeanstalk.com/api/users?username=powerplayer</div></font></p>
        <p>Data returned for individual users is:<font color="blue"> UserID, Score, High Score, & Number of Quiz Attempts</font></p>
        <p><a runat="server" href="~/Default">Back to Login</a></p>
    </form>
</body>
</html>
