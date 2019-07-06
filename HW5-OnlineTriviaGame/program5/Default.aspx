<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="program5._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Login or Register to Play Trivia</h2>
    <a runat="server" href="~/Api">REST API Instructions</a>
    <p></p>
    <asp:Label ID="idLabel" runat="server" Text="UserID: " Width="65"></asp:Label>
    <asp:TextBox ID="userIDTxt" runat="server"></asp:TextBox>
    <p></p>
    <asp:Label ID="pwLabel" runat="server" Text="Password: "></asp:Label>
    <asp:TextBox ID="pwTxt" runat="server"></asp:TextBox>
    <p></p>
        <asp:Button ID="loginBtn" runat="server" Text="Login" OnClick="loginBtn_Click" />
    <p>
        <asp:Label ID="responseLabel" runat="server" Text=""></asp:Label>
    </p>
    <a runat="server" href="~/Register">Register</a>
 
 
        
    </asp:Content>
