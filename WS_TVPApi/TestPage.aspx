<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TestPage.aspx.cs" Inherits="TestPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:Label ID="GetSiteMapTestLbl" Text="Initialization" Font-Bold="true" runat="server"></asp:Label>
    <hr />
    <asp:Label ID="SteGUIDLbl" runat="server" Text="Site Guid" Width="150px"></asp:Label>
    <asp:TextBox ID="SiteGuidTxt" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="CountryLbl" runat="server" Text="Locale Country" Width="150px"></asp:Label>
    <asp:TextBox ID="CountryTxt" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="LanguageLbl" runat="server" Text="Locale Language" Width="150px"></asp:Label>
    <asp:TextBox ID="LanguageText" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="DeviceLbl" runat="server" Text="Locale Device" Width="150px"></asp:Label>
    <asp:TextBox ID="DeviceTxt" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="PlatformLbl" runat="server" Text="Platform" Width="150px"></asp:Label>
    <asp:TextBox ID="PlatformTxt" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="UserNameLbl" runat="server" Text="User Name" Width="150px"></asp:Label>
    <asp:TextBox ID="UserNameTxt" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="PassLbl" runat="server" Text="Password" Width="150px"></asp:Label>
    <asp:TextBox ID="PassTxt" runat="server"></asp:TextBox>
    <br />
    <br />
    <hr />
    <asp:Label ID="GetSiteMapLbl" runat="server" Text="Get Site Map" Width="150px" Font-Underline="true"></asp:Label>
    <br />
    <br />
    <asp:Button ID="SiteMapTestBtn" Text="Submit" runat="server" OnClick="SiteMapTestClk" />
    <br />
    <br />
    <hr />
    
    <asp:Button ID="ClearCacheBtn" Text="Clear Cache" runat="server" OnClick="ClearCacheClk" />
    </div>
    </form>
</body>
</html>
