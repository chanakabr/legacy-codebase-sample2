<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PricingMonitoring.aspx.cs" Inherits="PricingMonitoring" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:Label runat="server"> TaskID  </asp:Label>
    <asp:TextBox runat="server" Text="0" ID="TaskID" ></asp:TextBox>

     <asp:Label ID="Label1" runat="server" Text="0"> IntervalInSec  </asp:Label>
    <asp:TextBox  runat="server" ID="IntervalInSec" Text="10"></asp:TextBox>

     <asp:Label ID="Label2" runat="server">  Parameters (groupID, UserGuid,MediaFiles)  </asp:Label>
    <asp:TextBox runat="server" ID="groupID" Text ="109" ></asp:TextBox>
    <asp:TextBox runat="server" ID="UserGuid" ></asp:TextBox>
    <asp:TextBox runat="server"  ID="MediaFiles" Text ="219307" ></asp:TextBox>


    <asp:Button ID="btnPriceMonitoring" runat="server" Text="Run Price Monitoring" />
    </div>
    </form>
</body>
</html>
