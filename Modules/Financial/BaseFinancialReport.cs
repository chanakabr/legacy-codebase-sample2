using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TVinciShared;
using System.Data;
using System.Collections;
using ICSharpCode.SharpZipLib.Zip;
using KLogMonitor;
using System.Reflection;

namespace Financial
{
    public enum ReportResponse
    {
        OK = 1,
        ERROR = 2,
        NO_RECORDS = 3,
        ALLREADY_EXSITS = 4,
    }

    public enum ReportType
    {
        FinancialReport = 1,
        BreakDownReport = 2,
        UnKNOWN = 3,
        Zip = 4

    }

    public class GiftObject
    {
        public Int32 nCounter;
        public Int32 nGifts;
        public double dSum;

        public GiftObject()
        {
            nCounter = 0;
            nGifts = 0;
            dSum = 0;
        }
    }

    public class BaseFinancialReport
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected Hashtable hParentEntityTotals;
        protected Hashtable ht;

        protected Hashtable hPPVCounter;
        protected Hashtable hSubsCounter;
        protected Hashtable hPrePaidCounter;


        protected Int32 m_nGroupID;

        protected DateTime m_dStartDate;
        protected DateTime m_dEndDate;

        protected string m_sFileName;
        protected string m_sFileExt;

        protected Object m_oReport;

        protected ReportResponse m_eReportResponse;
        protected ReportType m_eReportType;

        protected Int32 m_nRHEntityID;

        public BaseFinancialReport(DateTime dStartDate, DateTime dEndDate, Int32 nGroupID, Int32 nRHEntityID)
        {
            hParentEntityTotals = new Hashtable();
            ht = new Hashtable();
            hPPVCounter = new Hashtable();

            hSubsCounter = new Hashtable();
            hPrePaidCounter = new Hashtable();

            m_nGroupID = nGroupID;
            m_dStartDate = dStartDate;
            m_dEndDate = dEndDate;

            m_sFileName = string.Empty;
            m_sFileExt = string.Empty;

            m_oReport = null;

            m_eReportResponse = ReportResponse.OK;
            m_eReportType = ReportType.UnKNOWN;

            m_nRHEntityID = nRHEntityID;
        }

        public void CreateReport(string serverPath)
        {
            string[] sfilesEnntries = new string[2];
            string sReport = string.Empty;
            string sRH = string.Empty;

            DeleteOldReport();

            if (m_nRHEntityID > 0)
            {
                sReport = GetReportForRH();
                sRH = "_" + m_nRHEntityID;
            }
            else
            {
                sReport = GetReport();
            }

            m_oReport = sReport;
            m_eReportType = ReportType.FinancialReport;

            string sFileName = string.Format("{0}_{1}{2}", m_dStartDate.Day + "-" + m_dStartDate.Month + "-" + m_dStartDate.Year, m_dEndDate.Day + "-" + m_dEndDate.Month + "-" + m_dEndDate.Year, sRH);

            m_sFileName = sFileName;
            m_sFileExt = "xml";

            AddFile(serverPath);
            string sReportType = "Financial_Report";
            sfilesEnntries[0] = string.Format("{0}_{1}_{2}.{3}", sReportType, m_nGroupID.ToString(), sFileName, m_sFileExt);

            m_oReport = GetRowDataReport();
            m_eReportType = ReportType.BreakDownReport;

            m_sFileName = string.Format("{0}_{1}{2}", m_dStartDate.Day + "-" + m_dStartDate.Month + "-" + m_dStartDate.Year, m_dEndDate.Day + "-" + m_dEndDate.Month + "-" + m_dEndDate.Year, sRH);
            m_sFileExt = "xml";

            AddFile(serverPath);
            sReportType = "Break_Down_Report";
            sfilesEnntries[1] = string.Format("{0}_{1}_{2}.{3}", sReportType, m_nGroupID.ToString(), sFileName, m_sFileExt);

            SaveZipFile(sfilesEnntries, serverPath);
        }

        private void SaveZipFile(string[] sfilesEnntries, string serverPath)
        {

            String path = serverPath;
            path = System.IO.Path.Combine(path, "financial_report");
            path = System.IO.Path.Combine(path, m_nGroupID.ToString());

            string zipName = Utils.SaveZipFile(sfilesEnntries, path);

            string sFTPUN = string.Empty;
            string sFTPPass = string.Empty;
            string sFTP = string.Empty;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select pics_ftp_username, pics_ftp_password, reports_ftp from groups where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sFTPUN = Utils.GetStrSafeVal(ref selectQuery, "pics_ftp_username", 0);
                    sFTPPass = Utils.GetStrSafeVal(ref selectQuery, "pics_ftp_password", 0);
                    sFTP = Utils.GetStrSafeVal(ref selectQuery, "reports_ftp", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            string savePath = System.IO.Path.Combine(path, zipName);

            FTPUploader t = new FTPUploader(savePath, sFTP, sFTPUN, sFTPPass);
            t.Upload();


            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("fr_reports");

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("start_date", "=", m_dStartDate);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", m_dEndDate);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("file_name", "=", "\"" + zipName + "\"");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", 422);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RH_ENTITY_ID", "=", m_nRHEntityID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REPORT_TYPE", "=", (int)ReportType.Zip);
            insertQuery.SetConnectionKey("CONNECTION_STRING");
            insertQuery.Execute();

            insertQuery.Finish();
            insertQuery = null;
        }

        public void AddFile(string serverPath)
        {
            if (m_oReport == null || m_eReportResponse == ReportResponse.NO_RECORDS || m_eReportResponse == ReportResponse.ERROR)
                return;

            //Create File in Folder
            string sReportType = "Financial_Report";
            if (m_eReportType == ReportType.BreakDownReport)
            {
                sReportType = "Break_Down_Report";
            }

            String path = serverPath;
            path = System.IO.Path.Combine(path, "financial_report");
            path = System.IO.Path.Combine(path, m_nGroupID.ToString());

            // Determine whether the directory exists.
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }

            string uniqueFileName = string.Format("{0}_{1}_{2}.{3}", sReportType, m_nGroupID.ToString(), m_sFileName, m_sFileExt);

            string savePath = System.IO.Path.Combine(path, uniqueFileName);

            try
            {
                System.IO.File.WriteAllText(savePath, (string)m_oReport);
            }
            catch (Exception ex)
            {
                log.Error("Error - GroupID=" + m_nGroupID + " startDate=" + m_dStartDate.ToString("yyyy-MM-dd") + " endDate=" + m_dEndDate.ToString("yyyy-MM-dd") + " Error=" + ex.Message, ex);
                return;
            }

            //string sFTPUN = string.Empty;
            //string sFTPPass = string.Empty;
            //string sFTP = string.Empty;

            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery += "select pics_ftp_username, pics_ftp_password, reports_ftp from groups where";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", m_nGroupID);
            //if (selectQuery.Execute("query", true) != null)
            //{
            //    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            //    if (nCount > 0)
            //    {
            //        sFTPUN = Utils.GetStrSafeVal(ref selectQuery, "pics_ftp_username", 0);
            //        sFTPPass = Utils.GetStrSafeVal(ref selectQuery, "pics_ftp_password", 0);
            //        sFTP = Utils.GetStrSafeVal(ref selectQuery, "reports_ftp", 0);
            //    }
            //}
            //selectQuery.Finish();
            //selectQuery = null;

            //FTPUploader t = new FTPUploader(savePath, sFTP, sFTPUN, sFTPPass);
            //t.Upload();         
        }




        private void DeleteOldReport()
        {
            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
            directQuery += "delete from fr_reports where ";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("start_date", "=", m_dStartDate);
            directQuery += "and";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", m_dEndDate);
            directQuery += "and";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            directQuery += "and";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("rh_entity_id", "=", m_nRHEntityID);
            directQuery.Execute();
            directQuery.Finish();
            directQuery = null;
        }

        public virtual string GetReport()
        {
            log.Debug("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + " RightHolder:" + m_nRHEntityID.ToString() + " startDate:" + m_dStartDate.ToString() + " endDate:" + m_dEndDate.ToString());


            StringBuilder sRet = new StringBuilder();
            sRet.Append("<finacialReport>");
            sRet.Append("<totals>");
            sRet.Append(GetTotals());
            sRet.Append("</totals>");

            CountPPVItems();
            CountSubsItems();

            double dPrePaidSum = CountPrePaidItems();

            double sum = 0.0;
            Int32 counter = 0;

            string sTotalPPV = GetTotalPPV(ref sum, ref counter);
            sRet.Append("<totalPPV amount=\"" + Math.Round(sum, 2).ToString() + "\" count=\"" + counter + "\">");
            sRet.Append(sTotalPPV);
            sRet.Append("</totalPPV>");
            sum = 0.0;
            counter = 0;
            string sTotalSubs = GetTotalSubs(ref sum, ref counter);
            sRet.Append("<totalSubscriptions amount=\"" + Math.Round(sum, 2).ToString() + "\" count=\"" + counter + "\">");
            sRet.Append(sTotalSubs);
            sRet.Append("</totalSubscriptions>");

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
                            GetSubscriptionByHolderName(ref sRet, sEntites);
                            sRet.Append("</subscriptions>");

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

            sRet.Append("</holder>");
            sRet.Append("</contentHolders>");

            sRet.Append("<Gifts>");
            GetGifts(ref sRet);
            sRet.Append("</Gifts>");

            sRet.Append("<PrePaids amount=\"" + dPrePaidSum + "\">");
            GetPrePaids(ref sRet);
            sRet.Append("</PrePaids>");

            sRet.Append("<Coupons>");
            sRet.Append(GetCoupons());
            sRet.Append("</Coupons>");

            sRet.Append("</finacialReport>");

            string xml = sRet.ToString();

            return xml;
        }

        private void GetPrePaids(ref StringBuilder sRet)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + " RightHolder:" + m_nRHEntityID.ToString() + "exception: " + ex.Message, ex);
            }
        }

        public virtual void GetGifts(ref StringBuilder sRet)
        {
            try
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
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
            }
        }

        protected void GetSubscriptionByHolderName(ref StringBuilder sRet, string sEntites)
        {
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select rel_sub, sum(amount) as total from fr_financial_entity_revenues where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 2);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
                selectQuery += "and entity_id in " + sEntites + " ";
                selectQuery += "group by rel_sub";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount2 = selectQuery.Table("query").DefaultView.Count;
                    for (int j = 0; j < nCount2; j++)
                    {
                        double dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", j);
                        Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "rel_sub", j);

                        string sItemName = ht[nItemID].ToString();

                        GiftObject go = new GiftObject();
                        if (hSubsCounter.Contains(nItemID))
                        {
                            go = (GiftObject)hSubsCounter[nItemID];
                        }

                        sRet.Append("<subscription item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
            }
        }

        protected void GetPPVByHolderName(ref StringBuilder sRet, string sEntites)
        {
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery2 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery2 += "select item_id, sum(amount) as total from fr_financial_entity_revenues where ";
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery2 += "and";
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 1);
                selectQuery2 += "and";
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
                selectQuery2 += "and";
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
                selectQuery2 += "and entity_id in " + sEntites + " ";
                selectQuery2 += "group by item_id";
                if (selectQuery2.Execute("query", true) != null)
                {
                    Int32 nCount2 = selectQuery2.Table("query").DefaultView.Count;
                    for (int j = 0; j < nCount2; j++)
                    {
                        double dTotal = Utils.GetDoubleSafeVal(ref selectQuery2, "total", j);
                        Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery2, "item_id", j);

                        string sItemName = string.Empty;
                        if (ht[nItemID] != null)
                            sItemName = ht[nItemID].ToString();

                        GiftObject go = new GiftObject();

                        if (hPPVCounter.Contains(nItemID))
                        {
                            go = (GiftObject)hPPVCounter[nItemID];
                        }

                        sRet.Append("<ppv item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");
                    }
                }
                selectQuery2.Finish();
                selectQuery2 = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
            }
        }

        protected void GetSubscriptionByGroupName(ref StringBuilder sRet)
        {
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select rel_sub, sum(amount) as total from fr_financial_entity_revenues where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 2);
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

                        if (hSubsCounter.Contains(nItemID))
                        {
                            go = (GiftObject)hSubsCounter[nItemID];
                        }

                        sRet.Append("<subscription item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
            }
        }

        protected void GetPPVByGroupName(ref StringBuilder sRet)
        {
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select item_id, sum(amount) as total from fr_financial_entity_revenues where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 1);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_id", "=", 0);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
                selectQuery += "group by item_id";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        double dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", i);
                        Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "item_id", i);

                        string sItemName = string.Empty;
                        if (ht[nItemID] != null)
                            sItemName = ht[nItemID].ToString();

                        GiftObject go = new GiftObject();

                        if (hPPVCounter.Contains(nItemID))
                        {
                            go = (GiftObject)hPPVCounter[nItemID];
                        }

                        sRet.Append("<ppv item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
            }
        }

        protected string GetGroupName()
        {
            string sGroupName = string.Empty;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select group_name from groups where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nGroupID);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sGroupName = Utils.GetStrSafeVal(ref selectQuery, "group_name", 0);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
                sGroupName = string.Empty;
            }

            return sGroupName;
        }

        protected string FinancialEntitiesByParentEntity()
        {
            string sEntites = string.Empty;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from fr_financial_entities where status = 1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_type", "=", 1);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_entity_id", "=", m_nRHEntityID);
                selectQuery.SetConnectionKey("CONNECTION_STRING");

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                    if (nCount1 > 0)
                    {
                        sEntites = "(";
                        for (int x = 0; x < nCount1; x++)
                        {
                            if (x > 0)
                                sEntites += ",";

                            Int32 nEID = Utils.GetIntSafeVal(ref selectQuery, "id", x);
                            sEntites += nEID.ToString();
                        }
                        sEntites += ")";
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
                sEntites = string.Empty;
            }
            return sEntites;
        }

        public virtual string GetReportForRH()
        {
            log.Debug("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + " RightHolder:" + m_nRHEntityID.ToString() + " startDate:" + m_dStartDate.ToString() + " endDate:" + m_dEndDate.ToString());

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


            sRet.Append("</finacialReport>");

            return sRet.ToString();
        }

        protected void GetSumRevenuesSubscription(string sEntites, ref double sum, ref Int32 counter, out StringBuilder sSubs)
        {
            sum = 0.0;
            counter = 0;
            sSubs = new StringBuilder();
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select rel_sub, sum(amount) as total from fr_financial_entity_revenues where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 2);
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

                        object oItemName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nItemID, "pricing_connection");
                        string sItemName = string.Empty;
                        if (oItemName != null && oItemName != DBNull.Value)
                            sItemName = oItemName.ToString();

                        GiftObject go = new GiftObject();
                        if (hSubsCounter.Contains(nItemID))
                        {
                            go = (GiftObject)hSubsCounter[nItemID];
                        }

                        sSubs.Append("<subscription item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");

                        sum += dTotal;
                        counter += go.nCounter;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
            }
        }

        protected void GetSumRevenuesPPV(string sEntites, out double sum, out Int32 counter, out StringBuilder sPPV)
        {
            sum = 0.0;
            counter = 0;
            sPPV = new StringBuilder();
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select item_id, sum(amount) as total from fr_financial_entity_revenues where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 1);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
                selectQuery += "and entity_id in " + sEntites + " ";
                selectQuery += "group by item_id";
                selectQuery.SetConnectionKey("CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount2 = selectQuery.Table("query").DefaultView.Count;
                    for (int j = 0; j < nCount2; j++)
                    {
                        double dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", j);
                        Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "item_id", j);

                        string sItemName = string.Empty;
                        if (!ht.Contains(nItemID))
                        {
                            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                            selectQuery1 += "select name from media where id in (select media_id from media_files where ";
                            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nItemID);
                            selectQuery1 += ")";
                            selectQuery1.SetConnectionKey("CONNECTION_STRING");
                            if (selectQuery1.Execute("query", true) != null)
                            {
                                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                                if (nCount1 > 0)
                                {
                                    ht.Add(nItemID, Utils.GetStrSafeVal(ref selectQuery1, "name", 0));
                                }
                            }
                            selectQuery1.Finish();
                            selectQuery1 = null;
                        }

                        sItemName = ht[nItemID].ToString();

                        GiftObject go = new GiftObject();

                        if (hPPVCounter.Contains(nItemID))
                        {
                            go = (GiftObject)hPPVCounter[nItemID];
                        }

                        sPPV.Append("<ppv item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");

                        sum += dTotal;
                        counter += go.nCounter;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
            }
        }

        protected double GetTotalRevenues(string sEntites)
        {
            double dTotal = 0.0;
            try
            {
                //Get "Total revenues (gross)"
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select sum(amount) as total from fr_financial_entity_revenues where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
                selectQuery += "and entity_id in " + sEntites;
                selectQuery.SetConnectionKey("CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", 0);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
            }
            return dTotal;
        }

        protected void CountPPVItems()
        {
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select media_file_id, count(media_file_id) as counter from billing_transactions where is_active=1 and status=1 and ";
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
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "<>", 0);
                selectQuery += "group by media_file_id order by media_file_id";
                selectQuery.SetConnectionKey("CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        Int32 nMediaFileID = Utils.GetIntSafeVal(ref selectQuery, "media_file_id", i);
                        Int32 nItemCounter = Utils.GetIntSafeVal(ref selectQuery, "counter", i);

                        GiftObject go = new GiftObject();
                        go.nCounter = nItemCounter;
                        hPPVCounter.Add(nMediaFileID, go);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select media_file_id, count(media_file_id) as counter from billing_transactions where is_active=1 and status=1 and ";
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
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "<>", 0);
                selectQuery += "group by media_file_id order by media_file_id";
                selectQuery.SetConnectionKey("CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        Int32 nMediaFileID = Utils.GetIntSafeVal(ref selectQuery, "media_file_id", i);
                        Int32 nItemCounter = Utils.GetIntSafeVal(ref selectQuery, "counter", i);

                        if (hPPVCounter.Contains(nMediaFileID))
                        {
                            GiftObject go = (GiftObject)hPPVCounter[nMediaFileID];
                            go.nGifts = nItemCounter;
                            hPPVCounter[nMediaFileID] = go;
                        }
                        else
                        {
                            GiftObject go = new GiftObject();
                            go.nGifts = nItemCounter;
                            hPPVCounter.Add(nMediaFileID, go);
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
            }
        }

        protected string GetTotalPPV(ref double dSum, ref Int32 nCounter)
        {
            dSum = 0.0;
            nCounter = 0;
            StringBuilder sRet = new StringBuilder();

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select item_id, sum(amount) as total from fr_financial_entity_revenues where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 1);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
                selectQuery += "group by item_id";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        double dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", i);
                        Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "item_id", i);

                        string sItemName = string.Empty;

                        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                        selectQuery1 += "select name from media where id in (select media_id from media_files where ";
                        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nItemID);
                        selectQuery1 += ")";
                        if (selectQuery1.Execute("query", true) != null)
                        {
                            Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                            if (nCount1 > 0)
                            {
                                sItemName = Utils.GetStrSafeVal(ref selectQuery1, "name", 0);
                                ht[nItemID] = sItemName;
                            }
                        }
                        selectQuery1.Finish();
                        selectQuery1 = null;

                        GiftObject go = new GiftObject();

                        if (hPPVCounter.Contains(nItemID))
                        {
                            go = (GiftObject)hPPVCounter[nItemID];
                        }

                        sRet.Append("<ppv item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");

                        dSum += dTotal;
                        nCounter += go.nCounter;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport-GetTotalPPV - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
                sRet = new StringBuilder();
            }
            return sRet.ToString();
        }

        public virtual void CountSubsItems()
        {
            try
            {
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
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport- CountSubsItems - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
            }
        }

        protected string GetTotalSubs(ref double dSum, ref Int32 nCounter)
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
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 2);
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

                        object oItemName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nItemID, "pricing_connection");
                        string sItemName = string.Empty;
                        if (oItemName != null && oItemName != DBNull.Value)
                            sItemName = oItemName.ToString();

                        if (nItemID != 0)
                        {
                            ht[nItemID] = sItemName;

                            GiftObject go = new GiftObject();

                            if (hSubsCounter.Contains(nItemID))
                            {
                                go = (GiftObject)hSubsCounter[nItemID];
                            }

                            sRet.Append("<subscription item_id=\"" + nItemID + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\" count=\"" + go.nCounter + "\"/>");

                            dSum += dTotal;
                            nCounter += go.nCounter;
                        }
                        else
                        {
                            sItemName = "UNKNOWN";
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport- GetTotalSubs GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
            }

            return sRet.ToString();
        }

        protected double CountPrePaidItems()
        {
            double dSum = 0;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select pre_paid_code, count(pre_paid_code) as counter, sum(price) as 'total' from billing_transactions where is_active=1 and status=1 and ";
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
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_code", "<>", string.Empty);
                selectQuery += " and pre_paid_code is not null";
                selectQuery += " group by pre_paid_code order by pre_paid_code";
                selectQuery.SetConnectionKey("CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "pre_paid_code", i);
                        Int32 nItemCounter = Utils.GetIntSafeVal(ref selectQuery, "counter", i);
                        double dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", i);

                        dSum += dTotal;

                        GiftObject go = new GiftObject();
                        go.nCounter = nItemCounter;
                        go.dSum = dTotal;

                        hPrePaidCounter.Add(nItemID, go);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select pre_paid_code, count(pre_paid_code) as 'counter' from billing_transactions where is_active=1 and status=1 and ";
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
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_code", "<>", string.Empty);
                selectQuery += " and pre_paid_code is not null";
                selectQuery += " group by pre_paid_code order by pre_paid_code";
                selectQuery.SetConnectionKey("CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        Int32 nItemID = Utils.GetIntSafeVal(ref selectQuery, "pre_paid_code", i);
                        Int32 nItemCounter = Utils.GetIntSafeVal(ref selectQuery, "counter", i);

                        if (hPrePaidCounter.Contains(nItemID))
                        {
                            GiftObject go = (GiftObject)hPrePaidCounter[nItemID];
                            go.nGifts = nItemCounter;
                            hPrePaidCounter[nItemID] = go;
                        }
                        else
                        {
                            GiftObject go = new GiftObject();
                            go.nGifts = nItemCounter;
                            hPrePaidCounter.Add(nItemID, go);
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("BaseFinancialReport- CountPrePaidItems - GroupID=" + m_nGroupID.ToString() + "exception: " + ex.Message, ex);
                dSum = 0.0;
            }

            return dSum;
        }

        protected string GetCoupons()
        {
            StringBuilder sRet = new StringBuilder();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select cu.MEDIA_FILE_ID, cu.SUBSCRIPTION_CODE, cu.coupon_id, c.COUPON_GROUP_ID, cg.CODE from coupons as c, coupon_uses as cu, coupons_groups as cg where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cu.GROUP_ID", "=", m_nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cu.create_date", ">=", m_dStartDate);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cu.create_date", "<", m_dEndDate);
            selectQuery += "and (";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cu.MEDIA_FILE_ID", "<>", 0);
            selectQuery += "or";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cu.SUBSCRIPTION_CODE", "<>", 0);
            selectQuery += ") and cu.coupon_id=c.id and cg.ID=c.COUPON_GROUP_ID order by c.COUPON_GROUP_ID";
            selectQuery.SetConnectionKey("pricing_connection");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nMediaFileID = Utils.GetIntSafeVal(ref selectQuery, "media_file_id", i);
                    Int32 nSubID = Utils.GetIntSafeVal(ref selectQuery, "SUBSCRIPTION_CODE", i);
                    string sGroupName = Utils.GetStrSafeVal(ref selectQuery, "code", i);

                    string sItemName = string.Empty;
                    string sType = "PPV";

                    if (nMediaFileID != 0)
                    {
                        if (!ht.Contains(nMediaFileID))
                        {
                            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                            selectQuery1 += "select name from media where id in (select media_id from media_files where ";
                            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaFileID);
                            selectQuery1 += ")";
                            if (selectQuery1.Execute("query", true) != null)
                            {
                                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                                if (nCount1 > 0)
                                {
                                    sItemName = Utils.GetStrSafeVal(ref selectQuery1, "name", 0);
                                    ht.Add(nMediaFileID, sItemName);
                                }
                            }
                            selectQuery1.Finish();
                            selectQuery1 = null;
                        }
                        else
                        {
                            sItemName = (string)ht[nMediaFileID];
                        }
                    }
                    else
                    {
                        sType = "Subscription";

                        if (!ht.Contains(nSubID))
                        {
                            object oItemName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nSubID, "pricing_connection");
                            if (oItemName != null && oItemName != DBNull.Value)
                                sItemName = oItemName.ToString();
                            ht.Add(nSubID, sItemName);
                        }
                        else
                        {
                            sItemName = (string)ht[nSubID];
                        }
                    }
                    sRet.Append("<coupon name=\"" + ProtocolsFuncs.XMLEncode(sGroupName, true) + "\" type=\"" + sType + "\" item_name=\"" + ProtocolsFuncs.XMLEncode(sItemName, true) + "\"/>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return sRet.ToString();
        }

        protected string GetTotals()
        {
            StringBuilder sRet = new StringBuilder();

            //Get "Total revenues (gross)"
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select sum(amount) as total from fr_financial_entity_revenues where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    double dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", 0);
                    sRet.Append("<total name=\"Total revenues (gross)\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\"/>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            //Get Total revenues by entity 
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select entity_id, sum(amount) as total from fr_financial_entity_revenues where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_id", ">", 0);
            selectQuery += "group by entity_id";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    double dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", i);
                    Int32 nEntityID = Utils.GetIntSafeVal(ref selectQuery, "entity_id", i);

                    int nEType = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entities", "entity_type", nEntityID).ToString());
                    int nEParent = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entities", "parent_entity_id", nEntityID).ToString());

                    string sName = string.Empty;

                    if (nEType == 1)
                    {
                        sName = ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entities", "name", nEParent).ToString();
                    }
                    else
                    {
                        sName = ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entities", "name", nEntityID).ToString();
                    }

                    if (!hParentEntityTotals.Contains(sName))
                    {
                        hParentEntityTotals.Add(sName, dTotal);
                    }
                    else
                    {
                        hParentEntityTotals[sName] = (double)hParentEntityTotals[sName] + dTotal;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            foreach (DictionaryEntry de in hParentEntityTotals)
            {
                string sName = (string)de.Key;
                double dTotal = (double)de.Value;
                sRet.Append("<total name=\"revenues due to " + ProtocolsFuncs.XMLEncode(sName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\"/>");
            }

            string groupName = GetGroupName();
            //Get total revenues for entity_id == 0 (MediaCorp/....)
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select sum(amount) as total from fr_financial_entity_revenues where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_id", "=", 0);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    double dTotal = Utils.GetDoubleSafeVal(ref selectQuery, "total", 0);
                    sRet.Append("<total name=\"revenues due to " + ProtocolsFuncs.XMLEncode(groupName, true) + "\" amount=\"" + Math.Round(dTotal, 2).ToString().Replace(",", ".") + "\"/>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return sRet.ToString();
        }

        protected string GetRowDataReport()
        {
            Hashtable hEntitiesName = new Hashtable();
            Hashtable hMFIDToMID = new Hashtable();
            Hashtable hMIDToName = new Hashtable();
            Hashtable hSubIDToName = new Hashtable();
            Hashtable hPPVMToPrice = new Hashtable();
            Hashtable hMFTOPPVM = new Hashtable();

            StringBuilder sRet = new StringBuilder();

            string row = "<breakDown>";
            sRet.Append(row);

            string sEntites = string.Empty;

            if (m_nRHEntityID > 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery1 += "select id from fr_financial_entities where status = 1 and ";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery1 += "and";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("entity_type", "=", 1);
                selectQuery1 += "and";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("parent_entity_id", "=", m_nRHEntityID);
                selectQuery1.SetConnectionKey("CONNECTION_STRING");
                if (selectQuery1.Execute("query", true) != null)
                {
                    Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                    if (nCount1 > 0)
                    {
                        sEntites = "(";
                        for (int x = 0; x < nCount1; x++)
                        {
                            if (x > 0)
                                sEntites += ",";

                            Int32 nEID = Utils.GetIntSafeVal(ref selectQuery1, "id", x);
                            sEntites += nEID.ToString();
                        }
                        sEntites += ")";
                    }
                }
                selectQuery1.Finish();
                selectQuery1 = null;
            }

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from fr_financial_entity_revenues where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate);
            if (m_nRHEntityID > 0)
            {
                selectQuery += "and entity_id in " + sEntites;
            }
            selectQuery += " order by type, purchase_date, id";
            selectQuery.SetConnectionKey("CONNECTION_STRING");

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nEntityID = Financial.Utils.GetIntSafeVal(ref selectQuery, "entity_id", i);
                    Int32 nItemID = Financial.Utils.GetIntSafeVal(ref selectQuery, "item_id", i);
                    string sSiteGUID = Financial.Utils.GetStrSafeVal(ref selectQuery, "site_guid", i);
                    Int32 nType = Financial.Utils.GetIntSafeVal(ref selectQuery, "type", i);
                    double dAmount = Financial.Utils.GetDoubleSafeVal(ref selectQuery, "amount", i);
                    string sCurrrencyCD = Financial.Utils.GetStrSafeVal(ref selectQuery, "currency_code", i);
                    DateTime dPurchase = Financial.Utils.GetDateSafeVal(ref selectQuery, "purchase_date", i);
                    string sCountry = Financial.Utils.GetStrSafeVal(ref selectQuery, "country", i);
                    Int32 nRelSub = Financial.Utils.GetIntSafeVal(ref selectQuery, "rel_sub", i);
                    double nCatPrice = Financial.Utils.GetDoubleSafeVal(ref selectQuery, "catalog_price", i);
                    double nActPrice = Financial.Utils.GetDoubleSafeVal(ref selectQuery, "actual_price", i);
                    Int32 nCouponID = Financial.Utils.GetIntSafeVal(ref selectQuery, "rel_coupon", i);
                    Int32 nPrePaidID = Financial.Utils.GetIntSafeVal(ref selectQuery, "rel_pre_paid", i);
                    Int32 nPaymentNumber = Financial.Utils.GetIntSafeVal(ref selectQuery, "transaction_payment_number", i);

                    string sEntityName = "No Holder";
                    if (nEntityID == 0)
                    {
                        sEntityName = GetGroupName();
                    }
                    else if (nEntityID > 0)
                    {
                        if (hEntitiesName.Contains(nEntityID))
                        {
                            sEntityName = hEntitiesName[nEntityID].ToString();
                        }
                        else
                        {
                            Int32 nParentEntityID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entities", "parent_entity_id", nEntityID, "CONNECTION_STRING").ToString());
                            if (nParentEntityID > 0)
                            {
                                sEntityName = ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entities", "name", nParentEntityID, "CONNECTION_STRING").ToString();
                            }
                            else
                            {
                                sEntityName = ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entities", "name", nEntityID, "CONNECTION_STRING").ToString();
                            }

                            hEntitiesName.Add(nEntityID, sEntityName);
                        }
                    }

                    string sType = "PPV";
                    if (nType == 2)
                    {
                        sType = "Subscription";
                    }
                    else if (nType == 3 && m_nGroupID == 109) // Only For Filmo
                    {
                        sType = "Collection";
                    }

                    Int32 nMediaID = 0;
                    string sItemName = string.Empty;
                    string sSubName = string.Empty;

                    if (nRelSub != 0)
                    {
                        if (!hSubIDToName.Contains(nRelSub))
                        {
                            hSubIDToName.Add(nRelSub, ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nRelSub, "pricing_connection").ToString());
                        }
                        sSubName = hSubIDToName[nRelSub].ToString();
                    }

                    if (nItemID != 0)
                    {
                        if (!hMFIDToMID.Contains(nItemID))
                        {
                            hMFIDToMID.Add(nItemID, int.Parse(ODBCWrapper.Utils.GetTableSingleVal("media_files", "media_id", nItemID, "CONNECTION_STRING").ToString()));
                        }

                        nMediaID = (Int32)hMFIDToMID[nItemID];

                        if (!hMIDToName.Contains(nMediaID))
                        {
                            hMIDToName.Add(nMediaID, ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID, "CONNECTION_STRING").ToString());
                        }
                        sItemName = hMIDToName[nMediaID].ToString();
                    }

                    row = string.Format("<row Right_Holder=\"{0}\" Item_Name=\"{1}\" Media_ID=\"{2}\" User_Guid=\"{3}\" Type=\"{4}\" Amount=\"{5}\" Currency=\"{6}\"  Purchase_Date=\"{7}\" Country=\"{8}\" Subscription=\"{9}\" Catalog_Price=\"{10}\" Actual_Price=\"{11}\" Payment_Number=\"{12}\" ",
                        ProtocolsFuncs.XMLEncode(sEntityName, true), ProtocolsFuncs.XMLEncode(sItemName, true), nMediaID, sSiteGUID, sType, dAmount, sCurrrencyCD, dPurchase.ToString("yyyy-MM-dd HH:mm"), ProtocolsFuncs.XMLEncode(sCountry, true), ProtocolsFuncs.XMLEncode(sSubName, true), nCatPrice, nActPrice, nPaymentNumber);

                    if (m_nRHEntityID == 0)
                    {
                        string sCouponName = string.Empty;
                        string sPrePaidName = string.Empty;
                        double dPrePaidDiscount = 0;

                        if (nCouponID > 0)
                        {
                            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                            selectQuery1 += "select code from coupons_groups where id in (select COUPON_GROUP_ID from coupons where ";
                            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nCouponID);
                            selectQuery1 += ")";
                            selectQuery1.SetConnectionKey("pricing_connection");
                            if (selectQuery1.Execute("query", true) != null)
                            {
                                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                                if (nCount1 > 0)
                                {
                                    sCouponName = Utils.GetStrSafeVal(ref selectQuery1, "code", 0);
                                }
                            }
                            selectQuery1.Finish();
                            selectQuery1 = null;
                        }

                        if (nPrePaidID > 0)
                        {
                            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                            selectQuery1 += "select name, price_code, value_price_code from pre_paid_modules where";
                            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPrePaidID);
                            selectQuery1.SetConnectionKey("pricing_connection");
                            if (selectQuery1.Execute("query", true) != null)
                            {
                                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                                if (nCount1 > 0)
                                {
                                    sPrePaidName = Utils.GetStrSafeVal(ref selectQuery1, "name", 0);

                                    Int32 nP1 = Utils.GetIntSafeVal(ref selectQuery1, "price_code", 0);
                                    Int32 nP2 = Utils.GetIntSafeVal(ref selectQuery1, "value_price_code", 0);

                                    ODBCWrapper.DataSetSelectQuery selectQuery2 = new ODBCWrapper.DataSetSelectQuery();
                                    selectQuery2 += "select 1-(pc1.price/pc2.price) as dis from (select price from price_codes where ";
                                    selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nP1);
                                    selectQuery2 += ") as pc1, (select price from price_codes where ";
                                    selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nP2);
                                    selectQuery2 += ") as pc2";
                                    selectQuery2.SetConnectionKey("pricing_connection");
                                    if (selectQuery2.Execute("query", true) != null)
                                    {
                                        Int32 nCount2 = selectQuery1.Table("query").DefaultView.Count;
                                        if (nCount2 > 0)
                                        {
                                            dPrePaidDiscount = Utils.GetDoubleSafeVal(ref selectQuery2, "dis", 0);
                                        }
                                    }
                                    selectQuery2.Finish();
                                    selectQuery2 = null;
                                }
                            }
                            selectQuery1.Finish();
                            selectQuery1 = null;
                        }

                        row += string.Format(" Coupon=\"{0}\" Pre_Paid=\"{1}\" Pre_Paid_Discount=\"{2}\"", ProtocolsFuncs.XMLEncode(sCouponName, true), ProtocolsFuncs.XMLEncode(sPrePaidName, true), dPrePaidDiscount);
                    }

                    row += "/>";
                    sRet.Append(row);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            sRet.Append("</breakDown>");
            string sTemp = sRet.ToString();

            return sTemp;
        }
    }
}
