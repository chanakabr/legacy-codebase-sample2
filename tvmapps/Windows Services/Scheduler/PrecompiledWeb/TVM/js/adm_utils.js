// JScript File
String.prototype.trim = function() {
	return this.replace(/^\s+|\s+$/g,"");
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
        return getElementValue(window.document.getElementById(theID));
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

function callback_page_content_with_editor(result)
{
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
    window.document.getElementById("download_csv").href=result;
    window.document.getElementById("download_csv").click();
    
}

function submitASPForm(sNewFormAction)
{
    document.forms[0].action = sNewFormAction;
    document.forms[0].__VIEWSTATE.name = 'NOVIEWSTATE';
    document.forms[0].submit();
}

function popUp(URL , WiNname) 
{
    theWin = window.open(URL, WiNname , "toolbar=0,scrollbars=1,location=0,statusbar=0,menubar=0,resizable=0,width=700,height=525,left = 290,top = 249.5");
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

function OpenPicBrowser(theID , maxPics)
{
    //theVal = document.getElementById(theID).value;
    theVal = window.document.getElementsByName(theID)[0].value;
    
    theURL = "adm_pic_popup_selector.aspx?pics_ids=" + theVal + "&theID=" + theID + "&maxPics=" + maxPics;
    popUp(theURL , 'PicSelector');
}

function OpenVidBrowser(theID , maxPics , vidTable , vidTableTags , vidTableTagsRef)
{
    //theVal = document.getElementById(theID).value;
    theVal = window.document.getElementById(theID).value;
    
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

function tagKeyPress(CollectionTable , MiddleTable , collectionTextField , CollCss , extraWhere , headerText)
{
    //var currVal = document.getElementById(headerText+"_coll").value;
    var currVal = window.document.getElementById(headerText+"_coll").value;
    
    theString = SplitByChar(currVal , ";");
    theString = SplitByChar(theString , ",");
    theString = SplitByChar(theString , ":");
    if (theString != "")
        RS.Execute("adm_utils.aspx", "GetCollectionFill" , MiddleTable , CollectionTable , collectionTextField , theString , CollCss , extraWhere , currVal , headerText , callback_coll_fill, errorCallback);       
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

function getElementValue(formElement)
{
    if(formElement.length != null) var type = formElement[0].type;
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

function ChangeActiveStateRow(theTableName , theID , requestedStatus)
{
    RS.Execute("adm_utils.aspx", "ChangeActiveStateRow" , theTableName , theID , requestedStatus , callback_ChangeActiveStateRow, errorCallback);       
}

function ChangeOrderNumRow(theTableName , theID , theFieldName , deltaNum)
{
    RS.Execute("adm_utils.aspx", "ChangeOrderNumRow" , theTableName , theID , theFieldName , deltaNum , callback_ChangeActiveStateRow, errorCallback);       
}

function ChangeOnOffStateRow(theTableName ,theFieldName, theIndexField , theIndexVal , requestedStatus ,theOnStr,theOffStr)
{
    RS.Execute("adm_utils.aspx", "ChangeOnOffStateRow" , theTableName , theFieldName , theIndexField , theIndexVal , requestedStatus , theOnStr , theOffStr , callback_ChangeActiveStateRow, errorCallback);       
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

function submitASPFormWithCheck(thePage)
{
    if( validateGenericForm() )
        submitASPForm(thePage);
    else
    {
        grayOut(true , {'opacity':'35'});
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
        var sID = i + "_id";
        var sInputType = i + "_inputtype";
        var sValidation = i + "_validation";
        //if (document.getElementById(sType) == null)
        if (window.document.getElementById(sType) == null)
        {
            bCont = false;
        }
        else
        {
            //var sTypeVal = document.getElementById(sType).value;
            var sTypeVal = window.document.getElementById(sType).value;
            
            if (sTypeVal == "date" || sTypeVal == "string" || sTypeVal == "int" || sTypeVal == "long_string" || sTypeVal == "file" || sTypeVal == "multi")
            {
                //if (document.getElementById(sInputType) == null)
                if (window.document.getElementById(sInputType) == null)
                {
                    //theVal = getElementValue(document.getElementById(sName));
                    theVal = getElementValue(window.document.getElementById(sName));
                    
                }
                else
                {
                    theVal = getElementValue(document.getElementsByName(sName));
                }
                //if (document.getElementById(sMust) != null)
                
                if (window.document.getElementById(sMust) != null)
                {
                    //if (getElementValue(document.getElementById(sMust)) == "True")
                    
                    if (getElementValue(window.document.getElementById(sMust)) == "True")
                    {
                        if (theVal == null || theVal == "")
                        {
                            bOK = false;
//                            document.getElementById(sName).style.cssText = "border: 1px solid red;color: red;";
                            window.document.getElementById(sName).style.cssText = "border: 1px solid red;color: red;";
                            
                        }
                    }
                }
            }
            if (sTypeVal == "datetime")
            {
                //theVal = getElementValue(document.getElementById(sName));
                theVal = getElementValue(window.document.getElementById(sName));
                
                //theValMin = getElementValue(document.getElementById(sNameMin));
                theValMin = getElementValue(window.document.getElementById(sNameMin));
                
                //theValHour = getElementValue(document.getElementById(sNameHour));
                theValHour = getElementValue(window.document.getElementById(sNameHour));
                
                
                //if (document.getElementById(sMust) != null)
                
                if (window.document.getElementById(sMust) != null)
                {
                
                    //if (getElementValue(document.getElementById(sMust)) == "True")
                    
                    if (getElementValue(window.document.getElementById(sMust)) == "True")
                    {
                        if (theVal == "" || theValMin == "" || theValHour == "")
                        {
                            bOK = false;
                            //document.getElementById(sName).style.cssText = "border: 1px solid red;color: red;";
                            window.document.getElementById(sName).style.cssText = "border: 1px solid red;color: red;";
                            
                            //document.getElementById(sNameMin).style.cssText = "border: 1px solid red;color: red;";
                            window.document.getElementById(sNameMin).style.cssText = "border: 1px solid red;color: red;";
                            //document.getElementById(sNameHour).style.cssText = "border: 1px solid red;color: red;";
                            window.document.getElementById(sNameHour).style.cssText = "border: 1px solid red;color: red;";
                        }
                        else
                        {
                            //document.getElementById(sName).style.cssText = "border: 1px solid #000000;color: #000000;";
                            window.document.getElementById(sName).style.cssText = "border: 1px solid #000000;color: #000000;";
                            
                            //document.getElementById(sNameMin).style.cssText = "border: 1px solid #000000;color: #000000;";
                            window.document.getElementById(sNameMin).style.cssText = "border: 1px solid #000000;color: #000000;";
                            
                            //document.getElementById(sNameHour).style.cssText = "border: 1px solid #000000;color: #000000;";
                            window.document.getElementById(sNameHour).style.cssText = "border: 1px solid #000000;color: #000000;";
                            
                        }
                    }
                }
            }
            if (sTypeVal == "time")
            {
                //theVal = getElementValue(document.getElementById(sName));
                theVal = getElementValue(window.document.getElementById(sName));
                
                //theVal2 = getElementValue(document.getElementById(sName2));
                theVal2 = getElementValue(window.document.getElementById(sName2));
                
                //if (document.getElementById(sMust) != null)
                if (window.document.getElementById(sMust) != null)
                {
                
                    //if (getElementValue(document.getElementById(sMust)) == "True")
                    
                    if (getElementValue(window.document.getElementById(sMust)) == "True")
                    {
                        if (theVal == "" || theVal2 == "")
                        {
                            bOK = false;
                            //document.getElementById(sName).style.cssText = "border: 1px solid red;color: red;";
                            //document.getElementById(sName2).style.cssText = "border: 1px solid red;color: red;";
                            
                            window.document.getElementById(sName).style.cssText = "border: 1px solid red;color: red;";
                            window.document.getElementById(sName2).style.cssText = "border: 1px solid red;color: red;";
                        }
                        else
                        {
                            //document.getElementById(sName).style.cssText = "border: 1px solid #000000;color: #000000;";
                            //document.getElementById(sName2).style.cssText = "border: 1px solid #000000;color: #000000;";
                            
                            window.document.getElementById(sName).style.cssText = "border: 1px solid #000000;color: #000000;";
                            window.document.getElementById(sName2).style.cssText = "border: 1px solid #000000;color: #000000;";
                        }
                    }
                }
            }
        }
        i = i + 1;
    }
    return bOK;
}
