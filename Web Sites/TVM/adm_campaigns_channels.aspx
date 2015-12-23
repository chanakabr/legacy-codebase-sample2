<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_campaigns_channels.aspx.cs" Inherits="adm_campaigns_channels" %>

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
    var flashObj1 = new SWFObj
    (
        'codebase', 'http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,0,0',
        'width', '100%',
        'height', '100%',
        'src', 'flash/DualList',
        'scale', 'NoScale',
        'id', 'DualList',
        'bgcolor', '#869CA7',
        'wmode', 'Window',
        'name', 'DualList',
        'allowFullScreen', 'false',
        'allowScriptAccess', 'sameDomain',
        'movie', 'flash/DualList'
    ); //end AC code
    function flashEvents(json) {
        switch (json.eventType) {
            case "move":
                //alert(json.id + "," + json.kind);
                RS.Execute("adm_campaigns_channels.aspx", "changeItemStatus", json.id, json.kindc, callback_changeItemStatus, errorCallback);
                break;
            case "ready":
                var flashObj1 = document.getElementById(json.id);
                initDualObj();
                break;
        }
    }

    function callback_changeItemStatus(ret) {
    }

    function callback_init_dobj(ret) {
        //alert(ret);
        debugger;
        var flashObj1 = document.getElementById("DualList");
        var split_array = ret.split("~~|~~");
        if (split_array.length == 3) {
            theTitle1 = split_array[0];
            theTitle2 = split_array[1];
            var xmlStr = split_array[2];
            flashObj1.callFlashAction({ action: "setList", data: xmlStr, title1: theTitle1, title2: theTitle2 });
        }
    }

    function initDualObj() {
        RS.Execute("adm_campaigns_channels.aspx", "initDualObj", callback_init_dobj, errorCallback);
    }
    function initPage() {
        flashObj1.write("DualListPH");
    }
</script>
</head>
<body class="admin_body" onload="initPage();">
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
									    <div id="DualListPH"></div>
									</td>
								</tr>
								<tr>
								    <td onclick='window.document.location.href="adm_campaigns.aspx?search_save=1";'><a href="#" class="btn_back"></a></td>
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
