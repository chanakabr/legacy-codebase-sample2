
var eStatus = {None : 0, Initialized : 2, Logged : 4, ValidVersion : 8, Error : 16, RequireUpgrade : 32, VGDKAssigned : 64, HasVGDK : 128};

// ************************* START OF Constructor *************************
function VGObject() {

    // fields
    this._vgdk = null;    
    this._vgdkName = "";    
    this._version= "?";    
    this._status = eStatus.None;
    
    this.notifyListenerList = new Array();
    
    this.AddNotifyEventHandler = function(func)
    {        
        this.notifyListenerList.push(func);    
    }
    
    this.RaiseNotifyEvent = function(item)
    {
        for(var i in this.notifyListenerList)
        {
            try
            {
                this.notifyListenerList[i](item);        
            }
            catch(e)
            {
            
            }
        }    
    }
                
    // private methods
    this.login = loginMethod;
    this.logout = logoutMethod;        
        
    // public methods    
    this.initializeVgdk = initializeVgdkMethod;            
    this.verifyLogin = verifyLoginMethod;
    this.replaceVGDK = replaceVGDKMethod;    
    this.changeScreenSaverStatus = changeScreenSaverStatusMethod;        
    
    
    // public properties
    this.getStatus = getStatusMethod;    
    this.getVersion = getVersionMethod;
    this.getRequiresInstallOrUpgrade = getRequiresInstallOrUpgradeMethod;	
    this.getIsInitialized = getIsInitializedMethod;            
                
    this.eDownloadAction = { PAUSE : 'PAUSE', CANCEL : 'CANCEL', RESUME : 'RESUME', DELETE : 'DELETE'};
    
    this.PerformDownloadAction = function(downloadAction, assetID)
    {          
        try
        {
            TvinciPlayerAddToLog("PerformDownloadAction - entering method with action '" + downloadAction + "' asset '" + assetID + "'");                                            
            this.validatePreExecute("PerformDownloadAction",null);
            
            var result;
            switch (downloadAction)
            {
                case this.eDownloadAction.PAUSE:
                    result = this._vgdk.PauseDownload(assetID);
                    break;
                case this.eDownloadAction.CANCEL:
                    result = this._vgdk.CancelDownload(assetID);
                    break;
                case this.eDownloadAction.RESUME:
                    result = this._vgdk.ResumeDownload(assetID);
                    break;
                case this.eDownloadAction.DELETE:
                    result = this._vgdk.Delete(assetID);
                    break;
                default:
                    throw("unknown action '" + downloadAction + "'");
            }
            
            TvinciPlayerAddToLog("PerformDownloadAction - executing action against _vgdk returned  '" + result +"'");                                            
            return result == 0;
        }
        catch(e)
        {
            TvinciPlayerAddToLog("PerformDownloadAction - error occured while calling performing action against _vgdk with error message '" + e.message +"'");                                            
            return false;
        }
    
    }
    
    this.StartDownload = function(item)
    {                         
        try
        {
            TvinciPlayerAddToLog("StartDownload - entering method with asset '" + item.asset_id + "' fecm '" + item.fecm + "'");                                            
            
            this.validatePreExecute("StartDownload",item);

            var ndsItem = this.tryGetMediaList(item.asset_id, false);
                        
            if (ndsItem == null)
            {
                var result = this._vgdk.Download(item.asset_id, "", item.fecm, this.getClearURI(item));                                                
                TvinciPlayerAddToLog("StartDownload - calling '_vgdk.Download' with clearURI '" + this.getClearURI(item) + "' returned with '" + result +"'");                                        
                return (result == 0);                    
            }
            else if (ndsItem.assetState == 2)
            {
                var resumeResult = this._vgdk.ResumeDownload(item.asset_id);
                top.TvinciPlayerAddToLog("StartDownload - item is in download pause mode. executing '_vgdk.ResumeDownload' returned with '" + resumeResult + "'");                                        
                return (resumeResult == 0);
                
            }
            else
            {
                top.TvinciPlayerAddToLog("StartDownload - item is already in download queue. return true");                                        
                return true;            
            }
            
        }
        catch(e)
        {
            TvinciPlayerAddToLog("StartDownload - error occured while calling '_vgdk.Download' with clearURI '" + this.getClearURI(item) + "' error '" + e.message +"'");                                            
            return false;
        }
    }
    
    
    this.ExportToUSB = function(assetID, path)
    {        
        try
        {
            TvinciPlayerAddToLog("ExportToUSB - entering function with assetID '" + assetID + "' path '" + path + "'");                                                                    
            this.validatePreExecute("ExportToUSB",null);
                                                            
            var result = GetVG()._vgdk.Export(assetID,path);                        
            return (result == 0);
        }catch(e)
        {
            TvinciPlayerAddToLog("ExportToUSB - error occured while calling '_vgdk.Export' with message '" + e.message + "'");                                            
            return false;
        }    
    }
    
                                       
    this.GetMediaList = function(assetID, shouldExtractDonloadProgress)
    {   
        try
        {
            this.validatePreExecute("GetMediaList",null);
            
            return this.tryGetMediaList(assetID,shouldExtractDonloadProgress)
        }
        catch(e)
        {
            return null;
        }
    }
    
    
    function tryGetMediaList(assetID, shouldExtractDonloadProgress)
    {           
        shouldExtractDonloadProgress = (typeof shouldExtractDonloadProgress != "undefined" && shouldExtractDonloadProgress == true);
        
	    try	    
	    {	    	            	            	        	        
	        var content = this._vgdk.LocalCatalogXML;
	        var result = this.convertAssetListToJson(content, shouldExtractDonloadProgress);	        	    
	        
	        if (typeof assetID != 'undefined' && assetID != null)
	        {    	        	            
	            for(var itemKey in result)
	            {
	                var item = result[itemKey];
	                
	                if (item.assetID == assetID)
	                {
	                    TvinciPlayerAddToLog("tryGetMediaList - returning information about asset '" + item.assetID + "' assetState '" + item.assetState + "' errorId '" + item.errorId + "' downloadProgress '" + item.downloadProgress+ "'");
	                    return item;
	                }	            
	            }	            	            
	            
	            TvinciPlayerAddToLog("tryGetMediaList - item with asset '" + assetID + "' not found. return null.");    	        
	            return null;
	        }
	        	        	        
	        if (result.length == 0)
            {
                TvinciPlayerAddToLog("tryGetMediaList - no items found. return null.");    	        
                return null;
            }	            
            else
            {
                TvinciPlayerAddToLog("tryGetMediaList - returning information about all assets");    	        
                //TvinciPlayerAddToLog("tryGetMediaList - returning information about all assets ('" + escape(content) + "')");    	        
	            return result;       	        	        	        	        
	        }
	    }catch(e)
	    {	        
	        return null;
	    }                
    };
    this.tryGetMediaList = tryGetMediaList;
    
        
    this.getPlayURL = function(item)
    {                	                       
        var result = "";        
        this.validatePreExecute("getPlayURL",item);
                    	
    	try
    	{
    	    if (item.play_method == "0")
            {
                if (item.fecm = '')
                {
                    var clearURI = this.getClearURI(item);
                    TvinciPlayerAddToLog("getPlayURL - No item.fecm found - free content assumed. return with clearUri '" + clearURI + "'");
                    result = clearURI;
                }else if (item.fecm == ' ')
                {
                    item.fecm = '';                
                    result = this._vgdk.StreamPlayURL(item.asset_id, item.duration, item.fecm, this.getClearURI(item), this.getSubtitleURI(item));   
                }                                    
            }else if (item.play_method == "2")
            {            
                result = this._vgdk.FilePlayURL(item.asset_id, item.duration, this.getSubtitleURI(item));            
            }
            else
            {                
                result = "";
            }   
        }
        catch(e)
        {
            TvinciPlayerAddToLog("getPlayURL - error occured while tring to create play url. error '" + e.message + "'");
            result = "";            
        }
        
        TvinciPlayerAddToLog("getPlayURL - returning with play url '" + result + "'");            
        return result;                
    }

    this.GetSubtitleURI = function(item) 
    {
        try 
        {            
            this.validatePreExecute("GetSubtitleURI", item);

            return this.getSubtitleURI(item);
        }
        catch (e) 
        {
            top.TvinciPlayerAddToLog("GetSubtitleURI - error occured while creating subtitles URI. returning with empty string.");
            return "";
        }
    }
        
    this.getSubtitleURI = function(item)
    {    
        if (item.language != "" && item.language.toLowerCase() != "none")
        {
            return top.CreateSubtitlesUri(item);
        }	   
        else
        {
            return "";
        }        
    }

    this.GetClearURI = function(item) 
    {
        try 
        {            
            this.validatePreExecute("GetClearURI", item);

            return this.getClearURI(item);
        }
        catch (e) 
        {
            top.TvinciPlayerAddToLog("GetClearURI - error occured while creating clear URI. returning with empty string.");
            return "";
        }
    }

    this.getClearURI = function(item)
    {
        return CreateClearUri(item);
    }

    this.validatePreExecute = function(methodName, item)
    {   
        if (typeof item != "undefined" && item != null)
        {
            TvinciPlayerAddToLog(methodName + " - executed with the following parameters :" +                 
                "' | item.asset_id '" + item.asset_id + 
                "' | item.fecm '" +  item.fecm + 
                "' | item.duration '" + item.duration + 
                "' | item.cp '" + item.cp + 
                "' | item.ap '" + item.ap +
                "' | item.language '" + item.language +
                "' | item.tid '" + item.tid + 
                "' | item.play_method '" + item.play_method +                 
                "'");	 
        }else
        {
            TvinciPlayerAddToLog(methodName + " - entering method.");
        }
            
        if ((this._status & eStatus.Initialized) != eStatus.Initialized)
        {
            TvinciPlayerAddToLog(methodName + " - You must perform initialization before executing the method");
            throw(methodName + " - You must perform initialization before executing the method");
        }     
        
        if (this._vgdk == null)
	    {	    
	        TvinciPlayerAddToLog(methodName + " - cannot find actual nds player");
            throw(methodName + " - cannot find actual nds player");	        
	    }    
    }
    
    this.removeVGDK = function()
    {
        TvinciPlayerAddToLog("removeVGDK - removing current vgdk if assigned");
        
        if (this._vgdk != null)
        {
            TvinciPlayerAddToLog("removeVGDK - found previous object. executing delete on object");
            delete this._vgdk;        
        }
        
        this._vgdk = null;
        this._vgdkName = "";
    }


    this.GetDownloadProgress = function(assetID)
    {
        var result = -1;
        try
        {
            this.validatePreExecute("GetDownloadProgress",null);
            
            result = this._vgdk.AssetProgress(AssetID);                
        }
        catch(e)
        {
            result = -1;
        }        
        
        TvinciPlayerAddToLog("GetDownloadProgress - calling '_vgdk.AssetProgress' for asset '" + assetID + "' returned with '" + result + "'");
        return result;
    }    
    
    this.convertAssetListToJson = function(assetsStr, shouldExtractDonloadProgress)
    {    
        var result = new Array();
    
        try
        {
            var AssetsXml = new ActiveXObject("MSXML2.DOMDocument"); 
                AssetsXml.loadXML(assetsStr);
        
            var AssetsCollection = AssetsXml.selectNodes("*/asset");
        
            for (var i=0; i<AssetsCollection.length; i++)
            {
                var AssetID = AssetsCollection[i].getAttribute('assetId');
                var StartTime = AssetsCollection[i].getAttribute('startTime');
                var AssetState = parseInt(AssetsCollection[i].getAttribute('assetState'), 10);
                var ErrorId = AssetsCollection[i].getAttribute('errorId');
            
                var downloadProgress = -1;
                if (shouldExtractDonloadProgress)
                {         
                    try
                    {
                        downloadProgress = this._vgdk.AssetProgress(AssetID);                
                    }
                    catch(e)
                    {
                        downloadProgress = -1;                
                    }
                }
                            
                result.push({ "assetID" : AssetID, "startTime" : StartTime, "assetState" : AssetState, "errorId" : ErrorId, "downloadProgress" : downloadProgress });
            }
        }
        catch(err)
        {
            //Handle errors
            TvinciPlayerAddToLog("convertAssetListToJson - Failed to convert downloads list to json with error '" + err.message + "'");
        }               
        
        return result;
    }


}             
// ************************* END OF Constructor *************************


// ************************* START OF Public Methods *************************
 

function replaceVGDKMethod(vgdk, name)
{
    TvinciPlayerAddToLog("replaceVGDK - entering method with vgdk '" + vgdk + "' name '" + name + "'");        

    this.removeVGDK();                
    
    if (typeof(vgdk) == 'undefined' || vgdk == null)
    {
        name = "";
        vgdk = null;
    }else if (name == "")
    {
        throw("When vgdk is assigned. name is required");
    }
            
    if (vgdk == null)
    {
        this._status &= ~eStatus.HasVGDK;    
    }else
    {
        this._status |= eStatus.VGDKAssigned;    
        this._status |= eStatus.HasVGDK;    
    }
    
    this._vgdk = vgdk;
    this._vgdkName = name;    
            
    this.RaiseNotifyEvent({"type" : "VGDKReplaced", "name" : name});            
}

function changeScreenSaverStatusMethod(allowScreen)
{              
    if (((this._status & eStatus.Initialized) != eStatus.Initialized))
    {
        TvinciPlayerAddToLog("changeScreenSaverStatus - You must perform initialization before executing the method");
        return;
    }     
    
    if (this._vgdk != null)
	{	    
	    TvinciPlayerAddToLog("changeScreenSaverStatus - access method with 'allowScreen' = " + allowScreen);
    	
    	try
    	{
    	    this._vgdk.AllowScreenSaver(allowScreen);        
    	}
    	catch(e)
    	{
    	    TvinciPlayerAddToLog("changeScreenSaverStatus - failed to change screen saver status");    	    
    	}
	}		
}

  
function verifyLoginMethod(userName, password, checkInitialization)
{
    TvinciPlayerAddToLog("verifyLogin - for username '" + userName + "' password '" + password + "'");
    
    if (typeof checkInitialization != 'boolean')
    {
        checkInitialization = true;
    }
        
    if (checkInitialization && ((this._status & eStatus.Initialized) != eStatus.Initialized))
    {
        throw("verifyLogin - You must perform initialization before executing the method");
    }     
    
    if (this._vgdk == null)
	{	    
        TvinciPlayerAddToLog("verifyLogin - 'this._vgdk' is null. return false.");	
	    return false;	    
	}
            
    if (userName == "" || password == "")    
    {                    
        throw ("verifyLogin - Username and password must be set");
    }
                    
    try
    {                
        TvinciPlayerAddToLog("verifyLogin - executing 'IsUserLoggedIn' method");

        if (this._vgdk.IsUserLoggedIn(userName))
        {
            TvinciPlayerAddToLog("verifyLogin - executing 'IsUserLoggedIn' returned with 'true'");            
	        return true;
        }
        else
        {
            TvinciPlayerAddToLog("verifyLogin - the user is not logged. performing logging");
            this.logout(checkInitialization);                        
            return this.login(userName,password,checkInitialization);	        
        }
    }catch(e)
    {
        TvinciPlayerAddToLog("verifyLogin - executing 'IsUserLoggedIn' method failed! performing manual logout & login");
        this.logout(checkInitialization);            
        return this.login(userName,password,checkInitialization);	    
    }               				                    
}

function initializeVgdkMethod(userName, password) {                
            
    TvinciPlayerAddToLog("initializeVgdk - entering method with username '" + userName + "' pass '" + password + "'");
        
    try
    {                                
        this._status = this._status & ~eStatus.Initialized;
                            
        if (this._vgdkIdentifier == "")
        {
            throw("initializeVgdk - Cannot perform initialization when no vgdk object was assigned");            
        }
                                                
        if (this._vgdk == null)
        {                        
            TvinciPlayerAddToLog("initializeVgdk - cannot find activex. assuming not installed.");                                                                        
            this._status |= eStatus.RequireUpgrade;
        }else
        {                                              
            if (userName == "" || password == "")    
            {
                TvinciPlayerAddToLog("initializeVgdk - Cannot verify if user is logged. missing username or password");                                                            
            }else
            {
                if (this.verifyLogin(userName,password,false))
                {
                    this._status |= eStatus.Logged;

                    try
                    {
                        var resUpgrade = this._vgdk.IsSoftwareUpgradeRequired();                    
                        TvinciPlayerAddToLog("initializeVgdk - calling 'IsSoftwareUpgradeRequired' returned with result '" + resUpgrade + "'");
                        
                        if (resUpgrade != 0) 
                        {   
                            this._status |= eStatus.RequireUpgrade;
                        }               
                        else
                        {
                            this._status |= eStatus.ValidVersion;
                        }                                             
                    }catch(e)
                    {
                        TvinciPlayerAddToLog("initializeVgdk - failed to check if need to upgrade using vgdk with identifier '" + this._vgdkIdentifier + "'. error message - " + e.message);                                        
                    }                                        
                }                      
            }    
        }    
    }
    catch(e)
    {
        this._status |= eStatus.Error;                    
        TvinciPlayerAddToLog("initializeVgdk - Error occured while tring to verify if user is logged. error message '" + e.message + "'");                
        this.replaceVGDKMethod(null,"");
    }                                  
       
    this._status |= eStatus.Initialized;
    this.RaiseNotifyEvent({ "type": "initialized", "activeVgdk": this._vgdkName, "status" : this._status });                                              				    
}


        
    
    
    
        

// ************************* END OF Public Methods *************************

// ************************* START OF Public Properties *************************

function getRequiresInstallOrUpgradeMethod() {
    return ((this._status & eStatus.Initialized) == eStatus.Initialized) &&                
           ((this._status & eStatus.RequireUpgrade) == eStatus.RequireUpgrade);
}

function getIsInitializedMethod() {
    return ((this._status & eStatus.Initialized) == eStatus.Initialized);
}


function getStatusMethod()
{
    TvinciPlayerAddToLog("getStatusMethod - returning with value '" + this._status + "'");
    return this._status;
}


function getVersionMethod() {
    return this._version;
}
// ************************* END OF Public Properties *************************

// ************************* START OF Private methods *************************





function loginMethod(user,pswd, checkInitialization) {		  		  
	if (typeof checkInitialization != 'boolean')
    {
        checkInitialization = true;
    }
      	
    if (checkInitialization && (this._status & eStatus.Initialized) != eStatus.Initialized)
    {
        throw("You must perform initialization before executing the method");
    }        
	
	if (this._vgdk == null)
	{	    
	    return false;
	}
	
    try 
    {        
        TvinciPlayerAddToLog("login - calling 'this._vgdk.Login' method with user '" + user + "' password '" + pswd + "'");    	
    	var res = this._vgdk.Login(user,pswd);    
    	TvinciPlayerAddToLog("login - calling 'this._vgdk.Login' method returned with result '" + res + "'");    	
    	return (res == 0);
    } catch(e) {
        TvinciPlayerAddToLog("login - calling 'this._vgdk.Login' throw exception");    	
        return false;
    }   			
}

function logoutMethod(checkInitialization) {	
		
	if (typeof checkInitialization != 'boolean')
    {
        checkInitialization = true;
    }
    
	if (checkInitialization && (this._status & eStatus.Initialized) != eStatus.Initialized)
    {
        throw("logout - You must perform initialization before executing the method");
    } 	    	
    	
    if (this._vgdk == null)
	{	    
	    return false;
	}
	
    try {
        return (this._vgdk.Logout() == 0);
    } catch(e) {
        TvinciPlayerAddToLog("logout - calling 'this._vgdk.Login' throw exception");    	
		return false;
    }
}
	
// ************************* END OF Private Methods *************************

//function TvinciPlayerAddToLog(infoMessage) {
//    if(typeof top.TvinciPlayerAddToLog == 'function')     
//    { 
//        if (top.TvinciPlayerAddToLog != this.TvinciPlayerAddToLog)
//        {
//            top.TvinciPlayerAddToLog(infoMessage);                   
//        }
//    }
//}


