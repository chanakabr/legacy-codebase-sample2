<%@ Page Language="C#" AutoEventWireup="true" Inherits="adm_comments_filter_tester_popup" Codebehind="adm_comments_filter_tester_popup.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
        <meta http-equiv="Content-Type" content="text/html; charset=windows-1255"/>
		<title><% TVinciShared.PageUtils.GetTitle(); %></title>
		<meta content="" name="description"/>
		<meta name="robots" content="all"/>
		<meta name="revisit-after" content="1 days"/>
		<meta name="Author" content="Guy Barkan"/>
		<meta content="<% TVinciShared.PageUtils.GetKeyWords(); %>" name="keywords"/>
		<meta http-equiv="Pragma" content="no-cache"/>
		<link href="css/styles-en.css" type="text/css" rel="stylesheet"/>
		<link href="css/addPic-en.css" type="text/css" rel="stylesheet"/>
        <link href="css/styles-en.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        
        <asp:TextBox ID="TextBox1" runat="server" Width="378px" TextMode="multiline" 
            Height="119px"></asp:TextBox>
        &nbsp;&nbsp;&nbsp;
        
        <asp:Button ID="Button1" runat="server" onclick="Button1_Click" 
            PostBackUrl="~/adm_comments_filter_tester_popup.aspx" Text="Test" />
        
        <br />
        <br />
        <asp:TextBox ID="TextBox2" runat="server" Enabled="False" Height="119px" TextMode="multiline"
            Width="378px"></asp:TextBox>
        <br />
        

    </div>
    </form>
</body>
</html>



