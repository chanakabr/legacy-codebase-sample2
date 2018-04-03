<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_coupons_groups.aspx.cs" Inherits="adm_coupons_groups" %>

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
    function GetPageTable(orderBy , pageNum) {
        search_on_off = GetSafeDocumentIDVal("search_on_off");
        search_free = GetSafeDocumentIDVal("search_free");
        RS.Execute("adm_coupons_groups.aspx", "GetPageContent", orderBy, pageNum, search_free, search_on_off ,callback_page_content, errorCallback);
    }
    function create_csv()
    {    
        RS.Execute("adm_coupons_groups.aspx", "GetTableCSV", callback_create_csv, errorCallback);
    }

    var isNN = document.layers ? true : false;
    var isIE = document.all ? true : false;
    var mouseX = 0;
    var mouseY = 0;
    var openmouseX = 0;
    var openmouseY = 0;
    function initM() {
        if (isNN)
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

    function CouponsGenerator(coupon_group_id) {
        openCouponsGenerator(coupon_group_id);
    }

    function openCouponsGenerator(coupon_group_id) {
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
        theHtml += "<td><a href='javascript:CouponGenerator(" + coupon_group_id + ");'>Coupon generator</a></td>";
        theHtml += "</tr>";
        theHtml += "<tr>";
        theHtml += "<td><a href='javascript:CouponNameGenerator(" + coupon_group_id + ");'>Coupon name generator</a></td>";
        theHtml += "</tr>";
        theHtml += "</table></td></tr></table>";
        oDiv = window.document.getElementById("tag_collections_div");
        oDiv.style.display = "block";
        oDiv.style.left = (openmouseX - 100) + 'px';
        oDiv.style.top = openmouseY + 'px';
        oDiv.innerHTML = theHtml;
    }

    function CouponGenerator(coupon_group_id) {
        theHtml = "<table dir=ltr style=\"background-color: #FFFFFF;\"  cellspacing='1' cellpadding='0' border='1'>";
        theHtml += "<tr><td>";
        theHtml += "<table dir='ltr' border='0' cellpadding='6' cellspacing='0'>";
        theHtml += "<tr class='adm_table_header_nbg' style=\"FONT-WEIGHT: bold;FONT-SIZE: 12px;COLOR: #000000;FONT-FAMILY: Arial, Arial , David , Courier New ;border-color: #aaaaaa; TEXT-DECORATION: none;\">";
        theHtml += "<td class=calendar_table_cell align='left'  style='width: 100%;cursor: pointer;' onclick='openmouseX = 0;openmouseY = 0;closeCollDiv(\"\")'>X</td>";
        theHtml += "<td class=calendar_table_cell align='right'  style='cursor: pointer;' onclick='openCouponsGenerator(\"\")'>back</td>";
        theHtml += "<table>";
        theHtml += "<tr>";
        theHtml += "<td class='adm_table_header_nbg' nowrap>Number of coupons to generate</td>";
        theHtml += "<td><input id='number_of_coupons' class='FormInput' name='number_of_coupons' type='text' dir='ltr' size=10 /></td>";
        theHtml += "</tr>";
        theHtml += "<tr>";
        theHtml += "<tr>";
        theHtml += "<td class='adm_table_header_nbg' nowrap>Use special characters</td>";
        theHtml += "<td><input type='checkbox' class='FormInput' id='use_special_characters' name='use_special_characters' /></td>";
        theHtml += "</tr>";
        theHtml += "<tr>";
        theHtml += "<td class='adm_table_header_nbg' nowrap>Use numbers</td>";
        theHtml += "<td><input type='checkbox' class='FormInput' id='use_numbers' name='use_numbers' /></td>";
        theHtml += "</tr>";
        theHtml += "<tr>";
        theHtml += "<td class='adm_table_header_nbg' nowrap>Use letters</td>";
        theHtml += "<td><input type='checkbox' class='FormInput' id='use_letters' name='use_letters' /></td>";
        theHtml += "</tr>";
        theHtml += "<td id='confirm_btn'><a href='javascript:ManipCouponGenerator(" + coupon_group_id + ",document.getElementById(\"number_of_coupons\").value,document.getElementById(\"use_special_characters\").checked,document.getElementById(\"use_numbers\").checked,document.getElementById(\"use_letters\").checked);' class='btn'></a></td>";
        theHtml += "</tr>";
        theHtml += "</table></td></tr></table>";
        oDiv = window.document.getElementById("tag_collections_div");
        oDiv.style.display = "block";
        oDiv.style.left = (openmouseX - 100) + 'px';
        oDiv.style.top = openmouseY + 'px';
        oDiv.innerHTML = theHtml;
    }

    function ManipCouponGenerator(coupon_group_id, number_of_coupons, use_special_characters, use_numbers, use_letters) {
        sURL = "AjaxManipCouponGenerator.aspx?type=CouponGenerator&coupon_group_id=" + escape(coupon_group_id)
            + "&number_of_coupons=" + escape(number_of_coupons)
            + "&use_special_characters=" + escape(use_special_characters)
            + "&use_numbers=" + escape(use_numbers)
            + "&use_letters=" + escape(use_letters);

        postFile(sURL, callback_ManipCouponGenerator);
    }

    function callback_ManipCouponGenerator() {
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
            openmouseX = 0; openmouseY = 0; closeCollDiv('');
        }
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
																<td><% TVinciShared.DBTableWebEditor.GetSearchFree("Code", "search_free", "ltr"); %></td>
																<td class="space01">&nbsp;&nbsp;</td>
																<td><% TVinciShared.DBTableWebEditor.GetSearchSelectOptions("On/Off" , "search_on_off" , "lu_on_off" , "id" , "DESCRIPTION" , "ID" , "---" , "-1" , true , ""); %></td>
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