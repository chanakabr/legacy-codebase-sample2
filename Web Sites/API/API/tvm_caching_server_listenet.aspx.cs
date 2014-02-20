using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

public partial class tvm_caching_server_listenet : System.Web.UI.Page
{

    static protected void HandleCache(ref string[] sXMLRequestPart , string sXMLResponse)
    {
        string sResponse = "";
        Int32 nID = caching_server_utils.GetCacheID(ref sXMLRequestPart , ref sResponse);
        if (nID == 0)
            InsertNewCache(ref sXMLRequestPart, sXMLResponse);
        else
            UpdateCache(nID, sXMLResponse);
    }

    static protected void InsertNewCache(ref string[] sXMLRequestPart, string sXMLResponse)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tvm_caching_server");

        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST", "=", sXMLRequestPart[0]);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST2", "=", sXMLRequestPart[1]);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST3", "=", sXMLRequestPart[2]);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST4", "=", sXMLRequestPart[3]);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST5", "=", sXMLRequestPart[4]);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST6", "=", sXMLRequestPart[5]);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RESPONSE", "=", sXMLResponse);
        
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    static protected void UpdateCache(Int32 nID, string sXMLResponse)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("tvm_caching_server");

        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RESPONSE", "=", sXMLResponse);
        updateQuery += " where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.IsSecureConnection == false && caching_server_utils.GetCallerIP() != "127.0.0.1")
        {
            Logger.Logger.Log("TVM caching server listener ", "Http call not allowed", "tvm_caching_server_listener");
            return;
        }
        if (caching_server_utils.IsIPOK("127.0.0.1;213.8.27.24;213.8.27.23;213.8.27.137;213.8.115.108") == false)
        {
            Logger.Logger.Log("TVM caching server listener ", "IP: " + caching_server_utils.GetCallerIP()+ " not allowed", "tvm_caching_server_listener");
            return;
        }
        string sRequest = caching_server_utils.GetRequestXML(Request);
        string xmlData = caching_server_utils.EscapeDecoder(sRequest);
        XmlDocument theDoc = new XmlDocument();
        try
        {
            theDoc.LoadXml(xmlData);
            XmlNode theRequest = theDoc.SelectSingleNode("/root/req/root");
            XmlNode theResponse = theDoc.SelectSingleNode("/root/res/response");
            string sXMLRequest = caching_server_utils.ConvertXMLToString(ref theRequest);
            string sXMLResponse = caching_server_utils.ConvertXMLToString(ref theResponse);

            string[] sXMLRequestPart = {"" , "" , "" , "" , "" , "" , ""};
            caching_server_utils.SplitString(sXMLRequest, ref sXMLRequestPart);

            if (sXMLRequestPart[0] != "")
                HandleCache(ref sXMLRequestPart , sXMLResponse);
        }
        catch
        {
            Logger.Logger.Log("TVM status notification: Wrong notification", "Exception for notification: " + sRequest, "tvm_notification");
        }
    }
}
