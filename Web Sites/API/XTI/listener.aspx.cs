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

public partial class listener : System.Web.UI.Page
{

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

    protected void UpdateXTI(Int32 nTransID)
    {
        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
        directQuery += "update xti set waiting_syncs=waiting_syncs+1 ";
        if (nTransID != -1)
        {
            directQuery += ", trans_id=" + nTransID.ToString();
        }
        directQuery += " where";
        if (Session["O_ENVIRONMENT"] != null && Session["O_ENVIRONMENT"].ToString() == "test")
            directQuery += " id=1 ";
        if (Session["O_ENVIRONMENT"] != null && Session["O_ENVIRONMENT"].ToString() == "prod")
            directQuery += " id=2 ";
        directQuery.Execute();
        directQuery.Finish();
        directQuery = null;
    }

    protected Int32 GetAccountCount()
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select count(*) as co from accounts";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CO"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string sBaseResponse = "";
        try
        {
            /*
            string sXML = GetFiles();
            Logger.Logger.Log("Alert", sXML, "xti");
            System.Xml.XmlDocument theDoc = new System.Xml.XmlDocument();
            if (sXML != "")
            {
                theDoc.LoadXml(sXML);
                theDoc = XTI.stripDocumentNamespace(theDoc);
                string sXpath = "ExportAlerts/VodAssetAlert";
                System.Xml.XmlNode theNodeVal = theDoc.SelectSingleNode(sXpath);
                if (theNodeVal != null)
                {
                    UpdateXTI(-1);
                    sBaseResponse = "<response><status description=\"OK - Scheduald XTI will run in the next few minutes\"/></response>";
                }
                else
                    sBaseResponse = "<response><status description=\"ERROR - Request not well formed\"/></response>";
            }
            else
            {
                */
                if (Request.QueryString["test"] != null)
                {
                    if (Request.QueryString["test"].ToString() == "guy")
                    {
                        string sError = "";
                        bool bOK = false;
                        //XTIFeeder.XTI.GetVodAssetList(1 , "0", "heb", 35, "+00:00", ref bOK, ref sError);
                        string sRet = XTIFeeder.XTI.GetVodAssetList(5, XTIFeeder.XTI.GetLastXTIID(5), "heb", 90, "+00:00", "http://10.80.66.10:8080/XTI/upload", 141, 142, 0, 0, "PcShowVod", ref bOK, ref sError , false);
                        //string sRet = XTIFeeder.XTI.GetVodAssetList(2, XTIFeeder.XTI.GetLastXTIID(2), "heb", 35, "+00:00", "http://10.80.66.10:8080/XTI/upload", 39, 42, 21, "GibraltarVod", ref bOK, ref sError);
                        //string sRet = XTIFeeder.XTI.GetVodAssetList(1, XTIFeeder.XTI.GetLastXTIID(1), "heb", 16, "+00:00", "http://10.80.66.10:8080/XTI/upload", 13, 18, 16, "GibraltarVod", ref bOK, ref sError);
                        //MtvFeeder.Feeder.ActualWork();
                        //sBaseResponse = "<response><status description=\"OK - Scheduald XTI will run on all data in the next few minutes\"/></response>";
                        //MediaEOHSync.MediaEOHSync t = new MediaEOHSync.MediaEOHSync(1, 60, "");
                        //t.DoTheJob();
                        TvinciImporter.ImporterImpl.DoTheWork(68, "http://www.tvinci.com/xml/tin.xml", "http://www.tvinci.com/xml/tin.aspx" , 2);
                    }
                    else if (Request.QueryString["test"].ToString() == "allall")
                    {
                        UpdateXTI(0);
                        sBaseResponse = "<response><status description=\"OK - Scheduald XTI will run on all data in the next few minutes\"/></response>";
                        
                    }
                    else if (Request.QueryString["test"].ToString() == "danidani")
                    {
                        UpdateXTI(-1);
                        sBaseResponse = "<response><status description=\"OK - Scheduald XTI will run on last known changes in the next few minutes\"/></response>";
                    }
                    else
                    {
                        sBaseResponse = "<response><status description=\"Fail - Please qualify the test type\"/></response>";
                    }
                //}
            }
        }
        catch (Exception ex)
        {
            sBaseResponse = "<response><error description=\"" + XMLEncode(ex.Message , true) + "\"/></response>";
            Logger.Logger.Log("Exception", ex.Message, "xti");
        }
        Logger.Logger.Log("Response", sBaseResponse, "xti");
        Response.ClearHeaders();
        Response.Clear();
        Response.ContentType = "text/xml";
        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
        Response.Write(sBaseResponse);
    }
}
