<%@ page language="C#" autoeventwireup="true" inherits="NDSPlayer_NDSInitialize, App_Web_sf4fen6x" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
        
    <script type="text/javascript" src="js/PlayerMessages.js" language="javascript"></script>
    <script type="text/javascript" src="js/VGObject.js" language="javascript"></script>
    
    <script type="text/javascript">
    var VG = null;

function GetVG()
{
    if (VG == null)
        VG = new VGObject();
    return VG;
}            

      if ( window.ActiveXObject )	{ // IE      
	        try 
	        {				        
                var actualVgdk = new ActiveXObject("NDSAssetMgr.AssetMgrGIB");																		            			
                GetVG().replaceVGDK(actualVgdk);
                
                GetVG().notifyEvent = function(event)                
                {        
                    if (event == "Initialized")
                    {                        
			            GetVG().replaceVGDK(null);
                        window.document.getElementsByName("NDSPlayer")[0].src = "NDSPlayer.aspx";                    
                    }                
                }                
	        }
	        catch(e)
	        {	        
	        }		
        } 		       		    		    					
    </script>
    
    <iframe id="NDSPlayer" name="NDSPlayer" src=""></iframe>
    </form>
</body>
</html>
