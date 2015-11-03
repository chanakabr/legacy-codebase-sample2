using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Xml;
using KLogMonitor;
using System.Reflection;

public partial class xti_listener : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected string GetFiles()
    {
        Int32 nCount = Request.Files.Count;
        string s = "";
        if (nCount > 0)
        {
            string sFileName = Request.Files[0].FileName;
            System.IO.Stream requestStream = Request.Files[0].InputStream;

            Int32 nContentLength = Request.Files[0].ContentLength;
            byte[] tempBuffer = new byte[nContentLength];
            requestStream.Read(tempBuffer, 0, nContentLength);
            s = System.Text.UTF8Encoding.UTF8.GetString(tempBuffer);
        }
        return s;
    }

    private string XMLEncode(string sToEncode, bool bAttribute)
    {
        if (sToEncode.Length == 0)
            return string.Empty;
        //XmlAttribute element = m_xmlDox.CreateAttribute("E");
        //element.InnerText = sToEncode;
        sToEncode = sToEncode.Replace("&", "&amp;");
        sToEncode = sToEncode.Replace("<", "&lt;");
        sToEncode = sToEncode.Replace(">", "&gt;");
        if (bAttribute == true)
        {
            sToEncode = sToEncode.Replace("'", "&apos;");
            sToEncode = sToEncode.Replace("\"", "&quot;");
        }
        return sToEncode;
    }

    protected void UpdateXTI(Int32 nTransID, Int32 nXTIID)
    {
        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
        directQuery += "update xti set waiting_syncs=waiting_syncs+1 ";
        if (nTransID != -1)
        {
            directQuery += ", trans_id=" + nTransID.ToString();
        }
        directQuery += " where";
        directQuery += " id=" + nXTIID.ToString();
        directQuery.Execute();
        directQuery.Finish();
        directQuery = null;
    }

    protected Int32 GetXTIIdByIP(string sIP)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from xti where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(caller_ip))", "=", sIP.Trim());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    static public XmlDocument stripDocumentNamespace(XmlDocument oldDom)
    {
        // some config files have a default namespace
        // we are going to get rid of that to simplify our xpath expressions
        if (oldDom.DocumentElement.NamespaceURI.Length > 0)
        {
            oldDom.DocumentElement.SetAttribute("xmlns", "");
            // must serialize and reload the DOM
            // before this will actually take effect
            XmlDocument newDom = new XmlDocument();
            newDom.LoadXml(oldDom.OuterXml);
            return newDom;
        }
        else return oldDom;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string sBaseResponse = "";
        try
        {
            string sXML = GetFiles();
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            log.Debug("Alert - " + sIP + "||" + sXML);
            Int32 nXTIId = GetXTIIdByIP(sIP);
            if (nXTIId == 0)
                sBaseResponse = "<response><status description=\"IP not recognized\"/></response>";
            else
            {
                System.Xml.XmlDocument theDoc = new System.Xml.XmlDocument();
                if (sXML != "")
                {
                    theDoc.LoadXml(sXML);
                    theDoc = stripDocumentNamespace(theDoc);
                    string sXpath = "ExportAlerts/VodAssetAlert";
                    System.Xml.XmlNode theNodeVal = theDoc.SelectSingleNode(sXpath);
                    if (theNodeVal != null)
                    {
                        UpdateXTI(-1, nXTIId);
                        sBaseResponse = "<response><status description=\"OK - Scheduald XTI will run in the next few minutes for XTI ID: " + nXTIId.ToString() + "\"/></response>";
                    }
                    else
                        sBaseResponse = "<response><status description=\"ERROR - Request not well formed\"/></response>";
                }
            }
        }
        catch (Exception ex)
        {
            sBaseResponse = "<response><error description=\"" + XMLEncode(ex.Message, true) + "\"/></response>";
            log.Error("Exception - " + ex.Message, ex);
        }
        log.Debug("Response - " + sBaseResponse);
        Response.ClearHeaders();
        Response.Clear();
        Response.ContentType = "text/xml";
        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
        Response.Write(sBaseResponse);
    }
}
