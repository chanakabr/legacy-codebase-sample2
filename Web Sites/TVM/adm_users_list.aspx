<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_users_list.aspx.cs" Inherits="adm_users_list" %>

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
<script type="text/javascript">
    var isNN = document.layers ? true : false;
    var isIE = document.all ? true : false;
    var mouseX = 0;
    var mouseY = 0;
    var openmouseX = 0;
    var openmouseY = 0;
    function initM() {
      if ( isNN )
        document.captureEvents(Event.MOUSEMOVE)
      document.onmousemove = handleMouseMove;
    }
    function handleMouseMove(evt) {
      var winX = document.documentElement.scrollLeft;
      var winY = document.documentElement.scrollTop;
      mouseX = isNN ? evt.pageX : window.event.clientX + winX;
      mouseY = isNN ? evt.pageY : window.event.clientY + winY;
      return false;
    }
    
    function Gift(user_id)
    {
        openGift(user_id);
    }
    
    
    function FreePPV(user_id)
    {
        theHtml = "<table dir=ltr style=\"background-color: #FFFFFF;\"  cellspacing='1' cellpadding='0' border='1'>";
        theHtml += "<tr><td>";
        theHtml += "<table dir='ltr' border='0' cellpadding='6' cellspacing='0'>";
        theHtml += "<tr class='adm_table_header_nbg' style=\"FONT-WEIGHT: bold;FONT-SIZE: 12px;COLOR: #000000;FONT-FAMILY: Arial, Arial , David , Courier New ;border-color: #aaaaaa; TEXT-DECORATION: none;\">";
        theHtml += "<td class=calendar_table_cell align='left'  style='width: 100%;cursor: pointer;' onclick='openmouseX = 0;openmouseY = 0;closeCollDiv(\"\")'>X</td>";        
        theHtml += "<td class=calendar_table_cell align='right'  style='cursor: pointer;' onclick='openGift(\"\")'>back</td>";        
        theHtml += "<table>";
        theHtml += "<tr>";
        theHtml += "<td class='adm_table_header_nbg' nowrap>Media_file_id</td>";
        theHtml += "<td><input id='media_file_id' class='FormInput' name='media_file_id' type='text' dir='ltr' size=10 /></td>";
        theHtml += "</tr>"; 
        theHtml += "<tr>";
        theHtml += "<td class='adm_table_header_nbg' nowrap>Remarks</td>";
        theHtml += "<td><textarea type='html' class='FormInput' id='action_desc' name='action_desc' dir='ltr' cols=20 rows=6></textarea></td>";
        theHtml += "</tr>"; 
        theHtml += "<tr>";
        theHtml += "<td id='confirm_btn'><a href='javascript:ManipPPV(" + user_id + ",document.getElementById(\"media_file_id\").value,document.getElementById(\"action_desc\").value);' class='btn'></a></td>";
        theHtml += "</tr>"; 
        theHtml += "</table></td></tr></table>";
        oDiv = window.document.getElementById("tag_collections_div");
	    oDiv.style.display = "block";
	    oDiv.style.left = (openmouseX-100) + 'px';
	    oDiv.style.top = openmouseY + 'px';
	    oDiv.innerHTML = theHtml;
    }
    
    function Password(user_id)
    {
        theHtml = "<table dir=ltr style=\"background-color: #FFFFFF;\"  cellspacing='1' cellpadding='0' border='1'>";
        theHtml += "<tr><td>";
        theHtml += "<table dir='ltr' border='0' cellpadding='6' cellspacing='0'>";
        theHtml += "<tr class='adm_table_header_nbg' style=\"FONT-WEIGHT: bold;FONT-SIZE: 12px;COLOR: #000000;FONT-FAMILY: Arial, Arial , David , Courier New ;border-color: #aaaaaa; TEXT-DECORATION: none;\">";
        theHtml += "<td class=calendar_table_cell align='left'  style='width: 100%;cursor: pointer;' onclick='openmouseX = 0;openmouseY = 0;closeCollDiv(\"\")'>X</td>";        
        theHtml += "<td class=calendar_table_cell align='right'></td>";        
        theHtml += "<table>";
        theHtml += "<tr>";
        theHtml += "<td class='adm_table_header_nbg' nowrap>New password</td>";
        theHtml += "<td><input id='new_pass' class='FormInput' name='new_pass' type='text' dir='ltr' size=10 /></td>";
        theHtml += "</tr>"; 
        theHtml += "<tr>";
        theHtml += "<td class='adm_table_header_nbg' nowrap>New password</td>";
        theHtml += "<td><input id='new_pass2' class='FormInput' name='new_pass2' type='text' dir='ltr' size=10 /></td>";
        theHtml += "</tr>"; 
        theHtml += "<tr>";
        theHtml += "<td id='confirm_btn'><a href='javascript:ManipPassword(" + user_id + ",document.getElementById(\"new_pass\").value,document.getElementById(\"new_pass2\").value);' class='btn'></a></td>";
        theHtml += "</tr>"; 
        theHtml += "</table></td></tr></table>";
        oDiv = window.document.getElementById("tag_collections_div");
	    oDiv.style.display = "block";
	    oDiv.style.left = (mouseX-100) + 'px';
	    oDiv.style.top = mouseY + 'px';
	    oDiv.innerHTML = theHtml;
    }

    function Engagements(user_id, coupon_groups, engagement_type) {
      
        theHtml = "<table dir=ltr style=\"background-color: #FFFFFF;\"  cellspacing='1' cellpadding='0' border='1'>";
        theHtml += "<tr><td>";
        theHtml += "<table dir='ltr' border='0' cellpadding='6' cellspacing='0'>";
        theHtml += "<tr class='adm_table_header_nbg' style=\"FONT-WEIGHT: bold;FONT-SIZE: 12px;COLOR: #000000;FONT-FAMILY: Arial, Arial , David , Courier New ;border-color: #aaaaaa; TEXT-DECORATION: none;\">";
        theHtml += "<td class=calendar_table_cell align='left'  style='width: 100%;cursor: pointer;' onclick='openmouseX = 0;openmouseY = 0;closeCollDiv(\"\")'>X</td>";
        theHtml += "<td class=calendar_table_cell align='right'></td>";
        theHtml += "<table>";
        theHtml += "<tr>";

        theHtml += '<select id="CouponGroup">  <option value=-999>Coupon Group</option>';
        for (var i = 0; i < coupon_groups.length; i++) {
            var obj = coupon_groups[i];
            theHtml += '<option value=\"' + obj["Key"] + "\">" +  obj["Value"] + '</option>';
            }
        theHtml += '</select><br><br>';

        theHtml += '<select id="EngagementType" > <option value=-999>Engagement Type</option>';
        for (var i = 0; i < engagement_type.length; i++) {
            var obj = engagement_type[i];
            theHtml += '<option value=\"' + obj["Key"] + "\">" + obj["Value"] + '</option>';
        }

        theHtml += '</select><br><br>';
        theHtml += "<tr>";
        theHtml += "<td id='confirm_btn'><a href='javascript:ManipEngagements(" + user_id + ",document.getElementById(\"CouponGroup\"),document.getElementById(\"EngagementType\"));' class='btn'></a></td>";
        theHtml += "</tr>";
        theHtml += "</table></td></tr></table>";
        oDiv = window.document.getElementById("tag_collections_div");
        oDiv.style.display = "block";
        oDiv.style.left = (mouseX - 100) + 'px';
        oDiv.style.top = mouseY + 'px';
        oDiv.innerHTML = theHtml;
    }   

    function ManipEngagements(user_id, coupon_group, engagement_type) {
      
        var couponGroupVal = coupon_group.options[coupon_group.selectedIndex].value;

        var engagementTypeVal = engagement_type.options[engagement_type.selectedIndex].value;

        if (couponGroupVal == -999 || engagementTypeVal == -999) {
            alert("coupon group or engagement type are mandatory fields");
        }
        else {
            sURL = "AjaxManipEngagements.aspx?user_id=" + escape(user_id) + "&coupon_group=" + escape(couponGroupVal) + "&engagement_type=" + escape(engagementTypeVal);
            postFile(sURL, callback_ManipEngagements);
        }
    }

    
    function ManipFreeSub(user_id,sub_id,remarks)
    {
        sURL = "AjaxManipGift.aspx?type=sub&user_id=" + escape(user_id) + "&gift_code=" + escape(sub_id) + "&remarks=" + escape(remarks);
        postFile(sURL , callback_ManipGift);
    }
    
    function ManipPPV(user_id,media_file_id,remarks)
    {
        sURL = "AjaxManipGift.aspx?type=ppv&user_id=" + escape(user_id) + "&gift_code=" + escape(media_file_id) + "&remarks=" + escape(remarks);
        postFile(sURL , callback_ManipGift);
    }
    
    function ManipPassword(user_id,new_pass1,new_pass2)
    {
        if (new_pass1 != new_pass2 || new_pass1 == "")
            alert("Pass1 are Pass2 are not equal");
        else
        {
            sURL = "AjaxManipPassword.aspx?user_id=" + escape(user_id) + "&pass=" + escape(new_pass1);
            postFile(sURL , callback_ManipPassword);
        }
    }

    function callback_ManipPassword() {
        if (xmlhttp.readyState == 4) {
            if (xmlhttp.status == 200) {

                result1 = xmlhttp.responseText;
                result = result1.split("~~|~~")[0];
                try
                {
                    result2 = result1.split("~~|~~")[1];
                }
                catch (e) {
                    result2="???";
		        }
                if (result == "OK") {
                    alert("Done...");
                }
                else {
                    alert(result2);
                }
                
            }
            else
                alert("Error!");
            closeCollDiv('');
        }
    }
    
    function callback_ManipGift() {
        if (xmlhttp.readyState == 4) {
            if (xmlhttp.status == 200) {

                result1 = xmlhttp.responseText;
                result = result1.split("~~|~~")[0];
                try
                {
                    result2 = result1.split("~~|~~")[1];
                }
                catch (e) {
                    result2="???";
		        }
                if (result == "OK") {
                    alert("Done...");
                }
                else {
                    alert(result2);
                }
                
            }
            openmouseX = 0;openmouseY = 0;closeCollDiv('');
        }
    }

    function callback_ManipEngagements() {
        if (xmlhttp.readyState == 4) {
            if (xmlhttp.status == 200) {

                result1 = xmlhttp.responseText;
                result = result1.split("~~|~~")[0];
                try {
                    result2 = result1.split("~~|~~")[1];
                }
                catch (e) {
                    result2 = "???";
                }
                if (result == "OK") {
                    alert("Done...");
                }
                else {
                    alert(result2);
                }

            }
            else
                alert("Error!");
            closeCollDiv('');
        }
    }
    
    function FreeSub(user_id)
    {
        theHtml = "<table dir=ltr style=\"background-color: #FFFFFF;\"  cellspacing='1' cellpadding='0' border='1'>";
        theHtml += "<tr><td>";
        theHtml += "<table dir='ltr' border='0' cellpadding='6' cellspacing='0'>";
        theHtml += "<tr class='adm_table_header_nbg' style=\"FONT-WEIGHT: bold;FONT-SIZE: 12px;COLOR: #000000;FONT-FAMILY: Arial, Arial , David , Courier New ;border-color: #aaaaaa; TEXT-DECORATION: none;\">";
        theHtml += "<td class=calendar_table_cell align='left'  style='width: 100%;cursor: pointer;' onclick='openmouseX = 0;openmouseY = 0;closeCollDiv(\"\")'>X</td>";        
        theHtml += "<td class=calendar_table_cell align='right'  style='cursor: pointer;' onclick='openGift(\"\")'>back</td>";        
        theHtml += "<table>";
        theHtml += "<tr>";
        theHtml += "<td class='adm_table_header_nbg' nowrap>Subscription Code</td>";
        theHtml += "<td><input id='sub_code' class='FormInput' name='sub_code' type='text' dir='ltr' size=10 /></td>";
        theHtml += "</tr>"; 
        theHtml += "<tr>";
        theHtml += "<tr>";
        theHtml += "<td class='adm_table_header_nbg' nowrap>Remarks</td>";
        theHtml += "<td><textarea type='html' class='FormInput' id='action_desc' name='action_desc' dir='ltr' cols=20 rows=6></textarea></td>";
        theHtml += "</tr>"; 
        theHtml += "<td id='confirm_btn'><a href='javascript:ManipFreeSub(" + user_id + ",document.getElementById(\"sub_code\").value,document.getElementById(\"action_desc\").value);' class='btn'></a></td>";
        theHtml += "</tr>"; 
        theHtml += "</table></td></tr></table>";
        oDiv = window.document.getElementById("tag_collections_div");
	    oDiv.style.display = "block";
	    oDiv.style.left = (openmouseX-100) + 'px';
	    oDiv.style.top = openmouseY + 'px';
	    oDiv.innerHTML = theHtml;
    }
    
    function openGift(user_id)
    {
        if (openmouseX == 0)
            openmouseX = mouseX;
        if (openmouseY == 0)
            openmouseY = mouseY;
        openmouseY = mouseY;
        theHtml = "<table dir=ltr style=\"background-color: #FFFFFF;\"  cellspacing='1' cellpadding='0' border='1'>";
        theHtml += "<tr><td>";
        theHtml += "<table dir='ltr' border='0' cellpadding='6' cellspacing='0'>";
        theHtml += "<tr class='adm_table_header_nbg' style=\"FONT-WEIGHT: bold;FONT-SIZE: 12px;COLOR: #000000;FONT-FAMILY: Arial, Arial , David , Courier New ;border-color: #aaaaaa; TEXT-DECORATION: none;\">";
        theHtml += "<td class=calendar_table_cell align='right'  style='cursor: pointer;' onclick='openmouseX = 0;openmouseY = 0;closeCollDiv(\"\")'>X</td>";        
        theHtml += "<table>";
        theHtml += "<tr>";
        theHtml += "<td><a href='javascript:FreeSub(" + user_id + ");'>Grant Free Subscription</a></td>";
        theHtml += "</tr>"; 
        theHtml += "<tr>";
        theHtml += "<td><a href='javascript:FreePPV(" + user_id + ");'>Grant Free PPV</a></td>";
        theHtml += "</tr>"; 
        theHtml += "</table></td></tr></table>";
        oDiv = window.document.getElementById("tag_collections_div");
	    oDiv.style.display = "block";
	    oDiv.style.left = (openmouseX-100) + 'px';
	    oDiv.style.top = openmouseY + 'px';
	    oDiv.innerHTML = theHtml;
    }
    
    function GetPageTable(orderBy , pageNum) {
        search_free = GetSafeDocumentIDVal("search_free");
      
        search_only_unapproved_comments = GetSafeDocumentIDVal("search_only_unapproved_comments");
        search_only_paid_users = "-1";
        RS.Execute("adm_users_list.aspx", "GetPageContent", orderBy, pageNum, search_free,  search_only_paid_users, search_only_unapproved_comments, callback_page_content, errorCallback);
    }
    function create_csv()
    {
        search_free = GetSafeDocumentIDVal("search_free");
        window.open("adm_users_list_excel.aspx?search_free=" + search_free, "_blank", "toolbar=yes, scrollbars=yes, resizable=yes, top=500, left=500, width=550, height=400");
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
								<tr>
									<td class="formInputs">
										<div>
											<table>
												<tr>
													<td>
														<table width="100%">
															<tr>
																<td><% TVinciShared.DBTableWebEditor.GetSearchFree("Free", "search_free", "ltr"); %></td>
																<td class="space01">&nbsp;&nbsp;</td>

																<td><% TVinciShared.DBTableWebEditor.GetSearchSelectOptions("Open Tickets", "search_only_unapproved_comments", "lu_only_paid", "id", "DESCRIPTION", "ID", "---", "-1", true, ""); %></td>
																
																<td>
																	<a class="btn2" onclick="GetPageTable('',1);" href="javascript:void(0);"></a>
															    </td>
															    <td nowrap="nowrap" width="100%">&nbsp;</td>
															</tr>
														</table>
													</td>
												</tr>
											</table>
										</div>
									</td>
								</tr>
								<tr>
									<td id="page_content">
									    <!-- the actual content -->
									</td>
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
</body>
</html>