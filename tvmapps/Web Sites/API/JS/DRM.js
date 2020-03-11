
/************************************************************************************/
/*******************	DRM System		*********************************************/
/************************************************************************************/

function isDRMInstalled(){
	try{
	
	 if (BrowserName=="Firefox")
	    {
	        
	    }
	    else
	    {
	
		    var netobj = new ActiveXObject("DRM.GetLicense.1");
		    if(netobj){
			    netobj = null;
			    return true;
		    }else{
			    return false;
		}
		}
	}catch(e){
		return false;
		netobj = null;
	}
}


function getDRMVersion(){
	var drmv = 0;
	try{
		var netobj = new ActiveXObject("DRM.GetLicense.1");
		drmv = netobj.getDRMVersion();
		netobj = null;
	}catch(e){
		drmv = 0;
	}
	return drmv;
}

function getDRMSecurityVersion(){
	var drmsv = 0;
	try{
		var netobj = new ActiveXObject("DRM.GetLicense.1");
		drmsv = netobj.getDRMSecurityVersion();
		netobj = null;
	}catch(e){
		drmsv = 0;
	}
	return drmsv;
}
