using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Security.Cryptography;
using KLogMonitor;
using System.Reflection;

namespace CollectionTasker
{
    public class CollectionHandler : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private Int32 m_nGroupID;
        private Int32 m_SubscriptionsPurchasesID;
        private Hashtable m_hSubIdToMetadataID;

        public CollectionHandler(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
            : base(nTaskID, nIntervalInSec, engrameters)
        {

            m_nGroupID = 0;
            m_SubscriptionsPurchasesID = 0;

            string[] seperator = { "||" };
            string[] splited = engrameters.Split(seperator, StringSplitOptions.None);
            if (splited.Length == 2)
            {
                m_nGroupID = int.Parse(splited[0]);
                m_SubscriptionsPurchasesID = int.Parse(splited[1]);
            }


            m_hSubIdToMetadataID = new Hashtable();
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new CollectionHandler(nTaskID, nIntervalInSec, engrameters);
        }

        protected override bool DoTheTaskInner()
        {
            log.Debug("Collection Handler start - GroupID=" + m_nGroupID + " start SubPurchasesID=" + m_SubscriptionsPurchasesID);

            Int32 nSubPurID = m_SubscriptionsPurchasesID;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            selectQuery += "select * from subscriptions_purchases where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", ">", m_SubscriptionsPurchasesID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    nSubPurID = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "id", i);

                    string sSubscriptionCode = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "subscription_code", i);
                    string sCurrenyCD = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "currency_CD", i);
                    string sCountryCd = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "country_code", i);
                    string sLANGUAGE_CODE = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "LANGUAGE_CODE", i);
                    string sDEVICE_NAME = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "DEVICE_NAME", i);

                    double dPrice = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "PRICE", i);

                    string key = sSubscriptionCode + "||" + dPrice + "||" + sCurrenyCD;

                    Int32 nPurchaseMetaDataID = 0;

                    if (m_hSubIdToMetadataID.Contains(key))
                    {
                        nPurchaseMetaDataID = (Int32)m_hSubIdToMetadataID[key];
                    }
                    else
                    {

                        //PriceReason theReason = PriceReason.UnKnown;
                        //Subscription theSub = null;
                        //Price p = ConditionalAccess.Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, "", ref theReason, ref theSub, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                        nPurchaseMetaDataID = GetCollectionMetaData(dPrice, sSubscriptionCode);   //, mca);
                        m_hSubIdToMetadataID[key] = nPurchaseMetaDataID;
                    }


                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                    updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_metadata", "=", nPurchaseMetaDataID);
                    updateQuery += "where";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSubPurID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            m_SubscriptionsPurchasesID = nSubPurID;

            //Update scheduled_tasks with last row id of subscriptions_purchases
            string parms = m_nGroupID.ToString() + "||" + m_SubscriptionsPurchasesID.ToString();

            ODBCWrapper.UpdateQuery updateQuery1 = new ODBCWrapper.UpdateQuery("scheduled_tasks");
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("parameters", "=", parms);
            updateQuery1 += "where";
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", m_nTaskID);
            updateQuery1.Execute();
            updateQuery1.Finish();
            updateQuery1 = null;

            log.Debug("Collection Handler finish - GroupID=" + m_nGroupID + " Last SubPurchasesID=" + m_SubscriptionsPurchasesID);

            return true;
        }

        private Int32 GetCollectionMetaData(double dPrice, string sCode)  //, TvinciCA.module mca)
        {
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            CollectionTasker.TvinciPricing.mdoule m = new CollectionTasker.TvinciPricing.mdoule();
            string sWSURL = GetWSURL("pricing_ws");
            if (sWSURL != "")
                m.Url = sWSURL;

            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetSubscriptionData", "pricing", sIP, ref sWSUserName, ref sWSPass);

            TvinciPricing.Subscription theSub = m.GetSubscriptionData(sWSUserName, sWSPass, sCode, "", "", "", true);

            if (theSub == null)
            {
                log.Debug("Null Subscription - GroupID=" + m_nGroupID + " theSub=" + sCode);
                return 0;
            }

            //Get Subscription fictivic media
            Int32 fictMediaID = theSub.m_fictivicMediaID;

            //Get Subscription Type
            int type = 1;

            object oType = ODBCWrapper.Utils.GetTableSingleVal("media", "meta1_bool", fictMediaID, "MAIN_CONNECTION_STRING");

            if (oType != null && oType != DBNull.Value)
                type = int.Parse(oType.ToString());

            //Perform action only if "Collection"
            if (type == 1)
            {
                return 0;
            }

            log.Debug("Collection - GroupID=" + m_nGroupID + " SubscriptionID=" + theSub.m_sObjectCode);

            StringBuilder sRet = new StringBuilder();
            sRet.Append("<collection>");
            sRet.Append("<collectionPrice>");
            sRet.Append(dPrice.ToString());
            sRet.Append("</collectionPrice>");

            double dCatPrice = 0;

            int[] mediaList = m.GetSubscriptionMediaList(sWSUserName, sWSPass, theSub.m_sObjectCode, 0, string.Empty);

            if (mediaList != null && mediaList.Length > 0)
            {

                //Get list of all subscription medias
                string sMediaList = "(";
                for (int i = 0; i < mediaList.Length; i++)
                {
                    if (i > 0)
                        sMediaList += ",";
                    sMediaList += mediaList[i].ToString();
                }
                sMediaList += ")";

                //Get all media_files and get cat_price
                StringBuilder media_files = new StringBuilder();
                media_files.Append("<mediaFiles>");

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select id from media_files where status=1 and is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_type_id", "=", 11); /////////////////////// media_type_id???
                selectQuery += "and media_id in " + sMediaList;
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {

                        Int32 nMediaFileID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());

                        double price = 0.0;
                        Int32 nPPV = 0;

                        Object oPPV = ODBCWrapper.Utils.GetTableSingleVal("ppv_modules_media_files", "ppv_module_id", "media_file_id", "=", nMediaFileID, "pricing_connection");

                        if (oPPV != null && oPPV != DBNull.Value)
                            nPPV = int.Parse(oPPV.ToString());

                        TvinciPricing.PPVModule ppvm = m.GetPPVModuleData(sWSUserName, sWSPass, nPPV.ToString(), string.Empty, string.Empty, string.Empty);

                        if (ppvm != null)
                        {
                            price = ppvm.m_oPriceCode.m_oPrise.m_dPrice;
                        }

                        if (price > 0)
                        {
                            media_files.Append("<mediaFile id=\"" + nMediaFileID + "\" price=\"" + price + "\"></mediaFile>");
                        }

                        dCatPrice += price;

                    }
                }

                media_files.Append("</mediaFiles>");

                selectQuery.Finish();
                selectQuery = null;

                sRet.Append("<catalogPrice>");
                sRet.Append(dCatPrice.ToString());
                sRet.Append("</catalogPrice>");

                sRet.Append(media_files.ToString());

            }

            sRet.Append("</collection>");

            return InsertMetaDataToTable(sRet.ToString());

        }

        private Int32 InsertMetaDataToTable(string sMetaData)
        {
            Int32 nPID = 0;

            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("purchase_metadata");
            insertQuery.SetConnectionKey("CA_CONNECTION_STRING");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("METADATA", "=", sMetaData);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select max(id) as id from purchase_metadata";
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nPID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());

            }
            selectQuery.Finish();
            selectQuery = null;

            return nPID;
        }

        public string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        private void GetCache(Int32 nMin, Int32 nMax)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            selectQuery += "select * from subscriptions_purchases where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", ">", nMin);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "<", nMax);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {

                    string sSubscriptionCode = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "subscription_code", i);
                    string sCurrenyCD = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "currency_CD", i);
                    string sCountryCd = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "country_code", i);
                    string sLANGUAGE_CODE = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "LANGUAGE_CODE", i);
                    string sDEVICE_NAME = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "DEVICE_NAME", i);

                    Int32 nMetaID = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "collection_metadata", i);

                    double dPrice = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "PRICE", i);

                    string key = sSubscriptionCode + "||" + dPrice + "||" + sCurrenyCD;

                    if (!m_hSubIdToMetadataID.Contains(key))
                    {
                        m_hSubIdToMetadataID[key] = nMetaID;


                    }

                }
            }

            selectQuery.Finish();
            selectQuery = null;

        }
    }



}
