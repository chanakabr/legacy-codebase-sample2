<%@ Page Language="C#" AutoEventWireup="true" CodeFile="epg_uploader.aspx.cs" Inherits="epg_uploader" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">


<script type="text/javascript">
    function checkFileExtension(elem) {
        var filePath = elem.value;


        if (filePath.indexOf('.') == -1) {
            alert('No file selected');
            return false;
        }


        var validExtensions = new Array();
        var ext = filePath.substring(filePath.lastIndexOf('.') + 1).toLowerCase();


        validExtensions[0] = 'xml';
        

        for (var i = 0; i < validExtensions.length; i++) {
            if (ext == validExtensions[i])
                return true;
        }


        alert('The file extension ' + ext.toUpperCase() + ' is not allowed!');
        return false;
    }

    function isNumberKey(evt) {
        var charCode = (evt.which) ? evt.which : event.keyCode
        if (charCode > 31 && (charCode < 48 || charCode > 57))
            return false;

        return true;
    }

</script>

<head runat="server">

</head>
<body>
    <form id="form1" runat="server">
    <h1>Epg Ingest</h1>
    <div>
    <table>

      <tr> 
         <td><asp:Label ID="lblGroupID" runat="server" Text="GroupID:"></asp:Label> </td>
         <td><asp:TextBox ID="txtGroupID" runat="server"   onkeypress="return isNumberKey(event)"></asp:TextBox></td>           
      </tr>  

      <tr>  
         <td><asp:Label ID="lblChannel" runat="server" Text="EpgChannel:"></asp:Label> </td>
         <td><asp:TextBox ID="txtChannel" runat="server"  ></asp:TextBox></td>           
      </tr>

      <tr>       
          <td colspan="2" ><asp:FileUpload ID="fileUploader" runat="server"  /></td>
      </tr>
      <tr>       
          <td colspan="2" ><asp:Button ID="btnProceessXml" runat="server" Text="Process file" OnClick="btnProceessXml_Click" OnClientClick="return checkFileExtension(fileUploader);" /></td>
      </tr>
      <tr>       
          <td colspan="2" ><asp:Label ID="lblStatus" runat="server" Text=""></asp:Label></td>           
      </tr>             
              
    </table>
    </div>
    </form>
</body>
</html>
