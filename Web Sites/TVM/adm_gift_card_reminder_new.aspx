<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_gift_card_reminder_new.aspx.cs" Inherits="adm_gift_card_reminder_new" %>

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
<script language="JavaScript" src="js/calendar.js" type="text/javascript"></script>
<script language="JavaScript" src="js/AnchorPosition.js" type="text/javascript"></script>
<script language="JavaScript" src="js/dom-drag.js" type="text/javascript"></script>
<script language="JavaScript" src="js/FCKeditor/fckeditor.js" type="text/javascript"></script>
<script type="text/javascript">
    var peek_date_string = '<%=Session["peek_date"]%>';
    var Su = '<%=Session["Su"]%>';
    var Mo = '<%=Session["Mo"]%>';
    var Tu = '<%=Session["Tu"]%>';
    var We = '<%=Session["We"]%>';
    var Th = '<%=Session["Th"]%>';
    var Fr = '<%=Session["Fr"]%>';
    var Sa = '<%=Session["Sa"]%>';
    MonthNumber = '<%=Session["MonthList"]%>';
    function AfterDateSelect(orderBy, pageNum, theDate) {
    }
    function GetPageTable(orderBy, pageNum) {

        RS.Execute("adm_gift_card_reminder_new.aspx", "GetPageContent", orderBy, pageNum, callback_page_content_with_editor, errorCallback);
    }
</script>
</head>
<body class="admin_body" onload="GetPageTable('' , 0);">
<form id="form1" name="form1" action="" method="post" runat=server>
    <div id="tag_collections_div" class="floating_div"></div>
    <div id="calendarDiv" class="floating_div"></div>
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
		<!-- all working area -->
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
						<!-- empty area -->
						<td style="width:10px; white-space:nowrap;" nowrap>&nbsp;</td>
						<!-- main working area -->
						<td style="width:800px;" valign=top nowrap>
							<!-- working area -->
							<table style="border-collapse:collapse;">
								<!-- header -->
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
								<% TVinciShared.PageUtils.GetErrorMsg(1); %>
								<tr>
								    <td id="page_content" width=100% nowrap=nowrap>
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

