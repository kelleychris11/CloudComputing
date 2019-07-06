<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Program4._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <!--Buttons and Text Fields-->
    <asp:Button ID="LoadBtn" runat="server" Text="LoadData" OnClick="LoadBtn_Click" />
    <asp:Button ID="ClearBtn" runat="server" Text="Clear" OnClick="ClearBtn_Click" Width="142px" />

    <p></p>
    <p><asp:Label ID="Label1" runat="server" Text="FirstName:" Width="70px" Height="25px"></asp:Label>
        <asp:TextBox ID="FirstNameTxt" runat="server" Height="23px"></asp:TextBox>
    <p><asp:Label ID="Label2" runat="server" Text="LastName:" Width ="70px" Height="25px"></asp:Label>
    <asp:TextBox ID="LastNameTxt" runat="server" Height="21px"></asp:TextBox>
    </p>
    <p>
        <asp:Button ID="SubmitBtn" runat="server" Text="Submit Query" OnClick="SubmitBtn_Click" />
    </p>
    <p>
        &nbsp;</p>

    <!--Print results of loading data-->
        <%if (hasData) %>
    <%{ %>
    <div><strong><%Response.Write("Retrieved Data From: "); %></strong>
        <a href ="<%Response.Write(loadURL); %>"><%Response.Write(loadURL); %></a>
    </div>
        <div><strong><%Response.Write("Data Posted At: "); %></strong>
        <a href ="<%Response.Write(S3_URL); %>"><%Response.Write(S3_URL); %></a>
    </div>
    <%} %>

    <!--print message to user about action attempted-->
    <div><strong><%Response.Write(responseMessage); %></strong></div>

    <!--Print results of Query-->
    <%if (hasResponse) %>
    <%{ %>
        <div><strong><%Response.Write("_________________________________"); %></strong></div>
        <%for (int i = 0; i < responseList.Count; i++) %>
        <%{ %>
        <div><strong><%Response.Write(responseList[i].sortKey + " "); %>
        <%Response.Write(responseList[i].partKey); %></strong></div>

        <%for (int j = 0; j < responseList[i].key.Count; j++) %>
        <%{ %>
       <div> <%Response.Write(responseList[i].key[j] + " = "); %>
        <%Response.Write(responseList[i].value[j] + "\n"); %> </div>
        <%} %>
        <%if (i == responseList.Count - 1) %>
        <%{ %>
        <div><strong><%Response.Write("_________________________________");%></strong></div>
        <%} %>
        <%else %>
        <%{ %>      
        <div><%Response.Write("_________________________________"); %></div>
        <%} %>
    <%} %>
    
    <%} %>
     
</asp:Content>
