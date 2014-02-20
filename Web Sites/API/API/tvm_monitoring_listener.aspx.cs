using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

public partial class tvm_monitoring_listener : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.IsSecureConnection == false && caching_server_utils.GetCallerIP() != "127.0.0.1")
        {
            Logger.Logger.Log("TVM status notification ", "Http call not allowed", "tvm_notification");
            return;
        }
        if (caching_server_utils.IsIPOK("127.0.0.1;213.8.115.108;80.179.194.132") == false)
        {
            Logger.Logger.Log("TVM status notification ", "IP not allowed", "tvm_notification");
            return;
        }
        string sRequest = caching_server_utils.GetRequestXML(Request);
        string xmlData = caching_server_utils.EscapeDecoder(sRequest);
        XmlDocument theDoc = new XmlDocument();
        string sStatus = "";
        try
        {
            theDoc.LoadXml(xmlData);
            XmlNode theStatus = theDoc.SelectSingleNode("/root/tvm/@status");
            sStatus = theStatus.Value.ToLower().Trim();
            if (sStatus == "ok")
                //here you should mark TVM as OK
                Logger.Logger.Log("TVM status notification: TVM ok", sStatus, "tvm_notification");
            else
                //here you should mark TVM as in problem
                Logger.Logger.Log("TVM status notification: TVM fail", sStatus, "tvm_notification");
        }
        catch
        {
            Logger.Logger.Log("TVM status notification: Wrong notification", "Exception for notification: " + sRequest, "tvm_notification");
        }
        Response.Clear();
        Response.Write(sStatus);
    }
}
