<%@ Page Language="C#" AutoEventWireup="true" CodeFile="InApp.aspx.cs" Inherits="InApp" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <div>Validation Receipts</div>
    <asp:TextBox id="txtReceipts" runat="server" TextMode="MultiLine"></asp:TextBox>
    <br />
    <asp:Button ID="btnValidationReceipts" runat="server" Text=" Validation Receipts" />
    <br />
    <asp:Label ID="lblValidateReceitsResponse" Text="Validation Receits Response:" runat="server"></asp:Label>
    <br />
    <asp:Label ID="lblValidateReceitsResponseResault" runat="server"></asp:Label>
    </div>
    </form>
</body>
</html>
