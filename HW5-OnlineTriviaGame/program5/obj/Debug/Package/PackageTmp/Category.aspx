<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Category.aspx.cs" Inherits="program5.Category" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Choose Your Category</h2>
            <asp:Label ID="categoryLabel" runat="server" Text="Select Category: "></asp:Label>
            <asp:DropDownList ID="catList" runat="server">
            </asp:DropDownList>
            <br />
            <br />
            <asp:Button ID="playGameBtn" runat="server" Text="Play Game" OnClick="playGameBtn_Click" />
        </div>
    </form>
</body>
</html>
