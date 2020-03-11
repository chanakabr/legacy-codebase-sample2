using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using TVinciShared;
using System.Text;
using KLogMonitor;
using System.Reflection;

public partial class ingest_api : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected void Page_Load(object sender, EventArgs e)
    {

        string sXML = GetFormParameters();
        log.Debug("INGEST_API - Start request - input is " + sXML);
        XmlDocument theDoc = new XmlDocument();

        string responseSTR = string.Empty;
        try
        {
            theDoc.LoadXml(sXML);

            XmlNode userNode = theDoc.SelectSingleNode("Feeder/userName");
            string userName = userNode.FirstChild.Value.ToLower().Trim();

            XmlNode passNode = theDoc.SelectSingleNode("Feeder/passWord");
            string passWord = passNode.FirstChild.Value.ToLower().Trim();

            XmlNode dataNode = theDoc.SelectSingleNode("Feeder/data");
            string data = dataNode.FirstChild.Value.Trim();
            int playerID = 0;
            int groupID = PageUtils.GetGroupByUNPass(userName, passWord, ref playerID);

            if (groupID > 0)
            {
                string xmlStr = data;

                TvinciImporter.ImporterImpl.DoTheWorkInner(xmlStr, groupID, string.Empty, ref responseSTR, false);

                if (string.IsNullOrEmpty(responseSTR))
                {
                    responseSTR = GetResponse(false, string.Empty);
                }
                else
                {
                    XmlDocument theRes = new XmlDocument();

                    try
                    {
                        theRes.LoadXml(responseSTR);

                        XmlNode theNode = theRes.FirstChild;

                        string status = GetItemParameterVal(ref theNode, "status");
                        string desc = GetItemParameterVal(ref theNode, "message");
                        string coguid = GetItemParameterVal(ref theNode, "co_guid");
                        string tvmid = GetItemParameterVal(ref theNode, "tvm_id");

                        responseSTR = GetResponse(status, desc, coguid, tvmid);
                    }
                    catch
                    {
                        responseSTR = GetResponse(false, string.Empty);
                    }
                }
            }
            else
            {
                responseSTR = GetResponse(false, "INVALID_CREDENTIALS");
            }

        }
        catch (Exception ex)
        {
            log.Error("", ex);
            responseSTR = GetResponse(false, ex.Message);
        }
        log.Debug("INGEST_API - For input " + sXML + " response is " + responseSTR);
        Response.ContentType = "text/xml";
        Response.ClearHeaders();
        Response.Clear();
        Response.Write(responseSTR);
    }

    private string GetResponse(bool isValid, string description)
    {
        string retVal = string.Empty;

        string statusStr = "OK";
        if (!isValid)
        {
            statusStr = "ERROR";
        }
        return GetResponse(statusStr, description, string.Empty, string.Empty);
    }

    private string GetResponse(string status, string description, string coguid, string tvmid)
    {
        string retVal = string.Empty;
        StringBuilder sb = new StringBuilder();
        sb.Append("<Response>");
        sb.AppendFormat("<status>{0}</status>", status);
        sb.AppendFormat("<description>{0}</description>", description);
        sb.AppendFormat("<assetID>{0}</assetID>", coguid);
        sb.AppendFormat("<tvmID>{0}</tvmID>", tvmid);
        sb.Append("</Response>");
        return sb.ToString();
    }

    protected string GetFormParameters()
    {
        Int32 nCount = Request.TotalBytes;
        string sFormParameters = Encoding.UTF8.GetString(Request.BinaryRead(nCount));
        return sFormParameters;
    }

    private string GetItemParameterVal(ref XmlNode theNode, string sParameterName)
    {
        string sVal = "";
        if (theNode != null)
        {
            XmlAttributeCollection theAttr = theNode.Attributes;
            if (theAttr != null)
            {
                int nCount = theAttr.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sName = theAttr[i].Name.ToLower();
                    if (sName.ToLower().Trim() == sParameterName.ToLower().Trim())
                    {
                        sVal = theAttr[i].Value.ToString();
                        break;
                    }
                }
            }
        }
        return sVal;
    }
}
