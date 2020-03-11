<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FBTest.aspx.cs" Inherits="FBTest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <a href="https://www.facebook.com/dialog/oauth?client_id=397481886958054&redirect_uri=http://192.168.16.124/TVMTest/fbtest.aspx&scope=publish_stream" > Login </a>
    <br />
    Token: <asp:TextBox runat="server" ID="TokenTxt"></asp:TextBox>
    </div>
    <asp:Literal runat="server" ID="FriendsLit"></asp:Literal>
    <asp:ImageButton ID="IB1" runat="server" ImageUrl="~/image.jpg" PostBackUrl="http://www.tvinci.com" />
    </form>
</body>
</html>
