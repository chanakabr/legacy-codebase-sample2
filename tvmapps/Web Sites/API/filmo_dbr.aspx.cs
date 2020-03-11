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
using ODBCWrapper;

public partial class filmo_dbr : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "";
        try
        {
            string key = Request.QueryString["key"];
            string sBaseURL = Request.QueryString["url"];
            if (String.IsNullOrEmpty(sBaseURL) == true)
                sBaseURL = "";
            if (string.IsNullOrEmpty(key))
            {
                string fileIDStr = Request.QueryString["fileID"];
                if (!string.IsNullOrEmpty(fileIDStr))
                {
                    int fileID = int.Parse(fileIDStr);
                    if (!GenerateKey(fileID, sBaseURL, ref key))
                    {
                        HttpContext.Current.Response.StatusCode = 404;
                        return;
                    }
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return;
                }
            }
            
            
            string sBRT = Request.QueryString["brt"];
            if (String.IsNullOrEmpty(sBRT) == true)
                sBRT = "4;9;19";
            else
            {
                sBRT = HttpUtility.HtmlDecode(sBRT);
            }
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
                        string[] sep = {";"};
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
                            if (sEnding.Length == 1)
                                sEnding = "0" + sEnding;
                            //sRet += "<file bitrate=\"" + sEnding + "\" only_fullscreen=\"false\">" + TVinciShared.ProtocolsFuncs.XMLEncode(m.GetURL(u.Scheme + "://" + u.Host + sPathWithoutFile + sFile + "_" + sEnding + ".mp4"), true) + "</file>";
                            sRet += "<file bitrate=\"" + sEnding + "\" only_fullscreen=\"false\">" + TVinciShared.ProtocolsFuncs.XMLEncode(string.Format("{0}?key={1}",u.Scheme + "://" + u.Host + sPathWithoutFile + sFile + "_" + sEnding + ".mp4", key), true) + "</file>";
                        }
                        sRet += "</set>\r\n";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            sRet = "<set>" +TVinciShared.ProtocolsFuncs.XMLEncode(ex.Message , true) + "</set>";
        }
        Response.ClearHeaders();
        Response.Clear();
        Response.ContentType = "text/xml";
        Response.Expires = -1;
        Response.Write(sRet);
    }

    private bool GenerateKey(int fileID, string url, ref string keyStr)
    {
        bool retVal = false;
        bool isFree = false;
        DataSetSelectQuery selectQuery = new DataSetSelectQuery();
        
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += " select * from ppv_modules_media_files ";
        selectQuery += " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", fileID);
        selectQuery += " and is_active = 1 and status = 1";
        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            if (count > 0)
            {
                isFree = true;

            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (!isFree)
        {
            DataSetSelectQuery fileSelectQuery = new DataSetSelectQuery();
            fileSelectQuery.SetConnectionKey("CONNECTION_STRING");
            fileSelectQuery += " select STREAMING_CODE from media_files ";
            fileSelectQuery += " where ";
            fileSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", fileID);
            fileSelectQuery += " and is_active = 1 and status = 1";
            if (fileSelectQuery.Execute("query", true) != null)
            {
                int count = fileSelectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    string fileCode = fileSelectQuery.Table("query").DefaultView[0].Row["STREAMING_CODE"].ToString();
                    if (url.EndsWith(fileCode))
                    {
                        retVal = true;
                    }
                }
            }
            fileSelectQuery.Finish();
            fileSelectQuery = null;
            if (retVal)
            {
                CDNetworksVault.MediaVault m = new CDNetworksVault.MediaVault("filmofvs", "guest", 7200);
                Uri fileUrl = new Uri(url);
                string[] sFileSegments = fileUrl.Segments;
                string sPathWithoutFile = "";
                for (int i = 0; i < sFileSegments.Length - 1; i++)
                {
                    sPathWithoutFile += sFileSegments[i];
                }
                string sFile = sFileSegments[sFileSegments.Length - 1];
                string keyUrlStr = m.GetURL(fileUrl.Scheme + "://" + fileUrl.Host + sPathWithoutFile + sFile + "_" + "04" + ".mp4");
                Uri keyUrl = new Uri(keyUrlStr);
                keyStr = HttpUtility.ParseQueryString(keyUrl.Query).Get("key");
            }
        }
        return retVal;
    }

}
