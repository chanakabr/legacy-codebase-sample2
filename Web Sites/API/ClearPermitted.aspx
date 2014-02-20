<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ClearPermitted.aspx.cs" Inherits="ClearPermitted" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script type="text/javascript">

    function pageLoad() {
        var itemsErrMsg = document.getElementById("ItemsErrorMsg");
        var subsErrMsg = document.getElementById("SubsErrorMsg");
        var permittedItemsCB = document.getElementById("PermittedItemsCB");
        var permittedSubsCB = document.getElementById("PermittedSubsCB");
        if (permittedItemsCB.checked) {
            if (itemsErrMsg != undefined && itemsErrMsg.value != '') {
                if (itemsErrMsg.value == 'Error') {
                    alert("Permitted Items Error Occured");
                }
                else if (itemsErrMsg.value == 'None') {
                    alert("Permitted Items Deleted")
                }
            }
            else {
                alert("Permitted Items Deleted");
            }
        }
        if (permittedSubsCB.checked) {
            if (subsErrMsg != undefined && subsErrMsg.value != '') {
                if (subsErrMsg.value == 'Error') {
                    alert("Permitted Subscriptions Error Occured");
                }
                else if (subsErrMsg.value == 'None') {
                    alert("Permitted Subscriptions Deleted")
                }
            }
            else {
                alert("Permitted Subscriptions Deleted");
            }
        }
    }
</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body onload="pageLoad()">
    <form id="form1" runat="server">
    <div>
    <asp:HiddenField ID="ItemsErrorMsg" runat="server" />
    <asp:HiddenField ID="SubsErrorMsg" runat="server" />
    <table>
    
    <tr>
    <td>
    <asp:Label runat="server" Text="UserName:"></asp:Label>
    </td>
    <td>
    <asp:TextBox ID="UNTxt" runat="server" ></asp:TextBox>
    </td>
    </tr>
    <tr>
    <td>
    <asp:Label ID="Label2" runat="server" Text="Password:"></asp:Label>
    </td>
    <td>
    <asp:TextBox ID="PassTxt" runat="server" ></asp:TextBox>
    </td>
    </tr>
    <tr>
    <td>
    <asp:Label runat="server" Text="Item ID (Comma Seperated, 0 for all):"></asp:Label>
    </td>
    <td>
    <asp:TextBox ID="ItemIDTxt" runat="server" ></asp:TextBox>
    </td>
    <td>
    <asp:CheckBox runat="server" ID="PermittedItemsCB" />
    </td>
    </tr>
    <tr>
    <td>
    <asp:Label ID="Label1" runat="server" Text="Subscription ID (Comma Seperated, 0 for all):"></asp:Label>
    </td>
    <td>
    <asp:TextBox ID="SubscriptionIDTxt" runat="server" ></asp:TextBox>
    </td>
    <td>
    <asp:CheckBox runat="server" ID="PermittedSubsCB" />
    </td>
    </tr>
    <tr>
    <td>
    <asp:Button runat="server" Text="Clear" OnClick="ClearButtonClick" />
    </td>
    </tr>
    </table>
    </div>
    </form>
</body>
</html>
