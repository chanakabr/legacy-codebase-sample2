<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_pic_popup_uploader.aspx.cs" Inherits="adm_pic_popup_uploader" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1255" />
    <title><% TVinciShared.PageUtils.GetTitle(); %></title>
    <meta content="" name="description" />
    <meta name="robots" content="all" />
    <meta name="revisit-after" content="1 days" />
    <meta name="Author" content="Guy Barkan" />
    <meta content="<% TVinciShared.PageUtils.GetKeyWords(); %>" name="keywords" />
    <meta http-equiv="Pragma" content="no-cache">
    <link href="css/styles-en.css" type="text/css" rel="stylesheet" />
    <link href="css/addPic-en.css" type="text/css" rel="stylesheet" />
    <script type="text/javascript" language="JavaScript" src="js/rs.js"></script>
    <script type="text/javascript" language="JavaScript" src="js/adm_utils.js"></script>
    <script type="text/javascript" language="JavaScript" src="js/utils.js"></script>
    <script type="text/javascript" language="JavaScript" src="js/AnchorPosition.js"></script>
    <script type="text/javascript" language="JavaScript" src="js/dom-drag.js"></script>
</head>
<body onload="GetPageTable('' , 0);StartValues();">
    <form id="form1" runat="server">
        <%--<input type="hidden" id="ids_place" name="ids_place" style="width: 0px; height: 0px; display: none;" value="<% GetIDs(); %>" />--%>
        <input type="file" runat="server" id="file_marker" name="file_marker" style="width: 0px; height: 0px; display: none;" />
        <div id="tag_collections_div" class="floating_div"></div>
        <div class="wrapper">
            <table style="width: auto;">
                <tr>
                    <td colspan="2">
                        <div class="h1">
                            <h1>Pics uploader</h1>
                        </div>
                        <br />
                    </td>
                </tr>
                <tr>
                    <td class='adm_table_header_nbg' nowrap><span class="red">*&nbsp;&nbsp;</span>Name</td>
                    <td class="align1">
                        <asp:TextBox ID="txtName" runat="server" Width="300" MaxLength="128"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td class='adm_table_header_nbg' nowrap><span class="red">*&nbsp;&nbsp;</span>Pic Link</td>
                    <td class="align1">
                        <asp:TextBox ID="txtPicLink" runat="server" Width="300" MaxLength="128"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td class='adm_table_header_nbg' nowrap>
                        <asp:Label ID="lblPicRatio" runat="server" Visible="True">Pic Ratio</asp:Label>
                    </td>
                    <td class="align1">
                        <asp:DropDownList ID="ddlRatio" runat="server" Width="100" MaxLength="128" AutoPostBack="True" OnSelectedIndexChanged="ddlRatio_SelectedIndexChanged"></asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td class='adm_table_header_nbg' nowrap></td>
                    <td class="align1">
                        <asp:Image ID="imgPicRatio" runat="server" Width="90" Height="65" Visible="False"></asp:Image>
                    </td>
                </tr>
            </table>
            <!--/picSelected -->
            <div class="butLine">
                <table>
                    <tr>
                        <td colspan="3">
                            <asp:Label ID="lblStatus" runat="server" Visible="False"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Button ID="btnConfirm" runat="server" OnClick="btnConfirm_Click"
                                CssClass="btn_confirm" BorderStyle="None"
                                PostBackUrl="~/adm_pic_popup_uploader.aspx" /></td>
                        <td>&nbsp;</td>
                        <td>
                            <asp:Button ID="btnCancel" runat="server" OnClick="btnCancel_Click"
                                CssClass="btn_cancel" BorderStyle="None"
                                PostBackUrl="~/adm_pic_popup_uploader.aspx" /></td>
                    </tr>
                </table>
            </div>
            <br />
        </div>
    </form>

</body>
</html>

