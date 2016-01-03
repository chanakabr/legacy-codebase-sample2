<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_user_purchases_report.aspx.cs" Inherits="adm_user_purchases_report" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title><% TVinciShared.PageUtils.GetTitle(); %></title>
<meta http-equiv="Content-Type" content="text/html; charset=windows-1255" />
<meta content="" name="Description" />
<meta content="all" name="robots" />
<meta content="1 days" name="revisit-after" />
<meta content="Guy Barkan" name="Author" />
<meta content="<% TVinciShared.PageUtils.GetKeyWords(); %>" name="Keywords" />
<meta http-equiv="Pragma" content="no-cache" />
<link href="css/styles-en.css" type="text/css" rel="stylesheet" />
<script language="JavaScript" src="js/rs.js" type="text/javascript"></script>
<script language="JavaScript" src="js/adm_utils.js" type="text/javascript"></script>
<script language="JavaScript" src="js/ajaxFuncs.js" type="text/javascript"></script>
<script type="text/javascript" src="js/SWFObj.js" language="javascript"></script>
<script type="text/javascript" src="js/WMPInterface.js" language="javascript"></script>
<script type="text/javascript" src="js/WMPObject.js" language="javascript"></script>
<script type="text/javascript" src="js/FlashUtils.js" language="javascript"></script>
<script type="text/javascript" src="js/Player.js" language="javascript"></script>
<script type="text/javascript" src="js/VGObject.js" language="javascript"></script>
<script type="text/javascript">
    var isNN = document.layers ? true : false;
    var isIE = document.all ? true : false;
    var mouseX;
    var mouseY;
    function initM() {
      if ( isNN )
        document.captureEvents(Event.MOUSEMOVE)
      document.onmousemove = handleMouseMove;
    }
    function StopSubRenewals(user_id, subCode,  purchase_id)
    {
        sURL = "AjaxSubRenewable.aspx?user_id=" + escape(user_id) + "&sub_code=" + subCode + "&purchase_id=" + purchase_id + "&status=0";
        postFile(sURL , callback_SubRenewals);
    }
    function RenewSubRenewals(user_id, subCode,  purchase_id)
    {
        sURL = "AjaxSubRenewable.aspx?user_id=" + escape(user_id) + "&sub_code=" + subCode + "&purchase_id=" + purchase_id + "&status=1";
        postFile(sURL , callback_SubRenewals);
    }
    function ChangeSubDates(user_id, subCode,  purchase_id , sub_renewable, date_change_type)
    {
        sURL = "AjaxSubDateChange.aspx?user_id=" + escape(user_id) + "&sub_code=" + subCode + "&purchase_id=" + purchase_id + "&change_type=" + date_change_type + "&sub_renewable=" + sub_renewable;
        postFile(sURL , callback_SubRenewals);
    }
    function openStrech(user_id , subCode , purchase_id , sub_renewable)
    {
        theHtml = "<table dir=ltr style=\"background-color: #FFFFFF;\"  cellspacing='1' cellpadding='0' border='1'>";
        theHtml += "<tr><td>";
        theHtml += "<table dir='ltr' border='0' cellpadding='6' cellspacing='0'>";
        theHtml += "<tr class='adm_table_header_nbg' style=\"FONT-WEIGHT: bold;FONT-SIZE: 12px;COLOR: #000000;FONT-FAMILY: Arial, Arial , David , Courier New ;border-color: #aaaaaa; TEXT-DECORATION: none;\">";
        theHtml += "<td class=calendar_table_cell align='right' onclick='closeCollDiv(\"\")'>X</td>";        
        //theHtml += "<td nowrap valign=top>";
        theHtml += "<table>";
        theHtml += "<tr>";
        theHtml += "<td><a href='javascript:ChangeSubDates(" + user_id + "," + subCode + "," + purchase_id + "," + sub_renewable + ",1);'>Final Cancel</a></td>";
        theHtml += "</tr>"; 
        theHtml += "<tr>";
        theHtml += "<td><a href='javascript:ChangeSubDates(" + user_id + "," + subCode + "," + purchase_id + "," + sub_renewable + ",2);'>Add 7 days</a></td>";
        theHtml += "</tr>"; 
        theHtml += "<tr>";
        theHtml += "<td><a href='javascript:ChangeSubDates(" + user_id + "," + subCode + "," + purchase_id + "," + sub_renewable + ",3);'>Add Month</a></td>";
        theHtml += "</tr>"; 
        theHtml += "</table>";
        theHtml += "</td>";
        theHtml += "</tr></table></td></tr></table>";
        oDiv = window.document.getElementById("tag_collections_div");
	    oDiv.style.display = "block";
	    //var mp = getMousePosition(event);
	    oDiv.style.left = (mouseX-100) + 'px';
	    oDiv.style.top = mouseY + 'px';
	    oDiv.innerHTML = theHtml;
    }
    function StrechSub(user_id, subCode,  purchase_id , sub_renewable)
    {
        openStrech(user_id, subCode,  purchase_id , sub_renewable);
    }
    
    
    
    function callback_SubRenewals() {
        if (xmlhttp.readyState == 4) {
            if (xmlhttp.status == 200) 
            {
                result1 = xmlhttp.responseText;
                result = result1.split("~~|~~")[0];
                if (result == "OK") {
                    GetPageTable('' , 0);
                }
                else {
                     alert("Error");
                }
            }
        }
    }
    
    function handleMouseMove(evt) {
      var winX = document.documentElement.scrollLeft;
      var winY = document.documentElement.scrollTop;
      mouseX = isNN ? evt.pageX : window.event.clientX + winX;
      mouseY = isNN ? evt.pageY : window.event.clientY + winY;
      return false;
    }

    
    function GetPageTable(orderBy , pageNum)
    {
        RS.Execute("adm_user_purchases_report.aspx", "GetPageContent", orderBy , pageNum , callback_page_content, errorCallback);
    }
    function create_csv()
    {
        RS.Execute("adm_user_purchases_report.aspx", "GetTableCSV" , callback_create_csv, errorCallback);
    }
</script>
</head>
<body class="admin_body" onload="initM();GetPageTable('' , 0);">
<form id="form1" name="form1" action="" method="post" runat=server>
    <div class="floating_div" id="tag_collections_div"></div>
	<table align=center cellpadding=0 cellspacing=0 class="admContainer">
		<!-- top banner -->
		<tr>
			<td nowrap class="adm_top_banner">
				<table cellpadding=0 cellspacing=0 >
					<tr>
						<td valign="middle"><img src="images/admin-tvinci.gif" alt="" /></td>
						<td width="100%" valign="top" class="align0">
							<table class="adm_logOutTbl">
							    <tr>
							        <td class="logo">
							            <img src="<% TVinciShared.PageUtils.GetAdminLogo(); %>" alt="" />
							        </td>
							        <td style=" padding: 5px 0 0 5px; vertical-align: top;" class="Right">
							            <table>
							                <tr>
							                    <td style="text-align: left; padding-bottom: 5px;">
							                        <span class="small_header">Group:</span>
							                        <span class="small_text"><% TVinciShared.PageUtils.GetGroupName(); %></span> 
							                    </td>
							                </tr>
							                <tr>
							                    <td style=" text-align: left; padding-bottom: 5px;">
							                        <span class="small_header">User: </span>
							                        <span class="small_text"><% TVinciShared.PageUtils.GetLoginName(); %></span> 
							                    </td>
							                </tr>
							                <tr>
							                    <td class="align0" valign="middle"><a href="logout.aspx" class="logout"></a></td>
							                </tr>
							            </table>
							        </td>
							    </tr>
							</table>
						</td>
					</tr>
				</table>
			</td>
		</tr>
		<tr>
			<td>
				<table cellpadding=0 cellspacing=0>
					<tr>
						<!-- right menu -->
						<td class="adm_right_menu">
							<table>
								<tr>
									<td class="adm_main_header"><% TVinciShared.PageUtils.GetCurrentDate(); %></td>
								</tr>
								<% GetMainMenu(); %>
							</table>
						</td>
						<td style="width:10px; white-space:nowrap;" nowrap>&nbsp;</td>
						<td style="width:800px;" valign=top nowrap>
							<table style="border-collapse:collapse;">
								<tr>
									<td class="adm_main_header"><h3><% GetHeader(); %></h3></td>
								</tr>
								<!-- top menu area-->
								<tr>
									<!-- empty area -->
									<td>
										<table>
											<tr>
												<td nowrap="nowrap" class="selectorList"><ul><% GetSubMenu(); %></ul></td>
											</tr>
										</table>
									</td>
								</tr>
								<!-- content -->
								<!--tr>
									<td class="formInputs">
										<div>
											<table>
												<tr>
													<td>
														<table width="100%">
															<tr>
																<td><% TVinciShared.DBTableWebEditor.GetSearchFree("Tag", "search_tag" , "ltr"); %></td>
																<td class="space01">&nbsp;&nbsp;</td>
																<td><% TVinciShared.DBTableWebEditor.GetSearchFree("Free Text", "search_free" , "ltr"); %></td>
																<td nowrap="nowrap" width="100%">&nbsp;</td>
																<td>
																	<a class="btn2" onclick="GetPageTable('',1);" href="javascript:void(0);"></a>
															    </td>
															</tr>
														</table>
													</td>
												</tr>
											</table>
										</div>
									</td>
								</tr-->
								<tr>
									<td id="page_content">
									    <!-- the actual content -->
									</td>
								</tr>
								<tr>
								    <td onclick='window.document.location.href="adm_users_list.aspx?search_save=1";'><a href="#" class="btn_back"></a></td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</td>
		</tr>
	</table>
</form>
<div class="footer">
	<table>
		<tr>
			<td>
				<div class="rights"> Copyright © 2006 Kaltura Ltd. All rights reserved. | &nbsp;<a style="color:#0080ff;" tabindex="2000" href="mailto:info@kaltura.com">Contact Us</a></div></td><td ><img src="images/admin-footerLogo.png" alt="Kaltura" />
			</td>
		</tr>
	</table>
</div>
    <div id="PlayerWindow" class="playerPop" style="width:525px; height:361px; left:120px; top:10%;display:none;">
        <a href="javascript:void(0);" id="player_closr_h" class="close">Close</a>                              
        <div class="inner">
                <div id="PlayerOuterContainer" style="position:absolute;z-index:0;top:0;left:0;width:100%;height:100%;"></div>
                <div class="inner" id="WMPDiv" style="position:absolute;z-index:0;top:0;left:0">
                </div>
        </div>
    </div>
    <div id="ndsdiv" style="position:absolute;z-index:1;top:0;left:0;width:0;height:0;"></div>
</body>
</html>

