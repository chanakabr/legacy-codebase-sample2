using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using KLogMonitor;

public partial class tvm_caching_server : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.IsSecureConnection == false && caching_server_utils.GetCallerIP() != "127.0.0.1")
        {
            log.Debug("TVM caching server Http call not allowed");
            return;
        }
        string sRequest = caching_server_utils.GetRequestXML(Request);
        string xmlData = caching_server_utils.EscapeDecoder(sRequest);
        XmlDocument theDoc = new XmlDocument();
        try
        {
            theDoc.LoadXml(xmlData);
            if (theDoc.SelectSingleNode("root/flashvars/@site_guid") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["site_guid"];
                attr.OwnerElement.RemoveAttribute("site_guid");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@no_cache") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["no_cache"];
                attr.OwnerElement.RemoveAttribute("no_cache");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@alt_tvm") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["alt_tvm"];
                attr.OwnerElement.RemoveAttribute("alt_tvm");
            }
            XmlNode theRequest = theDoc.SelectSingleNode("/root");

            string sXMLRequest = caching_server_utils.ConvertXMLToString(ref theRequest);

            string[] sXMLRequestPart = { "", "", "", "", "", "", "" };
            caching_server_utils.SplitString(sXMLRequest, ref sXMLRequestPart);
            string sResponse = "";
            if (sXMLRequestPart[0] != "")
                caching_server_utils.GetCacheID(ref sXMLRequestPart, ref sResponse);
            if (sResponse == "")
                //here get the default response fot the specific protocol
                sResponse = "No response found";
            Response.ClearHeaders();
            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Expires = -1;
            Response.Write(sResponse);
        }
        catch (Exception ex)
        {
            log.Error("TVM status notification: Wrong notification- Exception for notification: " + sRequest, ex);
        }
    }
}
