<%@ Page Language="C#" AutoEventWireup="true" Inherits="_Default" Codebehind="Default.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
     <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js"></script>
</head>
<body>
<script>
    $.ajax({
        type: 'POST',
        url: 'http://localhost:8080/gateways/JsonPostGateway.aspx?m=GetSiteMap',
        data: { Media: 100 },
        success: undefined,
        dataType: 'json'
    });
    </script>
    <form id="form1" runat="server">
    <div>sdfs
   
    
    </div>
    </form>
</body>
</html>
