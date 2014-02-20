<%@ page language="C#" autoeventwireup="true" inherits="NDSPlayer, App_Web_sf4fen6x" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Untitled Page</title>
</head>
<body>

<script type="text/javascript" src="js/PlayerMessages.js" language="javascript"></script>

<object id='theVgdk' CLASSID='clsid:25827FF5-104D-42ab-A802-CDCD838DD02B' BORDER=0 VSPACE=0 HSPACE=0 ALIGN=TOP HEIGHT=0% WIDTH=0%></object>
  
  <script type="text/javascript">
  
  top.GetVG().replaceVGDK(theVgdk);
  
  function getFlashObject()
	{    
		var element = top.document.getElementsByName("player");
    		if (element != 'undefined' && element != null)
    		{
	    		return element[0];
		}else
		{
		    return null;
		}
	}

	function SendEventToFlash(eventCode , theMessage , theType , theAction)
	{
		try
		{
			getFlashObject().jsEventHandler(eventCode, theMessage, theType, theAction);	
		}
		catch(theException){}
	}

	function theVgdk::Error(AssetID, ErrorID, ErrorTxt) 
	{	    
		var errItem;		
		errItem = findErr(ErrorTxt); 		 		
  		if (errItem == null)
			return;
  		if (typeof top.TvinciNDSAddToLog == 'function')   
  		{
  		    top.TvinciNDSAddToLog(errItem[ErrType],errItem[ErrDescription], ErrorTxt);
  		}
  		else
  		{  		    
  		}
		SendEventToFlash(errItem[ErrCode] , errItem[ErrDescription] , errItem[ErrPlayerBufferAction] , errItem[ErrPlayerAction]);
	}
  </script>


</body>
</html>
