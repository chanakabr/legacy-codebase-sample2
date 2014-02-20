

/************************************************************************************/
/*******************	Operating System		*************************************/
/************************************************************************************/
var OSVer;
var UserAgent;
var timeStart = new Date();
var timeEnd = new Date();
function getConnectionSpeed()
{
    timeStart = new Date();
    objImage = new Image();     
    objImage.onLoad=imagesLoaded();
    objImage.src='http://admin.tvinci.com/pics/100708120829_full.bmp';
}

function imagesLoaded()
{         
    var timeEnd = new Date();
    var theSize = 1057974;
    alert(timeEnd - timeStart);
}

function getOperatingSystemVersion(){
try
{
	(typeof(navigator.userAgent) != "undefined") ?  UserAgent = navigator.userAgent : UserAgent = "Property not supported or blank!";
    if(UserAgent.indexOf("Windows NT 6.0") > -1){
        if(UserAgent.indexOf("WOW64") > -1)
		    OSVer = "Windows Vista 64 bit";
		else
		    OSVer = "Windows Vista "
	}
	else if(UserAgent.indexOf("NT 5.2") > -1){
		OSVer = "Windows 2003";
	}
	else if(UserAgent.indexOf("Windows NT 5.1") > -1){
		OSVer = "Windows XP";
	}
	else if(UserAgent.indexOf("Windows NT 5.0") > -1){
		OSVer = "Windows 2000";
	}
	else if(UserAgent.indexOf("Windows 98") > -1){
		OSVer = "Windows 98";
	}
	else if(UserAgent.indexOf("Windows 95") > -1){
		OSVer = "Windows 95";
	}
	
	else if(UserAgent.indexOf("Mac OS X") > -1){
		OSVer = "Mac OS X";
	}
	else if(UserAgent.indexOf("Macintosh") > -1){
		OSVer = "Mac OS";
	}
	else if(navigator.appVersion.indexOf("X11")!=-1) 
	    OSVer = "Unix"; 
	else if (navigator.appVersion.indexOf("Linux")!=-1) 
	    OSVer = "Linux";
	else {
		OSVer = "Unknown";
	}
	return OSVer;
	}
	catch(e){}
}


/************************************************************************************/
/*******************	Browser		*************************************************/
/************************************************************************************/


function getBrowserName(){
	return browser;
}

function getBrowserVersion(){
	return version;
}

function getBrowserMinorVersion(){	
	var OSMinorVer;
	(typeof(navigator.appMinorVersion) != "undefined") ? OSMinorVer = navigator.appMinorVersion : OSMinorVer = "Property not supported or blank!";
	return OSMinorVer;
}

function getColorDepth()
{
    theRes = "";
    theRes = screen.colorDepth;
    return theRes + "bit";
}

function getScreenResolution()
{
    return screen.width +":"+ screen.height;
}

function getCookiesEnabled()
{
    try
    {
        return navigator.cookieEnabled;
    }
    catch(e)
    {
        return false;
    }
    
}

function getJavascriptEnabled()
{
    return true;
}

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

function getFlashVersion(){
    BrowserName = getBrowserName();
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

var detect = navigator.userAgent.toLowerCase();
var browser,version,total,thestring;

if (checkIt('konqueror'))
{
	browser = "Konqueror";
}
else if (checkIt('safari')) browser = "Safari";
else if (checkIt('omniweb')) browser = "OmniWeb";
else if (checkIt('opera')) browser = "Opera";
else if (checkIt('webtv')) browser = "WebTV";
else if (checkIt('icab')) browser = "iCab";
else if (checkIt('msie')) browser = "Microsoft Internet Explorer";
else if (checkIt('firefox')) browser = "Firefox";
else if (!checkIt('compatible'))
{
	browser = "Netscape Navigator";
	version = detect.charAt(8);
}
else browser = "An unknown browser";

if (!version) version = detect.charAt(place + thestring.length);

	function checkIt(string)
	{
		place = detect.indexOf(string) + 1;
		thestring = string;
		return place;
	}


