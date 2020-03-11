

/************************************************************************************/
/*******************	Windows Media Player		*********************************/
/************************************************************************************/

function isMediaPlayerInstalled(){
	try{
	    if (window.ActiveXObject)
        {
            control = new ActiveXObject("WMPlayer.OCX.7");
        }
        else if (window.GeckoActiveXObject)
        {
        control = new GeckoActiveXObject("WMPlayer.OCX.7");
        }
    if(control)
       return true
    else
        return  false  
	}
	catch(e)
	{
        return false;
    }
}

function getMediaPlayerVersion(){
	var player_version = "6.4";
	var WM
	try{
	    if (window.ActiveXObject)
            {
                control = new ActiveXObject("WMPlayer.OCX.7");
            }
        else if (window.GeckoActiveXObject)
            {
                control = new GeckoActiveXObject("WMPlayer.OCX.7");
            }
        if(control)
            player_version= control.versionInfo
   
	    }
	
	catch(e){
	}
	return player_version;
}



/************************************************************************************/
/*******************	Flash		*************************************************/
/************************************************************************************/


function getFlashVersion(){
     if (BrowserName=="Firefox")
	    {
	        var useFlash = navigator.mimeTypes &&
            navigator.mimeTypes["application/x-shockwave-flash"] &&
            navigator.mimeTypes["application/x-shockwave-flash"].enabledPlugin
            if(useFlash)
                {
                    var flashversion = 0; 
                    x = navigator.plugins["Shockwave Flash"]; 
                    if (x) 
                        { 
                            if (x.description) 
                            { 
                                y = x.description; 
                                flashversion = y.charAt(y.indexOf('.')-1);
                                return  flashversion
                              } 
                          } 
   
  
                    }
                 else
                     return 0;
           }
	 else
	        {
	            for(var i=10; i>0; i--){
		        try{
			        var flash = new ActiveXObject("ShockwaveFlash.ShockwaveFlash." + i);
			        return i;
		            }
		        catch(e){
		                }
	        }
	}
	return 0;
}




/************************************************************************************/
/*******************	Shockwave		*********************************************/
/************************************************************************************/

function getSWVersion() {
	
	var tVersionString;

	if (navigator.mimeTypes && navigator.mimeTypes["application/x-director"] && navigator.mimeTypes["application/x-director"].enabledPlugin) {
		alert( navigator.mimeTypes["application/x-director"] );
		if (navigator.plugins && navigator.plugins["Shockwave for Director"] && (tVersionIndex = navigator.plugins["Shockwave for Director"].description.indexOf(".")) != - 1) {	
			tVersionString = navigator.plugins["Shockwave for Director"].description.substring(tVersionIndex-2, tVersionIndex+2);
		}
	} else if (navigator.userAgent && navigator.userAgent.indexOf("MSIE")>=0 && (navigator.userAgent.indexOf("Windows 95")>=0 || navigator.userAgent.indexOf("Windows 98")>=0 || navigator.userAgent.indexOf("Windows NT")>=0 )) {
		try{
			var tSWControl = new ActiveXObject("SWCtl.SWCtl");
			if(tSWControl)
				tVersionString = tSWControl.ShockwaveVersion("");
		}catch(e){}
	}
	
	return tVersionString;
	
}


function getShokwaveMajorVersion(swfVer){
	try{
		var arr = swfVer.split( "." );
		return parseInt(arr[0]);
	}catch(e){
		return 0;
	}
}
