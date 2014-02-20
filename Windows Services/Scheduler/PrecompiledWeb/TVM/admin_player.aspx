<%@ page language="C#" autoeventwireup="true" inherits="admin_player, App_Web__-8binqh" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>video_player</title>
    <script type="text/javascript" language="javascript">AC_FL_RunContent = 0;</script>
    <script type="text/javascript" src="js/AC_RunActiveContent.js" language="javascript"></script>
    <script type="text/javascript" src="js/SWFObj.js" language="javascript"></script>
    <script type="text/javascript" src="js/WMPInterface.js" language="javascript"></script>
    <script type="text/javascript" src="js/WMPObject.js" language="javascript"></script>
    <script type="text/javascript" src="js/FlashUtils.js" language="javascript"></script>
    <script type="text/javascript">
        function initPage()
        {    
            var flashObj = new SWFObj
            (
                'codebase', 'http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,0,0',
                'width', '<% GetPlayerWidth(); %>',
                'height', '<% GetPlayerHeight(); %>',
                'src', 'flash/video_player',
                'quality', 'high',
                'pluginspage', 'http://www.macromedia.com/go/getflashplayer',
                'align', 'right',
                'play', 'true',
                'loop', 'true',
                'scale', 'showall',
                'devicefont', 'false',
                'id', 'video_player',
                'bgcolor', '#ffffff',
                'wmode', 'transparent',
                'name', 'video_player',
                'menu', 'true',
                'allowFullScreen', 'true',
                'allowScriptAccess','sameDomain',
                'movie', 'flash/video_player',
                'salign', '',
                'flashVars', '<% GetFlashVars(); %>'
            ); //end AC code
            var wmpVid = new WMPObject("", "WMPObj", 0, 0);
            wmpVid.addParam('TYPE', 'application/x-mplayer2');
            wmpVid.addParam('PLUGINSPACE', 'http://www.microsoft.com/Windows/MediaPlayer/download/default.asp');
            wmpVid.addParam('Autostart', 'false'); 
            wmpVid.addParam('uiMode', 'none'); 
            wmpVid.addParam('windowlessVideo', 'true'); 
            wmpVid.addParam('stretchToFit', 'true'); 
            wmpVid.addParam('ShowControls', '0');
            
            flashObj.write("flashDiv");
            wmpVid.write("WMPDiv");
            var interface = createWMPInterface();
            addInterfaceToCall(interface, "VideoInterface");
            interface.init(window.document.getElementById("WMPObj"),window.document.getElementById("WMPDiv"),window.document.getElementsByName("video_player")[0],window.document.getElementById("flashDiv"));
        }
    </script>
</head>
<body style="padding: 0 0 0 0; margin: 0 0 0 0;" onload="initPage();">
    <form id="form1" runat="server">
        <!--div style="position:absolute;top:0;left:0;"-->
            <div id="WMPDiv" style="position:absolute;z-index:0;top:0;left:0"></div>
            <div id="flashDiv" style="position:absolute;z-index:1;top:0;left:0;width:100%;height:100%;"></div>        
        <!--/div-->
    </form>
</body>
</html>
