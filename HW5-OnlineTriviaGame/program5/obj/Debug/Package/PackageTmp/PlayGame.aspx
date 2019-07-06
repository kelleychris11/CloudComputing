<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PlayGame.aspx.cs" Inherits="program5.PlayGame" MasterPageFile="~/Site.Master"%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
            <div>

            <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
            
        </div>
        <asp:RadioButton ID="answerBtn1" runat="server" GroupName="Answers" Enabled="true" />
        <br />
        <asp:RadioButton ID="answerBtn2" runat="server" GroupName="Answers" Enabled="true"/>
        <br />
        <asp:RadioButton ID="answerBtn3" runat="server" GroupName="Answers" Enabled="true"/>
        <br />
        <asp:RadioButton ID="answerBtn4" runat="server" GroupName="Answers" Enabled="true"/>
        <br />
        <asp:Button ID="submitBtn" runat="server" Text="Submit Answer" OnClick="submitBtn_Click" />
            <asp:Button ID="nextBtn" runat="server" Text="Next Question" OnClick="nextBtn_Click" Visible="false"/>
            <asp:Button ID="restartBtn" runat="server" Text="Restart Game" Visible="false" OnClick="restartBtn_Click" />
        <br />
            <asp:Label ID="attemptsLabel" runat="server" Text="Attempts Remaining: "></asp:Label>
            <asp:Image ID="attempt1Img" runat="server" Width="30" Height="30"/>
            <asp:Image ID="attempt2Img" runat="server" Width="30" Height="30"/>
            <asp:Image ID="attempt3Img" runat="server" Width="30" Height="30"/>
            <br />
            <br />
        <asp:Label ID="resultLabel" runat="server" Text=""></asp:Label>

            <asp:Image ID="resultImg" runat="server" Height="50" Width="50" Visible="false"/>

            <br />
            <p></p>
            <strong><asp:Label ID="scoreLabel" runat="server" Text=""></asp:Label></strong>

            <br />
            <strong><asp:Label ID="highScoreLabel" runat="server" Text=""></asp:Label></strong>

            <br />
            <br />
           <a runat="server" href="~/Default">Back to Login</a>
    </asp:Content>
