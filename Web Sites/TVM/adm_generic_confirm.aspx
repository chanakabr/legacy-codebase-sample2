<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_generic_confirm.aspx.cs" Inherits="adm_generic_confirm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<HEAD>
		<meta http-equiv="Content-Type" content="text/html; charset=windows-1255"/>
		<title><% TVinciShared.PageUtils.GetTitle(); %></title>
		<meta content="Mtv Israel" name="description"/>
		<meta name="robots" content="all"/>
		<meta name="revisit-after" content="1 days"/>
		<meta name="Author" content="Guy Barkan"/>
		<meta content="<% TVinciShared.PageUtils.GetKeyWords(); %>" name="keywords"/>
		<META http-equiv="Pragma" content="no-cache">
		<link href="css/styles.css" type="text/css" rel="stylesheet"/>
		<script type="text/javascript" language="JavaScript" src="js/rs.js"></script>
		<script type="text/javascript" language="JavaScript" src="js/adm_utils.js"></script>
		<script>
		    function FinalRemove()
		    {
		        document.getElementById('final_remove_but').click();
		    }
		</script>
</HEAD>
<body class="admin_body" onkeyup="if (event.keyCode == 13) document.getElementById('confirm_btn').click();">
    <form id="form1" runat="server">
        <table width=980px align=center cellpadding=0 cellspacing=0 dir=rtl>
            <!-- top banner -->
            <tr style="height: 60px;">
                <td style="width: 100%; white-space: nowrap;" nowrap class="adm_top_banner">
                    <table width=100% cellpadding=0 cellspacing=0 >
                        <tr style="height: 100%" >
                            <td  style="white-space: nowrap; height:60px;" nowrap>
                                <img src="<% TVinciShared.PageUtils.GetAdminLogo(); %>" alt="" />
                            </td>
                            <td nowrap style="width:100%; white-space:nowrap;"></td>
                            <td style="width:100%; white-space:nowrap;" nowrap valign=top>
                                <table>
                                    <tr>
                                        <td class=small_header>משתמש: </td>
                                        <td class=small_text align=left><% GetLoginName(); %></td>
                                    </tr>
                                    <tr>
                                        <td class=small_header>קבוצה: </td>
                                        <td class=small_text align=left>אדמין</td>
                                    </tr>
                                    <tr>
                                        <td class=small_text>
                                            <a href="logout.aspx" class=logout>התנתק</a>
                                        </td>
                                        <td class=small_text align=left style="cursor: pointer;" onclick="document.location.href='logout.aspx';">
                                            <img src="images/unlock.gif" alt="" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td>
                                <img src="images/lock1.gif" alt="" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td style="width: 100%; white-space: nowrap;" nowrap>
                    <table width=100%  cellpadding=0 cellspacing=0>
                        <tr>
                            <!-- right empty area -->
                            <td class="vertical_line_breaker_right" nowrap>&nbsp;</td>        
                            <!-- right menu -->
                            <td style="width: 100%; white-space: nowrap; height:100%" valign=top nowrap>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <!-- all working area -->
            <tr valign=top style="height: 450px;">
                <td style="width: 100%; white-space: nowrap;" nowrap>
                    <table width=100%  cellpadding=0 cellspacing=0>
                        <tr style="height: 450px;">
                            <!-- right empty area -->
                            <td class="vertical_line_breaker_right" nowrap>&nbsp;</td>        
                            <td style="width:10px; white-space:nowrap;background-color: #FCFCFC;" nowrap>&nbsp;</td>        
                            <!-- right menu -->
                            <td class="adm_right_menu" nowrap><% GetMainMenu();%></td>
                            <td class="vertical_line_breaker_left" nowrap>&nbsp;</td>        
                            <!-- empty area -->
                            <td style="width:15px; white-space:nowrap;" nowrap>&nbsp;</td>        
                            <!-- main working area -->
                            <td style="width: 100%; white-space: nowrap; height:100%" valign=top nowrap>
                                <table width=100%  cellpadding=0 cellspacing=0>
                                    <tr valign=top>
                                        <!-- working area -->
                                        <td style="width:100%; white-space: nowrap;" nowrap valign=top>
                                            <table width=100% cellpadding=0 cellspacing=0>
                                                <!-- header -->
                                                <tr>
                                                    <td class=adm_main_header nowrap width=100%>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;אשור רשומות&nbsp;&nbsp;</td>
                                                </tr>
		                                        <!-- top menu area-->
                                                <tr style="height: 40px;">
                                                    <!-- empty area -->
                                                    <td style="width:100%; white-space: nowrap;" nowrap>
                                                        <table  width=100% cellpadding=0 cellspacing=0>
                                                            <tr>
                                                                <td style="white-space: nowrap; width:100%;" nowrap class=adm_right_menu><ul><% GetSubMenu(); %></ul></td>
                                                                <td style="width:100%; white-space:nowrap;" nowrap>&nbsp;</td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
		                                        <!-- content -->
							                    <tr valign="top">
							                        <td id="page_content">
							                            <% GetPageContext(); %>
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
                </td>
            </tr>
            <tr>
                <td >
                    <table style="visibilty: hidden; display: none"><tr><td>
                    <asp:Button runat=server OnClick="RemoveTheRecord" id="final_remove_but"/>
                    </td></tr>
                    </table>
                </td>
            </tr>
        </table>
    </form>
    <table width=980px align=center cellpadding=0 cellspacing=0 dir=rtl height=100%><tr height=100%><td align=left width=100% nowrap height=100% valign=bottom><div class=rights style="height: 100%; vertical-align: bottom;">Powered by <a href='http://www.kaltura.com' target=_blank>Tvinci</a></div></td></tr></table>
    
</body>
</html>

