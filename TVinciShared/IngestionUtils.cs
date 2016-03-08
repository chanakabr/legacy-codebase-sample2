using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using System.Configuration;
using System.Threading;
using KLogMonitor;
using System.Reflection;

namespace TVinciShared
{
    public class LanguageString
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public LanguageString(string sLang, string sVal, bool bIsMainLang)
        {
            m_sLang = sLang;
            m_sVal = sVal;
            m_bIsMainLang = bIsMainLang;
        }
        public string m_sLang;
        public string m_sVal;
        bool m_bIsMainLang;
    }

    public class TranslatorStringHolder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public System.Collections.Hashtable m_theTable;

        public TranslatorStringHolder()
        {
            m_theTable = new System.Collections.Hashtable();
        }

        public void AddLanguageString(string sLang, string sVal, bool bIsMainLang)
        {
            AddLanguageString(sLang, sVal, "1", bIsMainLang);
        }

        public void AddLanguageString(string sLang, string sVal, string sCreditID, bool bIsMainLang)
        {
            if (m_theTable.Contains(sCreditID) == false)
            {
                System.Collections.Hashtable arr = new System.Collections.Hashtable();
                m_theTable[sCreditID] = arr;
            }
            try
            {
                if (((System.Collections.Hashtable)m_theTable[sCreditID]).Contains(sLang) == false)
                    ((System.Collections.Hashtable)(m_theTable[sCreditID])).Add(sLang, new LanguageString(sLang, sVal, bIsMainLang));
                else
                    ((LanguageString)(((System.Collections.Hashtable)(m_theTable[sCreditID]))[sLang])).m_sVal += "," + sVal;
            }
            catch
            { }
        }
    }

    public class IngestionUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static public Int32 GetMediaTranslateID(Int32 nMediaID, Int32 nLangID)
        {
            bool b = false;
            bool bb = true;
            return GetMediaTranslateID(nMediaID, nLangID, ref b, bb);
        }

        static public Int32 GetMediaTranslateID(Int32 nMediaID, Int32 nLangID, ref bool bExists, bool bForceCreate)
        {
            bExists = true;
            Int32 nMediaTransID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from media_translate where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nMediaTransID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nMediaTransID == 0)
            {
                bExists = false;
                if (bForceCreate == true)
                {
                    bool bExists1 = false;
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_translate");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("media_ID", "=", nMediaID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;

                    return GetMediaTranslateID(nMediaID, nLangID, ref bExists1, bForceCreate);
                }
            }
            return nMediaTransID;
        }

        static public void TranslateMediaBaseValues(Int32 nMediaID,
            string sMainLang,
            TranslatorStringHolder hMediaName,
            TranslatorStringHolder hMediaDesc,
            string sAssetID,
            string sSC,
            string sSubtitles,
            string sFecm,
            string sDuration,
            TranslatorStringHolder hItemType)
        {
            string sItemType = IngestionUtils.GetTransactionStringHolderValue(hItemType, "1", sMainLang);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from lu_languages where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(CODE3)))", "<>", sMainLang.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sLang = selectQuery.Table("query").DefaultView[i].Row["CODE3"].ToString();
                    Int32 nLangID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    string sMediaName = GetTransactionStringHolderValue(hMediaName, "1", sLang);
                    string sMediaDesc = GetTransactionStringHolderValue(hMediaDesc, "1", sLang);
                    //if (sMediaName.Trim() != "" || sMediaDesc.Trim() != "")
                    //{
                    Int32 nMediaTransID = 0;
                    bool b = false;
                    if (sMediaName.Trim() != "" || sMediaDesc.Trim() != "")
                        nMediaTransID = GetMediaTranslateID(nMediaID, nLangID, ref b, true);
                    else
                        nMediaTransID = GetMediaTranslateID(nMediaID, nLangID, ref b, false);
                    if (nMediaTransID != 0)
                    {
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_translate");
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sMediaName);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sMediaDesc);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META16_STR", "=", sAssetID);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META10_STR", "=", sSC);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META15_STR", "=", sSubtitles);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META2_STR", "=", sItemType);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META17_STR", "=", sFecm);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META6_STR", "=", sDuration);
                        updateQuery += "where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaTransID);

                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                    }
                }
            }

            selectQuery.Finish();
            selectQuery = null;
        }


        static protected Int32 GetTagID(string sVal, Int32 nGroupID, Int32 nTagTypeID, string sAddExtra)
        {
            Int32 nRet = 0;
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from tags where status<>2 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(VALUE)))", "=", sVal.Trim().ToLower());
            selectQuery += "and group_id " + sGroups;
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TAG_TYPE_ID", "=", nTagTypeID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            if (sAddExtra.Trim().ToLower() == "true" && nRet == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tags");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE", "=", sVal);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TAG_TYPE_ID", "=", nTagTypeID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", 43);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;

                //fictivic here
                string sTagType = ODBCWrapper.Utils.GetTableSingleVal("media_tags_types", "name", nTagTypeID).ToString();
                DBManipulator.BuildFictivicMedia(sTagType, sVal, 0, nGroupID);

                return GetTagID(sVal, nGroupID, nTagTypeID, "false");
            }
            return nRet;
        }

        static protected Int32 TranslateTag(Int32 nTagID, Int32 nLangID, Int32 nGroupID, bool bForceTranslate)
        {
            Int32 nTagTransID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from tags_translate tt where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tt.LANGUAGE_ID", "=", nLangID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tt.GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tt.TAG_ID", "=", nTagID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nTagTransID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nTagTransID == 0 && bForceTranslate == true)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tags_translate");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("tag_ID", "=", nTagID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;

                return TranslateTag(nTagID, nLangID, nGroupID, bForceTranslate);
            }
            return nTagTransID;
        }

        static public void UnActivateAllMediaFilePics(Int32 nMediaTypeID,
            Int32 nMediaID, Int32 nGroupID, Int32 nQualityID, string sLanguage = "")
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_files");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nMediaTypeID);
            updateQuery += "and";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_QUALITY_ID", "=", nQualityID);
            updateQuery += "and";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            updateQuery += "and";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (!string.IsNullOrEmpty(sLanguage))
            {
                updateQuery += "and";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Language", "=", sLanguage);
            }
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static public Int32 GetPicMediaFileID(Int32 nPicType,
            Int32 nMediaID, Int32 nGroupID, Int32 nMediaQualityID, bool bUnActivate, string sLanguage = "")
        {
            if (nPicType == 0)
                return 0;
            if (bUnActivate == true)
                UnActivateAllMediaFilePics(nPicType, nMediaID, nGroupID, nMediaQualityID, sLanguage);
            Int32 nMediaFileID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from media_files where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nPicType);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_QUALITY_ID", "=", nMediaQualityID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (!string.IsNullOrEmpty(sLanguage))
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE", "=", sLanguage);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nMediaFileID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nMediaFileID == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_files");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nPicType);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE", "=", sLanguage);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_QUALITY_ID", "=", nMediaQualityID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REF_ID", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EDITOR_REMARKS", "=", "Created by the XTI Service");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);

                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
                return GetPicMediaFileID(nPicType, nMediaID, nGroupID, nMediaQualityID, false, sLanguage);
            }
            return nMediaFileID;
        }

        static public Int32 GetPicMediaFileIDWithDates(Int32 nPicType,
            Int32 nMediaID, Int32 nGroupID, Int32 nMediaQualityID, bool bUnActivate, ref DateTime? startDate, ref DateTime? endDate, string sLanguage = "")
        {
            if (nPicType == 0)
                return 0;
            if (bUnActivate == true)
                UnActivateAllMediaFilePics(nPicType, nMediaID, nGroupID, nMediaQualityID, sLanguage);
            Int32 nMediaFileID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id, start_date, end_date from media_files where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nPicType);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_QUALITY_ID", "=", nMediaQualityID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (!string.IsNullOrEmpty(sLanguage))
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE", "=", sLanguage);
            }
            DataTable dt = selectQuery.Execute("query", true);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                 nMediaFileID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "id");
                 startDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dt.Rows[0], "start_date");
                 endDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dt.Rows[0], "end_date");
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nMediaFileID == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_files");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nPicType);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE", "=", sLanguage);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_QUALITY_ID", "=", nMediaQualityID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REF_ID", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EDITOR_REMARKS", "=", "Created by the XTI Service");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);

                bool isInserted = insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
                if (isInserted)
                {
                    return GetPicMediaFileID(nPicType, nMediaID, nGroupID, nMediaQualityID, false, sLanguage);
                }
            }
            return nMediaFileID;
        }

        static public void UpdateMediaPromoFile(Int32 nMediaID,
            string sMainLang,
            Int32 nGroupID,
            Int32 nTypeID,
            string sCDNCode,
            Int32 nCDNID,
            Int32 nMediaQualityID)
        {
            Int32 nMediaFileID = GetPicMediaFileID(nTypeID, nMediaID, nGroupID, nMediaQualityID, true);
            if (nMediaFileID != 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_files");
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("REF_ID", "=", nPicID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_SUPLIER_ID", "=", nCDNID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_CODE", "=", sCDNCode);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.Now);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaFileID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
        }

        static public Int32 GetFLVActive(Int32 nMediaID, Int32 nGroupID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select max(MEDIA_TYPE_ID) as m_i from media_files where status<>2 and MEDIA_TYPE_ID in (1,11,12,13,14,15) and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    if (selectQuery.Table("query").DefaultView[0].Row["m_i"] != DBNull.Value)
                        nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["m_i"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static public Int32 GetMediaTypeID(string sDescription, Int32 nGroupID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += "select id from media_types where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(DESCRIPTION)))", "=", sDescription.Trim().ToLower());
            selectQuery += " and GROUP_ID " + PageUtils.GetParentsGroupsStr(nGroupID);
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static public Int32 GetMediaTypeID(string sMediaType)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += "select id from lu_media_types where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(DESCRIPTION)))", "=", sMediaType.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static public void M2MHandling(
            string sMainPointerField,
            string sExtraFieldName,
            string sExtraFieldVal,
            string sExtraFieldType,
            string sCollectionPointerField,
            string sCollectionTable,
            string sMiddleTable,
            string sMiddleFieldRefToMain,
            string sMiddleFieldRefToCollection,
            string sAddExtra,
            string sMainLang,
            string sStr,
            Int32 nGroupID, Int32 nMediaID,
            bool bTranslateAuto)
        {
            System.Collections.Specialized.NameValueCollection coll = new System.Collections.Specialized.NameValueCollection();
            string[] sSpliter = { ";", "," };
            string[] sVals = sStr.Split(sSpliter, StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < sVals.Length; j++)
            {
                string sVal = sVals[j];
                Int32 nTagTypeID = 0;
                if (sExtraFieldVal != "")
                    nTagTypeID = int.Parse(sExtraFieldVal);
                Int32 nTagID = GetTagID(sVal, nGroupID, nTagTypeID, sAddExtra);
                TVinciShared.DBManipulator.GetManyToManyContainer(ref coll,
                    sMainPointerField,
                    sExtraFieldName,
                    sExtraFieldVal,
                    sExtraFieldType,
                    sCollectionPointerField,
                    sCollectionTable,
                    sMiddleTable,
                    sMiddleFieldRefToMain,
                    sMiddleFieldRefToCollection,
                    sAddExtra,
                    nMediaID.ToString(),
                    sVal,
                    nGroupID);
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                TVinciShared.DBManipulator.HandleMany2Many(ref coll, ref selectQuery, "");
                if (bTranslateAuto == true)
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select * from lu_languages where ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(CODE3)))", "<>", sMainLang.Trim().ToLower());
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        for (int i = 0; i < nCount; i++)
                        {
                            string sLang = selectQuery.Table("query").DefaultView[i].Row["CODE3"].ToString();
                            Int32 nLangID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                            Int32 nTagTransID = 0;
                            if (sVal.Trim() != "")
                                nTagTransID = TranslateTag(nTagID, nLangID, nGroupID, true);
                            else
                                nTagTransID = TranslateTag(nTagID, nLangID, nGroupID, false);
                            if (nTagTransID != 0)
                            {
                                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("tags_translate");
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE", "=", sVal.Trim());
                                updateQuery += "where ";
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nTagTransID);
                                updateQuery.Execute();
                                updateQuery.Finish();
                                updateQuery = null;
                            }
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                }
            }
        }

        static public string GetTransactionStringHolderValue(TranslatorStringHolder theTransStrHolder, string sCreditID, string sLang)
        {
            if (theTransStrHolder.m_theTable.Contains(sCreditID) == false)
                return "";
            System.Collections.Hashtable theTable = (System.Collections.Hashtable)(theTransStrHolder.m_theTable[sCreditID]);
            if (theTable.Contains(sLang) == false)
                return "";
            LanguageString theLangStr = (LanguageString)(theTable[sLang]);
            string sVal = theLangStr.m_sVal;
            return sVal;
        }

        static public void M2MHandling(
            string sMainPointerField,
            string sExtraFieldName,
            string sExtraFieldVal,
            string sExtraFieldType,
            string sCollectionPointerField,
            string sCollectionTable,
            string sMiddleTable,
            string sMiddleFieldRefToMain,
            string sMiddleFieldRefToCollection,
            string sAddExtra,
            string sMainLang,
            TranslatorStringHolder theStr,
            Int32 nGroupID, Int32 nMediaID)
        {
            System.Collections.Specialized.NameValueCollection coll = new System.Collections.Specialized.NameValueCollection();
            System.Collections.IEnumerator iter = theStr.m_theTable.Keys.GetEnumerator();
            string sFinalVal = "";
            while (iter.MoveNext())
            {
                string sKey = iter.Current.ToString();
                string sValues = GetTransactionStringHolderValue(theStr, sKey, sMainLang);
                string[] sSpliter = { ";" }; //{ ";", "," };
                string[] sVals = sValues.Split(sSpliter, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < sVals.Length; j++)
                {
                    string sVal = sVals[j];
                    if (sFinalVal != "")
                        sFinalVal += ";";
                    sFinalVal += sVal;
                    Int32 nTagTypeID = int.Parse(sExtraFieldVal);
                    Int32 nTagID = GetTagID(sVal, nGroupID, nTagTypeID, sAddExtra);
                    if (sVal != "")
                    {

                        ODBCWrapper.DataSetSelectQuery selectQuery = null;
                        selectQuery = new ODBCWrapper.DataSetSelectQuery();
                        selectQuery += "select * from lu_languages where ";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(CODE3)))", "<>", sMainLang.Trim().ToLower());
                        if (selectQuery.Execute("query", true) != null)
                        {
                            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                            for (int i = 0; i < nCount; i++)
                            {
                                string sLang = selectQuery.Table("query").DefaultView[i].Row["CODE3"].ToString();
                                Int32 nLangID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                                string sTranslatedVal = GetTransactionStringHolderValue(theStr, sKey, sLang);
                                Int32 nTagTransID = 0;
                                if (sTranslatedVal.Trim() != "")
                                    nTagTransID = TranslateTag(nTagID, nLangID, nGroupID, true);
                                else
                                    nTagTransID = TranslateTag(nTagID, nLangID, nGroupID, false);
                                if (nTagTransID != 0 && sTranslatedVal != "")
                                {
                                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("tags_translate");
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE", "=", sTranslatedVal.Trim());
                                    updateQuery += "where ";
                                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nTagTransID);
                                    updateQuery.Execute();
                                    updateQuery.Finish();
                                    updateQuery = null;
                                }
                            }
                        }
                        selectQuery.Finish();
                        selectQuery = null;
                    }
                }
            }
            ODBCWrapper.DataSetSelectQuery selectQuery1 = null;
            TVinciShared.DBManipulator.GetManyToManyContainer(ref coll,
                sMainPointerField,
                sExtraFieldName,
                sExtraFieldVal,
                sExtraFieldType,
                sCollectionPointerField,
                sCollectionTable,
                sMiddleTable,
                sMiddleFieldRefToMain,
                sMiddleFieldRefToCollection,
                sAddExtra,
                nMediaID.ToString(),
                sFinalVal,
                nGroupID);
            TVinciShared.DBManipulator.HandleMany2Many(ref coll, ref selectQuery1, "");
        }


        static public byte[] StringToBytes(string str)
        {
            System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding();
            return encoding.GetBytes(str);
        }

        public static void UploadIngestToFTP(int ingestID, Dictionary<string, byte[]> files)
        {
            ThreadPool.QueueUserWorkItem(delegate
                {
                    string ftpUrl = WS_Utils.GetTcmConfigValue("IngestFtpUrl");
                    string ftpUser = WS_Utils.GetTcmConfigValue("IngestFtpUser");
                    string ftpPass = WS_Utils.GetTcmConfigValue("IngestFtpPass");

                    byte[] zip = ZipUtils.Compress(files);

                    bool shouldRetry = true;
                    int retryCount = 0;

                    try
                    {
                        while (shouldRetry && retryCount < 10)
                        {
                            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(string.Format("{0}/Ingest-{1}.zip", ftpUrl, ingestID));
                            request.Credentials = new NetworkCredential(ftpUser, ftpPass);
                            request.Method = WebRequestMethods.Ftp.UploadFile;
                            request.KeepAlive = false;
                            request.UsePassive = false;
                            request.UseBinary = true;
                            request.ContentLength = zip.Length;
                            request.GetResponse();
                            Stream requestStream = request.GetRequestStream();
                            requestStream.Write(zip, 0, zip.Length);
                            requestStream.Close();
                            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                            response.Close();
                            shouldRetry = false;
                        }
                        if (retryCount == 10)
                        {
                            log.Error("Failed to upload files to FTP Ingest ID = " + ingestID);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("", ex);
                        Thread.Sleep(60000);
                        retryCount++;
                    }
                });
        }

        static public void InsertIngestMediaData(int nIngestID, int nMediaID, string sCoGuid, string sStatus)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ingest_media");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ingest_id", nIngestID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", nMediaID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("co_guid", sCoGuid);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("result_status", sStatus);

            insertQuery.Execute();
            insertQuery.Finish();

        }

        static public int InsertIngestToDB(DateTime createDate, int ingestType, int nGroupID)
        {
            log.Debug("Ingest - DB insert start");
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ingest");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ingest_type", ingestType);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", createDate);

            insertQuery.Execute();
            insertQuery.Finish();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "SELECT ID FROM ingest WHERE ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "AND";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "=", createDate);
            DataTable dt = selectQuery.Execute("query", true);
            selectQuery.Finish();
            log.Debug("Ingest - DB insert end");
            int ingestID = 0;

            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    ingestID = int.Parse(dt.Rows[0][0].ToString());
                }
            }

            return ingestID;
        }
    }
}
