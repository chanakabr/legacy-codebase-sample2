using ApiObjects;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;

namespace TVinciShared
{
    /// <summary>
    /// Summary description for PageUtils
    /// </summary>
    public class PageUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public PageUtils()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        static public Int32 GetDefaultPICID(Int32 nGroupID)
        {
            object oBaseDefaultPicID = ODBCWrapper.Utils.GetTableSingleVal("groups", "DEFAULT_PIC_ID", nGroupID, 3600);
            if (oBaseDefaultPicID != DBNull.Value && oBaseDefaultPicID != null)
                return int.Parse(oBaseDefaultPicID.ToString());
            else
                return 0;
        }

        static public void GetGroupName()
        {
            HttpContext.Current.Response.Write(LoginManager.GetLoginGroupName());
        }

        static public void GetLoginName()
        {
            HttpContext.Current.Response.Write(LoginManager.GetLoginName());
        }

        static public void GetCurrentDate()
        {
            HttpContext.Current.Response.Write(DateUtils.GetStrFromDate(DateTime.Now));
        }

        static public void GetUserName()
        {
            HttpContext.Current.Response.Write(LoginManager.GetLoginName());
        }

        static public string GetStatusQueryPart(string sPre)
        {
            bool bLogin = LoginManager.CheckLogin();
            string sStatus = "";
            if (sPre != "")
                sStatus = sPre + ".";
            sStatus += "status";
            if (bLogin == true)
                return " " + sStatus + " in (1,3,4) ";
            else
                return " " + sStatus + " in (1,4) ";
        }

        static public void GetTitle()
        {
            return;
            //string sHeader = "";
            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery += "select header from site_configuration where id=1";
            //if (selectQuery.Execute("query", true) != null)
            //{
            //    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            //    if (nCount > 0)
            //        sHeader = selectQuery.Table("query").DefaultView[0].Row["HEADER"].ToString();
            //}
            //selectQuery.Finish();
            //selectQuery = null;
            //HttpContext.Current.Response.Write(sHeader);
        }

        static public string ReWriteTableValue(string sVal)
        {
            try
            {
                double dVal;

                if (double.TryParse(sVal, out dVal))
                {
                    return String.Format("{0:0.##}", dVal);
                }
                else
                {
                    return sVal;
                }
            }
            catch
            {
                return sVal;
            }
        }

        static public bool DoesAccountBelongToGroup(Int32 nAccountID, Int32 nGroupID)
        {
            bool bBelongs = false;
            Int32 nParentGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("accounts", "group_id", nAccountID, 86400).ToString());
            Int32 nAccountGroup = nParentGroupID;
            if (nAccountGroup == nGroupID)
                return true;
            while (bBelongs == false && nParentGroupID != 0)
            {
                nParentGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "PARENT_GROUP_ID", nParentGroupID, 86400).ToString());
                if (nParentGroupID == nGroupID)
                    bBelongs = true;
            }
            return bBelongs;
        }

        static public string GetBasePicURL(Int32 nGroupID)
        {
            object oBasePicsURL = ODBCWrapper.Utils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID, 86400);
            string sBasePicsURL = "";
            if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
                sBasePicsURL = oBasePicsURL.ToString();
            if (sBasePicsURL == "")
                sBasePicsURL = "pics";
            else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
                sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
                sBasePicsURL = "http://" + sBasePicsURL;
            return sBasePicsURL;
        }

        static public string GetBasePicURL(string basePicUrl)
        {
            string result = string.Empty;

            if (string.IsNullOrEmpty(basePicUrl) == false)
            {
                result = basePicUrl;
            }
            else
            {
                result = "pics";
            }

            if (result.ToLower().Trim().StartsWith("http://") == false &&
                result.ToLower().Trim().StartsWith("https://") == false)
            {
                result = "http://" + result;
            }

            return result;
        }

        static public string GetPicURL(Int32 nPicID, string sPicSize, string picDB)
        {
            if (nPicID == 0)
                return "";
            string sPicURL = "";
            Int32 nPicGroupID = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += string.Format("select group_id,base_url from {0} (nolock) where ", picDB);
            //selectQuery.SetCachedSec(86400);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPicID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sPicURL = selectQuery.Table("query").DefaultView[0].Row["base_url"].ToString();
                    nPicGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "group_id", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            string sBasePicsURL = GetBasePicURL(nPicGroupID);
            bool bWithEnding = true;
            if (sBasePicsURL.EndsWith("=") == false)
                sBasePicsURL += "/";
            else
                bWithEnding = false;
            sBasePicsURL += ImageUtils.GetTNName(sPicURL, sPicSize.Replace("x", "X"));
            if (bWithEnding == false)
            {
                string sTmp = "";
                string[] s = sBasePicsURL.Split('.');
                for (int i = 0; i < s.Length - 1; i++)
                {
                    if (i > 0)
                        sTmp += ".";
                    sTmp += s[i];
                }
                sBasePicsURL = sTmp;
            }
            return sBasePicsURL;
        }

        static public string GetPicURL(long picID, string pic_base_url, string pic_remote_base_url, string sPicSize)
        {
            string result = string.Empty;

            if (picID > 0)
            {
                string sBasePicsURL = GetBasePicURL(pic_remote_base_url);
                bool bWithEnding = true;

                if (sBasePicsURL.EndsWith("=") == false)
                    sBasePicsURL += "/";
                else
                    bWithEnding = false;
                sBasePicsURL += ImageUtils.GetTNName(pic_base_url, sPicSize.Replace("x", "X"));
                if (bWithEnding == false)
                {
                    string sTmp = "";
                    string[] s = sBasePicsURL.Split('.');
                    for (int i = 0; i < s.Length - 1; i++)
                    {
                        if (i > 0)
                            sTmp += ".";
                        sTmp += s[i];
                    }
                    sBasePicsURL = sTmp;
                }
                result = sBasePicsURL;
            }

            return result;
        }

        static public string GetPicURL(Int32 nPicID, string sPicSize)
        {
            return GetPicURL(nPicID, sPicSize, "pics");
        }
        static public void GetAdminLogo()
        {
            string sHeader = "";

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select p.base_url from pics p (nolock),groups g (nolock) where g.admin_logo=p.id and ";
            selectQuery.SetCachedSec(86400);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", LoginManager.GetLoginGroupID());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sHeader = selectQuery.Table("query").DefaultView[0].Row["base_url"].ToString();
            }
            selectQuery.Finish();
            selectQuery = null;
            //HttpContext.Current.Response.Write(PageUtils.GetBasePicURL(LoginManager.GetLoginGroupID()) + "/" + ImageUtils.GetTNName(sHeader, "full"));
            HttpContext.Current.Response.Write("http://tvm.tvinci.com/pics/" + ImageUtils.GetTNName(sHeader, "full"));
        }

        static public void GetKeyWords()
        {
            return;
            //string sHeader = "";
            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery += "select key_words from site_configuration where id=1";
            //if (selectQuery.Execute("query", true) != null)
            //{
            //    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            //    if (nCount > 0)
            //        sHeader = selectQuery.Table("query").DefaultView[0].Row["key_words"].ToString();
            //}
            //selectQuery.Finish();
            //selectQuery = null;
            //HttpContext.Current.Response.Write(sHeader);
        }

        static public void GetErrorMsg(Int32 nCollspan)
        {
            if (HttpContext.Current.Session["error_msg"] == null && HttpContext.Current.Session["ok_msg"] == null)
                return;
            string sText = "";
            if (HttpContext.Current.Session["error_msg"] != null)
                sText = HttpContext.Current.Session["error_msg"].ToString();
            else if (HttpContext.Current.Session["ok_msg"] != null)
            {
                sText = HttpContext.Current.Session["ok_msg"].ToString();
            }

            string sTmp = "<tr><td class=alert_text nowrap>* &nbsp;&nbsp;";
            if (HttpContext.Current.Session["error_msg"] != null && HttpContext.Current.Session["error_msg"].ToString() != "")
                sTmp += sText;
            sTmp += "</td></tr>";
            HttpContext.Current.Session["error_msg"] = null;
            HttpContext.Current.Session["ok_msg"] = null;
            HttpContext.Current.Response.Write(sTmp);
        }

        static public void GetSiteMap()
        {
            string sRet = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from site_map where (link_parent_id=null or link_parent_id=0) and " + PageUtils.GetStatusQueryPart("") + " order by LINK_LOC_VAL";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    sRet += "<ul>";
                    sRet += "<li><h3><a href=\"";
                    sRet += selectQuery.Table("query").DefaultView[i].Row["LINK_HREF"].ToString();
                    sRet += "\" title=\"";
                    sRet += selectQuery.Table("query").DefaultView[i].Row["LINK_TITLE"].ToString();
                    sRet += "\">";
                    sRet += selectQuery.Table("query").DefaultView[i].Row["LINK_TEXT"].ToString();
                    sRet += "</a></h3></li>";
                    ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery1 += "select * from site_map where ";
                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("link_parent_id", "=", int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString()));
                    selectQuery1 += "and " + PageUtils.GetStatusQueryPart("") + " order by LINK_LOC_VAL";
                    if (selectQuery1.Execute("query", true) != null)
                    {
                        Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                        for (int j = 0; j < nCount1; j++)
                        {
                            sRet += "<li><a href=\"";
                            sRet += selectQuery1.Table("query").DefaultView[j].Row["LINK_HREF"].ToString();
                            sRet += "\" title=\"";
                            sRet += selectQuery1.Table("query").DefaultView[j].Row["LINK_TITLE"].ToString();
                            sRet += "\">";
                            sRet += selectQuery1.Table("query").DefaultView[j].Row["LINK_TEXT"].ToString();
                            sRet += "</a></li>";
                        }
                    }
                    selectQuery1.Finish();
                    selectQuery1 = null;
                    sRet += "</ul>";
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            HttpContext.Current.Response.Write(sRet);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, Int32 nID, string connectionString = null)
        {
            return GetTableSingleVal(sTable, sFieldName, "id", "=", nID, connectionString);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, string sWhereField, string sWhereSign, object sWhereVal, string connectionString = null)
        {
            object oRet = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (!string.IsNullOrEmpty(connectionString))
                selectQuery.SetConnectionKey(connectionString);
            selectQuery += "select " + sFieldName + " from " + sTable + " WITH (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sWhereField, sWhereSign, sWhereVal);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    oRet = selectQuery.Table("query").DefaultView[0].Row[sFieldName];
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return oRet;
        }

        static public bool AddTag(string sTagName)
        {
            sTagName = sTagName.Trim();
            object t = GetTableSingleVal("tags", "id", "value", "=", sTagName);
            if (t == null)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tags");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", sTagName);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            return true;
        }

        static public string GetAllChildGroupsStr()
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("in (").Append(LoginManager.GetLoginGroupID());
            string s = "";
            GetAllGroupsStr(LoginManager.GetLoginGroupID(), ref s);
            sRet.Append(s);
            sRet.Append(")");
            return sRet.ToString();
        }
        static public string GetFullChildGroupsStr(Int32 nGroupID, string sConnKey)
        {
            if (nGroupID == 0)
                return "in (0)";
            if (CachingManager.CachingManager.Exist("GetFullChildGroupsStr_" + nGroupID.ToString()) == true)
                return (string)(CachingManager.CachingManager.GetCachedData("GetFullChildGroupsStr_" + nGroupID.ToString()));
            StringBuilder sRet = new StringBuilder();
            sRet.Append("in (").Append(nGroupID);
            string s = "";
            GetAllGroupsStr(nGroupID, ref s, sConnKey);
            sRet.Append(s);
            sRet.Append(")");
            CachingManager.CachingManager.SetCachedData("GetFullChildGroupsStr_" + nGroupID.ToString(), sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            return sRet.ToString();
        }
        static public string GetFullGroupsStr(Int32 nGroupID, string sConnKey)
        {
            if (CachingManager.CachingManager.Exist("GetFullGroupsStr_" + nGroupID.ToString()) == true)
                return (string)(CachingManager.CachingManager.GetCachedData("GetFullGroupsStr_" + nGroupID.ToString()));
            StringBuilder sRet = new StringBuilder();
            sRet.Append("in (").Append(nGroupID);
            string s = "";
            GetAllGroupsStr(nGroupID, ref s, sConnKey);
            sRet.Append(s);
            string sParents = GetMiniParentsGroupsStr(nGroupID);
            if (sParents != "")
                sRet.Append(",").Append(sParents);
            sRet.Append(")");
            CachingManager.CachingManager.SetCachedData("GetFullGroupsStr_" + nGroupID.ToString(), sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            return sRet.ToString();
        }

        static public string GetAllGroupTreeStr()
        {
            Int32 nGroupID = GetUpperGroupID(LoginManager.GetLoginGroupID());
            return GetAllGroupTreeStr(nGroupID);
        }
        static public string GetAllGroupTreeStr(Int32 nGroupID)
        {
            return GetAllGroupTreeStr(nGroupID, string.Empty);

        }

        static public string GetAllGroupTreeStr(Int32 nGroupID, string sConnectionKey)
        {
            string sCachedData = string.Empty;
            if (TryGetCachedData("GetAllGroupTreeStr", nGroupID, ref sCachedData))
                return sCachedData;
            StringBuilder sRet = new StringBuilder("in (");
            DataTable dt = UtilsDal.GetGroupsTree(nGroupID, sConnectionKey);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        if (dt.Rows[i]["id"] != null)
                            sRet.Append(dt.Rows[i]["id"].ToString());
                        else
                            log.Debug("GetAllGroupTreeStr - GetAllGroupTreeStr: null in iteration: " + i + " in Group ID: " + nGroupID + " Connection key: " + sConnectionKey);
                    }
                    else
                    {
                        if (dt.Rows[i]["id"] != null)
                            sRet.Append(String.Concat(",", dt.Rows[i]["id"].ToString()));
                        else
                            log.Debug("GetAllGroupTreeStr - GetAllGroupTreeStr: null in iteration: " + i + " in Group ID: " + nGroupID + " Connection key: " + sConnectionKey);
                    }
                }
            }
            sRet.Append(")");
            string strRetVal = sRet.ToString();
            if (!strRetVal.Contains("null"))
                SetCachedData("GetAllGroupTreeStr", nGroupID, strRetVal, 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            return strRetVal;
        }

        private static void SetCachedData(string sKeyOfCachedData, int nGroupID, string sDataToCache, int nTimeToBeInCacheInSeconds, System.Web.Caching.CacheItemPriority ePriority, int nMediaID, bool bToRenew)
        {
            string sRealKey = GetRealCachedKey(sKeyOfCachedData, nGroupID);
            CachingManager.CachingManager.SetCachedData(sRealKey, sDataToCache, nTimeToBeInCacheInSeconds, ePriority, nMediaID, bToRenew);
        }

        private static bool TryGetCachedData(string sKeyOfCachedData, int nGroupID, ref string sRet)
        {
            string sRealKey = GetRealCachedKey(sKeyOfCachedData, nGroupID);
            if (CachingManager.CachingManager.Exist(sRealKey))
            {
                sRet = (string)(CachingManager.CachingManager.GetCachedData(sRealKey));
                return true;
            }
            return false;
        }

        private static string GetRealCachedKey(string sKeyOfCachedData, int nGroupID)
        {
            return String.Concat(sKeyOfCachedData, "_", nGroupID.ToString());
        }

        static public string GetAllGroupsStr(Int32 nID, ref string sRet)
        {
            return GetAllGroupsStr(nID, ref sRet, string.Empty);
        }
        static public string GetAllGroupsStr(Int32 nID, ref string sRet, string sConnKey)
        {
            if (CachingManager.CachingManager.Exist("GetAllGroupsStr_" + nID.ToString()) == true)
            {
                sRet = (string)(CachingManager.CachingManager.GetCachedData("GetAllGroupsStr_" + nID.ToString()));
                return sRet;
            }
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (sConnKey != "")
                selectQuery.SetConnectionKey(sConnKey);
            selectQuery += "select id from groups (nolock) where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_group_id", "=", nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    sRet += ",";
                    sRet += selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    GetAllGroupsStr(int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString()), ref sRet, sConnKey);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            CachingManager.CachingManager.SetCachedData("GetAllGroupsStr_" + nID.ToString(), sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            return sRet;
        }

        static public void AddCutCroptDimentions(ref DataRecordUploadField dr_upload)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mps.width, mps.height, mps.TO_CROP, mps.ratio_id from media_pics_sizes mps where mps.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mps.GROUP_ID", "=", LoginManager.GetLoginGroupID());
            //dr_upload.AddPicDimension(90, 65, "tn", true);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nWidth = int.Parse(selectQuery.Table("query").DefaultView[i].Row["width"].ToString());
                    Int32 nHeight = int.Parse(selectQuery.Table("query").DefaultView[i].Row["height"].ToString());
                    Int32 nCrop = int.Parse(selectQuery.Table("query").DefaultView[i].Row["TO_CROP"].ToString());
                    string sRatio = selectQuery.Table("query").DefaultView[i].Row["ratio_id"].ToString();
                    string sNameEnd = nWidth.ToString() + "X" + nHeight.ToString();
                    bool bCrop = true;
                    if (nCrop == 0)
                        bCrop = false;
                    dr_upload.AddPicDimension(nWidth, nHeight, sNameEnd, bCrop, sRatio);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }


        static public void AddCutCroptDimentionsEpg(ref DataRecordUploadField dr_upload)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select eps.width, eps.height , eps.TO_CROP, eps.ratio_id  from EPG_pics_sizes eps where eps.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("eps.GROUP_ID", "=", LoginManager.GetLoginGroupID());
            //dr_upload.AddPicDimension(90, 65, "tn", true);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nWidth = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "width", i);
                    Int32 nHeight = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "height", i);
                    Int32 nCrop = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "TO_CROP", i);
                    string sRatio = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "ratio_id", i);
                    string sNameEnd = nWidth.ToString() + "X" + nHeight.ToString();
                    bool bCrop = true;
                    if (nCrop == 0)
                        bCrop = false;
                    dr_upload.AddPicDimension(nWidth, nHeight, sNameEnd, bCrop, sRatio);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public string GetPreHeader()
        {
            try
            {
                Int32 nUpperGroup = int.Parse(GetTableSingleVal("accounts", "group_id", LoginManager.GetLoginID()).ToString());
                Int32 nCurrentGroup = LoginManager.GetLoginGroupID();
                bool bFirst = true;
                string sRet = "";
                bool bCont = true;
                while (bCont == true)
                {
                    if (nCurrentGroup == nUpperGroup || nCurrentGroup == 0)
                        bCont = false;
                    string sGroupName = PageUtils.GetTableSingleVal("groups", "group_name", nCurrentGroup).ToString();
                    if (bFirst == false)
                        sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_browse_as.aspx?group_id=" + nCurrentGroup.ToString() + "';\">" + sGroupName + "</span><span class=\"arrow\"> &raquo; </span>" + sRet;
                    else
                        sRet = sGroupName;
                    bFirst = false;
                    nCurrentGroup = int.Parse(PageUtils.GetTableSingleVal("groups", "parent_group_id", nCurrentGroup).ToString());
                }
                return sRet;
            }
            catch
            {
                return "";
            }
        }

        static public bool IsTvinciUser()
        {
            Int32 nUpperGroup = int.Parse(GetTableSingleVal("accounts", "group_id", LoginManager.GetLoginID()).ToString());
            if (nUpperGroup == 1)
                return true;
            return false;
        }

        static public Int32 GetUpperGroupID(Int32 nGroupID)
        {
            if (nGroupID == 1)
                return 1;
            Int32 nParentGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", nGroupID, 86400).ToString());
            if (nParentGroupID == 1)
                return nGroupID;
            else
                return GetUpperGroupID(nParentGroupID);
        }
        static public Int32 GetUpperGroupID(Int32 nGroupID, string sConnectionKey)
        {
            if (nGroupID == 1)
                return 1;
            Int32 nParentGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", nGroupID, 86400, sConnectionKey).ToString());
            if (nParentGroupID == 1)
                return nGroupID;
            else
                return GetUpperGroupID(nParentGroupID, sConnectionKey);
        }

        static public string GetMiniParentsGroupsStr(Int32 nGroupID)
        {
            if (nGroupID == 0)
                return "";
            StringBuilder sRet = new StringBuilder();
            Int32 nParentGroupID = nGroupID;
            while (nParentGroupID != 1)
            {
                object oParentGroupID = ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", nParentGroupID, 86400);
                if (oParentGroupID != null && oParentGroupID != DBNull.Value)
                {
                    nParentGroupID = int.Parse(oParentGroupID.ToString());
                    if (nParentGroupID != 1)
                    {
                        if (sRet.ToString() != "")
                            sRet.Append(",");
                        sRet.Append(nParentGroupID);
                    }
                }
                else
                    break;
            }
            return sRet.ToString();
        }

        static public string GetGroupsStrByParent(Int32 nParentGroupID)
        {
            if (nParentGroupID == 0)
                return "";

            string groups = string.Empty;
            List<string> lGroups = new List<string>();
            DataTable dt = DAL.TvmDAL.GetChildGroupTreeStr(nParentGroupID);
            if (dt != null && dt.DefaultView.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    lGroups.Add(ODBCWrapper.Utils.GetSafeStr(dr["id"]));
                }
                if (lGroups.Count > 0)
                {
                    groups = string.Format(" in ( {0} ) ", string.Join(",", lGroups.ToArray()));
                }
                else
                {
                    groups = " in (0) ";
                }

            }
            return groups;
        }
        static public string GetConcatGroupsStrByParent(Int32 nParentGroupID)
        {
            if (nParentGroupID == 0)
                return "";
            StringBuilder sRet = new StringBuilder();
            sRet.Append(nParentGroupID);

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select g.id from groups g where g.PARENT_GROUP_ID = " + nParentGroupID.ToString();
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        string groupID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                        if (!groupID.Equals(nParentGroupID))
                        {
                            sRet.Append(",");
                            sRet.Append(groupID);
                        }
                    }
                }
            }

            return sRet.ToString();
        }

        public static List<int> GetGroupListByParent(int nParentGroupID)
        {
            List<int> lRes = new List<int>();

            if (nParentGroupID == 0)
                return lRes;

            lRes.Add(nParentGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select g.id from groups g where g.PARENT_GROUP_ID = " + nParentGroupID.ToString();
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        int groupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[i].Row, "Id");
                        if (groupID != nParentGroupID && groupID != 0)
                        {
                            lRes.Add(groupID);
                        }
                    }
                }
            }

            return lRes;
        }

        static public string GetParentsGroupsStr(Int32 nGroupID)
        {
            if (nGroupID == 0)
                return "";
            StringBuilder sRet = new StringBuilder();
            sRet.Append("in (").Append(nGroupID);
            Int32 nParentGroupID = nGroupID;
            while (nParentGroupID != 1)
            {
                object oParentGroupID = ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", nParentGroupID, 86400);
                if (oParentGroupID != null && oParentGroupID != DBNull.Value)
                {
                    nParentGroupID = int.Parse(oParentGroupID.ToString());
                    if (nParentGroupID != 1)
                    {
                        if (sRet.ToString() != "in (")
                            sRet.Append(",");
                        sRet.Append(nParentGroupID);
                    }
                }
                else
                    break;
            }
            if (sRet.ToString() == "in (")
                sRet.Append("0)");
            else
                sRet.Append(")");
            return sRet.ToString();
        }

        static public System.Data.DataColumn GetColumn(string sName, object defVal)
        {
            System.Data.DataColumn col1 = new System.Data.DataColumn(sName);
            col1.DataType = defVal.GetType();
            return col1;
        }

        static public bool DoesGeoBlockTypeIncludeCountry(Int32 nGeoBlockID, Int32 nCountryID)
        {
            Int32 nCO = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += "select count(*) as co from geo_block_types_countries (nolock) where STATUS=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GEO_BLOCK_TYPE_ID", "=", nGeoBlockID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            selectQuery += "and status=1";

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCO = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nCO > 0)
                return true;
            return false;
        }

        static public bool DoesPlayerRuleTypeIncludePlayer(Int32 nPlayerRuleID, Int32 nPlayerID)
        {
            Int32 nCO = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select count(*) as co from players_groups_types_groups (nolock) where STATUS=1 and ";
            selectQuery.SetCachedSec(86400);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("players_groups_type_ID", "=", nPlayerRuleID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("groups_passwords_ID", "=", nPlayerID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCO = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nCO > 0)
                return true;
            return false;
        }

        static public bool DoesWatchPermissionRuleOK(Int32 nWatchPermissionID, Int32 nGroup)
        {
            if (nWatchPermissionID == 0)
                return false;
            Int32 nRuleID = nWatchPermissionID;
            Int32 nONLY_OR_BUT = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("watch_permissions_types", "ONLY_OR_BUT", nRuleID, 86400).ToString());
            bool bOK = false;
            bool bExsitInRuleM2M = DoesWatchPermissionTypeIncludesGroup(nRuleID, nGroup);
            //No one except
            if (nONLY_OR_BUT == 0)
                bOK = bExsitInRuleM2M;
            //All except
            if (nONLY_OR_BUT == 1)
                bOK = !bExsitInRuleM2M;
            return bOK;
        }

        static public bool DoesPlayerRuleOK(Int32 nPlayerRuleID, Int32 nPlayerID)
        {
            if (nPlayerRuleID == 0)
                return true;
            Int32 nRuleID = nPlayerRuleID;
            Int32 nONLY_OR_BUT = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("players_groups_types", "ONLY_OR_BUT", nRuleID, 86400).ToString());
            bool bOK = false;
            bool bExsitInRuleM2M = DoesPlayerRuleTypeIncludePlayer(nRuleID, nPlayerID);
            //No one except
            if (nONLY_OR_BUT == 0)
                bOK = bExsitInRuleM2M;
            //All except
            if (nONLY_OR_BUT == 1)
                bOK = !bExsitInRuleM2M;
            return bOK;
        }

        static public bool DoesWatchPermissionTypeIncludesGroup(Int32 nWatchPermissionID, Int32 nGroup, string sConnectionKey)
        {
            if (nWatchPermissionID == 0)
                return true;
            Int32 nCO = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery.SetConnectionKey(sConnectionKey);
            selectQuery += "select count(*) as co from watch_permissions_types_groups (nolock) where STATUS=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("watch_permissions_type_ID", "=", nWatchPermissionID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroup);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCO = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nCO > 0)
                return true;
            return false;
        }

        static public bool DoesWatchPermissionTypeIncludesGroup(Int32 nWatchPermissionID, Int32 nGroup)
        {
            return DoesWatchPermissionTypeIncludesGroup(nWatchPermissionID, nGroup, string.Empty);
        }

        static protected void GetPermittedWatchRulesID(Int32 nCurrentGroupID, Int32 nOwnerGroupID, ref string sIDs)
        {
            GetPermittedWatchRulesID(nCurrentGroupID, nOwnerGroupID, ref sIDs);
        }
        static protected void GetPermittedWatchRulesID(Int32 nCurrentGroupID, Int32 nOwnerGroupID, ref string sIDs, string sConnectionKey)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(sConnectionKey);
            selectQuery += "select * from watch_permissions_types wpt (nolock) where STATUS=1 and is_active=1 and ";
            selectQuery.SetCachedSec(86400);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nOwnerGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nRuleID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    Int32 nONLY_OR_BUT = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ONLY_OR_BUT"].ToString());
                    bool bAdd = false;
                    bool bExsitInRuleM2M = DoesWatchPermissionTypeIncludesGroup(nRuleID, nCurrentGroupID, sConnectionKey);
                    //No one except
                    if (nONLY_OR_BUT == 0)
                        bAdd = bExsitInRuleM2M;
                    //All except
                    if (nONLY_OR_BUT == 1)
                        bAdd = !bExsitInRuleM2M;

                    if (bAdd == true)
                    {
                        if (sIDs != "")
                            sIDs += ",";
                        sIDs += nRuleID.ToString();
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected void GetPermittedWatchRulesIDForGroup(Int32 nCurrentGroupID, Int32 nParentGroupID, ref string sIDs)
        {
            GetPermittedWatchRulesIDForGroup(nCurrentGroupID, nParentGroupID, ref sIDs, string.Empty);
        }
        static protected void GetPermittedWatchRulesIDForGroup(Int32 nCurrentGroupID, Int32 nParentGroupID, ref string sIDs, string sConnectionKey)
        {
            if (nCurrentGroupID != nParentGroupID)
                GetPermittedWatchRulesID(nCurrentGroupID, nParentGroupID, ref sIDs, sConnectionKey);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(sConnectionKey);
            selectQuery += "select id from groups (nolock) where status=1 and is_active=1 and ";
            selectQuery.SetCachedSec(86400);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PARENT_GROUP_ID", "=", nParentGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nG = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                    GetPermittedWatchRulesIDForGroup(nCurrentGroupID, nG, ref sIDs, sConnectionKey);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public string GetCallerIP()
        {
            string sIP = "";
            if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                sIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Trim();

            //if (sIP == "" || sIP.ToLower() == "unknown")
            //sIP = HttpContext.Current.Request.UserHostAddress.Trim();

            if (sIP == "" || sIP.ToLower() == "unknown")
                sIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].Trim();

            ////Only for staging TVM!!!!!
            //if (sIP.StartsWith("192.168.16"))
            //{
            //    sIP = "80.179.194.132";
            //}
            return sIP;
        }

        static public Int32 GetIPCountry2(string sIP)
        {
            if (string.IsNullOrEmpty(sIP))
            {
                return GetIPCountry2();
            }
            else
            {
                if (HttpContext.Current.Session["tvinci_geo_" + sIP] != null)
                {
                    try
                    {
                        return int.Parse(HttpContext.Current.Session["tvinci_geo_" + sIP].ToString());
                    }
                    catch { }
                }

                return GetIPCountry2NoCache(sIP);
                //return nCountry;
            }
        }

        static public Int32 GetIPCountry2NoCache(string sIP)
        {
            Int32 nCountry = 0;
            if (sIP == "127.0.0.1" || sIP == "::1")
                nCountry = 18;
            else if (sIP != "")
            {
                string[] splited = sIP.Split('.');

                Int64 nIPVal = Int64.Parse(splited[3]) + Int64.Parse(splited[2]) * 256 + Int64.Parse(splited[1]) * 256 * 256 + Int64.Parse(splited[0]) * 256 * 256 * 256;
                Int32 nID = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select top 1 COUNTRY_ID,ID from ip_to_country (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_FROM", "<=", nIPVal);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_TO", ">=", nIPVal);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nCountry = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COUNTRY_ID"].ToString().ToLower());
                        nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString().ToLower());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            HttpContext.Current.Session["tvinci_geo_" + sIP] = nCountry;
            return nCountry;
        }

        static public Int32 GetIPCountry2()
        {
            string sIP = GetCallerIP();
            return GetIPCountry2(sIP);
        }



        static public string GetPermittedWatchRulesID(Int32 nGroupID)
        {
            return GetPermittedWatchRulesID(nGroupID, string.Empty);
        }
        static public string GetPermittedWatchRulesID(Int32 nGroupID, string sConnectionKey)
        {
            Int32 nUpeerGroup = GetUpperGroupID(nGroupID, sConnectionKey);
            string sIDs = "";
            GetPermittedWatchRulesIDForGroup(nGroupID, nUpeerGroup, ref sIDs, sConnectionKey);
            return sIDs;
        }

        static public bool DoesGroupIsParentOfGroup(Int32 nGroupToCheck)
        {
            bool bYes = false;
            Int32 nUpperGroup = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("accounts", "group_id", LoginManager.GetLoginID(), 86400).ToString());
            if (nGroupToCheck == nUpperGroup)
                return true;
            DoesGroupIsParentOfGroup(nUpperGroup, nGroupToCheck, ref bYes);
            return bYes;
        }

        static public Int32 GetStringMetaIDByMetaName(Int32 nGroupID, string sMetaName)
        {
            Int32 nMetaID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 1; i < 21; i++)
                    {
                        object oCurMetaName = selectQuery.Table("query").DefaultView[0].Row["META" + i.ToString() + "_STR_NAME"];
                        if (oCurMetaName != DBNull.Value && oCurMetaName != null)
                        {
                            if (sMetaName.Trim().ToLower() == oCurMetaName.ToString().Trim().ToLower())
                                nMetaID = i;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nMetaID;
        }

        static public Int32 GetDoubleMetaIDByMetaName(Int32 nGroupID, string sMetaName)
        {
            Int32 nMetaID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 1; i < 11; i++)
                    {
                        object oCurMetaName = selectQuery.Table("query").DefaultView[0].Row["META" + i.ToString() + "_DOUBLE_NAME"];
                        if (oCurMetaName != DBNull.Value && oCurMetaName != null)
                        {
                            if (sMetaName.Trim().ToLower() == oCurMetaName.ToString().Trim().ToLower())
                                nMetaID = i;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nMetaID;
        }

        static public Int32 GetBoolMetaIDByMetaName(Int32 nGroupID, string sMetaName)
        {
            Int32 nMetaID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 1; i < 11; i++)
                    {
                        object oCurMetaName = selectQuery.Table("query").DefaultView[0].Row["META" + i.ToString() + "_BOOL_NAME"];
                        if (oCurMetaName != DBNull.Value && oCurMetaName != null)
                        {
                            if (sMetaName.Trim().ToLower() == oCurMetaName.ToString().Trim().ToLower())
                                nMetaID = i;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nMetaID;
        }

        static public void DoesGroupIsParentOfGroup(Int32 nParentGroupID, Int32 nGroupToCheck, ref bool bYes)
        {
            if (bYes == true)
                return;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from groups (nolock) where ";
            selectQuery.SetCachedSec(86400);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_group_id", "=", nParentGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nGID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                    if (nGID == nGroupToCheck)
                    {
                        bYes = true;
                    }
                    else
                    {
                        DoesGroupIsParentOfGroup(nGID, nGroupToCheck, ref bYes);
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public Int32 GetGroupByUNPass(string sUN, string sPass, ref Int32 nPlayerID)
        {
            Int32 nGroupID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select group_id,id from groups_passwords (nolock) where is_active=1 and status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUN.ToLower().Trim());
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPass.ToLower().Trim());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                    var idObj = selectQuery.Table("query").DefaultView[0].Row["id"];
                    if (idObj != null)
                        nPlayerID = int.Parse(idObj.ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            // string sHost = PageUtils.GetTableSingleVal("groups", "GROUP_NAME", nGroupID).ToString();
            return nGroupID;
        }

        static public bool DoesStringSecurityValid(string sToChek)
        {
            sToChek = sToChek.ToLower();
            if (sToChek.IndexOf("onload") != -1 ||
                sToChek.IndexOf("script") != -1 ||
                sToChek.IndexOf("onmouse") != -1 ||
                sToChek.IndexOf("onunload") != -1 ||
                sToChek.IndexOf("onkey") != -1 ||
                sToChek.IndexOf("ajax") != -1)
                return false;
            return true;
        }

        static public Int32 GetGroupIDByDomain()
        {
            Int32 nGroupID = 0;
            string sHost = "";
            if (HttpContext.Current.Request.ServerVariables["REMOTE_HOST"] != null)
                sHost = HttpContext.Current.Request.ServerVariables["REMOTE_HOST"].ToLower();
            log.Debug("Domain - " + sHost);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += "select group_id from groups_domains (nolock) where is_active=1 and status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DOMAIN", "=", sHost.ToLower().Trim());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nGroupID;
        }

        static public void SendBugMail(Int32 nBugID, string sAction, string sTemplate, bool bWithOpener)
        {
            string sProject = PageUtils.GetTableSingleVal("bs_projects", "NAME", int.Parse(HttpContext.Current.Session["project_id"].ToString())).ToString();
            string sMessage = "";
            string sBaseName = "";
            if (sTemplate == "FeatureReport.html")
                sBaseName = "feature";
            else
                sBaseName = "bug";

            if (sAction == "New")
                sMessage = "A new " + sBaseName + " entered to the " + sBaseName + " system";
            if (sAction == "Update")
                sMessage = "The " + sBaseName + " was updated";
            if (sAction == "Close")
                sMessage = "The " + sBaseName + " was closed";
            if (sAction == "Reopen")
                sMessage = "The " + sBaseName + " was reopened";

            string sID = nBugID.ToString();
            string sProjectID = HttpContext.Current.Session["project_id"].ToString();
            string sCreateDate = "";
            string sShortDescription = "";
            string sSavirity = "";
            string sDepartment = "";
            string sStatus = "";
            string sReporter = "";
            string sAssigned = "";
            string sDescription = "";
            string sDescriptionToClient = "";
            string sRecreate = "";
            string sCloseDate = "";
            string sCloseVersion = "";
            string sCloser = "";
            string sCloseDesc = "";
            string sReopenDate = "";
            string sReopenDesc = "";
            string sReopener = "";
            Int32 nReporter = 0;
            Int32 nAssigned = 0;
            Int32 nCloser = 0;
            Int32 nReopener = 0;
            string sFiles = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from bs_project_bugs where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nBugID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    if (selectQuery.Table("query").DefaultView[0].Row["CREATE_DATE"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["CREATE_DATE"] != null)
                        sCreateDate = DateUtils.GetStrFromDate((DateTime)(selectQuery.Table("query").DefaultView[0].Row["CREATE_DATE"])); ;
                    sShortDescription = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["NAME"]);
                    Int32 nSavirity = int.Parse(selectQuery.Table("query").DefaultView[0].Row["SAVIRITY_ID"].ToString());
                    sSavirity = ODBCWrapper.Utils.GetTableSingleVal("lu_savirity", "description", nSavirity).ToString();

                    Int32 nDepartment = int.Parse(selectQuery.Table("query").DefaultView[0].Row["BUG_FIELD_ID"].ToString());
                    sDepartment = ODBCWrapper.Utils.GetTableSingleVal("lu_bugs_fields", "description", nDepartment).ToString();

                    Int32 nStatus = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CARE_STATUS"].ToString());
                    sStatus = ODBCWrapper.Utils.GetTableSingleVal("lu_care_status", "description", nStatus).ToString();

                    nReporter = int.Parse(selectQuery.Table("query").DefaultView[0].Row["REPORTER_ACCOUNT_ID"].ToString());
                    if (nReporter != 0)
                        sReporter = ODBCWrapper.Utils.GetTableSingleVal("accounts", "username", nReporter).ToString();

                    nAssigned = int.Parse(selectQuery.Table("query").DefaultView[0].Row["RESPONSIBLE_ACCOUNT_ID"].ToString());
                    if (nAssigned != 0)
                        sAssigned = ODBCWrapper.Utils.GetTableSingleVal("accounts", "username", nAssigned).ToString();

                    sDescription = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["DESCRIPTION"]).Replace("\r\n", "<br/>");
                    sDescriptionToClient = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["DESCRIPTION_TO_CLIENT"]).Replace("\r\n", "<br/>");
                    sRecreate = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["RECREATE_DESC"]).Replace("\r\n", "<br/>");

                    if (selectQuery.Table("query").DefaultView[0].Row["CLOSE_DATE"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["CLOSE_DATE"] != null)
                        sCloseDate = DateUtils.GetStrFromDate((DateTime)(selectQuery.Table("query").DefaultView[0].Row["CLOSE_DATE"])); ;

                    sCloseVersion = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["CLOSE_VERSION"]);

                    nCloser = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CLOSER_ACCOUNT_ID"].ToString());
                    if (nCloser != 0)
                        sCloser = ODBCWrapper.Utils.GetTableSingleVal("accounts", "username", nCloser).ToString();

                    sCloseDesc = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["CLOSE_DESCRIPTION"]).Replace("\r\n", "<br/>");

                    if (selectQuery.Table("query").DefaultView[0].Row["REOPEN_DATE"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["REOPEN_DATE"] != null)
                        sReopenDate = DateUtils.GetStrFromDate((DateTime)(selectQuery.Table("query").DefaultView[0].Row["REOPEN_DATE"])); ;

                    sReopenDesc = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["REOPEN_DESCRIPTION"]).Replace("\r\n", "<br/>");

                    nReopener = int.Parse(selectQuery.Table("query").DefaultView[0].Row["REOPENER_ACCOUNT_ID"].ToString());
                    if (nReopener != 0)
                        sReopener = ODBCWrapper.Utils.GetTableSingleVal("accounts", "username", nCloser).ToString();

                    string sFile = "";
                    if (selectQuery.Table("query").DefaultView[0].Row["FILE1"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["FILE1"] != null)
                        sFile = selectQuery.Table("query").DefaultView[0].Row["FILE1"].ToString();
                    if (sFile != "")
                    {
                        if (sFiles != "")
                            sFiles += ", ";
                        sFiles += "<a href=\"" + "https://admin.tvinci.com/bugs/" + sFile + "\" target=\"_blank\">" + sFile + "</a>";
                    }

                    if (selectQuery.Table("query").DefaultView[0].Row["FILE2"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["FILE2"] != null)
                        sFile = selectQuery.Table("query").DefaultView[0].Row["FILE2"].ToString();
                    if (sFile != "")
                    {
                        if (sFiles != "")
                            sFiles += ", ";
                        sFiles += "<a href=\"" + "https://admin.tvinci.com/bugs/" + sFile + "\" target=\"_blank\">" + sFile + "</a>";
                    }

                    if (selectQuery.Table("query").DefaultView[0].Row["FILE3"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["FILE3"] != null)
                        sFile = selectQuery.Table("query").DefaultView[0].Row["FILE3"].ToString();
                    if (sFile != "")
                    {
                        if (sFiles != "")
                            sFiles += ", ";
                        sFiles += "<a href=\"" + "https://admin.tvinci.com/bugs/" + sFile + "\" target=\"_blank\">" + sFile + "</a>";
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            string sMailData = GetBugsSendMailText(sProjectID, sProject, sMessage, sID, sCreateDate, sShortDescription, sSavirity,
                sDepartment, sStatus, sReporter, sAssigned, sDescription, sDescriptionToClient, sRecreate, sCloseDate,
                sCloseVersion, sCloser, sCloseDesc, sReopenDate, sReopenDesc, sReopener, sFiles, sTemplate);

            Mailer t = new Mailer(1);
            string sEmail = "";
            if (bWithOpener == false)
                sEmail = GetBugsEmails(int.Parse(sID), int.Parse(HttpContext.Current.Session["project_id"].ToString()), nReporter, nAssigned, nCloser, nReopener, bWithOpener);
            else
                sEmail = GetBugsEmails(int.Parse(sID), 0, nReporter, 0, 0, nReopener, bWithOpener);
            sEmail = MergeEmail(sEmail, "support@tvinci.com");
            string sMailHeader = "";
            if (sTemplate == "FeatureReport.html")
                sMailHeader = "Feature number: " + sID + "( " + sAction + ")";
            else if (sTemplate == "BagReport.html")
                sMailHeader = "Bug number: " + sID + "( " + sAction + ")";
            else if (sTemplate == "BagReportToClient.html")
                sMailHeader = "Bug ID: " + sID + " - Handling progress";
            else
                sMailHeader = "Feature ID: " + sID + " - Handling progress";
            t.SendMail(sEmail, "", sMailData, sMailHeader, "TVM Bug/Features System", "support@tvinci.com");
        }

        static public void SendGroupBugMail(Int32 nBugID, string sAction, string sTemplate)
        {
            string sMessage = "";
            sMessage = "Your bug/feature message has entered to the TVM database";

            string sID = nBugID.ToString();
            Int32 nReporter = 0;
            string sCreateDate = "";
            string sShortDescription = "";
            string sSavirity = "";
            string sReporter = "";
            string sDescription = "";
            string sRecreate = "";
            string sFiles = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from bs_project_bugs where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nBugID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    if (selectQuery.Table("query").DefaultView[0].Row["CREATE_DATE"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["CREATE_DATE"] != null)
                        sCreateDate = DateUtils.GetStrFromDate((DateTime)(selectQuery.Table("query").DefaultView[0].Row["CREATE_DATE"])); ;
                    sShortDescription = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["NAME"]);
                    Int32 nSavirity = int.Parse(selectQuery.Table("query").DefaultView[0].Row["SAVIRITY_ID"].ToString());
                    sSavirity = ODBCWrapper.Utils.GetTableSingleVal("lu_savirity", "description", nSavirity).ToString();

                    nReporter = int.Parse(selectQuery.Table("query").DefaultView[0].Row["REPORTER_ACCOUNT_ID"].ToString());
                    if (nReporter != 0)
                        sReporter = ODBCWrapper.Utils.GetTableSingleVal("accounts", "username", nReporter).ToString();

                    sDescription = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["DESCRIPTION"]).Replace("\r\n", "<br/>");
                    sRecreate = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["RECREATE_DESC"]).Replace("\r\n", "<br/>");

                    string sFile = "";
                    if (selectQuery.Table("query").DefaultView[0].Row["FILE1"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["FILE1"] != null)
                        sFile = selectQuery.Table("query").DefaultView[0].Row["FILE1"].ToString();
                    if (sFile != "")
                    {
                        if (sFiles != "")
                            sFiles += ", ";
                        sFiles += "<a href=\"" + "https://admin.tvinci.com/bugs/" + sFile + "\" target=\"_blank\">" + sFile + "</a>";
                    }

                    if (selectQuery.Table("query").DefaultView[0].Row["FILE2"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["FILE2"] != null)
                        sFile = selectQuery.Table("query").DefaultView[0].Row["FILE2"].ToString();
                    if (sFile != "")
                    {
                        if (sFiles != "")
                            sFiles += ", ";
                        sFiles += "<a href=\"" + "https://admin.tvinci.com/bugs/" + sFile + "\" target=\"_blank\">" + sFile + "</a>";
                    }

                    if (selectQuery.Table("query").DefaultView[0].Row["FILE3"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["FILE3"] != null)
                        sFile = selectQuery.Table("query").DefaultView[0].Row["FILE3"].ToString();
                    if (sFile != "")
                    {
                        if (sFiles != "")
                            sFiles += ", ";
                        sFiles += "<a href=\"" + "https://admin.tvinci.com/bugs/" + sFile + "\" target=\"_blank\">" + sFile + "</a>";
                    }

                }
            }
            selectQuery.Finish();
            selectQuery = null;
            string sMailData = GetBugsSendMailText("0", "", sMessage, sID, sCreateDate, sShortDescription, sSavirity,
                "", "", sReporter, "", sDescription, "", sRecreate, "",
                "", "", "", "", "", "", sFiles, sTemplate);

            Mailer t = new Mailer(1);
            string sEmail = GetBugsEmails(int.Parse(sID), 0, nReporter, 0, 0, nReporter, true);
            sEmail = MergeEmail(sEmail, "support@tvinci.com");
            string sMailHeader = "";
            sMailHeader = sMessage;
            t.SendMail(sEmail, "", sMailData, sMailHeader, "TVM Bug/Features System", "support@tvinci.com");
        }

        static public string MergeEmail(string sEmailLine, Int32 nAccountID)
        {
            string sEmail = GetSafeAccountMail(nAccountID);
            return MergeEmail(sEmailLine, sEmail);
        }

        static public string MergeEmail(string sEmailLine, string sEmail)
        {
            if (sEmailLine.IndexOf(sEmail) != -1)
                return sEmailLine;
            if (sEmailLine != "")
                sEmailLine += ";";
            sEmailLine += sEmail;
            return sEmailLine;
        }

        static public string GetSafeAccountMail(Int32 nAccountID)
        {
            return ODBCWrapper.Utils.GetSafeStr(ODBCWrapper.Utils.GetTableSingleVal("accounts", "email_add", nAccountID));
        }

        static protected string GetBugsEmails(Int32 nBugID, Int32 nProjectID, Int32 nReporter, Int32 nAssigned, Int32 nCloser, Int32 nReopener, bool bWithOpener)
        {
            string sRet = "";
            if (nProjectID != 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select a.email_add from bs_projects_accounts bpa,accounts a where bpa.status=1 and a.status=1 and bpa.account_id=a.id and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("bpa.project_id", "=", nProjectID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sEmail = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[i].Row["email_add"]);
                        if (sEmail != "")
                        {
                            if (sRet != "")
                                sRet += ";";
                            sRet += sEmail;
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            if (bWithOpener == true)
                sRet = MergeEmail(sRet, nReporter);
            sRet = MergeEmail(sRet, nAssigned);
            sRet = MergeEmail(sRet, nCloser);
            sRet = MergeEmail(sRet, nReopener);
            return sRet;
        }

        static protected string GetBugsSendMailText(string sProjectID, string sProject, string sMessage, string sID, string sCreateDate,
                    string sShortDescription, string sSavirity, string sDepartment, string sStatus,
                    string sReporter, string sAssigned, string sDescription, string sDescriptionToClient, string sRecreate,
                    string sCloseDate, string sCloseVersion, string sCloser,
                    string sCloseDesc, string sReopenDate, string sReopenDesc, string sReopener, string sFiles, string sTemplate)
        {
            MailTemplateEngine mt = new MailTemplateEngine();
            string sFilePath = HttpContext.Current.Server.MapPath("");
            sFilePath += "/mailTemplates/" + sTemplate;
            mt.Init(sFilePath);
            mt.Replace("PROJECT_ID", sProjectID);
            mt.Replace("BUG_PROJECT", sProject);
            mt.Replace("BUG_MESSAGE", sMessage);
            mt.Replace("BUG_ID", sID);
            mt.Replace("BUG_CREATE_DATE", sCreateDate);
            mt.Replace("BUG_SHORT_DESCRIPTION", sShortDescription);
            mt.Replace("BUG_SAVIRITY", sSavirity);
            mt.Replace("BUG_DEPARTMENT", sDepartment);
            mt.Replace("BUG_STATUS", sStatus);
            mt.Replace("BUG_REPORTER", sReporter);
            mt.Replace("BUG_ASSIGNED", sAssigned);
            mt.Replace("BUG_DESCRIPTION", sDescription);
            mt.Replace("BUG_DESCRIPTION_TO_CLIENT", sDescriptionToClient);
            mt.Replace("BUG_RECREATE", sRecreate);
            mt.Replace("BUG_CLOSE_DATE", sCloseDate);
            mt.Replace("BUG_CLOSE_VERSION", sCloseVersion);
            mt.Replace("BUG_CLOSER", sCloser);
            mt.Replace("BUG_CLOSE_DESC", sCloseDesc);
            mt.Replace("BUG_REOPEN_DATE", sReopenDate);
            mt.Replace("BUG_REOPEN_DESC", sReopenDesc);
            mt.Replace("BUG_REOPENER", sReopener);
            mt.Replace("BUG_LINKS", sFiles);
            string sMailData = mt.GetAsString();

            return sMailData;
        }


        //get the parent groupID + regular groupID
        public static string GetRegularChildGroupsStr(int nGroupID, string sConnKey)
        {
            string groups = string.Empty;
            List<string> lGroups = new List<string>();
            DataTable dt = DAL.NotificationDal.GetRegularChildGroupsStr(nGroupID);
            if (dt != null && dt.DefaultView.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    lGroups.Add(ODBCWrapper.Utils.GetSafeStr(dr["group_id"]));
                }
                groups = string.Join(",", lGroups.ToArray());
            }
            return groups;
        }

        public static string BuildEpgUrl(int groupId, string baseUrl, int version = 0, int width = 0, int height = 0, int quality = 100)
        {
            string url = string.Empty;

            string imageId = Path.GetFileNameWithoutExtension(baseUrl);
            if (string.IsNullOrEmpty(imageId))
            {
                log.Error("Image ID is empty");
                return url;
            }

            url = ImageUtils.BuildImageUrl(groupId, imageId, version, width, height, quality);
            return url;
        }

        public static string BuildVodUrl(int groupId, string baseUrl, int ratioId, int version = 0, int width = 0, int height = 0, int quality = 100)
        {
            string url = string.Empty;

            string imageId = Path.GetFileNameWithoutExtension(baseUrl);
            if (string.IsNullOrEmpty(imageId))
            {
                log.Error("Image ID is empty");
                return url;
            }
            else
                imageId += "_" + ratioId;

            url = ImageUtils.BuildImageUrl(groupId, imageId, version, width, height, quality);
            return url;
        }

        public static string GetPicImageUrlByRatio(int picId, int width = 0, int height = 0, int? groupId = null)
        {
            string imageUrl = string.Empty;
            string baseUrl = string.Empty;
            int ratioId = 0;
            int version = 0;

            if (!groupId.HasValue)
            {
                groupId = LoginManager.GetLoginGroupID();
            }

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select p.RATIO_ID, p.BASE_URL, p.VERSION from pics p where ID = " + picId.ToString();

            if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
            {
                baseUrl = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["BASE_URL"]);
                ratioId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["RATIO_ID"]);
                version = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["VERSION"]);
                int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId.Value);

                imageUrl = PageUtils.BuildVodUrl(parentGroupID, baseUrl, ratioId, version, width, height);
            }
            else
            {
                log.ErrorFormat("GetPicImageUrlByRatio imageUrl is empty. PicId {0}", picId);
            }

            return imageUrl;
        }

        public static string GetPicImageUrlByRatio(int assetId, eAssetImageType asssetImageType, int ratioId, int width = 0, int height = 0)
        {
            string imageUrl = string.Empty;
            string baseUrl = string.Empty;
            int version = 0;
            int groupId = LoginManager.GetLoginGroupID();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select p.RATIO_ID, p.BASE_URL, p.VERSION from pics p where ASSET_ID = " + assetId.ToString();
            selectQuery += "And ASSET_IMAGE_TYPE = " + ((int)asssetImageType).ToString();
            selectQuery += "And RATIO_ID = " + ratioId.ToString();
            selectQuery += "And STATUS = 1";

            if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
            {
                baseUrl = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["BASE_URL"]);
                ratioId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["RATIO_ID"]);
                version = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["VERSION"]);
                int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

                imageUrl = PageUtils.BuildVodUrl(parentGroupID, baseUrl, ratioId, version, width, height);
            }
            else
            {
                log.ErrorFormat("GetPicImageUrlByRatio imageUrl is empty. AssetId {0}, AsssetImageType {1}", assetId, (int)asssetImageType);
            }

            return imageUrl;
        }

        public static string GetEpgPicImageUrl(int picId, int width = 0, int height = 0)
        {
            string imageUrl = string.Empty;
            string baseUrl = string.Empty;
            int version = 0;
            int groupId = LoginManager.GetLoginGroupID();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select p.BASE_URL, p.ID, p.version from epg_pics p where p.id = " + picId.ToString();

            if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
            {
                baseUrl = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["BASE_URL"]);
                picId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["ID"]);
                version = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["version"]);
                int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

                imageUrl = PageUtils.BuildEpgUrl(parentGroupID, baseUrl, version, width, height);
            }

            return imageUrl;
        }

        public static string GetEpgPicImageUrl(string epgIdentifier, int channelId, int rationId, int width = 0, int height = 0)
        {
            string imageUrl = string.Empty;
            string baseUrl = string.Empty;
            int version = 0;
            int groupId = LoginManager.GetLoginGroupID();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select top 1 ep.BASE_URL, ep.ID, ep.version from epg_pics ep inner join epg_multi_pictures emp on emp.pic_id = ep.ID ";
            selectQuery += " Where emp.epg_Identifier  = '" + epgIdentifier + "' ";
            selectQuery += " And emp.channel_id  = " + channelId.ToString();
            selectQuery += " And emp.ratio_id  = " + rationId.ToString();
            selectQuery += " And ep.status  = 1";
            selectQuery += " Order by ep.id desc";

            if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
            {
                baseUrl = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["BASE_URL"]);
                version = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["version"]);
                int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

                imageUrl = PageUtils.BuildEpgUrl(parentGroupID, baseUrl, version, width, height);
            }

            return imageUrl;
        }

        public static string GetEpgChannelsSchedulePicImageUrlByScheduleId(string epgChannelsScheduleId, string channelId, out int picId)
        {
            string imageUrl = string.Empty;
            picId = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += string.Format("SELECT top 1 ep.base_url, ep.ID, ep.version,g.parent_group_id  FROM epg_pics ep (NOLOCK) " +
                 " INNER JOIN epg_multi_pictures emp (NOLOCK)  on emp.pic_id = ep.ID " +
                 " INNER JOIN [epg_channels_schedule] ecs (NOLOCK) on ecs.epg_Identifier =  emp.epg_Identifier and ecs.EPG_CHANNEL_ID = emp.channel_id " +
                 " INNER JOIN groups g (NOLOCK) " +
                 " ON g.Id = ecs.Group_Id " +
                 " WHERE ecs.id  = {0}" +
                 " 	AND  ecs.EPG_CHANNEL_ID  = {1}  " +
                 " 	AND  emp.ratio_id= g.EPG_RATIO_ID " +
                 " 	And ep.status  = 1  " +
                 " 	ORDER BY ep.id", epgChannelsScheduleId, channelId);

            if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
            {
                imageUrl = GetEpgImageUrl(selectQuery, out picId);
            }

            selectQuery.Finish();
            selectQuery = null;

            return imageUrl;
        }

        public static string GetEpgChannelsSchedulePicImageUrlByEpgIdentifier(string epgIdentifier, string channelId, out int picId)
        {
            string imageUrl = string.Empty;
            picId = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += string.Format("SELECT top 1 ep.base_url, ep.ID, ep.version,g.parent_group_id  FROM epg_pics ep (NOLOCK) " +
                 " INNER JOIN epg_multi_pictures emp (NOLOCK)  on emp.pic_id = ep.ID " +
                 " INNER JOIN [epg_channels_schedule] ecs (NOLOCK) on ecs.epg_Identifier =  emp.epg_Identifier and ecs.EPG_CHANNEL_ID = emp.channel_id " +
                 " INNER JOIN groups g (NOLOCK) " +
                 " ON g.Id = ecs.Group_Id " +
                 " WHERE ecs.epg_Identifier = '{0}' " +
                 " 	AND  ecs.EPG_CHANNEL_ID  = {1}  " +
                 " 	AND  emp.ratio_id= g.EPG_RATIO_ID " +
                 " 	And ep.status  = 1  " +
                 " 	ORDER BY ep.id", epgIdentifier, channelId);

            if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
            {
                imageUrl= GetEpgImageUrl(selectQuery, out picId);
            }

            selectQuery.Finish();
            selectQuery = null;

            return imageUrl;
        }

        private static string GetEpgImageUrl(ODBCWrapper.DataSetSelectQuery selectQuery, out int picId )
        {
            picId = 0;

            string baseUrl = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "BASE_URL", 0);
            picId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
            int version = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "version", 0);
            int parentGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "parent_group_id", 0);

            return PageUtils.BuildEpgUrl(parentGroupID, baseUrl, version);
        }
    }
}