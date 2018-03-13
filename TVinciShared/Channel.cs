using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using System.Collections;
using System.Data;
using System.Configuration;
using KLogMonitor;
using System.Reflection;
using ConfigurationManager;

namespace TVinciShared
{

    public enum OrderDir
    {
        ASC = 1,
        DESC = 2
    }

    public class Channel
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected Int32 m_nChannelID;
        protected Int32 m_nChannelType;
        protected System.Data.DataTable m_dtChannelTags;
        protected System.Data.DataTable m_dtStrDbMedias;
        protected Int32 m_nOrderBy;
        protected Int32 m_nOrderDir;
        protected Int32 m_nLangID;
        protected bool m_bIsLangMain;
        protected string m_sStrValuesQuery;
        protected string m_sDoubleValuesQuery;
        protected string m_sBoolValuesQuery;
        protected Int32 m_nMediaType;
        protected Int32 m_nGroupID;
        protected bool m_bWithCache;
        protected Int32 m_nIsAnd;
        protected Int32 m_nStatus;
        protected Int32 m_nIsActive;
        protected Int32 m_nWatcherID;
        protected Int32 m_nCountryID;
        protected Int32 m_nDeviceID;
        protected string m_sOrderBy;
        protected string m_sOrderByAdd;
        protected DateTime m_dLinearStartDate;
        protected string s_ConnectionKey = string.Empty;

        protected int[] m_nDevicesRules;

        public DateTime GetLinearDateTime()
        {
            return m_dLinearStartDate;
        }
        public Channel(Int32 nChannelID, Int32 nLangID, Int32 nGroupID, int[] devicesRules)
        {
            m_nChannelID = nChannelID;
            if (nLangID == 0)
            {
                //get main lang
                nLangID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "LANGUAGE_ID", nGroupID).ToString());
            }
            m_nLangID = nLangID;
            m_nGroupID = nGroupID;

            m_nDevicesRules = devicesRules;
        }

        public Channel(Int32 nChannelID, bool bWithCache, Int32 nLangID, bool bIsLangMain, Int32 nCountryID, Int32 nDeviceID)
        {
            m_bWithCache = bWithCache;
            m_nChannelID = nChannelID;
            m_dtChannelTags = null;
            m_dtStrDbMedias = null;
            m_nGroupID = 0;
            m_sStrValuesQuery = "";
            m_sDoubleValuesQuery = "";
            m_sBoolValuesQuery = "";
            m_sOrderBy = "";
            m_sOrderByAdd = "";
            m_nLangID = 0;
            GetChannelsData();
            StartChannelTags();
            m_bIsLangMain = bIsLangMain;
            m_nCountryID = nCountryID;
            m_nDeviceID = nDeviceID;
            s_ConnectionKey = string.Empty;
        }
        public Channel(Int32 nChannelID, bool bWithCache, Int32 nLangID, bool bIsLangMain, Int32 nCountryID, Int32 nDeviceID, string sConnectionKey)
        {
            s_ConnectionKey = sConnectionKey;
            m_bWithCache = bWithCache;
            m_nChannelID = nChannelID;
            m_dtChannelTags = null;
            m_dtStrDbMedias = null;
            m_nGroupID = 0;
            m_sStrValuesQuery = "";
            m_sDoubleValuesQuery = "";
            m_sBoolValuesQuery = "";
            m_sOrderBy = "";
            m_sOrderByAdd = "";
            m_nLangID = 0;
            GetChannelsData();
            StartChannelTags();
            m_bIsLangMain = bIsLangMain;
            m_nCountryID = nCountryID;
            m_nDeviceID = nDeviceID;

        }
        public Channel(Int32 nChannelID, bool bWithCache, Int32 nLangID, bool bIsLangMain, Int32 nCountryID, Int32 nDeviceID, Int32 nGroupID, string sConnectionKey)
        {
            m_nGroupID = nGroupID;
            s_ConnectionKey = sConnectionKey;
            m_bWithCache = bWithCache;
            m_nChannelID = nChannelID;
            m_dtChannelTags = null;
            m_dtStrDbMedias = null;
            m_sStrValuesQuery = "";
            m_sDoubleValuesQuery = "";
            m_sBoolValuesQuery = "";
            m_sOrderBy = "";
            m_sOrderByAdd = "";
            m_nLangID = 0;
            GetChannelsData();
            StartChannelTags();
            m_bIsLangMain = bIsLangMain;
            m_nCountryID = nCountryID;
            m_nDeviceID = nDeviceID;

        }
        public Channel(Int32 nChannelID, bool bWithCache, ref System.Xml.XmlNode theOrderBy, Int32 nGroupID, Int32 nLangID, bool bIsLangMain, Int32 nCountryID, Int32 nDeviceID)
        {
            m_bWithCache = bWithCache;
            m_nChannelID = nChannelID;
            m_dtChannelTags = null;
            m_dtStrDbMedias = null;
            m_nGroupID = nGroupID;
            m_sStrValuesQuery = "";
            m_sDoubleValuesQuery = "";
            m_sBoolValuesQuery = "";
            m_sOrderByAdd = "";
            m_sOrderBy = GetOrderByStr(ref theOrderBy, ref m_sOrderByAdd);
            GetChannelsData();
            StartChannelTags();
            m_nLangID = nLangID;
            m_bIsLangMain = bIsLangMain;
            m_nCountryID = nCountryID;
            m_nDeviceID = nDeviceID;
        }

        public Channel(Int32 nChannelID, bool bWithCache, string sOrderBy, string sOrderByAdd, Int32 nGroupID, Int32 nLangID, bool bIsLangMain, Int32 nCountryID, Int32 nDeviceID)
        {
            m_nCountryID = nCountryID;
            m_bWithCache = bWithCache;
            m_nChannelID = nChannelID;
            m_dtChannelTags = null;
            m_dtStrDbMedias = null;
            m_nGroupID = nGroupID;
            m_sStrValuesQuery = "";
            m_sDoubleValuesQuery = "";
            m_sBoolValuesQuery = "";
            m_sOrderByAdd = sOrderByAdd;
            m_sOrderBy = sOrderBy;
            GetChannelsData();
            StartChannelTags();
            m_nLangID = nLangID;
            m_bIsLangMain = bIsLangMain;
            m_nDeviceID = nDeviceID;
        }

        public OrderDir GetChannelOrderDir()
        {
            OrderDir retVal = OrderDir.DESC;
            if (m_nOrderDir > 0)
            {
                retVal = (OrderDir)m_nOrderDir;
            }
            return retVal;
        }

        public string GetOrderByStr()
        {
            string retVal = string.Empty;
            if (string.IsNullOrEmpty(m_sOrderByAdd))
            {
                if (m_nOrderBy == -10)
                    retVal = " m.start_date ";
                else if (m_nOrderBy == -11)
                    retVal = " m.name ";
                else if (m_nOrderBy == -12)
                    retVal = " m.create_date ";
                else if (m_nOrderBy == -9)
                    retVal = " m.like_counter ";
                else if (m_nOrderBy == -8)
                    retVal = ",((m.VOTES_SUM/( case when m.VOTES_COUNT=0 then 1 else m.VOTES_COUNT end)) * ( case when m.VOTES_COUNT<5 then m.VOTES_COUNT else 5 end)) ,m.VOTES_COUNT ";
                else if (m_nOrderBy == -7)
                    retVal = " m.VIEWS ";
                //else if (m_nOrderBy == -6)
                //selectQuery += ",newid() ";
                else if (m_nOrderBy == 1)
                    retVal = " m.META1_STR ";
                else if (m_nOrderBy == 2)
                    retVal = " m.META2_STR ";
                else if (m_nOrderBy == 3)
                    retVal = " m.META3_STR ";
                else if (m_nOrderBy == 4)
                    retVal = " m.META4_STR ";
                else if (m_nOrderBy == 5)
                    retVal = " m.META5_STR ";
                else if (m_nOrderBy == 6)
                    retVal = " m.META6_STR ";
                else if (m_nOrderBy == 7)
                    retVal = " m.META7_STR ";
                else if (m_nOrderBy == 8)
                    retVal = " m.META8_STR ";
                else if (m_nOrderBy == 9)
                    retVal = " m.META9_STR ";
                else if (m_nOrderBy == 10)
                    retVal = " m.META10_STR ";
                else if (m_nOrderBy == 11)
                    retVal = " m.META11_STR ";
                else if (m_nOrderBy == 12)
                    retVal = " m.META12_STR ";
                else if (m_nOrderBy == 13)
                    retVal = " m.META13_STR ";
                else if (m_nOrderBy == 14)
                    retVal = " m.META14_STR ";
                else if (m_nOrderBy == 15)
                    retVal = " m.META15_STR ";
                else if (m_nOrderBy == 16)
                    retVal = " m.META16_STR ";
                else if (m_nOrderBy == 17)
                    retVal = " m.META17_STR ";
                else if (m_nOrderBy == 18)
                    retVal = " m.META18_STR ";
                else if (m_nOrderBy == 19)
                    retVal = " m.META19_STR ";
                else if (m_nOrderBy == 20)
                    retVal = " m.META20_STR ";

                else if (m_nOrderBy == 21)
                    retVal = " m.META1_DOUBLE ";
                else if (m_nOrderBy == 22)
                    retVal = " m.META2_DOUBLE ";
                else if (m_nOrderBy == 23)
                    retVal = " m.META3_DOUBLE ";
                else if (m_nOrderBy == 24)
                    retVal = " m.META4_DOUBLE ";
                else if (m_nOrderBy == 25)
                    retVal = " m.META5_DOUBLE ";

                else if (m_nOrderBy == 26)
                    retVal = " m.META6_DOUBLE ";
                else if (m_nOrderBy == 27)
                    retVal = " m.META7_DOUBLE ";
                else if (m_nOrderBy == 28)
                    retVal = " m.META8_DOUBLE ";
                else if (m_nOrderBy == 29)
                    retVal = " m.META9_DOUBLE ";
                else if (m_nOrderBy == 30)
                    retVal = " m.META10_DOUBLE ";
            }
            return retVal;
        }

        protected string GetOrderByStr(ref XmlNode theNode, ref string sAdditionalSelect)
        {
            string sOrderBy = "";
            try
            {
                if (theNode == null)
                    return "";
                SortedList theOrderBy = new SortedList();
                XmlNode theOrderRandomVal = theNode.SelectSingleNode("//order_values/random/@value");
                string sOrderRandomVal = "";
                if (theOrderRandomVal != null)
                    sOrderRandomVal = theOrderRandomVal.Value.ToLower().Trim();
                string sMetaFieldQuery = "";
                if (sOrderRandomVal == "")
                {
                    XmlNode theOrderNameDir = theNode.SelectSingleNode("//order_values/name/@order_dir");
                    XmlNode theOrderNameNum = theNode.SelectSingleNode("//order_values/name/@order_num");
                    Int32 nNameOrderNum = 0;
                    string sOrderNameDir = "";
                    if (theOrderNameDir != null)
                        sOrderNameDir = theOrderNameDir.Value;
                    if (theOrderNameNum != null)
                        nNameOrderNum = int.Parse(theOrderNameNum.Value);
                    if (theOrderNameDir != null)
                        theOrderBy[nNameOrderNum] = "m.name " + sOrderNameDir;

                    XmlNode theOrderDescDir = theNode.SelectSingleNode("//order_values/description/@order_dir");
                    XmlNode theOrderDescNum = theNode.SelectSingleNode("//order_values/description/@order_num");
                    Int32 nDescOrderNum = 0;
                    string sOrderDescDir = "";
                    if (theOrderDescDir != null)
                        sOrderDescDir = theOrderDescDir.Value;
                    if (theOrderDescNum != null)
                        nDescOrderNum = int.Parse(theOrderDescNum.Value);
                    if (theOrderDescDir != null)
                        theOrderBy[nDescOrderNum] = "m.description " + sOrderDescDir;

                    XmlNode theOrderDateDir = theNode.SelectSingleNode("//order_values/date/@order_dir");
                    XmlNode theOrderDateNum = theNode.SelectSingleNode("//order_values/date/@order_num");
                    Int32 nDateOrderNum = 0;
                    string sOrderDateDir = "";
                    if (theOrderDateDir != null)
                        sOrderDateDir = theOrderDateDir.Value;
                    if (theOrderDateNum != null)
                        nDateOrderNum = int.Parse(theOrderDateNum.Value);
                    if (theOrderDateDir != null)
                        theOrderBy[nDateOrderNum] = "m.start_date " + sOrderDateDir;

                    XmlNode theOrderViewsDir = theNode.SelectSingleNode("//order_values/views/@order_dir");
                    XmlNode theOrderViewsNum = theNode.SelectSingleNode("//order_values/views/@order_num");
                    Int32 nViewsOrderNum = 0;
                    string sOrderViewsDir = "";
                    if (theOrderViewsDir != null)
                        sOrderViewsDir = theOrderViewsDir.Value;
                    if (theOrderViewsNum != null)
                        nViewsOrderNum = int.Parse(theOrderViewsNum.Value);
                    if (theOrderViewsDir != null)
                        theOrderBy[nViewsOrderNum] = "m.views " + sOrderViewsDir;

                    XmlNode theOrderRateDir = theNode.SelectSingleNode("//order_values/rate/@order_dir");
                    XmlNode theOrderRateNum = theNode.SelectSingleNode("//order_values/rate/@order_num");
                    Int32 nRateOrderNum = 0;
                    string sOrderRateDir = "";
                    if (theOrderRateDir != null)
                        sOrderRateDir = theOrderRateDir.Value;
                    if (theOrderRateNum != null)
                        nRateOrderNum = int.Parse(theOrderRateNum.Value);
                    if (theOrderRateDir != null)
                        theOrderBy[nRateOrderNum] = "m.rate " + sOrderRateDir;

                    XmlNodeList theOrderMetaList = theNode.SelectNodes("//order_values/meta");
                    IEnumerator iterMeta = theOrderMetaList.GetEnumerator();
                    string sMetaField = "";
                    while (iterMeta.MoveNext())
                    {

                        XmlNode theMeta = (XmlNode)(iterMeta.Current);

                        XmlNode theOrderMetaDir = theMeta.SelectSingleNode("@order_dir");
                        XmlNode theOrderMetaNum = theMeta.SelectSingleNode("@order_num");
                        XmlNode theOrderMetaName = theMeta.SelectSingleNode("@name");
                        Int32 nMetaOrderNum = 0;
                        string sOrderMetaDir = "";
                        string sOrderMetaName = "";
                        if (theOrderMetaDir != null)
                            sOrderMetaDir = theOrderMetaDir.Value;
                        if (theOrderMetaNum != null)
                            nMetaOrderNum = int.Parse(theOrderMetaNum.Value);
                        if (theOrderMetaName != null)
                            sOrderMetaName = theOrderMetaName.Value;

                        if (sOrderMetaName != "")
                        {

                            Int32 nStrID = PageUtils.GetStringMetaIDByMetaName(m_nGroupID, sOrderMetaName);
                            if (nStrID != 0)
                                sMetaField = "META" + nStrID.ToString() + "_STR";
                            else
                            {
                                Int32 nDoubleID = PageUtils.GetDoubleMetaIDByMetaName(m_nGroupID, sOrderMetaName);
                                if (nDoubleID != 0)
                                    sMetaField = "META" + nDoubleID.ToString() + "_DOUBLE";
                                else
                                {
                                    Int32 nBoolID = PageUtils.GetBoolMetaIDByMetaName(m_nGroupID, sOrderMetaName);
                                    if (nBoolID != 0)
                                        sMetaField = "META" + nBoolID.ToString() + "_BOOL";
                                }

                            }
                            theOrderBy[nMetaOrderNum] = "m." + sMetaField + " " + sOrderMetaDir;
                            if (sMetaFieldQuery != "")
                                sMetaFieldQuery += ",";
                            sMetaFieldQuery += "m.";
                            sMetaFieldQuery += sMetaField;
                        }
                    }
                    IDictionaryEnumerator iter = theOrderBy.GetEnumerator();
                    bool bOrderFirst = true;
                    while (iter.MoveNext())
                    {
                        if (bOrderFirst == true)
                        {
                            sOrderBy += " order by ";
                            bOrderFirst = false;
                        }
                        else
                        {
                            sOrderBy += ",";
                            sAdditionalSelect += ",";
                        }
                        sOrderBy += iter.Value.ToString();
                        sAdditionalSelect += iter.Value.ToString();
                    }
                    sAdditionalSelect = sAdditionalSelect.Replace(" asc", "").Replace(" desc", "");
                }
            }
            catch
            { }
            return sOrderBy;
        }

        protected void GetChannelsData()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(s_ConnectionKey);
            selectQuery += "select * from channels WITH (nolock) where ";
            //selectQuery.SetCachedSec(7200);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nChannelID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    try
                    {
                        m_nStatus = int.Parse(selectQuery.Table("query").DefaultView[0].Row["status"].ToString());
                        m_nIsActive = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IS_ACTIVE"].ToString());
                        m_nChannelType = int.Parse(selectQuery.Table("query").DefaultView[0].Row["channel_type"].ToString());
                        m_nOrderBy = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ORDER_BY_TYPE"].ToString());
                        m_nOrderDir = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ORDER_BY_DIR"].ToString());
                        m_nIsAnd = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IS_AND"].ToString());
                        m_nMediaType = 0;
                        if (selectQuery.Table("query").DefaultView[0].Row["MEDIA_TYPE_ID"] != null &&
                            selectQuery.Table("query").DefaultView[0].Row["MEDIA_TYPE_ID"] != DBNull.Value)
                            m_nMediaType = int.Parse(selectQuery.Table("query").DefaultView[0].Row["MEDIA_TYPE_ID"].ToString());
                        m_nWatcherID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["WATCHER_ID"].ToString());
                        m_dLinearStartDate = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["LINEAR_START_TIME"]);

                        AddToQuery(ref selectQuery, "META1_STR", ref m_sStrValuesQuery, true, 1);
                        AddToQuery(ref selectQuery, "META2_STR", ref m_sStrValuesQuery, true, 2);
                        AddToQuery(ref selectQuery, "META3_STR", ref m_sStrValuesQuery, true, 3);
                        AddToQuery(ref selectQuery, "META4_STR", ref m_sStrValuesQuery, true, 4);
                        AddToQuery(ref selectQuery, "META5_STR", ref m_sStrValuesQuery, true, 5);
                        AddToQuery(ref selectQuery, "META6_STR", ref m_sStrValuesQuery, true, 6);
                        AddToQuery(ref selectQuery, "META7_STR", ref m_sStrValuesQuery, true, 7);
                        AddToQuery(ref selectQuery, "META8_STR", ref m_sStrValuesQuery, true, 8);
                        AddToQuery(ref selectQuery, "META9_STR", ref m_sStrValuesQuery, true, 9);
                        AddToQuery(ref selectQuery, "META10_STR", ref m_sStrValuesQuery, true, 10);

                        AddToQuery(ref selectQuery, "META11_STR", ref m_sStrValuesQuery, true, 11);
                        AddToQuery(ref selectQuery, "META12_STR", ref m_sStrValuesQuery, true, 12);
                        AddToQuery(ref selectQuery, "META13_STR", ref m_sStrValuesQuery, true, 13);
                        AddToQuery(ref selectQuery, "META14_STR", ref m_sStrValuesQuery, true, 14);
                        AddToQuery(ref selectQuery, "META15_STR", ref m_sStrValuesQuery, true, 15);
                        AddToQuery(ref selectQuery, "META16_STR", ref m_sStrValuesQuery, true, 16);
                        AddToQuery(ref selectQuery, "META17_STR", ref m_sStrValuesQuery, true, 17);
                        AddToQuery(ref selectQuery, "META18_STR", ref m_sStrValuesQuery, true, 18);
                        AddToQuery(ref selectQuery, "META19_STR", ref m_sStrValuesQuery, true, 19);
                        AddToQuery(ref selectQuery, "META20_STR", ref m_sStrValuesQuery, true, 20);

                        AddToQuery(ref selectQuery, "META1_DOUBLE", ref m_sDoubleValuesQuery, false, 1);
                        AddToQuery(ref selectQuery, "META2_DOUBLE", ref m_sDoubleValuesQuery, false, 2);
                        AddToQuery(ref selectQuery, "META3_DOUBLE", ref m_sDoubleValuesQuery, false, 3);
                        AddToQuery(ref selectQuery, "META4_DOUBLE", ref m_sDoubleValuesQuery, false, 4);
                        AddToQuery(ref selectQuery, "META5_DOUBLE", ref m_sDoubleValuesQuery, false, 5);

                        AddToQuery(ref selectQuery, "META6_DOUBLE", ref m_sDoubleValuesQuery, false, 6);
                        AddToQuery(ref selectQuery, "META7_DOUBLE", ref m_sDoubleValuesQuery, false, 7);
                        AddToQuery(ref selectQuery, "META8_DOUBLE", ref m_sDoubleValuesQuery, false, 8);
                        AddToQuery(ref selectQuery, "META9_DOUBLE", ref m_sDoubleValuesQuery, false, 9);
                        AddToQuery(ref selectQuery, "META10_DOUBLE", ref m_sDoubleValuesQuery, false, 10);

                        AddToQuery(ref selectQuery, "META1_BOOL", ref m_sBoolValuesQuery, false, 0);
                        AddToQuery(ref selectQuery, "META2_BOOL", ref m_sBoolValuesQuery, false, 0);
                        AddToQuery(ref selectQuery, "META3_BOOL", ref m_sBoolValuesQuery, false, 0);
                        AddToQuery(ref selectQuery, "META4_BOOL", ref m_sBoolValuesQuery, false, 0);
                        AddToQuery(ref selectQuery, "META5_BOOL", ref m_sBoolValuesQuery, false, 0);

                        AddToQuery(ref selectQuery, "META6_BOOL", ref m_sBoolValuesQuery, false, 0);
                        AddToQuery(ref selectQuery, "META7_BOOL", ref m_sBoolValuesQuery, false, 0);
                        AddToQuery(ref selectQuery, "META8_BOOL", ref m_sBoolValuesQuery, false, 0);
                        AddToQuery(ref selectQuery, "META9_BOOL", ref m_sBoolValuesQuery, false, 0);
                        AddToQuery(ref selectQuery, "META10_BOOL", ref m_sBoolValuesQuery, false, 0);
                    }
                    catch (Exception ex)
                    {
                        log.Error("exception - "+ m_nChannelID.ToString() + " : " + ex.Message + " | " + ex.StackTrace, ex);
                    }

                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        public void SetGroupID(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
            GetChannelsData();
            StartChannelTags();
        }
        public Int32 GetType()
        {
            return m_nChannelType;
        }
        public Int32 GetChannelWatcherID()
        {
            return m_nWatcherID;
        }

        protected void AddToQuery(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sFieldName, ref string sQuery, bool bString, Int32 nMediaTypeNum)
        {

            if (selectQuery.Table("query").DefaultView[0].Row[sFieldName] == DBNull.Value)
                return;
            if (selectQuery.Table("query").DefaultView[0].Row[sFieldName] == null)
                return;
            string sVal = selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString();
            if (sVal == "")
                return;
            if (bString == false && int.Parse(selectQuery.Table("query").DefaultView[0].Row["USE_" + sFieldName].ToString()) == 0)
                return;

            sFieldName = sFieldName.Trim().ToLower();
            Int32 nMediaTextTypeID = 0;
            if (sFieldName == "name")
                nMediaTextTypeID = 1;
            else if (sFieldName == "description")
                nMediaTextTypeID = 2;
            else if (sFieldName == "co_guid")
                nMediaTextTypeID = 3;
            else if (sFieldName == "EPG_IDENTIFIER")
                nMediaTextTypeID = 4;
            else if (sFieldName.StartsWith("meta") && sFieldName.EndsWith("_str"))
                nMediaTextTypeID = 5;
            else if (sFieldName.StartsWith("meta") && sFieldName.EndsWith("_double"))
                nMediaTextTypeID = 6;

            if (nMediaTextTypeID == 0)
            {
                if (sQuery != "")
                    sQuery += " and ";
                sQuery += "m." + sFieldName + "=";
                if (bString == true)
                    sQuery += "'";
                sQuery += sVal.Replace("''", "'").Replace("'", "''");
                if (bString == true)
                    sQuery += "' ";
            }
            else
            {
                Int32 nGroupID = m_nGroupID;
                if (nGroupID == 0)
                    nGroupID = LoginManager.GetLoginGroupID();
                if (nGroupID != 0)
                {
                    if (sQuery != "")
                        sQuery += " and ";
                    //string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
                    string sGroups = PageUtils.GetFullChildGroupsStr(nGroupID, s_ConnectionKey);
                    sQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=" + nMediaTextTypeID.ToString() + " and MEDIA_TEXT_TYPE_NUM=" + nMediaTypeNum.ToString() + " and value='" + sVal.Replace("''", "'").Replace("'", "''") + "' ";
                    if (sGroups != "")
                        sQuery += "and m.group_id " + sGroups;
                    sQuery += ") ";
                }

            }
        }

        protected void StartChannelTags()
        {
            Int32 nGroupID = m_nGroupID;
            if (nGroupID == 0)
                nGroupID = LoginManager.GetLoginGroupID();

            if (nGroupID != 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(s_ConnectionKey);
                selectQuery += "select t.id as tag_id from tags t,media_tags_types mtt,(select ct.tag_id,t.value,mtt.NAME from channel_tags ct (nolock),tags t,media_tags_types mtt where mtt.status=1 and mtt.id=t.TAG_TYPE_ID and t.id=ct.tag_id and ct.status=1 and t.status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ct.group_id", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ct.channel_id", "=", m_nChannelID);
                selectQuery += ")q where t.value=q.value and t.status=1 and t.group_id " + PageUtils.GetAllGroupTreeStr(nGroupID, s_ConnectionKey) + " and mtt.id=t.tag_type_id and mtt.name=q.name";
                if (selectQuery.Execute("query", true) != null)
                {
                    m_dtChannelTags = selectQuery.Table("query").Copy();
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            else
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(s_ConnectionKey);
                selectQuery += "select * from channel_tags ct,tags t where t.id=ct.tag_id and t.status=1 and ct.status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ct.channel_id", "=", m_nChannelID);
                if (selectQuery.Execute("query", true) != null)
                {
                    m_dtChannelTags = selectQuery.Table("query").Copy();
                }
                selectQuery.Finish();
                selectQuery = null;
            }
        }

        protected string GetTagsIDs(ref bool bAnd, Int32 nGroupID)
        {
            StringBuilder sRet = new StringBuilder();
            Int32 nCount = m_dtChannelTags.DefaultView.Count;
            if (nCount == 0)
                return "";
            if (m_nIsAnd == 0)
            {
                bAnd = false;
                sRet.Append("mt.tag_id in (");
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet.Append(",");
                    sRet.Append(m_dtChannelTags.DefaultView[i].Row["tag_id"]);
                }
                sRet.Append(") ");
            }
            else
            {
                bAnd = true;
                sRet.Append("( ");
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet.Append(" and ");
                    string sTags = GetMediaIDsByTags(int.Parse(m_dtChannelTags.DefaultView[i].Row["tag_id"].ToString()), nGroupID);
                    sRet.Append("(m.id ").Append(sTags).Append(")");
                }
                sRet.Append(") ");
            }
            return sRet.ToString();
        }

        protected string GetMediaIDsByTags(Int32 nTagID, Int32 nGroupID)
        {
            if (nTagID == 0)
                return "in (0)";
            StringBuilder sRet = new StringBuilder();
            string sGroups = PageUtils.GetFullChildGroupsStr(nGroupID, s_ConnectionKey);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(s_ConnectionKey);
            string sVal = ODBCWrapper.Utils.GetTableSingleVal("tags", "value", nTagID, s_ConnectionKey).ToString();
            Int32 nTagTypeID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tags", "tag_type_id", nTagID, 7200, s_ConnectionKey).ToString());
            string sTagTypeName = ODBCWrapper.Utils.GetTableSingleVal("media_tags_types", "NAME", nTagTypeID, 7200, s_ConnectionKey).ToString();
            selectQuery += "select distinct m.id from media m (nolock),media_tags_types(nolock) mtt,media_tags mt (nolock),tags t WITH (nolock) where  mt.media_id=m.id and mt.status=1 and mt.tag_id=t.id and t.status=1 and m.status=1 and m.is_active=1 and m.group_id " + sGroups + " and mtt.id=t.tag_type_id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.value", "=", sVal);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mtt.name", "=", sTagTypeName);
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.id", "=", nTagID);

            sRet.Append("in (");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount == 0)
                    sRet.Append("0");
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet.Append(",");
                    sRet.Append(selectQuery.Table("query").DefaultView[i].Row["ID"]);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sRet.Append(")");
            return sRet.ToString();
        }

        public string GetChannelMediaIDs()
        {
            return GetChannelMediaIDs(0, null, true, true);
        }

        public string GetChannelMediaIDs(Int32 nNumOfItems, Int32[] nFileTypes, bool bOnlyActiveMedias, bool bUseStartDate)
        {
            StringBuilder sRet = new StringBuilder();
            System.Data.DataTable d = GetChannelMediaDT(nNumOfItems, nFileTypes, bOnlyActiveMedias, bUseStartDate);
            if (d == null)
                return "";
            Int32 nCount = d.DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                if (i > 0)
                    sRet.Append(",");
                sRet.Append(d.DefaultView[i].Row["id"]);
            }
            return sRet.ToString();
        }

        public string GetChannelMediaIDs_OLD(Int32 nNumOfItems, Int32[] nFileTypes, bool bOnlyActiveMedias, bool bUseStartDate)
        {
            StringBuilder sRet = new StringBuilder();
            System.Data.DataTable d = GetChannelMediaDT_OLD(nNumOfItems, nFileTypes, bOnlyActiveMedias, bUseStartDate);
            if (d == null)
                return "";
            Int32 nCount = d.DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                if (i > 0)
                    sRet.Append(",");
                sRet.Append(d.DefaultView[i].Row["id"]);
            }
            return sRet.ToString();
        }

        public System.Data.DataTable GetChannelMediaDT(Int32[] nFileTypes)
        {
            return GetChannelMediaDT(0, nFileTypes);
        }

        public System.Data.DataTable GetChannelMediaDT()
        {
            return GetChannelMediaDT(0);
        }

        public Int32 GetTotalChannelSize()
        {
            if (CachingManager.CachingManager.Exist("GetTotalChannelSize" + m_nChannelID.ToString() + "_" + m_nLangID.ToString() + "_" + m_nCountryID.ToString() + "_" + m_nDeviceID.ToString()) == true && m_bWithCache == true)
                return int.Parse(CachingManager.CachingManager.GetCachedData("GetTotalChannelSize" + m_nChannelID.ToString() + "_" + m_nLangID.ToString() + "_" + m_nCountryID.ToString() + "_" + m_nDeviceID.ToString()).ToString());
            Int32 nCo = 0;
            Int32 nGroupID = m_nGroupID;
            if (nGroupID == 0)
                nGroupID = LoginManager.GetLoginGroupID();
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            bool bAnd = false;
            string sTagsIDs = GetTagsIDs(ref bAnd, nGroupID);
            //auto
            if (m_nChannelType == 1)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(s_ConnectionKey);
                //selectQuery.SetCachedSec(7200);
                selectQuery += "select count(distinct m.id) as co";
                selectQuery += " from media m (nolock) ";
                if (m_dtChannelTags.DefaultView.Count > 0 && sTagsIDs != "" && bAnd == false)
                    selectQuery += ",media_tags mt (nolock) ";
                //selectQuery += " where m.start_date<getdate() and (m.end_date is null or m.end_date>getdate()) and m.status=1 and m.is_active=1 ";
                selectQuery += " where m.status=1 and m.is_active=1 ";
                selectQuery += " and (m.id not in (select id from media (nolock) where (start_date>getdate() or end_date<getdate()) and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "))";
                selectQuery += " and ";
                selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or end_date<getdate()) and ";
                selectQuery += " (country_id=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", m_nCountryID);
                selectQuery += " ) and (LANGUAGE_ID=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", m_nLangID);
                selectQuery += ") and (DEVICE_ID=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", m_nDeviceID);
                selectQuery += ") and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "))";
                selectQuery += " and (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
                if (sWPGID != "")
                {
                    selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                    selectQuery += sWPGID;
                    selectQuery += ")";
                }
                selectQuery += ") ";
                if (m_dtChannelTags.DefaultView.Count > 0 && sTagsIDs != "")
                {
                    if (bAnd == false)
                    {
                        selectQuery += "and mt.status=1 and ";//mt.tag_id in (";
                        selectQuery += sTagsIDs;
                        //selectQuery += ")";
                        selectQuery += "and mt.MEDIA_ID=m.id ";
                    }
                    else
                    {
                        selectQuery += "and " + sTagsIDs;
                    }
                }
                if (m_sStrValuesQuery.Trim() != "")
                {
                    selectQuery += " and (";
                    selectQuery += m_sStrValuesQuery;
                    selectQuery += ")";
                }
                if (m_sDoubleValuesQuery.Trim() != "")
                {
                    selectQuery += " and (";
                    selectQuery += m_sDoubleValuesQuery;
                    selectQuery += ")";
                }
                if (m_sBoolValuesQuery.Trim() != "")
                {
                    selectQuery += " and (";
                    selectQuery += m_sBoolValuesQuery;
                    selectQuery += ")";
                }
                if (m_nMediaType != 0)
                {
                    selectQuery += " and (";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.MEDIA_TYPE_ID", "=", m_nMediaType);
                    selectQuery += ")";
                }
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nCo = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            //manu
            if (m_nChannelType == 2)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(s_ConnectionKey);
                selectQuery += "select count(*) as co ";
                selectQuery += " from media m (nolock),channels_media cm ";
                selectQuery += " WITH (nolock) where (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
                if (sWPGID != "")
                {
                    selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                    selectQuery += sWPGID;
                    selectQuery += ")";
                }
                //selectQuery += ") and m.start_date<getdate() and (m.end_date is null or m.end_date>getdate()) and cm.media_id=m.id and cm.status=1 and m.status=1 and m.is_active=1 and";
                selectQuery += ") and cm.media_id=m.id and cm.status=1 and m.status=1 and m.is_active=1 ";

                selectQuery += " and (m.id not in (select id from media (nolock) where (start_date>getdate() or end_date<getdate()) and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "))";
                selectQuery += " and ";
                selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or end_date<getdate()) and ";
                selectQuery += " (COUNTRY_ID=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", m_nCountryID);
                selectQuery += ") and  (LANGUAGE_ID=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", m_nLangID);
                selectQuery += ") and (DEVICE_ID=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", m_nDeviceID);
                selectQuery += ") and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "))";
                selectQuery += " and ";
                //selectQuery += ") and cm.media_id=m.id and cm.status=1 and m.status=1 and m.is_active=1 and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cm.channel_id", "=", m_nChannelID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nCo = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            CachingManager.CachingManager.SetCachedData("GetTotalChannelSize" + m_nChannelID.ToString() + "_" + m_nLangID.ToString() + "_" + m_nCountryID.ToString() + "_" + m_nDeviceID.ToString(), nCo, 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            return nCo;
        }

        protected bool IsPersonal()
        {
            object oIsPersonal = ODBCWrapper.Utils.GetTableSingleVal("channels", "is_personal", m_nChannelID, s_ConnectionKey);
            Int32 nIsPersonal = 0;
            if (oIsPersonal == null || oIsPersonal == DBNull.Value)
                nIsPersonal = 0;
            else
                nIsPersonal = (int.Parse(oIsPersonal.ToString()));
            if (nIsPersonal == 1)
                return true;
            return false;
        }

        protected Int32 GetWatcherID()
        {
            Int32 nWatcherID = 0;
            string sTVinciGUID = CookieUtils.GetCookie("tvinci_api");
            if (sTVinciGUID != "" && System.Web.HttpContext.Current.Session["tvinci_watcher_" + sTVinciGUID] != null)
            {
                try
                {
                    nWatcherID = int.Parse(System.Web.HttpContext.Current.Session["tvinci_watcher_" + sTVinciGUID].ToString());
                }
                catch { }
            }
            return nWatcherID;
        }

        protected string GetPersonalViewdMedia()
        {
            Int32 nGroupID = m_nGroupID;
            if (nGroupID == 0)
                nGroupID = LoginManager.GetLoginGroupID();
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(s_ConnectionKey);
            selectQuery += "select distinct media_id from watchers_media_actions WITH (nolock) where action_id=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", GetWatcherID());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            //selectQuery += "order by create_date desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet.Append("(");
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet.Append(",");
                    sRet.Append(selectQuery.Table("query").DefaultView[i].Row["media_id"]);
                }
                if (nCount > 0)
                    sRet.Append(")");
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        public System.Data.DataTable GetChannelMediaDT(Int32 nNumOfItems)
        {
            return GetChannelMediaDT(nNumOfItems, null);
        }

        protected Int32 GetTypeFromfriendlyType(Int32 nFriendlyType, Int32 nGroupID)
        {
            Int32 nID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(s_ConnectionKey);
            selectQuery.SetCachedSec(86400);
            selectQuery += "SELECT MEDIA_TYPE_ID FROM groups_media_type (nolock) WHERE ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            //selectQuery += " AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nFriendlyType);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    object oF_ID = selectQuery.Table("query").DefaultView[0].Row["MEDIA_TYPE_ID"];
                    if (oF_ID != DBNull.Value)
                        nID = int.Parse(oF_ID.ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nID;
        }

        public System.Data.DataTable GetChannelMediaDT(Int32 nNumOfItems, Int32[] nFileTypes)
        {
            return GetChannelMediaDT(nNumOfItems, nFileTypes, true, true);
        }

        // get Medias by channel with Lucene Search - return DataTable with MediaIds  
        public System.Data.DataTable GetChannelMediaDT(Int32 nNumOfItems, Int32[] nFileTypes, bool bOnlyActiveMedias, bool bUseStartDate)
        {
            DataTable dt = null;
            string sWSURL = string.Empty;

            try
            {
                Lucene_WCF.Service client = new Lucene_WCF.Service();
                sWSURL = WS_Utils.GetTcmConfigValue("LUCENE_WCF");
                
                if (!String.IsNullOrEmpty(sWSURL))
                    client.Url = sWSURL;

                log.Debug("GetChannelMedias - "+ string.Format("group:{0}, channel:{1}, lucene_url:{2}", m_nGroupID, m_nChannelID, client.Url));

                var medias = client.GetChannelMedias(m_nGroupID, m_nChannelID, m_nLangID, nFileTypes, bOnlyActiveMedias, bUseStartDate, m_nDevicesRules, 0, 0, 0);
                if (medias == null)
                {
                    return null;
                }

                dt = new DataTable();
                dt.Columns.Add("id");

                DataRow dataRow = dt.NewRow();

                foreach (int media in medias.m_resultIDs)
                {
                    dataRow["id"] = media.ToString();
                    dt.Rows.Add(dataRow);
                    dataRow = dt.NewRow();
                }

                log.Debug("GetChannelMedias - "+ string.Format("channel:{0}, res:{1}", m_nChannelID, medias.n_TotalItems));
            }
            catch (Exception ex)
            {
                log.Error("GetChannelMedias - Error - " + string.Format("cahnnel:{0}, ex:{1}", m_nChannelID, ex.Message), ex);
            }

            return dt;
        }
        
        public System.Data.DataTable GetChannelMediaDT_OLD(Int32 nNumOfItems, Int32[] nFileTypes, bool bOnlyActiveMedias, bool bUseStartDate)
        {
            log.Debug("GetChannelMediaDT - channel=" + m_nChannelID + " bUseStartDate=" + bUseStartDate.ToString());

            if (m_nStatus == 2 || m_nIsActive == 0)
                return null;
            string sFileTypesStr = "";
            if (nFileTypes != null && nFileTypes.Length > 0)
            {
                for (int j = 0; j < nFileTypes.Length; j++)
                {
                    if (j > 0)
                        sFileTypesStr += "|";
                    sFileTypesStr += nFileTypes[j].ToString();
                }
            }

            if (CachingManager.CachingManager.Exist("GetChannelMediaDT" + m_nChannelID.ToString() + "_" + nNumOfItems.ToString() + "_" + m_sOrderBy + "_" + sFileTypesStr + "_" + m_nLangID.ToString() + "_" + m_nCountryID.ToString() + "_" + m_nDeviceID.ToString() + "_" + m_nGroupID.ToString() + "_" + bUseStartDate.ToString()) == true && m_bWithCache == true)
            {
                log.Debug("Caching - GetChannelMediaDT" + m_nChannelID.ToString() + "_" + nNumOfItems.ToString() + "_" + m_sOrderBy + "_" + sFileTypesStr + "_" + m_nLangID.ToString() + "_" + m_nCountryID.ToString() + "_" + m_nDeviceID.ToString() + "_" + m_nGroupID.ToString() + "_" + bUseStartDate.ToString());
                return (System.Data.DataTable)(CachingManager.CachingManager.GetCachedData("GetChannelMediaDT" + m_nChannelID.ToString() + "_" + nNumOfItems.ToString() + "_" + m_sOrderBy + "_" + sFileTypesStr + "_" + m_nLangID.ToString() + "_" + m_nCountryID.ToString() + "_" + m_nDeviceID.ToString() + "_" + m_nGroupID.ToString() + "_" + bUseStartDate.ToString()));
            }
            System.Data.DataTable d = null;
            Int32 nGroupID = m_nGroupID;
            if (nGroupID == 0)
                nGroupID = LoginManager.GetLoginGroupID();
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID, s_ConnectionKey);
            bool bPersonalChannel = false;
            bPersonalChannel = IsPersonal();

            Int32 nLoginID = LoginManager.GetLoginID();
            bool bAnd = false;
            string sTagsIDs = GetTagsIDs(ref bAnd, nGroupID);


            //auto
            if (m_nChannelType == 1)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(s_ConnectionKey);
                //selectQuery.SetCachedSec(7200);

                if (m_nOrderBy == -6 && m_sOrderByAdd == "")
                    selectQuery += "select q.* from (";
                selectQuery += "select distinct ";
                if (nNumOfItems > 0)
                    selectQuery += "  top " + nNumOfItems.ToString() + " ";
                selectQuery += "m.id ";

                if (m_sOrderByAdd == "")
                {

                    if (m_nOrderBy == -10)
                        selectQuery += ",m.start_date ";
                    else if (m_nOrderBy == -12)
                        selectQuery += ",m.create_date ";
                    else if (m_nOrderBy == -11)
                        selectQuery += ",m.name ";
                    else if (m_nOrderBy == -9)
                        selectQuery += ",m.like_counter ";
                    else if (m_nOrderBy == -8)
                        selectQuery += ",((m.VOTES_SUM/( case when m.VOTES_COUNT=0 then 1 else m.VOTES_COUNT end)) * ( case when m.VOTES_COUNT<5 then m.VOTES_COUNT else 5 end)) ,m.VOTES_COUNT ";
                    else if (m_nOrderBy == -7)
                        selectQuery += ",m.VIEWS ";
                    //else if (m_nOrderBy == -6)
                    //selectQuery += ",newid() ";
                    else if (m_nOrderBy == 1)
                        selectQuery += ",m.META1_STR ";
                    else if (m_nOrderBy == 2)
                        selectQuery += ",m.META2_STR ";
                    else if (m_nOrderBy == 3)
                        selectQuery += ",m.META3_STR ";
                    else if (m_nOrderBy == 4)
                        selectQuery += ",m.META4_STR ";
                    else if (m_nOrderBy == 5)
                        selectQuery += ",m.META5_STR ";
                    else if (m_nOrderBy == 6)
                        selectQuery += ",m.META6_STR ";
                    else if (m_nOrderBy == 7)
                        selectQuery += ",m.META7_STR ";
                    else if (m_nOrderBy == 8)
                        selectQuery += ",m.META8_STR ";
                    else if (m_nOrderBy == 9)
                        selectQuery += ",m.META9_STR ";
                    else if (m_nOrderBy == 10)
                        selectQuery += ",m.META10_STR ";
                    else if (m_nOrderBy == 11)
                        selectQuery += ",m.META11_STR ";
                    else if (m_nOrderBy == 12)
                        selectQuery += ",m.META12_STR ";
                    else if (m_nOrderBy == 13)
                        selectQuery += ",m.META13_STR ";
                    else if (m_nOrderBy == 14)
                        selectQuery += ",m.META14_STR ";
                    else if (m_nOrderBy == 15)
                        selectQuery += ",m.META15_STR ";
                    else if (m_nOrderBy == 16)
                        selectQuery += ",m.META16_STR ";
                    else if (m_nOrderBy == 17)
                        selectQuery += ",m.META17_STR ";
                    else if (m_nOrderBy == 18)
                        selectQuery += ",m.META18_STR ";
                    else if (m_nOrderBy == 19)
                        selectQuery += ",m.META19_STR ";
                    else if (m_nOrderBy == 20)
                        selectQuery += ",m.META20_STR ";

                    else if (m_nOrderBy == 21)
                        selectQuery += ",m.META1_DOUBLE ";
                    else if (m_nOrderBy == 22)
                        selectQuery += ",m.META2_DOUBLE ";
                    else if (m_nOrderBy == 23)
                        selectQuery += ",m.META3_DOUBLE ";
                    else if (m_nOrderBy == 24)
                        selectQuery += ",m.META4_DOUBLE ";
                    else if (m_nOrderBy == 25)
                        selectQuery += ",m.META5_DOUBLE ";

                    else if (m_nOrderBy == 26)
                        selectQuery += ",m.META6_DOUBLE ";
                    else if (m_nOrderBy == 27)
                        selectQuery += ",m.META7_DOUBLE ";
                    else if (m_nOrderBy == 28)
                        selectQuery += ",m.META8_DOUBLE ";
                    else if (m_nOrderBy == 29)
                        selectQuery += ",m.META9_DOUBLE ";
                    else if (m_nOrderBy == 30)
                        selectQuery += ",m.META10_DOUBLE ";
                }
                else
                {
                    if (m_sOrderByAdd != "")
                        selectQuery += "," + m_sOrderByAdd;
                }

                selectQuery += " from media m (nolock) ";
                if (m_dtChannelTags.DefaultView.Count > 0 && sTagsIDs != "" && bAnd == false)
                    selectQuery += ",media_tags mt (nolock)";
                if (m_bIsLangMain == false)
                    selectQuery += ",media_translate mtt (nolock)";
                string sPersonal = "";
                if (bPersonalChannel == true)
                {
                    sPersonal = GetPersonalViewdMedia();
                    //selectQuery += ",watchers_media_actions wma";
                }
                //selectQuery += " where m.start_date<getdate() and (m.end_date is null or m.end_date>getdate()) and m.status=1 and m.is_active=1 ";
                selectQuery += " where m.status=1 ";

                if (bOnlyActiveMedias)
                    selectQuery += "and m.is_active=1 ";

                if (m_bIsLangMain == false)
                {
                    selectQuery += " and m.id=mtt.media_id and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mtt.LANGUAGE_ID", "=", m_nLangID);
                    selectQuery += " and mtt.NAME <> '' and mtt.NAME is not null ";
                }
                selectQuery += " and (m.id not in (select id from media (nolock) where";

                //With/ 
                selectQuery += GetDateRangeQuery(bUseStartDate);

                selectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(nGroupID, string.Empty);
                selectQuery += "))";
                selectQuery += " and ";
                selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where";

                selectQuery += GetDateRangeQuery(bUseStartDate);

                selectQuery += "(country_id=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", m_nCountryID);
                selectQuery += ") and (language_id=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", m_nLangID);
                selectQuery += ") and (DEVICE_ID=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", m_nDeviceID);
                selectQuery += ") and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "))";
                selectQuery += " and ";
                if (nFileTypes != null && nFileTypes.Length > 0)
                {
                    selectQuery += " m.id in (select distinct media_id from media_files where is_active=1 and status=1 and MEDIA_TYPE_ID in(";
                    for (int j = 0; j < nFileTypes.Length; j++)
                    {
                        if (j > 0)
                            selectQuery += ",";
                        selectQuery += GetTypeFromfriendlyType(nFileTypes[j], nGroupID);
                    }
                    selectQuery += ")";
                    selectQuery += ") and";
                }
                selectQuery += "(";

                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
                if (sWPGID != "")
                {
                    selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                    selectQuery += sWPGID;
                    selectQuery += ")";
                }
                selectQuery += ") ";
                if (bPersonalChannel == true)
                {
                    if (sPersonal != "")
                        selectQuery += " and m.id in " + sPersonal + " ";
                    else
                        selectQuery += " and m.id in (0) ";
                }
                if (m_dtChannelTags.DefaultView.Count > 0 && sTagsIDs != "")
                {
                    if (bAnd == false)
                    {
                        selectQuery += "and mt.status=1 and ";//and mt.tag_id in (";
                        selectQuery += sTagsIDs;
                        selectQuery += " and mt.MEDIA_ID=m.id ";
                    }
                    else
                    {
                        selectQuery += "and " + sTagsIDs;
                    }
                }


                if (m_sStrValuesQuery.Trim() != "")
                {
                    selectQuery += " and (";
                    selectQuery += m_sStrValuesQuery;
                    selectQuery += ")";
                }
                if (m_sDoubleValuesQuery.Trim() != "")
                {
                    selectQuery += " and (";
                    selectQuery += m_sDoubleValuesQuery;
                    selectQuery += ")";
                }

                if (m_sBoolValuesQuery.Trim() != "")
                {
                    selectQuery += " and (";
                    selectQuery += m_sBoolValuesQuery;
                    selectQuery += ")";
                }
                if (m_nMediaType != 0)
                {
                    selectQuery += " and (";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.MEDIA_TYPE_ID", "=", m_nMediaType);
                    selectQuery += ")";
                }
                if (m_sOrderByAdd == "")
                {
                    if (m_nOrderBy != -6)
                        selectQuery += " order by ";
                    //if (bPersonalChannel = true)
                    //selectQuery += " wma.create_date desc, ";
                    if (m_nOrderBy == -10)
                        selectQuery += "m.start_date ";
                    else if (m_nOrderBy == -12)
                        selectQuery += "m.create_date ";
                    else if (m_nOrderBy == -11)
                        selectQuery += "m.name ";
                    else if (m_nOrderBy == -9)
                        selectQuery += "m.like_counter ";
                    else if (m_nOrderBy == -8)
                        selectQuery += "((m.VOTES_SUM/( case when m.VOTES_COUNT=0 then 1 else m.VOTES_COUNT end)) * ( case when m.VOTES_COUNT<5 then m.VOTES_COUNT else 5 end))  ";
                    else if (m_nOrderBy == -7)
                        selectQuery += "m.VIEWS ";
                    //else if (m_nOrderBy == -6)
                    //selectQuery += "newid() ";
                    else if (m_nOrderBy == 0)
                        selectQuery += "m.id ";

                    else if (m_nOrderBy == 1)
                        selectQuery += "m.META1_STR ";
                    else if (m_nOrderBy == 2)
                        selectQuery += "m.META2_STR ";
                    else if (m_nOrderBy == 3)
                        selectQuery += "m.META3_STR ";
                    else if (m_nOrderBy == 4)
                        selectQuery += "m.META4_STR ";
                    else if (m_nOrderBy == 5)
                        selectQuery += "m.META5_STR ";
                    else if (m_nOrderBy == 6)
                        selectQuery += "m.META6_STR ";
                    else if (m_nOrderBy == 7)
                        selectQuery += "m.META7_STR ";
                    else if (m_nOrderBy == 8)
                        selectQuery += "m.META8_STR ";
                    else if (m_nOrderBy == 9)
                        selectQuery += "m.META9_STR ";
                    else if (m_nOrderBy == 10)
                        selectQuery += "m.META10_STR ";

                    else if (m_nOrderBy == 11)
                        selectQuery += "m.META11_STR ";
                    else if (m_nOrderBy == 12)
                        selectQuery += "m.META12_STR ";
                    else if (m_nOrderBy == 13)
                        selectQuery += "m.META13_STR ";
                    else if (m_nOrderBy == 14)
                        selectQuery += "m.META14_STR ";
                    else if (m_nOrderBy == 15)
                        selectQuery += "m.META15_STR ";
                    else if (m_nOrderBy == 16)
                        selectQuery += "m.META16_STR ";
                    else if (m_nOrderBy == 17)
                        selectQuery += "m.META17_STR ";
                    else if (m_nOrderBy == 18)
                        selectQuery += "m.META18_STR ";
                    else if (m_nOrderBy == 19)
                        selectQuery += "m.META19_STR ";
                    else if (m_nOrderBy == 20)
                        selectQuery += "m.META20_STR ";

                    else if (m_nOrderBy == 21)
                        selectQuery += "m.META1_DOUBLE ";
                    else if (m_nOrderBy == 22)
                        selectQuery += "m.META2_DOUBLE ";
                    else if (m_nOrderBy == 23)
                        selectQuery += "m.META3_DOUBLE ";
                    else if (m_nOrderBy == 24)
                        selectQuery += "m.META4_DOUBLE ";
                    else if (m_nOrderBy == 25)
                        selectQuery += "m.META5_DOUBLE ";

                    else if (m_nOrderBy == 26)
                        selectQuery += "m.META6_DOUBLE ";
                    else if (m_nOrderBy == 27)
                        selectQuery += "m.META7_DOUBLE ";
                    else if (m_nOrderBy == 28)
                        selectQuery += "m.META8_DOUBLE ";
                    else if (m_nOrderBy == 29)
                        selectQuery += "m.META9_DOUBLE ";
                    else if (m_nOrderBy == 30)
                        selectQuery += "m.META10_DOUBLE ";

                    if (m_nOrderDir == 2 && m_nOrderBy != -6)
                        selectQuery += "desc";
                    if (m_nOrderBy == -8)
                        selectQuery += ",m.VOTES_COUNT desc";
                }
                else
                    selectQuery += m_sOrderBy;
                if (m_nOrderBy == -6 && m_sOrderByAdd == "")
                    selectQuery += ")q order by newid()";
                if (selectQuery.Execute("query", true) != null)
                {
                    d = selectQuery.Table("query").Copy();
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            //manu
            if (m_nChannelType == 2)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(s_ConnectionKey);
                selectQuery += "select ";
                if (nNumOfItems > 0)
                    selectQuery += " top " + nNumOfItems.ToString() + " ";
                selectQuery += "m.id from media m (nolock),channels_media cm (nolock) ";
                if (m_bIsLangMain == false)
                    selectQuery += ",media_translate mtt (nolock)";
                selectQuery += " where ";
                if (nFileTypes != null && nFileTypes.Length > 0)
                {
                    selectQuery += " m.id in (select distinct media_id from media_files where is_active=1 and status=1 and MEDIA_TYPE_ID in(";
                    for (int j = 0; j < nFileTypes.Length; j++)
                    {
                        if (j > 0)
                            selectQuery += ",";
                        selectQuery += GetTypeFromfriendlyType(nFileTypes[j], nGroupID);
                    }
                    selectQuery += ")";
                    selectQuery += ") and";
                }
                selectQuery += "(";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
                if (sWPGID != "")
                {
                    selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                    selectQuery += sWPGID;
                    selectQuery += ")";
                }
                //selectQuery += ") and m.start_date<getdate() and (m.end_date is null or m.end_date>getdate()) and cm.media_id=m.id and cm.status=1 and m.status=1 and m.is_active=1 and";
                selectQuery += ") and cm.media_id=m.id and cm.status=1 and m.status=1";

                if (bOnlyActiveMedias)
                    selectQuery += "and m.is_active=1 ";

                selectQuery += " and (m.id not in (select id from media (nolock) where";
                selectQuery += GetDateRangeQuery(bUseStartDate);
                selectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(nGroupID, string.Empty);
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "))";
                selectQuery += " and ";
                selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where";
                selectQuery += GetDateRangeQuery(bUseStartDate);
                selectQuery += "(country_id=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", m_nCountryID);
                selectQuery += ") and (language_id=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", m_nLangID);
                selectQuery += ") and (DEVICE_ID=0 or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", m_nDeviceID);
                selectQuery += ") and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "))";

                if (m_bIsLangMain == false)
                {
                    selectQuery += " and m.id=mtt.media_id and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mtt.LANGUAGE_ID", "=", m_nLangID);
                    selectQuery += " and mtt.NAME <> '' and mtt.NAME is not null ";
                }
                selectQuery += "and";

                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cm.channel_id", "=", m_nChannelID);
                if (m_sOrderByAdd == "")
                {
                    selectQuery += " order by cm.order_num";
                    if (m_nOrderBy == -10)
                        selectQuery += ",m.start_date ";
                    else if (m_nOrderBy == -12)
                        selectQuery += ",m.create_date ";
                    else if (m_nOrderBy == -11)
                        selectQuery += ",m.name ";
                    else if (m_nOrderBy == -9)
                        selectQuery += ",m.like_counter ";
                    else if (m_nOrderBy == -8)
                        selectQuery += ",((m.VOTES_SUM/( case when m.VOTES_COUNT=0 then 1 else m.VOTES_COUNT end)) * ( case when m.VOTES_COUNT<5 then m.VOTES_COUNT else 5 end))  ";
                    else if (m_nOrderBy == -7)
                        selectQuery += ",m.VIEWS ";
                    else if (m_nOrderBy == -6)
                        selectQuery += ",newid() ";
                    else if (m_nOrderBy == 0)
                        selectQuery += ",m.id ";

                    else if (m_nOrderBy == 1)
                        selectQuery += ",m.META1_STR ";
                    else if (m_nOrderBy == 2)
                        selectQuery += ",m.META2_STR ";
                    else if (m_nOrderBy == 3)
                        selectQuery += ",m.META3_STR ";
                    else if (m_nOrderBy == 4)
                        selectQuery += ",m.META4_STR ";
                    else if (m_nOrderBy == 5)
                        selectQuery += ",m.META5_STR ";
                    else if (m_nOrderBy == 6)
                        selectQuery += ",m.META6_STR ";
                    else if (m_nOrderBy == 7)
                        selectQuery += ",m.META7_STR ";
                    else if (m_nOrderBy == 8)
                        selectQuery += ",m.META8_STR ";
                    else if (m_nOrderBy == 9)
                        selectQuery += ",m.META9_STR ";
                    else if (m_nOrderBy == 10)
                        selectQuery += ",m.META10_STR ";

                    else if (m_nOrderBy == 11)
                        selectQuery += ",m.META11_STR ";
                    else if (m_nOrderBy == 12)
                        selectQuery += ",m.META12_STR ";
                    else if (m_nOrderBy == 13)
                        selectQuery += ",m.META13_STR ";
                    else if (m_nOrderBy == 14)
                        selectQuery += ",m.META14_STR ";
                    else if (m_nOrderBy == 15)
                        selectQuery += ",m.META15_STR ";
                    else if (m_nOrderBy == 16)
                        selectQuery += ",m.META16_STR ";
                    else if (m_nOrderBy == 17)
                        selectQuery += ",m.META17_STR ";
                    else if (m_nOrderBy == 18)
                        selectQuery += ",m.META18_STR ";
                    else if (m_nOrderBy == 19)
                        selectQuery += ",m.META19_STR ";
                    else if (m_nOrderBy == 20)
                        selectQuery += ",m.META20_STR ";

                    else if (m_nOrderBy == 21)
                        selectQuery += ",m.META1_DOUBLE ";
                    else if (m_nOrderBy == 22)
                        selectQuery += ",m.META2_DOUBLE ";
                    else if (m_nOrderBy == 23)
                        selectQuery += ",m.META3_DOUBLE ";
                    else if (m_nOrderBy == 24)
                        selectQuery += ",m.META4_DOUBLE ";
                    else if (m_nOrderBy == 25)
                        selectQuery += ",m.META5_DOUBLE ";

                    else if (m_nOrderBy == 26)
                        selectQuery += ",m.META6_DOUBLE ";
                    else if (m_nOrderBy == 27)
                        selectQuery += ",m.META7_DOUBLE ";
                    else if (m_nOrderBy == 28)
                        selectQuery += ",m.META8_DOUBLE ";
                    else if (m_nOrderBy == 29)
                        selectQuery += ",m.META9_DOUBLE ";
                    else if (m_nOrderBy == 30)
                        selectQuery += ",m.META10_DOUBLE ";

                    if (m_nOrderDir == 2)
                        selectQuery += "desc";
                    if (m_nOrderBy == -8)
                        selectQuery += ",m.VOTES_COUNT desc";
                }
                else
                {
                    selectQuery += m_sOrderBy;
                }
                if (selectQuery.Execute("query", true) != null)
                {
                    d = selectQuery.Table("query").Copy();
                }
                selectQuery.Finish();
                selectQuery = null;
            }


            log.Debug("cache string - GetChannelMediaDT" + m_nChannelID.ToString() + "_" + nNumOfItems.ToString() + "_" + m_sOrderBy + "_" + sFileTypesStr + "_" + m_nLangID.ToString() + "_" + m_nCountryID.ToString() + "_" + m_nDeviceID.ToString() + "_" + m_nGroupID.ToString() + "_" + bUseStartDate.ToString());

            if (m_nWatcherID == 0)
                CachingManager.CachingManager.SetCachedData("GetChannelMediaDT" + m_nChannelID.ToString() + "_" + nNumOfItems.ToString() + "_" + m_sOrderBy + "_" + sFileTypesStr + "_" + m_nLangID.ToString() + "_" + m_nCountryID.ToString() + "_" + m_nDeviceID.ToString() + "_" + m_nGroupID.ToString() + "_" + bUseStartDate.ToString(), d, 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            else
                CachingManager.CachingManager.SetCachedData("GetChannelMediaDT" + m_nChannelID.ToString() + "_" + nNumOfItems.ToString() + "_" + m_sOrderBy + "_" + sFileTypesStr + "_" + m_nLangID.ToString() + "_" + m_nCountryID.ToString() + "_" + m_nDeviceID.ToString() + "_" + m_nGroupID.ToString() + "_" + bUseStartDate.ToString(), d, 3600, System.Web.Caching.CacheItemPriority.Normal, 0, false);
            return d;
        }


        private string GetDateRangeQuery(bool bUseStartDate)
        {
            string sQuery = string.Empty;

            if (bUseStartDate)
            {
                sQuery += " (start_date>getdate() or end_date<getdate()) and ";
            }
            else
            {
                sQuery += " end_date<getdate() and";
            }

            return sQuery;
        }
    }
}
