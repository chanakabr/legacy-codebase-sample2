using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;
using Logger;

namespace Financial
{
    public class TvinciFinancialCalculator : TvinciFinancialCalculatorBase
    {

        //Hashtable m_hWeightIDToWeightVal;
        //Hashtable m_hMediaFileToPPVM;
        //Hashtable m_hPPVMToPrice;

        //private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TvinciFinancialCalculator(Int32 nGroupID, DateTime dStart, DateTime dEnd)
            : base(nGroupID, dStart, dEnd)
        {
        }

     
        /*
        public override void Calculate()
        {
            Logger.Logger.Log("FC", "GroupID=" + m_nGroupID + " startDate=" + m_dStartDate.ToString("yyyy-MM-dd HH:mm") + " endDate=" + m_dEndDate.ToString("yyyy-MM-dd HH:mm"), "FinancialCalculator");


            bool bClean = ClearOldRecords();
            if (!bClean)
                return;

            //Start Calculate
            CalculatePPV();
            CalculatePPVWithPrePaid();
            CalculatePPVWithFullCoupon();

            CalculateSubscriptions();

            CalculateCollections();

            //For Test only
            //string res = FDRToString();

            AddDataRowsToTable();

            //UpdateGroupedRevenues();
        }

        private void CalculatePPV()
        {
            //Get all relevant ppv_purchases
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from billing_transactions where is_active=1 and status=1 and ";
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

                Logger.Logger.Log("FC PPV", "Count=" + nCount, "FinancialCalculator");

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

                    if (string.IsNullOrEmpty(fpo.m_sCountryName))
                    {
                        fpo.m_sCountryName = "Netherlands";
                    }

                    fpo.m_sSiteUserGUID = Utils.GetStrSafeVal(ref selectQuery, "site_guid", i);

                    fpo.m_eItemType = ItemType.PPV;
                    fpo.m_eRelatedTo = RelatedTo.PPV;

                    fpo.m_nItemID = Utils.GetIntSafeVal(ref selectQuery, "media_file_id", i);

                    fpo.m_dDiscountPrice = Utils.GetDoubleSafeVal(ref selectQuery, "price", i);

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
                        catch
                        {

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

        private void CalculatePPVWithPrePaid()
        {
            //Get all relevant ppv_purchases
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from ppv_purchases where is_active=1 and status=1 and ";
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

                Logger.Logger.Log("FC PPV Pre Paid", "Count=" + nCount, "FinancialCalculator");

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
                        catch
                        {

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

        private void CalculatePPVWithFullCoupon()
        {
            //Get all relevant ppv_purchases
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from ppv_purchases where is_active=1 and status=1 and ";
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

                Logger.Logger.Log("FC PPV Full Coupon", "Count=" + nCount, "FinancialCalculator");

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
                        catch
                        {
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

        private void CalculateCollections()
        {
            //Get all relevant Subscriptions
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select *  from subscriptions_purchases where is_active=1 and status=1 and ";
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

                Logger.Logger.Log("FC Collections", "Count=" + nCount, "FinancialCalculator");

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
                        catch
                        {

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
                                //send log message
                                Logger.Logger.Log("FC Collections", "Message=" + ex.Message, "FinancialCalculator");
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

        //////////////// Old CalculateSubscriptions
        //private void CalculateSubscriptions()
        //{

        //    //GetCollectionList
        //    string sColList = "(";
        //    Int32 nNumOfCollections = 0;

        //    //Get all relevant Subscriptions
        //    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //    selectQuery += "select distinct subscription_code from subscriptions_purchases where is_active=1 and status=1 and ";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
        //    selectQuery += " and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
        //    selectQuery += " and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_metadata", ">", 0);
        //    selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
        //    if (selectQuery.Execute("query", true) != null)
        //    {
        //        nNumOfCollections = selectQuery.Table("query").DefaultView.Count;

        //        for (int i = 0; i < nNumOfCollections; i++)
        //        {
        //            if (i > 0)
        //                sColList += ",";

        //            sColList += Utils.GetStrSafeVal(ref selectQuery, "subscription_code", i);
        //        }
        //    }
        //    selectQuery.Finish();
        //    selectQuery = null;

        //    sColList += ")";


        //    selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //    selectQuery += "select distinct subscription_code from billing_transactions where is_active=1 and status=1 and ";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
        //    selectQuery += " and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
        //    selectQuery += " and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", 0);
        //    selectQuery += " and subscription_code is not null and ";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "<>", string.Empty);
        //    if (nNumOfCollections > 0)
        //    {
        //        selectQuery += "and subscription_code not in " + sColList;
        //    }
        //    if (selectQuery.Execute("query", true) != null)
        //    {
        //        Int32 nCount = selectQuery.Table("query").DefaultView.Count;

        //        for (int i = 0; i < nCount; i++)
        //        {

        //            Int32 nSubCode = Utils.GetIntSafeVal(ref selectQuery, "subscription_code", i);

        //            if (nSubCode == 0)
        //                continue;

        //            Int32 nDM = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "DISCOUNT_MODULE_CODE", nSubCode, "pricing_connection").ToString());
        //            double dDiscountPercent = double.Parse(ODBCWrapper.Utils.GetTableSingleVal("discount_codes", "discount_percent", nDM, "pricing_connection").ToString());

        //            if (dDiscountPercent < 100.0)
        //                continue;

        //            Int32 nGifts = 0;
        //            Int32 nPurchases = 0;

        //            string sGiftsUsersList = "(";

        //            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
        //            selectQuery1 += "select site_guid from billing_transactions where is_active=1 and status=1 and ";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
        //            selectQuery1 += "and";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
        //            selectQuery1 += " and";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
        //            selectQuery1 += " and";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", 0);
        //            selectQuery1 += " and";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "=", nSubCode);
        //            selectQuery1 += " and";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "=", 7);
        //            if (selectQuery1.Execute("query", true) != null)
        //            {
        //                nGifts = selectQuery1.Table("query").DefaultView.Count;

        //                for (int j = 0; j < nGifts; j++)
        //                {
        //                    if (j > 0)
        //                    {
        //                        sGiftsUsersList += ",";
        //                    }
        //                    sGiftsUsersList += Utils.GetStrSafeVal(ref selectQuery1, "site_guid", j);
        //                }
        //            }
        //            selectQuery1.Finish();
        //            selectQuery1 = null;

        //            sGiftsUsersList += ")";


        //            //Get all relevant Subscriptions
        //            selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
        //            selectQuery1 += "select count(subscription_code) as csc, sum(price) as total, currency_code from billing_transactions where is_active=1 and status=1 ";
        //            selectQuery1 += "and";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
        //            selectQuery1 += "and";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
        //            selectQuery1 += "and";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
        //            selectQuery1 += "and";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "=", nSubCode);
        //            selectQuery1 += "and";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "<>", 7);
        //            selectQuery1 += "and";
        //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("billing_status", "=", 0);
        //            selectQuery1 += "group by subscription_code, currency_code order by subscription_code";
        //            if (selectQuery1.Execute("query", true) != null)
        //            {
        //                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
        //                Hashtable hMediaToWeight = new Hashtable();
        //                if (nCount1 > 0)
        //                {
        //                    FinancialPurchaseObject fpo = new FinancialPurchaseObject();

        //                    nPurchases = Utils.GetIntSafeVal(ref selectQuery1, "csc", 0);

        //                    fpo.m_sCurrencyCD = Utils.GetStrSafeVal(ref selectQuery1, "currency_code", 0);
        //                    if (!m_hCurrencyCDToCurrencyID.Contains(fpo.m_sCurrencyCD))
        //                    {
        //                        string sCurrenyID = ODBCWrapper.Utils.GetTableSingleVal("lu_currency", "id", "code3", "=", fpo.m_sCurrencyCD, "MAIN_CONNECTION_STRING").ToString();
        //                        m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD] = int.Parse(sCurrenyID);
        //                    }
        //                    fpo.m_nCurrencyID = (Int32)m_hCurrencyCDToCurrencyID[fpo.m_sCurrencyCD];

        //                    fpo.m_dDate = m_dStartDate;

        //                    fpo.m_eItemType = ItemType.SUBSCRIPTION;
        //                    fpo.m_eRelatedTo = RelatedTo.SUBSCRIPTION;

        //                    fpo.m_nRelSub = nSubCode;

        //                    fpo.m_dDiscountPrice = Utils.GetDoubleSafeVal(ref selectQuery1, "total", 0);
        //                    fpo.m_dCataloguePrice = fpo.m_dDiscountPrice;

        //                    //Get all played media files for subscription
        //                    ODBCWrapper.DataSetSelectQuery selectQuery2 = new ODBCWrapper.DataSetSelectQuery();
        //                    selectQuery2 += "select * from subscriptions_uses where is_active=1 and status=1 and ";
        //                    selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
        //                    selectQuery2 += "and";
        //                    selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
        //                    selectQuery2 += "and";
        //                    selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
        //                    selectQuery2 += "and";
        //                    selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "=", nSubCode);
        //                    selectQuery2 += "and";
        //                    selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("is_credit_downloaded", "=", 1);
        //                    if (nGifts > 0)
        //                    {
        //                        selectQuery2 += "and site_user_guid not in " + sGiftsUsersList;
        //                    }
        //                    selectQuery2 += "order by create_date";
        //                    selectQuery2.SetConnectionKey("CA_CONNECTION_STRING");
        //                    if (selectQuery2.Execute("query", true) != null)
        //                    {
        //                        Int32 nCount2 = selectQuery2.Table("query").DefaultView.Count;
        //                        FinancialMedia[] fms = new FinancialMedia[nCount2];

        //                        double dTotalAdjustedPlays = 0;

        //                        for (int x = 0; x < nCount2; x++)
        //                        {

        //                            fms[x] = new FinancialMedia();

        //                            fms[x].m_nMediaFileID = Utils.GetIntSafeVal(ref selectQuery2, "media_file_id", x);
        //                            fms[x].m_nMediaFileOwner = GetContractFamilyID(fms[x].m_nMediaFileID);

        //                            fms[x].m_sCountryName = Utils.GetStrSafeVal(ref selectQuery2, "country_code", x);
        //                            fms[x].m_sSiteUserGUID = Utils.GetStrSafeVal(ref selectQuery2, "site_user_guid", x);

        //                            fms[x].m_dDate = m_dStartDate;
        //                            fms[x].m_dDate = Utils.GetDateSafeVal(ref selectQuery2, "create_date", x);

        //                            Int32 nFWID = 0;

        //                            if (hMediaToWeight.Contains(fms[x].m_nMediaFileID) == false)
        //                            {
        //                                Object oWeight = ODBCWrapper.Utils.GetTableSingleVal("media_files", "financial_weight_id", fms[x].m_nMediaFileID, "MAIN_CONNECTION_STRING");

        //                                if (oWeight != null && oWeight != DBNull.Value)
        //                                {
        //                                    nFWID = int.Parse(oWeight.ToString());
        //                                }

        //                                hMediaToWeight.Add(fms[x].m_nMediaFileID, nFWID);
        //                            }
        //                            else
        //                            {
        //                                nFWID = (Int32)hMediaToWeight[fms[x].m_nMediaFileID];
        //                            }

        //                            if (!m_hWeightIDToWeightVal.Contains(nFWID))
        //                            {
        //                                m_hWeightIDToWeightVal[nFWID] = 1;
        //                                Object oWeight = ODBCWrapper.Utils.GetTableSingleVal("fr_financial_weights", "weight", nFWID, "MAIN_CONNECTION_STRING");

        //                                if (oWeight != null && oWeight != DBNull.Value)
        //                                {
        //                                    m_hWeightIDToWeightVal[nFWID] = double.Parse(oWeight.ToString());
        //                                }
        //                            }

        //                            fms[x].m_dMediaFileWeight = (double)m_hWeightIDToWeightVal[nFWID];

        //                            dTotalAdjustedPlays += fms[x].m_dMediaFileWeight;
        //                        }

        //                        CalcSubscription(fms, fpo, dTotalAdjustedPlays, fpo.m_dDiscountPrice);
        //                    }

        //                    selectQuery2.Finish();
        //                    selectQuery2 = null;

        //                }
        //            }

        //            selectQuery1.Finish();
        //            selectQuery1 = null;
        //        }
        //    }
        //    selectQuery.Finish();
        //    selectQuery = null;
        //}


        private void CalcSubscription(FinancialMedia[] fms, FinancialPurchaseObject fpo, double dTotalAdjustedPlays, double dTotalSubscriptionRevenues)
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



        /// <summary>
        /// New CalculateSubscriptions :
        /// with 100% coupons discounts
        /// </summary>
       

        private int GetGiftUserListForSub(int nSubCode, ref string sGiftsUsersList)
        {
            int nGifts = 0;
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

            return nGifts;
        }

        private string GetSubCustomdata(string sUserGuid, int nSubCode)
        {

            string sCustomdata = string.Empty;

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

            return sCustomdata;
        }

        private int GetCouponCodeForUserWithSub(Hashtable hSubUsesToSubPurchase, string sSiteGuid, int nSubID)
        {
            int nCouponCode = 0;

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

            return nCouponCode;
        }
        */

        /// New CalculateSubscriptions :
        /// with 100% coupons discounts
        public override void CalculateSubscriptions()
        {
            try
            {
                Hashtable hSubAmount = new Hashtable();
                Hashtable hSubWithCoupon = new Hashtable();
                Hashtable hSubUsesToSubPurchase = new Hashtable();
                Hashtable hMediaToWeight = new Hashtable();

                int nCount = 0;      

                #region Get List of collections
                List<string> lCollections = new List<string>();
                Int32 nNumOfCollections = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select distinct subscription_code from subscriptions_purchases (nolock) where is_active=1 and status=1 and ";
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
                    nNumOfCollections = selectQuery.Table("query").DefaultView.Count;

                    for (int i = 0; i < nNumOfCollections; i++)
                    {
                        lCollections.Add(Utils.GetStrSafeVal(ref selectQuery, "subscription_code", i));
                    }
                }
                selectQuery.Finish();
                selectQuery = null;                
                #endregion

                #region  Get all Subscriptions with 100% discount
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select distinct subscription_code from subscriptions_purchases (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_metadata", "=", 0);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("price", "=", 0);
                selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        int nSubID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "subscription_code", i);
                        if (!hSubWithCoupon.Contains(nSubID))
                        {
                            hSubWithCoupon.Add(nSubID, nSubID);
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                #endregion

                #region Get all income for each subscription 
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select subscription_code, sum(price) as total, currency_code from billing_transactions (nolock) where is_active=1 and status=1 ";
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", 0);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "<>", string.Empty);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "<>", 7);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_status", "=", 0);
                if (nNumOfCollections > 0)
                {
                    selectQuery += "and SUBSCRIPTION_CODE not in (" + string.Join(",", lCollections.ToArray()) +")";
                }
                selectQuery += "group by subscription_code, currency_code";
                if (selectQuery.Execute("query", true) != null)
                {
                    nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        int nSubID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "subscription_code", i);
                        double dAmount = ODBCWrapper.Utils.GetDoubleSafeVal(selectQuery, "total", i);
                        if (!hSubAmount.Contains(nSubID))
                        {
                            hSubAmount.Add(nSubID, dAmount);
                        }
                        else
                        {
                            double dSum = (double)hSubAmount[nSubID];
                            hSubAmount[nSubID] = dSum + dAmount;
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                #endregion

                #region Add Subscriptions with coupon to total hashtable
                foreach (DictionaryEntry de in hSubWithCoupon)
                {
                    int nSubID = (int)de.Key;

                    if (!hSubAmount.Contains(nSubID))
                    {
                        hSubAmount.Add(nSubID, 0.0);
                    }
                }
                #endregion

                #region Calculate Subscription revenues
                foreach (DictionaryEntry de in hSubAmount)
                {
                    FinancialPurchaseObject fpo = new FinancialPurchaseObject();

                    fpo.m_nRelSub = (int)de.Key;
                    fpo.m_dDiscountPrice = (double)de.Value;
                    fpo.m_dCataloguePrice = fpo.m_dDiscountPrice;

                    //Filmo only
                    fpo.m_sCurrencyCD = "EUR";
                    fpo.m_nCurrencyID = 4;

                    fpo.m_dDate = m_dStartDate;

                    fpo.m_eItemType = ItemType.SUBSCRIPTION;
                    fpo.m_eRelatedTo = RelatedTo.SUBSCRIPTION;

                    //Get all users who got the sub as gift 
                    List<string> lGiftUsersList = GetGiftUserListForSub(fpo.m_nRelSub);

                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select * from subscriptions_uses (nolock) where is_active=1 and status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "=", fpo.m_nRelSub);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_credit_downloaded", "=", 1);
                    if (lGiftUsersList.Count > 0)
                    {
                        selectQuery += "and site_user_guid not in (" + string.Join(",", lGiftUsersList.ToArray()) +")";
                    }
                    selectQuery += "order by create_date";
                    selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                    if (selectQuery.Execute("query", true) != null)
                    {
                        nCount = selectQuery.Table("query").DefaultView.Count;
                        FinancialMedia[] fms = new FinancialMedia[nCount];

                        double dTotalAdjustedPlays = 0;

                        for (int x = 0; x < nCount; x++)
                        {
                            fms[x] = new FinancialMedia();
                            fms[x].m_nMediaFileID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "media_file_id", x);
                            fms[x].m_nMediaFileOwner = GetContractFamilyID(fms[x].m_nMediaFileID);
                            fms[x].m_sCountryName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "country_code", x);
                            fms[x].m_sSiteUserGUID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "site_user_guid", x);
                            if (hSubWithCoupon.Contains(fpo.m_nRelSub))
                            {
                                fms[x].m_nCouponCode = GetCouponCodeForUserWithSub(hSubUsesToSubPurchase, fms[x].m_sSiteUserGUID, fpo.m_nRelSub);
                            }
                            fms[x].m_dDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "create_date", x);

                            Int32 nFWID = 0;
                            if (hMediaToWeight.Contains(fms[x].m_nMediaFileID) == false)
                            {
                                Object oWeight = ODBCWrapper.Utils.GetTableSingleVal("media_files", "financial_weight_id", fms[x].m_nMediaFileID, "MAIN_CONNECTION_STRING");
                                if (oWeight != null && oWeight != DBNull.Value)
                                {
                                    nFWID = int.Parse(oWeight.ToString());
                                }
                                hMediaToWeight.Add(fms[x].m_nMediaFileID, nFWID);
                            }
                            else
                            {
                                nFWID = (Int32)hMediaToWeight[fms[x].m_nMediaFileID];
                            }
                           
                            if (!m_hWeightIDToWeightVal.Contains(nFWID))
                            {
                                m_hWeightIDToWeightVal[nFWID] = 1;
                                Object oWeight = ODBCWrapper.Utils.GetTableSingleVal("fr_financial_weights", "weight", nFWID, "MAIN_CONNECTION_STRING");
                                if (oWeight != null && oWeight != DBNull.Value)
                                {
                                    m_hWeightIDToWeightVal[nFWID] = double.Parse(oWeight.ToString());
                                }
                            }

                            fms[x].m_dMediaFileWeight = (double)m_hWeightIDToWeightVal[nFWID];
                            dTotalAdjustedPlays += fms[x].m_dMediaFileWeight;
                        }
                        CalcSubscription(fms, fpo, dTotalAdjustedPlays, fpo.m_dDiscountPrice);
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                }
                #endregion
            }

            catch (Exception ex)
            {
                Logger.Logger.Log("CalculateSubscription"," groupId: " +m_nGroupID.ToString()+ " exception: " + ex.Message, "Calculate");   
            }
        }
    }
}
