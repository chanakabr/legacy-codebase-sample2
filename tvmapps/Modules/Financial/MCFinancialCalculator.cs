using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;
using System.Data;
using System.Threading;
using KLogMonitor;
using System.Reflection;

namespace Financial
{
    public class MCFinancialCalculator : TvinciFinancialCalculatorBase
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const int nPackage = 20;
        private Int32 m_nBillingTransactionID = 0;
        private Int32 m_nTaskID = 0;
        private bool m_bNew;
        //private DateTime m_dStartDate = DateTime.MaxValue;
        //private DateTime m_dEndDate = DateTime.MinValue;

        public MCFinancialCalculator(Int32 nGroupID, Int32 nTaskID, DateTime dStartDate, DateTime dEndDate, bool bNew)
            : base(nGroupID, dStartDate, dEndDate)
        {
            m_bNew = bNew;
            m_nTaskID = nTaskID;
        }

        public override void Calculate()
        {
            log.Debug("TvinciFinancialCalculatorBase - Calculate Start at:" + DateTime.Now.ToString() + " GroupId: " + m_nGroupID.ToString() + " startDate:" + m_dStartDate.ToString("yyyy-MM-dd HH:mm") +
                " endDate:" + m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
            int nLastBillingTransactionId = 0;

            //For Test
            if (!m_bNew)
            {
                bool bClean = ClearOldRecords();
                if (!bClean)
                    return;
            }
            //until here //For Test
            //Start Calculate

            //run on billing_transaction table for the calculation 
            try
            {
                Int32 nCount = 0;
                DataTable dt = null;

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from ";
                selectQuery += "(";
                selectQuery += "  select * , ROW_NUMBER() OVER (partition by purchase_id order by id) as transaction_payment_number ";
                selectQuery += "  from billing_transactions(nolock) where is_active=1 and status=1 ";
                selectQuery += " and ";
                selectQuery += " create_date between '" + m_dStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + m_dEndDate.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_status", "=", 0);
                selectQuery += ")Q";
                selectQuery += " where ";
                selectQuery += " billing_method <> 7";

                if (selectQuery.Execute("query", true) != null)
                {
                    nCount = selectQuery.Table("query").DefaultView.Count;
                    dt = selectQuery.Table("query");
                }
                //close db connection
                selectQuery.Finish();
                selectQuery = null;


                log.Debug("TvinciFinancialCalculatorBase - Calculate Total : " + nCount.ToString() + " rows return");
                if (dt == null || dt.DefaultView.Table == null || nCount == 0)
                    return;
                bool bUpdate = UpdateFinancialRevenuesn(dt, ref nLastBillingTransactionId);

                #region OLD CODE
                //    for (int i = 0; i < nCount; i++)
                //    {
                //        // Depand on   media_file_id (0 = PPV)
                //        FinancialPurchaseObject fpo = new FinancialPurchaseObject();
                //        fpo.m_nId = Utils.GetIntSafeVal(ref selectQuery, "id", i);

                //        nLastBillingTransactionId = fpo.m_nId; // save transactionId for local use
                //        //Update fpo CurrenyID
                //        fpo.m_sCurrencyCD = Utils.GetStrSafeVal(ref selectQuery, "currency_code", i);
                //        if (!m_hCurrencyCDToCurrencyID.Contains(fpo.m_sCurrencyCD))
                //        {
                //            string sCurrenyID = ODBCWrapper.Utils.GetTableSingleVal("lu_currency", "id", "code3", "=", fpo.m_sCurrencyCD, "MAIN_CONNECTION_STRING").ToString();
                //            m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD] = int.Parse(sCurrenyID);
                //        }
                //        fpo.m_nCurrencyID = (Int32)m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD];

                //        fpo.m_dDate = Utils.GetDateSafeVal(ref selectQuery, "create_date", i);

                //        fpo.m_sCountryName = Utils.GetStrSafeVal(ref selectQuery, "country_code", i);

                //        fpo.m_nRelSub = Utils.GetIntSafeVal(ref selectQuery, "subscription_code", i);

                //        fpo.m_sSiteUserGUID = Utils.GetStrSafeVal(ref selectQuery, "site_guid", i);

                //        fpo.m_eItemType = ItemType.PPV;
                //        fpo.m_eRelatedTo = RelatedTo.PPV;

                //        fpo.m_nItemID = Utils.GetIntSafeVal(ref selectQuery, "media_file_id", i);

                //        fpo.m_dDiscountPrice = Utils.GetDoubleSafeVal(ref selectQuery, "price", i);

                //        fpo.m_nBillingMethod = Utils.GetIntSafeVal(ref selectQuery, "BILLING_METHOD", i);
                //        fpo.m_nBillingProvider = Utils.GetIntSafeVal(ref selectQuery, "BILLING_PROVIDER", i);

                //        fpo.m_nPaymentNumber = Utils.GetIntSafeVal(ref selectQuery, "transaction_payment_number", i);

                //        //Get the Catalogue Price
                //        if (fpo.m_nItemID != 0) //PPV
                //        {
                //            CalculatePPV(fpo, selectQuery, i);
                //        }
                //        else if (fpo.m_nRelSub > 0) //Subscription
                //        {
                //            CalculateSubscription(fpo, selectQuery, i);
                //        }
                //    }
                //}


                //selectQuery.Finish();
                //selectQuery = null;
                #endregion OLD CODE
            }
            catch (Exception ex)
            {
                log.Error("TvinciFinancialCalculatorBase - Calculate groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }

            //For Test only
            //string res = FDRToString();

            UpdateGroupedRevenues();
            if (m_bNew && nLastBillingTransactionId > 0)
            {
                UpdateMCScheduledTasks(nLastBillingTransactionId);
            }
        }

        private bool UpdateFinancialRevenuesn(DataTable dt, ref int nLastBillingTransactionId)     //, int nStartIndex, int nEndIndex)
        {
            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    // Depand on   media_file_id (0 = PPV)
                    FinancialPurchaseObject fpo = new FinancialPurchaseObject();
                    fpo.m_nId = ODBCWrapper.Utils.GetIntSafeVal(dr["id"]);
                    nLastBillingTransactionId = fpo.m_nId;     // save transactionId for local use    

                    //Update fpo CurrenyID
                    fpo.m_sCurrencyCD = ODBCWrapper.Utils.GetSafeStr(dr["currency_code"]);
                    if (!m_hCurrencyCDToCurrencyID.Contains(fpo.m_sCurrencyCD))
                    {
                        string sCurrenyID = ODBCWrapper.Utils.GetTableSingleVal("lu_currency", "id", "code3", "=", fpo.m_sCurrencyCD, "MAIN_CONNECTION_STRING").ToString();
                        m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD] = int.Parse(sCurrenyID);
                    }
                    fpo.m_nCurrencyID = (Int32)m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD];

                    fpo.m_dDate = ODBCWrapper.Utils.GetDateSafeVal(dr["create_date"]);
                    fpo.m_sCountryName = ODBCWrapper.Utils.GetSafeStr(dr["country_code"]);
                    fpo.m_nRelSub = ODBCWrapper.Utils.GetIntSafeVal(dr["subscription_code"]);
                    fpo.m_sSiteUserGUID = ODBCWrapper.Utils.GetSafeStr(dr["site_guid"]);

                    fpo.m_eItemType = ItemType.PPV;
                    fpo.m_eRelatedTo = RelatedTo.PPV;

                    fpo.m_nItemID = ODBCWrapper.Utils.GetIntSafeVal(dr["media_file_id"]);
                    fpo.m_dDiscountPrice = ODBCWrapper.Utils.GetDoubleSafeVal(dr["price"]);
                    fpo.m_nBillingMethod = ODBCWrapper.Utils.GetIntSafeVal(dr["BILLING_METHOD"]);
                    fpo.m_nBillingProvider = ODBCWrapper.Utils.GetIntSafeVal(dr["BILLING_PROVIDER"]);
                    fpo.m_nPaymentNumber = ODBCWrapper.Utils.GetIntSafeVal(dr["transaction_payment_number"]);

                    //Get the Catalogue Price
                    if (fpo.m_nItemID != 0) //PPV
                    {
                        CalculatePPV(fpo, dr);
                    }
                    else if (fpo.m_nRelSub > 0) //Subscription
                    {
                        DateTime date = DateTime.Now;
                        CalculateSubscription(fpo, dr); //MultiThread Methode
                        log.Debug("End subscription - " + string.Format("totalSeconds={0}, subscriptionID={1} ", (DateTime.Now - date).TotalSeconds, fpo.m_nRelSub));
                    }
                    if (m_LDataRows.Count >= 1000) // every 1000 
                        AddDataRowsToTable();
                }

                if (m_LDataRows.Count > 0) // if this container include items
                    AddDataRowsToTable();
            }
            catch (Exception ex)
            {
                log.Error("Exception (UpdateFinancialRevenuesn) - " + string.Format(" ex:{0}", ex.Message), ex);
                return false;
            }
            return true;
        }

        //Update for the next calculation run the billing transaction id 
        private void UpdateMCScheduledTasks(int nLastBillingTransactionId)
        {
            try
            {
                string sScheduledParams = string.Empty;
                sScheduledParams = string.Format("{0}||{1}", m_nGroupID, nLastBillingTransactionId);
                ODBCWrapper.UpdateQuery updateTasker = new ODBCWrapper.UpdateQuery("scheduled_tasks");
                updateTasker += ODBCWrapper.Parameter.NEW_PARAM("PARAMETERS", "=", sScheduledParams);
                updateTasker += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.Now);
                updateTasker += "where";
                updateTasker += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", m_nTaskID);
                updateTasker.Execute();
                updateTasker.Finish();
                updateTasker = null;
            }
            catch (Exception ex)
            {
                log.Error("TvinciFinancialCalculatorBase - Calculate - group_id: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }
        }

        //calculate  a PPV transaction - base on Catalog price , and actual price
        private void CalculatePPV(FinancialPurchaseObject fpo, ODBCWrapper.DataSetSelectQuery selectQuery, int i)
        {
            Int32 nPPVM = Utils.GetIntSafeVal(ref selectQuery, "ppvmodule_code", i);
            fpo.m_eItemType = ItemType.PPV;
            fpo.m_eRelatedTo = RelatedTo.PPV;
            if (!m_hPPVMToPrice.Contains(nPPVM))
            {
                TvinciPricing.PPVModule thePPVModule = Utils.GetPPVModule(m_nGroupID, nPPVM, string.Empty, string.Empty, string.Empty);
                if (thePPVModule != null && thePPVModule.m_oPriceCode != null && thePPVModule.m_oPriceCode.m_oPrise != null)
                {
                    m_hPPVMToPrice[nPPVM] = thePPVModule.m_oPriceCode.m_oPrise.m_dPrice;
                    fpo.m_dCataloguePrice = thePPVModule.m_oPriceCode.m_oPrise.m_dPrice;
                }
            }
            else
            {
                fpo.m_dCataloguePrice = (double)m_hPPVMToPrice[nPPVM];
            }

            //handle Coupon uses
            string sCustomData = Utils.GetStrSafeVal(ref selectQuery, "customdata", i);

            if (!string.IsNullOrEmpty(sCustomData))
            {
                XmlDocument theDoc = new XmlDocument();

                try
                {
                    theDoc.LoadXml(sCustomData);
                    XmlNode couponNode = theDoc.SelectSingleNode("customdata/cc");
                    if (couponNode != null && couponNode.FirstChild != null)
                    {
                        string sCoupon = couponNode.FirstChild.Value;
                        fpo.m_nCouponCode = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons", "id", "code", "=", sCoupon, "pricing_connection").ToString());
                    }
                    Int32 nCFID = GetContractFamilyID(fpo.m_nItemID);

                    //Get relevant Contract from Contract Family, according to media file id. 
                    BaseContract bc = m_oBaseFinancial.GetContractWithContractFamilyID(nCFID, 0, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);

                    //Process contract chain
                    CalcContract(bc, fpo);
                }
                catch (Exception ex)
                {
                    log.Error("MCFinancialCalculator - CalculatePPV groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                }
            }

        }

        private void CalculatePPV(FinancialPurchaseObject fpo, DataRow dr)
        {
            Int32 nPPVM = ODBCWrapper.Utils.GetIntSafeVal(dr["ppvmodule_code"]);
            fpo.m_eItemType = ItemType.PPV;
            fpo.m_eRelatedTo = RelatedTo.PPV;
            if (!m_hPPVMToPrice.Contains(nPPVM))
            {
                TvinciPricing.PPVModule thePPVModule = Utils.GetPPVModule(m_nGroupID, nPPVM, string.Empty, string.Empty, string.Empty);
                if (thePPVModule != null && thePPVModule.m_oPriceCode != null && thePPVModule.m_oPriceCode.m_oPrise != null)
                {
                    m_hPPVMToPrice[nPPVM] = thePPVModule.m_oPriceCode.m_oPrise.m_dPrice;
                    fpo.m_dCataloguePrice = thePPVModule.m_oPriceCode.m_oPrise.m_dPrice;
                }
            }
            else
            {
                fpo.m_dCataloguePrice = (double)m_hPPVMToPrice[nPPVM];
            }

            //handle Coupon uses
            string sCustomData = ODBCWrapper.Utils.GetSafeStr(dr["customdata"]);

            if (!string.IsNullOrEmpty(sCustomData))
            {
                XmlDocument theDoc = new XmlDocument();

                try
                {
                    theDoc.LoadXml(sCustomData);
                    XmlNode couponNode = theDoc.SelectSingleNode("customdata/cc");
                    if (couponNode != null && couponNode.FirstChild != null)
                    {
                        string sCoupon = couponNode.FirstChild.Value;
                        fpo.m_nCouponCode = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons", "id", "code", "=", sCoupon, "pricing_connection").ToString());
                    }
                    Int32 nCFID = GetContractFamilyID(fpo.m_nItemID);

                    //Get relevant Contract from Contract Family, according to media file id. 
                    BaseContract bc = m_oBaseFinancial.GetContractWithContractFamilyID(nCFID, 0, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);

                    //Process contract chain
                    CalcContract(bc, fpo);
                }
                catch (Exception ex)
                {
                    log.Error("MCFinancialCalculator - CalculatePPV groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                }
            }
        }

        //calculate  a Subscription transaction - base on purchase_metadata table  , and actual price that was paid for the subscription
        private void CalculateSubscription(FinancialPurchaseObject fpo, ODBCWrapper.DataSetSelectQuery selectQuery, int i)
        {
            fpo.m_eItemType = ItemType.SUBSCRIPTION;
            fpo.m_eRelatedTo = RelatedTo.SUBSCRIPTION;
            fpo.m_dDiscountPrice = fpo.m_dCataloguePrice;

            string sCustomData = Utils.GetStrSafeVal(ref selectQuery, "customdata", i);
            if (!string.IsNullOrEmpty(sCustomData))
            {
                try
                {
                    XmlDocument theDoc = new XmlDocument();
                    theDoc.LoadXml(sCustomData);

                    //handle Coupon uses
                    XmlNode couponNode = theDoc.SelectSingleNode("customdata/cc");
                    if (couponNode != null && couponNode.FirstChild != null)
                    {
                        string sCoupon = couponNode.FirstChild.Value;
                        fpo.m_nCouponCode = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons", "id", "code", "=", sCoupon, "pricing_connection").ToString());
                    }
                }
                catch (Exception ex)
                {
                    log.Error("MCFinancialCalculator - CalculateSubscription  groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                }
            }
            //Get all played media files for subscription             
            int purchase_metadata_id = 0;
            // select the purchase_metadata_id from the new connection table 
            ODBCWrapper.DataSetSelectQuery selectQueryConnection = new ODBCWrapper.DataSetSelectQuery();
            selectQueryConnection.SetConnectionKey("CA_CONNECTION_STRING");
            selectQueryConnection += " select top 1 * from subscriptions_metadata where ";
            selectQueryConnection += ODBCWrapper.Parameter.NEW_PARAM("billing_transaction_id", "=", fpo.m_nId);
            selectQueryConnection += " order by id desc ";

            if (selectQueryConnection.Execute("query", true) != null)
            {
                Int32 nCountPM = selectQueryConnection.Table("query").DefaultView.Count;
                if (nCountPM > 0)
                {
                    purchase_metadata_id = Utils.GetIntSafeVal(ref selectQueryConnection, "purchase_metadata_id", 0);
                }
            }
            selectQueryConnection.Finish();
            selectQueryConnection = null;

            //get all data about this subscription (saved in purchase_metadata table)
            ODBCWrapper.DataSetSelectQuery selectQueryPM = new ODBCWrapper.DataSetSelectQuery();
            selectQueryPM += "select metadata from purchase_metadata where status=1 and ";
            selectQueryPM += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQueryPM += "and";
            selectQueryPM += ODBCWrapper.Parameter.NEW_PARAM("id", "=", purchase_metadata_id);
            selectQueryPM.SetConnectionKey("CA_CONNECTION_STRING");
            if (selectQueryPM.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQueryPM.Table("query").DefaultView.Count;
                if (nCount1 > 0)
                {
                    string metadata = Utils.GetStrSafeVal(ref selectQueryPM, "metadata", 0);

                    XmlDocument theDoc = new XmlDocument();

                    try
                    {
                        theDoc.LoadXml(metadata);

                        XmlNode collectionPrice = theDoc.SelectSingleNode("subscription/subscriptionPrice");
                        XmlNode catalogPrice = theDoc.SelectSingleNode("subscription/catalogPrice");

                        double dCatPrice = 0.0;
                        double dCollPrice = 0.0;

                        string sCollPrice = string.Empty;
                        string sCatPrice = string.Empty;

                        if (collectionPrice != null)
                        {
                            sCollPrice = collectionPrice.FirstChild.Value.Replace(",", ".");
                            dCollPrice = double.Parse(sCollPrice);
                        }

                        if (catalogPrice != null)
                        {
                            sCatPrice = catalogPrice.FirstChild.Value.Replace(",", ".");
                            dCatPrice = double.Parse(sCatPrice);
                        }

                        double discount = 0;

                        if (dCatPrice > 0)
                        {
                            discount = (dCollPrice / dCatPrice);
                        }

                        XmlNodeList owners = theDoc.SelectNodes("subscription/mediaFiles/mediaFile");

                        if (owners == null || owners.Count == 0)
                        {
                            CalcContract(null, fpo);
                        }
                        else
                        {
                            foreach (XmlNode node in owners)
                            {
                                XmlNode owner = node;
                                Int32 nMediaFileID = int.Parse(Utils.GetItemParameterVal(ref owner, "id"));
                                double dPrice = double.Parse(Utils.GetItemParameterVal(ref owner, "price").Replace(",", "."));

                                fpo.m_nItemID = nMediaFileID;

                                fpo.m_dDiscountPrice = dPrice * discount;
                                fpo.m_dCataloguePrice = dPrice;

                                Int32 nCFID = GetContractFamilyID(nMediaFileID);

                                BaseContract bc = m_oBaseFinancial.GetContractWithContractFamilyID(nCFID, 0, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);

                                //Process contract chain
                                CalcContract(bc, fpo);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("MCFinancialCalculator - CalculateSubscription groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                        CalcContract(null, fpo);
                    }
                }
            }
            selectQueryPM.Finish();
            selectQueryPM = null;
        }

        private void CalculateSubscription(FinancialPurchaseObject fpo, DataRow dr)
        {
            fpo.m_eItemType = ItemType.SUBSCRIPTION;
            fpo.m_eRelatedTo = RelatedTo.SUBSCRIPTION;
            fpo.m_dDiscountPrice = fpo.m_dCataloguePrice;

            string sCustomData = ODBCWrapper.Utils.GetSafeStr(dr["customdata"]);
            if (!string.IsNullOrEmpty(sCustomData))
            {
                try
                {
                    XmlDocument theDoc = new XmlDocument();
                    theDoc.LoadXml(sCustomData);

                    //handle Coupon uses
                    XmlNode couponNode = theDoc.SelectSingleNode("customdata/cc");
                    if (couponNode != null && couponNode.FirstChild != null)
                    {
                        string sCoupon = couponNode.FirstChild.Value;
                        fpo.m_nCouponCode = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons", "id", "code", "=", sCoupon, "pricing_connection").ToString());
                    }
                }
                catch (Exception ex)
                {
                    log.Error("MCFinancialCalculator - CalculateSubscription  groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                }
            }
            //Get all played media files for subscription             
            int purchase_metadata_id = 0;
            // select the purchase_metadata_id from the new connection table 
            ODBCWrapper.DataSetSelectQuery selectQueryConnection = new ODBCWrapper.DataSetSelectQuery();
            selectQueryConnection.SetConnectionKey("CA_CONNECTION_STRING");
            selectQueryConnection += " select top 1 * from subscriptions_metadata where ";
            selectQueryConnection += ODBCWrapper.Parameter.NEW_PARAM("billing_transaction_id", "=", fpo.m_nId);
            selectQueryConnection += " order by id desc ";

            if (selectQueryConnection.Execute("query", true) != null)
            {
                Int32 nCountPM = selectQueryConnection.Table("query").DefaultView.Count;
                if (nCountPM > 0)
                {
                    purchase_metadata_id = Utils.GetIntSafeVal(ref selectQueryConnection, "purchase_metadata_id", 0);
                }
            }
            selectQueryConnection.Finish();
            selectQueryConnection = null;

            string metadata = string.Empty;

            //get all data about this subscription (saved in purchase_metadata table)
            ODBCWrapper.DataSetSelectQuery selectQueryPM = new ODBCWrapper.DataSetSelectQuery();
            selectQueryPM += "select metadata from purchase_metadata where status=1 and ";
            selectQueryPM += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQueryPM += "and";
            selectQueryPM += ODBCWrapper.Parameter.NEW_PARAM("id", "=", purchase_metadata_id);
            selectQueryPM.SetConnectionKey("CA_CONNECTION_STRING");
            if (selectQueryPM.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQueryPM.Table("query").DefaultView.Count;
                if (nCount1 > 0)
                {
                    metadata = Utils.GetStrSafeVal(ref selectQueryPM, "metadata", 0); //metadata include all mediaFiles related to subscription
                }
            }
            //close connection string
            selectQueryPM.Finish();
            selectQueryPM = null;
            if (!string.IsNullOrEmpty(metadata))
            {
                XmlDocument theDoc = new XmlDocument();
                try
                {
                    theDoc.LoadXml(metadata);

                    XmlNode collectionPrice = theDoc.SelectSingleNode("subscription/subscriptionPrice");
                    XmlNode catalogPrice = theDoc.SelectSingleNode("subscription/catalogPrice");

                    double dCatPrice = 0.0;
                    double dCollPrice = 0.0;

                    string sCollPrice = string.Empty;
                    string sCatPrice = string.Empty;

                    if (collectionPrice != null)
                    {
                        sCollPrice = collectionPrice.FirstChild.Value.Replace(",", ".");
                        dCollPrice = double.Parse(sCollPrice);
                    }

                    if (catalogPrice != null)
                    {
                        sCatPrice = catalogPrice.FirstChild.Value.Replace(",", ".");
                        dCatPrice = double.Parse(sCatPrice);
                    }

                    double discount = 0;

                    if (dCatPrice > 0)
                    {
                        discount = (dCollPrice / dCatPrice);
                    }

                    XmlNodeList owners = theDoc.SelectNodes("subscription/mediaFiles/mediaFile");

                    if (owners == null || owners.Count == 0)
                    {
                        CalcContract(null, fpo);
                    }
                    else
                    {

                        int nThreadNum = 30;
                        string threadNum = Utils.GetWSURL("ThreadNum");
                        if (!string.IsNullOrEmpty(threadNum))
                            nThreadNum = ODBCWrapper.Utils.GetIntSafeVal(threadNum);

                        // foreach (XmlNode node in owners)
                        int i = 0;
                        while (i < owners.Count)
                        {
                            nThreadNum = Math.Min(owners.Count - i, nThreadNum);
                            ManualResetEvent[] handles = new ManualResetEvent[nThreadNum];
                            for (int j = 0; j < nThreadNum; j++)
                            {
                                XmlNode node = owners.Item(i);
                                i++;
                                handles[j] = new ManualResetEvent(false);
                                ParameterizedThreadStart start = new ParameterizedThreadStart(CalcContracForMediaFile);
                                Thread t = new Thread(start);
                                object[] vals = new object[4];
                                vals[0] = node;   //XmlNode
                                vals[1] = discount;
                                vals[2] = fpo;
                                vals[3] = handles[j];
                                t.Start(vals);
                            }

                            WaitHandle.WaitAll(handles);
                        }
                        #region OLD CODE
                        //XmlNode owner = node;
                        //Int32 nMediaFileID = int.Parse(Utils.GetItemParameterVal(ref owner, "id"));
                        //double dPrice = double.Parse(Utils.GetItemParameterVal(ref owner, "price").Replace(",", "."));

                        //fpo.m_nItemID = nMediaFileID;

                        //fpo.m_dDiscountPrice = dPrice * discount;
                        //fpo.m_dCataloguePrice = dPrice;

                        //Int32 nCFID = GetContractFamilyID(nMediaFileID);

                        //BaseContract bc = m_oBaseFinancial.GetContractWithContractFamilyID(nCFID, 0, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);

                        ////Process contract chain
                        //CalcContract(bc, fpo);
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    log.Error("MCFinancialCalculator - CalculateSubscription  groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                    CalcContract(null, fpo);
                }
            }
        }

        private void CalcContracForMediaFile(object val)
        {
            object[] vals = (object[])val;
            XmlNode owner = (XmlNode)vals[0]; //Convert    
            double discount = (double)vals[1];
            FinancialPurchaseObject fpo = (FinancialPurchaseObject)vals[2];
            ManualResetEvent handle = (ManualResetEvent)vals[3];

            bool bUpdate = CalcContracForMediaFile(owner, discount, fpo);

            handle.Set();
        }

        private bool CalcContracForMediaFile(XmlNode owner, double discount, FinancialPurchaseObject fpo)
        {
            try
            {
                Int32 nMediaFileID = int.Parse(Utils.GetItemParameterVal(ref owner, "id"));
                double dPrice = double.Parse(Utils.GetItemParameterVal(ref owner, "price").Replace(",", "."));

                fpo.m_nItemID = nMediaFileID;

                fpo.m_dDiscountPrice = dPrice * discount;
                fpo.m_dCataloguePrice = dPrice;

                Int32 nCFID = GetContractFamilyID(nMediaFileID);

                BaseContract bc = m_oBaseFinancial.GetContractWithContractFamilyID(nCFID, 0, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);

                //Process contract chain
                CalcContract(bc, fpo);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("TvinciFinancialCalculatorBase - CalcContracForMediaFile - " + string.Format("exception = {0}", ex.Message));
                return false;
            }
        }
    }
}