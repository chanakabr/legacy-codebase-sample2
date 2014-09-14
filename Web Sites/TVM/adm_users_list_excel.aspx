<%@ Page Language="C#" AutoEventWireup="true" CodeFile="adm_users_list_excel.aspx.cs" Inherits="adm_users_list_excel" %>


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
		<script type="text/javascript">
		    function create_csv() {
		        RS.Execute("adm_users_list_excel.aspx", "GetTableCSV", callback_create_csv, errorCallback);
		    }

		    function GetPageTable(orderBy, pageNum) {
		        RS.Execute("adm_users_list_excel.aspx", "GetPageContent", orderBy, pageNum, callback_page_content, errorCallback);
		    }
		</script>
</HEAD>
<body onload="GetPageTable('' , 0)">
    <form method="post"  id="form1" runat="server">     
        <input type="file" runat=server id="file_marker" name="file_marker" style="width: 0px; height: 0px; display: none;" />

        <div class="wrapper">
            <div id="users">
	            <div class="h1">
		            <h1>User List</h1>
	            </div>
	            <!--/h1 -->
	         <%--   <div class="find"><b class="top"><b></b></b>

		            <b class="bot"><b></b></b></div>--%>
		    
	            <!--/picSelected -->
	           <%-- <div class="butLine">
	                <table>
                        <tr>
                            <td>
                                <a  class="btn_confirm" title="Print To Excel" href="create_csv()"></a></td><td>&nbsp;
                            </td>
                            <td>
                                <a class="btn_cancel" href="javascript:window.close();" title="Cancel"></a>
                            </td>

                             <td>
                                <a class="btn_test" href="javascript:window.open();" title="test"></a>
                            </td>

                        </tr>
	                </table>
	            </div>	          
	            <br />
	        </div>--%>

              <div id="page_content"></div>	     
        </div>
    </form>
    
</body>
</html>