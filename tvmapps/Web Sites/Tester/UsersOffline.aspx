<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UsersOffline.aspx.cs" Inherits="UsersOffline" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button ID="btnAddOfflineItem" Text="Add Offline Item" runat="server" />
        <asp:Label ID="lblResault" runat="server" Text="N/A"></asp:Label>
    </div>
    <hr />
     <div>
        <asp:Button ID="btnRemoveOfflineItem" Text="Remove Offline Item" runat="server" />
        <asp:Label ID="lblRemove" runat="server" Text="N/A"></asp:Label>
    </div>
   <hr />
     <div>
        <asp:Button ID="btmClearOfflineItem" Text="Clear Offline Item" runat="server" />
        <asp:Label ID="lblClear" runat="server" Text="N/A"></asp:Label>
    </div>
    <hr />
    <div style="">
        <asp:Button Enabled="false" ID="btnGetOfflineItem" Text="Get Offline Item" runat="server" />
        <asp:Label ID="lblGetResault" runat="server" Text="0"></asp:Label>
        <asp:Repeater ID="repeaterOfflineREsault" runat="server">
            <ItemTemplate>
            </ItemTemplate>
            <SeparatorTemplate>
                <hr />
            </SeparatorTemplate>
        </asp:Repeater>
    </div>

     <div>
        <asp:Button ID="btnAllGetOfflineItem" Text="Get All Offline Item" runat="server" />
        <asp:Label ID="lblAllGetOfflineItem" runat="server" Text="0"></asp:Label>
        <asp:Repeater ID="repeaterAllOfflineREsault" runat="server">
            <ItemTemplate>
            </ItemTemplate>
            <SeparatorTemplate>
                <hr />
            </SeparatorTemplate>
        </asp:Repeater>
    </div>
     <hr />
    <div>
        <asp:Button ID="btnGetItemLeftViewLifeCycle" Text="Get Item Left View Life Cycle" runat="server" />
        <asp:Label ID="lblGetItemLeftViewLifeCycle" runat="server" Text="0"></asp:Label>
     
    </div>
     
        <asp:Button ID="btnGetLicensedLink" Text="Get Licensed Link" runat="server" />
        <asp:Label ID="lblGetLicensedLink" runat="server" Text="0"></asp:Label>


         
        <asp:Button ID="btnGetsubscriptionprice" Text="Get subscription price" runat="server" />
    </form>
</body>
</html>
