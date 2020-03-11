using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.IO;
using TVinciShared;
using System.Data.OleDb;
using KLogMonitor;
using System.Reflection;

namespace Financial
{
    public class FilmoFinancialReport : BaseFinancialReport
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private Hashtable hCollectionsCounter;

        public FilmoFinancialReport(DateTime dStartDate, DateTime dEndDate, Int32 nGroupID, Int32 nRHEntityID)
            : base(dStartDate, dEndDate, nGroupID, nRHEntityID)
        {
            hCollectionsCounter = new Hashtable();
        }

        public override string GetReport()
        {
            log.Debug("FinancialReport - GroupID=" + m_nGroupID + "RightHolder=" + m_nRHEntityID + " startDate=" + m_dStartDate.ToString("yyyy-MM-dd HH:mm") + " endDate=" + m_dEndDate.ToString("yyyy-MM-dd HH:mm"));

            StringBuilder sRet = new StringBuilder();
            sRet.Append("<finacialReport>");

            sRet.Append("<totals>");
            sRet.Append(GetTotals());
            sRet.Append("</totals>");

            CountPPVItems();
            CountSubsItems();
            CountCollectionsItems();

            double dPrePaidSum = CountPrePaidItems();

            double sum = 0.0;
            Int32 counter = 0;
            string sTotalPPV = GetTotalPPV(ref sum, ref counter);
            sRet.Append("<totalPPV amount=\"" + Math.Round(sum, 2).ToString() + "\" count=\"" + counter + "\">");
            sRet.Append(sTotalPPV);
            sRet.Append("</totalPPV>");

            string sTotalSubs = GetTotalSubs(ref sum, ref counter);
            sRet.Append("<totalSubscriptions amount=\"" + Math.Round(sum, 2).ToString() + "\" count=\"" + counter + "\">");
            sRet.Append(sTotalSubs);
            sRet.Append("</totalSubscriptions>");

            string sTotalColls = GetTotalColls(ref sum, ref counter);
            sRet.Append("<totalCollections amount=\"" + Math.Round(sum, 2).ToString() + "\" count=\"" + counter + "\">");
            sRet.Append(sTotalColls);
            sRet.Append("</totalCollections>");

            sRet.Append("<contentHolders>");

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id, name from fr_financial_entities where status = 1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_type", "=", 1);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_entity_id", "=", 0);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nParentID = Utils.GetIntSafeVal(ref selectQuery, "id", i);
                    string sName = Utils.GetStrSafeVal(ref selectQuery, "name", i);

                    sRet.Append("<holder name=\"" + ProtocolsFuncs.XMLEncode(sName, true) + "\">");

                    ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery1 += "select id from fr_financial_entities where status = 1 and ";
                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                    selectQuery1 += "and";
                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("entity_type", "=", 1);
                    selectQuery1 += "and";
                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("parent_entity_id", "=", nParentID);
                    if (selectQuery1.Execute("query", true) != null)
                    {
                        Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                        if (nCount1 > 0)
                        {
                            string sEntites = "(";
                            for (int x = 0; x < nCount1; x++)
                            {
                                if (x > 0)
                                    sEntites += ",";

                                Int32 nEID = Utils.GetIntSafeVal(ref selectQuery1, "id", x);
                                sEntites += nEID.ToString();
                            }
                            sEntites += ")";

                            //ppv
                            sRet.Append("<PPV>");
                            GetPPVByHolderName(ref sRet, sEntites);
                            sRet.Append("</PPV>");

                            //subscriptions
                            sRet.Append("<subscriptions>");
                            GetSubscriptionByHolderName(ref  sRet, sEntites);
                            sRet.Append("</subscriptions>");

                            //collections
                            sRet.Append("<collections>");
                            GetCollectionByHolderName(ref sRet, sEntites);
                            sRet.Append("</collections>");
                        }
                    }
                    selectQuery1.Finish();
                    selectQuery1 = null;
                    sRet.Append("</holder>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            //Get Holder Name 
            string sGroupName = GetGroupName();
            sRet.Append("<holder name=\"" + ProtocolsFuncs.XMLEncode(sGroupName, true) + "\">");

            //PPV
            sRet.Append("<PPV>");
            GetPPVByGroupName(ref sRet);
            sRet.Append("</PPV>");

            //subscriptions
            sRet.Append("<subscriptions>");
            GetSubscriptionByGroupName(ref sRet);
            sRet.Append("</subscriptions>");

            //collections
            sRet.Append("<collections>");
            GetCollectionByGroupName(ref sRet);
            sRet.Append("</collections>");

            sRet.Append("</holder>");
            sRet.Append("</contentHolders>");

            sRet.Append("<Gifts>");
            GetGifts(ref sRet);
            sRet.Append("</Gifts>");

            sRet.Append("<PrePaids amount=\"" + dPrePaidSum + "\">");
            foreach (DictionaryEntry de in hPrePaidCounter)
            {
                GiftObject go = (GiftObject)de.Value;
                Int32 nItemID = (Int32)de.Key;

                if (go.nCounter > 0)
                {

                    string sName = string.Empty;
                    object oName = ODBCWrapper.Utils.GetTableSingleVal("pre_paid_modules", "name", nItemID, "pricing_connection");
                    if (oName != null && oName != DBNull.Value)
                        sName = oName.ToString();

                    sRet.Append("<prepaid item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sName, true) + "\" amount=\"" + Math.Round(go.dSum, 2) + "\" count=\"" + go.nCounter + "\"/>");
                }
            }
            sRet.Append("</PrePaids>");

            sRet.Append("<Coupons>");

            sRet.Append(GetCoupons());

            sRet.Append("</Coupons>");


            sRet.Append("</finacialReport>");

            string xml = sRet.ToString();

            return xml;
        }

        public override void GetGifts(ref StringBuilder sRet)
        {
            foreach (DictionaryEntry de in hPPVCounter)
            {
                GiftObject go = (GiftObject)de.Value;
                Int32 nItemID = (Int32)de.Key;

                if (go.nGifts > 0)
                {

                    string sName = string.Empty;
                    if (ht.Contains(nItemID))
                    {
                        sName = ht[nItemID].ToString();
                    }
                    else
                    {
                        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                        selectQuery1 += "select name from media where id in (select media_id from media_files where ";
                        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nItemID);
                        selectQuery1 += ")";
                        if (selectQuery1.Execute("query", true) != null)
                        {
                            Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                            if (nCount1 > 0)
                            {

                                sName = Utils.GetStrSafeVal(ref selectQuery1, "name", 0);
                                ht[nItemID] = sName;
                            }
                        }
                        selectQuery1.Finish();
                        selectQuery1 = null;
                    }
                    sRet.Append("<gift item_type=\"PPV\" item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sName, true) + "\" count=\"" + go.nGifts + "\"/>");
                }
            }

            foreach (DictionaryEntry de in hSubsCounter)
            {
                GiftObject go = (GiftObject)de.Value;
                Int32 nItemID = (Int32)de.Key;

                if (go.nGifts > 0)
                {
                    string sName = string.Empty;
                    if (ht.Contains(nItemID))
                    {
                        sName = ht[nItemID].ToString();
                    }
                    else
                    {
                        sName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nItemID, "pricing_connection").ToString();
                    }
                    sRet.Append("<gift item_type=\"Subscription\" item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sName, true) + "\" count=\"" + go.nGifts + "\"/>");
                }
            }

            foreach (DictionaryEntry de in hCollectionsCounter)
            {
                GiftObject go = (GiftObject)de.Value;
                Int32 nItemID = (Int32)de.Key;

                if (go.nGifts > 0)
                {

                    string sName = string.Empty;
                    if (ht.Contains(nItemID))
                    {
                        sName = ht[nItemID].ToString();
                    }
                    else
                    {
                        sName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nItemID, "pricing_connection").ToString();
                    }
                    sRet.Append("<gift item_type=\"Collection\" item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sName, true) + "\" count=\"" + go.nGifts + "\"/>");
                }
            }

            foreach (DictionaryEntry de in hPrePaidCounter)
            {
                GiftObject go = (GiftObject)de.Value;
                Int32 nItemID = (Int32)de.Key;

                if (go.nGifts > 0)
                {
                    string sName = string.Empty;
                    object oName = ODBCWrapper.Utils.GetTableSingleVal("pre_paid_modules", "name", nItemID, "pricing_connection");
                    if (oName != null && oName != DBNull.Value)
                        sName = oName.ToString();

                    sRet.Append("<gift item_type=\"PrePaid\" item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sName, true) + "\" count=\"" + go.nGifts + "\"/>");
                }
            }
        }

        private void GetCollectionByGroupName(ref StringBuilder sRet)
        {
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select rel_sub, sum(amount) as total from fr_financial_entity_revenues where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 3);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_id", "=", 0);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
                selectQuery += "group by rel_sub";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        double dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", i);
                        Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "rel_sub", i);

                        string sItemName = string.Empty;
                        if (ht[nItemID] != null)
                            sItemName = ht[nItemID].ToString();

                        GiftObject go = new GiftObject();

                        if (hCollectionsCounter.Contains(nItemID))
                        {
                            go = (GiftObject)hCollectionsCounter[nItemID];
                        }

                        sRet.Append("<collection item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("FinancialReport - GroupID=" + m_nGroupID + " exception: " + ex.Message, ex);
            }
        }

        private void GetCollectionByHolderName(ref StringBuilder sRet, string sEntites)
        {
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery2 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery2 += "select rel_sub, sum(amount) as total from fr_financial_entity_revenues where ";
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery2 += "and";
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 3);
                selectQuery2 += "and";
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
                selectQuery2 += "and";
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
                selectQuery2 += "and entity_id in " + sEntites + " ";
                selectQuery2 += "group by rel_sub";
                if (selectQuery2.Execute("query", true) != null)
                {
                    Int32 nCount2 = selectQuery2.Table("query").DefaultView.Count;
                    for (int j = 0; j < nCount2; j++)
                    {
                        double dTotal = Utils.GetDoubleSafeVal(ref selectQuery2, "total", j);
                        Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery2, "rel_sub", j);

                        string sItemName = ht[nItemID].ToString();

                        GiftObject go = new GiftObject();

                        if (hCollectionsCounter.Contains(nItemID))
                        {
                            go = (GiftObject)hCollectionsCounter[nItemID];
                        }

                        sRet.Append("<collection item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");
                    }
                }
                selectQuery2.Finish();
                selectQuery2 = null;
            }
            catch (Exception ex)
            {
                log.Error("FinancialReport - GroupID=" + m_nGroupID + " exception: " + ex.Message, ex);
            }
        }

        public string GetReportForRH()
        {
            log.Debug("FinancialReport - FilmoFinancialReport " + "GroupID=" + m_nGroupID + " RightHolder:" + m_nRHEntityID.ToString() + " startDate:" + m_dStartDate.ToString() + " endDate:" + m_dEndDate.ToString());

            StringBuilder sRet = new StringBuilder();

            string sEntites = FinancialEntitiesByParentEntity();

            if (string.IsNullOrEmpty(sEntites))
            {
                m_eReportResponse = ReportResponse.NO_RECORDS;
                return string.Empty;
            }

            string sName = ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entities", "name", m_nRHEntityID, "CONNECTION_STRING").ToString();

            double dTotalRevenues = GetTotalRevenues(sEntites);

            sRet.Append("<finacialReport RightHolder=\"" + ProtocolsFuncs.XMLEncode(sName, true) + "\" start_date=\"" + m_dStartDate.ToShortDateString() + "\" end_date=\"" + m_dEndDate.ToShortDateString() + "\" amount=\"" + Math.Round(dTotalRevenues, 2).ToString().Replace(",", ".") + "\">");

            CountPPVItems();
            CountSubsItems();
            CountCollectionsItems();

            //Sum Revenues For PPV
            double sum = 0.0;
            Int32 counter = 0;
            StringBuilder sPPV;
            GetSumRevenuesPPV(sEntites, out sum, out counter, out sPPV);
            sRet.Append("<PPV amount=\"" + Math.Round(sum, 2).ToString() + "\" count=\"" + counter + "\">");
            sRet.Append(sPPV.ToString());
            sRet.Append("</PPV>");

            //Sum Revenues For Subscription
            sum = 0.0;
            counter = 0;
            StringBuilder sSubs;
            GetSumRevenuesSubscription(sEntites, ref sum, ref counter, out sSubs);
            sRet.Append("<subscriptions amount=\"" + Math.Round(sum, 2).ToString() + "\" count=\"" + counter + "\">");
            sRet.Append(sSubs.ToString());
            sRet.Append("</subscriptions>");

            //Sum Revenues For Collection 
            sum = 0.0;
            counter = 0;
            StringBuilder sColls = GetSumRevenuesCollection(sEntites, ref sum, ref counter);
            sRet.Append("<collections amount=\"" + Math.Round(sum, 2).ToString() + "\" count=\"" + counter + "\">");
            sRet.Append(sColls.ToString());
            sRet.Append("</collections>");

            sRet.Append("</finacialReport>");

            return sRet.ToString();
        }

        private StringBuilder GetSumRevenuesCollection(string sEntites, ref double sum, ref Int32 counter)
        {
            sum = 0.0;
            counter = 0;
            StringBuilder sColls = new StringBuilder();
            try
            {
                //collections
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select rel_sub, sum(amount) as total from fr_financial_entity_revenues where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 3);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
                selectQuery += "and entity_id in " + sEntites + " ";
                selectQuery += "group by rel_sub";
                selectQuery.SetConnectionKey("CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount2 = selectQuery.Table("query").DefaultView.Count;
                    for (int j = 0; j < nCount2; j++)
                    {
                        double dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", j);
                        Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "rel_sub", j);

                        string sItemName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nItemID, "pricing_connection").ToString();

                        GiftObject go = new GiftObject();

                        if (hCollectionsCounter.Contains(nItemID))
                        {
                            go = (GiftObject)hCollectionsCounter[nItemID];
                        }

                        sColls.Append("<collection item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");

                        sum += dTotal;
                        counter += go.nCounter;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Debug("FinancialReport - GroupID=" + m_nGroupID + " exception: " + ex.Message, ex);
                sColls = new StringBuilder();
            }
            return sColls;
        }

        private string GetCollectionsList()
        {
            string sRet = "(";

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select distinct subscription_code from subscriptions_purchases where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_metadata", ">", 0);
            selectQuery.SetConnectionKey("CA_CONNECTION");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount == 0)
                {
                    sRet += "0";
                }
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet += ",";

                    sRet += Utils.GetIntSafeVal(ref selectQuery, "subscription_code", i);

                }
            }
            selectQuery.Finish();
            selectQuery = null;

            sRet += ")";

            return sRet;
        }

        /*Count Subscription For Filmo*/
        public override void CountSubsItems()
        {
            string sCollectionsList = GetCollectionsList();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select subscription_code, count(subscription_code) as counter from billing_transactions where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "<>", 7);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_status", "=", 0);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", 0);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "<>", string.Empty);
            selectQuery += " and subscription_code is not null";
            selectQuery += " and subscription_code not in " + sCollectionsList;
            selectQuery += " group by subscription_code order by subscription_code";
            selectQuery.SetConnectionKey("CONNECTION_STRING");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "subscription_code", i);
                    Int32 nItemCounter = Utils.GetIntSafeVal(ref selectQuery, "counter", i);

                    GiftObject go = new GiftObject();
                    go.nCounter = nItemCounter;

                    hSubsCounter.Add(nItemID, go);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select subscription_code, count(subscription_code) as counter from billing_transactions where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "=", 7);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_status", "=", 0);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", 0);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "<>", string.Empty);
            selectQuery += " and subscription_code is not null";
            selectQuery += " and subscription_code not in " + sCollectionsList;
            selectQuery += " group by subscription_code order by subscription_code";
            selectQuery.SetConnectionKey("CONNECTION_STRING");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "subscription_code", i);
                    Int32 nItemCounter = Utils.GetIntSafeVal(ref selectQuery, "counter", i);

                    if (hSubsCounter.Contains(nItemID))
                    {
                        GiftObject go = (GiftObject)hSubsCounter[nItemID];
                        go.nGifts = nItemCounter;
                        hSubsCounter[nItemID] = go;
                    }
                    else
                    {
                        GiftObject go = new GiftObject();
                        go.nGifts = nItemCounter;
                        hSubsCounter.Add(nItemID, go);
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        private void CountCollectionsItems()
        {
            string sCollectionsList = GetCollectionsList();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select subscription_code, count(subscription_code) as counter from billing_transactions where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "<>", 7);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_status", "=", 0);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", 0);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "<>", string.Empty);
            selectQuery += " and subscription_code is not null";
            selectQuery += " and subscription_code in " + sCollectionsList;
            selectQuery += " group by subscription_code order by subscription_code";
            selectQuery.SetConnectionKey("CONNECTION_STRING");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "subscription_code", i);
                    Int32 nItemCounter = Utils.GetIntSafeVal(ref selectQuery, "counter", i);

                    GiftObject go = new GiftObject();
                    go.nCounter = nItemCounter;
                    hCollectionsCounter.Add(nItemID, go);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select subscription_code, count(subscription_code) as counter from billing_transactions where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "=", 7);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_status", "=", 0);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", 0);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "<>", string.Empty);
            selectQuery += " and subscription_code is not null";
            selectQuery += " and subscription_code in " + sCollectionsList;
            selectQuery += " group by subscription_code order by subscription_code";
            selectQuery.SetConnectionKey("CONNECTION_STRING");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "subscription_code", i);
                    Int32 nItemCounter = Utils.GetIntSafeVal(ref selectQuery, "counter", i);

                    if (hCollectionsCounter.Contains(nItemID))
                    {
                        GiftObject go = (GiftObject)hCollectionsCounter[nItemID];
                        go.nGifts = nItemCounter;
                        hCollectionsCounter[nItemID] = go;
                    }
                    else
                    {
                        GiftObject go = new GiftObject();
                        go.nGifts = nItemCounter;
                        hCollectionsCounter.Add(nItemID, go);
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        private string GetTotalColls(ref double dSum, ref Int32 nCounter)
        {
            dSum = 0.0;
            nCounter = 0;
            StringBuilder sRet = new StringBuilder();

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select rel_sub, sum(amount) as total from fr_financial_entity_revenues where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 3);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
                selectQuery += "group by rel_sub";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        double dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", i);
                        Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "rel_sub", i);

                        string sItemName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nItemID, "pricing_connection").ToString();
                        ht[nItemID] = sItemName;

                        GiftObject go = new GiftObject();

                        if (hCollectionsCounter.Contains(nItemID))
                        {
                            go = (GiftObject)hCollectionsCounter[nItemID];
                        }

                        sRet.Append("<collection item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");

                        dSum += dTotal;
                        nCounter += go.nCounter;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("FinancialReport + GroupID=" + m_nGroupID + " exception: " + ex.Message, ex);
            }
            return sRet.ToString();
        }
    }
}
