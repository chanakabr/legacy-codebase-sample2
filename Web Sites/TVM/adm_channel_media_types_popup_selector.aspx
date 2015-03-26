<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_channel_media_types_popup_selector.aspx.cs" Inherits="adm_channel_media_types_popup_selector" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

   <meta http-equiv="Content-Type" content="text/html; charset=windows-1255"/>
	<%--	<title><% TVinciShared.PageUtils.GetTitle(); %></title>--%>
		<meta content="" name="description"/>
		<meta name="robots" content="all"/>
		<meta name="revisit-after" content="1 days"/>
		<meta name="Author" content="Guy Barkan"/>
		<meta content="<% TVinciShared.PageUtils.GetKeyWords(); %>" name="keywords"/>
		<META http-equiv="Pragma" content="no-cache">
		<link href="css/styles-en.css" type="text/css" rel="stylesheet"/>
		<link href="css/addPic-en.css" type="text/css" rel="stylesheet"/>
		<script type="text/javascript" language="JavaScript" src="js/rs.js"></script>
		<script type="text/javascript" language="JavaScript" src="js/adm_utils.js"></script>
		<script type="text/javascript" language="JavaScript" src="js/utils.js"></script>
		<script type="text/javascript" language="JavaScript" src="js/AnchorPosition.js"></script>
		<script type="text/javascript" language="JavaScript" src="js/dom-drag.js"></script>
       <script type="text/javascript" src="js/SWFObj.js" language="javascript"></script> 

    <script type="text/javascript">
        var flashObj1 = new SWFObj
        (
         'codebase', 'http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,0,0',
         'width', '100%',
         'height', '100%',
         'src', 'flash/DualList',
         'scale', 'NoScale',
         'id', 'DualList',
         'bgcolor', '#869CA7',
         'wmode', 'Window',
         'name', 'DualList',
         'allowFullScreen', 'false',
         'allowScriptAccess', 'sameDomain',
         'movie', 'flash/DualList'
        );

        function flashEvents(json) {
         
            switch (json.eventType) {
                case "move":
                    if (json.callerID == 'DualList') {
                        RS.Execute("adm_channel_media_types_popup_selector.aspx", "changeItemStatus", json.id, json.kindc, callback_changeItemStatus, errorCallback);
                    }
                    break;
                case "ready":
                    if (json.id == 'DualList') {
                        var flashObj1 = document.getElementById(json.id);
                        initDualObj();
                    }
                    break;
            }
        }

        function callback_changeItemStatus(ret) {
        }

        function callback_changeItemStatus_UserTypes(ret) {
        }

        function AfterDateSelect(orderBy, pageNum, theDate) {
        }
        function GetPageTable(orderBy, pageNum) {
            flashObj1.write("DualListPH");          
        }

        function initDualObj() {
            RS.Execute("adm_channel_media_types_popup_selector.aspx", "initDualObj", callback_init_dobj, errorCallback);
        }
        function callback_init_dobj(ret) {
            var flashObj1 = document.getElementById("DualList");


            var split_array = ret.split("~~|~~");

            if (split_array.length == 3) {
                theTitle1 = split_array[0];
                theTitle2 = split_array[1];
                var xmlStr = split_array[2];
                flashObj1.callFlashAction({ action: "setList", data: xmlStr, title1: theTitle1, title2: theTitle2 });
            }
        }
</script>

    
</head>
<body class="admin_body" onload="GetPageTable('' , 0);">
    <form method="post"  id="form2" runat="server">
   
        <input type="file" runat=server id="file_marker" name="file_marker" style="width: 0px; height: 0px; display: none;" />
        <div id="tag_collections_div" class="floating_div"></div>
        <div>
          <tr><td><div id="DualListPH"></div></td></tr>
        </div>
    </form>
    
</body>
</html>
