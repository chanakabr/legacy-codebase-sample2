<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_alcr_ppvs_and_file_types.aspx.cs" Inherits="adm_alcr_ppvs_and_file_types" %>

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
<link href="components/duallist/css/duallist.css" type="text/css" rel="stylesheet" />
<script language="JavaScript" src="js/jquery-1.10.2.min.js" type="text/javascript"></script>
<script language="JavaScript" src="js/jquery-placeholder.js" type="text/javascript"></script>
<script src="https://code.jquery.com/ui/1.10.3/jquery-ui.js"></script>
<script language="JavaScript" src="js/rs.js" type="text/javascript"></script>
<script language="JavaScript" src="js/adm_utils.js" type="text/javascript"></script>
<script language="JavaScript" src="js/ajaxFuncs.js" type="text/javascript"></script>
<script type="text/javascript" src="js/SWFObj.js" language="javascript"></script>
<script type="text/javascript" src="js/WMPInterface.js" language="javascript"></script>
<script type="text/javascript" src="js/WMPObject.js" language="javascript"></script>
<script type="text/javascript" src="js/FlashUtils.js" language="javascript"></script>
<script type="text/javascript" src="js/Player.js" language="javascript"></script>
<script type="text/javascript" src="js/VGObject.js" language="javascript"></script>
<script language="JavaScript" src="js/calendar.js" type="text/javascript"></script>
<script language="JavaScript" src="js/AnchorPosition.js" type="text/javascript"></script>
<script language="JavaScript" src="js/dom-drag.js" type="text/javascript"></script>
<script language="JavaScript" src="js/FCKeditor/fckeditor.js" type="text/javascript"></script>
<!-- dual list -->
<script type="text/javascript" src="components/duallist/js/script.js"></script>
<script type="text/javascript" src="components/duallist/js/info.js"></script>
<script type="text/javascript" src="components/duallist/js/calender.js"></script>
<script type="text/javascript" src="components/duallist/js/list.js"></script>
<script type="text/javascript" src="components/duallist/js/duallist.js"></script>
<!-- end dual list -->

<script type="text/javascript">
    function ApplyChanges() {
        submitASPFormWithCheck("adm_alcr_ppvs_and_file_types.aspx?submited=1");
    }
</script>

</head>
<body class="admin_body" onload="initDuallistObj('adm_alcr_ppvs_and_file_types.aspx')">
    <form id="form1" name="form1" action="" method="post" runat="server">
        <div class="floating_div" id="tag_collections_div"></div>
        <table align="center" cellpadding="0" cellspacing="0" class="admContainer">
            <!-- top banner -->
            <tr>
                <td nowrap class="adm_top_banner">
                    <table cellpadding="0" cellspacing="0">
                        <tr>
                            <td valign="top">
                                <img src="images/admin-tvinci.gif" alt="" /></td>
                            <td width="100%" valign="top" class="align0">
                                <table class="adm_logOutTbl">
                                    <tr>
                                        <td class="logo">
                                            <img src="<% TVinciShared.PageUtils.GetAdminLogo(); %>" alt="" />
                                        </td>
                                        <td style="padding: 5px 0 0 5px; vertical-align: top;" class="Right">
                                            <table>
                                                <tr>
                                                    <td style="text-align: left; padding-bottom: 5px;">
                                                        <span class="small_header">Group:</span>
                                                        <span class="small_text"><% TVinciShared.PageUtils.GetGroupName(); %></span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="text-align: left; padding-bottom: 5px;">
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
                    <table cellpadding="0" cellspacing="0">
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
                            <td style="width: 10px; white-space: nowrap;" nowrap>&nbsp;</td>
                            <td style="width: 800px;" valign="top" nowrap>
                                <table style="border-collapse: collapse;">
                                    <tr>
                                        <td class="adm_main_header">
                                            <h3><% GetHeader(); %></h3>
                                        </td>
                                    </tr>
                                    <!-- top menu area-->
                                    <tr>
                                        <!-- empty area -->
                                        <td>
                                            <table>
                                                <tr>
                                                    <td nowrap="nowrap" class="selectorList">
                                                        <ul><% GetSubMenu(); %></ul>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <!-- content -->
                                    <tr><td><div id="DualListppvsToAdd"></div></td></tr>
                                    <tr><td><div id="DualListfileTypesToAdd"></div></td></tr>
                                    <tr><td><div id="DualListppvsToRemove"></div></td></tr>
                                    <tr><td><div id="DualListfileTypesToRemove"></div></td></tr>
                                    <tr style="padding-top:30px"><td id="confirm_btn" onclick='ApplyChanges();'><a tabindex="2000" href="#confirm_btn" class="btn"></a></td></tr>
                                    <tr>
                                        <td onclick='window.document.location.href="adm_asset_life_cycle_rules.aspx'><a href="#" class="btn_back"></a></td>
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
                    <div class="rights">Copyright © 2006 Kaltura Ltd. All rights reserved. | &nbsp;<a style="color: #0080ff;" tabindex="2000" href="mailto:info@kaltura.com">Contact Us</a></div>
                </td>
                <td>
                    <img src="images/admin-footerLogo.png" alt="Kaltura" />
                </td>
            </tr>
        </table>
    </div>
    <div id="PlayerWindow" class="playerPop" style="width: 525px; height: 361px; left: 120px; top: 10%; display: none;">
        <a href="javascript:void(0);" id="player_closr_h" class="close">Close</a>
        <div class="inner">
            <div id="PlayerOuterContainer" style="position: absolute; z-index: 0; top: 0; left: 0; width: 100%; height: 100%;"></div>
            <div class="inner" id="WMPDiv" style="position: absolute; z-index: 0; top: 0; left: 0">
            </div>
        </div>
    </div>
    <div id="ndsdiv" style="position: absolute; z-index: 1; top: 0; left: 0; width: 0; height: 0;"></div>
</body>
</html>
