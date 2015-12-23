<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_statistics_wpd.aspx.cs" Inherits="adm_statistics_wpd" %>

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
<script type="text/javascript">
    function GetPageTable(orderBy , pageNum)
    {
        RS.Execute("adm_statistics_wpd.aspx", "GetPageContent", orderBy , pageNum , callback_page_content, errorCallback);
    }

    function GetPageTable(orderBy, pageNum, startD,startM,startY,endD,endM,endY) {
        RS.Execute("adm_statistics_wpd.aspx", "GetPageContent", orderBy, pageNum, startD, startM, startY, endD, endM, endY, callback_page_content, errorCallback);
    }
    function create_csv()
    {
        RS.Execute("adm_statistics_wpd.aspx", "GetTableCSV" , callback_create_csv, errorCallback);
    }
    function reloadPage() 
    {
        try {
            startD = Number(Trim(document.getElementById("s_day").value));
            startM = Number(Trim(document.getElementById("s_mounth").value));
            startY = Number(Trim(document.getElementById("s_year").value));
            endD = Number(Trim(document.getElementById("s_day_to").value));
            endM = Number(Trim(document.getElementById("s_mounth_to").value));
            endY = Number(Trim(document.getElementById("s_year_to").value));

            if (IsNumeric(startD) == false || startD < 0 || startD > 31)
                return putError("Problematic date");
            else if (IsNumeric(startM) == false || startM < 1 || startM > 12)
                return putError("Problematic date1");
            else if (IsNumeric(startY) == false || startY < 2006)
                return putError("Problematic date2");
            else if (IsNumeric(endD) == false || endD < 0 || endD > 31)
                return putError("Problematic date3");
            else if (IsNumeric(endM) == false || endM < 1 || endM > 12)
                return putError("Problematic date4");
            else if (IsNumeric(endY) == false || endY < 2006)
                return putError("Problematic date5");

            strStart = startD + "/" + startM + "/" + startY;
            strEnd = endD + "/" + endM + "/" + endY;

            GetPageTable('', 0, startD, startM, startY, endD, endM, endY);
        }
        catch (theEx) {
            putError("Problematic date");
        }
    }
</script>
</head>
<body class="admin_body" onload="reloadPage();">
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
									<td>
									    <table width=100%>
									        <% GetSearchPannel(); %>
									        <tr>
									            <td id="page_content" colspan="2">
									                
									            </td>
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
	</table>
</form>
<div class="footer">
	<table>
		<tr>
			<td>
				<div class="rights"> Copyright © 2006 Kaltura Ltd. All rights reserved. | +972 3 609 8070  | &nbsp;<a style="color:#0080ff;" tabindex="2000" href="mailto:info@tvinci.com">Contact Us</a></div></td><td ><img src="images/admin-footerLogo.png" alt="TVINCI" />
			</td>
		</tr>
	</table>
</div>
</body>
</html>