function startChecks(){
	checkSystemConfiguration();
	checkConnectionSpeed();
	checkApplications();
	checkDRM();
}

function checkSystemConfiguration() 
{
    try
    {
        var hasError = false;
        //Operating System
        if( !checkOperatingSystem() )
	        hasError = true;

        //Browser
        if( !checkBrowser() )
	        hasError = true;

        //Screen Resolution
        if( !checkResolution() )
	        hasError = true;

        //Color Depth
        if( !checkColorDepth() )
	        hasError = true;

        //Cookies Enabled
        if( !checkCookies() )
	        hasError = true;
    }
    catch(e)
    {
        hasError = true;
    }
    if( !hasError ) 
    {
	    document.getElementById("SysConf_Error").innerHTML = "&nbsp;";
    }
}


function checkOperatingSystem()
{
    var theOS = "";
    try
    {
        (typeof(navigator.userAgent) != "undefined") ?  strOS = navigator.userAgent : strOS = "Property not supported or blank!";
        var osVer = getOperatingSystemVersion();
        if( osVer.indexOf("Windows") >= 0 ) 
        {
            if( osVer.indexOf("95") >= 0)
            {
	            return false;
            }
            else
            {
	            document.getElementById(hdnOperatingSystemStatus_id).value = "1";
	            document.getElementById("SysConf_OS_Status").src = imgStatus_Success;
	            return true;
	        }
        } 
        else 
        {
	        document.getElementById(hdnOperatingSystemStatus_id).value = "1";
	        document.getElementById("SysConf_OS_Status").src = imgStatus_Failure;
	        return false;
        }
    }
    catch(e)
    {
        return false;
    }
}


function checkBrowser(){
try{
var browserName = getBrowserName();
var browserVersion = getBrowserVersion();
document.getElementById(hdnBrowser_id).value = browserName +" (Version "+ browserVersion +")";
document.getElementById("SysConf_WB_Result").innerHTML = browserName +" (Version "+ browserVersion +")";
if( browserName == "Microsoft Internet Explorer") 
    {
     
    
        temp=navigator.appVersion.split("MSIE")
        version=parseFloat(temp[1])
        if (version>=5.5)
        {
	        document.getElementById(hdnBrowserStatus_id).value = "1";
	        document.getElementById("SysConf_WB_Status").src = imgStatus_Success;
	        return true;
	    }
	    else
	    {
	        document.getElementById(hdnBrowserStatus_id).value = "0";
	        document.getElementById("SysConf_WB_Status").src = imgStatus_Failure;
	        document.getElementById(IE_Err_id).innerHTML ="Details";
	        document.getElementById(IE_Err_id).style.display = "block";
	    return false;
	       
	     }
       }
 else 
    {
     if(browserName == "Firefox")
        {
         
        if((navigator.userAgent.substr(navigator.userAgent.indexOf("Firefox/")+8,3)>="1.5"))
            {
            
	        document.getElementById(hdnBrowserStatus_id).value = "1";
	        document.getElementById("SysConf_WB_Status").src = imgStatus_Success;
	        return true;       
            }
           else
           {
	        document.getElementById(hdnBrowserStatus_id).value = "0";
	        document.getElementById("SysConf_WB_Status").src = imgStatus_Failure;
	        document.getElementById(FF_Err_id).innerHTML ="Details";
	        document.getElementById(FF_Err_id).style.display = "block";
	        return false;
	        }
	    }
	  else
	  {
	   document.getElementById(hdnBrowserStatus_id).value = "0";
	   document.getElementById("SysConf_WB_Status").src = imgStatus_Failure;
	   return false;
	  }
}
}
catch(e){
return false;
}
}


function checkResolution(){
   try
   {
document.getElementById(hdnScreenResolution_id).value = screen.width +":"+ screen.height;
document.getElementById("SysConf_SR_Result").innerHTML = screen.width +":"+ screen.height;
if( screen.width >= 800 ) {
	document.getElementById(hdnScreenResolutionStatus_id).value = "1";
	document.getElementById("SysConf_SR_Status").src = imgStatus_Success;
	return true;
} else {
	document.getElementById(hdnScreenResolutionStatus_id).value = "0";
	document.getElementById("SysConf_SR_Status").src = imgStatus_Failure;
    document.getElementById(Res_Err_id).innerHTML ="Details";
	document.getElementById(Res_Err_id).style.display = "block";
	return false;
}
}
catch(e){
return false;
}
}


function checkColorDepth() {
 try
   {
document.getElementById(hdnColorDepth_id).value = screen.colorDepth;
document.getElementById("SysConf_CD_Result").innerHTML = screen.colorDepth +"bit" ;
if( screen.colorDepth < 16 ) {
	document.getElementById(hdnColorDepthStatus_id).value = "0";
	document.getElementById("SysConf_CD_Status").src = imgStatus_Failure;
    document.getElementById(Color_Err_id).innerHTML ="Details";
	document.getElementById(Color_Err_id).style.display = "block";
	return false;
} else if( screen.colorDepth < 32 ) {
	document.getElementById(hdnColorDepthStatus_id).value = "1";
	document.getElementById("SysConf_CD_Status").src = imgStatus_Exclamation;
	document.getElementById(Color_Err_id).innerHTML ="Details";
	document.getElementById(Color_Err_id).style.display = "block";
	return true;
} else {
	document.getElementById(hdnColorDepthStatus_id).value = "1";
	document.getElementById("SysConf_CD_Status").src = imgStatus_Success;
	return true;
}
}
catch(e){
return false;
}
}


function checkCookies() {
try
{
if( navigator.cookieEnabled ) {
	document.getElementById(hdnCookiesEnabledStatus_id).value = "1";
	document.getElementById("SysConf_CE_Result").innerHTML = "Yes";
	document.getElementById("SysConf_CE_Status").src = imgStatus_Success;
	return true;
}else{
	document.getElementById(hdnCookiesEnabledStatus_id).value = "0";
	document.getElementById("SysConf_CE_Result").innerHTML = "No";
	document.getElementById("SysConf_CE_Status").src = imgStatus_Failure;
	if(getBrowserName() == "Microsoft Internet Explorer")
	{
	document.getElementById(IE_CookieErr_id).innerHTML ="Details";
	document.getElementById(IE_CookieErr_id).style.display = "block";
	}
	else
	{
	document.getElementById(FF_CookieErr_id).innerHTML ="Details";
	document.getElementById(FF_CookieErr_id).style.display = "block";
	}
	
	return false;
}
}
catch(e){
return false;
}
}