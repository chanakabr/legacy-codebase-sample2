<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_channel_media_types_popup_selector.aspx.cs" Inherits="adm_channel_media_types_popup_selector" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>   
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1255" />
	<title><% TVinciShared.PageUtils.GetTitle(); %></title>
    <meta content="" name="Description" />
    <meta content="all" name="robots" />
    <meta content="1 days" name="revisit-after" />
    <meta content="Guy Barkan" name="Author" />
    <meta content="<% TVinciShared.PageUtils.GetKeyWords(); %>" name="Keywords" />
    <meta http-equiv="Pragma" content="no-cache" />
    <link href="css/styles-en.css" type="text/css" rel="stylesheet" />
    <link href="components/duallist/css/duallist.css" type="text/css" rel="stylesheet" />
    <script language="JavaScript" src="js/jquery-1.10.2.min.js" type="text/javascript"></script>
    <script language="JavaScript" src="js/jquery-placeholder.js" type="text/javascript"></script>
    <script src="https://code.jquery.com/ui/1.10.3/jquery-ui.js"></script>
    <script language="JavaScript" src="js/rs.js" type="text/javascript"></script>
    <script language="JavaScript" src="js/adm_utils.js" type="text/javascript"></script>
    <script language="JavaScript" src="js/ajaxFuncs.js" type="text/javascript"></script>
    <script type="text/javascript" src="js/SWFObj.js" language="javascript"></script>
    <script type="text/javascript" src="js/WMPInterface.js" language="javascript"></script>
    <script type="text/javascript" src="js/WMPObject.js" language="javascript"></script>
    <script type="text/javascript" src="js/FlashUtils.js" language="javascript"></script>
    <script type="text/javascript" src="js/Player.js" language="javascript"></script>
    <script type="text/javascript" src="js/VGObject.js" language="javascript"></script>
    <!-- dual list -->
    <script type="text/javascript" src="components/duallist/js/script.js"></script>
    <script type="text/javascript" src="components/duallist/js/info.js"></script>
    <script type="text/javascript" src="components/duallist/js/calender.js"></script>
    <script type="text/javascript" src="components/duallist/js/list.js"></script>
    <script type="text/javascript" src="components/duallist/js/duallist.js"></script>
    <!-- end dual list -->

</head>
<body class="admin_body" onload="initDuallistObj('adm_channel_media_types_popup_selector.aspx')">

    <form method="post" id="form1" action="" runat="server">
          <tr>              
                <td id="page_content">
                    <div id="DualListPH"></div>
                </td>
          </tr>        
    </form>
    
</body>
</html>
