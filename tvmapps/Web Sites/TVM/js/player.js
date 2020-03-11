/* returning JSON
   status = "error, success"
   downloadStatus = "notStarted, pending, downloading,paused, downloaded"
   CanPlay = "yes","no"          
   downloadProgress = {}
   errorAction = "_,message,relogin" */    
function MediaStatus(item)
{   
    var assetID = item.asset_id;
    TvinciPlayerAddToLog("MediaStatus - entering method with asset '" + assetID + "'");
            
    try
    {
        if (NDSPreExecuteValidate(item.msisdn, item.password) == true)
        {            
            var obj = GetVG().GetMediaList(assetID, true);
            var downloadStatus = "";
            var canPlay = "no";
            
            if (obj != null)
            {        
                TvinciPlayerAddToLog("MediaStatus -  found media information assetID '" + obj.assetID + "' state '" + obj.assetState + "'");
                
                var state = obj.assetState;
                var downloadProgress = obj.downloadProgress;
                                
                if (state == 8 || (state <= 5 && obj.downloadProgress >= 5))
                {
                    canPlay = "yes";                
                }
                
                if (state == 1) 
                {
                    downloadStatus = "pending";
                }else if(state == 2)
                {
                    downloadStatus = "paused";
                }else if(state == 3 || state == 6)            
                {
                    downloadStatus = "downloading";            
                }else if(state == 8)
                {
                    downloadStatus = "downloaded";                            
                }else
                {
                    throw("asset state '" + state + "' is not supported");
                }
                 
                TvinciPlayerAddToLog("MediaStatus -  returning to flash with status 'success' downloadStatus '" + downloadStatus + "' canPlay '" + canPlay + "' downloadProgress '" + downloadProgress + "'");               
                return ({ "status" : "success", "downloadStatus" : downloadStatus, "canPlay" : canPlay, "errorAction" : "", "downloadProgress" : downloadProgress});                                             
            }
            else
            {            
                TvinciPlayerAddToLog("MediaStatus -  Cannot find media in download list. returning to flash with status 'success' downloadStatus 'notStarted' canPlay 'no' errorAction '' downloadProgress '0'");               
                return ({ "status" : "success", "downloadStatus" : "notStarted", "canPlay" : "no", "errorAction" : "", "downloadProgress" : 0});                                             
                TvinciPlayerAddToLog("MediaStatus -  . assuming that is should be downloaded");               
            }
        }        
    }catch(e)
    {
        // do default 
        TvinciPlayerAddToLog("MediaStatus -  Error occured while trying to extract media information with error '" + e.message + "'");               
    }
    
    TvinciPlayerAddToLog("MediaStatus -  returning to flash with status 'error' downloadStatus '' canPlay '' errorAction 'message' downloadProgress '0'");               
    return ({ "status" : "error", "downloadStatus" : "", "canPlay" : "", "errorAction" : "message", "downloadProgress" : 0});            
}

function NDSDownload(item)
{   
   TvinciPlayerAddToLog("NDSDownload - download requested for item '" + item.constructor + "'");	
    
    var result = "";
    
    try
    {
        var status = "error";        
        
        if (NDSPreExecuteValidate(item.msisdn, item.password) == false)
        {
            status = "error";    
        }else
        {
            try
            {
                if (GetVG().StartDownload(item))
                {
                    status = "success";                            
                }else
                {
                    status = "error";                                            
                }
                
            }catch(e)
            {            
                status = "error";    
            }                  
        }
                
        result = {"status" : status};
	    
	}catch(e)
	{
	    result = {"status" : "error"};	
	}
	
	var flashObj = getFlashObject(); 
    TvinciPlayerAddToLog("NDSDownload - returning to flash with status '" + result.status + "'");	
	flashObj.NDSDownloadCallback(result);         
}

function NDSGetFilePlay(item){    
    
    TvinciPlayerAddToLog("NDSGetFilePlay - Play requested for item '" + item.constructor + "'");	
            
    try
    {
        if (item.play_method != "2")
        {
            throw("Currently not supported play_method which is not 2");
        }
        
        var status = "error";
        var url = "";
        
        if (NDSPreExecuteValidate(item.msisdn, item.password) == false)
        {
            status = "error";    
        }else
        {
            try
            {
                url = GetVG().getPlayURL(item).replace(/\\/g,"\\\\");
                status = "success";                            
            }catch(e)
            {            
                status = "error";    
            }                  
        }
                
        result = {"status" : status, "url" : url};
	    
	}catch(e)
	{
	    result = {"status" : "error", "url" : ""};	
	}
	
	var flashObj = getFlashObject(); 
    TvinciPlayerAddToLog("NDSGetFilePlay - returning to flash with status '" + result.status + "' url '" + result.url + "'");	
	flashObj.NDSPlayCallback(result); 	                                   	
}


var eNDSStatus = {Valid : 0, Error : 2, InstallationProblem : 4};
function NDSValidateInstallationSendToFlash(ndsStatus)
{
    try
    {    
        var status = "$ERROR";       
        
        switch(ndsStatus)
        {
            case eNDSStatus.Valid:
            status = "$OK";
            break;
            case eNDSStatus.Error:
            status = "$ERROR";
            break;
            case eNDSStatus.InstallationProblem:
            status = "$INSTALLATION_PROBLEM";
            break;            
        }
                        
        var flashObj = getFlashObject(); 
                
        if (typeof flashObj != 'undefined' && flashObj != null)
        {	    
            //TvinciPlayerAddToLog("NDSValidateInstallationSendToFlash - updating flash player that nds validation status is '" + status + "'");
            flashObj.NDSValidated(status); 
        }	else
        {
            //TvinciPlayerAddToLog("NDSValidateInstallationSendToFlash - CANNOT find flash player to update that nds validation status is '" + status + "'");
        }
    }
    catch(e)
    {	                
        //TvinciPlayerAddToLog("NDSValidateInstallationSendToFlash - error occured while tring to update flash player that nds player validation status is '" + status + "' error " + e.message + "'");
    }		        
}

function changeScreenSaverStatus(allowScreen)
{    
// not used
    return;
    TvinciPlayerAddToLog("changeScreenSaverStatus - Executed with 'allowscreen' " + allowScreen);
    
    try
    {
        GetVG().changeScreenSaverStatus(allowScreen);
    }catch(e)
    {
    }
}

function NDSPreExecuteValidate(userName, password)
{         
    TvinciPlayerAddToLog("NDSPreExecuteValidate - Entering function with user '" + userName + "' password '" + password + "'");
    
    try 
    {                        
        var isIE = (navigator.appName == "Microsoft Internet Explorer");  
         
        if (isIE==false) 
        {        
            TvinciPlayerAddToLog("NDSPreExecuteValidate - Client browser must be internet explorer");
            return false;
        }
                                  
        if (!GetVG().getIsInitialized())
        {  
            TvinciPlayerAddToLog("NDSPreExecuteValidate - nds player is not initialized. performing initialization.");
            GetVG().initializeVgdk(userName,password);
        }
                    
        if(GetVG().getRequiresInstallOrUpgrade())
        {       
            TvinciPlayerAddToLog("NDSPreExecuteValidate - nds player install/upgrade required");
            return false;                         
        }   
        
        if (!GetVG().verifyLogin(userName,password))              
         {            
            TvinciPlayerAddToLog("NDSPreExecuteValidate - error occured while verifing that the user is logged.");
            return false;
         }
             
        TvinciPlayerAddToLog("NDSPreExecuteValidate - player is valid");
        return true;
    }
    catch(e)
    {    
        TvinciPlayerAddToLog("NDSPreExecuteValidate - error occured with message '" + e.message + "'");
        return false;
    }
}

function NDSPerformLogin(userName,password)
{   
    TvinciPlayerAddToLog("NDSPerformLogin - enter method with user '" + userName + "' password '" + password + "'");
    NDSUser = userName;
    NDSPassword = password;    
    if ((GetVG().getStatus() & eStatus.HasVGDK) == eStatus.HasVGDK)
    {
        GetVG().initializeVgdk(NDSUser,NDSPassword);        
    }    
}

function NDSValidateInstallation(itemID, type)
{    
    //TvinciPlayerAddToLog("NDSValidateInstallation - enter method with itemID '" + itemID + "' type '" + type + "'");
    //return NDSValidateInstallationSendToFlash(0);
    var result = eNDSStatus.Error;
    var startValidation = false;
                
    try
    {    
        var ndsStatus = GetVG().getStatus();
        
        if (((ndsStatus & eStatus.Logged) != eStatus.Logged) ||
        ((ndsStatus & eStatus.Error) == eStatus.Error))
        {            
            if (((GetVG().getStatus() & eStatus.VGDKAssigned) == eStatus.VGDKAssigned))
            {                
                //TvinciPlayerAddToLog("NDSValidateInstallation - invalid situation. user not logged but it seems that a version of nds player is installed. player should not try to play");
                //result = eNDSStatus.Error;     
                //TvinciPlayerAddToLog("NDSValidateInstallation - user not logged and a nds player seems to be installed. assuming user is tring to play free content");
                result = eNDSStatus.Valid;
            }
            else
            {
                //TvinciPlayerAddToLog("NDSValidateInstallation - User not logged,no vgdk seems to be installed. start installation");
                startValidation = true;
            }
        }else
        {
            // user logged - check if need to upgrade
            if(GetVG().getRequiresInstallOrUpgrade())
            {
                //TvinciPlayerAddToLog("NDSValidateInstallation - upgrade is required by nds player");
                startValidation = true;
            }
            else
            {
                //TvinciPlayerAddToLog("NDSValidateInstallation - nds player version is valid");
                result = eNDSStatus.Valid;
            }
        }
        
        if (startValidation)
        {
            //TvinciPlayerAddToLog("NDSValidateInstallation - nds installation is being executed. pospone response to flash.");
            if (type.match(/^(package|media)$/) == null || itemID == '')
            {
                result = eNDSStatus.Error;        
            
            }else
            {            
                if (typeof StartNDSInstallationFromPlayer  == 'function')
                {   
                    if (type == 'package')
                    {
                        itemID = "ip" + itemID;
                    }else
                    {
                        itemID = "im" + itemID;
                    }
                    
                    StartNDSInstallationFromPlayer(itemID);
                    suspendResult = true;
                }
                else
                {
                    result = eNDSStatus.Error;        
                }                        
            }
        }                                                   
    }        
    catch(e)
    {
        //TvinciPlayerAddToLog("NDSValidateInstallation - error occured while verifing nds installation '" + e.message + "'");               
        result = eNDSStatus.Error;        
        startValidation = false;
    }                                             
     
    
    if (!startValidation)    
    {
        //TvinciPlayerAddToLog("NDSValidateInstallation - return value '" + result + "'");
        NDSValidateInstallationSendToFlash(result);                
    }        
}

function getFlashObject(){
    var element = document.getElementsByName("player");
    
    if (typeof element != 'undefined' && element != null)
    {
	    return element[0];
	}else
	{
	    return null;
	}
}


function NDSPlayResultToFlash(ret)
{
    var flashObj = getFlashObject(); 
    TvinciPlayerAddToLog('NDSPlaySendToFlash - returning to flash with result \'' + ret + '\'');	
    
    if (typeof oldPlayerHandleNDSResponse == 'function')
    {
        oldPlayerHandleNDSResponse(flashObj,ret);    
    }
    else
    {
       flashObj.NDSPlayCallback(ret);
   }
}

function NDSPlay(theValues) {
    try
    {   
        var theValusArray = new Array();
        theValusArray = theValues.split(',');                                	    
        
        if (theValusArray.length < 7)
        {
            TvinciPlayerAddToLog("NDSPlay - input arguments count must be equal to 7 (input : '" + theValues + "'");
            NDSPlayResultToFlash("");
            return;
        }
        if (theValusArray[7] != "")
            NDSUser = theValusArray[7];
        //if (theValusArray[8] != "")
            //NDSPassword = theValusArray[8];
        // Create json item
        
        
	    var item = { 
	        "asset_id": theValusArray[0],
	        "fecm": theValusArray[1],
	        "duration": theValusArray[2],
	        "cp": theValusArray[3],
	        "ap": theValusArray[4],
	        "language": theValusArray[5],
	        "tid": theValusArray[6],
	        "play_method": 0,
	        "msisdn": NDSUser
	    };
	    
	    TvinciPlayerAddToLog("NDSPlay - executed with the following parameters :" + 	    
	    "' | assetID '" + item.asset_id + 
	    "' | FECM '" +  item.fecm + 
	    "' | duration '" + item.duration + 
	    "' | cp '" + item.cp + 
	    "' | ap '" + item.ap +
	    "' | language '" + item.language +
	    "' | tid '" + item.tid + 
	    "'");	    
	    	          
        var clearUri = '';
        if (item.fecm == '')
        {
            // Programmer NOTICE!!! calling private method to bypass validation at the moment.
            // users which are not logged can watch free content
            clearUri = GetVG().getClearURI(item);
            
            TvinciPlayerAddToLog("NDSPlay - No fecm found - free content assumed. return with clearUri '" + clearUri + "'");
            NDSPlayResultToFlash(clearUri);
            return;
        }        
        
        // backward competability (ask guy if need to know why)       
        if (item.fecm == ' ')
        {
            item.fecm = '';
        }
        
        // checking if user is logged
        try
        {
            var ndsStatus = GetVG().getStatus();
            
            if (((ndsStatus & eStatus.Logged) != eStatus.Logged) ||
            ((ndsStatus & eStatus.Error) == eStatus.Error))
            {
                TvinciPlayerAddToLog("NDSPlay - user verification failed (user not logged or error occured while tring to login");
                NDSPlayResultToFlash("");
                return;
            }
        }
        catch(e)
        {
            TvinciPlayerAddToLog("NDSPlay - error occured while verifing that user is logged '" + e.message + "'");
            NDSPlayResultToFlash("");
            return;
        }
                           
        try
        {                                        
            // Create clear uri
	        clearUri = GetVG().GetClearURI(item);	        
        }
        catch(e)
        {
            TvinciPlayerAddToLog("NDSPlay - error occured while tring to extract clear uri '" + e.message + "'");
            NDSPlayResultToFlash("");
            return;
        }
        
        if (clearUri == "")
        {
            TvinciPlayerAddToLog("NDSPlay - Failed to create clear uri.");            
            NDSPlayResultToFlash("");
            return;
        }

        // Create subtitles uri	    	    
        if (item.language == "heb") 
        {
            item.language = "hebrew";
        } 
        else if (item.language == "rus")
        {
            item.language = "russian";
        }else
        {
            item.language = "";
        }	        	        
	    
	    var srtUri = "";
	    if (item.language != "")
	    {
	        srtUri = GetVG().GetSubtitleURI(item);	    
	        
	        if (srtUri == "")
            {
                TvinciPlayerAddToLog("NDSPlay - Creating subtitles uri returned empty string. continue without subtitle");
            }
        }
	     
	    
                  
        TvinciPlayerAddToLog("NDSPlay - clearUri '" + clearUri + "' | srtUri '" + srtUri + "'");
                                                                                                          				
		try
		{		    		    
		    TvinciPlayerAddToLog("NDSPlay - Calling '_vgdk.StreamPlayURL'");
	        
			var playResult = GetVG()._vgdk.StreamPlayURL(item.asset_id, item.duration, item.fecm, clearUri, srtUri); 
			
			if (playResult == null || playResult == '')
            {
                TvinciPlayerAddToLog("NDSPlay - '_vgdk.StreamPlayURL' returned null");
                NDSPlayResultToFlash("");
                return;
            }

            TvinciPlayerAddToLog("NDSPlay - '_vgdk.StreamPlayURL' returned with '" + playResult + "'"); 
            NDSPlayResultToFlash(playResult);
            return;            
		}
		catch(e)
		{
		    TvinciPlayerAddToLog('NDSPlay - the following error occured while calling vg object \'' + e.message + '\'');                      				    		    
		    NDSPlayResultToFlash("");
            return;
		}   						                                
    }
    catch(e)
    {    
        TvinciPlayerAddToLog('NDSPlay - the following error occured \'' + e.message + '\'');             
        NDSPlayResultToFlash("");
        return;
    }
}










function getFlashPersistanceByFlash()
{
    return FlashPersistData;
}

function gotoNDSInstallationFromMedia(mediaID)
{           
    window.location = "ndsinstallation.aspx?" + base64encode("RequestedMediaID=" + mediaID + "&" + GetLanguageQuery());
}

function gotoNDSInstallationFromPackage(packageID)
{           
    window.location = "ndsinstallation.aspx?" + base64encode("RequestedPackageID=" + packageID + "&" + GetLanguageQuery());
}

// obslete - don't use !
function gotoItemPageFromFlash(mediaID)
{           
    window.location = "TVMItemInformation.aspx?" + base64encode("ID=" + mediaID + "&" + GetLanguageQuery());
}






function gotoLoginFromFlash()
{   
    var flashPers = ''; 
    if (typeof getFlashPersistanceFromFlash == 'function')
    {
        flashPers = getFlashPersistanceFromFlash();
    }
    
    gotoLogin("",flashPers);
}





var customChannelRequest = "";
function getCustomChannelRequest()
{
    return customChannelRequest;
}

function gotoPageFromFlash(item)
{
    try
    {
        // calculate language
        var lang = GetLanguageQuery();
        
        if (item.destination.toLowerCase() == "ndsdownloadmanager")
        {            
            window.location = "my.aspx?" + GetLanguageQuery();
        } 
    }
    catch(e)
    {
        // do nothing
    }
}

function gotoPurchasePackageInFlash(packageID){
	var flashObj = getFlashObject(); 
	if (typeof flashObj != 'undefined' && flashObj != null)
	{	    	
	    flashObj.callFlashAction({action:"purchase_package", packageID:packageID})	    
	}	
}

function gotoViewItem(assetID)
{
    gotoPurchaseItemInFlash(assetID);
}

function gotoLoginFromSite(action)
{    

    var siteData = (action != "") ? action : "";   
    
    var flashData = ''; 
    if (typeof getFlashPersistanceFromFlash == 'function')
    {
        flashData = getFlashPersistanceFromFlash();
    }
            
    if (typeof flashData != 'undefined' && flashData != null && flashData != "")
    {
        gotoLogin(siteData, flashData);
    } 
    else
    {
        gotoLogin(siteData, "");    
    }
}

function gotoPage(pageURL, queryString, flashData, newWindow)
{

    if (typeof newWindow == "undefined")
    {
        newWindow = false;
    }
    
    if (typeof flashData == "undefined")
    {    
        if (typeof getFlashPersistanceFromFlash == 'function')
        {
            flashData = getFlashPersistanceFromFlash();
        }        
    }
    
    var url = window.location.href;    
                
    var query;
    var link;
    
    var qLocation = url.indexOf("?",0);    
    if (qLocation != -1)
    {       
        link = url.substr(0,qLocation);   
              
        if (url.length == qLocation +1)
        {
            query = "";
        }
        else
        {
            query = base64decode(url.substr(qLocation+1,url.length-qLocation-1));    
        }                    
    }
    else
    {
       link = url;
       query = "";
    }
                        
    if (flashData != "")
    {
        if (query != "" )
        {
            query += "&";
        }
        
        query += "FlashPersist=" + flashData;
    }        
        
    if (query != "")
    {
        link += "?" + query;
    }
    
    var actualQuery = "CallerURL=" + escape(link);
    
    if (queryString != "")
    {
        if (actualQuery != "" )
        {
            actualQuery += "&";
            
        }
        actualQuery += queryString;
    }
                                                                  
    var result = pageURL + "?" + escape(base64encode(actualQuery));                
             
    if (newWindow)
    {
        window.open(result);
    }else
    { 
        window.location.href = result;        
    }
}

function gotoLogin(siteData, flashData)
{             
    return gotoPage(loginPageURL,'',flashData)    
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















////////////////////////////////////////////////////////////////////////////////

var m_NDSPlayTimer;
var m_NDSValidationTimer;
var m_NDSResponseUIDCounter = 0;
var m_NDSValidationUIDCounter = 0;
var m_MaxLoadCalls = 15;
var m_MaxUpgradeCalls = 90;


















//function NDSPlay(theValues) {
//    TvinciPlayerAddToLog("NDSPlay - Start play request {msisdn,pass,assetID,FECM,duration,cp,ap, language, transactionID},{" + theValues + "}");
//	m_NDSResponseUIDCounter ++;
//	clearTimeout(m_NDSPlayTimer);
//	m_NDSPlayTimer = setTimeout("NDSPlaySendToFlash('"+theValues+"',"+m_NDSResponseUIDCounter+" , 0 , '')", 40);	
//}

//function NDSPlaySendToFlash(theValues, uid , nCounter , last_status){
//	nCounter++;
//	
//	if (nCounter == 1)
//	{
//	    TvinciPlayerAddToLog("NDSPlaySendToFlash - Executed for uid '" + uid + "'");
//	
//	}else
//	{
//	    TvinciPlayerAddToLog("NDSPlaySendToFlash - Executed recursive for uid '" + uid + "'. previous status '" + last_status + "'. attempt number '" + nCounter + "'");	    
//	}
//		
//	if (last_status == "$LOCAL_RETRY" && nCounter == m_MaxLoadCalls)
//	{
//	    TvinciPlayerAddToLog('NDSPlaySendToFlash - maximum attempts reached. return to flash response \'$LOAD_TIMEOUT\'');
//		getFlashObject().getHandleNDSResponse("$LOAD_TIMEOUT");
//		return;
//	}
//	
//	if (last_status == "$INSTALL_UPGRADE_REQUIRED" )//&& nCounter == m_MaxUpgradeCalls
//	{
//	    TvinciPlayerAddToLog('NDSPlaySendToFlash - maximum attempts reached. return to flash response \'$UPGRADE_TIMEOUT\'');
//		getFlashObject().getHandleNDSResponse("$UPGRADE_TIMEOUT");
//		return;
//	}

//	var ret = NDSPlayReturn(theValues);
//	switch(ret){
//		case "$LOCAL_RETRY":
//		    TvinciPlayerAddToLog("NDSPlaySendToFlash - Retring to execute method. Attempt number '" + nCounter + "' (max attempts allowed'" + m_MaxLoadCalls + "')");
//			clearTimeout(m_NDSPlayTimer);
//			m_NDSPlayTimer = setTimeout("NDSPlaySendToFlash('"+theValues+"',"+uid+","+ nCounter +" , '$LOCAL_RETRY')", 1000);			
//			break;
////		case "$INSTALL_UPGRADE_REQUIRED":
////		    TvinciPlayerAddToLog("NDSPlaySendToFlash - Retring to execute method. Attempt number '" + nCounter + "' (max attempts allowed'" + m_MaxUpgradeCalls + "')");
////			clearTimeout(m_NDSPlayTimer);
////			m_NDSPlayTimer = setTimeout("NDSPlaySendToFlash('"+theValues+"',"+uid+","+ nCounter +" , '$INSTALL_UPGRADE_REQUIRED')", 1000);			
////			break;
//	}
//	if(uid != m_NDSResponseUIDCounter)
//		return;
//	var flashObj = getFlashObject(); 
//    TvinciPlayerAddToLog('NDSPlaySendToFlash - returning to flash with result \'' + ret + '\'');	
//    
//    if (typeof oldPlayerHandleNDSResponse == 'function')
//    {
//        oldPlayerHandleNDSResponse(flashObj,ret);    
//    }
//    else
//    {
//       flashObj.NDSPlayCallback(ret);
//    }	
//}

//function NDSPlayReturn(theValues)
//{        
//    try
//    {   	    
//        var theValusArray = new Array();
//        theValusArray = theValues.split(',');
//        if (theValusArray.length < 7)
//            return "$INVALID_PARAMETERS";     
//        
//        var msisdn = theValusArray[0];
//        var pass = theValusArray[1];
//        var assetID = theValusArray[2];
//        var FECM = theValusArray[3];
//        var duration = theValusArray[4];
//	    var cp = theValusArray[5];
//	    var ap = theValusArray[6];
//	    var language = theValusArray[7];
//	    var tid = theValusArray[8];
//	    
//	    TvinciPlayerAddToLog("NDSPlayReturn - executed with the following parameters :" + 
//	    " msisdn '" + msisdn + 
//	    "' | pass '" + pass + 
//	    "' | assetID '" + assetID + 
//	    "' | FECM '" +  FECM + 
//	    "' | duration '" + duration + 
//	    "' | cp '" + cp + 
//	    "' | ap '" + ap +
//	    "' | language '" + language +
//	    "' | tid '" + tid + 
//	    "'");	    
//	    
//	    // Create json item
//	    var item = { "msisdn": msisdn,
//	        "password": pass,
//	        "asset_id": assetID,
//	        "fecm": FECM,
//	        "duration": duration,
//	        "cp": cp,
//	        "ap": ap,
//	        "language": language,
//	        "tid": tid,
//	        "play_method": 0
//	    };
//        
//        var clearUri = '';
//        if (FECM ==	'')
//        {
//            // Programmer NOTICE!!! calling private method to bypass validation at the moment.
//            // currently users which are not logged cannot watch free content
//            clearUri = GetVG().getClearURI(item);
//            TvinciPlayerAddToLog("NDSPlayReturn - No fecm found - free content assumed. return with clearUri '" + clearUri);
//            return clearUri;
//        }        

//                        
//        // Create clear uri
//	    clearUri = GetVG().GetClearURI(item);
//	    if (clearUri == "")
//        {
//            TvinciPlayerAddToLog("NDSPlayReturn - Failed to create clear uri. returning with empty url");
//            return "";
//        }

//        // Create subtitles uri	    
//	    if (language == "heb" || language == "rus") 
//	    {
//	        if (language == "heb") 
//	        {
//	            item.language = "hebrew";
//	        } 
//	        else if (language == "rus")
//	        {
//	            item.language = "russian";
//	        }	        	        
//	    }
//	    
//	    var srtUri = GetVG().GetSubtitleURI(item);	    
//	    if (srtUri == "")
//        {
//            TvinciPlayerAddToLog("NDSPlayReturn - Creating subtitles uri returned empty string. continue without subtitle");
//        }
//	            
//        if (FECM == ' ')
//        {
//            FECM = '';
//        }

//        if (GetVG()._vgdk == null)
//         {
//            return "$LOCAL_RETRY";
//         }
//             
//             
//        TvinciPlayerAddToLog("NDSPlayReturn - clearUri '" + clearUri + "' | srtUri '" + srtUri + "'");
//        // If asst manager is not loaded/installed

//     
//                                         
//        if (!GetVG().getIsInitialized())
//        {                                                                               
//            GetVG().initializeVgdk(msisdn,pass);
//                        
//            if(GetVG().getRequiresInstallOrUpgrade()) {
//                TvinciPlayerAddToLog("NDSPlayReturn - GetVG()._vgdk is null. returning with result '$INSTALL_UPGRADE_REQUIRED'");
//                return "$INSTALL_UPGRADE_REQUIRED"; 
//            }            
//            
//            return "$LOCAL_RETRY";
//        }
//         
//         
//         TvinciPlayerAddToLog("NDSPlayReturn - Verifing that the user is logged");
//         if (!GetVG().verifyLogin(msisdn,pass))              
//         {
//            TvinciPlayerAddToLog("NDSPlayReturn - Failed to verify that the user is logged - returning with result '$LOG_IN_FAILED'");
//            return "$LOG_IN_FAILED"; 
//         }
//         
//         TvinciPlayerAddToLog("NDSPlayReturn - User is logged");
//                         
////        try
////		{
////		    //TvinciPlayerAddToLog("NDSPlayReturn - tring to clear previous license.");
////			//GetVG()._vgdk.Delete(assetID);			
////			//TvinciPlayerAddToLog("NDSPlayReturn - license cleared succesfully.");
////		}
////		catch(theExp1)
////		{}   
//						
//		try
//		{
//		    var eee ;
//		    
//		    TvinciPlayerAddToLog("NDSPlayReturn - Calling '_vgdk.StreamPlayURL' with the following parameters:" + 
//	        " assetID '" + assetID + 
//	        "' | duration '" + duration + 
//	        "' | FECM '" + FECM + 
//	        "' | clearUri '" +  clearUri + 
//	        "' | srtUri '" + srtUri + 	    
//	        "'");	    
//	        
//			eee = GetVG()._vgdk.StreamPlayURL(assetID, duration, FECM, clearUri, srtUri); 
//			
//			if (eee == null || eee == '')
//            {
//                TvinciPlayerAddToLog("NDSPlayReturn - '_vgdk.StreamPlayURL' returned null");
//                return "";
//            }
//            
//            TvinciPlayerAddToLog("NDSPlayReturn - '_vgdk.StreamPlayURL' returned with '" + eee + "'"); 
//            return eee;
//		}
//		catch(theExp)
//		{
//		    TvinciPlayerAddToLog('NDSPlayReturn - the following error occured while calling vg object \'' + theExp.message + '\'');                      				    		    
//		    return "";
//		}   						                                
//    }
//    catch(theExp)
//    {    
//        TvinciPlayerAddToLog('NDSPlayReturn - the following error occured \'' + theExp.message + '\'');             
//        return "";
//    }
//}






