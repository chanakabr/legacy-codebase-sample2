<%@ Page Language="C#" AutoEventWireup="true" CodeFile="EPGTester.aspx.cs" Inherits="EPGTester" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div style="text-align:center;">
    <img src="/images/KfarQaraBG.jpg" />
    <asp:Button ID="btnTestEPG" Text="Start Test EPG" runat="server" Enabled=TRUE />
    <br />
    <asp:Button ID="btnMediaCorpEPG" Text="Media Corp Start Test EPG" runat="server"  />
    <br />
    <asp:Button ID="btnMediaCorpxmltvEPG" Text="Media Corpxmltv EPG" runat="server" Enabled=true />
    <br />
    <asp:Button ID="btnMediaCorpXMLNodeEPG" text="Media Corp XML Node EPG" runat="server" Enabled="true" />
    <br />
    <asp:Button ID="btnYesEPG" text="Yes XMLtv EPG" runat="server" Enabled="true" />
    </div>
    </form>
</body>
</html>
