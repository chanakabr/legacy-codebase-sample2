using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;
using KLogMonitor;
using System.Reflection;

namespace Financial
{
    public class TvinciFinancialCalculatorBase : FinancialCalculator
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected Hashtable m_hWeightIDToWeightVal;
        protected Hashtable m_hMediaFileToPPVM;
        protected Hashtable m_hPPVMToPrice;

        public TvinciFinancialCalculatorBase(Int32 nGroupID, DateTime dStart, DateTime dEnd)
            : base(nGroupID, dStart, dEnd)
        {
            m_hWeightIDToWeightVal = new Hashtable();
            m_hMediaFileToPPVM = new Hashtable();
            m_hPPVMToPrice = new Hashtable();
        }

        public override void Calculate()
        {
            log.Debug("Calculate -  GroupID:" + m_nGroupID.ToString() + " startDate:" + m_dStartDate.ToString("yyyy-MM-dd HH:mm") + " endDate:" + m_dEndDate.ToString("yyyy-MM-dd HH:mm"));

            bool bClean = ClearOldRecords();
            if (!bClean)
                return;

            //Start Calculate
            /*
             * Old Ver.
            CalculatePPV();
            CalculatePPVWithPrePaid();
            CalculatePPVWithFullCoupon();
            */
            CalculatePPVAll();

            CalculateSubscriptions();

            CalculateCollections();


            //For Test only
            //string res = FDRToString();

            AddDataRowsToTable();
        }

        public virtual void CalculatePPV()
        {
            try
            {
                //Get all relevant ppv_purchases
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from billing_transactions (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "<>", 0);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "<>", 7);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_status", "=", 0);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    log.Debug("CalculatePPV - " + nCount.ToString() + " rows returned");

                    for (int i = 0; i < nCount; i++)
                    {
                        FinancialPurchaseObject fpo = new FinancialPurchaseObject();

                        //Update fpo CurrenyID
                        fpo.m_sCurrencyCD = Utils.GetStrSafeVal(ref selectQuery, "currency_code", i);
                        if (!m_hCurrencyCDToCurrencyID.Contains(fpo.m_sCurrencyCD))
                        {
                            string sCurrenyID = ODBCWrapper.Utils.GetTableSingleVal("lu_currency", "id", "code3", "=", fpo.m_sCurrencyCD, "MAIN_CONNECTION_STRING").ToString();
                            m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD] = int.Parse(sCurrenyID);
                        }
                        fpo.m_nCurrencyID = (Int32)m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD];

                        fpo.m_dDate = Utils.GetDateSafeVal(ref selectQuery, "create_date", i);

                        fpo.m_sCountryName = Utils.GetStrSafeVal(ref selectQuery, "country_code", i);


                        fpo.m_sSiteUserGUID = Utils.GetStrSafeVal(ref selectQuery, "site_guid", i);

                        fpo.m_eItemType = ItemType.PPV;
                        fpo.m_eRelatedTo = RelatedTo.PPV;

                        fpo.m_nItemID = Utils.GetIntSafeVal(ref selectQuery, "media_file_id", i);

                        fpo.m_dDiscountPrice = Utils.GetDoubleSafeVal(ref selectQuery, "price", i);

                        //Add BillingMethod,BillingProvider to fpo object
                        fpo.m_nBillingMethod = Utils.GetIntSafeVal(ref selectQuery, "BILLING_METHOD", i);
                        fpo.m_nBillingProvider = Utils.GetIntSafeVal(ref selectQuery, "BILLING_PROVIDER", i);

                        //Get the Catalogue Price
                        Int32 nPPVM = Utils.GetIntSafeVal(ref selectQuery, "ppvmodule_code", i);
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
                            }
                            catch (Exception ex)
                            {
                                log.Error("CalculatePPV -  groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                            }
                        }

                        Int32 nCFID = GetContractFamilyID(fpo.m_nItemID);

                        //Get relevant Contract from Contract Family, according to media file id. 
                        BaseContract bc = m_oBaseFinancial.GetContractWithContractFamilyID(nCFID, 0, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);

                        //Process contract chain
                        CalcContract(bc, fpo);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("CalculatePPV  groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }
        }

        public virtual void CalculatePPVWithPrePaid()
        {
            try
            {
                //Get all relevant ppv_purchases
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from ppv_purchases (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pp", ">", 0);
                selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    //_logger.Info(string.Format("{0} : {1} {2}", "CalculatePPVWithPrePaid", nCount, "Rows returned"));
                    log.Debug("CalculatePPVWithPrePaid - " + nCount.ToString() + " Rows Returned");

                    for (int i = 0; i < nCount; i++)
                    {
                        FinancialPurchaseObject fpo = new FinancialPurchaseObject();

                        fpo.m_nPrePaidCode = Utils.GetIntSafeVal(ref selectQuery, "rel_pp", i);

                        //Update fpo CurrenyID
                        fpo.m_sCurrencyCD = Utils.GetStrSafeVal(ref selectQuery, "currency_cd", i);
                        if (!m_hCurrencyCDToCurrencyID.Contains(fpo.m_sCurrencyCD))
                        {
                            string sCurrenyID = ODBCWrapper.Utils.GetTableSingleVal("lu_currency", "id", "code3", "=", fpo.m_sCurrencyCD, "MAIN_CONNECTION_STRING").ToString();
                            m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD] = int.Parse(sCurrenyID);
                        }
                        fpo.m_nCurrencyID = (Int32)m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD];

                        fpo.m_dDate = Utils.GetDateSafeVal(ref selectQuery, "create_date", i);

                        fpo.m_sCountryName = Utils.GetStrSafeVal(ref selectQuery, "country_code", i);

                        if (string.IsNullOrEmpty(fpo.m_sCountryName))
                        {
                            fpo.m_sCountryName = "Netherlands";
                        }

                        fpo.m_sSiteUserGUID = Utils.GetStrSafeVal(ref selectQuery, "site_user_guid", i);

                        fpo.m_eItemType = ItemType.PPV;
                        fpo.m_eRelatedTo = RelatedTo.PPV;

                        fpo.m_nItemID = Utils.GetIntSafeVal(ref selectQuery, "media_file_id", i);

                        fpo.m_dDiscountPrice = Utils.GetDoubleSafeVal(ref selectQuery, "price", i);

                        double dDiscount = 1;

                        TvinciPricing.PrePaidModule thePrePaidModule = Utils.GetPrePaidModule(m_nGroupID, fpo.m_nPrePaidCode, string.Empty, string.Empty, string.Empty);
                        if (thePrePaidModule != null)
                        {
                            double dPPPrice = thePrePaidModule.m_PriceCode.m_oPrise.m_dPrice;
                            double dPPCredit = thePrePaidModule.m_CreditValue.m_oPrise.m_dPrice;

                            if (dPPCredit != 0)
                                dDiscount = dPPPrice / dPPCredit;

                            fpo.m_dDiscountPrice *= dDiscount;
                        }

                        string sCustomData = Utils.GetStrSafeVal(ref selectQuery, "customdata", i);
                        if (!string.IsNullOrEmpty(sCustomData))
                        {
                            XmlDocument theDoc = new XmlDocument();

                            try
                            {
                                theDoc.LoadXml(sCustomData);

                                //handle Coupon uses
                                XmlNode couponNode = theDoc.SelectSingleNode("customdata/cc");
                                if (couponNode != null && couponNode.FirstChild != null)
                                {
                                    string sCoupon = couponNode.FirstChild.Value;
                                    fpo.m_nCouponCode = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons", "id", "code", "=", sCoupon, "pricing_connection").ToString());
                                }

                                //Get the Catalogue Price
                                XmlNode ppvmNode = theDoc.SelectSingleNode("customdata/ppvm");
                                if (ppvmNode != null && ppvmNode.FirstChild != null)
                                {
                                    Int32 nPPVM = int.Parse(ppvmNode.FirstChild.Value);

                                    if (!m_hPPVMToPrice.Contains(nPPVM))
                                    {
                                        TvinciPricing.PPVModule thePPVModule = Utils.GetPPVModule(m_nGroupID, nPPVM, string.Empty, string.Empty, string.Empty);
                                        if (thePPVModule != null && thePPVModule.m_oPriceCode != null && thePPVModule.m_oPriceCode.m_oPrise != null)
                                        {
                                            fpo.m_dCataloguePrice = thePPVModule.m_oPriceCode.m_oPrise.m_dPrice;
                                        }
                                    }
                                    else
                                    {
                                        fpo.m_dCataloguePrice = (double)m_hPPVMToPrice[nPPVM];
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("CalculatePPVWithPrePaid groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                            }
                        }

                        Int32 nCFID = GetContractFamilyID(fpo.m_nItemID);

                        //Get relevant Contract from Contract Family, according to media file id. 
                        BaseContract bc = m_oBaseFinancial.GetContractWithContractFamilyID(nCFID, 0, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);

                        //Process contract chain
                        CalcContract(bc, fpo);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("CalculatePPVWithPrePaid groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }
        }

        public virtual void CalculatePPVWithFullCoupon()
        {
            try
            {
                //Get all relevant ppv_purchases
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from ppv_purchases (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("price", "=", 0);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_transaction_id", "=", 0);
                selectQuery += "and (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_pp", "=", 0);
                selectQuery += "or rel_pp is null)";
                selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    //_logger.Info(string.Format("{0} : {1} {2}", "CalculatePPVWithFullCoupon", nCount, "Rows returned"));
                    log.Debug("CalculatePPVWithFullCoupon - " + nCount.ToString() + " Rows Returned");

                    for (int i = 0; i < nCount; i++)
                    {
                        FinancialPurchaseObject fpo = new FinancialPurchaseObject();

                        //Update fpo CurrenyID
                        fpo.m_sCurrencyCD = Utils.GetStrSafeVal(ref selectQuery, "currency_cd", i);
                        if (!m_hCurrencyCDToCurrencyID.Contains(fpo.m_sCurrencyCD))
                        {
                            string sCurrenyID = ODBCWrapper.Utils.GetTableSingleVal("lu_currency", "id", "code3", "=", fpo.m_sCurrencyCD, "MAIN_CONNECTION_STRING").ToString();
                            m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD] = int.Parse(sCurrenyID);
                        }
                        fpo.m_nCurrencyID = (Int32)m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD];
                        fpo.m_dDate = Utils.GetDateSafeVal(ref selectQuery, "create_date", i);
                        fpo.m_sCountryName = Utils.GetStrSafeVal(ref selectQuery, "country_code", i);

                        if (string.IsNullOrEmpty(fpo.m_sCountryName))
                        {
                            fpo.m_sCountryName = "Netherlands";
                        }

                        fpo.m_sSiteUserGUID = Utils.GetStrSafeVal(ref selectQuery, "site_user_guid", i);

                        fpo.m_eItemType = ItemType.PPV;
                        fpo.m_eRelatedTo = RelatedTo.PPV;

                        fpo.m_nItemID = Utils.GetIntSafeVal(ref selectQuery, "media_file_id", i);
                        fpo.m_dDiscountPrice = Utils.GetDoubleSafeVal(ref selectQuery, "price", i);
                        string sCustomData = Utils.GetStrSafeVal(ref selectQuery, "customdata", i);

                        if (!string.IsNullOrEmpty(sCustomData))
                        {
                            XmlDocument theDoc = new XmlDocument();

                            try
                            {
                                theDoc.LoadXml(sCustomData);

                                //handle Coupon uses
                                XmlNode couponNode = theDoc.SelectSingleNode("customdata/cc");
                                if (couponNode != null && couponNode.FirstChild != null)
                                {
                                    string sCoupon = couponNode.FirstChild.Value;
                                    fpo.m_nCouponCode = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons", "id", "code", "=", sCoupon, "pricing_connection").ToString());
                                    TvinciPricing.CouponData theCoupon = Utils.GetCouponStatus(m_nGroupID, sCoupon);

                                    if (theCoupon != null && theCoupon.m_oCouponGroup != null && theCoupon.m_oCouponGroup.m_oDiscountCode != null)
                                    {
                                        if (theCoupon.m_oCouponGroup.m_oDiscountCode.m_dPercent != 100)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    continue;
                                }

                                //Get the Catalogue Price
                                XmlNode ppvmNode = theDoc.SelectSingleNode("customdata/ppvm");
                                if (ppvmNode != null && ppvmNode.FirstChild != null)
                                {
                                    Int32 nPPVM = int.Parse(ppvmNode.FirstChild.Value);

                                    if (!m_hPPVMToPrice.Contains(nPPVM))
                                    {
                                        TvinciPricing.PPVModule thePPVModule = Utils.GetPPVModule(m_nGroupID, nPPVM, string.Empty, string.Empty, string.Empty);
                                        if (thePPVModule != null && thePPVModule.m_oPriceCode != null && thePPVModule.m_oPriceCode.m_oPrise != null)
                                        {
                                            fpo.m_dCataloguePrice = thePPVModule.m_oPriceCode.m_oPrise.m_dPrice;
                                        }
                                    }
                                    else
                                    {
                                        fpo.m_dCataloguePrice = (double)m_hPPVMToPrice[nPPVM];
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("CalculatePPVWithFullCoupon groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        Int32 nCFID = GetContractFamilyID(fpo.m_nItemID);

                        //Get relevant Contract from Contract Family, according to media file id. 
                        BaseContract bc = m_oBaseFinancial.GetContractWithContractFamilyID(nCFID, 0, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);

                        //Process contract chain
                        CalcContract(bc, fpo);

                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("CalculatePPVWithFullCoupon groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }
        }

        public virtual void CalculateCollections()
        {
            try
            {
                //Get all relevant Collection
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select *  from subscriptions_purchases (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_metadata", ">", 0);
                selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    //_logger.Info(string.Format("{0} : {1} {2}","CalculateCollections", nCount, "Rows returned"));
                    log.Debug("CalculateCollections - " + nCount.ToString() + " Rows returned");

                    for (int i = 0; i < nCount; i++)
                    {
                        FinancialPurchaseObject fpo = new FinancialPurchaseObject();

                        Int32 nSubCode = Utils.GetIntSafeVal(ref selectQuery, "subscription_code", i);

                        fpo.m_nPrePaidCode = Utils.GetIntSafeVal(ref selectQuery, "rel_pp", i);

                        Int32 nBillingTransactionID = Utils.GetIntSafeVal(ref selectQuery, "billing_transaction_ID", i);
                        Int32 nBillingMethod = 0;
                        Object oBM = ODBCWrapper.Utils.GetTableSingleVal("billing_transactions", "billing_method", nBillingTransactionID);

                        if (oBM != null && oBM != DBNull.Value)
                            nBillingMethod = int.Parse(oBM.ToString());

                        //Ignore Gifts Or Bad tran
                        if ((nBillingTransactionID == 0 && fpo.m_nPrePaidCode == 0) || nBillingMethod == 7)
                            continue;

                        Int32 nMetadataID = Utils.GetIntSafeVal(ref selectQuery, "collection_metadata", i);

                        fpo.m_sCurrencyCD = Utils.GetStrSafeVal(ref selectQuery, "currency_cd", i);
                        if (!m_hCurrencyCDToCurrencyID.Contains(fpo.m_sCurrencyCD))
                        {
                            string sCurrenyID = ODBCWrapper.Utils.GetTableSingleVal("lu_currency", "id", "code3", "=", fpo.m_sCurrencyCD, "MAIN_CONNECTION_STRING").ToString();
                            m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD] = int.Parse(sCurrenyID);
                        }
                        fpo.m_nCurrencyID = (Int32)m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD];

                        fpo.m_dDate = Utils.GetDateSafeVal(ref selectQuery, "create_date", i);

                        fpo.m_sSiteUserGUID = Utils.GetStrSafeVal(ref selectQuery, "site_user_guid", i);
                        fpo.m_sCountryName = Utils.GetStrSafeVal(ref selectQuery, "country_code", i);


                        fpo.m_dCataloguePrice = Utils.GetDoubleSafeVal(ref selectQuery, "price", i);
                        fpo.m_dDiscountPrice = fpo.m_dCataloguePrice;

                        ////////////////////////////////// No Country Handler ////////////////////////////////
                        if (string.IsNullOrEmpty(fpo.m_sCountryName))
                        {
                            fpo.m_sCountryName = "Netherlands";
                            Object oCountry = ODBCWrapper.Utils.GetTableSingleVal("billing_transactions", "country_code", nBillingTransactionID);
                            if (oCountry != null && oCountry != DBNull.Value && !string.IsNullOrEmpty(oCountry.ToString()))
                                fpo.m_sCountryName = oCountry.ToString();
                        }

                        string sCustomData = Utils.GetStrSafeVal(ref selectQuery, "customdata", i);
                        if (!string.IsNullOrEmpty(sCustomData))
                        {
                            XmlDocument theDoc = new XmlDocument();
                            try
                            {
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
                                log.Error("CalculateCollection - groupId: " + m_nGroupID.ToString() + " exception:" + ex.Message, ex);
                            }
                        }

                        fpo.m_eItemType = ItemType.COLLECTION;
                        fpo.m_eRelatedTo = RelatedTo.PPV;

                        fpo.m_nRelSub = nSubCode;

                        //Get all played media files for subscription
                        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                        selectQuery1 += "select metadata from purchase_metadata where status=1 and ";
                        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                        selectQuery1 += "and";
                        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMetadataID);
                        selectQuery1.SetConnectionKey("CA_CONNECTION_STRING");
                        if (selectQuery1.Execute("query", true) != null)
                        {
                            Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                            if (nCount1 > 0)
                            {
                                string metadata = Utils.GetStrSafeVal(ref selectQuery1, "metadata", 0);

                                XmlDocument theDoc = new XmlDocument();

                                try
                                {
                                    theDoc.LoadXml(metadata);

                                    XmlNode collectionPrice = theDoc.SelectSingleNode("collection/collectionPrice");
                                    XmlNode catalogPrice = theDoc.SelectSingleNode("collection/catalogPrice");

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

                                    XmlNodeList owners = theDoc.SelectNodes("collection/mediaFiles/mediaFile");

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
                                    log.Error("CalculateCollection - groupId: " + m_nGroupID.ToString() + " exception:" + ex.Message, ex);
                                    CalcContract(null, fpo);
                                }
                            }
                        }
                        selectQuery1.Finish();
                        selectQuery1 = null;
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("CalculateCollection - groupId: " + m_nGroupID.ToString() + " exception:" + ex.Message, ex);
            }
        }

        public virtual void CalcSubscription(FinancialMedia[] fms, FinancialPurchaseObject fpo, double dTotalAdjustedPlays, double dTotalSubscriptionRevenues)
        {
            try
            {
                if (fms.Length == 0)
                {
                    //Subscription with no uses - all revenues to Account (0)
                    AddPriceToContainer(0, dTotalSubscriptionRevenues, fpo, 0);
                    return;
                }

                //Loop all media_files
                foreach (FinancialMedia fm in fms)
                {
                    //Calculate adj share
                    double dAdjShare = fm.m_dMediaFileWeight / dTotalAdjustedPlays;

                    //Calculate the "Net rev share" 
                    double dAdjRevShare = dAdjShare * dTotalSubscriptionRevenues;

                    //Update The price
                    fpo.m_dCataloguePrice = dAdjRevShare;

                    fpo.m_nCouponCode = fm.m_nCouponCode;

                    Int32 nPPVModule = 0;
                    if (!m_hMediaFileToPPVM.Contains(fm.m_nMediaFileID))
                    {
                        ODBCWrapper.DataSetSelectQuery selectQueryPM = new ODBCWrapper.DataSetSelectQuery();
                        selectQueryPM += "select ppv_module_id from ppv_modules_media_files where is_active=1 and status=1 and";
                        selectQueryPM += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", fm.m_nMediaFileID);
                        selectQueryPM.SetConnectionKey("pricing_connection");
                        if (selectQueryPM.Execute("query", true) != null)
                        {
                            Int32 nCount1 = selectQueryPM.Table("query").DefaultView.Count;
                            if (nCount1 > 0)
                            {
                                nPPVModule = Financial.Utils.GetIntSafeVal(ref selectQueryPM, "ppv_module_id", 0);
                                m_hMediaFileToPPVM.Add(fm.m_nMediaFileID, nPPVModule);
                            }
                        }
                        selectQueryPM.Finish();
                        selectQueryPM = null;
                    }
                    else
                    {
                        nPPVModule = (Int32)m_hMediaFileToPPVM[fm.m_nMediaFileID];
                    }

                    if (!m_hPPVMToPrice.Contains(nPPVModule))
                    {
                        TvinciPricing.PPVModule thePPVModule = Utils.GetPPVModule(m_nGroupID, nPPVModule, string.Empty, string.Empty, string.Empty);
                        if (thePPVModule != null && thePPVModule.m_oPriceCode != null && thePPVModule.m_oPriceCode.m_oPrise != null)
                        {
                            m_hPPVMToPrice[nPPVModule] = thePPVModule.m_oPriceCode.m_oPrise.m_dPrice;
                            fpo.m_dCataloguePrice = thePPVModule.m_oPriceCode.m_oPrise.m_dPrice;
                        }
                    }
                    else
                    {
                        fpo.m_dCataloguePrice = (double)m_hPPVMToPrice[nPPVModule];
                    }

                    fpo.m_dDiscountPrice = dAdjRevShare;

                    fpo.m_nItemID = fm.m_nMediaFileID;
                    fpo.m_dDate = fm.m_dDate;
                    fpo.m_sSiteUserGUID = fm.m_sSiteUserGUID;

                    fpo.m_sCountryName = fm.m_sCountryName;

                    /////////////////////////////  No Country Handler /////////////////////////////
                    //Get Country by site_guid
                    if (string.IsNullOrEmpty(fpo.m_sCountryName))
                    {
                        fpo.m_sCountryName = "Netherlands";

                        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                        selectQuery += "select country_code from billing_transactions where is_active=1 and status=1 and ";
                        selectQuery += "country_code is not null and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("country_code", "<>", string.Empty);
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_guid", "=", m_nGroupID);
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                        if (selectQuery.Execute("query", true) != null)
                        {
                            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                            if (nCount > 0)
                            {
                                fpo.m_sCountryName = Financial.Utils.GetStrSafeVal(ref selectQuery, "country_code", 0);
                            }
                        }
                        selectQuery.Finish();
                        selectQuery = null;
                    }
                    //Get relevant contract for owner
                    BaseContract bc = m_oBaseFinancial.GetContractWithContractFamilyID(fm.m_nMediaFileOwner, 0, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);

                    //Process contract chain
                    CalcContract(bc, fpo);
                }
            }
            catch (Exception ex)
            {
                log.Error("CalculateSubscriptions - groupId: " + m_nGroupID.ToString() + " exception:" + ex.Message, ex);
            }
        }

        public virtual void CalculateSubscriptions()
        {
            try
            {
                //get all subscription transaction from billing_transaction table
                #region
                ODBCWrapper.DataSetSelectQuery selectBTQuery = new ODBCWrapper.DataSetSelectQuery();
                selectBTQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectBTQuery += " select * from billing_transactions ";
                selectBTQuery += " where is_active=1 and status=1 and ";
                selectBTQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_status", "=", 0);
                selectBTQuery += " and ";
                selectBTQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", ">", 0);
                selectBTQuery += " and ";
                selectBTQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", 0);
                selectBTQuery += " and";
                selectBTQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectBTQuery += " and";
                selectBTQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectBTQuery += " and";
                selectBTQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));

                if (selectBTQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectBTQuery.Table("query").DefaultView.Count;
                    Int32 nBillingTransactionID = 0;
                    log.Debug("CalculateSubscriptions - " + nCount.ToString() + " Rows returned (billing_transactions)");

                    for (int i = 0; i < nCount; i++)
                    {
                        FinancialPurchaseObject fpo = new FinancialPurchaseObject();
                        nBillingTransactionID = Utils.GetIntSafeVal(ref selectBTQuery, "id", i);
                        fpo.m_nBillingMethod = Utils.GetIntSafeVal(ref selectBTQuery, "billing_method", i);
                        fpo.m_nBillingProvider = Utils.GetIntSafeVal(ref selectBTQuery, "billing_provider", i);
                        fpo.m_sSiteUserGUID = Utils.GetStrSafeVal(ref selectBTQuery, "site_user_guid", i);
                        fpo.m_sCountryName = Utils.GetStrSafeVal(ref selectBTQuery, "country_code", i);
                        fpo.m_nRelSub = Utils.GetIntSafeVal(ref selectBTQuery, "subscription_code", i);
                        fpo.m_dCataloguePrice = Utils.GetDoubleSafeVal(ref selectBTQuery, "price", i);
                        fpo.m_dDiscountPrice = fpo.m_dCataloguePrice;
                        fpo.m_dDate = Utils.GetDateSafeVal(ref selectBTQuery, "create_date", i);
                        fpo.m_eItemType = ItemType.SUBSCRIPTION;
                        fpo.m_eRelatedTo = RelatedTo.SUBSCRIPTION;

                        //Update fpo CurrenyID
                        fpo.m_sCurrencyCD = Utils.GetStrSafeVal(ref selectBTQuery, "currency_code", i);
                        if (!m_hCurrencyCDToCurrencyID.Contains(fpo.m_sCurrencyCD))
                        {
                            string sCurrenyID = ODBCWrapper.Utils.GetTableSingleVal("lu_currency", "id", "code3", "=", fpo.m_sCurrencyCD, "MAIN_CONNECTION_STRING").ToString();
                            m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD] = int.Parse(sCurrenyID);
                        }
                        fpo.m_nCurrencyID = (Int32)m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD];

                        string sCustomData = Utils.GetStrSafeVal(ref selectBTQuery, "customdata", i);
                        if (!string.IsNullOrEmpty(sCustomData))
                        {
                            XmlDocument theDoc = new XmlDocument();
                            try
                            {
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
                                log.Error("CalculateSubscriptions -  groupId: " + m_nGroupID.ToString() + " exception:" + ex.Message, ex);
                            }
                        }

                        //Call get All Related media files
                        GetAllMediasChannel(nBillingTransactionID, 0, ref fpo);
                    }

                }
                selectBTQuery.Finish();
                selectBTQuery = null;
                #endregion

                //get all subscription from subscriptions_purchases (no billing transaction made for those)
                ODBCWrapper.DataSetSelectQuery selectSPQuery = new ODBCWrapper.DataSetSelectQuery();
                selectSPQuery.SetConnectionKey("CA_CONNECTION_STRING");
                selectSPQuery += "select * from subscriptions_purchases";
                selectSPQuery += "where is_active=1 and status=1 and ";
                selectSPQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectSPQuery += " and";
                selectSPQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectSPQuery += " and";
                selectSPQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectSPQuery += " and";
                selectSPQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_transaction_id", "=", 0);// this filed dosen't exist yet

                if (selectSPQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectSPQuery.Table("query").DefaultView.Count;
                    log.Debug("CalculateSubscriptions - " + nCount.ToString() + " Rows returned (subscriptions_purchases) ");


                    int nCollectionMetadata = 0;
                    for (int i = 0; i < nCount; i++)
                    {
                        FinancialPurchaseObject fpo = new FinancialPurchaseObject();
                        fpo.m_sSiteUserGUID = Utils.GetStrSafeVal(ref selectSPQuery, "site_user_guid", i);
                        fpo.m_sCountryName = Utils.GetStrSafeVal(ref selectSPQuery, "country_code", i);
                        fpo.m_nRelSub = Utils.GetIntSafeVal(ref selectSPQuery, "subscription_code", i);
                        fpo.m_dDate = Utils.GetDateSafeVal(ref selectSPQuery, "create_date", i);
                        fpo.m_eItemType = ItemType.SUBSCRIPTION;
                        fpo.m_eRelatedTo = RelatedTo.SUBSCRIPTION;
                        fpo.m_sCurrencyCD = Utils.GetStrSafeVal(ref selectSPQuery, "currency_cd", i);

                        fpo.m_dCataloguePrice = Utils.GetDoubleSafeVal(ref selectSPQuery, "price", i);
                        fpo.m_dDiscountPrice = fpo.m_dCataloguePrice;

                        //Update fpo CurrenyID
                        fpo.m_sCurrencyCD = Utils.GetStrSafeVal(ref selectSPQuery, "currency_code", i);
                        if (!m_hCurrencyCDToCurrencyID.Contains(fpo.m_sCurrencyCD))
                        {
                            string sCurrenyID = ODBCWrapper.Utils.GetTableSingleVal("lu_currency", "id", "code3", "=", fpo.m_sCurrencyCD, "MAIN_CONNECTION_STRING").ToString();
                            m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD] = int.Parse(sCurrenyID);
                        }
                        fpo.m_nCurrencyID = (Int32)m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD];


                        nCollectionMetadata = Utils.GetIntSafeVal(ref selectBTQuery, "collection_metadata", i);

                        string sCustomData = Utils.GetStrSafeVal(ref selectSPQuery, "customdata", i);

                        if (!string.IsNullOrEmpty(sCustomData))
                        {
                            XmlDocument theDoc = new XmlDocument();
                            try
                            {
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
                                log.Error("CalculateSubscriptions - groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                            }
                        }
                        //Call get All Related media files
                        GetAllMediasChannel(0, nCollectionMetadata, ref fpo);
                    }
                }

                selectSPQuery.Finish();
                selectSPQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("CalculateSubscriptions - groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }

        }

        //get all media files related to subscription
        private void GetAllMediasChannel(int nBillingTransactionID, int nCollectionMetadata, ref FinancialPurchaseObject fpo)
        {
            try
            {
                ODBCWrapper.DataSetSelectQuery selectPMtQuery = new ODBCWrapper.DataSetSelectQuery();
                selectPMtQuery.SetConnectionKey("CA_CONNECTION_STRING");
                selectPMtQuery += "select metadata from purchase_metadata where status=1 ";
                if (nBillingTransactionID != 0)
                {
                    selectPMtQuery += " and ";
                    selectPMtQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_transaction_id", "=", nBillingTransactionID);
                }
                if (nCollectionMetadata != 0)
                {
                    selectPMtQuery += " and ";
                    selectPMtQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_metadata", "=", nCollectionMetadata);
                }
                if (selectPMtQuery.Execute("query", true) != null)
                {
                    Int32 nCountPM = selectPMtQuery.Table("query").DefaultView.Count;
                    if (nCountPM > 0)
                    {
                        string metadata = Utils.GetStrSafeVal(ref selectPMtQuery, "metadata", 0);
                        XmlDocument theDoc = new XmlDocument();
                        try
                        {
                            theDoc.LoadXml(metadata);
                            XmlNode subscriptionPrice = theDoc.SelectSingleNode("collection/collectionPrice");
                            XmlNode catalogPrice = theDoc.SelectSingleNode("collection/catalogPrice");

                            double dCatPrice = 0.0;
                            double dCollPrice = 0.0;

                            string sCollPrice = string.Empty;
                            string sCatPrice = string.Empty;

                            if (subscriptionPrice != null)
                            {
                                sCollPrice = subscriptionPrice.FirstChild.Value.Replace(",", ".");
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

                            XmlNodeList owners = theDoc.SelectNodes("collection/mediaFiles/mediaFile");

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
                            //_logger.Error(ex.Message, ex);
                            log.Error("GetAllMediasChannel - groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                            CalcContract(null, fpo);
                        }
                    }
                }
                selectPMtQuery.Finish();
                selectPMtQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("GetAllMediasChannel - groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }
        }

        public virtual int GetGiftUserListForSub(int nSubCode, ref string sGiftsUsersList)
        {
            int nGifts = 0;
            try
            {
                sGiftsUsersList = "(";
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select site_guid from billing_transactions where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", 0);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "=", nSubCode);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "=", 7);
                if (selectQuery.Execute("query", true) != null)
                {
                    nGifts = selectQuery.Table("query").DefaultView.Count;

                    for (int j = 0; j < nGifts; j++)
                    {
                        if (j > 0)
                        {
                            sGiftsUsersList += ",";
                        }
                        sGiftsUsersList += Utils.GetStrSafeVal(ref selectQuery, "site_guid", j);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                sGiftsUsersList += ")";
            }
            catch (Exception ex)
            {
                log.Error("GetGiftUserListForSub - groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }
            return nGifts;
        }

        public virtual List<string> GetGiftUserListForSub(int nSubCode)
        {
            List<string> lUsersList = new List<string>();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select site_guid, customdata from billing_transactions where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", 0);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "=", nSubCode);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "=", 7);
            if (selectQuery.Execute("query", true) != null)
            {
                int nGifts = selectQuery.Table("query").DefaultView.Count;
                for (int j = 0; j < nGifts; j++)
                {
                    string sCoupon = string.Empty;
                    string sUserGuid = Utils.GetStrSafeVal(ref selectQuery, "site_guid", j);
                    try
                    {
                        XmlDocument theDoc = new XmlDocument();
                        string sCustomData = Utils.GetStrSafeVal(ref selectQuery, "customdata", j);

                        theDoc.LoadXml(sCustomData);
                        XmlNode couponNode = theDoc.SelectSingleNode("customdata/cc");
                        if (couponNode != null && couponNode.FirstChild != null)
                        {
                            sCoupon = couponNode.FirstChild.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("GetGiftUserListForSub - groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                    }

                    if (string.IsNullOrEmpty(sCoupon))
                    {
                        lUsersList.Add(sUserGuid);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }

            return lUsersList;
        }

        public virtual string GetSubCustomdata(string sUserGuid, int nSubCode)
        {
            string sCustomdata = string.Empty;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select top 1 customdata from subscriptions_purchases where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_metadata", "=", 0);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "=", nSubCode);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", sUserGuid);
                selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sCustomdata = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "customdata", 0);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("GetSubCustomdata - groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }
            return sCustomdata;
        }

        public virtual int GetCouponCodeForUserWithSub(Hashtable hSubUsesToSubPurchase, string sSiteGuid, int nSubID)
        {
            int nCouponCode = 0;
            try
            {
                if (!hSubUsesToSubPurchase.ContainsKey(sSiteGuid + "|" + nSubID))
                {
                    string sCustomData = GetSubCustomdata(sSiteGuid, nSubID);
                    if (string.IsNullOrEmpty(sCustomData))
                    {
                        hSubUsesToSubPurchase.Add(sSiteGuid + "|" + nSubID, 0);
                    }
                    else
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
                                nCouponCode = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons", "id", "code", "=", sCoupon, "pricing_connection").ToString());
                                hSubUsesToSubPurchase.Add(sSiteGuid + "|" + nSubID, nCouponCode);
                            }
                            else
                            {
                                hSubUsesToSubPurchase.Add(sSiteGuid + "|" + nSubID, 0);
                            }
                        }
                        catch
                        {
                            hSubUsesToSubPurchase.Add(sSiteGuid + "|" + nSubID, 0);
                        }
                    }
                }
                else
                {
                    nCouponCode = (int)hSubUsesToSubPurchase[sSiteGuid + "|" + nSubID];
                }
            }
            catch (Exception ex)
            {
                log.Error("GetCouponCodeForUserWithSub - groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }
            return nCouponCode;
        }

        public virtual void CalculatePPVAll()
        {
            try
            {
                //Get all relevant ppv_purchases
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select pp.ID, pp.SITE_USER_GUID, pp.CREATE_DATE, pp.COUNTRY_CODE, pp.MEDIA_FILE_ID, pp.PRICE, pp.CURRENCY_CD, pp.CUSTOMDATA, pp.rel_pp,";
                selectQuery += "bt.ID 'BT_ID', bt.ppvmodule_code, bt.BILLING_METHOD, bt.BILLING_PROVIDER, bt.SUBSCRIPTION_CODE, bt.BILLING_STATUS from ConditionalAccess.dbo.ppv_purchases pp";
                selectQuery += "left join billing_transactions bt on pp.BILLING_TRANSACTION_ID=bt.ID where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pp.create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pp.create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pp.group_id", "=", m_nGroupID);
                selectQuery += "order by pp.CREATE_DATE";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    log.Debug("CalculatePPV - " + nCount.ToString() + " rows returned");
                    for (int i = 0; i < nCount; i++)
                    {
                        FinancialPurchaseObject fpo = new FinancialPurchaseObject();

                        fpo.m_nId = Utils.GetIntSafeVal(ref selectQuery, "ID", i);

                        fpo.m_sCurrencyCD = Utils.GetStrSafeVal(ref selectQuery, "CURRENCY_CD", i);
                        if (!m_hCurrencyCDToCurrencyID.Contains(fpo.m_sCurrencyCD))
                        {
                            string sCurrenyID = ODBCWrapper.Utils.GetTableSingleVal("lu_currency", "id", "code3", "=", fpo.m_sCurrencyCD, "MAIN_CONNECTION_STRING").ToString();
                            m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD] = int.Parse(sCurrenyID);
                        }
                        fpo.m_nCurrencyID = (Int32)m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD];
                        fpo.m_dDate = Utils.GetDateSafeVal(ref selectQuery, "create_date", i);
                        fpo.m_sCountryName = Utils.GetStrSafeVal(ref selectQuery, "country_code", i);
                        fpo.m_sSiteUserGUID = Utils.GetStrSafeVal(ref selectQuery, "SITE_USER_GUID", i);
                        fpo.m_eItemType = ItemType.PPV;
                        fpo.m_eRelatedTo = RelatedTo.PPV;
                        fpo.m_nItemID = Utils.GetIntSafeVal(ref selectQuery, "media_file_id", i);
                        fpo.m_dDiscountPrice = Utils.GetDoubleSafeVal(ref selectQuery, "price", i);
                        fpo.m_dCataloguePrice = fpo.m_dDiscountPrice;

                        int nbtID = Utils.GetIntSafeVal(ref selectQuery, "bt_id", i);
                        if (nbtID > 0)
                        {
                            if (Utils.GetIntSafeVal(ref selectQuery, "BILLING_STATUS", i) != 0)
                            {
                                log.Debug("CalculatePPV - " + string.Format("BILLING_STATUS!=0 purchaseID:{0}", fpo.m_nId));
                                continue;
                            }

                            //Add BillingMethod,BillingProvider to fpo object
                            fpo.m_nBillingMethod = Utils.GetIntSafeVal(ref selectQuery, "BILLING_METHOD", i);
                            fpo.m_nBillingProvider = Utils.GetIntSafeVal(ref selectQuery, "BILLING_PROVIDER", i);
                            string sSub = Utils.GetStrSafeVal(ref selectQuery, "SUBSCRIPTION_CODE", i);
                            fpo.m_nRelSub = string.IsNullOrEmpty(sSub) ? 0 : int.Parse(sSub);
                        }

                        fpo.m_nPrePaidCode = Utils.GetIntSafeVal(ref selectQuery, "rel_pp", i);
                        if (fpo.m_nPrePaidCode > 0)
                        {
                            TvinciPricing.PrePaidModule thePrePaidModule = Utils.GetPrePaidModule(m_nGroupID, fpo.m_nPrePaidCode, string.Empty, string.Empty, string.Empty);
                            if (thePrePaidModule != null)
                            {
                                double dPPPrice = thePrePaidModule.m_PriceCode.m_oPrise.m_dPrice;
                                double dPPCredit = thePrePaidModule.m_CreditValue.m_oPrise.m_dPrice;
                                double dDiscount = 1;

                                if (dPPCredit != 0)
                                    dDiscount = dPPPrice / dPPCredit;

                                fpo.m_dDiscountPrice *= dDiscount;
                            }
                        }

                        string sCoupon = string.Empty;
                        string sCustomData = Utils.GetStrSafeVal(ref selectQuery, "customdata", i);
                        if (!string.IsNullOrEmpty(sCustomData))
                        {
                            XmlDocument theDoc = new XmlDocument();
                            try
                            {
                                theDoc.LoadXml(sCustomData);

                                //handle Coupon uses
                                XmlNode couponNode = theDoc.SelectSingleNode("customdata/cc");
                                if (couponNode != null && couponNode.FirstChild != null)
                                {
                                    sCoupon = couponNode.FirstChild.Value;
                                    fpo.m_nCouponCode = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons", "id", "code", "=", sCoupon, "pricing_connection").ToString());
                                }

                                //Get the Catalogue Price
                                XmlNode ppvmNode = theDoc.SelectSingleNode("customdata/ppvm");
                                if (ppvmNode != null && ppvmNode.FirstChild != null)
                                {
                                    int nPPVM = int.Parse(ppvmNode.FirstChild.Value);

                                    if (!m_hPPVMToPrice.Contains(nPPVM))
                                    {
                                        TvinciPricing.PPVModule thePPVModule = Utils.GetPPVModule(m_nGroupID, nPPVM, string.Empty, string.Empty, string.Empty);
                                        if (thePPVModule != null && thePPVModule.m_oPriceCode != null && thePPVModule.m_oPriceCode.m_oPrise != null)
                                        {
                                            fpo.m_dCataloguePrice = thePPVModule.m_oPriceCode.m_oPrise.m_dPrice;
                                        }
                                    }
                                    else
                                    {
                                        fpo.m_dCataloguePrice = (double)m_hPPVMToPrice[nPPVM];
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("CalculatePPV - sCustomData:" + sCustomData + " exception: " + ex.Message, ex);
                                continue;
                            }
                        }


                        if (fpo.m_nPrePaidCode == 0 && (fpo.m_nBillingMethod == 7 || fpo.m_nBillingMethod == 0))
                        {
                            if (string.IsNullOrEmpty(sCoupon))
                            {
                                continue;
                            }

                            TvinciPricing.CouponData theCoupon = Utils.GetCouponStatus(m_nGroupID, sCoupon);
                            if (theCoupon != null && theCoupon.m_oCouponGroup != null && theCoupon.m_oCouponGroup.m_oDiscountCode != null)
                            {
                                if (theCoupon.m_oCouponGroup.m_oDiscountCode.m_dPercent != 100)
                                {
                                    continue;
                                }
                            }
                        }

                        Int32 nCFID = GetContractFamilyID(fpo.m_nItemID);

                        //Get relevant Contract from Contract Family, according to media file id. 
                        BaseContract bc = m_oBaseFinancial.GetContractWithContractFamilyID(nCFID, 0, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);

                        //Process contract chain
                        CalcContract(bc, fpo);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("CalculatePPV - groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }
        }
    }
}
