<%@ page language="C#" autoeventwireup="true" inherits="admin_tree_player, App_Web_tl6m4-ya" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>video_player</title>
    <script src="js/SWFObj.js" language="javascript"></script>
    <script src="js/WMPInterface.js" language="javascript"></script>
    <script src="js/FlashUtils.js" language="javascript"></script>
    <script src="js/WMPObject.js" language="javascript"></script>
    <script language="javascript">
    //------------------------------------------------
    //---------------------- FLASH OBJECT ------------
    var flashObj = new SWFObj(
    'codebase', 'http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,0,0',
    'width', '800px',
    'height', '300px',
    'src', 'flash/admin_tree_player',
    'quality', 'high',
    'pluginspage', 'http://www.macromedia.com/go/getflashplayer',
    'align', 'middle',
    'play', 'true',
    'loop', 'true',
    'scale', 'showall',
    'wmode', 'transparent',
    'devicefont', 'false',
    'id', 'player',
    'bgcolor', '#999999',
    'name', 'player',
    'menu', 'true',
    'allowFullScreen', 'true',
    'allowScriptAccess','always',
    'movie', 'flash/admin_tree_player',
    'salign', '',
    'flashVars', '<% GetFlashVars(); %>'

    );
    //------------------------------------------------
    //---------------------- WMP OBJECT --------------
    var wmpVid = new WMPObject("", "WMPObj", 0, 0);
    wmpVid.addParam('TYPE', 'application/x-mplayer2');
    wmpVid.addParam('PLUGINSPACE', 'http://www.microsoft.com/Windows/MediaPlayer/download/default.asp');
    wmpVid.addParam('Autostart', 'false'); 
    wmpVid.addParam('uiMode', 'none'); 
    wmpVid.addParam('windowlessVideo', 'true'); 
    wmpVid.addParam('stretchToFit', 'true'); 
    wmpVid.addParam('ShowControls', '0');
    //------------------------------------------------

    function initPage(){
	    flashObj.write("flashDiv");
	    wmpVid.write("WMPDiv");
	    var interface = createWMPInterface();
	    addInterfaceToCall(interface, "VideoInterface");
	    var flashObject = document.getElementsByName("player")[0];
	    //flashObject.setFlashID("tempid01");
	    interface.init(document.getElementById("WMPObj"),document.getElementById("WMPDiv"),flashObject,document.getElementById("flashDiv"));
    }

    function selectTreeItem(flashID, itemType, itemID){
	    window.status = [flashID, itemType, itemID];
    }


</script>
</head>
<body bgcolor="#545454" style="margin:0px;" onload="initPage();">
	<div id="WMPDiv" style="position:absolute;z-index:0;top:0;left:0"></div>
	<div id="flashDiv" style="position:absolute;z-index:1;top:0;left:0;width:100%;height:100%;"></div>
</body>
</html>