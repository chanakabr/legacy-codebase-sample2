<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_video_popup_selector.aspx.cs" Inherits="adm_video_popup_selector" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<HEAD>
		<meta http-equiv="Content-Type" content="text/html; charset=windows-1255"/>
		<title><% TVinciShared.PageUtils.GetTitle(); %></title>
		<meta content="" name="description"/>
		<meta name="robots" content="all"/>
		<meta name="revisit-after" content="1 days"/>
		<meta name="Author" content="Guy Barkan"/>
		<meta content="<% TVinciShared.PageUtils.GetKeyWords(); %>" name="keywords"/>
		<META http-equiv="Pragma" content="no-cache">
		<link href="css/styles-en.css" type="text/css" rel="stylesheet"/>
		<link href="css/addPic-en.css" type="text/css" rel="stylesheet"/>
		<script type="text/javascript" language="JavaScript" src="js/rs.js"></script>
		<script type="text/javascript" language="JavaScript" src="js/adm_utils.js"></script>
		<script type="text/javascript" language="JavaScript" src="js/AnchorPosition.js"></script>
		<script type="text/javascript" language="JavaScript" src="js/dom-drag.js"></script>
		<script type="text/javascript">
		    		    
		    function Send()
		    {
    		    theIDS = window.document.getElementById("ids_place").value;
		        toSend = window.document.getElementById("ids_place").value;
		        if (toSend.lastIndexOf(";") == toSend.length - 1)
		            toSend = theIDS.substring(0 ,theIDS.length -1); 
		        if (window.opener && window.opener.document.getElementsByName("<% GetSendID(); %>")[0])
		        {
		            window.opener.document.getElementsByName("<% GetSendID(); %>")[0].value = toSend;
		            window.opener.ChangeVid("<% GetSendID(); %>" , toSend);
		            window.close();
		        }
		          
		        window.close();
		          
		    }
		    
		    function removePic(theID)
		    {
		        document.getElementById("vid_" + theID).outerHTML = "";
		        theIDS = document.getElementById("ids_place").value;
		        if (theIDS.lastIndexOf(";") != theIDS.length - 1)
		            theIDS = theIDS + ";";
		        theIdToRemove = theID + ";";
		        
		        var temp = new Array();
                temp = theIDS.split(theIdToRemove);
                theIDS = temp[0] + temp[1];
                document.getElementById("ids_place").value = theIDS;
		    }
		    function addPic(thePicID , thePicURL , thePicAlt)
		    {
		        thePicAlt = thePicAlt.replace("~~qoute~~" , "\"").replace("~~apos~~" , "'");
		        //theHtml = "<div id=\"img_" + thePicID + "\">\r\n<div class='image_wrap'>";
		        //theHtml += "<div>\r\n" + thePicAlt + "\r\n</div>\r\n";
                //theHtml += "<img alt='" + thePicAlt + "' src='" + thePicURL + "' />\r\n";
                //theHtml += "<div>\r\n<a class='adm_table_link' href='javascript:removePic(" + thePicID + ");'>הורד</a>\r\n</div>\r\n";
                //theHtml += "</div>\r\n</div>\r\n";
                
                theHtml = "<li id=\"vid_" + thePicID + "\">";
                theHtml += "<h5 title=\"" + thePicAlt + "\">" + thePicAlt + "</h5>";
                theHtml += "<img src=\"" + thePicURL + "\" alt=\"" + thePicAlt + "\" title=\"" + thePicAlt + "\" /><a href=\"javascript:removePic(" + thePicID + ");\" title=\"Remove\">Remove</a></li>";
                theDiv = document.getElementById("selected_place").innerHTML;
                theIDS = document.getElementById("ids_place").value;
                if (theIDS == "0")
                    theIDS = "";
                thePicID = thePicID+";";
                searchPicId = ";" + thePicID;
                var temp = new Array();
                temp = theIDS.split(";");
                
                if(temp.length + 1 > <% GetMaxPics(); %> && theIDS != "")
                {
                    alert("More pics then needed - Action canceled");
                }
                else if (theIDS.indexOf(searchPicId) == -1 && (theIDS.indexOf(thePicID) == -1 || theIDS.indexOf(thePicID) > 0))
                {
                    theIDS = theIDS + thePicID;
                    theDiv = theDiv + theHtml;
                    document.getElementById("ids_place").value = theIDS;
                    document.getElementById("selected_place").innerHTML = theDiv;
                }
                else
                    alert("Media exists");
		    }
            function SearchPics()
            {
                theTags = document.getElementById("pics_tag_coll").value;
                if (theTags != "")
                    RS.Execute("adm_video_popup_selector.aspx", "SearchPics", document.getElementById("pics_tag_coll").value , callback_search_pics, errorCallback);
                else
                    alert("Insert tags");
            }
            function callback_search_pics(result)
            {
                if (result == "")
                    result = "No pics";
                document.getElementById("pics_place").innerHTML = result;
            }
            function callback_selected_pics(result)
            {
                document.getElementById("selected_place").innerHTML = result;
            }
            function StartValues()
            {
                theIDs = document.getElementById("ids_place").value;
                if (theIDs != "")
                    RS.Execute("adm_video_popup_selector.aspx", "GetPics", theIDs , callback_selected_pics, errorCallback);
            }
		</script>
</HEAD>
<body onload="StartValues();">
    <form method="post"  id="form1" runat="server">
        <input type="hidden" id="ids_place" name="ids_place" style="width: 0px; height: 0px; display: none;" value="<% GetIDs(); %>"/>
        <input type="file" runat=server id="file_marker" name="file_marker" style="width: 0px; height: 0px; display: none;" />
        <div id="tag_collections_div" class="floating_div"></div>
        <div class="wrapper">
            <div id="pic_b">
	            <div class="h1">
		            <h1>Media browser</h1>
	            </div>
	            <!--/h1 -->
	            <div class="find"><b class="top"><b></b></b>
		            <!-- -->
		            <label for="seach">Query:
		            <input name='pics_tag_coll' id='pics_tag_coll' name="" type="text" />
                     <%--   onkeyup='return tagKeyPress("tags" , "media_tags" , "VALUE" , "adm_table_header_nbg" , "(c.id in(select distinct tag_id from media_tags where status=1) )" , "pics_tag" , "");'/>--%>
		            </label>
		            <a href="javascript:SearchPics();" class="btn_search"></a>
		            <!-- -->
		            <b class="bot"><b></b></b></div>
		        <!--/picRes -->
		        <div class="picSelected">
		            <h2>Chosen videos:</h2>
		            <ul>
		                <div id="selected_place">
				        </div>
		            </ul>
	            </div>
	            <div class="butLine">
	                <table><tr><td><a href="javascript:Send();" class="btn_confirm" title="Send selection"></a></td><td>&nbsp;</td><td><a class="btn_cancel" href="javascript:window.close();" title="Cancel"></a></td></tr></table>
	            </div>
	            <!--/picSelected -->
	            <!--div class="butLine">&nbsp;<a href="javascript:Send();" class="btn4" title="Send selection">Send selection</a><a class="btn4" href="javascript:window.close();" title="Cancel" class="blueLink"></a></div-->
	            <!--/find -->
	            <div class="picRes">
		            <h2>Search results</h2>
		            <ul>
		                <div id="pics_place">
				        </div>
		            </ul>
	            </div>
	            <!--/butLine -->
	            <br />
	        </div>
        </div>
    </form>
</body>
</html>

