using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

public partial class turkcell_dbr : System.Web.UI.Page
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
                sBRT = "low;med;high";
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
                        string sFileExt = string.Empty;
                        if (sFile.IndexOf('.') > 0)
                        {
                            sFileExt = sFile.Substring(sFile.IndexOf('.') + 1);
                            sFile = sFile.Substring(0, sFile.IndexOf('.'));
                        }
                        //CDNetworksVault.MediaVault m = new CDNetworksVault.MediaVault("filmofvs", "guest", 7200);
                        string[] sep = { ";" };
                        string[] bitRates = sBRT.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                        System.Collections.SortedList s = new SortedList();
                        for (int i = 0; i < bitRates.Length; i++)
                        {
                            s[i] = bitRates[i];
                        }
                        int count = 1;
                        IDictionaryEnumerator iter = s.GetEnumerator();
                        while (iter.MoveNext() == true)
                        {
                            string sEnding = iter.Value.ToString();
                            //if (sEnding.Length == 1)
                            //sEnding = "0" + sEnding;
                            sRet += "<file bitrate=\"" + count + "\" only_fullscreen=\"false\">" + TVinciShared.ProtocolsFuncs.XMLEncode(u.Scheme + "://" + u.Host + sPathWithoutFile + sFile + "/" + sEnding , true) + "</file>";
                            count++;

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