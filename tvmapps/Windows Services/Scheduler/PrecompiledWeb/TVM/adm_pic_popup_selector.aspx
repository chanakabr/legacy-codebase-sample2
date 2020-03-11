<%@ page language="C#" autoeventwireup="true" inherits="adm_pic_popup_selector, App_Web__-8binqh" %>

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
		<script type="text/javascript" language="JavaScript" src="js/utils.js"></script>
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
		            window.opener.ChangePic("<% GetSendID(); %>" , toSend);
		            window.close();
		        }
		          
		        window.close();
		          
		    }
		    
		    function removePic(theID)
		    {
		        window.document.getElementById("out_img_" + theID).innerHTML = "";
		        theIDS = window.document.getElementById("ids_place").value;
		        if (theIDS.lastIndexOf(";") != theIDS.length - 1)
		            theIDS = theIDS + ";";
		        theIdToRemove = theID + ";";
		        
		        var temp = new Array();
                temp = theIDS.split(theIdToRemove);
                theIDS = temp[0] + temp[1];
                window.document.getElementById("ids_place").value = theIDS;
		    }
		    function addPic(thePicID , thePicURL , thePicAlt)
		    {
		        thePicAlt = thePicAlt.replace("~~qoute~~" , "\"").replace("~~apos~~" , "'");
		        //theHtml = "<div id=\"img_" + thePicID + "\">\r\n<div class='image_wrap'>";
		        //theHtml += "<div>\r\n" + thePicAlt + "\r\n</div>\r\n";
                //theHtml += "<img alt='" + thePicAlt + "' src='" + thePicURL + "' />\r\n";
                //theHtml += "<div>\r\n<a class='adm_table_link' href='javascript:removePic(" + thePicID + ");'>הורד</a>\r\n</div>\r\n";
                //theHtml += "</div>\r\n</div>\r\n";
                
                theHtml = "<div id=\"out_img_" + thePicID + "\"><li id=\"img_" + thePicID + "\">";
                theHtml += "<h5 title=\"" + thePicAlt + "\">" + thePicAlt + "</h5>";
                theHtml += "<img src=\"" + thePicURL + "\" alt=\"" + thePicAlt + "\" title=\"" + thePicAlt + "\" /><a href=\"javascript:removePic(" + thePicID + ");\" title=\"Remove\">Remove</a></li></div>";
                theDiv = window.document.getElementById("selected_place").innerHTML;
                theIDS = window.document.getElementById("ids_place").value;
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
                    window.document.getElementById("ids_place").value = theIDS;
                    window.document.getElementById("selected_place").innerHTML = theDiv;
                }
                else
                    alert("Pic exists");
		    }
		    function GetPageTable(orderBy , pageNum)
            {
                RS.Execute("adm_pic_popup_selector.aspx", "GetPageContent", orderBy , pageNum , callback_page_content, errorCallback);
            }
            function SearchPics()
            {
                theTags = window.document.getElementById("pics_tag_coll").value;
                if (theTags != "")
                    RS.Execute("adm_pic_popup_selector.aspx", "SearchPics", window.document.getElementById("pics_tag_coll").value , callback_search_pics, errorCallback);
                else
                    alert("Insert tags");
            }
            function callback_search_pics(result)
            {
                if (result == "")
                    result = "No pics";
                window.document.getElementById("pics_place").innerHTML = result;
            }
            function callback_selected_pics(result)
            {
                window.document.getElementById("selected_place").innerHTML = result;
            }
            function StartValues()
            {
                theIDs = window.document.getElementById("ids_place").value;
                if (theIDs != "")
                    RS.Execute("adm_pic_popup_selector.aspx", "GetPics", theIDs , callback_selected_pics, errorCallback);
                <% GetNewIDFunc(); %>
            }
		</script>
</HEAD>
<body onload="GetPageTable('' , 0);StartValues();">
    <form method="post"  id="form1" runat="server">
        <input type="hidden" id="ids_place" name="ids_place" style="width: 0px; height: 0px; display: none;" value="<% GetIDs(); %>"/>
        <input type="file" runat=server id="file_marker" name="file_marker" style="width: 0px; height: 0px; display: none;" />
        <div id="tag_collections_div" class="floating_div"></div>
        <div class="wrapper">
            <div id="pic_b">
	            <div class="h1"><a href="javascript:OpenID('upload_p');CloseID('pic_b');">Pics uploader</a>
		            <h1>Pics browser</h1>
	            </div>
	            <!--/h1 -->
	            <div class="find"><b class="top"><b></b></b>
		            <!-- -->
		            <label for="seach">Tags:
		            <input name='pics_tag_coll' id='pics_tag_coll' name="" type="text" onkeyup='return tagKeyPress("tags" , "pics_tag" , "VALUE" , "adm_table_header_nbg" , "tag_type_id=0" , "pics_tag");'/>
		            </label>
		            <a href="javascript:SearchPics();" class="btn">Search</a>
		            <!-- -->
		            <b class="bot"><b></b></b></div>
		        <!--/picRes -->
	            <div class="picSelected">
		            <h2>Chosen pics:</h2>
		            <ul>
		                <div id="selected_place">
				        </div>
		            </ul>
	            </div>
	            <!--/picSelected -->
	            <div class="butLine">
	                <table><tr><td><a href="javascript:Send();" class="btn" title="Send selection">Send selection</a></td><td><a class="btn" href="javascript:window.close();" title="Cancel">Cancel</a></td></tr></table>
	            </div>
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
	        <div id="upload_p" style="display:none;">
	            <div class="h1"><a href="javascript:CloseID('upload_p');OpenID('pic_b');">Pics search</a>
		            <h1>Pics uploader</h1>
	            </div>
	            <div id="page_content"></div>
	            <!--/butLine -->
	        </div>
        </div>
    </form>
    
</body>
</html>

