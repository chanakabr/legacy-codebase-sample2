<%@ Page Language="C#" AutoEventWireup="true" Inherits="admin_player" Codebehind="admin_player.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>video_player</title>
    
    <script type="text/javascript" language="javascript">
        AC_FL_RunContent = 0;
    </script>
    <script type="text/javascript" src="js/AC_RunActiveContent.js" language="javascript"></script>
    <script type="text/javascript" src="js/adm_utils.js" language="javascript"></script>
    <script type="text/javascript" src="js/SWFObj.js" language="javascript"></script>
    <script type="text/javascript" src="js/WMPInterface.js" language="javascript"></script>
    <script type="text/javascript" src="js/WMPObject.js" language="javascript"></script>
    <script type="text/javascript" src="js/FlashUtils.js" language="javascript"></script>
    <script type="text/javascript" src="js/Player.js" language="javascript"></script>
    <script type="text/javascript" src="js/VGObject.js" language="javascript"></script>
   
    <script type="text/javascript">
        function initPage()
        {    
            var flashObj = new SWFObj
            (
                'codebase', 'http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,0,0',
                'width', '<% GetPlayerWidth(); %>',
                'height', '<% GetPlayerHeight(); %>',
                'src', 'flash/lucy_player_admin',
                'quality', 'high',
                'pluginspage', 'http://www.macromedia.com/go/getflashplayer',
                'align', 'right',
                'play', 'true',
                'loop', 'true',
                'scale', 'showall',
                'devicefont', 'false',
                'id', 'player1',
                'bgcolor', '#ffffff',
                'wmode', 'transparent',
                'name', 'player',
                'menu', 'true',
                'allowFullScreen', 'true',
                'allowScriptAccess','sameDomain',
                'movie', 'flash/lucy_player_admin',
                'salign', '',
                'flashVars', '<% GetFlashVars(); %>'
            ); //end AC code
            flashObj.write("flashDiv");
            var interface = createWMPInterface();
            addInterfaceToCall(interface, "VideoWMPInterface");
            addInterfaceToCall(interface, "VideoInterface");
            interface.init(window.document.getElementById("WMPObj"), window.document.getElementById("WMPDiv"), window.document.getElementsByName("player")[0], window.document.getElementById("flashDiv"));
        }
    </script>
</head>
<body style="padding: 0 0 0 0; margin: 0 0 0 0;" onload="initPage();">
    <form id="form1" runat="server">

            <div id="WMPDiv" style="position:absolute;z-index:0;top:0;left:0">
                <object id="WMPObj" style="DISPLAY: none" height="0" width="0" classid="CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6">
                    <PARAM value="application/x-mplayer2" name="TYPE" />
                    <PARAM value="http://www.microsoft.com/Windows/MediaPlayer/download/default.asp" name="PLUGINSPACE" />
                    <PARAM value="false" name="Autostart" />
                    <PARAM value="none" name="uiMode" />
                    <PARAM value="flase" name="windowlessVideo" />
                    <PARAM value="true" name="stretchToFit" />
                    <PARAM value="0" name="ShowControls" />
                    <PARAM value="" name="src" />
                </object>
            </div>
            <div id="flashDiv" style="position:absolute;z-index:1;top:0;left:0;width:100%;height:100%;"></div>        
        
        <% GetNDSIframe(); %>
    </form>
</body>
</html>
