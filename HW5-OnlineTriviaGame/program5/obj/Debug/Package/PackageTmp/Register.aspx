<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="program5.Register" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Register New UserName and Password</h2>
            <asp:Label ID="idLabel" runat="server" Text="UserID: " Width="65"></asp:Label>
            <asp:TextBox ID="userIDTxt" runat="server"></asp:TextBox>
        </div>
        <asp:Label ID="pwLabel" runat="server" Text="Password: "></asp:Label>
        <asp:TextBox ID="pwTxt" runat="server"></asp:TextBox>
        <p>
            <asp:Button ID="submitBtn" runat="server" Text="Submit" OnClick="submitBtn_Click" />
        </p>
        <p>
        <asp:Label ID="responseLabel" runat="server" Text=""></asp:Label>

        </p>
        <a runat="server" href="~/Default">Back to Login</a>
        <br />

    </form>
</body>
</html>
