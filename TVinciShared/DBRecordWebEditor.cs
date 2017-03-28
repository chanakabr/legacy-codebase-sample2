using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Collections.Generic;

namespace TVinciShared
{
    public class PicDimension
    {
        public Int32 m_nWidth;
        public Int32 m_nHeight;
        public string m_sNameEnd;
        public string m_ratio;
        public bool m_bCorp;
        public PicDimension(Int32 nWidth, Int32 nHeight, string sNameEnd, bool bCorp)
        {
            m_nWidth = nWidth;
            m_nHeight = nHeight;
            m_sNameEnd = sNameEnd;
            m_bCorp = bCorp;
        }

        public PicDimension(Int32 nWidth, Int32 nHeight, string sNameEnd, bool bCorp, string ratio)
        {
            m_nWidth = nWidth;
            m_nHeight = nHeight;
            m_sNameEnd = sNameEnd;
            m_bCorp = bCorp;
            m_ratio = ratio;
        }
    }

    public abstract class BaseDataRecordField
    {
        protected string m_sFieldHeader;
        protected string m_sInputCss;
        protected string m_sHeaderCss;
        protected string m_sFieldName;
        protected string m_sIndexFieldName;
        protected string m_sIndexFieldVal;
        protected string m_sStartValue;
        protected bool m_bStartValue;
        protected string m_sDefaultVal;
        protected bool m_bMust;
        protected Int32 m_nDefault;
        protected string m_sConnectionKey;
        protected bool ignore;
        protected int? m_mulFactor;

        protected BaseDataRecordField() { }

        public void Initialize(string sFieldHeader,
            string sHeaderCss,
            string sInputCss,
            string sFieldName,
            bool bMust)
        {
            m_bMust = bMust;
            m_sFieldHeader = sFieldHeader;
            m_sHeaderCss = sHeaderCss;
            m_sInputCss = sInputCss;
            m_sFieldName = sFieldName;
            m_sStartValue = "";
            m_bStartValue = false;
            m_sDefaultVal = "";
            m_nDefault = -1;
            m_sConnectionKey = "";
        }

        //added
        public void Initialize(string sFieldHeader,
           string sHeaderCss,
           string sInputCss,
           string sFieldName, string sValue,
           bool bMust)
        {
            m_bMust = bMust;
            m_sFieldHeader = sFieldHeader;
            m_sHeaderCss = sHeaderCss;
            m_sInputCss = sInputCss;
            m_sFieldName = sFieldName;
            m_sStartValue = sValue;
            m_bStartValue = false;
            m_sDefaultVal = "";
            m_nDefault = -1;
            m_sConnectionKey = "";
        }

        public void SetIgnore(bool ignore)
        {
            this.ignore = ignore;
        }

        public void SetConnectionKey(string sKey)
        {
            m_sConnectionKey = sKey;
        }

        public void setMulFactor(int mulFactor)
        {
            m_mulFactor = mulFactor;
        }

        public string GetConnectionKey()
        {
            return m_sConnectionKey;
        }

        public abstract string GetFieldHtml(long nID);
        public virtual string GetFieldHtml(long nID, ref Int32 nToAdd)
        {
            nToAdd = 1;
            return GetFieldHtml(nID);
        }
        public virtual string GetFieldType() { return ""; }
        public virtual void SetValue(string sVal)
        {
            m_sStartValue = sVal;
            m_bStartValue = true;
        }
        public void SetDefault(Int32 nDefault)
        {
            m_nDefault = nDefault;
        }

        protected void ConvertDBObjToStr(ref DataTable dt)
        {
            if (this.GetFieldType() == "date")
            {
                m_sStartValue = "";
                if (dt.DefaultView[0].Row[m_sFieldName] != DBNull.Value)
                {
                    DateTime t = (DateTime)(dt.DefaultView[0].Row[m_sFieldName]);
                    m_sStartValue = DateUtils.GetStrFromDate(t);
                }
            }
            else if (this.GetFieldType() == "time")
            {
                if (dt.DefaultView[0].Row[m_sFieldName] != DBNull.Value)
                {
                    DateTime t = (DateTime)(dt.DefaultView[0].Row[m_sFieldName]);

                    m_sStartValue = t.Hour.ToString() + ":" + t.Minute.ToString();

                }
                else
                    m_sStartValue = "00:00";
            }
            else if (this.GetFieldType() == "datetime")
            {
                if (dt.DefaultView[0].Row[m_sFieldName] == null ||
                    dt.DefaultView[0].Row[m_sFieldName].ToString() == "")
                    m_sStartValue = "";
                else
                {
                    DateTime t = (DateTime)(dt.DefaultView[0].Row[m_sFieldName]);
                    m_sStartValue = DateUtils.GetStrFromDate(t);
                    m_sStartValue += " ";
                    if (t.Minute.ToString().Length == 1)
                    {
                        m_sStartValue += t.Hour.ToString() + ":0" + t.Minute.ToString();
                    }
                    else
                    {
                        m_sStartValue += t.Hour.ToString() + ":" + t.Minute.ToString();
                    }
                }
            }
            else
                m_sStartValue = dt.DefaultView[0].Row[m_sFieldName].ToString();
        }


        //added
        //protected void ConvertObjToStr(ref object obj)
        //{
        //    m_sStartValue = "";

        //    {
        //        if (this.GetFieldType() == "date")
        //        {
        //           if (obj != null)
        //           {
        //            if (obj.GetType() == typeof(DateTime))
        //               m_sStartValue = DateUtils.GetStrFromDate((DateTime)obj);                    
        //           }
        //        }
        //        else if (this.GetFieldType() == "time")
        //        {
        //            if (obj != null)
        //            {
        //                DateTime t = (DateTime)obj;
        //                m_sStartValue = t.Hour.ToString() + ":" + t.Minute.ToString();
        //            }
        //            else
        //                m_sStartValue = "00:00";
        //        }
        //        else if (this.GetFieldType() == "datetime")
        //        {
        //            if ( obj != null || obj.ToString() == "")
        //                m_sStartValue = "";
        //            else
        //            {
        //                DateTime t = (DateTime) obj;
        //                m_sStartValue = DateUtils.GetStrFromDate(t);
        //                m_sStartValue += " ";
        //                m_sStartValue += t.Hour.ToString() + ":" + t.Minute.ToString();
        //            }
        //        }
        //        else
        //            m_sStartValue = obj.ToString();
        //    }
        //}

        public virtual void SetValue(string sTable, string sIndexFieldName, object oIndexFieldVal)
        {
            if (m_bStartValue == true || m_sFieldName == "")
                return;
            if (CachingManager.CachingManager.Exist("SetValue_" + sTable + "_" + sIndexFieldName + "_" + oIndexFieldVal.ToString() + "_" + m_sConnectionKey) == true)
            {
                DataTable dt = ((DataTable)(CachingManager.CachingManager.GetCachedData("SetValue_" + sTable + "_" + sIndexFieldName + "_" + oIndexFieldVal.ToString() + "_" + m_sConnectionKey)));
                ConvertDBObjToStr(ref dt);
                return;
            }
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(m_sConnectionKey);
            //selectQuery += "select " + m_sFieldName + " from " + sTable + " where ";
            selectQuery += "select * from " + sTable + " where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sIndexFieldName, "=", oIndexFieldVal);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    DataTable dt = selectQuery.Table("query");
                    ConvertDBObjToStr(ref dt);
                    /*
                    if (this.GetFieldType() == "date")
                    {
                        m_sStartValue = "";
                        if (selectQuery.Table("query").DefaultView[0].Row[m_sFieldName] != DBNull.Value)
                        {
                            DateTime t = (DateTime)(selectQuery.Table("query").DefaultView[0].Row[m_sFieldName]);
                            m_sStartValue = DateUtils.GetStrFromDate(t);
                        }
                    }
                    else if (this.GetFieldType() == "time")
                    {
                        if (selectQuery.Table("query").DefaultView[0].Row[m_sFieldName] != DBNull.Value)
                        {
                            DateTime t = (DateTime)(selectQuery.Table("query").DefaultView[0].Row[m_sFieldName]);
                            m_sStartValue = t.Hour.ToString() + ":" + t.Minute.ToString();
                        }
                        else
                            m_sStartValue = "00:00";
                    }
                    else if (this.GetFieldType() == "datetime")
                    {
                        if (selectQuery.Table("query").DefaultView[0].Row[m_sFieldName] == null ||
                            selectQuery.Table("query").DefaultView[0].Row[m_sFieldName].ToString() == "")
                            m_sStartValue = "";
                        else
                        {
                            DateTime t = (DateTime)(selectQuery.Table("query").DefaultView[0].Row[m_sFieldName]);
                            m_sStartValue = DateUtils.GetStrFromDate(t);
                            m_sStartValue += " ";
                            m_sStartValue += t.Hour.ToString() + ":" + t.Minute.ToString();
                        }
                    }
                    else
                        m_sStartValue = selectQuery.Table("query").DefaultView[0].Row[m_sFieldName].ToString();
                    */
                }
                CachingManager.CachingManager.SetCachedData("SetValue_" + sTable + "_" + sIndexFieldName + "_" + oIndexFieldVal.ToString() + "_" + m_sConnectionKey, selectQuery.Table("query").Copy(), 3600, System.Web.Caching.CacheItemPriority.Normal, 0, false);
            }
            selectQuery.Finish();
            selectQuery = null;
        }




    }

    public class DataRecordShortTextField : BaseDataRecordField
    {
        protected bool m_bEnabled;
        protected bool m_bIsPass;
        protected string m_sDir;
        protected long m_nWidth;
        protected long m_nMaxLength;
        protected int m_extID;
        protected string m_filedPrivateName;

        public DataRecordShortTextField(string sDir, bool bEnabled, long nWidth, long nMaxLength)
            : base()
        {
            m_bEnabled = bEnabled;
            m_sDir = sDir;
            m_nWidth = nWidth;
            m_nMaxLength = nMaxLength;
            m_bIsPass = false;
            m_extID = 0;
            m_filedPrivateName = string.Empty;
        }

        public DataRecordShortTextField(string sDir, bool bEnabled, long nWidth, long nMaxLength, int extID)
            : base()
        {
            m_bEnabled = bEnabled;
            m_sDir = sDir;
            m_nWidth = nWidth;
            m_nMaxLength = nMaxLength;
            m_bIsPass = false;
            m_extID = extID;
            m_filedPrivateName = string.Empty;
        }

        public void SetPassword()
        {
            m_bIsPass = true;
        }

        public void setFiledName(string name)
        {
            m_filedPrivateName = name;
        }

        public override string GetFieldHtml(long nID)
        {
            StringBuilder sTmp = new StringBuilder();
            sTmp.Append("<tr>");
            sTmp.Append("<td class='" + m_sHeaderCss + "' nowrap>");
            if (m_bMust == true)
                sTmp.Append("<span class=\"red\">*&nbsp;&nbsp;</span>");
            sTmp.Append(m_sFieldHeader);
            sTmp.Append("</td>");
            //sTmp += "<td width=10px nowrap></td>";
            sTmp.Append("<td class=\"align1\">");
            sTmp.Append(GetInnerFieldHtml(nID));

            sTmp.Append("</td>");
            //sTmp += "<td width=100% nowrap></td>";
            sTmp.Append("</tr>");
            return sTmp.ToString();
        }

        public string GetInnerFieldHtml(long nID)
        {
            StringBuilder sTmp = new StringBuilder();
            sTmp.Append("<input tabindex=\"").Append((nID + 1).ToString()).Append("\" class='").Append(m_sInputCss).Append("' name='").Append(nID.ToString()).Append("_val' type='");
            if (m_bIsPass == false)
                sTmp.Append("text' ");
            else
                sTmp.Append("password' ");
            if (m_bEnabled == false)
                sTmp.Append("readonly='readonly' ");
            sTmp.Append("dir='").Append(m_sDir).Append("' ");
            sTmp.Append("size=").Append(m_nWidth.ToString()).Append(" ");
            sTmp.Append("maxlength=").Append(m_nMaxLength.ToString()).Append(" ");
            m_sStartValue = HttpContext.Current.Server.HtmlDecode(m_sStartValue).Replace("\"", "''");
            if (m_sStartValue != "")
                sTmp.Append("value=\"").Append(m_sStartValue.ToString()).Append("\" ");
            sTmp.Append("/>");
            sTmp.Append("<input tabindex=\"2000\" tabindex=\"").Append((nID + 1).ToString()).Append("\" type='hidden' name='").Append(nID.ToString()).Append("_type' value='string'/>");
            sTmp.Append("<input tabindex=\"2000\" tabindex=\"").Append((nID + 1).ToString()).Append("\" type='hidden' name='").Append(nID.ToString()).Append("_must' value='").Append(m_bMust.ToString()).Append("'/>");
            sTmp.Append("<input tabindex=\"2000\" tabindex=\"").Append((nID + 1).ToString()).Append("\" type='hidden' name='").Append(nID.ToString()).Append("_field' value='").Append(m_sFieldName).Append("'/>");
            sTmp.Append("<input tabindex=\"2000\" tabindex=\"").Append((nID + 1).ToString()).Append("\" type='hidden' name='").Append(nID.ToString()).Append("_fieldName' value='").Append(m_filedPrivateName).Append("'/>");
            if (m_extID > 0)
            {
                sTmp.Append("<input tabindex=\"2000\" tabindex=\"").Append((nID + 1).ToString()).Append("\" type='hidden' name='").Append(nID.ToString()).Append("_ext' value='").Append(m_extID).Append("'/>");
            }
            return sTmp.ToString();
        }
    }

    public class DataRecordVideoViewerField : BaseDataRecordField
    {
        protected string m_sTable;
        protected Int32 m_nID;
        protected string m_sPreField;
        public DataRecordVideoViewerField(string sTable, Int32 nID)
            : base()
        {
            m_sTable = sTable;
            m_nID = nID;
            m_sPreField = "";
        }

        public void SetPreField(string sPre)
        {
            m_sPreField = sPre;
        }

        protected string GetLocalTNImage()
        {

            try
            {
                Int32 nMediaID = int.Parse(PageUtils.GetTableSingleVal("media_files", "MEDIA_ID", m_nID).ToString());
                object sBaseURL = PageUtils.GetTableSingleVal("media", "MEDIA_PIC_ID", nMediaID);
                if (sBaseURL != null && sBaseURL != DBNull.Value && sBaseURL.ToString() != "0")
                {
                    string sPicURL = PageUtils.GetTableSingleVal("pics", "base_url", int.Parse(sBaseURL.ToString())).ToString();
                    Int32 nGroupID = LoginManager.GetLoginGroupID();
                    object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
                    string sBasePicsURL = "";
                    if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
                        sBasePicsURL = oBasePicsURL.ToString();
                    if (sBasePicsURL == "")
                        sBasePicsURL = "pics";
                    else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
                        sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
                        sBasePicsURL = "http://" + sBasePicsURL;
                    bool bWithEnding = true;
                    if (sBasePicsURL.EndsWith("=") == false)
                        sBasePicsURL += "/";
                    else
                        bWithEnding = false;
                    sBasePicsURL += ImageUtils.GetTNName(sPicURL, "tn"); ;
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
                return "";
            }
            catch
            {
                return "";
            }
        }

        protected string GetLocalTNBigImage()
        {
            try
            {
                Int32 nMediaID = int.Parse(PageUtils.GetTableSingleVal("media_files", "MEDIA_ID", m_nID).ToString());
                object sBaseURL = PageUtils.GetTableSingleVal("media", "MEDIA_PIC_ID", nMediaID);
                //object sBaseURL = PageUtils.GetTableSingleVal(m_sTable, "MEDIA_PIC_ID", m_nID);
                if (sBaseURL != null && sBaseURL != DBNull.Value && sBaseURL.ToString() != "0")
                {
                    string sPicURL = PageUtils.GetTableSingleVal("pics", "base_url", int.Parse(sBaseURL.ToString())).ToString();
                    return ImageUtils.GetTNName(sPicURL, "467X350");
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        public string GetCDNCode()
        {
            string sBaseURL = "";
            string sSuplierField = "vid_t." + m_sPreField + "STREAMING_SUPLIER_ID";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select sc.CDN_STR_ID from streaming_companies sc,";
            selectQuery += m_sTable;
            selectQuery += " vid_t where " + sSuplierField + "=sc.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("vid_t.id", "=", m_nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sBaseURL = selectQuery.Table("query").DefaultView[0].Row["CDN_STR_ID"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sBaseURL;
        }

        protected string GetStreamCompCode()
        {
            object sCode = PageUtils.GetTableSingleVal(m_sTable, m_sFieldName, m_nID);
            if (sCode != null && sCode != DBNull.Value)
                return sCode.ToString();

            object sPicID = PageUtils.GetTableSingleVal(m_sTable, "REF_ID", m_nID);
            if (sPicID != null && sPicID != DBNull.Value)
            {
                Int32 nGroupID = LoginManager.GetLoginGroupID();
                return ProtocolsFuncs.GetPicURL(int.Parse(sPicID.ToString()), "full");
            }
            return "";
        }

        protected string GetStreamCompID()
        {
            string sSuplierField = m_sPreField + "STREAMING_SUPLIER_ID";
            object sCode = PageUtils.GetTableSingleVal(m_sTable, sSuplierField, m_nID);
            if (sCode != null && sCode != DBNull.Value)
                return sCode.ToString();
            return "";
        }

        protected string GetStreamBaseURLForPlayer()
        {
            string sBaseURL = "";
            string sSuplierField = "vid_t." + m_sPreField + "STREAMING_SUPLIER_ID";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select sc.VIDEO_BASE_URL from streaming_companies sc,";
            selectQuery += m_sTable;
            selectQuery += " vid_t where " + sSuplierField + "=sc.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("vid_t.id", "=", m_nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sBaseURL = selectQuery.Table("query").DefaultView[0].Row["VIDEO_BASE_URL"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sBaseURL;
        }

        protected string GetStreamBaseURL()
        {
            string sBaseURL = "";
            string sSuplierField = "vid_t." + m_sPreField + "STREAMING_SUPLIER_ID";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select sc.ADM_VIDEO_BASE_URL from streaming_companies sc,";
            selectQuery += m_sTable;
            selectQuery += " vid_t where " + sSuplierField + "=sc.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("vid_t.id", "=", m_nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sBaseURL = selectQuery.Table("query").DefaultView[0].Row["ADM_VIDEO_BASE_URL"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sBaseURL;
        }

        protected string GetStreamBaseTNURL()
        {
            string sBaseURL = "";
            string sSuplierField = "vid_t." + m_sPreField + "STREAMING_SUPLIER_ID";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select sc.THUMB_NAILS_BASE_URL from streaming_companies sc,";
            selectQuery += m_sTable;
            selectQuery += " vid_t where " + sSuplierField + "=sc.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("vid_t.id", "=", m_nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sBaseURL = selectQuery.Table("query").DefaultView[0].Row["THUMB_NAILS_BASE_URL"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sBaseURL;
        }

        public string GetPlayerTN(bool bLocal)
        {
            string sLocalImage = GetLocalTNImage();
            string sStreamID = GetStreamCompCode();
            string sBaseURL = GetStreamBaseTNURL();
            string sRet = "";
            if (bLocal == false || sLocalImage == "")
            {
                sRet = "<img SRC=\"" + GetPlayerTNSrc(bLocal);
                sRet += "\" style=\"border:solid 1px #333333;\"/>";
                if (sBaseURL != "" && sStreamID != "")
                    return sRet;
                else
                    return "";
            }
            else
            {
                sRet = "<img SRC=\"" + sLocalImage;
                sRet += "\" style=\"border:solid 1px #333333;\"/>";
                return sRet;
            }
        }

        public string GetPlayerTNSrc(bool bLocal)
        {
            string sLocalImage = GetLocalTNImage();
            string sStreamID = GetStreamCompCode();
            string sBaseURL = GetStreamBaseTNURL();
            string sRet = "";
            if (bLocal == false || sLocalImage == "")
            {
                sRet = sBaseURL;
                sRet += sStreamID;
                if (sBaseURL != "" && sStreamID != "")
                    return sRet;
                else
                    return "";
            }
            else
            {
                sRet = sLocalImage;
                return sRet;
            }

        }

        public string GetPlayerLocalTNSrc()
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
            string sBasePicsURL = "";
            if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
                sBasePicsURL = oBasePicsURL.ToString();
            if (sBasePicsURL == "")
                sBasePicsURL = "pics";
            else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
                sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
                sBasePicsURL = "http://" + sBasePicsURL;

            string sLocalImage = GetLocalTNImage();
            string sRet = "";
            if (sLocalImage != "")
                sRet = sBasePicsURL + "/" + sLocalImage;
            else
                sRet = sBasePicsURL + "/" + "default_channel.jpg";
            return sRet;
        }

        public string GetPlayerLocalTNBigSrc()
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
            string sBasePicsURL = "";
            if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
                sBasePicsURL = oBasePicsURL.ToString();
            if (sBasePicsURL == "")
                sBasePicsURL = "pics";
            else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
                sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
                sBasePicsURL = "http://" + sBasePicsURL;

            string sLocalImage = GetLocalTNBigImage();
            string sRet = "";
            if (sLocalImage != "")
                sRet = sBasePicsURL + "/" + sLocalImage;
            else
                sRet = sBasePicsURL + "/default_channel_big.jpg";
            return sRet;
        }

        public string GetFLVSrc()
        {
            string sStreamID = GetStreamCompCode();
            string sBaseURL = GetStreamBaseURLForPlayer();
            string sRet = sBaseURL;
            sRet += sStreamID;
            if (sBaseURL != "" && sStreamID != "")
                return sRet;
            return "";
        }

        public string GetPlayerSrc()
        {
            string sStreamID = GetStreamCompCode();
            string sBaseURL = GetStreamBaseURL();
            string sRet = sBaseURL;
            sRet += sStreamID;
            if (sStreamID != "")
                return sRet;
            return "";
        }

        public string GetPlayerFrame()
        {
            string sStreamID = GetStreamCompCode();
            //string sBaseURL = GetStreamBaseURL();
            //string sCDNID = GetStreamCompID();
            //string sCDNType = GetCDNCode();
            //string sIFRameLink = "adm_video_player.aspx?cdnid=" + sCDNID.ToString() + "&cdntype=" + sCDNType.ToString() + "&streamid=" + sStreamID.ToString();
            //string sTVBOXLink = "tvbox.aspx?clip=" + m_nID.ToString();
            string sRet = "";
            //sRet = "<IFRAME SRC=\"admin_player.aspx?player_type=video&size=big&cdn_type=" + sCDNType + "&flv=" + HttpContext.Current.Server.UrlEncode(GetFLVSrc()) + "&autoplay=true";
            sRet = "<IFRAME SRC=\"admin_player.aspx?player_type=video&size=big&media_file_id=" + m_nID.ToString() + "&autoplay=true";
            sRet += "\" WIDTH=\"480\" HEIGHT=\"400\" FRAMEBORDER=\"0\"></IFRAME>";
            if (sStreamID != "")
                return sRet;
            return "";
        }

        public string GetPlayerSmallFrame()
        {
            string sStreamID = GetStreamCompCode();
            //string sBaseURL = GetStreamBaseURL();
            //string sCDNID = GetStreamCompID();
            //string sCDNType = GetCDNCode();
            //string sIFRameLink = "adm_video_player.aspx?cdnid=" + sCDNID.ToString() + "&cdntype=" + sCDNType.ToString() + "&streamid=" + sStreamID.ToString();
            //string sTVBOXLink = "tvbox.aspx?clip=" + m_nID.ToString();
            string sRet = "";
            //sRet = "<IFRAME SRC=\"admin_player.aspx?player_type=video&size=small&cdn_type=" + sCDNType + "&flv=" + HttpContext.Current.Server.UrlEncode(GetFLVSrc()) + "&autoplay=false";
            sRet = "<IFRAME SRC=\"admin_player.aspx?player_type=video&size=small&media_file_id=" + m_nID.ToString() + "&autoplay=false";
            sRet += "\" WIDTH=\"200\" HEIGHT=\"175\" FRAMEBORDER=\"0\"></IFRAME>";
            if (sStreamID != "")
                return sRet;
            return "";
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += "<table>";
            //string sPicURL = GetPlayerTNSrc(true);
            //sTmp += "<tr><td>";
            //sTmp += "<img src=\"";
            //sTmp += sPicURL + "\" class=\"img_border\"/>";
            //sTmp += "</td></tr>";
            //sTmp += "<tr><td>";
            sTmp += GetPlayerFrame();
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='video_player'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "</td></tr></table>";
            sTmp += "</td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordMediaViewerField : BaseDataRecordField
    {
        protected Int32 m_nID;
        protected string m_sPreField;
        protected Int32 m_nMediaType;
        protected string m_sTable;
        public DataRecordMediaViewerField(string sPreField, Int32 nID)
            : base()
        {
            m_sTable = "media_files";
            m_sPreField = sPreField;
            m_nID = nID;
            object o = PageUtils.GetTableSingleVal(m_sTable, "MEDIA_TYPE_ID", m_nID);
            if (o != DBNull.Value && o != null)
                m_nMediaType = int.Parse(o.ToString());
            else
                m_nMediaType = 0;
        }

        public void VideoTable(string sTable)
        {
            m_sTable = sTable;
            if (sTable == "commercial_files")
            {
                object o = PageUtils.GetTableSingleVal(m_sTable, "MEDIA_TYPE_ID", m_nID);
                if (o != DBNull.Value && o != null)
                    m_nMediaType = int.Parse(o.ToString());
                else
                    m_nMediaType = 0;
            }
        }

        public string GetPlayerSrc()
        {
            string sStreamID = GetStreamCompCode();
            string sBaseURL = GetStreamBaseURL();
            string sRet = sBaseURL;
            sRet += sStreamID;
            if (sStreamID != "")
                return sRet;
            return "";
        }

        public string GetTNImage()
        {
            return GetTNImage(0, 0);
        }

        public string GetTNImage(Int32 nWidth, Int32 nHeight)
        {
            if (m_sTable != "media_files")
                return "";
            try
            {
                Int32 nMediaID = 0;
                Int32 nGroupID = 0;
                nMediaID = int.Parse(PageUtils.GetTableSingleVal("media_files", "MEDIA_ID", m_nID).ToString());
                nGroupID = int.Parse(PageUtils.GetTableSingleVal("media_files", "GROUP_ID", m_nID).ToString());
                object sBaseURL = PageUtils.GetTableSingleVal("media", "MEDIA_PIC_ID", nMediaID);
                if (sBaseURL != null && sBaseURL != DBNull.Value)
                {
                    if (sBaseURL.ToString() == "0")
                        sBaseURL = PageUtils.GetDefaultPICID(nGroupID).ToString();
                    string sPicURL = PageUtils.GetTableSingleVal("pics", "base_url", int.Parse(sBaseURL.ToString())).ToString();
                    //Int32 nGroupID = LoginManager.GetLoginGroupID();
                    object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
                    string sBasePicsURL = "";
                    if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
                        sBasePicsURL = oBasePicsURL.ToString();
                    if (sBasePicsURL == "")
                        sBasePicsURL = "pics";
                    else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
                        sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
                        sBasePicsURL = "http://" + sBasePicsURL;
                    string sPic = "";
                    if (nWidth == 0 && nHeight == 0)
                        sPic = ImageUtils.GetTNName(sPicURL, "tn");
                    else
                        sPic = ImageUtils.GetTNName(sPicURL, nWidth.ToString() + "X" + nHeight.ToString());
                    bool bWithEnding = true;
                    if (sBasePicsURL.EndsWith("=") == false)
                        sBasePicsURL += "/";
                    else
                        bWithEnding = false;
                    if (bWithEnding == false)
                    {
                        string sTmp = "";
                        string[] s = sPic.Split('.');
                        for (int i = 0; i < s.Length - 1; i++)
                        {
                            if (i > 0)
                                sTmp += ".";
                            sTmp += s[i];
                        }
                        sPic = sTmp;
                    }
                    sBasePicsURL += sPic;
                    return sBasePicsURL;
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        public void GetCDNData(ref string sCDNImpl, ref Int32 nCDNID, ref string sCDNNotidyURL)
        {
            GetCDNData(ref sCDNImpl, ref nCDNID, ref sCDNNotidyURL, 0);
        }

        public void GetCDNData(ref string sCDNImpl, ref Int32 nCDNID, ref string sCDNNotidyURL, Int32 nCacheSecs)
        {
            sCDNImpl = "normal";
            string sSuplierField = "vid_t." + m_sPreField + "STREAMING_SUPLIER_ID";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (nCacheSecs > 0)
                selectQuery.SetCachedSec(nCacheSecs);
            selectQuery += "select sc.id,sc.CDN_STR_ID,sc.CDN_BASE_NOTIFY from streaming_companies sc (nolock),";
            selectQuery += m_sTable;
            selectQuery += " vid_t (nolock) where " + sSuplierField + "=sc.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("vid_t.id", "=", m_nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sCDNImpl = selectQuery.Table("query").DefaultView[0].Row["CDN_STR_ID"].ToString();
                    nCDNID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    sCDNNotidyURL = selectQuery.Table("query").DefaultView[0].Row["CDN_BASE_NOTIFY"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        public string GetCDNCode()
        {
            string sBaseURL = "";
            string sSuplierField = "vid_t." + m_sPreField + "STREAMING_SUPLIER_ID";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select sc.CDN_STR_ID from streaming_companies sc,";
            selectQuery += m_sTable;
            selectQuery += " vid_t";
            selectQuery += " where " + sSuplierField + "=sc.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("vid_t.id", "=", m_nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sBaseURL = selectQuery.Table("query").DefaultView[0].Row["CDN_STR_ID"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sBaseURL;
        }

        protected string GetStreamingType()
        {
            string sRet = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select lmt.player_description from lu_media_types lmt,media_files mf where lmt.id=mf.media_type_id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", m_nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sRet = selectQuery.Table("query").DefaultView[0].Row["player_description"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet;
        }

        protected string GetStreamCompCode()
        {
            return GetStreamCompCode(0);
        }

        protected string GetStreamCompCode(Int32 nCacheSec)
        {
            string sCodeField = m_sPreField + "STREAMING_CODE";
            object sCode = "";
            if (nCacheSec > 0)
                sCode = ODBCWrapper.Utils.GetTableSingleVal(m_sTable, sCodeField, m_nID, nCacheSec);
            else
                sCode = ODBCWrapper.Utils.GetTableSingleVal(m_sTable, sCodeField, m_nID);
            if (sCode != null && sCode != DBNull.Value && GetStreamingType() != "PNG")
                return sCode.ToString();

            object sPicID = PageUtils.GetTableSingleVal(m_sTable, "REF_ID", m_nID);
            if (sPicID != null && sPicID != DBNull.Value)
            {
                Int32 nGroupID = LoginManager.GetLoginGroupID();
                return ProtocolsFuncs.GetPicURL(int.Parse(sPicID.ToString()), "full");
            }
            return "";
        }

        public string GetConfigData()
        {
            return GetConfigData(0);
        }

        public string GetConfigData(Int32 nCacheSec)
        {
            string sCodeField = m_sPreField + "ADDITIONAL_DATA";
            object sCode = "";
            if (nCacheSec > 0)
                sCode = HttpContext.Current.Server.HtmlDecode(ODBCWrapper.Utils.GetTableSingleVal(m_sTable, sCodeField, m_nID, nCacheSec).ToString()).Replace("<br\\>", "");
            else
                sCode = HttpContext.Current.Server.HtmlDecode(ODBCWrapper.Utils.GetTableSingleVal(m_sTable, sCodeField, m_nID).ToString()).Replace("<br\\>", "");
            return sCode.ToString();
        }

        protected string GetStreamCompID()
        {
            string sSuplierField = m_sPreField + "STREAMING_SUPLIER_ID";
            object sCode = PageUtils.GetTableSingleVal(m_sTable, sSuplierField, m_nID);
            if (sCode != null && sCode != DBNull.Value)
                return sCode.ToString();
            return "";
        }

        protected string GetStreamBaseURL()
        {
            string sBaseURL = "";
            string sSuplierField = "vid_t." + m_sPreField + "STREAMING_SUPLIER_ID";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select sc.ADM_VIDEO_BASE_URL from streaming_companies sc,";
            selectQuery += m_sTable;
            selectQuery += " vid_t";
            selectQuery += " where " + sSuplierField + "=sc.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("vid_t.id", "=", m_nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sBaseURL = selectQuery.Table("query").DefaultView[0].Row["ADM_VIDEO_BASE_URL"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sBaseURL;
        }

        public string GetPlayerTN()
        {
            string sImage = GetTNImage();
            string sRet = "";
            sRet = "<img SRC=\"" + sImage;
            sRet += "\" style=\"border:solid 1px #333333;\"/>";
            return sRet;
        }

        public string GetPlayerTNLink()
        {
            //sRet = "media_file_id=" + m_nID.ToString();
            string sFV = "";
            string sFileFormat = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select gp.USERNAME,gp.PASSWORD,mf.media_id,lmt.player_description,lmt.DESCRIPTION as t_d,lmq.DESCRIPTION as q_d from groups_passwords gp,lu_media_types lmt,lu_media_quality lmq,media_files mf where mf.MEDIA_TYPE_ID=lmt.id and mf.MEDIA_QUALITY_ID=lmq.id and gp.group_id=mf.group_id and gp.status=1 and gp.is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", m_nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    string sPlayerUN = selectQuery.Table("query").DefaultView[0].Row["USERNAME"].ToString();
                    string sPlayerPass = selectQuery.Table("query").DefaultView[0].Row["PASSWORD"].ToString();
                    sFileFormat = selectQuery.Table("query").DefaultView[0].Row["t_d"].ToString();
                    string sFileFormatPD = selectQuery.Table("query").DefaultView[0].Row["player_description"].ToString();
                    string sFileQuality = selectQuery.Table("query").DefaultView[0].Row["q_d"].ToString();
                    string sMediaID = selectQuery.Table("query").DefaultView[0].Row["media_id"].ToString();
                    if (sFileFormat == "GIB")
                        sFileFormat = "gib";
                    if (sFileFormatPD.Trim().ToUpper() != "PNG")
                        sFV += "pic_size1=full&pic_size2=full&";
                    sFV += "debug_protocols=true&server_base_url=http://vod.orange.co.il/&lang=&auto_play=false&Prod=1&auto_init=1&config_file=http://admin.tvinci.com/flash/config_admin.xml&language_file=http://admin.tvinci.com/flash/lucy_language.xml&skin_file=http://admin.tvinci.com/flash/gui_admin.swf&layout_file=http://admin.tvinci.com/flash/lucy_layout7.xml";
                    sFV += "&media_id=" + sMediaID + "&auto_play=true&object_id=" + sPlayerUN + "&object_key=" + sPlayerPass + "&file_format=" + sFileFormat + "&file_quality=" + sFileQuality + "&";
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            string sImage = GetTNImage();
            string sRet = "";
            sRet = "<div onClick=\"openFlyPlayer('" + sFV + "','" + sFileFormat + "' , '" + m_nID.ToString() + "');\"><img SRC=\"images/player_icon_02.png\"";
            sRet += "\" style=\"border:solid 1px #333333;\"/></div>";
            return sRet;
        }

        protected string GetStreamBaseURLForPlayer()
        {
            return GetStreamBaseURLForPlayer(0);
        }

        protected string GetStreamBaseURLForPlayer(Int32 nCacheSec)
        {
            string sBaseURL = "";
            string sSuplierField = "vid_t." + m_sPreField + "STREAMING_SUPLIER_ID";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select sc.VIDEO_BASE_URL from streaming_companies sc (nolock),";
            selectQuery += m_sTable;
            selectQuery += " vid_t (nolock) ";
            selectQuery += " where " + sSuplierField + "=sc.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("vid_t.id", "=", m_nID);
            selectQuery.SetCachedSec(nCacheSec);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sBaseURL = selectQuery.Table("query").DefaultView[0].Row["VIDEO_BASE_URL"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sBaseURL.Trim();
        }

        public string GetFLVSrc()
        {
            return GetFLVSrc(0);
        }

        public string GetFLVSrc(Int32 nGroupID)
        {
            return GetFLVSrc(nGroupID, 0);
        }

        public string GetFLVSrc(Int32 nGroupID, Int32 nCacheSec)
        {
            string sStreamID = GetStreamCompCode(nCacheSec);
            string sBaseURL = GetStreamBaseURLForPlayer(nCacheSec);
            string sRet = sBaseURL;
            sRet += sStreamID;
            //if (sBaseURL != "" && sStreamID != "")
            if (sStreamID != "")
            {
                if (nGroupID != 0)
                {
                    object oGroupCD = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_COUNTRY_CODE", nGroupID, 86400);
                    if (oGroupCD != null && oGroupCD != DBNull.Value)
                        sRet = sRet.Replace("!--COUNTRY_CD--", oGroupCD.ToString().Trim().ToLower());
                    if (sRet.IndexOf("!--tick_time--") != -1)
                    {
                        long lT = DateTime.UtcNow.Ticks;
                        object oGroupSecret = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_SECRET_CODE", nGroupID, 86400);
                        sRet = sRet.Replace("!--tick_time--", "tick=" + lT.ToString());
                        string sToHash = "";
                        string sHashed = "";
                        if (oGroupSecret != null && oGroupSecret != DBNull.Value)
                        {
                            sToHash = oGroupSecret.ToString() + lT.ToString();
                            sHashed = TVinciShared.ProtocolsFuncs.CalculateMD5Hash(sToHash);
                        }
                        sRet = sRet.Replace("!--hash--", "hash=" + sHashed);
                    }
                    if (sRet.IndexOf("!--group--") != -1)
                    {
                        sRet = sRet.Replace("!--group--", "group=" + nGroupID.ToString());
                    }
                    if (sRet.IndexOf("!--config_data--") != -1)
                    {
                        sRet = sRet.Replace("!--config_data--", "brt=" + GetConfigData(7200));
                    }
                }
                sRet = HttpContext.Current.Server.HtmlDecode(sRet).Replace("''", "\"");
                return sRet;
            }
            return "";
        }

        public string GetPlayerFrame()
        {
            string sStreamID = GetStreamCompCode();
            //string sBaseURL = GetStreamBaseURL();
            //string sCDNID = GetStreamCompID();
            //string sCDNType = GetCDNCode();
            //string sIFRameLink = "adm_video_player.aspx?cdnid=" + sCDNID.ToString() + "&cdntype=" + sCDNType.ToString() + "&streamid=" + sStreamID.ToString();
            //string sTVBOXLink = "tvbox.aspx?clip=" + m_nID.ToString();
            string sRet = "";
            //if (m_nMediaType == 1)
            //{
            //sRet = "<IFRAME SRC=\"admin_player.aspx?player_type=video&size=big&cdn_type=" + sCDNType + "&flv=" + HttpContext.Current.Server.UrlEncode(GetFLVSrc()) + "&autoplay=false";
            sRet = "<IFRAME SRC=\"admin_player.aspx?player_type=video&size=big&media_file_id=" + m_nID.ToString() + "&autoplay=false";
            sRet += "\" WIDTH=\"480\" HEIGHT=\"400\" FRAMEBORDER=\"0\"></IFRAME>";
            if (sStreamID != "")
                return sRet;
            //}
            return "";
        }

        public string GetPlayerSmallFrame()
        {
            string sStreamID = GetStreamCompCode();
            //string sBaseURL = GetStreamBaseURL();
            //string sCDNID = GetStreamCompID();
            //string sCDNType = GetCDNCode();
            //string sIFRameLink = "adm_video_player.aspx?cdnid=" + sCDNID.ToString() + "&cdntype=" + sCDNType.ToString() + "&streamid=" + sStreamID.ToString();
            //string sTVBOXLink = "tvbox.aspx?clip=" + m_nID.ToString();
            string sRet = "";
            //if (m_nMediaType == 1)
            //{
            //sRet = "<IFRAME SRC=\"admin_player.aspx?player_type=video&size=small&cdn_type=" + sCDNType + "&flv=" + HttpContext.Current.Server.UrlEncode(GetFLVSrc()) + "&autoplay=false";
            sRet = "<IFRAME SRC=\"admin_player.aspx?player_type=video&size=small&media_file_id=" + m_nID.ToString() + "&autoplay=false";
            sRet += "\" WIDTH=\"200\" HEIGHT=\"200\" FRAMEBORDER=\"0\"></IFRAME>";
            if (sStreamID != "")
                return sRet;
            //}
            return "";
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            sTmp += "<td class=\"align1\">";
            //sTmp += "<table><tr><td>";
            sTmp += GetPlayerFrame();
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='video_player'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            //sTmp += "</td></tr></table>";
            sTmp += "</td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordFlashViewerField : BaseDataRecordField
    {
        protected string m_sTable;
        protected Int32 m_nID;
        public DataRecordFlashViewerField(string sTable, Int32 nID)
            : base()
        {
            m_sTable = sTable;
            m_nID = nID;
        }

        protected string GetFlashCode()
        {
            string sFlashURL = PageUtils.GetTableSingleVal(m_sTable, "FLASH_URL", m_nID).ToString();
            string sFlashWidth = PageUtils.GetTableSingleVal(m_sTable, "FLASH_Width", m_nID).ToString();
            string sFlashHeight = PageUtils.GetTableSingleVal(m_sTable, "FLASH_Height", m_nID).ToString();
            string sRet = "";
            sRet += "<object id=\"" + m_nID.ToString() + "_flash_viewer\" classid=\"clsid:D27CDB6E-AE6D-11cf-96B8-444553540000\" width=\"" + sFlashWidth + "px\" height=\"" + sFlashHeight + "px\">";
            sRet += "<param name=\"movie\" value=\"flash/" + sFlashURL + "\" />";
            sRet += "<param name=\"scale\" value=\"noscale\" />";
            sRet += "<param name=\"SALIGN\" value=\"LT\" />";
            sRet += "<param name=\"allowScriptAccess\" value=\"sameDomain\" />";
            sRet += "<param name=\"wmode\" value=\"opaque\" />";
            sRet += "</object>";
            return sRet;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += GetFlashCode();
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='video_player'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "</td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordCutWithDoubleField : BaseDataRecordField
    {
        DataRecordCheckBoxField m_checkBoxField;
        DataRecordShortDoubleField m_doubleField;
        string m_sCheckTitle;
        public DataRecordCutWithDoubleField(ref DataRecordCheckBoxField checkBoxField, ref DataRecordShortDoubleField doubleField, string sCheckTitle, int? mulFactor = null)
            : base()
        {
            m_checkBoxField = checkBoxField;
            m_doubleField = doubleField;
            m_sCheckTitle = sCheckTitle;
            m_mulFactor = mulFactor;
        }

        public DataRecordCutWithDoubleField(ref DataRecordCheckBoxField checkBoxField, ref DataRecordShortDoubleField doubleField, int? mulFactor = null)
            : base()
        {
            m_checkBoxField = checkBoxField;
            m_doubleField = doubleField;
            m_sCheckTitle = "";
            m_mulFactor = mulFactor;
        }

        public override string GetFieldHtml(long nID, ref int nToAdd)
        {
            nToAdd = 2;
            return GetFieldHtml(nID);
        }

        public override void SetValue(string sTable, string sIndexFieldName, object oIndexFieldVal)
        {
            m_checkBoxField.SetValue(sTable, sIndexFieldName, oIndexFieldVal);
            m_doubleField.SetValue(sTable, sIndexFieldName, oIndexFieldVal);
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += m_doubleField.GetInnerFieldHtml(nID + 1);
            sTmp += "&nbsp;&nbsp;&nbsp;";
            sTmp += m_checkBoxField.GetInnerFieldHtml(nID);
            sTmp += "&nbsp;";
            sTmp += "<span class='FormInputnbg'> " + m_sCheckTitle + "</span> ";
            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordCutWithStrField : BaseDataRecordField
    {
        DataRecordCheckBoxField m_checkBoxField;
        DataRecordShortTextField m_strField;
        string m_sCheckTitle;
        public DataRecordCutWithStrField(ref DataRecordCheckBoxField checkBoxField, ref DataRecordShortTextField strField, string sCheckTitle)
            : base()
        {
            m_checkBoxField = checkBoxField;
            m_strField = strField;
            m_sCheckTitle = sCheckTitle;
        }

        public DataRecordCutWithStrField(ref DataRecordCheckBoxField checkBoxField, ref DataRecordShortTextField strField)
            : base()
        {
            m_checkBoxField = checkBoxField;
            m_strField = strField;
            m_sCheckTitle = "";
        }

        public override string GetFieldHtml(long nID, ref int nToAdd)
        {
            nToAdd = 2;
            return GetFieldHtml(nID);
        }

        public override void SetValue(string sTable, string sIndexFieldName, object oIndexFieldVal)
        {
            m_checkBoxField.SetValue(sTable, sIndexFieldName, oIndexFieldVal);
            m_strField.SetValue(sTable, sIndexFieldName, oIndexFieldVal);
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += m_strField.GetInnerFieldHtml(nID + 1);
            sTmp += "&nbsp;&nbsp;&nbsp; ";
            sTmp += m_checkBoxField.GetInnerFieldHtml(nID);
            sTmp += "&nbsp;";
            sTmp += "<span class='FormInputnbg'> " + m_sCheckTitle + "</span> ";
            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordCutWithBoolField : BaseDataRecordField
    {
        DataRecordCheckBoxField m_checkBoxField;
        DataRecordBoolField m_boolField;
        string m_sCheckTitle;
        public DataRecordCutWithBoolField(ref DataRecordCheckBoxField checkBoxField, ref DataRecordBoolField boolField, string sCheckTitle)
            : base()
        {
            m_checkBoxField = checkBoxField;
            m_boolField = boolField;
            m_sCheckTitle = sCheckTitle;
        }

        public DataRecordCutWithBoolField(ref DataRecordCheckBoxField checkBoxField, ref DataRecordBoolField boolField)
            : base()
        {
            m_checkBoxField = checkBoxField;
            m_boolField = boolField;
            m_sCheckTitle = "";
        }

        public override string GetFieldHtml(long nID, ref int nToAdd)
        {
            nToAdd = 2;
            return GetFieldHtml(nID);
        }

        public override void SetValue(string sTable, string sIndexFieldName, object oIndexFieldVal)
        {
            m_checkBoxField.SetValue(sTable, sIndexFieldName, oIndexFieldVal);
            m_boolField.SetValue(sTable, sIndexFieldName, oIndexFieldVal);
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";

            sTmp += "<td class=\"align1\"> ";

            sTmp += m_boolField.GetInnerFieldHtml(nID + 1);
            sTmp += "&nbsp;&nbsp;&nbsp;";
            sTmp += m_checkBoxField.GetInnerFieldHtml(nID);
            sTmp += "&nbsp;";
            sTmp += "<span class='FormInputnbg'> " + m_sCheckTitle + "</span> ";
            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordCheckBoxField : BaseDataRecordField
    {
        protected bool m_bEnabled;
        protected string m_filedPrivateName;
        public DataRecordCheckBoxField(bool bEnabled)
            : base()
        {
            m_bEnabled = bEnabled; 
            m_filedPrivateName= string.Empty;
        }
        public void setFiledName(string name)
        {
            m_filedPrivateName = name;
        }

        public string GetInnerFieldHtml(long nID)
        {
            string sTmp = "";

            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='checkbox' ";
            if (m_bEnabled == false)
                sTmp += "disabled ";
            if (m_sStartValue == "1")
                sTmp += "checked";
            else
                if (m_sStartValue == "" && m_nDefault == 1)
                    sTmp += "checked";
            sTmp += "/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='checkbox'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";

            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_fieldName' value='" + m_filedPrivateName + "'/>";

            return sTmp;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            sTmp += "<td class=\"align1\">";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += GetInnerFieldHtml(nID);
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordBoolField : BaseDataRecordField
    {
        protected bool m_bEnabled;
        public DataRecordBoolField(bool bEnabled)
            : base()
        {
            m_bEnabled = bEnabled;
        }

        public string GetInnerFieldHtml(long nID)
        {
            string sTmp = "";
            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" type='radio' name='" + nID.ToString() + "_val' value='0' class='" + m_sHeaderCss + "'";
            if (m_sStartValue == "0")
            {
                sTmp += " checked ";
            }
            else
            {
                if (m_sStartValue == "" && (m_nDefault == 0 || m_nDefault == -1))
                {
                    sTmp += " checked ";
                }
            }
            sTmp += "><span class='FormInputnbg'>No</span>";

            sTmp += "<input  tabindex=\"" + (nID + 1).ToString() + "\" type='radio' name='" + nID.ToString() + "_val' value='1' class='" + m_sHeaderCss + "'";
            if (m_sStartValue == "1")
            {
                sTmp += " checked ";
            }
            else
            {
                if (m_sStartValue == "" && m_nDefault == 1)
                {
                    sTmp += " checked ";
                }
            }
            sTmp += "><span class='FormInputnbg'>Yes</span>";

            sTmp += "<br/>";

            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='int'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_inputtype' value='radio'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            return sTmp;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";

            sTmp += GetInnerFieldHtml(nID);
            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordShortIntField : BaseDataRecordField
    {
        protected bool m_bEnabled;
        protected long m_nWidth;
        protected long m_nMaxLength;

        protected long? m_nMaxValue;
        protected long? m_nMinValue;

        protected string m_filedPrivateName;


        public DataRecordShortIntField(bool bEnabled, long nWidth, long nMaxLength, int? minVal = null, int? maxVal = null, int? mulFactor = null)
            : base()
        {
            m_bEnabled = bEnabled;
            m_nWidth = nWidth;
            m_nMaxLength = nMaxLength;
            m_nMinValue = minVal;
            m_nMaxValue = maxVal;
            m_filedPrivateName = string.Empty;
            m_mulFactor = mulFactor;
        }

        public void setFiledName(string name)
        {
            m_filedPrivateName = name;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            if (m_bEnabled == true)
                sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='";
            if (m_bEnabled == true)
                sTmp += "text' ";
            else
                sTmp += "hidden' ";
            //    sTmp += "disabled ";
            sTmp += "dir='ltr' ";
            sTmp += "size=" + m_nWidth.ToString() + " ";
            sTmp += "maxlength=" + m_nMaxLength.ToString() + " ";
            if (m_sStartValue != "")
            {
                if (m_mulFactor.HasValue)
                {
                    int startValue = int.Parse(m_sStartValue.ToString()) / m_mulFactor.Value;
                    sTmp += "value='" + startValue.ToString() + "' ";
                }
                else
                {
                    sTmp += "value='" + m_sStartValue.ToString() + "' ";
                }
            }
            else if (m_nDefault != -1)
            {
                sTmp += "value='" + m_nDefault.ToString() + "' ";
            }
            sTmp += "/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='int'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_fieldName' value='" + m_filedPrivateName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_mulFactor' value='" + m_mulFactor + "'/>";
            if (m_nMinValue.HasValue)
                sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_min' value='" + m_nMinValue + "'/>";
            if (m_nMaxValue.HasValue)
                sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_max' value='" + m_nMaxValue + "'/>";
            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }

    }

    public class DataRecordShortDoubleField : DataRecordShortIntField
    {
        public DataRecordShortDoubleField(bool bEnabled, long nWidth, long nMaxLength, int? mulFactor = null)
            : base(bEnabled, nWidth, nMaxLength)
        {
            this.m_mulFactor = mulFactor;
        }

        public string GetInnerFieldHtml(long nID)
        {
            string sTmp = "";
            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='";
            if (m_bEnabled == true)
                sTmp += "text' ";
            else
                sTmp += "hidden' ";
            //    sTmp += "disabled ";
            sTmp += "dir='ltr' ";
            sTmp += "size=" + m_nWidth.ToString() + " ";
            sTmp += "maxlength=" + m_nMaxLength.ToString() + " ";
            if (m_sStartValue != "")
                sTmp += "value='" + m_sStartValue.ToString() + "' ";
            else
                if (m_nDefault != -1)
                    sTmp += "value='" + m_nDefault.ToString() + "' ";
            sTmp += "/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='double'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_mulFactor' value='" + m_mulFactor + "'/>";
            return sTmp;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            if (m_bEnabled == true)
                sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += GetInnerFieldHtml(nID);
            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordShortIntWithSearchField : BaseDataRecordField
    {
        protected long m_nWidth;
        protected string m_sDir;
        protected string m_sCollectionTable;
        protected string m_sCollectionPointerField;
        protected string m_sCollectionTextField;
        public DataRecordShortIntWithSearchField(long nWidth, string sDir, string sCollectionTable, string sCollectionTextField, string sCollectionPointer, int? mulFactor = null)
            : base()
        {
            m_nWidth = nWidth;
            m_sDir = sDir;
            m_sCollectionPointerField = sCollectionPointer;
            m_sCollectionTable = sCollectionTable;
            m_sCollectionTextField = sCollectionTextField;
            m_mulFactor = mulFactor;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='";
            sTmp += "hidden' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=" + m_nWidth.ToString() + " ";
            if (m_sStartValue != "")
                sTmp += "value='" + m_sStartValue.ToString() + "' ";
            sTmp += "/>";
            sTmp += "<input  type='html' class='" + m_sInputCss + "' name='" + nID.ToString() + "_val_text' id='" + nID.ToString() + "_val_text' onkeyup='return searchPress(\"" + m_sCollectionTable + "\" , \"" + m_sCollectionTextField + "\",\"" + m_sCollectionPointerField + "\" , \"" + nID.ToString() + "\" , \"tags\");' ";
            sTmp += "dir='" + m_sDir + "' ";
            sTmp += "size=" + m_nWidth.ToString() + " ";
            sTmp += "value='" + m_sStartValue.ToString() + "' ";
            sTmp += "/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='int'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_mulFactor' value='" + m_mulFactor + "'/>";
            sTmp += "</td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordMultiPicBrowser : DataRecordMultiField
    {
        public DataRecordMultiPicBrowser(
            string sMiddleTable,
            string sMiddleMainFieldName,
            string sMiddleCollFieldName)
            : base("pics", "id", "id", sMiddleTable,
                sMiddleMainFieldName, sMiddleCollFieldName, false,
                "ltr", 60, "tags")
        {

        }

        public override string GetSelectsHtml(long nID)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
            string sBasePicsURL = "";
            if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
                sBasePicsURL = oBasePicsURL.ToString();
            if (sBasePicsURL == "")
                sBasePicsURL = "pics";
            else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
                sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
                sBasePicsURL = "http://" + sBasePicsURL;

            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap valign='top'>";
            if (m_bMust == true)
                sTmp += "<font color=red>*&nbsp;&nbsp;</font>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td  class=\"align1\">";
            sTmp += "<table width=100%>";
            if (m_sStartValue != "")
            {
                string[] sPicIDs = m_sStartValue.Split(';');
                sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                Int32 nRowCounter = 0;
                for (int i = 0; i < sPicIDs.Length; i++)
                {
                    Int32 nPicID = 0;
                    try
                    {
                        nPicID = int.Parse(sPicIDs[i]);
                    }
                    catch
                    {
                    }

                    if (nPicID > 0)
                    {
                        object oPic = PageUtils.GetTableSingleVal("pics", "BASE_URL", nPicID);
                        if (oPic != DBNull.Value)
                        {
                            nRowCounter++;
                            string sPicURL = ImageUtils.GetTNName(oPic.ToString(), "tn");
                            sTmp += "<img src=\"";
                            sTmp += sBasePicsURL;
                            bool bWithEnding = true;
                            if (sBasePicsURL.EndsWith("=") == false)
                                sTmp += "/";
                            else
                                bWithEnding = false;
                            if (bWithEnding == false)
                            {
                                string sTmp1 = "";
                                string[] s = sPicURL.Split('.');
                                for (int j = 0; j < s.Length - 1; j++)
                                {
                                    if (j > 0)
                                        sTmp1 += ".";
                                    sTmp1 += s[j];
                                }
                                sPicURL = sTmp1;
                            }
                            sTmp += sPicURL + "\" class=\"img_border\"/>";
                            if (nRowCounter == 6)
                            {
                                nRowCounter = 0;
                                sTmp += "<br/>";
                            }

                        }
                        else
                        {
                            sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                            sTmp += "</td></tr>";
                        }
                    }
                    else
                    {
                        sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                        sTmp += "</td></tr>";
                    }
                }
                sTmp += "</td></tr>";
            }
            else
            {
                sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                sTmp += "</td></tr>";
            }
            sTmp += "<tr><td colspan=2 class=\"align1\">";
            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" class='FormInput' name='" + nID.ToString() + "_val' type='";
            sTmp += "hidden' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=30 ";
            sTmp += "maxlength=8 ";
            if (m_sStartValue != "")
                sTmp += "value='" + m_sStartValue.ToString() + "' ";
            sTmp += "/><a class=\"btn\" href=\"javascript:OpenPicBrowser('" + nID.ToString() + "_val' , 1000);\">Pics manager</a>";

            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='multi'/>";
            // the field in the collection table
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            // the collection table
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_collection_table' value='" + m_sCollectionTable + "'/>";
            //The middle table
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_middle_table' value='" + m_sMiddleTable + "'/>";
            //The middle ref cell to the main
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_middle_ref_main_field' value='" + m_sMiddleMainFieldName + "'/>";
            //The middle ref cell to the collection
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_middle_ref_collection_field' value='" + m_sMiddleCollFieldName + "'/>";
            //The main pointer field
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_main_pointer_field' value='" + m_sMainPointerField + "'/>";

            string sExtraFieldName = "";
            string sExtraFieldVal = "";
            string sExtraFieldType = "";
            if (m_sExtraWhere != "")
            {
                string[] toSplitWith = { "=" };
                string[] splited = m_sExtraWhere.Split(toSplitWith, StringSplitOptions.RemoveEmptyEntries);
                if (splited.Length == 2)
                {
                    sExtraFieldName = splited[0].ToString();
                    sExtraFieldVal = splited[1].ToString();
                    if (sExtraFieldVal.StartsWith("'") == true)
                    {
                        sExtraFieldType = "string";
                        sExtraFieldVal = sExtraFieldVal.Substring(1, sExtraFieldVal.Length - 2);
                    }
                    else
                    {
                        sExtraFieldType = "int";
                    }
                }
            }
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_extra_field_name' value='" + sExtraFieldName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_extra_field_val' value='" + sExtraFieldVal + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_extra_field_type' value='" + sExtraFieldType + "'/>";
            //The main pointer field
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_collection_pointer_field' value='" + m_sCollectionPointerField + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_collection_auto_add' value='" + m_bAddExtra.ToString() + "'/>";
            sTmp += "</td>";
            sTmp += "</tr>";
            sTmp += "</table></td></tr>";
            return sTmp;
        }
    }

    public class DataRecordMultiVidBrowser : DataRecordMultiField
    {
        protected string m_sVidTable;
        protected string m_sVidTableTags;
        protected string m_sVidTableTagsRef;
        public DataRecordMultiVidBrowser(
            string sVidTable,
            string vidTableTags,
            string vidTableTagsRef,
            string sMiddleTable,
            string sMiddleMainFieldName,
            string sMiddleCollFieldName)
            : base(sVidTable, "id", "id", sMiddleTable,
                sMiddleMainFieldName, sMiddleCollFieldName, false,
                "ltr", 60, "tags")
        {
            m_sVidTable = sVidTable;
            m_sVidTableTags = vidTableTags;
            m_sVidTableTagsRef = vidTableTagsRef;
        }

        public override string GetSelectsHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap valign='top'>";
            if (m_bMust == true)
                sTmp += "<font color=red>*&nbsp;&nbsp;</font>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td  align=right   width=100% nowrap>";
            sTmp += "<table width=100%>";
            if (m_sStartValue != "")
            {
                string[] sVidIDs = m_sStartValue.Split(';');
                sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                for (int i = 0; i < sVidIDs.Length; i++)
                {
                    Int32 nVidID = 0;
                    try
                    {
                        nVidID = int.Parse(sVidIDs[i]);
                    }
                    catch
                    {
                    }

                    if (nVidID > 0)
                    {
                        Int32 nMediaVidID = 0;
                        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                        selectQuery += "select mf.id from media_files mf where mf.status=1  and mf.MEDIA_TYPE_ID=1 and ";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.media_id", "=", nVidID);
                        if (selectQuery.Execute("query", true) != null)
                        {
                            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                            if (nCount > 0)
                            {
                                nMediaVidID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                            }
                        }
                        selectQuery.Finish();
                        selectQuery = null;
                        string sPre = "";
                        if (m_sFieldName.IndexOf("STREAMING_CODE") > 0)
                            sPre = m_sFieldName.Substring(0, m_sFieldName.Length - m_sFieldName.IndexOf("STREAMING_CODE"));
                        DataRecordMediaViewerField dr_player = new DataRecordMediaViewerField(sPre, nMediaVidID);
                        string sName = PageUtils.GetTableSingleVal("media", "name", nVidID).ToString();
                        dr_player.Initialize("הוידאו", "adm_table_header_nbg", "FormInput", "STREAMING_CODE", false);
                        string sPicURL = dr_player.GetTNImage();
                        if (sPicURL != "")
                        {
                            sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_vid_beowse\">";
                            string sObjectID = nID.ToString() + "_val";
                            sTmp += "<img style=\"cursor: pointer;\" onclick=\"ChangeVideoPlayer('" + sObjectID + "','" + dr_player.GetPlayerSrc() + "');\" src=\"";
                            sTmp += sPicURL + "\" class=\"img_border\"/>";
                            sTmp += "<div class=\"vid_name\">\r\n" + sName + "</div>\r\n";
                            sTmp += "</td></tr>";
                            sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_val_palyer\">";
                            sTmp += dr_player.GetPlayerFrame();
                            sTmp += "</td></tr>";
                        }
                        else
                        {
                            sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_vid_beowse\">";
                            sTmp += "</td></tr>";
                        }
                    }
                    else
                    {
                        sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_vid_beowse\">";
                        sTmp += "</td></tr>";
                    }
                }
                sTmp += "</td></tr>";
            }
            else
            {
                sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                sTmp += "</td></tr>";
            }
            sTmp += "<tr><td colspan=2 align=right   width=100% nowrap>";
            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='";
            sTmp += "hidden' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=30 ";
            sTmp += "maxlength=8 ";
            if (m_sStartValue != "")
                sTmp += "value='" + m_sStartValue.ToString() + "' ";
            sTmp += "/><a class=\"btn_vid_browse\" href=\"javascript:OpenVidBrowser('" + nID.ToString() + "_val' , 100 , '" + m_sVidTable + "','" + m_sVidTableTags + "','" + m_sVidTableTagsRef + "');\"></a>";

            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='multi'/>";
            // the field in the collection table
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            // the collection table
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_collection_table' value='" + m_sCollectionTable + "'/>";
            //The middle table
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_middle_table' value='" + m_sMiddleTable + "'/>";
            //The middle ref cell to the main
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_middle_ref_main_field' value='" + m_sMiddleMainFieldName + "'/>";
            //The middle ref cell to the collection
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_middle_ref_collection_field' value='" + m_sMiddleCollFieldName + "'/>";
            //The main pointer field
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_main_pointer_field' value='" + m_sMainPointerField + "'/>";

            string sExtraFieldName = "";
            string sExtraFieldVal = "";
            string sExtraFieldType = "";
            if (m_sExtraWhere != "")
            {
                string[] toSplitWith = { "=" };
                string[] splited = m_sExtraWhere.Split(toSplitWith, StringSplitOptions.RemoveEmptyEntries);
                if (splited.Length == 2)
                {
                    sExtraFieldName = splited[0].ToString();
                    sExtraFieldVal = splited[1].ToString();
                    if (sExtraFieldVal.StartsWith("'") == true)
                    {
                        sExtraFieldType = "string";
                        sExtraFieldVal = sExtraFieldVal.Substring(1, sExtraFieldVal.Length - 2);
                    }
                    else
                    {
                        sExtraFieldType = "int";
                    }
                }
            }
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_extra_field_name' value='" + sExtraFieldName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_extra_field_val' value='" + sExtraFieldVal + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_extra_field_type' value='" + sExtraFieldType + "'/>";
            //The main pointer field
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_collection_pointer_field' value='" + m_sCollectionPointerField + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_collection_auto_add' value='" + m_bAddExtra.ToString() + "'/>";
            sTmp += "</td>";
            sTmp += "</tr>";
            sTmp += "</table></td></tr>";
            return sTmp;
        }
    }

    public class DataRecordMultiField : BaseDataRecordField
    {
        protected string m_sDir;
        protected long m_nWidth;
        protected string m_sCollCss;
        protected Int32 m_nCollectionLength;

        protected string m_sCollectionTable;

        protected string m_sCollectionPointerField;
        protected string m_sMainPointerField;
        protected string m_sMiddleTable;
        protected string m_sMiddleMainFieldName;
        protected string m_sMiddleCollFieldName;
        protected string m_sOrderBy;
        protected string m_sCollectionQuery;
        protected string m_sExtraWhere = "";
        protected string m_sJoinCondition = string.Empty;
        protected bool m_bAddExtra;
        protected string m_sMiddleTableType;

        public void SetJoinCondition(string sJoinCondition)
        {
            m_sJoinCondition = sJoinCondition;
        }

        public void SetExtraWhere(string sExtraWhere)
        {
            m_sExtraWhere = sExtraWhere;
        }

        public void SetMiddleTableType(string middleTableType)
        {
            this.m_sMiddleTableType = middleTableType;
        }

        public override void SetValue(string sTable, string sIndexFieldName, object oIndexFieldVal)
        {
            string sFieldName = "c." + m_sFieldName;
            if (m_sFieldName.IndexOf("+") != -1)
            {
                string[] splited = m_sFieldName.Split('+');
                sFieldName = "(";
                for (int j = 0; j < splited.Length; j++)
                {
                    if (j > 0)
                        sFieldName += "+' '+";
                    sFieldName += "c." + splited[j];
                }
                sFieldName += ")";
            }

            string sMidleMainRef = "m." + m_sMiddleMainFieldName;
            string sMidleCollRef = "m." + m_sMiddleCollFieldName;
            string sCollPointer = "c." + m_sCollectionPointerField;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(m_sConnectionKey);
            selectQuery += "select distinct " + sFieldName + " as txt from ";
            selectQuery += m_sCollectionTable;

            selectQuery += " c, " + m_sMiddleTable + " m where " + PageUtils.GetStatusQueryPart("m") + " and " + PageUtils.GetStatusQueryPart("c") + " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sMidleMainRef, "=", oIndexFieldVal);
            selectQuery += "and " + sMidleCollRef + "=" + sCollPointer;
            if (m_sExtraWhere != "")
            {
                string sExtra = "c." + m_sExtraWhere;
                selectQuery += "and " + sExtra;
            }

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        string sCollectiontxt = selectQuery.Table("query").DefaultView[i].Row["txt"].ToString();
                        m_sStartValue += sCollectiontxt;
                        m_sStartValue += ";";
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        public DataRecordMultiField(
            string sCollectionTable,
            string sCollectionPointerField,
            string sMainPointerField,
            string sMiddleTable,
            string sMiddleMainFieldName,
            string sMiddleCollFieldName,
            bool bAddExtra,
            string sDir,
            long nWidth,
            string sCollCss)
            : base()
        {
            m_sExtraWhere = "";
            m_nCollectionLength = 8;
            m_sMainPointerField = sMainPointerField;
            m_sMiddleTable = sMiddleTable;
            m_sMiddleMainFieldName = sMiddleMainFieldName;
            m_sMiddleCollFieldName = sMiddleCollFieldName;
            m_sCollectionTable = sCollectionTable;
            m_sCollectionPointerField = sCollectionPointerField;
            m_bAddExtra = bAddExtra;
            m_sDir = sDir;
            m_nWidth = nWidth;
            m_sCollCss = sCollCss;
            m_sOrderBy = "";
            m_sCollectionQuery = "";
        }

        public void SetCollectionLength(Int32 nCollectionLength)
        {
            m_nCollectionLength = nCollectionLength;
        }

        public void SetCollectionQuery(string sTheQuery)
        {
            m_sCollectionQuery = sTheQuery;
        }

        public void SetOrderCollectionBy(string sOrderBy)
        {
            m_sOrderBy = sOrderBy;
        }

        protected string GetCollectionTDString()
        {
            string sTD = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(m_sConnectionKey);
            if (m_sCollectionQuery != "")
                selectQuery += m_sCollectionQuery;
            else
            {
                string sFieldName = m_sFieldName;
                if (m_sFieldName.IndexOf("+") != -1)
                {
                    string[] splited = m_sFieldName.Split('+');
                    sFieldName = "(";
                    for (int j = 0; j < splited.Length; j++)
                    {
                        if (j > 0)
                            sFieldName += "+' '+";
                        sFieldName += splited[j];
                    }
                    sFieldName += ")";
                }
                selectQuery += "select top " + m_nCollectionLength.ToString() + " " + sFieldName + " as txt," + m_sCollectionPointerField + " as val from ";
                selectQuery += m_sCollectionTable;
                selectQuery += " where " + PageUtils.GetStatusQueryPart("");
                Int32 nGroupID = LoginManager.GetLoginGroupID();
                string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
                selectQuery += " and (";
                selectQuery += "group_id " + sGroups;

                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += " or group_id is null or group_id=0)  ";
                if (m_sExtraWhere != "")
                {
                    selectQuery += "and";
                    selectQuery += m_sExtraWhere;
                }
                if (m_sJoinCondition != string.Empty)
                {
                    selectQuery += " and " + m_sJoinCondition;
                }
                if (m_sOrderBy != "")
                {
                    selectQuery += " order by ";
                    selectQuery += m_sOrderBy;
                }
                else
                    selectQuery += " order by newid()";
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        string sCollectionVal = selectQuery.Table("query").DefaultView[i].Row["val"].ToString();
                        string sCollectiontxt = selectQuery.Table("query").DefaultView[i].Row["txt"].ToString();
                        string sID = m_sMiddleTable + sCollectionVal;
                        sID = sID.Replace("'", "");
                        sTD += "<a tabindex='2000' id='tag_" + sID + "' class='";
                        if (m_sStartValue.IndexOf(sCollectiontxt) != -1)
                            sTD += "tags_selected";
                        else
                            sTD += "tags";
                        sTD += "' ";
                        string sEncodedCollectiontxt = sCollectiontxt.Replace("'", "~~apos~~").Replace("&quot;", "~~qoute~~").Replace("\"", "~~qoute~~");
                        string sEncodedFieldHeader = m_sFieldHeader.Replace("'", "~~apos~~").Replace("&quot;", "~~qoute~~").Replace("\"", "~~qoute~~");
                        //sTD += " onclick='tagSelect(\"" + sID + "\",\"" + sEncodedCollectiontxt + "\",\"" + m_sFieldHeader + "\");return false;' ";
                        //sTD += " onclick='' ";
                        sTD += " href='javascript:tagSelect(\"" + sID + "\",\"" + sEncodedCollectiontxt + "\",\"" + sEncodedFieldHeader + "\");'>";
                        sTD += sCollectiontxt;
                        sTD += "</a>";
                        sTD += "&nbsp;&nbsp;&nbsp;";
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sTD;
        }

        virtual public string GetSelectsHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td nowrap class=\"adm_table_header_nbg\">";
            if (m_bMust == true)
                sTmp += "<font color=red>*&nbsp;&nbsp;</font>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td >";
            sTmp += "<table cellpadding=\"0\" cellspacing=\"0\">";
            sTmp += "<tr>";
            sTmp += "<td  class=\"align1\" >";

            //sTmp += "<textarea type='html' class='FormInput' name='" + nID.ToString() + "_val' id='" + m_sMiddleTable + "_coll' onkeyup='return tagKeyPress(\"" + m_sCollectionTable + "\" , \"" + m_sMiddleTable + "\" , \"" + m_sFieldName + "\" , \"" + m_sCollCss + "\");' ";
            sTmp += "<textarea tabindex=\"" + (nID + 1).ToString() + "\" class=\"FormInput\" id=\"" + m_sFieldHeader + "_coll\"  dir=\"" + m_sDir + "\" onkeyup='return tagKeyPress(\"" + m_sCollectionTable + "\" , \"" + m_sMiddleTable + "\" , \"" + m_sFieldName + "\" , \"" + m_sCollCss + "\" , \"" + m_sExtraWhere + "\" , \"" + m_sFieldHeader + "\" , \"" + m_sConnectionKey + "\");' name=\"" + nID.ToString() + "_val\" rows=\"3\" cols=\"" + m_nWidth.ToString() + "\" type=\"html\">";
            sTmp += m_sStartValue.ToString() + "</textarea>";

            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='multi'/>";
            // the field in the collection table
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            // the collection table
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_collection_table' value='" + m_sCollectionTable + "'/>";
            //The middle table
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_middle_table' value='" + m_sMiddleTable + "'/>";
            //The middle ref cell to the main
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_middle_ref_main_field' value='" + m_sMiddleMainFieldName + "'/>";
            //The middle ref cell to the collection
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_middle_ref_collection_field' value='" + m_sMiddleCollFieldName + "'/>";
            //The main pointer field
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_main_pointer_field' value='" + m_sMainPointerField + "'/>";

            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_middle_table_type' value='" + m_sMiddleTableType + "'/>";

            string sExtraFieldName = "";
            string sExtraFieldVal = "";
            string sExtraFieldType = "";
            if (m_sExtraWhere != "")
            {
                string[] toSplitWith = { "=" };
                string[] splited = m_sExtraWhere.Split(toSplitWith, StringSplitOptions.RemoveEmptyEntries);
                if (splited.Length == 2)
                {
                    sExtraFieldName = splited[0].ToString();
                    sExtraFieldVal = splited[1].ToString();
                    if (sExtraFieldVal.StartsWith("'") == true)
                    {
                        sExtraFieldType = "string";
                        sExtraFieldVal = sExtraFieldVal.Substring(1, sExtraFieldVal.Length - 2);
                    }
                    else
                    {
                        sExtraFieldType = "int";
                    }
                }
            }
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_extra_field_name' value='" + sExtraFieldName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_extra_field_val' value='" + sExtraFieldVal + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_extra_field_type' value='" + sExtraFieldType + "'/>";
            //The main pointer field
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_collection_pointer_field' value='" + m_sCollectionPointerField + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_collection_auto_add' value='" + m_bAddExtra.ToString() + "'/>";
            sTmp += "</td>";
            sTmp += "<td nowrap=\"nowrap\"></td>";
            sTmp += "</tr>";
            sTmp += "<tr>";
            sTmp += "<td class=\"tags\" colspan=\"2\"><div>";
            sTmp += GetCollectionTDString();
            sTmp += "</div></td>";
            sTmp += "</tr></table></td>";
            return sTmp;
        }

        public override string GetFieldHtml(long nID)
        {
            //string sTmp = "<tr><td width=100% colspan=3 id=\"multi_" + nID.ToString() + "\">";
            //sTmp += GetSelectsHtml(nID);
            //sTmp += "</td></tr>";
            return GetSelectsHtml(nID);
        }
    }

    public class DataRecordBrowserField : DataRecordOnePicBrowserField
    {
        private string m_functionName;
        private string m_sFieldType;
        private string className;

        public DataRecordBrowserField(string functionName)
            : base()
        {
            m_functionName = functionName;
            m_sFieldType = "browser";
            className = "btn_mediaTypes";
        }

        public DataRecordBrowserField(string functionName, string lastPage)
            : base()
        {
            m_functionName = functionName;
            m_sFieldType = "browser";
            className = "btn_mediaTypes";
        }

        public void SetClassName(string className)
        {
            this.className = className;
        }

        public override string GetFieldHtml(long nID)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";

            sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += "<table width=100%>";
            if (m_sStartValue != "")
            {
                Int32 nchannelID = 0;
                try
                {
                    nchannelID = int.Parse(m_sStartValue);
                }
                catch
                {
                }
            }
            sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_media_type\">";
            sTmp += "</td></tr>";

            sTmp += "<tr><td colspan=2 class=\"align1\">";

            sTmp += "<input tabindex=\"2000\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='";
            sTmp += "hidden' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=8 ";
            sTmp += "maxlength=8 ";
            if (m_sStartValue != "")
                sTmp += "value='" + m_sStartValue.ToString() + "' ";

            sTmp += "/><a tabindex=\"" + (nID + 1).ToString() + "\" class=\"" + className + "\" onclick=\"" + m_functionName + "('" + nID.ToString() + "_val' , '" + base.getLastPageName() + "');\" href=\"javascript:void(0);\"></a>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='" + m_sFieldType + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";

            sTmp += "</td>";
            sTmp += "</tr>";
            sTmp += "</table></td></tr>";
            return sTmp;
        }

    }

    public class DataRecordOnePicBrowserField : BaseDataRecordField
    {
        private string m_lastPage;
        private string m_epgIdentifier;
        private int m_channelID;
        private int picId;
        private bool isDownloadPicWithImageServer;
        private string ImageUrl;

        public DataRecordOnePicBrowserField()
            : base()
        {
            m_lastPage = string.Empty;
            m_epgIdentifier = string.Empty;
        }

        public DataRecordOnePicBrowserField(string lastPage)
            : base()
        {
            m_lastPage = lastPage;
        }

        public DataRecordOnePicBrowserField(string lastPage, string epgIdentifier, int channelID)
            : base()
        {
            m_lastPage = lastPage;
            m_epgIdentifier = epgIdentifier;
            m_channelID = channelID;
        }

        public DataRecordOnePicBrowserField(string lastPage, string epgIdentifier, int channelID, bool isDownloadPicWithImageServer, string imageUrl, int picId)
            : base()
        {
            m_lastPage = lastPage;
            m_epgIdentifier = epgIdentifier;
            m_channelID = channelID;
            this.isDownloadPicWithImageServer = isDownloadPicWithImageServer;
            this.ImageUrl = imageUrl;
            this.picId = picId;
        }

        public DataRecordOnePicBrowserField(string lastPage, bool isNewPicSelector, string imageUrl, int picId)
        {
            this.m_lastPage = lastPage;
            this.isDownloadPicWithImageServer = isNewPicSelector;
            this.ImageUrl = imageUrl;
            this.picId = picId;
        }

        public DataRecordOnePicBrowserField(bool isDownloadPicWithImageServer, string imageUrl, int picId)
        {
            this.isDownloadPicWithImageServer = isDownloadPicWithImageServer;
            this.ImageUrl = imageUrl;
            this.picId = picId;
        }

        protected string getLastPageName()
        {
            return m_lastPage;
        }

        public override string GetFieldHtml(long nID)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";

            string sBasePicsURL = "";

            if (isDownloadPicWithImageServer && !string.IsNullOrEmpty(ImageUrl))
            {
                sBasePicsURL = ImageUrl;
            }
            else
            {
                object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
                if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
                    sBasePicsURL = oBasePicsURL.ToString();
                if (sBasePicsURL == "")
                    sBasePicsURL = "pics";
                else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
                    sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
                    sBasePicsURL = "http://" + sBasePicsURL;
            }

            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += "<table width=100%>";
            if (m_sStartValue != "")
            {
                string sPicID = m_sStartValue;
                if (m_sStartValue.EndsWith(";"))
                    sPicID = sPicID.Substring(0, sPicID.Length - 1);
                Int32 nPicID = 0;
                try
                {
                    nPicID = int.Parse(sPicID);
                }
                catch
                {
                }

                if (nPicID > 0 || !string.IsNullOrEmpty(ImageUrl))
                {
                    object oPic = PageUtils.GetTableSingleVal("pics", "BASE_URL", int.Parse(sPicID));
                    if (oPic != DBNull.Value && oPic != null)
                    {
                        sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                        string sPicURL = ImageUtils.GetTNName(oPic.ToString(), "tn");
                        if (sBasePicsURL.EndsWith("=") == true)
                        {
                            string sTmp1 = "";
                            string[] s = sPicURL.Split('.');
                            for (int i = 0; i < s.Length - 1; i++)
                            {
                                if (i > 0)
                                    sTmp1 += ".";
                                sTmp1 += s[i];
                            }
                            sPicURL = sTmp1;
                        }
                        sTmp += "<img src=\"";
                        sTmp += sBasePicsURL;
                        if (sBasePicsURL.EndsWith("=") == false)
                            sTmp += "/";
                        Random random = new Random();
                        int randomInt = random.Next();
                        sTmp += sPicURL + "\" class=\"img_border\"/>";
                        sTmp += "</td></tr>";
                    }
                    else
                    {
                        sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                        sTmp += "</td></tr>";
                    }
                }
                else
                {
                    sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                    sTmp += "</td></tr>";
                }
            }
            else
            {
                sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                sTmp += "</td></tr>";
            }
            sTmp += "<tr><td colspan=2 class=\"align1\">";

            sTmp += "<input tabindex=\"2000\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='";
            sTmp += "hidden' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=8 ";
            sTmp += "maxlength=8 ";
            if (m_sStartValue != "")
                sTmp += "value='" + m_sStartValue.ToString() + "' ";

            string onClickMethod = "OpenPicBrowser";
            if (isDownloadPicWithImageServer)
            {
                onClickMethod = "OpenPicUploaderBrowser";
            }

            sTmp += "/><a tabindex=\"" + (nID + 1).ToString() + "\" class=\"btn_browse\" onclick=\"" + onClickMethod + "('" + nID.ToString() + "_val' , 1, '" + m_lastPage + "');\" href=\"javascript:void(0);\"></a>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='int'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";

            sTmp += "</td>";
            sTmp += "</tr>";
            sTmp += "</table></td></tr>";
            return sTmp;
        }

        public string GetFieldHtmlCB(long nID)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();

            string sBasePicsURL = "";

            if (isDownloadPicWithImageServer && !string.IsNullOrEmpty(ImageUrl))
            {
                sBasePicsURL = ImageUrl;
            }
            else
            {
                object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
                if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
                    sBasePicsURL = oBasePicsURL.ToString();
                if (sBasePicsURL == "")
                    sBasePicsURL = "pics";
                else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
                    sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
                    sBasePicsURL = "http://" + sBasePicsURL;
            }

            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += "<table width=100%>";
            if (m_sStartValue != "")
            {
                string sPicID = m_sStartValue;
                if (m_sStartValue.EndsWith(";"))
                    sPicID = sPicID.Substring(0, sPicID.Length - 1);
                Int32 nPicID = 0;
                try
                {
                    nPicID = int.Parse(sPicID);
                }
                catch
                {
                }

                if (nPicID > 0)
                {
                    object oPic = PageUtils.GetTableSingleVal("EPG_pics", "BASE_URL", int.Parse(sPicID));
                    if (oPic != DBNull.Value && oPic != null)
                    {
                        sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                        string sPicURL = ImageUtils.GetTNName(oPic.ToString(), "tn");
                        if (sBasePicsURL.EndsWith("=") == true)
                        {
                            string sTmp1 = "";
                            string[] s = sPicURL.Split('.');
                            for (int i = 0; i < s.Length - 1; i++)
                            {
                                if (i > 0)
                                    sTmp1 += ".";
                                sTmp1 += s[i];
                            }
                            sPicURL = sTmp1;
                        }
                        sTmp += "<img src=\"";
                        sTmp += sBasePicsURL;
                        if (sBasePicsURL.EndsWith("=") == false)
                            sTmp += "/";
                        Random random = new Random();
                        int randomInt = random.Next();
                        sTmp += sPicURL + "\" class=\"img_border\"/>";
                        sTmp += "</td></tr>";
                    }
                    else
                    {
                        sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                        sTmp += "</td></tr>";
                    }
                }
                else
                {
                    sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                    sTmp += "</td></tr>";
                }
            }
            else
            {
                sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_pic_beowse\">";
                sTmp += "</td></tr>";
            }
            sTmp += "<tr><td colspan=2 class=\"align1\">";

            sTmp += "<input tabindex=\"2000\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='";
            sTmp += "hidden' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=8 ";
            sTmp += "maxlength=8 ";
            if (m_sStartValue != "")
                sTmp += "value='" + m_sStartValue.ToString() + "' ";

            string onClickMethod = "OpenPicBrowserEpg";
            if (isDownloadPicWithImageServer)
            {
                onClickMethod = "OpenPicUploaderBrowserEPG";
            }
            
            sTmp += "/><a tabindex=\"" + (nID + 1).ToString() + "\" class=\"btn_browse\" onclick=\"" + onClickMethod + "('" + nID.ToString() + "_val' , 1, '" + m_lastPage + "','" + m_epgIdentifier + "','" + m_channelID + "');\" href=\"javascript:void(0);\"></a>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='int'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "</td>";
            sTmp += "</tr>";
            sTmp += "</table></td></tr>";
            return sTmp;
        }
    }

    public class DataRecordOneVideoBrowserField : BaseDataRecordField
    {
        protected string m_sVidTable;
        protected string m_sVidTableTags;
        protected string m_sVidTableTagsRef;
        public DataRecordOneVideoBrowserField(string sVidTable, string vidTableTags, string vidTableTagsRef)
            : base()
        {
            m_sVidTable = sVidTable;
            m_sVidTableTags = vidTableTags;
            m_sVidTableTagsRef = vidTableTagsRef;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += "<table width=100%>";
            if (m_sStartValue != "")
            {
                string sVidID = m_sStartValue;
                if (m_sStartValue.EndsWith(";"))
                    sVidID = sVidID.Substring(0, sVidID.Length - 1);
                Int32 nVidID = 0;
                try
                {
                    nVidID = int.Parse(sVidID);
                }
                catch
                {
                }

                if (nVidID > 0)
                {
                    Int32 nMediaVidID = 0;
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select mf.id from media_files mf where mf.status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.media_id", "=", nVidID);
                    selectQuery += " order by mf.MEDIA_TYPE_ID";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            nMediaVidID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                    string sPre = "";
                    if (m_sFieldName.IndexOf("STREAMING_CODE") > 0)
                        sPre = m_sFieldName.Substring(0, m_sFieldName.Length - m_sFieldName.IndexOf("STREAMING_CODE"));
                    DataRecordMediaViewerField dr_player = new DataRecordMediaViewerField(sPre, nMediaVidID);
                    string sName = PageUtils.GetTableSingleVal("media", "name", nVidID).ToString();
                    dr_player.Initialize("Video", "adm_table_header_nbg", "FormInput", "STREAMING_CODE", false);
                    string sPicURL = dr_player.GetTNImage();
                    if (sPicURL != "")
                    {
                        sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_vid_beowse\">";
                        string sObjectID = nID.ToString() + "_val";
                        sTmp += "<img style=\"cursor: pointer;\" onclick=\"ChangeVideoPlayer('" + sObjectID + "','" + dr_player.GetPlayerSrc() + "');\" src=\"";
                        sTmp += sPicURL + "\" class=\"img_border\"/>";
                        sTmp += "<div class=\"vid_name\">\r\n" + sName + "</div>\r\n";
                        sTmp += "</td></tr>";
                        sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_val_palyer\" class=\"align1\">";
                        sTmp += dr_player.GetPlayerFrame();
                        sTmp += "</td></tr>";
                    }
                    else
                    {
                        sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_vid_beowse\">";
                        sTmp += "</td></tr>";
                    }
                }
                else
                {
                    sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_vid_beowse\">";
                    sTmp += "</td></tr>";
                }
            }
            else
            {
                sTmp += "<tr><td colspan=\"3\" id=\"" + nID.ToString() + "_vid_beowse\">";
                sTmp += "</td></tr>";
            }
            sTmp += "<tr><td colspan=2 class=\"align1\">";
            sTmp += "<input tabindex=\"2000\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='";
            sTmp += "hidden' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=8 ";
            sTmp += "maxlength=8 ";
            if (m_sStartValue != "")
                sTmp += "value='" + m_sStartValue.ToString() + "' ";
            sTmp += "/><a class=\"btn_vid_browse\" href=\"javascript:OpenVidBrowser('" + nID.ToString() + "_val' , 1 , '" + m_sVidTable + "','" + m_sVidTableTags + "','" + m_sVidTableTagsRef + "');\"></a>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='int'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "</td>";
            sTmp += "</tr>";
            sTmp += "</table></td></tr>";
            return sTmp;
        }
    }

    public class DataRecordLongTextField : BaseDataRecordField
    {
        protected bool m_bEnabled;
        protected string m_sDir;
        protected long m_nWidth;
        protected long m_nHeight;
        protected string m_filedPrivateName;
        public DataRecordLongTextField(string sDir, bool bEnabled, long nWidth, long nHeight)
            : base()
        {
            m_bEnabled = bEnabled;
            m_sDir = sDir;
            m_nWidth = nWidth;
            m_nHeight = nHeight;
            m_filedPrivateName = string.Empty;
        }
        public void setFiledName(string name)
        {
            m_filedPrivateName = name;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += "<textarea  tabindex=\"" + (nID + 1).ToString() + "\" type='html' class='" + m_sInputCss + "' id='" + nID.ToString() + "_val' name='" + nID.ToString() + "_val' ";
            if (m_bEnabled == false)
                sTmp += "disabled ";
            sTmp += "dir='" + m_sDir + "' ";
            sTmp += "cols=" + m_nWidth.ToString() + " ";
            sTmp += "rows=" + m_nHeight.ToString() + " ";
            sTmp += ">" + m_sStartValue.ToString().Replace("&lt;br\\&gt;", "\r\n") + "</textarea>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='long_string'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_fieldName' value='" + m_filedPrivateName + "'/>";
            sTmp += "</td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordDropDownField : BaseDataRecordField
    {
        string m_sRefTable;
        string m_sTextField;
        string m_sValueField;
        string m_sWhereField;
        object m_sWhereVal;
        string m_sOnString;
        string m_sOrderBy;
        string m_sWhereStr;
        Int32 m_nWidth;
        Int32 m_nSize;
        bool m_bWithNoSelect;
        bool m_bEnabled;
        string m_sQueryString;
        string m_sFieldType;
        DataTable m_dtQueryDT;
        //string m_sDefaultVal;
        string m_sNoSelectStr;
        protected string m_filedPrivateName;
        public DataRecordDropDownField(string sRefTable, string sTextField, string sValueField, string sWhereField, object sWhereVal, Int32 nWidth, bool bWithNoSelect)
            : base()
        {
            m_nSize = 0;
            m_sDefaultVal = "";
            m_sOrderBy = "";
            m_bWithNoSelect = bWithNoSelect;
            m_bEnabled = true;
            m_sRefTable = sRefTable;
            m_sTextField = sTextField;
            m_sValueField = sValueField;
            m_sWhereField = sWhereField;
            m_sWhereVal = sWhereVal;
            m_nWidth = nWidth;
            m_sWhereStr = "";
            m_sNoSelectStr = "---";
            m_sQueryString = "";
            m_sFieldType = "int";
            m_dtQueryDT = null;
            m_filedPrivateName = string.Empty;
        }

        public void setFiledName(string name)
        {
            m_filedPrivateName = name;
        }

        public void SetSelectsDT(DataTable dt)
        {
            m_dtQueryDT = dt.Copy();
        }

        public void SetFieldType(string sType)
        {
            m_sFieldType = sType;
        }

        public void SetListHeight(Int32 nSize)
        {
            m_nSize = nSize;
        }

        public void SetDefaultVal(string sDefault)
        {
            m_sDefaultVal = sDefault;
        }

        public void SetEnable(bool bEnabled)
        {
            m_bEnabled = bEnabled;
        }

        public void SetOnString(string sOnString)
        {
            m_sOnString = sOnString;
        }

        public void SetOrderBy(string sOrderBy)
        {
            m_sOrderBy = sOrderBy;
        }

        public void SetWhereString(string sOnString)
        {
            m_sWhereStr = sOnString;
        }

        public void SetNoSelectStr(string sNoSelectStr)
        {
            m_sNoSelectStr = sNoSelectStr;
        }

        public void SetSelectsQuery(string sQuery)
        {
            m_sQueryString = sQuery;
        }

        public string GetSelectsHtml(long nID)
        {
            string sTmp = "";
            sTmp += "<select  tabindex=\"" + (nID + 1).ToString() + "\" ";
            if (m_bEnabled == false)
                sTmp += "disabled ";
            sTmp += "id='selector" + nID.ToString() + "'";
            sTmp += m_sOnString;
            sTmp += " name='" + nID.ToString() + "_val' class='" + m_sInputCss + "'";
            if (m_nSize > 0)
                sTmp += " size=" + m_nSize.ToString();
            sTmp += " >";
            if (m_dtQueryDT == null)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                if (m_sConnectionKey != "")
                    selectQuery.SetConnectionKey(m_sConnectionKey);
                if (m_sTextField.IndexOf("+") != -1)
                {
                    string[] splited = m_sTextField.Split('+');
                    m_sTextField = "(";
                    for (int j = 0; j < splited.Length; j++)
                    {
                        if (j > 0)
                            m_sTextField += "+' '+";
                        m_sTextField += splited[j];
                    }
                    m_sTextField += ")";
                }
                if (m_sQueryString == "")
                {
                    selectQuery += "select " + m_sTextField + " as txt," + m_sValueField + " from " + m_sRefTable;
                    if (m_sWhereField != "")
                    {
                        selectQuery += " where ";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM(m_sWhereField, "=", m_sWhereVal);
                        if (m_sWhereStr != "")
                        {
                            selectQuery += "and";
                            selectQuery += m_sWhereStr;
                        }
                    }
                    else
                    {
                        if (m_sWhereStr != "")
                        {
                            selectQuery += " where ";
                            selectQuery += m_sWhereStr;
                        }
                    }
                    if (m_sOrderBy != "")
                    {
                        selectQuery += " order by ";
                        selectQuery += m_sOrderBy;
                    }
                }
                else
                    selectQuery += m_sQueryString;
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount == 0)
                        sTmp += "<option width=" + m_nWidth.ToString() + "  value='' selected>" + m_sNoSelectStr + "</option>";
                    else
                    {
                        if (m_bWithNoSelect == true)
                        {
                            sTmp += "<option width=" + m_nWidth.ToString() + "  value='0'";
                            if (m_sStartValue == "" && m_nDefault == -1 && m_sDefaultVal == "")
                                sTmp += " selected ";
                            sTmp += ">" + m_sNoSelectStr + "</option>";
                        }
                    }
                    for (int i = 0; i < nCount; i++)
                    {
                        string sText = selectQuery.Table("query").DefaultView[i].Row["txt"].ToString();
                        string sVal = "";
                        if (m_sValueField.IndexOf(" as ") != -1)
                            sVal = selectQuery.Table("query").DefaultView[i].Row[m_sValueField.Substring(m_sValueField.IndexOf(" as ") + 4)].ToString();
                        else
                            sVal = selectQuery.Table("query").DefaultView[i].Row[m_sValueField].ToString();
                        sTmp += "<option width=" + m_nWidth.ToString() + "  value='" + sVal + "'";
                        if (sVal == m_sStartValue || (m_sStartValue == "" && i == m_nDefault) || (m_sStartValue == "" && sText == m_sDefaultVal) || (m_sStartValue == "" && sVal == m_sDefaultVal))
                        {
                            sTmp += " selected ";
                        }
                        sTmp += ">" + sText + "</option>";
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            else
            {
                Int32 nCount = m_dtQueryDT.DefaultView.Count; ;
                if (nCount == 0)
                    sTmp += "<option width=" + m_nWidth.ToString() + "  value='' selected>" + m_sNoSelectStr + "</option>";
                else
                {
                    if (m_bWithNoSelect == true)
                    {
                        sTmp += "<option width=" + m_nWidth.ToString() + "  value='0'";
                        if (m_sStartValue == "" && m_nDefault == -1 && m_sDefaultVal == "")
                            sTmp += " selected ";
                        sTmp += ">" + m_sNoSelectStr + "</option>";
                    }
                }
                for (int i = 0; i < nCount; i++)
                {
                    string sText = m_dtQueryDT.DefaultView[i].Row["txt"].ToString();
                    string sVal = "";
                    if (m_sValueField.IndexOf(" as ") != -1)
                        sVal = m_dtQueryDT.DefaultView[i].Row[m_sValueField.Substring(m_sValueField.IndexOf(" as ") + 4)].ToString();
                    else
                        sVal = m_dtQueryDT.DefaultView[i].Row[m_sValueField].ToString();
                    sTmp += "<option width=" + m_nWidth.ToString() + "  value='" + sVal + "'";
                    if (sVal == m_sStartValue || (m_sStartValue == "" && i == m_nDefault) || (m_sStartValue == "" && sText == m_sDefaultVal) || (m_sStartValue == "" && sVal == m_sDefaultVal))
                    {
                        sTmp += " selected ";
                    }
                    sTmp += ">" + sText + "</option>";
                }
            }
            sTmp += "</select>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='" + m_sFieldType + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_fieldName' value='" + m_filedPrivateName + "'/>";
            return sTmp;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\" id=td_selector" + nID.ToString() + ">";
            //sTmp += "<td align=right   width=100% nowrap id=td_selector" + nID.ToString() + ">";
            sTmp += GetSelectsHtml(nID);
            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordRadioField : BaseDataRecordField
    {
        string m_sRefTable;
        string m_sTextField;
        string m_sValueField;
        string m_sWhereField;
        object m_sWhereVal;
        string m_sOrderBy;
        string m_sWhereStr;
        string m_sQueryString;
        string m_sFieldType;
        DataTable m_dtQueryDT;
        protected string m_filedPrivateName;
        public void setFiledName(string name)
        {
            m_filedPrivateName = name;
        }
        public DataRecordRadioField(string sRefTable, string sTextField, string sValueField, string sWhereField, object sWhereVal)
            : base()
        {
            m_sDefaultVal = "";
            m_sOrderBy = "";
            m_sRefTable = sRefTable;
            m_sTextField = sTextField;
            m_sValueField = sValueField;
            m_sWhereField = sWhereField;
            m_sWhereVal = sWhereVal;
            m_sWhereStr = "";
            m_sQueryString = "";
            m_dtQueryDT = null;
            m_sFieldType = "int";
        }

        public void SetFieldType(string sType)
        {
            m_sFieldType = sType;
        }

        public void SetSelectsQuery(string sQuery)
        {
            m_sQueryString = sQuery;
        }

        public void SetSelectsDT(DataTable dt)
        {
            m_dtQueryDT = dt.Copy();
        }

        public void SetDefaultVal(string sDefault)
        {
            m_sDefaultVal = sDefault;
        }

        public void SetOrderBy(string sOrderBy)
        {
            m_sOrderBy = sOrderBy;
        }

        public void SetWhereString(string sOnString)
        {
            m_sWhereStr = sOnString;
        }

        public string GetSelectsHtml(long nID)
        {
            string sTmp = "";
            if (m_dtQueryDT == null)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                if (m_sTextField.IndexOf("+") != -1)
                {
                    string[] splited = m_sTextField.Split('+');
                    m_sTextField = "(";
                    for (int j = 0; j < splited.Length; j++)
                    {
                        if (j > 0)
                            m_sTextField += "+' '+";
                        m_sTextField += splited[j];
                    }
                    m_sTextField += ")";
                }
                if (m_sConnectionKey != "")
                    selectQuery.SetConnectionKey(m_sConnectionKey);
                if (m_sQueryString == "")
                {
                    selectQuery += "select " + m_sTextField + " as txt," + m_sValueField + " from " + m_sRefTable;

                    if (m_sWhereField != "")
                    {
                        selectQuery += " where ";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM(m_sWhereField, "=", m_sWhereVal);
                        if (m_sWhereStr != "")
                        {
                            selectQuery += "and";
                            selectQuery += m_sWhereStr;
                        }
                    }
                    else
                    {
                        if (m_sWhereStr != "")
                        {
                            selectQuery += " where ";
                            selectQuery += m_sWhereStr;
                        }
                    }
                    if (m_sOrderBy != "")
                    {
                        selectQuery += " order by ";
                        selectQuery += m_sOrderBy;
                    }
                }
                else
                    selectQuery += m_sQueryString;
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount == 0)
                        sTmp += "<input  tabindex=\"" + (nID + 1).ToString() + "\" value='0' type='radio' name='" + nID.ToString() + "_val' class='" + m_sHeaderCss + "'>---";
                    for (int i = 0; i < nCount; i++)
                    {
                        string sText = selectQuery.Table("query").DefaultView[i].Row["txt"].ToString();
                        string sVal = selectQuery.Table("query").DefaultView[i].Row[m_sValueField].ToString();
                        sTmp += "<input  tabindex=\"" + (nID + 1).ToString() + "\" type='radio' name='" + nID.ToString() + "_val' value='" + sVal + "' class='" + m_sHeaderCss + "'";
                        if (sVal == m_sStartValue || (m_sStartValue == "" && i == m_nDefault) || (m_sStartValue == "" && sText == m_sDefaultVal))
                        {
                            sTmp += " checked ";
                        }
                        sTmp += "><span class='FormInputnbg'>" + sText + "</span><br/>";
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            else
            {
                Int32 nCount = m_dtQueryDT.DefaultView.Count;
                if (nCount == 0)
                    sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" value='0' type='radio' name='" + nID.ToString() + "_val' class='" + m_sHeaderCss + "'>---";
                for (int i = 0; i < nCount; i++)
                {
                    string sText = m_dtQueryDT.DefaultView[i].Row["txt"].ToString();
                    string sVal = m_dtQueryDT.DefaultView[i].Row[m_sValueField].ToString();
                    sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" type='radio' name='" + nID.ToString() + "_val' value='" + sVal + "' class='" + m_sHeaderCss + "'";
                    if (sVal == m_sStartValue || (m_sStartValue == "" && i == m_nDefault) || (m_sStartValue == "" && sText == m_sDefaultVal))
                    {
                        sTmp += " checked ";
                    }
                    sTmp += "><span class='FormInputnbg'>" + sText + "</span><br/>";
                }
            }
            sTmp += "</select>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='" + m_sFieldType + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_inputtype' value='radio'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            return sTmp;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\" id=td_selector" + nID.ToString() + ">";
            sTmp += GetSelectsHtml(nID);
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_fieldName' value='" + m_filedPrivateName + "'/>";
            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }
    
    public class DataRecordTimeField : BaseDataRecordField
    {
        public DataRecordTimeField()
            : base()
        { }

        public override string GetFieldType()
        {
            return "time";
        }

        public string GetSelectsHtml(long nID)
        {
            string sRefStartVal = "";

            if (m_sStartValue != "")
                sRefStartVal = m_sStartValue.Split(':')[0].ToString();

            string sTmp = "";

            sTmp += "<input  tabindex=\"" + (nID + 1).ToString() + "\" tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='text' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=2 ";
            sTmp += "maxlength=2 ";
            if (sRefStartVal != "")
            {
                if (sRefStartVal.Length == 1)
                    sRefStartVal = "0" + sRefStartVal;
                sTmp += "value='" + sRefStartVal.ToString() + "' ";
            }
            sTmp += "/>";

            sTmp += " : ";

            if (m_sStartValue != "")
                sRefStartVal = m_sStartValue.Split(':')[1].ToString();

            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val2' type='text' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=2 ";
            sTmp += "maxlength=2 ";
            if (sRefStartVal != "")
            {
                if (sRefStartVal.Length == 1)
                    sRefStartVal = "0" + sRefStartVal;
                sTmp += "value='" + sRefStartVal.ToString() + "' ";
            }
            sTmp += "/>";

            return sTmp;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\" id=td_selector" + nID.ToString() + ">";
            sTmp += GetSelectsHtml(nID);
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='time'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordUploadField : BaseDataRecordField
    {
        protected long m_nWidth;
        protected string m_sDir;
        protected System.Collections.ArrayList m_PicDimensions;
        bool m_bIsPic;
        string m_ratioIndex;
        public DataRecordUploadField(long nWidth, string sPicsDirectory, bool isPic)
            : base()
        {
            m_nWidth = nWidth;
            m_sDir = sPicsDirectory;
            m_bIsPic = isPic;
            m_PicDimensions = new System.Collections.ArrayList();
        }

        public DataRecordUploadField(long nWidth, string sPicsDirectory, bool isPic, string ratioIndex)
            : base()
        {
            m_nWidth = nWidth;
            m_sDir = sPicsDirectory;
            m_bIsPic = isPic;
            m_ratioIndex = ratioIndex;
            m_PicDimensions = new System.Collections.ArrayList();
        }

        public void AddPicDimension(Int32 nWidth, Int32 nHeight, string sNameEnd, bool bCrop)
        {
            PicDimension theDim = new PicDimension(nWidth, nHeight, sNameEnd, bCrop);
            m_PicDimensions.Add(theDim);
        }

        public void AddPicDimension(Int32 nWidth, Int32 nHeight, string sNameEnd, bool bCrop, string sRatio)
        {
            PicDimension theDim = new PicDimension(nWidth, nHeight, sNameEnd, bCrop, sRatio);
            m_PicDimensions.Add(theDim);
        }

        public override string GetFieldType()
        {
            return "upload";
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "";
            string[] s = m_sStartValue.Split('.');
            if (m_sStartValue != "")
            {
                sTmp += "<tr><td colspan=\"3\">";
                if (s.Length == 2 && m_bIsPic == true)
                {
                    string sPic = s[0] + "_tn." + s[1];
                    string sFullPath = HttpContext.Current.Server.MapPath("") + "\\" + m_sDir + "\\" + sPic;
                    if (System.IO.File.Exists(sFullPath) == true)
                    {
                        sTmp += "<img src=\"";
                        sTmp += m_sDir + "/" + sPic + "\" />";
                    }
                }
                sTmp += "</td></tr>";
            }
            sTmp += "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\" >";
            if (s.Length == 2 && m_bIsPic == false)
            {
                string sPic = s[0] + "." + s[1];
                string sFullPath = HttpContext.Current.Server.MapPath("") + "\\" + m_sDir + "\\" + sPic;
                string sPicURL = "<a href=\"" + "http://tvm.tvinci.com/" + m_sDir + "/" + sPic + "\" target=\"_blank\">" + sPic + "</a>";
                if (System.IO.File.Exists(sFullPath) == true)
                {
                    sTmp += "<span>(Current : ";
                    sTmp += (string.IsNullOrEmpty(sPic) ? "- No selection -" : sPicURL);
                    sTmp += ")</span>";
                }
            }
            sTmp += "</br>";
            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='file' ";
            sTmp += "size=" + m_nWidth.ToString() + " ";
            m_sStartValue = HttpContext.Current.Server.HtmlDecode(m_sStartValue).Replace("\"", "''");
            if (m_sStartValue != "")
                sTmp += "value=\"" + m_sStartValue.ToString() + "\" ";
            sTmp += "/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='file'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_isPic' value='" + m_bIsPic.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_directory' value='" + m_sDir + "'/>";
            if (!string.IsNullOrEmpty(m_ratioIndex))
            {
                sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_ratioIndex' value='" + m_ratioIndex + "'/>";
            }
            System.Collections.IEnumerator iter = m_PicDimensions.GetEnumerator();
            Int32 nIterCount = 0;
            while (iter.MoveNext())
            {
                string sWidthName = "_picDim_width_" + nIterCount.ToString();
                string sHeightName = "_picDim_height_" + nIterCount.ToString();
                string sEndName = "_picDim_endname_" + nIterCount.ToString();
                string sCropName = "_crop_" + nIterCount.ToString();
                string sRatioName = "_picDim_ratio_" + nIterCount.ToString();
                string sRatioIndex = "_picDim_ratioIndex_" + nIterCount.ToString();
                sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + sWidthName + "' value='" + ((PicDimension)(iter.Current)).m_nWidth.ToString() + "'/>";
                sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + sHeightName + "' value='" + ((PicDimension)(iter.Current)).m_nHeight.ToString() + "'/>";
                sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + sEndName + "' value='" + ((PicDimension)(iter.Current)).m_sNameEnd.ToString() + "'/>";
                sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + sCropName + "' value='" + ((PicDimension)(iter.Current)).m_bCorp.ToString() + "'/>";
                if (!string.IsNullOrEmpty(((PicDimension)(iter.Current)).m_ratio))
                {
                    sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + sRatioName + "' value='" + ((PicDimension)(iter.Current)).m_ratio.ToString() + "'/>";
                }

                nIterCount++;
            }
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_picDim' value='" + m_bIsPic.ToString() + "'/>";
            sTmp += "</td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordTextEditorField : BaseDataRecordField
    {
        protected bool m_bEnabled;
        protected string m_sDir;
        protected long m_nWidth;
        protected long m_nHeight;
        public DataRecordTextEditorField(string sDir, bool bEnabled, long nWidth, long nHeight)
            : base()
        {
            m_bEnabled = bEnabled;
            m_sDir = sDir;
            m_nWidth = nWidth;
            m_nHeight = nHeight;
        }

        public override string GetFieldHtml(long nID)
        {

            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += "<textarea tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' id='" + nID.ToString() + "_val' name='" + nID.ToString() + "_val' ";
            if (m_bEnabled == false)
                sTmp += "disabled ";
            sTmp += "dir='" + m_sDir + "' ";
            sTmp += "cols=" + m_nWidth.ToString() + " ";
            sTmp += "rows=" + m_nHeight.ToString() + " ";
            sTmp += ">" + m_sStartValue.ToString() + "</textarea>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='long_string'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "</td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordDateField : BaseDataRecordField
    {
        protected bool m_bEnabled;

        public override string GetFieldType()
        {
            return "date";
        }
        public DataRecordDateField(bool bEnabled)
            : base()
        {
            m_bEnabled = bEnabled;
        }

        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\" id=td_selector" + nID.ToString() + ">";
            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' id='Date_" + nID.ToString() + "' name='" + nID.ToString() + "_val' ";
            if (m_bEnabled == true)
                sTmp += "type='text' ";
            else
                sTmp += "type='hidden' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=12 ";
            if (m_sStartValue != "")
                sTmp += "value='" + m_sStartValue.ToString() + "' ";
            sTmp += ">&nbsp;";
            if (m_bEnabled == true)
                sTmp += "<img src='images/icon_calendar.gif' border=0 onclick='javascript:Initialization(document.form1.Date_" + nID.ToString() + ");'>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='date'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordDateTimeField : BaseDataRecordField
    {
        protected bool m_bEnabled;
        protected string m_filedPrivateName;
        protected string m_timeZone;


        public override string GetFieldType()
        {
            return "datetime";
        }

        public void setFiledName(string name)
        {
            m_filedPrivateName = name;
        }
        public void setTimeZone(string timeZone)
        {
            if (!string.IsNullOrEmpty(timeZone))
            {
                m_timeZone = timeZone;
            }
        }

        public DataRecordDateTimeField(bool bEnabled)
            : base()
        {
            m_bEnabled = bEnabled;
            m_filedPrivateName = string.Empty;
            m_timeZone = string.Empty;
        }

        public void SetDefault(DateTime t)
        {
            string sDefault = "";
            if (int.Parse(t.Day.ToString()) < 10)
                sDefault += "0";
            sDefault += t.Day.ToString() + "/";
            if (int.Parse(t.Month.ToString()) < 10)
                sDefault += "0";
            sDefault += t.Month.ToString() + "/";
            sDefault += t.Year.ToString() + " ";
            if (int.Parse(t.Hour.ToString()) < 10)
                sDefault += "0";
            sDefault += t.Hour.ToString() + ":";
            if (int.Parse(t.Minute.ToString()) < 10)
                sDefault += "0";
            sDefault += t.Minute.ToString() + ":";
            if (int.Parse(t.Second.ToString()) < 10)
                sDefault += "0";
            sDefault += t.Second.ToString();


            m_sDefaultVal = sDefault;
        }

        public string GetSelectsHtml(long nID, string sStartMin, string sStartHour)
        {
            string sTmp = "";

            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_valHour' type='text' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=2 ";
            sTmp += "maxlength=2 ";
            if (sStartHour != "")
            {
                if (sStartHour.Length == 1)
                    sStartHour = "0" + sStartHour;
                sTmp += "value='" + sStartHour + "' ";
            }
            sTmp += "/>";

            sTmp += " : ";

            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_valMin' type='text' ";
            sTmp += "dir='ltr' ";
            sTmp += "size=2 ";
            sTmp += "maxlength=2 ";
            if (sStartMin != "")
            {
                if (sStartMin.Length == 1)
                    sStartMin = "0" + sStartMin;
                sTmp += "value='" + sStartMin.ToString() + "' ";
            }
            sTmp += "/>";
            return sTmp;
        }

        public override string GetFieldHtml(long nID)
        {
            string sStartDate = "";
            string sStartTime = "";
            string sStartDay = "";
            string sStartMounth = "";
            string sStartYear = "";
            string sStartHour = "";
            string sStartMin = "";
            if (m_sStartValue == "")
                m_sStartValue = m_sDefaultVal;
            if (m_sStartValue != "")
            {
                if (!string.IsNullOrEmpty(m_timeZone))
                {                    
                    DateTime dateTime = ODBCWrapper.Utils.GetDateSafeVal(m_sStartValue, "dd/MM/yyyy H:mm");
                    dateTime = ODBCWrapper.Utils.ConvertFromUtc(dateTime, m_timeZone);
                    m_sStartValue = dateTime.ToString("dd/MM/yyyy HH:mm");                    
                }
                sStartDate = m_sStartValue.Split(' ')[0].ToString();
                sStartTime = m_sStartValue.Split(' ')[1].ToString();
                sStartDay = sStartDate.Split('/')[0].ToString();
                sStartMounth = sStartDate.Split('/')[1].ToString();
                sStartYear = sStartDate.Split('/')[2].ToString();
                sStartHour = sStartTime.Split(':')[0].ToString();
                sStartMin = sStartTime.Split(':')[1].ToString();
            }
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\" id=td_selector" + nID.ToString() + ">";

            sTmp += "<input tabindex=\"" + (nID + 1).ToString() + "\" class='" + m_sInputCss + "' id='Date_" + nID.ToString() + "' name='" + nID.ToString() + "_val' ";
            if (m_bEnabled == true)
                sTmp += "type='text' ";
            else
                sTmp += "type='hidden' ";
            sTmp += "dir=ltr' ";
            sTmp += "size=12 ";
            if (m_sStartValue != "")
                sTmp += "value='" + m_sStartValue.Split(' ')[0].ToString() + "' ";
            sTmp += ">&nbsp;";
            if (m_bEnabled == true)
                sTmp += "<img src='images/icon_calendar.gif' border=0 onclick='Initialization(document.form1.Date_" + nID.ToString() + ");'>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='datetime'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_fieldName' value='" + m_filedPrivateName + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_fieldHeader' value='" + m_sFieldHeader + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_ignore' value='" + ignore + "'/>";
            sTmp += "&nbsp;&nbsp;";
            sTmp += GetSelectsHtml(nID, sStartMin, sStartHour);



            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }

    public class DataRecordTVMChannelCategoryField : DataRecordShortIntField
    {
        protected bool m_bIsCategory;
        protected string m_sPUN;
        protected string m_sPP;
        protected string m_sFrameName;
        public DataRecordTVMChannelCategoryField(bool bIsCategory, string sPUN, string sPass)
            : base(true, 6, 6)
        {
            m_bIsCategory = bIsCategory;
            m_sPUN = sPUN;
            m_sPP = sPass;
            m_sFrameName = "";
        }

        public void SetFrameName(string sFrameName)
        {
            m_sFrameName = sFrameName;
        }
        public override string GetFieldHtml(long nID)
        {
            string sTmp = "<tr>";
            sTmp += "<td class='" + m_sHeaderCss + "' nowrap>";
            if (m_bMust == true)
                sTmp += "<span class=\"red\">*&nbsp;&nbsp;</span>";
            if (m_bEnabled == true)
                sTmp += m_sFieldHeader;
            sTmp += "</td>";
            //sTmp += "<td width=10px nowrap></td>";
            sTmp += "<td class=\"align1\">";
            sTmp += "<table width=100%>";
            sTmp += "<tr><td>";
            sTmp += "<input tabindex=\"2000\" class='" + m_sInputCss + "' name='" + nID.ToString() + "_val' type='";
            //if (m_bEnabled == true)
            //sTmp += "text' ";
            //else
            sTmp += "hidden' ";
            //    sTmp += "disabled ";
            sTmp += "dir='ltr' ";
            sTmp += "size=" + m_nWidth.ToString() + " ";
            sTmp += "maxlength=" + m_nMaxLength.ToString() + " ";
            if (m_sStartValue != "")
                sTmp += "value='" + m_sStartValue.ToString() + "' ";
            else
                if (m_nDefault != -1)
                    sTmp += "value='" + m_nDefault.ToString() + "' ";
            sTmp += "/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_type' value='int'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_must' value='" + m_bMust.ToString() + "'/>";
            sTmp += "<input tabindex=\"2000\" type='hidden' name='" + nID.ToString() + "_field' value='" + m_sFieldName + "'/>";
            sTmp += "</td></tr>";
            sTmp += "<tr><td>";
            string sSV = m_sStartValue;
            if (sSV == "" && m_nDefault != -1)
            {
                sSV = m_nDefault.ToString();
            }

            string itemType = (m_bIsCategory ? "category" : "channel");
            string sCatChnName = sSV;
            object oSelected = null;
            if (m_bIsCategory == true)
                oSelected = ODBCWrapper.Utils.GetTableSingleVal("categories", "CATEGORY_NAME", int.Parse(sSV));
            else
                oSelected = ODBCWrapper.Utils.GetTableSingleVal("channels", "NAME", int.Parse(sSV));
            if (oSelected != null && oSelected != DBNull.Value)
                sCatChnName = oSelected.ToString();
            sTmp += "Choose '" + itemType + "' item ";
            sTmp += "<span id='TVMItemView_" + nID.ToString() + "'>(Current : ";
            sTmp += (string.IsNullOrEmpty(sSV) ? "- no item selected -" : sCatChnName);
            sTmp += ")</span>";

            sTmp += "</td></tr>";
            sTmp += "<tr><td>";
            sTmp += "<IFRAME ";
            if (m_sFrameName != "")
                sTmp += "ID=\"" + m_sFrameName + "\" NAME=\"" + m_sFrameName + "\"";
            sTmp += " SRC=\"admin_category_chooser.aspx?";
            if (m_bIsCategory == true)
                sTmp += "start_category_id=" + sSV;
            else
                sTmp += "start_channel_id=" + sSV;
            sTmp += "&pun=" + m_sPUN + "&ppass=" + m_sPP + "&container_id=" + nID.ToString();
            sTmp += "\" WIDTH=\"600\" HEIGHT=\"300\" FRAMEBORDER=\"0\"></IFRAME>";
            sTmp += "</td></tr>";

            sTmp += "</table>";
            sTmp += "</td>";
            //sTmp += "<td width=100% nowrap></td>";
            sTmp += "</tr>";
            return sTmp;
        }
    }
    
    /// <summary>
    /// Summary description for DBRecordWebEditor
    /// </summary>
    public class DBRecordWebEditor
    {
        protected System.Collections.Hashtable m_Records;
        protected string m_sUpperCss;
        protected string m_sDBTableName;
        protected string m_sSuccessPage;
        protected string m_sUniqueField;
        protected string m_sFieldIndexName;
        protected object m_oFieldIndexValue;
        protected string m_sBackURL;
        protected string m_sBackNextParameterName;
        protected string m_sConnectionKey;

        public DBRecordWebEditor(string sDBTableName, string sUpperCss, string sSuccessPage, string sUniqueField, string sFieldIndexName, object oFieldIndexValue, string sBackURL, string sBackNextParameterName)
        {
            m_sBackNextParameterName = sBackNextParameterName;
            m_sUpperCss = sUpperCss;
            m_sDBTableName = sDBTableName;
            m_sSuccessPage = sSuccessPage;
            m_sUniqueField = sUniqueField;
            m_sFieldIndexName = sFieldIndexName;
            m_oFieldIndexValue = oFieldIndexValue;
            m_sBackURL = sBackURL;
            m_Records = new System.Collections.Hashtable();
            m_sConnectionKey = "";
        }

        public void SetConnectionKey(string sKey)
        {
            m_sConnectionKey = sKey;
        }

        protected string GetBackURLParams()
        {
            string sRet = "?" + m_sBackNextParameterName + "=";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(m_sConnectionKey);
            selectQuery += "select top 1 " + m_sFieldIndexName + " from ";
            selectQuery += m_sDBTableName;
            selectQuery += " where status<>2 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(m_sFieldIndexName, ">", m_oFieldIndexValue);
            selectQuery += "order by ";
            selectQuery += m_sFieldIndexName;
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet += selectQuery.Table("query").DefaultView[0].Row[m_sFieldIndexName].ToString();
                else
                    sRet = "";
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet;
        }

        protected string GetUpperString()
        {
            if (HttpContext.Current.Session["error_msg"] == null)
                return "";
            if (HttpContext.Current.Session["error_msg"].ToString() == "")
                return "";
            string sTmp = "<tr><td><table width=100% cellpadding=0 cellspacing=1 class='" + m_sUpperCss + "' ><tr height=20px ><td class=alert_text nowrap>";
            if (HttpContext.Current.Session["error_msg"] != null && HttpContext.Current.Session["error_msg"].ToString() != "")
                sTmp += HttpContext.Current.Session["error_msg"].ToString();
            sTmp += "</td><td width=100% nowrap></td></tr></table></td></tr>";
            return sTmp;
        }

        public void AddRecord(BaseDataRecordField theColumn)
        {
            if (theColumn.GetConnectionKey() == "")
                AddRecord(theColumn, m_sConnectionKey);
            else
                AddRecord(theColumn, theColumn.GetConnectionKey());
        }

        public void AddRecord(BaseDataRecordField theColumn, string sConnectionKey)
        {
            m_Records.Add(m_Records.Count.ToString(), theColumn);
            theColumn.SetConnectionKey(sConnectionKey);
        }

        protected string GetCurrentPageURL()
        {
            string sURL = HttpContext.Current.Request.FilePath.ToString();
            Int32 nStart = sURL.LastIndexOf('/');
            Int32 nEnd = sURL.Length;
            string sPage = sURL.Substring(nStart + 1, nEnd - nStart - 1);
            return sPage;
        }

        public string GetTableHTMLHeader()
        {
            string sTable = "<table style=\"height: 0px;\"><tr><td width=\"100%\" nowrap><table style=\"height: 0px;\">";
            sTable += "<tr height=0px><td width=100% nowrap height=0px>";
            sTable += "<input tabindex=\"2000\" type='hidden' name='table_name' value='" + m_sDBTableName + "'/>";
            sTable += "<input tabindex=\"2000\" type='hidden' name='unique_field' value='" + m_sUniqueField + "'/>";
            sTable += "<input tabindex=\"2000\" type='hidden' name='failure_back_page' value='" + GetCurrentPageURL() + "'/>";
            sTable += "<input tabindex=\"2000\" type='hidden' name='success_back_page' value='" + m_sSuccessPage + "'/>";
            if (m_oFieldIndexValue != null)
                sTable += "<input tabindex=\"2000\" type='hidden' name='id' value='" + m_oFieldIndexValue.ToString() + "'/>";
            sTable += "</td></tr></table></td></tr>";
            return sTable;
        }

        /*public string GetTableHTMLFooter(string sThePage)
        {
            return GetTableHTMLFooter(sThePage, true);
        }*/

        public string GetTableHTMLFooter(string sThePage, bool bRemoveConfirm, Int32 nRows)
        {
            if (nRows < 10)
                return "";
            //string sTable = "</table></td></tr>";
            if (sThePage == "")
                sThePage = "adm_generic_insert.aspx";
            string sTable = "<tr>";
            sTable += "<td  width=\"100%\" nowrap>";
            sTable += "<table width=100% style=\"padding-right: 10px;\">";
            sTable += "<tr>";
            if (bRemoveConfirm == false)
            {
                sTable += "<td id=\"confirm_btn\" onclick='submitASPFormWithCheck(\"" + sThePage + "\");'><a tabindex=\"2000\" href=\"#confirm_btn\" class=\"btn\"></a></td>";
            }
            if (m_sBackURL != "")
            {
                sTable += "<td id=\"cancel_btn\" onclick='window.document.location.href=\"" + m_sBackURL + "\";'><a tabindex=\"2000\" href=\"#cancel_btn\" class=\"btn\"></a></td>";
            }
            sTable += "<td nowrap=\"nowrap\" width=\"100%\"></td>";
            sTable += "</tr></table></td></tr>";
            sTable += "</table>";
            return sTable;
        }

        public string GetTableHTMLUpper(string sThePage, bool bRemoveConfirm)
        {
            if (sThePage == "")
                sThePage = "adm_generic_insert.aspx";
            //string sTable = "<tr><td>&nbsp;</td></tr><tr>";
            string sTable = "<tr>";
            sTable += "<td  width=\"100%\" nowrap>";
            sTable += "<table width=100% style=\"padding-right: 10px;\">";
            sTable += "<tr>";
            if (bRemoveConfirm == false)
            {
                sTable += "<td id=\"confirm_btn\" onclick='submitASPFormWithCheck(\"" + sThePage + "\");'><a tabindex=\"2000\" href=\"#confirm_btn\" class=\"btn\"></a></td>";
            }
            if (m_sBackURL != "")
            {
                sTable += "<td id=\"cancel_btn\" onclick='window.document.location.href=\"" + m_sBackURL + "\";'><a tabindex=\"2000\" href=\"#cancel_btn\" class=\"btn\"></a></td>";
            }
            sTable += "<td nowrap=\"nowrap\" width=\"100%\"></td>";
            sTable += "</tr></table></td></tr><tr><td>&nbsp;</td></tr>";
            return sTable;
        }

        public string GetTableHTMLInner(Int32 nStart, Int32 nEnd, ref Int32 nRows)
        {
            if (nStart < 0)
                nStart = 0;
            if (nEnd <= 0)
                nEnd = m_Records.Count;
            StringBuilder sTable = new StringBuilder();
            sTable.Append("<tr><td width=100% nowrap><table width=100%>");
            Int32 nC = nStart;
            for (int i = nStart; i < nEnd; i++)
            {
                nRows++;
                Int32 nToAdd = 1;
                if (m_oFieldIndexValue != null)
                    ((BaseDataRecordField)(m_Records[i.ToString()])).SetValue(m_sDBTableName, m_sFieldIndexName, m_oFieldIndexValue);
                sTable.Append(((BaseDataRecordField)(m_Records[i.ToString()])).GetFieldHtml(nC, ref nToAdd));
                sTable.Append("<tr><td width=100% nowrap class=horizon_line_space colspan=3></td></tr>");
                nC += nToAdd;
            }
            sTable.Append("</table></td></tr>");
            return sTable.ToString();
        }

        public void SaveTable(string sTable)
        {
            HttpContext.Current.Session["last_page_html"] = sTable;
        }

        public string GetTableHTML(string thePage)
        {
            return GetTableHTML(thePage, false);
        }

        public string GetTableHTML(string thePage, bool bRemoveConfirm)
        {
            string sTable = GetTableHTMLHeader();
            sTable += GetTableHTMLUpper(thePage, bRemoveConfirm);
            Int32 nRows = 0;
            sTable += GetTableHTMLInner(0, 0, ref nRows);
            sTable += GetTableHTMLFooter(thePage, bRemoveConfirm, nRows);
            SaveTable(sTable);
            return sTable;
        }


        public string GetTableHTMLCB(string thePage, bool bRemoveConfirm, object epg)
        {
            string sTable = GetTableHTMLHeader();
            sTable += GetTableHTMLUpper(thePage, bRemoveConfirm);
            Int32 nRows = 0;
            sTable += GetTableHTMLInnerCB(0, 0, epg, ref nRows);
            sTable += GetTableHTMLFooter(thePage, bRemoveConfirm, nRows);
            SaveTable(sTable);
            return sTable;
        }


        public string GetTableHTMLInnerCB(Int32 nStart, Int32 nEnd, object epg, ref Int32 nRows)
        {
            if (nStart < 0)
                nStart = 0;
            if (nEnd <= 0)
                nEnd = m_Records.Count;
            StringBuilder sTable = new StringBuilder();
            sTable.Append("<tr><td width=100% nowrap><table width=100%>");
            Int32 nC = nStart;
            for (int i = nStart; i < nEnd; i++)
            {
                nRows++;
                Int32 nToAdd = 1;
                if (m_Records[i.ToString()].GetType() == typeof(DataRecordOnePicBrowserField))
                {
                    sTable.Append(((DataRecordOnePicBrowserField)(m_Records[i.ToString()])).GetFieldHtmlCB(nC));
                }
                else
                {
                    sTable.Append(((BaseDataRecordField)(m_Records[i.ToString()])).GetFieldHtml(nC, ref nToAdd));
                }
                sTable.Append("<tr><td width=100% nowrap class=horizon_line_space colspan=3></td></tr>");
                nC += nToAdd;
            }
            sTable.Append("</table></td></tr>");
            return sTable.ToString();
        }
    }
}