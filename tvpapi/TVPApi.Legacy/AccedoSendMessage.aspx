<%@ Page Language="C#" AutoEventWireup="true" Inherits="AccedoSendMessage" Codebehind="AccedoSendMessage.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.4/jquery.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Create message:
        <table border="0">
            <tr>
                <td>SiteGUID:</td>
                <td><input type="text" id="tbSiteGUID" /></td>
            </tr>
            <tr>
                <td>Reciever UDID:</td>
                <td><input type="text" id="tbRecieverUDID" /></td>
            </tr>
            <tr>
                <td>MediaID:</td>
                <td><input type="text" id="tbMediaID" value="0" /></td>
            </tr>
            <tr>
                <td>MediaTypeID:</td>
                <td><input type="text" id="tbMediaTypeID" value="0" /></td>
            </tr>
            <tr>
                <td>Location:</td>
                <td><input type="text" id="tbLocation" value="0" /></td>
            </tr>
            <tr>
                <td>Action:</td>
                <td><input type="text" id="tbAction" /></td>
            </tr>
            <tr>
                <td>Username:</td>
                <td><input type="text" id="tbUsername" /></td>
            </tr>
            <tr>
                <td>Password:</td>
                <td><input type="text" id="tbPassword" /></td>
            </tr>
            <tr>
                <td><input type="button" value="Send message" onclick="SendMessage();" /></td>
            </tr>
        </table>
        <br />
        <br />
        <div id="dvResponse" style="display:none;"></div>
        
    </div>
    </form>

    <script type="text/javascript">
        function SendMessage() {
            var sSiteGUID = document.getElementById('tbSiteGUID').value;
            var sRecieverUDID = document.getElementById('tbRecieverUDID').value;
            var sMediaID = document.getElementById('tbMediaID').value;
            var sMediaTypeID = document.getElementById('tbMediaTypeID').value;
            var sLocation = document.getElementById('tbLocation').value;
            var sAction = document.getElementById('tbAction').value;
            var sUsername = document.getElementById('tbUsername').value;
            var sPassword = document.getElementById('tbPassword').value;

            $.ajax
            ({
                type: "POST",
                url: 'http://localhost/tvpapi/gateways/jsonpostgw.aspx?m=SendMessage',
                dataType: 'json',
                async: true,
                data: '{"sSiteGuid": "' + sSiteGUID + '","sRecieverUDID": "' + sRecieverUDID + '","iMediaID": ' + sMediaID + ',"iMediaTypeID": ' + sMediaTypeID + ',"iLocation": ' + sLocation + ',"sAction": "'+sAction+'","sUsername": "'+sUsername+'","sPassword": "'+sPassword+'"}',
                success: function (msg) {
                    document.getElementById('dvResponse').innerHTML = 'The following JSON object sent to device:<br/><br/>{"sSiteGuid": "' + sSiteGUID + '","sRecieverUDID": "' + sRecieverUDID + '","iMediaID": ' + sMediaID + ',"iMediaTypeID": ' + sMediaTypeID + ',"iLocation": ' + sLocation + ',"sAction": "' + sAction + '","sUsername": "' + sUsername + '","sPassword": "' + sPassword + '"}';
                    document.getElementById('dvResponse').style.display = 'block';
                }
            })
        }


    </script>
</body>
</html>
