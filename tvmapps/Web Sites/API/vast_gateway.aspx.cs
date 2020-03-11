using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Xml;
using VASTParser;
using KLogMonitor;
using System.Reflection;

public partial class vast_gateway : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected string GetSafeQueryString(string sKey)
    {
        try
        {
            return Request.QueryString[sKey].ToString();
        }
        catch
        {
            return "0";
        }
    }

    protected string GetFormParameters()
    {
        Int32 nCount = Request.TotalBytes;
        string sFormParameters = System.Text.Encoding.UTF8.GetString(Request.BinaryRead(nCount));
        return sFormParameters;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        int nMediaID = int.Parse(GetSafeQueryString("media_id"));
        int nGroupID = int.Parse(GetSafeQueryString("group_id"));
        string test = GetSafeQueryString("test");
        string adType = GetSafeQueryString("t");
        if (nMediaID != 0 && test.Equals("true") && nGroupID != 0)
        {
            VASTParser.VASTParser parser = VASTFactory.GetVASTImpl(nMediaID, nGroupID, adType);
            string sXml = GetFormParameters();
            if (string.IsNullOrEmpty(sXml))
            {
                string url = parser.GetVastURL();
                //string mediaFileXml = parser.GetAdFileXml();
                //string retXml = WS_Utils.SendXMLHttpReq(@"http://admatcher.videostrip.com/?categories=default&puid=23940891&host=url.com&fmt=vast20", string.Empty, string.Empty);
                Response.Clear();
                Response.Expires = -1;
                Response.ContentType = "text/xml";
                Response.Write(url);
            }
            else
            {
                parser.SetXml(sXml);
                string retXml = parser.GetAdFileXml();
                Response.Clear();
                Response.Expires = -1;
                Response.ContentType = "text/xml";
                log.Debug("Vast XML - Vast response xml is " + sXml);
                Response.Write(retXml);

            }
        }
        else
        {
            string companionAdXml = string.Empty;
            string sXml = GetFormParameters();
            if (!string.IsNullOrEmpty(sXml))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sXml);
                XmlNode theType = xmlDoc.SelectSingleNode("/root/request/@type");
                string sType = theType.Value.ToLower().Trim();
                if (!string.IsNullOrEmpty(sType) && sType.Equals("vast_companion_ad"))
                {
                    string sPlayerUN = ProtocolsFuncs.GetFlashVarsValue(ref xmlDoc, "player_un");
                    string sPlayerPass = ProtocolsFuncs.GetFlashVarsValue(ref xmlDoc, "player_pass");
                    int nPlayerID = 0;
                    nGroupID = PageUtils.GetGroupByUNPass(sPlayerUN, sPlayerPass, ref nPlayerID);
                    if (nGroupID != 0)
                    {
                        XmlNode theMediaID = xmlDoc.SelectSingleNode("/root/request/params/@id");
                        if (theMediaID != null)
                        {
                            nMediaID = int.Parse(theMediaID.Value.ToLower().Trim());
                            XmlNode theWidth = xmlDoc.SelectSingleNode("/root/request/params/@width");
                            string sWidth = theWidth.Value.Trim();
                            XmlNode theHeight = xmlDoc.SelectSingleNode("/root/request/params/@height");
                            string sHeight = theHeight.Value.Trim();
                            VASTParser.VASTParser parser = VASTFactory.GetVASTImpl(nMediaID, nGroupID, "pre");
                            companionAdXml = parser.GetCompanionAdXml(sWidth, sHeight);

                        }
                        else
                        {
                            TVinciShared.ProtocolsFuncs.GetErrorMessage("Mising params");
                        }
                    }
                    else
                    {
                        companionAdXml = TVinciShared.ProtocolsFuncs.GetErrorMessage("Site Unauthorized to query TVinci");
                    }
                }
                else
                {
                    companionAdXml = TVinciShared.ProtocolsFuncs.GetErrorMessage("Unknown protocol type");
                }
            }
            Response.Clear();
            Response.Expires = -1;
            Response.ContentType = "text/xml";
            Response.Write(companionAdXml.ToString());
        }

    }
}