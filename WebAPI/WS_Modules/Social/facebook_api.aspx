<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="facebook_api.aspx.cs" Inherits="WS_Social.facebook_api" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript">  
    function initFB() 
    {
        if (<%=IsInitFB() %>)
            window.location.href = '<%= GetFBURL() %>';
    }
    </script>
    <style type="text/css">
        *
        {
            font-family: "lucida grande" ,tahoma,verdana,arial,sans-serif;
            color: #203360;
        }
    </style>
</head>
<body onload="initFB()" style="text-align: center">
    <form id="form1" runat="server">
    <div>
        <div>
            <br />
            <asp:PlaceHolder runat="server" ID="innerIframe" Visible="false">
                <iframe runat="server" id="iframe" style="display: none"></iframe>
            </asp:PlaceHolder>
            <asp:Image ID="Image1" runat="server" Height="32px" ImageUrl="images/large-facebook.gif"
                Style="text-align: center" Width="32px" />
            <br />
        </div>
        <asp:Label ID="Lbl1" runat="server" Visible="true">Please wait while we connect with Facebook</asp:Label>
    </div>
    </form>
</body>
</html>
