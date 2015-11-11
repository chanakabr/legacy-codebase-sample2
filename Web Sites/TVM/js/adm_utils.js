// JScript File

function CreateClearUri(item) {
    // Exceptions are not handled - must be handled by caller
    if (typeof (item.asset_id) == 'undefined' || typeof (item.msisdn) == 'undefined' || typeof (item.tid) == 'undefined' || typeof (item.cp) == 'undefined' || typeof (item.ap) == 'undefined') {
        TvinciPlayerAddToLog('CreateClearUri - item is not defined properly');
        return '';
    }
    return 'http://switch3.castup.net/cunet/gm.asp?ai=545&ar=' + item.asset_id + '&ak=null&cuud=userid:' + item.msisdn + ',tid:' + item.tid + ',cp:' + item.cp + ',ap:' + item.ap + '';
}

function CreateSubtitlesUri(item) {
    // Exceptions are not handled - must be handled by caller
    if (typeof (item.asset_id) == 'undefined' || typeof (item.language) == 'undefined' || typeof (item.msisdn) == 'undefined' || typeof (item.tid) == 'undefined' || typeof (item.cp) == 'undefined' || typeof (item.ap) == 'undefined') {
        TvinciPlayerAddToLog('CreateSubtitlesUri - item is not defined properly');
        return '';
    }
    return 'http://switch3.castup.net/cunet/gm.asp?ai=545&ar=' + item.asset_id + '-' + item.language + '&ak=null&srt=true&cuud=userid:' + item.msisdn + ',tid:' + item.tid + ',cp:' + item.cp + ',ap:' + item.ap + '';
}

function copyToClipboard(s)
{
	if( window.clipboardData && clipboardData.setData )
	{
		clipboardData.setData("Text", s);
	}
	else
	{
		// You have to sign the code to enable this or allow the action in about:config by changing
		user_pref("signed.applets.codebase_principal_support", true);
		netscape.security.PrivilegeManager.enablePrivilege('UniversalXPConnect');

		var clip = Components.classes['@mozilla.org/widget/clipboard;[[[[1]]]]'].createInstance(Components.interfaces.nsIClipboard);
		if (!clip) return;

		// create a transferable
		var trans = Components.classes['@mozilla.org/widget/transferable;[[[[1]]]]'].createInstance(Components.interfaces.nsITransferable);
		if (!trans) return;

		// specify the data we wish to handle. Plaintext in this case.
		trans.addDataFlavor('text/unicode');

		// To get the data from the transferable we need two new objects
		var str = new Object();
		var len = new Object();

		var str = Components.classes["@mozilla.org/supports-string;[[[[1]]]]"].createInstance(Components.interfaces.nsISupportsString);

		var copytext=meintext;

		str.data=copytext;

		trans.setTransferData("text/unicode",str,copytext.length*[[[[2]]]]);

		var clipid=Components.interfaces.nsIClipboard;

		if (!clip) return false;

		clip.setData(trans,null,clipid.kGlobalClipboard);	   
	}
}

function changeMainChooser(tvmID)
{
    //window.frames.main_chooser.changeTvmAccount("un" , "pass");
    var qs = new Querystring(window.frames.main_chooser.location.href);
    var container = qs.get("container_id");
    newURL = "admin_category_chooser.aspx?start_channel_id=0&container_id=" + container + "&tvm_id=" + tvmID;
    window.frames.main_chooser.location.href = newURL;
}

function changeSubChooser(tvmID)
{
    //window.frames.sub_chooser.changeTvmAccount("un" , "pass");
    var qs = new Querystring(window.frames.main_chooser.location.href);
    var container = qs.get("container_id");
    newURL = "admin_category_chooser.aspx?start_channel_id=0&container_id=" + container + "&tvm_id=" + tvmID;
    window.frames.sub_chooser.location.href = newURL;
}


function TvinciPlayerAddToLog(ddd) {
    alert(ddd);
}


function GetVG()
  {

    var elem = document.frames['NDSFrame'];
    
    if (elem != null && elem != 'undefined')
    {
        return elem.GetVG();                            
    }else
    {
        return null;
    }
  
  }
          
String.prototype.trim = function() {
	return this.replace(/^\s+|\s+$/g,"");
}

var currentAdminPlayerID = null;
var NDSUser = "";
var NDSPassword = "";


function openFlyPlayer(flashvars, filetype, theID) {
    if (document.getElementById('PlayerWindow').style.display == 'none') {
        document.getElementById('PlayerWindow').style.display = 'block';
        if (filetype == "gib" && document.getElementById('ndsdiv').innerHTML == '')
            document.getElementById('ndsdiv').innerHTML = '<iframe id=\"NDSFrame\" width=\"0px;\" height=\"0px;\" src=\"NDSPlayer.htm\"></iframe>';
        if (document.getElementById('WMPObj') == null) {
            wmpObj = '<object id="WMPObj" style="DISPLAY: none" height="0" width="0" classid="CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6">';
            wmpObj += '<PARAM value="application/x-mplayer2" name="TYPE" />';
            wmpObj += '<PARAM value="http://www.microsoft.com/Windows/MediaPlayer/download/default.asp" name="PLUGINSPACE" />';
            wmpObj += '<PARAM value="false" name="Autostart" />';
            wmpObj += '<PARAM value="none" name="uiMode" />';
            wmpObj += '<PARAM value="flase" name="windowlessVideo" />';
            wmpObj += '<PARAM value="true" name="stretchToFit" />'
            wmpObj += '<PARAM value="0" name="ShowControls" />';
            wmpObj += '<PARAM value="" name="src" />';
            wmpObj += '</object>';
            document.getElementById('WMPDiv').innerHTML = wmpObj;

        }
        startPlayers(flashvars, theID);
    }
    else 
    {
        //document.getElementById('WMPObj').width = 1;
        document.getElementById('WMPObj').controls.stop();
        startPlayers(flashvars, theID);
    }
}

function centerDiv(theDiv) {
    var x = (window.screen.availWidth / 2) - (theDiv.offsetWidth / 2);
    var y = (window.screen.availHeight / 2) - (theDiv.offsetHeight / 2);
    document.getElementById("PlayerWindow").style.top = y;
    document.getElementById("PlayerWindow").style.left = x;
}


function closeFlyPlayer(theID) {
    //document.getElementById('WMPObj').width = 1;
    document.getElementById('WMPObj').controls.stop();
    document.getElementById('PlayerWindow').style.display = 'none';
    deleteFlashPlayer();
}
function deleteFlashPlayer() {
    try {
        if (currentAdminPlayerID != null)
            document.getElementById(currentAdminPlayerID).outerHTML = "<div id=\"" + currentAdminPlayerID + "\" style=\"position:absolute;z-index:0;top:0;left:0;width:100%;height:100%;\"></div>";
    }
    catch (ex) {
        alert(ex);
    }
}

function startPlayersInner(theFV, theID) {
    var flashObj = new SWFObj
    (
        'codebase', 'http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,0,0',
        'width', '475',
        'height', '304',
        'src', 'flash/lucy_player',
        'quality', 'high',
        'pluginspage', 'http://www.macromedia.com/go/getflashplayer',
        'align', 'right',
        'play', 'true',
        'loop', 'true',
        'scale', 'showall',
        'devicefont', 'false',
        'id', theID,
        'bgcolor', '#ffffff',
        'wmode', 'transparent',
        'name', theID,
        'menu', 'true',
        'allowFullScreen', 'true',
        'allowScriptAccess', 'sameDomain',
        'movie', 'flash/lucy_player',
        'salign', '',
        'flashVars', theFV
    ); //end AC code
    try {
        flashObj.outerWrite("PlayerOuterContainer");
    }
    catch (ex) {
        flashObj.outerWrite(currentAdminPlayerID);
    }
    centerDiv(document.getElementById("PlayerWindow"));
}

function startPlayers(theFV, theID) 
{
    startPlayersInner(theFV, theID);
    currentAdminPlayerID = theID;
    window.document.getElementById("player_closr_h").onclick = function() {
        closeFlyPlayer(theID);
    }
    initWMPPlayer(theID);
}

function initWMPPlayer(theID) {
    var interface = createWMPInterface();
    addInterfaceToCall(interface, "VideoWMPInterface");
    addInterfaceToCall(interface, "VideoInterface");
    //alert(window.document.getElementById("WMPObj"));
    //alert(window.document.getElementById("WMPDiv"));
    interface.init(window.document.getElementById("WMPObj"), window.document.getElementById("WMPDiv"), window.document.getElementsByName(theID)[0], window.document.getElementById("PlayerContainer"));
}


function TvinciPlayerAddToLog(ddd) {
    return;
}

function rtclickcheck(keyp)
{ 
    if (navigator.appName == "Netscape" && keyp.which == 3)
    { 	
        alert("Function not allowed"); return false; 
    } 
	if (navigator.appVersion.indexOf("MSIE") != -1 && event.button == 2) 
	{
	    alert("Function not allowed"); return false; 
	} 
} 
document.onmousedown = rtclickcheck;


function GetSafeDocumentIDVal(theID)
{
    //if (document.getElementById(theID) != null)
        //return getElementValue(document.getElementById(theID));
    if (window.document.getElementById(theID) != null)
        return window.document.getElementById(theID).value;
    if (window.document.getElementsByName(theID) != null && window.document.getElementsByName(theID)[0] != null)
        return window.document.getElementsByName(theID)[0].value;
    return "";
}

function getPagingGoto(order_by)
{
    GetPageTable(order_by,window.document.getElementById('paging_goto').value);
}

function GetObjectById(theID)
{
    if (window.document.getElementById(theID) != null)
        return window.document.getElementById(theID);
    if (window.document.getElementsByName(theID)[0] != null)
        return window.document.getElementsByName(theID)[0];
    return "";
}

// ==========================================================================
// Fuctions to mimic LTrim, RTrim, and Trim...

// Author          Aurיlien Tisnי	(CS)
// Date            03 avr. 2003 23:11:39
// Last Update     $Date$
// Version         $Revision$
// ==========================================================================

// --------------------------------------------------------------------------
// Remove leading blanks from our string.

// I               str - the string we want to LTrim
// Return          the input string without any leading whitespace

// Date            03 avr. 2003 23:12:13
// Author          Aurיlien Tisnי	(CS)
// --------------------------------------------------------------------------
function LTrim(str)
{
  var whitespace = new String(" \t\n\r");

  var s = new String(str);

  if (whitespace.indexOf(s.charAt(0)) != -1) {
    // We have a string with leading blank(s)...

    var j=0, i = s.length;

    // Iterate from the far left of string until we
    // don't have any more whitespace...
    while (j < i && whitespace.indexOf(s.charAt(j)) != -1)
    j++;


    // Get the substring from the first non-whitespace
    // character to the end of the string...
    s = s.substring(j, i);
  }

  return s;
}

// --------------------------------------------------------------------------
// Remove trailing blanks from our string.

// I               str - the string we want to RTrim
// Return          the input string without any trailing whitespace

// Date            03 avr. 2003 23:13:50
// Author          Aurיlien Tisnי	(CS)
// --------------------------------------------------------------------------
function RTrim(str)
{
  // We don't want to trip JUST spaces, but also tabs,
  // line feeds, etc.  Add anything else you want to
  // "trim" here in Whitespace
  var whitespace = new String(" \t\n\r");

  var s = new String(str);

  if (whitespace.indexOf(s.charAt(s.length-1)) != -1) {
    // We have a string with trailing blank(s)...

    var i = s.length - 1;       // Get length of string

    // Iterate from the far right of string until we
    // don't have any more whitespace...
    while (i >= 0 && whitespace.indexOf(s.charAt(i)) != -1)
      i--;


    // Get the substring from the front of the string to
    // where the last non-whitespace character is...
    s = s.substring(0, i+1);
  }

  return s;
}


// --------------------------------------------------------------------------
// Remove trailing and leading blanks from our string.

// I               str - the string we want to Trim
// Return          the trimmed input string

// Date            03 avr. 2003 23:15:09
// Author          Aurיlien Tisnי	(CS)
// --------------------------------------------------------------------------
function Trim(str)
{
  return RTrim(LTrim(str));
}

// EOF


function IsNumeric(sText)
{
    var ValidChars = "0123456789.";
    var IsNumber=true;
    var Char;

    for (i = 0; i < sText.length && IsNumber == true; i++) 
    { 
        Char = sText.charAt(i); 
        if (ValidChars.indexOf(Char) == -1) 
        {
            IsNumber = false;
        }
    }
    return IsNumber;
}


function callback_page_content(result)
{
    var split_array=result.split("~|~");
    for ( var i = 0 ; i < split_array.length ; i++ )
	{
	    theID = "page_content";
	    if (i > 0)
	        theID = theID + i;
	    //document.getElementById(theID).innerHTML = split_array[i];
	    window.document.getElementById(theID).innerHTML = split_array[i];
	    
	}
	//FlowPlayer=document.getElementById("FlowPlayer");
	FlowPlayer=window.document.getElementById("FlowPlayer");
	//if (anylinkmenu1 != null && anylinkmenu1 != "")
	    //anylinkmenu.init("menuanchorclass");
	window.scroll(0,0);
}

function BuiltEditors()
{
    var oInp = document.getElementsByTagName('textarea');
    var oRad = new Array();
    var j=0;
    for(var i=0;i<oInp.length;i++)
    {
        if(oInp[i].getAttribute('type')=='textarea')
        {
            var oFCKeditor = null;
            theID = oInp[i].getAttribute('id');
            oFCKeditor = new FCKeditor( theID );
            oFCKeditor.BasePath = "js/FCKeditor/" ;
	        oFCKeditor.Config[ "AutoDetectLanguage" ] = false ;
	        oFCKeditor.Config[ "DefaultLanguage" ] = "he" ;
	        oFCKeditor.Config[ "ContentLangDirection" ]	= "rtl" ;
	        oFCKeditor.Height = "300px"
	        oFCKeditor.ReplaceTextarea() ;
            j++;
        }
    }
}

function callback_page_content_with_editor(result) {
    callback_page_content(result);
    BuiltEditors();
}

function callback_page_content_sc(result)
{
    //document.getElementById("page_content").innerHTML = result;
    window.document.getElementById("page_content").innerHTML = result;
    
    //GetChapterAgain(document.getElementById('selector0').options[document.getElementById('selector0').selectedIndex].value);
    GetChapterAgain(window.document.getElementById('selector0').options[window.document.getElementById('selector0').selectedIndex].value);
    
}

function errorCallback(result)
{
    alert(result);
}

function downloadFile()
{
	//document.getElementById("downloadFile").click();
	window.document.getElementById("downloadFile").click();
	
}

function callback_create_csv(result)
{
    //document.getElementById("download_csv").href=result;
    //document.getElementById("download_csv").click();
    //window.document.getElementById("download_csv").href=result;
    //window.document.getElementById("download_csv").click();
    
}

function submitASPForm(sNewFormAction) {
    document.forms[0].action = sNewFormAction;
    document.forms[0].__VIEWSTATE.name = 'NOVIEWSTATE';
    document.forms[0].submit();
}

function popUp(URL , WiNname) 
{
    // WiNname
    var theWin = window.open(URL, WiNname, "toolbar=0,scrollbars=1,location=0,statusbar=0,menubar=0,resizable=0,width=800,height=525,left = 290,top = 249.5");
    if (theWin == null)
    {
        alert("המערכת זיהתה חסימה של PopUp Blocker. אנא נטרלו את פעולתם על האתר לעבודה תקינה.");
    }
}

function ChangePic(theID , theVal)
{
    RS.Execute("adm_utils.aspx", "ChangePic" , theID , theVal , callback_change_pic, errorCallback);       
}

function ChangeVid(theID , theVal , theTable)
{
    RS.Execute("adm_utils.aspx", "ChangeVid" , theID , theVal , theTable , callback_change_vid, errorCallback);       
}

function callback_ChangeActiveStateRow(result)
{
    var split_array=result.split("~~|~~");
    if (split_array.length = 2)
    {
        //document.getElementById(split_array[0]).innerHTML = split_array[1];
        window.document.getElementById(split_array[0]).innerHTML = split_array[1];
        
    }
}

function callback_ChangeOnOffStateRow(result)
{
    var split_array=result.split("~~|~~");
    if (split_array.length = 2)
    {
        //document.getElementById(split_array[0]).innerHTML = split_array[1];
        window.document.getElementById(split_array[0]).innerHTML = split_array[1];
        
    }
}

function callback_downloadreport(result) {
    //alert(result);
}


function callback_change_pic(result)
{
    var split_array=result.split("~|~");
    if (split_array.length = 2)
    {
        //document.getElementById(theID).value = split_array[0];
        window.document.getElementById(theID).value = split_array[0];
        
        var split2 = (split_array[0]).split("_val");
        theBrowseId = split2[0] + "_pic_beowse";
//        document.getElementById(theBrowseId).innerHTML = split_array[1];
        window.document.getElementById(theBrowseId).innerHTML = split_array[1];
        
    }
}

function ChangeVideoPlayer(theObjID , iframeSrc)
{
    theObjID = theObjID + "_palyer";
    theHTML = "<IFRAME SRC=\"" + iframeSrc;
    theHTML += "\" WIDTH=\"450\" HEIGHT=\"400\" FRAMEBORDER=\"0\"></IFRAME>";
    //document.getElementById(theObjID).innerHTML = theHTML;
    window.document.getElementById(theObjID).innerHTML = theHTML;
    
}

function callback_change_vid(result)
{
    var split_array=result.split("~|~");
    if (split_array.length = 2)
    {
        //document.getElementById(theID).value = split_array[0];
        var split2 = (split_array[0]).split("_val");
        theBrowseId = split2[0] + "_vid_beowse";
        //document.getElementById(theBrowseId).innerHTML = split_array[1];        
        window.document.getElementById(theBrowseId).innerHTML = split_array[1];        
        
    }
}

function OpenPicBrowser(theID , maxPics, lastPage)
{
    //theVal = document.getElementById(theID).value;
    theVal = window.document.getElementsByName(theID)[0].value;
    
    theURL = "adm_pic_popup_selector.aspx?pics_ids=" + theVal + "&theID=" + theID + "&maxPics=" + maxPics + "&lastPage=" + lastPage;
    popUp(theURL , 'PicSelector');
}


function OpenMediaTypeBrowser(theID, lastPage) {   
    theVal = window.document.getElementsByName(theID)[0].value;
    theURL = "adm_channel_media_types_popup_selector.aspx?channel_id=" + theVal + "&lastPage=" + lastPage;
    popUp(theURL, 'MediaTypeSelector');
    

}


//function OpenPicBrowserEpg(theID, maxPics, lastPage)
//{
//    debugger;
//    theVal = window.document.getElementsByName(theID)[0].value;
//    theURL = "adm_epg_pic_popup_selector.aspx?pics_ids=" + theVal + "&theID=" + theID + "&maxPics=" + maxPics + "&lastPage=" + lastPage ;
//    popUp(theURL, 'PicSelector');
//}



function OpenPicBrowserEpg(theID, maxPics, lastPage, epgIdentifier, channelID) {
    debugger;
    theVal = window.document.getElementsByName(theID)[0].value;
    theURL = "adm_epg_pic_popup_selector.aspx?pics_ids=" + theVal + "&theID=" + theID + "&maxPics=" + maxPics + "&lastPage=" + lastPage + "&epgIdentifier=" + epgIdentifier + "&channelID=" + channelID;
    popUp(theURL, 'PicSelector');
}

function OpenPicBrowser(theID, maxPics, lastPage, epgIdentifier) {
  
    theVal = window.document.getElementsByName(theID)[0].value;

    theURL = "adm_pic_popup_selector.aspx?pics_ids=" + theVal + "&theID=" + theID + "&maxPics=" + maxPics + "&lastPage=" + lastPage;// + "&epgIdentifier=" + epgIdentifier;
    popUp(theURL, 'PicSelector');
}


function OpenCommentsFilterBrowser() {
    
    theURL = "adm_comments_filter_tester_popup.aspx?regex=0";
    popUp(theURL, 'CommentsFilterTester');
}


function OpenVidBrowser(theID , maxPics , vidTable , vidTableTags , vidTableTagsRef)
{
    //theVal = document.getElementById(theID).value;
    //theVal = window.document.getElementById(theID).value;
    theVal = GetSafeDocumentIDVal(theID);
    theURL = "adm_video_popup_selector.aspx?pics_ids=" + theVal + "&theID=" + theID + "&maxPics=" + maxPics + "&vidTable=" + vidTable + "&vidTableTags=" + vidTableTags + "&vidTableTagsRef=" + vidTableTagsRef;
    popUp(theURL , 'PicSelector');
}

function tagSelect(CollectionVal , Collectiontxt , fieldHeader)
{
    Collectiontxt = Collectiontxt.replace(/~~qoute~~/g , "\"").replace(/~~apos~~/g , "'");
    Collectiontxt1 = Collectiontxt + ";";
    Collectiontxt2 = ";" + Collectiontxt + ";";
    fieldHeader = fieldHeader.replace(/~~qoute~~/g , "\"").replace(/~~apos~~/g , "'");
    //currVal = document.getElementById(fieldHeader+"_coll").value;
    currVal = window.document.getElementById(fieldHeader+"_coll").value;
    
    if (currVal.indexOf(Collectiontxt1) == 0)
    {
        //return;
        loc = currVal.indexOf(Collectiontxt1);
        len = Collectiontxt.length;
        currVal1 = currVal.substr(0 , loc);
        currVal2 = "";
        if ((loc + len) < currVal.length)
        {
            currVal2 = currVal.substr((loc + len + 1) , currVal.length - (loc + len + 1));
        }
        currVal = currVal1 + currVal2;
//        document.getElementById(fieldHeader+"_coll").value = currVal;
        window.document.getElementById(fieldHeader+"_coll").value = currVal;
        
        //if (window.document.getElementById("tag_" + CollectionVal) != null)
            //window.document.getElementById("tag_" + CollectionVal).className = "tags";
        if (window.document.getElementById("tag_" + CollectionVal) != null)
            window.document.getElementById("tag_" + CollectionVal).className = "tags";
        
        closeCollDiv(fieldHeader+"_coll");
    }
    else if (currVal.indexOf(Collectiontxt2) > 0)
    {
        loc = currVal.indexOf(Collectiontxt2);
        len = Collectiontxt.length;
        currVal1 = currVal.substr(0 , loc);
        currVal2 = "";
        if ((loc + len) < currVal.length)
        {
            currVal2 = currVal.substr((loc + len + 1) , currVal.length - (loc + len + 1));
        }
        currVal = currVal1 + currVal2;
        //document.getElementById(fieldHeader+"_coll").value = currVal;
        window.document.getElementById(fieldHeader+"_coll").value = currVal;
        
        //if (window.document.getElementById("tag_" + CollectionVal) != null)
            //window.document.getElementById("tag_" + CollectionVal).className = "tags";
        if (window.document.getElementById("tag_" + CollectionVal) != null)
            window.document.getElementById("tag_" + CollectionVal).className = "tags";
            
        closeCollDiv(fieldHeader+"_coll");
    }
    else
    {
        loc = currVal.lastIndexOf(";");
        len = currVal.length;
        currVal = currVal.substr(0 , loc + 1);
        currVal = currVal + Collectiontxt  + ";";
        //document.getElementById(fieldHeader+"_coll").value = currVal;
        window.document.getElementById(fieldHeader+"_coll").value = currVal;
        
        //if (window.document.getElementById("tag_" + CollectionVal) != null)
            //window.document.getElementById("tag_" + CollectionVal).className = "tags_selected";
        if (window.document.getElementById("tag_" + CollectionVal) != null)
            window.document.getElementById("tag_" + CollectionVal).className = "tags_selected";
        
        closeCollDiv(fieldHeader+"_coll");
    }
    
}
function SplitByChar(toSplit , theCharToSplitWith)
{
    loc = toSplit.lastIndexOf(theCharToSplitWith);
    len = toSplit.length;
    return toSplit.substr(loc + 1,len-loc);
}

function tagKeyPress(CollectionTable , MiddleTable , collectionTextField , CollCss , extraWhere , headerText , connKey)
{
    //var currVal = document.getElementById(headerText+"_coll").value;
    var currVal = window.document.getElementById(headerText+"_coll").value;
    
    theString = SplitByChar(currVal , ";");
    theString = SplitByChar(theString , ",");
    theString = SplitByChar(theString , ":");
    if (theString != "")
        RS.Execute("adm_utils.aspx", "GetCollectionFill" , MiddleTable , CollectionTable , collectionTextField , theString , CollCss , extraWhere , currVal , headerText , connKey , callback_coll_fill, errorCallback);       
    else
        closeCollDiv(headerText+"_coll");
    return true;
}

function getMousePosition(e) 
{ 
   return e.pageX ? {'x':e.pageX, 'y':e.pageY} : 
          {'x':e.clientX + document.documentElement.scrollLeft + document.body.scrollLeft, 'y':
          e.clientY + document.documentElement.scrollTop + document.body.scrollTop}; 
}; 
   
function showMousePos(e) 
{ 
   if (!e) e = event; // make sure we have a reference to the event 
   var mp = getMousePosition(e);
   //var divref = document.getElementById('PopUp');
   var divref = window.document.getElementById('PopUp');
   
   divref.style.left = mp.x + 'px';
   divref.style.top  = mp.y + 'px';   
   divref.style.display = 'block'; 
   return;
}; 


function openTechDetails(theUpdater , sCreate , sUpdate , sPublish)
{
    theHtml = "<table dir=ltr style=\"background-color: #FFFFFF;\"  cellspacing='1' cellpadding='0' border='1'>";
    theHtml += "<tr><td>";
    theHtml += "<table dir='ltr' border='0' cellpadding='6' cellspacing='0'>";
    theHtml += "<tr class='adm_table_header_nbg' style=\"FONT-WEIGHT: bold;FONT-SIZE: 12px;COLOR: #000000;FONT-FAMILY: Arial, Arial , David , Courier New ;border-color: #aaaaaa; TEXT-DECORATION: none;\">";
    theHtml += "<td nowrap valign=top>";
    theHtml += "<table>";
    theHtml += "<tr>";
    theHtml += "<td>Modified by: </td>";
    theHtml += "<td>" + theUpdater + "</td>";
    theHtml += "</tr>"; 
    theHtml += "<tr>";
    theHtml += "<td>Created: </td>";
    theHtml += "<td>" + sCreate + "</td>";
    theHtml += "</tr>"; 
    theHtml += "<tr>";
    theHtml += "<td>Updated: </td>";
    theHtml += "<td>" + sUpdate + "</td>";
    theHtml += "</tr>"; 
    theHtml += "<tr>";
    theHtml += "<td>Published: </td>";
    theHtml += "<td>" + sPublish + "</td>";
    theHtml += "</tr>"; 
    theHtml += "</table>";
    theHtml += "</td>";
    theHtml += "</tr></table></td></tr></table>";
    //oDiv = document.getElementById("tag_collections_div");
    oDiv = window.document.getElementById("tag_collections_div");
    
	oDiv.style.display = "block";
	var mp = getMousePosition(event);
	oDiv.style.left = (mp.x-130) + 'px';
	oDiv.style.top = mp.y + 'px';
	oDiv.innerHTML = theHtml;
//	Drag.init(oDiv);
    
}

function openLocalWindow(theText)
{
    if (theText == "")
        theText = "- No editor remarks -";
    theHtml = "<table dir=rtl style=\"background-color: #FFFFFF;\"  cellspacing='1' cellpadding='0' border='1' width='120px'>";
    theHtml += "<tr><td>";
    theHtml += "<table dir='rtl' border='0' cellpadding='6' cellspacing='0'>";
    theHtml += "<tr class='adm_table_header_nbg' style=\"FONT-WEIGHT: bold;FONT-SIZE: 12px;COLOR: #000000;FONT-FAMILY: Arial, Arial , David , Courier New ;border-color: #aaaaaa; TEXT-DECORATION: none;\">";
    theHtml += "<td nowrap valign=top>";
    theHtml += "<table>";
    theHtml += "<tr>";
    theHtml += "<td>" + theText + "</td>";
    theHtml += "</tr>"; 
    theHtml += "</table>";
    theHtml += "</td>";
    theHtml += "</tr></table></td></tr></table>";
    //oDiv = document.getElementById("tag_collections_div");
    oDiv = window.document.getElementById("tag_collections_div");
    
	oDiv.style.display = "block";
	var mp = getMousePosition(event);
	oDiv.style.left = (mp.x-130) + 'px';
	oDiv.style.top = mp.y + 'px';
	oDiv.innerHTML = theHtml;    
	//oDiv.innerHTML = theText
}

function searchPress(CollectionTable , CollectionTextField , CollectionPointerField , pageID , CollCss)
{
    //var currVal = document.getElementById(pageID+"_val_text").value;
    var currVal = window.document.getElementById(pageID+"_val_text").value;
    
    RS.Execute("adm_utils.aspx", "GetCollectionSearch" , currVal , CollectionTable , CollectionTextField , CollectionPointerField , pageID , CollCss , callback_coll_fill, errorCallback);       
}

function closeCollDiv(focusTo)
{
	//oClockDiv = document.getElementById("tag_collections_div");
	oClockDiv = window.document.getElementById("tag_collections_div");
	
	if (oClockDiv != null && oClockDiv != 'undefined')
	{
	    oClockDiv.innerHTML="";
	    if (focusTo != "")
	        //document.getElementById(focusTo).focus();
	        window.document.getElementById(focusTo).focus();
	        
	 }
}

function callback_coll_fill(result)
{
    if (result == "")
        return;
    loc = result.indexOf("|");
    
    CollectionTable = result.substr(0 , loc);
    theTable = result.substr(loc + 1 , result.length - loc);
    var coordinates = getAnchorPosition(CollectionTable+"_coll");
   
	//oDiv = document.getElementById("tag_collections_div");
	oDiv = window.document.getElementById("tag_collections_div");
	
	oDiv.style.display = "block";
	oDiv.style.left = coordinates.x + 300;
	oDiv.style.top = coordinates.y + 15;
	oDiv.innerHTML = theTable;
	Drag.init(oDiv);
}

function ShowBanner(path , width , height)
{
    var thePath = "adm_watch_flash.aspx?path=";
    thePath = thePath + path;
    thePath = thePath + "&width="
    thePath = thePath + width;
    thePath = thePath + "&height="
    thePath = thePath + height;
    window.open( thePath, "banner_watcher", 'toolbar=no,location=no,status=no,menubar=no,scrollbars=no,resizable=no,width=' + width + ',height=' + height ) ;
}

function getElementValue(formElement) {
    if(formElement.length != null && formElement.length != 0) var type = formElement[0].type;
    if((typeof(type) == 'undefined') || (type == 0)) var type = formElement.type;
    
    switch(type)
    {
        case 'undefined': return "";

        case 'radio':
	        for(var x=0; x < formElement.length; x++) 
		        if(formElement[x].checked == true)
	        return formElement[x].value;

        case 'select-multiple':
	        var myArray = new Array();
	        for(var x=0; x < formElement.length; x++) 
		        if(formElement[x].selected == true)
			        myArray[myArray.length] = formElement[x].value;
	        return myArray;

        case 'checkbox': return formElement.checked;
	
        default: return formElement.value;
    }
}

function grayOut(vis, options) 
{  
	// Pass true to gray out screen, false to ungray  
	// options are optional.  This is a JSON object with the following (optional) properties  
	// opacity:0-100         
	// Lower number = less grayout higher = more of a blackout   
	// zindex: #             
	// HTML elements with a higher zindex appear on top of the gray out  
	// bgcolor: (#xxxxxx)    
	// Standard RGB Hex color code  
	// grayOut(true, {'zindex':'50', 'bgcolor':'#0000FF', 'opacity':'70'});  
	// Because options is JSON opacity/zindex/bgcolor are all optional and can appear  
	// in any order.  Pass only the properties you need to set.  
	var options = options || {};   
	var zindex = options.zindex || 50;  
	var opacity = options.opacity || 70;  
	var opaque = (opacity / 100);  
	var bgcolor = options.bgcolor || '#000000';  
	//var dark=document.getElementById('darkenScreenObject');  
	var dark=window.document.getElementById('darkenScreenObject');  
	
	if (!dark) 
	{    
		// The dark layer doesn't exist, it's never been created.  So we'll    
		// create it here and apply some basic styles.    
		// If you are getting errors in IE see: http://support.microsoft.com/default.aspx/kb/927917    
		var tbody = document.getElementsByTagName("body")[0];    
		var tnode = document.createElement('div');           
		// Create the layer.        
		tnode.style.position='absolute';                 
		// Position absolutely        
		tnode.style.top='0px';                           
		// In the top        
		tnode.style.left='0px';                          
		// Left corner of the page        
		tnode.style.overflow='hidden';                   
		// Try to avoid making scroll bars                    
		tnode.style.display='none';                      
		// Start out Hidden        
		tnode.id='darkenScreenObject';                   
		// Name it so we can find it later    
		tbody.appendChild(tnode);                            
		// Add it to the web page    
		//dark=document.getElementById('darkenScreenObject');  
		dark=window.document.getElementById('darkenScreenObject');  
		
		// Get the object.  
	}  
	if (vis) 
	{    		
		// Calculate the page width and height     
		if( document.body && ( document.body.scrollWidth || document.body.scrollHeight ) ) 
		{        
			var pageWidth = document.body.scrollWidth+'px';        
			var pageHeight = document.body.scrollHeight+'px';    
		} 
		else if( document.body.offsetWidth ) 
		{      
			var pageWidth = document.body.offsetWidth+'px';      
			var pageHeight = document.body.offsetHeight+'px';    
		} 
		else 
		{       
			var pageWidth='100%';       
			var pageHeight='100%';    
		}       
		//set the shader to cover the entire page and make it visible.    
		dark.style.opacity=opaque;
		dark.style.MozOpacity=opaque;                       
		dark.style.filter='alpha(opacity='+opacity+')';     
		dark.style.zIndex=zindex;            
		dark.style.backgroundColor=bgcolor;      
		dark.style.width= pageWidth;    
		dark.style.height= pageHeight;    
		dark.style.display='block';                            
	} 
	else 
	{     
		dark.style.display='none';  
	}
}

function ChangeActiveStateRow(theTableName, theID, requestedStatus, sConnectionKey, sPage) {
    if (theTableName != null && theTableName != 'undefined' && theTableName == 'comment_filters') {
        RS.Execute("adm_comments_filter.aspx", "ChangeActiveStateRow", theTableName, theID, requestedStatus, sConnectionKey, callback_ChangeActiveStateRow, errorCallback);
    }
    
    RS.Execute("adm_utils.aspx", "ChangeActiveStateRow", theTableName, theID, requestedStatus, sConnectionKey, sPage, callback_ChangeActiveStateRow, errorCallback);

    if (sPage != null && sPage != "")
        RS.Execute(sPage, "UpdateOnOffStatus", theTableName, theID, requestedStatus);
}

//function ChangeActiveStateRow(theTableName, theID, requestedStatus) {
    //RS.Execute("adm_utils.aspx", "ChangeActiveStateRow", theTableName, theID, requestedStatus, callback_ChangeActiveStateRow, errorCallback);
//}

function ChangeOrderNumRow(theTableName , theID , theFieldName , deltaNum , connKey)
{
    RS.Execute("adm_utils.aspx", "ChangeOrderNumRow" , theTableName , theID , theFieldName , deltaNum , connKey, callback_ChangeActiveStateRow, errorCallback);       
}

function ChangeOnOffStateRow(theTableName ,theFieldName, theIndexField , theIndexVal , requestedStatus ,theOnStr,theOffStr , connKey) {
    if (theTableName != null && theTableName != 'undefined' && theTableName == 'comment_filters') {
        RS.Execute("adm_comments_filter.aspx", "ChangeActiveStateRow", theTableName, theIndexVal, requestedStatus, connKey, callback_ChangeActiveStateRow, errorCallback);
    }
    else
        RS.Execute("adm_utils.aspx", "ChangeOnOffStateRow", theTableName, theFieldName, theIndexField, theIndexVal, requestedStatus, theOnStr, theOffStr, connKey, callback_ChangeActiveStateRow, errorCallback);
}

function CheckEnter()
{
    if (event.keyCode == 13)
        return true;
    return false;
}

function pageWidth() 
{
    return window.innerWidth != null? window.innerWidth : document.documentElement && document.documentElement.clientWidth ?       document.documentElement.clientWidth : document.body != null ? document.body.clientWidth : null;
} 
function pageHeight() {
    return  window.innerHeight != null? window.innerHeight : document.documentElement && document.documentElement.clientHeight ?  document.documentElement.clientHeight : document.body != null? document.body.clientHeight : null;
} 
function posLeft() 
{
    return typeof window.pageXOffset != 'undefined' ? window.pageXOffset :document.documentElement && document.documentElement.scrollLeft ? document.documentElement.scrollLeft : document.body.scrollLeft ? document.body.scrollLeft : 0;
} 
function posTop() 
{
    return typeof window.pageYOffset != 'undefined' ?  window.pageYOffset : document.documentElement && document.documentElement.scrollTop ? document.documentElement.scrollTop : document.body.scrollTop ? document.body.scrollTop : 0;
} 
function posRight() 
{
    return posLeft()+pageWidth();
} 
function posBottom() 
{
    return posTop()+pageHeight();
}      

function OpenID(theID)
{
	//document.getElementById(theID).style.display = "";
	window.document.getElementById(theID).style.display = "";
	
}

function CloseID(theID)
{
	//document.getElementById(theID).style.display = "none";
	window.document.getElementById(theID).style.display = "none";
	
}

var overrideMsg = '';

function submitASPFormWithCheck(thePage) {
    
    if( validateGenericForm() )
        submitASPForm(thePage);
    else
    {
        grayOut(true, { 'opacity': '35' });
        if (overrideMsg != '') {
            alert(overrideMsg);
            overrideMsg = '';
        }
        else
            alert("Please fill all requiered fields");
        grayOut(false);
    }
}

function validateGenericForm()
{
    var bCont = true;
    var i = 0;
    var bOK = true;
    while (bCont == true)
    {
        var sName = i + "_val";
        var sNameMin = i + "_valHour";
        var sNameHour = i + "_valMin";
        var sName2 = i + "_val2";
        var sType = i + "_type";
        var sMust = i + "_must";
        var sMin = i + "_min";
        var sMax = i + "_max";
        var sID = i + "_id";
        var sInputType = i + "_inputtype";
        var sValidation = i + "_validation";
        //if (document.getElementById(sType) == null)
        if (window.document.getElementsByName(sType)[0] == null)
        {
            bCont = false;
        }
        else
        {
            //var sTypeVal = document.getElementsByName(sType)[0].value;
            var sTypeVal = window.document.getElementsByName(sType)[0].value;
            
            if (sTypeVal == "date" || sTypeVal == "string" || sTypeVal == "int" || sTypeVal == "long_string" || sTypeVal == "file" || sTypeVal == "multi")
            {
                //if (document.getElementsByName(sInputType)[0] == null)
                if (window.document.getElementsByName(sInputType)[0] == null)
                {
                    //theVal = getElementValue(document.getElementsByName(sName)[0]);
                    theVal = getElementValue(window.document.getElementsByName(sName)[0]);
                    
                }
                else
                {
                    theVal = getElementValue(document.getElementsByName(sName));
                }
                //if (document.getElementsByName(sMust)[0] != null)
                
                if (window.document.getElementsByName(sMust)[0] != null)
                {
                    //if (getElementValue(document.getElementsByName(sMust)[0]) == "True")
                    
                    if (getElementValue(window.document.getElementsByName(sMust)[0]).toLowerCase() == "true")
                    {
                        if (theVal == null || theVal == "")
                        {
                            bOK = false;
                            //document.getElementsByName(sName)[0].style.cssText = "border: 1px solid red;color: red;";
                            window.document.getElementsByName(sName)[0].style.cssText = "border: 1px solid red;color: red;";
                            
                        }
                    }
                }

                if (sTypeVal == "int") {
                    if (theVal != '' && parseInt(theVal).toString() != theVal) {
                        bOK = false;
                        overrideMsg = 'Field has to be int';
                        window.document.getElementsByName(sName)[0].style.cssText = "border: 1px solid red;color: red;";
                    }

                    var minEle = window.document.getElementsByName(sMin)[0];
                    if (minEle != null) {
                        if (parseInt(minEle.value) > parseInt(theVal)) {
                            bOK = false;
                            overrideMsg = "Field has to be >= " + minEle.value;
                            window.document.getElementsByName(sName)[0].style.cssText = "border: 1px solid red;color: red;";
                        }
                    }

                    var maxEle = window.document.getElementsByName(sMax)[0];
                    if (maxEle != null) {
                        if (parseInt(maxEle.value) < parseInt(theVal)) {
                            bOK = false;
                            overrideMsg = "Field has to be <= " + maxEle.value;
                            window.document.getElementsByName(sName)[0].style.cssText = "border: 1px solid red;color: red;";
                        }
                    }
                }
            }
            if (sTypeVal == "datetime")
            {
                //theVal = getElementValue(document.getElementsByName(sName)[0]);
                theVal = getElementValue(window.document.getElementsByName(sName)[0]);
                
                //theValMin = getElementValue(document.getElementsByName(sNameMin)[0]);
                theValMin = getElementValue(window.document.getElementsByName(sNameMin)[0]);
                
                //theValHour = getElementValue(document.getElementsByName(sNameHour)[0]);
                theValHour = getElementValue(window.document.getElementsByName(sNameHour)[0]);
                
                
                //if (document.getElementsByName(sMust)[0] != null)
                
                if (window.document.getElementsByName(sMust)[0] != null)
                {
                
                    //if (getElementValue(document.getElementsByName(sMust)[0]) == "True")
                    
                    if (getElementValue(window.document.getElementsByName(sMust)[0]).toLowerCase() == "true")
                    {
                        if (theVal == "" || theValMin == "" || theValHour == "")
                        {
                            bOK = false;
                            //document.getElementsByName(sName)[0].style.cssText = "border: 1px solid red;color: red;";
                            window.document.getElementsByName(sName)[0].style.cssText = "border: 1px solid red;color: red;";
                            
                            //document.getElementsByName(sNameMin)[0].style.cssText = "border: 1px solid red;color: red;";
                            window.document.getElementsByName(sNameMin)[0].style.cssText = "border: 1px solid red;color: red;";
                            //document.getElementsByName(sNameHour)[0].style.cssText = "border: 1px solid red;color: red;";
                            window.document.getElementsByName(sNameHour)[0].style.cssText = "border: 1px solid red;color: red;";
                        }
                        else
                        {
                            //document.getElementsByName(sName)[0].style.cssText = "border: 1px solid #000000;color: #000000;";
                            window.document.getElementsByName(sName)[0].style.cssText = "border: 1px solid #000000;color: #000000;";
                            
                            //document.getElementsByName(sNameMin)[0].style.cssText = "border: 1px solid #000000;color: #000000;";
                            window.document.getElementsByName(sNameMin)[0].style.cssText = "border: 1px solid #000000;color: #000000;";
                            
                            //document.getElementsByName(sNameHour)[0].style.cssText = "border: 1px solid #000000;color: #000000;";
                            window.document.getElementsByName(sNameHour)[0].style.cssText = "border: 1px solid #000000;color: #000000;";
                            
                        }
                    }
                }
            }
            if (sTypeVal == "time")
            {
                //theVal = getElementValue(document.getElementsByName(sName)[0]);
                theVal = getElementValue(window.document.getElementsByName(sName)[0]);
                
                //theVal2 = getElementValue(document.getElementsByName(sName2)[0]);
                theVal2 = getElementValue(window.document.getElementsByName(sName2)[0]);
                
                //if (document.getElementsByName(sMust)[0] != null)
                if (window.document.getElementsByName(sMust)[0] != null)
                {
                
                    //if (getElementValue(document.getElementsByName(sMust)[0]) == "True")
                    
                    if (getElementValue(window.document.getElementsByName(sMust)[0]) == "True")
                    {
                        if (theVal == "" || theVal2 == "")
                        {
                            bOK = false;
                            //document.getElementsByName(sName)[0].style.cssText = "border: 1px solid red;color: red;";
                            //document.getElementsByName(sName2)[0].style.cssText = "border: 1px solid red;color: red;";
                            
                            window.document.getElementsByName(sName)[0].style.cssText = "border: 1px solid red;color: red;";
                            window.document.getElementsByName(sName2)[0].style.cssText = "border: 1px solid red;color: red;";
                        }
                        else
                        {
                            //document.getElementsByName(sName)[0].style.cssText = "border: 1px solid #000000;color: #000000;";
                            //document.getElementsByName(sName2)[0].style.cssText = "border: 1px solid #000000;color: #000000;";
                            
                            window.document.getElementsByName(sName)[0].style.cssText = "border: 1px solid #000000;color: #000000;";
                            window.document.getElementsByName(sName2)[0].style.cssText = "border: 1px solid #000000;color: #000000;";
                        }
                    }
                }
            }
        }
        i = i + 1;
    }
    return bOK;
}
