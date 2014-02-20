var m_NDSPlayTimer;
var m_NDSValidationTimer;
var m_NDSResponseUIDCounter = 0;
var m_NDSValidationUIDCounter = 0;
var m_MaxLoadCalls = 15;
var m_MaxUpgradeCalls = 90;

//Dummy functions --------------------------

function NDSPlay(theValues){
	try
	{
	//alert(1)
    //TvinciPlayerAddToLog("NDSPlay - Start play request {msisdn,pass,assetID,FECM,duration,cp,ap, language (optional)},{" + theValues + "}");
	m_NDSResponseUIDCounter ++;
	clearTimeout(m_NDSPlayTimer);
	m_NDSPlayTimer = setTimeout("NDSPlaySendToFlash('"+theValues+"',"+m_NDSResponseUIDCounter+" , 0 , '')", 40);			
	}
	catch (e)
	{
		alert(e.message)
	}


}

function NDSValidateInstallationSendToFlash(theStatus)
{
    var flashObj = getFlashObject(); 
    if (flashObj != 'undefined' && flashObj != null)
    {	    
        flashObj.NDSValidated(theStatus); 
    }	
}


function NDSValidateInstallation(userPass)
{     
    var arr = userPass.toString().split('|');
    var userName = arr[0];
    var password =arr[1];
    
    var isIE = (navigator.appName == "Microsoft Internet Explorer");   
    if (isIE==false) 
    {
        NDSValidateInstallationSendToFlash("$ERROR");
        return;
    }
                              
    if (!GetVG().getIsInitialized())
    {  
        GetVG().initializeVgdk(userName,password);
    }
                
    if(GetVG().getRequiresInstallOrUpgrade())
    {        
        var theVersion = GetVG().getVersion();
        if(theVersion == null || theVersion == 'undefined' || theVersion.indexOf("?") > -1) 
        {
            NDSValidateInstallationSendToFlash("$INSTALL_REQUIRED");            
            return;
        } else 
        {
            NDSValidateInstallationSendToFlash("$UPGRADE_REQUIRED");
            return;
        }
    }   
    
    NDSValidateInstallationSendToFlash("$OK");
    return;
}


function NDSPlaySendToFlash(theValues, uid , nCounter , last_status){
	nCounter++;
	
	if (nCounter == 1)
	{
	    TvinciPlayerAddToLog("NDSPlaySendToFlash - Executed for uid '" + uid + "'");
	
	}else
	{
	    TvinciPlayerAddToLog("NDSPlaySendToFlash - Executed recursive for uid '" + uid + "'. previous status '" + last_status + "'. attempt number '" + nCounter + "'");	    
	}
		
	if (last_status == "$LOCAL_RETRY" && nCounter == m_MaxLoadCalls)
	{
	    TvinciPlayerAddToLog('NDSPlaySendToFlash - maximum attempts reached. return to flash response \'$LOAD_TIMEOUT\'');
		getFlashObject().getHandleNDSResponse("$LOAD_TIMEOUT");
		return;
	}
	
	if (last_status == "$INSTALL_UPGRADE_REQUIRED" )//&& nCounter == m_MaxUpgradeCalls
	{
	    TvinciPlayerAddToLog('NDSPlaySendToFlash - maximum attempts reached. return to flash response \'$UPGRADE_TIMEOUT\'');
		getFlashObject().getHandleNDSResponse("$UPGRADE_TIMEOUT");
		return;
	}

	var ret = NDSPlayReturn(theValues);

	switch(ret){
		case "$LOCAL_RETRY":
		    TvinciPlayerAddToLog("NDSPlaySendToFlash - Retring to execute method. Attempt number '" + nCounter + "' (max attempts allowed'" + m_MaxLoadCalls + "')");
			clearTimeout(m_NDSPlayTimer);
			m_NDSPlayTimer = setTimeout("NDSPlaySendToFlash('"+theValues+"',"+uid+","+ nCounter +" , '$LOCAL_RETRY')", 1000);			
			break;
//		case "$INSTALL_UPGRADE_REQUIRED":
//		    TvinciPlayerAddToLog("NDSPlaySendToFlash - Retring to execute method. Attempt number '" + nCounter + "' (max attempts allowed'" + m_MaxUpgradeCalls + "')");
//			clearTimeout(m_NDSPlayTimer);
//			m_NDSPlayTimer = setTimeout("NDSPlaySendToFlash('"+theValues+"',"+uid+","+ nCounter +" , '$INSTALL_UPGRADE_REQUIRED')", 1000);			
//			break;
	}
	if(uid != m_NDSResponseUIDCounter)
		return;
	var flashObj = getFlashObject(); 
    TvinciPlayerAddToLog('NDSPlaySendToFlash - returning to flash with result \'' + ret + '\'');	
	flashObj.getHandleNDSResponse(ret);
}

function NDSPlayReturn(theValues)
{        
    try
    {   	    
        var theValusArray = new Array();
        theValusArray = theValues.split(',');
        if (theValusArray.length < 7)
            return "$INVALID_PARAMETERS";     
        
        var msisdn = theValusArray[0];
        var pass = theValusArray[1];
        var assetID = theValusArray[2];
        var FECM = theValusArray[3];
        var duration = theValusArray[4];
	    var cp = theValusArray[5];
	    var ap = theValusArray[6];
	    
	    var language = "none";
	    if (theValusArray.length >= 8)
		    language = theValusArray[7];
		
		
	    TvinciPlayerAddToLog("NDSPlayReturn - executed with the following parameters :" + 
	    " msisdn '" + msisdn + 
	    "' | pass '" + pass + 
	    "' | assetID '" + assetID + 
	    "' | FECM '" +  FECM + 
	    "' | duration '" + duration + 
	    "' | cp '" + cp + 
	    "' | ap '" + ap + 
	    "' | language '" + language + 
	    "'");	    
	
		
            clearUri = "http://switch3.castup.net/cunet/gm.asp?ai=545&ar=" + assetID;
	    srtUri = "";
	    if (language == "heb" || language == "rus")
	    {
	        srtUri = clearUri;
	        if (language == "heb")
	        {
	            language = "hebrew";
	        }else if (language == "rus")
	        {
	            language = "russian";
	        }
    	    	
	        srtUri += "-" + language;
	        srtUri += "&ak=null";
	    }
	    
	    clearUri += "&ak=null";

        
        if (FECM ==	'')
        {
            TvinciPlayerAddToLog("NDSPlayReturn - No fecm found - free content assumed. return with clearUri '" + clearUri);
            return clearUri;
        }
        TvinciPlayerAddToLog("NDSPlayReturn - clearUri '" + clearUri + "' | srtUri '" + srtUri + "'");
        // If asst manager is not loaded/installed
		

         if (GetVG()._vgdk == null)
         {
            return "$LOCAL_RETRY";
         }
		 
                                         
        if (!GetVG().getIsInitialized())
        {                                                                               
            GetVG().initializeVgdk(msisdn,pass);
                        
            if(GetVG().getRequiresInstallOrUpgrade()) {
                TvinciPlayerAddToLog("NDSPlayReturn - GetVG()._vgdk is null. returning with result '$INSTALL_UPGRADE_REQUIRED'");
                return "$INSTALL_UPGRADE_REQUIRED"; 
            }            
            
            return "$LOCAL_RETRY";
        }
		
         
         
         TvinciPlayerAddToLog("NDSPlayReturn - Verifing that the user is logged");
         if (!GetVG().verifyLogin(msisdn,pass))              
         {
            TvinciPlayerAddToLog("NDSPlayReturn - Failed to verify that the user is logged - returning with result '$LOG_IN_FAILED'");
            return "$LOG_IN_FAILED"; 
         }
         TvinciPlayerAddToLog("NDSPlayReturn - User is logged");
           

//        try
//		{
//		    //TvinciPlayerAddToLog("NDSPlayReturn - tring to clear previous license.");
//			//GetVG()._vgdk.Delete(assetID);			
//			//TvinciPlayerAddToLog("NDSPlayReturn - license cleared succesfully.");
//		}
//		catch(theExp1)
//		{}   
		
		
		try
		{
		    var eee ;
		    
		    TvinciPlayerAddToLog("NDSPlayReturn - Calling '_vgdk.StreamPlayURL' with the following parameters:" + 
	        " assetID '" + assetID + 
	        "' | duration '" + duration + 
	        "' | FECM '" + FECM + 
	        "' | clearUri '" +  clearUri + 
	        "' | srtUri '" + srtUri + 	    
	        "'");	    
	        
			eee = GetVG()._vgdk.StreamPlayURL(assetID, duration, FECM, clearUri, srtUri); 
			
			if (eee == null)
            {
                TvinciPlayerAddToLog("NDSPlayReturn - '_vgdk.StreamPlayURL' returned null");
                return "";
            }
            
            TvinciPlayerAddToLog("NDSPlayReturn - '_vgdk.StreamPlayURL' returned with '" + eee + "'"); 
            return eee;
		}
		catch(theExp)
		{
		    TvinciPlayerAddToLog('NDSPlayReturn - the following error occured while calling vg object \'' + theExp.message + '\'');                      				    		    
		    return "";
		}   						                                
    }
    catch(theExp)
    {    
		alert("3 "+theExp.message)
        TvinciPlayerAddToLog('NDSPlayReturn - the following error occured \'' + theExp.message + '\'');             
        return "";
    }
}

function getFlashObject(){    
    var element = document.getElementsByName("player");
    
    if (element != 'undefined' && element != null)
    {
	    return element[0];
	}else
	{
	    return null;
	}
}

function getFlashPersistanceFromFlash(){  

	var flashObj = getFlashObject(); 
	if (flashObj != 'undefined' && flashObj != null)
	{
	    var ret = flashObj.getSessionString();		
        return ret;
    }
    else
    {
        return "";
    }
}

function getFlashPersistanceByFlash()
{
    return FlashPersistData;
}

function gotoItemPageFromFlash(mediaID)
{
    var russian =(window.location.href.toLowerCase().indexOf("language=ru") != -1);
    
    window.location = "TVMItemInformation.aspx?ID=" + mediaID + (russian ? "&Language=ru" : "");

}

function gotoPurchaseItemInFlash(assetID){
     
     setTimeout('DummyFunction(\''+assetID+'\')',1000);   
//	var flashObj = getFlashObject(); 
//	if (flashObj != 'undefined' && flashObj != null)
//	{	    
//	    flashObj.purchaseItem(assetID);
//	}
}

function DummyFunction(assetID){


	var flashObj = getFlashObject(); 
	if (flashObj != 'undefined' && flashObj != null)
	{	    
	    flashObj.purchaseItem(assetID);
	}
}




function gotoLoginFromFlash()
{    
    gotoLogin("",getFlashPersistanceFromFlash());
}





var customChannelRequest = "";
function getCustomChannelRequest()
{
    return customChannelRequest;
}

function gotoPurchasePackageInFlash(packageID,imageURL, packageName, price, duration){
	var flashObj = getFlashObject(); 
	if (flashObj != 'undefined' && flashObj != null)
	{	    
	    flashObj.purchasePackage(packageID, imageURL, packageName, price, duration); 
	}	
}

function gotoViewItem(assetID)
{
    gotoPurchaseItemInFlash(assetID);
}

function gotoLoginFromSite(action)
{    
    var siteData = (action != "") ? action : "";   
    var flashData = getFlashPersistanceFromFlash();
    
    if (flashData != 'undefined' && flashData != null && flashData != "")
    {
        gotoLogin(siteData, flashData);
    } 
    else
    {
        gotoLogin(siteData, "");    
    }
}

function gotoLogin(siteData, flashData)
{        
    var url = window.location.href;    
    var containsQuery = (url.indexOf("?") != -1);
    
    if (siteData != "")
    {
        if (containsQuery)
        {
            url = url + "&SitePersist=" + siteData;
        }else
        {
            url = url + "?SitePersist=" + siteData;
            containsQuery = true;        
        }    
    }
    
    if (flashData != "")
    {
        if (containsQuery)
        {
            url = url + "&FlashPersist=" + flashData;
        }else
        {
            url = url + "?FlashPersist=" + flashData;
            containsQuery = true;        
        }    
    }                   
            
    window.location.href = "protected/Login.aspx?CallerURL=" + url.replace(/&/g,"~!~");            
}
// -----------------------------------------------



//    window.onload = function ()
//    {
//        ScrollToAnchor("PackageContent");
//    }

    function ScrollToAnchor(AnchorID)
    {
        window.scrollTo(0, document.getElementById(AnchorID).offsetTop);
    }

	var iIntervalID = 0;
	
	function scrollUp()
	{
	    if(iIntervalID == 0)
    		iIntervalID = window.setInterval('doTheScroll()', 5);
	}
	
	function doTheScroll()
	{											   
		var scrollPos = document.documentElement.scrollTop;
		if(scrollPos > 0)
		{
			window.scrollTo(0, scrollPos-30);
		}
		else
		{
			ClearTheInterval();
		}
	}	
	
	function ClearTheInterval()
	{
		window.clearInterval(iIntervalID);
		iIntervalID = 0;
	}


