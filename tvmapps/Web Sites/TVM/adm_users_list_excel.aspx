<%@ Page Language="C#" AutoEventWireup="true" Inherits="adm_users_list_excel" Codebehind="adm_users_list_excel.aspx.cs" %>


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
</HEAD>
<body <%--onload="GetPageTable('' , 0)"--%>>
    <form method="post"  id="form1" runat="server">     
        <input type="file" runat=server id="file_marker" name="file_marker" style="width: 0px; height: 0px; display: none;" />

        <div class="wrapper">
            <div id="users">
	            <div class="h1">
		            <h1>User List</h1>     
                    <h2>
                    <table>
                        <tr>
                            <td style="text-align: left; padding-bottom: 5px;">
							    <span class="small_header">Total Records :</span>                                
							    <span class="small_text"><% GetTotalRecords(); %></span> 
							    </td>                            
                    	    <td class="space01">&nbsp;&nbsp;</td>
                        </tr>
                        <tr>
                            <td style="text-align: left; padding-bottom: 5px;">
							    <span class="small_header">Each Section Total Records :</span>                               
							    <span class="small_text"><% GetBulkSize(); %></span> 
							    </td>
                            <td class="space01">&nbsp;&nbsp;</td>
                        </tr>
                        
                        <tr>                                        
                            <td>
                                <span style="font-size:13px; font-weight:bold;line-height:28px;">Select Section :&nbsp;
                                <asp:DropDownList ID="dropSectionsList" runat="server" Width="200px"></asp:DropDownList>&nbsp;
                            </td>                                                                        
                        </tr>
                        <tr>                                        
                            <td>
                                <span style="font-size:13px; font-weight:bold;line-height:28px;">Free :&nbsp;
                                <asp:TextBox ID="txtFree" runat="server" Width="260px"></asp:TextBox>&nbsp;
                            </td>                                                                        
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="ButtonGenerator" runat="server" onclick="GetExcel"   CssClass="btn_generate" BorderStyle="None" 
                                    PostBackUrl="~/adm_users_list_excel.aspx" />
                            </td>
                        </tr>
                    </table>
                    </h2>
	            </div>           

              <div id="page_content"></div>	     
        </div>
    </form>
    
</body>
</html>