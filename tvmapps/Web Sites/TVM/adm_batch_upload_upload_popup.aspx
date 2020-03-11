<%@ Page Language="C#" AutoEventWireup="true" Inherits="adm_batch_upload_upload_popup" Codebehind="adm_batch_upload_upload_popup.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<script type="text/javascript" src="js/ajaxFuncs.js"></script>
<script type="text/javascript">
    function batchupload(u) {
        alert("batch upload");
        sURL = "AjaxBatchUpload.aspx?u=" + u;
        postFile(sURL, callback_BatchUpload);
    }

</script>
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
		<span style="font-size:13px; font-weight:bold;line-height:28px;">Upload Excel file</span><br />
        <asp:FileUpload id="FileUpload1" runat="server" size="60"/>
        <asp:Button ID="btnUpload" runat="server" Text="Upload" OnClick="UploadExcel"/>
        <span style="font-size:13px; font-weight:bold;line-height:28px;"> <asp:Label ID="LblUploadStatus" runat="server" Visible="False"></asp:Label></span><br />
        <asp:Label ID="lblIndication" runat="server" Text="Label"></asp:Label >
    </div>
    </form>
</body>
</html>
