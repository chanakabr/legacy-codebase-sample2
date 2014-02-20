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

public partial class bftv_dbr : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "";
        try
        {
            string sBaseURL = Request.QueryString["url"];
            if (String.IsNullOrEmpty(sBaseURL) == true)
                sBaseURL = "";
            string sBRT = Request.QueryString["brt"];
            if (String.IsNullOrEmpty(sBRT) == true)
                sBRT = "4;8;13";
            string sTick = Request.QueryString["tick"];
            if (String.IsNullOrEmpty(sTick) == true)
                sTick = "";
            string sGroupID = Request.QueryString["group"];
            if (String.IsNullOrEmpty(sGroupID) == true)
                sGroupID = "";
            string sHash = Request.QueryString["hash"];
            if (String.IsNullOrEmpty(sHash) == true)
                sHash = "";
            if (sBaseURL == "" || sTick == "" || sGroupID == "" || sHash == "")
            {
                sRet = "<set>" + TVinciShared.ProtocolsFuncs.XMLEncode("Invalid request", true) + "</set>";
            }
            else
            {
                DateTime t = DateTime.FromFileTimeUtc(long.Parse(sTick.ToString()));
                if (((TimeSpan)(DateTime.UtcNow - t)).Minutes > 120)
                {
                    sRet = "<set>" + TVinciShared.ProtocolsFuncs.XMLEncode("Too old", true) + "</set>";
                }
                else
                {
                    object oGroupSecret = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_SECRET_CODE", int.Parse(sGroupID), 86400);
                    string sSecret = "";
                    if (oGroupSecret != null && oGroupSecret != DBNull.Value)
                        sSecret = oGroupSecret.ToString();
                    string sToHash = "";
                    string sHashed = "";
                    sToHash = sSecret + sTick.ToString();
                    sHashed = TVinciShared.ProtocolsFuncs.CalculateMD5Hash(sToHash);
                    if (sHash != sHashed)
                    {
                        sRet = "<set>" + TVinciShared.ProtocolsFuncs.XMLEncode("Wrong hash", true) + "</set>";
                    }
                    else
                    {
                        sRet = "<set>";
                        Uri u = new Uri(sBaseURL);
                        string[] sFileSegments = u.Segments;
                        string sPathWithoutFile = "";
                        for (int i = 0; i < sFileSegments.Length - 1; i++)
                        {
                            sPathWithoutFile += sFileSegments[i];
                        }
                        string sFile = sFileSegments[sFileSegments.Length - 1];
                        //CDNetworksVault.MediaVault m = new CDNetworksVault.MediaVault("filmofvs", "guest", 7200);
                        string[] sep = { ";" };
                        string[] bitRates = sBRT.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                        System.Collections.SortedList s = new SortedList();
                        for (int i = 0; i < bitRates.Length; i++)
                        {
                            Int32 nBitRate = int.Parse(bitRates[i]);
                            s[nBitRate] = nBitRate;
                        }
                        IDictionaryEnumerator iter = s.GetEnumerator();
                        while (iter.MoveNext() == true)
                        {
                            string sEnding = iter.Value.ToString();
                            //if (sEnding.Length == 1)
                                //sEnding = "0" + sEnding;
                            sRet += "<file bitrate=\"" + sEnding + "\" only_fullscreen=\"false\">" + TVinciShared.ProtocolsFuncs.XMLEncode(u.Scheme + "://" + u.Host + sPathWithoutFile + sFile + "_" + sEnding + ".m4v", true) + "</file>";
                        }
                        sRet += "</set>\r\n";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            sRet = "<set>" + TVinciShared.ProtocolsFuncs.XMLEncode(ex.Message, true) + "</set>";
        }
        Response.ClearHeaders();
        Response.Clear();
        Response.ContentType = "text/xml";
        Response.Expires = -1;
        Response.Write(sRet);
    }
}
