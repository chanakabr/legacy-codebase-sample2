using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using TVinciShared;
using System.Xml;
using KLogMonitor;
using System.Reflection;

namespace Financial
{

    public class FinancialPurchaseObject
    {
        public Int32 m_nId;
        public double m_dCataloguePrice;
        public double m_dDiscountPrice;
        public double m_dTaxPrice;
        public double m_dProcessorPrice;
        public double m_dProcessorAndTax;

        public Int32 m_nCurrencyID;
        public string m_sCurrencyCD;
        public string m_sCountryName;
        public DateTime m_dDate;

        public string m_sSiteUserGUID;
        public Int32 m_nItemID;
        public Int32 m_nRelSub;
        public RelatedTo m_eRelatedTo;
        public ItemType m_eItemType;

        public Int32 m_nPrePaidCode;
        public Int32 m_nCouponCode;

        public Int32 m_nBillingMethod;
        public Int32 m_nBillingProvider;
        public Int32 m_nPaymentNumber;


        public FinancialPurchaseObject()
        {
            m_nId = 0;
            m_dCataloguePrice = 0.0;
            m_dDiscountPrice = 0.0;
            m_dTaxPrice = 0.0;
            m_dProcessorPrice = 0.0;
            m_dProcessorAndTax = 0.0;

            m_nCurrencyID = 0;
            m_sCurrencyCD = string.Empty;
            m_sCountryName = string.Empty;
            m_dDate = new DateTime();

            m_sSiteUserGUID = string.Empty;
            m_nItemID = 0;
            m_nRelSub = 0;
            m_eItemType = ItemType.PPV;
            m_eRelatedTo = RelatedTo.PPV;

            m_nPrePaidCode = 0;
            m_nCouponCode = 0;

            m_nBillingMethod = 0;
            m_nBillingProvider = 0;

            m_nPaymentNumber = 0;
        }
    }

    public class CalcObject
    {
        public double m_dInPrice;
        public double m_dRevenuePrice;
        public double m_dOutPrice;

        public CalcObject()
        {
            m_dInPrice = 0.0;
            m_dRevenuePrice = 0.0;
            m_dOutPrice = 0.0;
        }
    }

    public class FinancialDataRow
    {
        public Int32 nEntityID;
        public Int32 nItemID;
        public string sSiteUserGUID;
        public Int32 nType;

        public double nAmount;

        public string sCurrencyCD;
        public string sCountryName;
        public DateTime dDate;

        public Int32 nRelSub;
        public Int32 nContractID;

        public double nCatPrice;
        public double nActPrice;

        public Int32 nPrePaidID;
        public Int32 nCouponID;

        public Int32 nPaymentNumber;

        public FinancialDataRow()
        {
            nEntityID = 0;
            nItemID = 0;
            sSiteUserGUID = string.Empty;
            nType = 0;

            nAmount = 0.0;

            sCurrencyCD = string.Empty;
            sCountryName = string.Empty;
            dDate = new DateTime();

            nRelSub = 0;
            nContractID = 0;

            nCatPrice = 0;
            nActPrice = 0;

            nPrePaidID = 0;
            nCouponID = 0;

            nPaymentNumber = 0;
        }
    }

    public class FinancialMedia
    {
        public Int32 m_nMediaFileID;
        public Int32 m_nMediaFileCounter;
        public Int32 m_nMediaFileOwner;

        public double m_dMediaFileWeight;

        public DateTime m_dDate;

        public string m_sSiteUserGUID;
        public string m_sCountryName;

        public int m_nCouponCode;

        public FinancialMedia()
        {
            m_nMediaFileID = 0;
            m_nMediaFileCounter = 0;

            m_nMediaFileOwner = 0;
            m_dMediaFileWeight = 0.0;

            m_dDate = new DateTime();

            m_sSiteUserGUID = string.Empty;
            m_sCountryName = string.Empty;

            m_nCouponCode = 0;
        }
    }

    public abstract class FinancialCalculator
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected Hashtable m_hContractsFamilies;
        protected Hashtable m_hTaxes;
        protected Hashtable m_hProcessors;

        protected BaseFinancial m_oBaseFinancial;

        protected Int32 m_nGroupID;

        protected DateTime m_dStartDate;
        protected DateTime m_dEndDate;

        protected Hashtable m_hLevelToContracts;
        protected Hashtable m_hMediaToFamily;
        protected Hashtable m_hCurrencyCDToCurrencyID;

        protected List<FinancialDataRow> m_LDataRows;

        public FinancialCalculator(Int32 nGroupID, DateTime dStart, DateTime dEnd)
        {
            m_nGroupID = nGroupID;

            m_dStartDate = dStart;
            m_dEndDate = dEnd;

            m_oBaseFinancial = new BaseFinancial(nGroupID);
            m_oBaseFinancial.Initialize();

            m_hContractsFamilies = new Hashtable();
            m_hTaxes = new Hashtable(); ;
            m_hProcessors = new Hashtable();

            m_hLevelToContracts = new Hashtable();
            m_hMediaToFamily = new Hashtable();
            m_hCurrencyCDToCurrencyID = new Hashtable();

            m_LDataRows = new List<FinancialDataRow>();
        }



        public bool ClearOldRecords()
        {
            //Clean DB from relevants dates
            bool bOK = true;

            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
            directQuery += "delete from fr_financial_entity_revenues where is_active=1 and status=1 and ";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
            directQuery += "and";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
            directQuery += "and";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            bOK = directQuery.Execute();
            directQuery.Finish();
            directQuery = null;

            directQuery = new ODBCWrapper.DirectQuery();
            directQuery += "delete from fr_financial_entity_revenues_grouped where is_active=1 and status=1 and ";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
            directQuery += "and";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
            directQuery += "and";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            bOK = directQuery.Execute();
            directQuery.Finish();
            directQuery = null;

            return bOK;
        }

        public abstract void Calculate();

        protected void CalcContract(BaseContract contract, FinancialPurchaseObject fpo)
        {

            if (contract == null)
            {
                //No Contract !!!
                log.Debug("CalcContract - No Contract: " + FpoToStr(fpo));
                AddPriceToContainer(-1, fpo.m_dDiscountPrice, fpo, -1);
                return;
            }

            log.Debug("CalcContract - " + string.Format("Contract:{0}, Date:{1}, Price:{2}, Type:{3}", contract.m_nContractID, fpo.m_dDate, fpo.m_dDiscountPrice, fpo.m_eItemType.ToString()));

            TvinciPricing.CouponsGroup theCoupon = null;
            if (fpo.m_nCouponCode > 0)
            {

                Int32 nCouponGroupID = 0;
                object oCoupon = ODBCWrapper.Utils.GetTableSingleVal("coupons", "COUPON_GROUP_ID", fpo.m_nCouponCode, "pricing_connection");
                if (oCoupon != null && oCoupon != DBNull.Value)
                {
                    nCouponGroupID = int.Parse(oCoupon.ToString());
                }

                theCoupon = Utils.GetCouponGroup(m_nGroupID, nCouponGroupID);
            }

            //Calculate Tax Revenue
            CalcObject tax = new CalcObject();
            BaseContract tbc = m_oBaseFinancial.GetValidTaxCotract(fpo.m_dDiscountPrice, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);
            if (tbc != null)
            {
                double dInPrice = fpo.m_dDiscountPrice;

                if (theCoupon != null && theCoupon.m_nFinancialEntityID > 1) //(1=CP , when the CP get the full price anyway !)
                {
                    dInPrice = fpo.m_dCataloguePrice;
                }

                tax.m_dInPrice = dInPrice;
                tax.m_dRevenuePrice = tbc.Calculate(dInPrice);
                tax.m_dOutPrice = tax.m_dInPrice - tax.m_dRevenuePrice;

                AddPriceToContainer(tbc.m_nFinancialEntityID, tax.m_dRevenuePrice, fpo, tbc.m_nContractID);
            }

            #region New Calculate Processor Revenue
            CalcObject processor = new CalcObject();
            processor.m_dInPrice = fpo.m_dDiscountPrice; //total amount for this transaction for all processors

            //Get all relavent contract for processor - can be more then one contract match
            List<ProcessorContract> pcList = m_oBaseFinancial.GetValidProcessorCotract(fpo.m_dDiscountPrice, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo, fpo.m_nBillingMethod, fpo.m_nBillingProvider);
            bool bContractRangeIsValid = false;
            foreach (ProcessorContract pc in pcList)
            {
                CalcObject tempProcessor = new CalcObject();
                if (pc.m_cContractRange != null)
                {
                    if (pc.m_cContractRange.m_eStartCountSince == StartCountSince.Month) // Count or Sum in Monthly Frequency
                    {
                        if (pc.m_cContractRange.m_nValueRangeType == ValueRangeType.CountTransaction)
                        {
                            //count number of transaction until this one
                            int countTransaction = CountTransaction(fpo, pc.m_nPaymentMethodEntityID, pc.m_cContractRange.m_eStartCountSince);
                            //check if range is match 
                            if (countTransaction > pc.m_cContractRange.m_nValueRangeMax || countTransaction < pc.m_cContractRange.m_nValueRangeMin)
                            {
                                continue;
                            }
                            bContractRangeIsValid = true;
                        }
                    }
                }
                else
                {
                    bContractRangeIsValid = true;
                }

                if (bContractRangeIsValid)
                {
                    tempProcessor.m_dInPrice = fpo.m_dDiscountPrice;
                    tempProcessor.m_dRevenuePrice = pc.Calculate(fpo.m_dDiscountPrice);
                    tempProcessor.m_dOutPrice = tempProcessor.m_dInPrice - tempProcessor.m_dRevenuePrice;

                    processor.m_dRevenuePrice += tempProcessor.m_dRevenuePrice; // sum total amount for Revenue Price per contract processors

                    log.Debug("FinancialCalculator - CalcContract  CalcContract.Processor Contract: " + CoToStr(tempProcessor, pc));

                    AddPriceToContainer(pc.m_nFinancialEntityID, tempProcessor.m_dRevenuePrice, fpo, pc.m_nContractID);
                }
            }
            processor.m_dOutPrice = processor.m_dInPrice - processor.m_dRevenuePrice; // calculate total out price amount for all processors

            #endregion

            //CalcObject - holds Tax-And-Processor
            CalcObject taxAndprocessor = new CalcObject();
            taxAndprocessor.m_dInPrice = fpo.m_dDiscountPrice;
            taxAndprocessor.m_dRevenuePrice = processor.m_dRevenuePrice + tax.m_dRevenuePrice;
            taxAndprocessor.m_dOutPrice = taxAndprocessor.m_dInPrice - taxAndprocessor.m_dRevenuePrice;

            //Update FinancialPurchaseObject vals 
            fpo.m_dTaxPrice = tax.m_dOutPrice;
            fpo.m_dProcessorPrice = processor.m_dOutPrice;
            fpo.m_dProcessorAndTax = taxAndprocessor.m_dOutPrice;

            //Calculate revenues for contract
            switch (contract.m_eCalculatedOn)
            {
                case CalculatedOn.OnLevel:
                    {
                        //Calculate revenue on "On Level"
                        CalcOnLevel(contract, fpo, theCoupon);
                        break;
                    }
                default:
                    {
                        CalcObject coContract = new CalcObject();

                        //Get The InPrice For the base Contract
                        double dPrice = GetInPrice(contract, fpo);
                        //Calculate Contract
                        double dCalRev = contract.Calculate(dPrice);

                        AddPriceToContainer(contract.m_nFinancialEntityID, dCalRev, fpo, contract.m_nContractID);

                        coContract.m_dInPrice = dPrice;
                        coContract.m_dRevenuePrice = dCalRev;
                        coContract.m_dOutPrice = dPrice - dCalRev;

                        //_logger.Info(string.Format("{0} : {1}","Contract", CoToStr(coContract, contract)));
                        log.Debug("FinancialCalculator - CalcContract -  Contract: " + CoToStr(coContract, contract));
                        AddPriceToContainer(0, fpo.m_dDiscountPrice - (taxAndprocessor.m_dRevenuePrice + coContract.m_dRevenuePrice), fpo, 0);
                        log.Debug("FinancialCalculator - CalcContract - GroupId: " + m_nGroupID.ToString() + " Contract " + " amount:" + coContract.m_dOutPrice.ToString());
                        break;
                    }
            }
        }

        protected void AddPriceToContainer(Int32 nEntityID, double dVal, FinancialPurchaseObject fpo, Int32 nContractID)
        {

            FinancialDataRow fdr = new FinancialDataRow();
            fdr.nEntityID = nEntityID;
            fdr.nItemID = fpo.m_nItemID;
            fdr.sSiteUserGUID = fpo.m_sSiteUserGUID;
            fdr.nType = (Int32)fpo.m_eItemType;
            fdr.nAmount = dVal;
            fdr.sCurrencyCD = fpo.m_sCurrencyCD;
            fdr.dDate = fpo.m_dDate;
            fdr.sCountryName = fpo.m_sCountryName;
            fdr.nRelSub = fpo.m_nRelSub;
            fdr.nContractID = nContractID;

            fdr.nCatPrice = fpo.m_dCataloguePrice;
            fdr.nActPrice = fpo.m_dDiscountPrice;

            fdr.nCouponID = fpo.m_nCouponCode;
            fdr.nPrePaidID = fpo.m_nPrePaidCode;
            fdr.nPaymentNumber = fpo.m_nPaymentNumber;

            m_LDataRows.Add(fdr);
        }

        protected Int32 GetFDRIndex(Int32 nEntityID, FinancialPurchaseObject fpo, Int32 nContractID)
        {

            Int32 nCount = m_LDataRows.Count;

            for (int i = nCount - 1; i > -1; i--)
            {
                FinancialDataRow fdr = m_LDataRows[i];

                if (fdr.nEntityID == nEntityID && fdr.nItemID == fpo.m_nItemID && fdr.sSiteUserGUID.Equals(fpo.m_sSiteUserGUID)
                    && fdr.nType == (Int32)fpo.m_eItemType && fdr.dDate.Equals(fpo.m_dDate) && fdr.nContractID == nContractID
                    && fdr.nCouponID == fpo.m_nCouponCode)
                {
                    return i;
                }
            }

            return -1;
        }

        protected void CalcOnLevel(BaseContract contract, FinancialPurchaseObject fpo)
        {
            CalcOnLevel(contract, fpo, null);
        }

        protected void CalcOnLevel(BaseContract contract, FinancialPurchaseObject fpo, TvinciPricing.CouponsGroup theCoupon)
        {
            List<int> contracts = new List<int>();
            contracts.Add(contract.m_nContractID);
            List<KeyValuePair<Int32, List<ContentOwnerContract>>> Levels = new List<KeyValuePair<Int32, List<ContentOwnerContract>>>();

            Int32 level = contract.m_nCalculatedOnLevel;
            CalculatedOn eCalOn = contract.m_eCalculatedOn;

            while (eCalOn == CalculatedOn.OnLevel)
            {
                List<ContentOwnerContract> list = m_oBaseFinancial.GetValidCotractsForLevel(level, 0.0, fpo.m_nCurrencyID, fpo.m_sCountryName, fpo.m_dDate, fpo.m_eRelatedTo);
                Levels.Add(new KeyValuePair<int, List<ContentOwnerContract>>(level, list));

                eCalOn = CalculatedOn.FinalAfterTaxAndProcessing;

                if (list != null && list.Count > 0)
                {
                    List<int> listIds = list.Select(x => x.m_nContractID).ToList<int>();
                    int matches = contracts.Intersect(listIds).ToList().Count;
                    if (matches > 0)
                    {
                        log.Debug("Circle - " + string.Format("contract:{0}, fpo:{1}", contract.m_nContractID, FpoToStr(fpo)));
                        return;
                    }

                    contracts.AddRange(listIds);

                    level = list[0].m_nCalculatedOnLevel;
                    eCalOn = list[0].m_eCalculatedOn;
                }
                else
                {
                    return;
                }
            }

            if (Levels.Count == 0)
            {
                return;
            }

            //One CalcObject for each level.
            CalcObject[] coForLevel = new CalcObject[Levels.Count];

            for (int i = 0; i < Levels.Count; i++)
            {
                coForLevel[i] = new CalcObject();
                KeyValuePair<Int32, List<ContentOwnerContract>> kpv = Levels[Levels.Count - 1 - i];

                if (i == 0)
                {
                    ContentOwnerContract bc = kpv.Value[0];

                    coForLevel[i].m_dInPrice = GetInPrice(bc, fpo);
                }
                else
                    coForLevel[i].m_dInPrice = coForLevel[i - 1].m_dOutPrice;

                foreach (ContentOwnerContract bc in kpv.Value)
                {
                    //Calculate revenue for contract, and add to CalcObject of the level.
                    double dSum = bc.Calculate(coForLevel[i].m_dInPrice);
                    AddPriceToContainer(bc.m_nFinancialEntityID, dSum, fpo, bc.m_nContractID);
                    coForLevel[i].m_dRevenuePrice += dSum;
                }

                //Update the left amount after level revenue.
                coForLevel[i].m_dOutPrice = coForLevel[i].m_dInPrice - coForLevel[i].m_dRevenuePrice;

            }

            //Get The InPrice For the base Contract
            double dPrice = coForLevel[Levels.Count - 1].m_dOutPrice;

            //Calculate Base Contract
            double dCalRev = contract.Calculate(dPrice);

            //Calculate revenues for account
            double dAccountRev = dPrice - dCalRev;

            //Coupon Handling
            if (theCoupon != null && theCoupon.m_nFinancialEntityID == (Int32)RightHolderType.RightHolder)
            {
                //Case Discount On Right Holder
                double diff = fpo.m_dCataloguePrice - fpo.m_dDiscountPrice;

                if (m_oBaseFinancial.IsContractOwnerIsRightHolder((ContentOwnerContract)contract))
                {
                    //Right Holder
                    dCalRev = dCalRev - diff;
                }
                else
                {
                    //fictivic Right Holder
                    double dTotalLevelPercentage = 0;

                    //Get all real rights holders 
                    List<ContentOwnerContract> lRRH = Levels[0].Value;

                    //Calculate Total Percentage for level
                    foreach (ContentOwnerContract coc in lRRH)
                    {
                        dTotalLevelPercentage += coc.m_nPercentageAmount;
                    }

                    foreach (ContentOwnerContract coc in lRRH)
                    {
                        //Update the real revenue for the right holder
                        Int32 index = GetFDRIndex(coc.m_nFinancialEntityID, fpo, coc.m_nContractID);
                        if (index != -1)
                        {
                            m_LDataRows[index].nAmount = m_LDataRows[index].nAmount - (diff * coc.m_nPercentageAmount / dTotalLevelPercentage);
                        }
                    }
                }
            }
            else if (theCoupon != null && theCoupon.m_nFinancialEntityID == (Int32)RightHolderType.Account)
            {
                //Case Account
                dAccountRev = dAccountRev - (fpo.m_dCataloguePrice - fpo.m_dDiscountPrice);
            }

            //Add Base Contract and Account revenues 
            AddPriceToContainer(contract.m_nFinancialEntityID, dCalRev, fpo, contract.m_nContractID);
            AddPriceToContainer(0, dAccountRev, fpo, 0);
        }

        protected Int32 GetContractFamilyID(Int32 nMediaFileID)
        {

            if (m_hMediaToFamily[nMediaFileID] != null)
            {
                return (Int32)m_hMediaToFamily[nMediaFileID];
            }

            Int32 nCFID = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select contract_family_id from fr_media_files_contract_families where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", nMediaFileID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCFID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["contract_family_id"].ToString());
                    m_hMediaToFamily[nMediaFileID] = nCFID;
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return nCFID;

        }

        protected void AddDataRowsToTable()
        {
            string query = string.Empty;
            Int32 nCounter = 0;

            for (int i = 0; i < m_LDataRows.Count; i++)
            {
                //InsertToTable(fdr.nEntityID, fdr.nItemID, fdr.nType, fdr.nAmount, fdr.sCurrencyCD, fdr.sCountryName, fdr.sSiteUserGUID, fdr.dDate, fdr.nRelSub, fdr.nContractID);

                FinancialDataRow fdr = m_LDataRows[i];

                query += "insert into fr_financial_entity_revenues(GROUP_ID, entity_ID, item_id, rel_sub, type, SITE_GUID, amount, CURRENCY_CODE, country, purchase_date, updater_id, IS_ACTIVE, STATUS, contract_id, catalog_price, actual_price, rel_coupon, rel_pre_paid, transaction_payment_number) values ";
                query += string.Format("({0}, {1}, {2}, {3}, {4}, '{5}', {6}, '{7}', '{8}', '{9}', {10}, 1, 1, {11}, {12}, {13}, {14}, {15}, {16});", m_nGroupID, fdr.nEntityID, fdr.nItemID, fdr.nRelSub, fdr.nType, fdr.sSiteUserGUID, fdr.nAmount,
                    fdr.sCurrencyCD, fdr.sCountryName, fdr.dDate.ToString("yyyy-MM-dd HH:mm"), 422, fdr.nContractID, fdr.nCatPrice, fdr.nActPrice, fdr.nCouponID, fdr.nPrePaidID, fdr.nPaymentNumber);

                nCounter++;

                if (nCounter == 300 || ((i + 1) == m_LDataRows.Count))
                {

                    ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                    directQuery += query;
                    directQuery.Execute();
                    directQuery.Finish();
                    directQuery = null;

                    nCounter = 0;
                    query = string.Empty;
                }
            }
        }

        protected void InsertToTable(Int32 nEntityID, Int32 nItemID, Int32 nType, double dPrice, string sCurrencyCD, string sCountry,
            string sSiteGUID, DateTime dPDate, Int32 nRelSub, Int32 nContractID)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("fr_financial_entity_revenues");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_ID", "=", nEntityID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("item_id", "=", nItemID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_sub", "=", nRelSub);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", nType);

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("amount", "=", dPrice);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrencyCD);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("country", "=", sCountry);

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "=", dPDate);

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", 422);

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("contract_id", "=", nContractID);

            insertQuery.Execute();
            insertQuery.Finish();

        }

        protected void UpdateGroupedRevenues()
        {

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ffe1.id, ffe1.entity_type ,ffe1.name, f2.total as total_revenus, f2.type as type, f2.currency_code, f2.country from fr_financial_entities as ffe1, ";
            selectQuery += "(select ffe.parent_entity_id as peid, ffe.name, ffer.entity_id, ffer.total as total, ffer.type as type, ffer.currency_code, ffer.country from fr_financial_entities as ffe, ";
            selectQuery += "(select entity_id, sum(amount) as total, type, currency_code, country  from fr_financial_entity_revenues as ffer ";
            selectQuery += "where is_active=1 and status=1 and entity_id<>0 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery += "group by entity_id, type, currency_code, country) as ffer";
            selectQuery += "where ffe.id=ffer.entity_id) as f2 ";
            selectQuery += "where (f2.peid=ffe1.id or (f2.peid=0 AND ffe1.id=f2.entity_id)) order by f2.type";

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nEntityID = Utils.GetIntSafeVal(ref selectQuery, "id", i);
                    Int32 nAssetType = Utils.GetIntSafeVal(ref selectQuery, "type", i);
                    Int32 nEntityType = Utils.GetIntSafeVal(ref selectQuery, "entity_type", i);
                    double dPrice = Utils.GetDoubleSafeVal(ref selectQuery, "total_revenus", i);
                    string sCurrencyCD = Utils.GetStrSafeVal(ref selectQuery, "currency_code", i);
                    string sCountry = Utils.GetStrSafeVal(ref selectQuery, "country", i);

                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("fr_financial_entity_revenues_grouped");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);

                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_ID", "=", nEntityID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("asset_type", "=", nAssetType);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_type", "=", nEntityType);

                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("price", "=", dPrice);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrencyCD);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("country", "=", sCountry);

                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", 422);

                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);

                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rel_date", "=", m_dStartDate);

                    insertQuery.Execute();
                    insertQuery.Finish();

                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        protected double GetInPrice(BaseContract bc, FinancialPurchaseObject fpo)
        {
            double dPrice = 0.0;

            switch (bc.m_eCalculatedOn)
            {
                case CalculatedOn.CataloguePrice:
                    {
                        //Calculate revenue on "Catalogue Price"
                        dPrice = fpo.m_dCataloguePrice;
                        break;
                    }
                case CalculatedOn.FinalPriceAfterDiscount:
                    {
                        //Calculate revenue on "Price After Discount"
                        dPrice = fpo.m_dDiscountPrice;
                        break;
                    }
                case CalculatedOn.FinalAfterTax:
                    {
                        //Calculate revenue on "After Tax"
                        dPrice = fpo.m_dTaxPrice;
                        break;
                    }
                case CalculatedOn.FinalAfterProcessing:
                    {
                        //Calculate revenue on "After Processing"
                        dPrice = fpo.m_dProcessorPrice;
                        break;
                    }
                case CalculatedOn.FinalAfterTaxAndProcessing:
                    {
                        //Calculate revenue on "After Tax And Processing"
                        dPrice = fpo.m_dProcessorAndTax;
                        break;
                    }
                default:
                    {
                        //Calculate revenue on "After Tax And Processing"
                        dPrice = fpo.m_dProcessorAndTax;
                        break;
                    }
            }

            return dPrice;
        }

        protected string FpoToStr(FinancialPurchaseObject fpo)
        {
            string sFpo = "cat_price=" + fpo.m_dCataloguePrice;
            sFpo += " purchase_date=" + fpo.m_dDate;
            sFpo += " dis_price=" + fpo.m_dDiscountPrice;
            sFpo += " P&T_price=" + fpo.m_dProcessorAndTax;
            sFpo += " Processor_price=" + fpo.m_dProcessorPrice;
            sFpo += " Tax_price=" + fpo.m_dTaxPrice;
            sFpo += " item_type=" + fpo.m_eItemType;
            sFpo += " related_to=" + fpo.m_eRelatedTo;
            sFpo += " Curreny_ID=" + fpo.m_nCurrencyID;
            sFpo += " rel_sub=" + fpo.m_nRelSub;
            sFpo += " Country_Name=" + fpo.m_sCountryName;
            sFpo += " Curreny_CD=" + fpo.m_sCurrencyCD;
            sFpo += " user_guid=" + fpo.m_sSiteUserGUID;

            return sFpo;
        }

        protected string CoToStr(CalcObject co, BaseContract bc)
        {
            string sCo = "contract_id=" + bc.m_nContractID;
            sCo += " entity_id=" + bc.m_nFinancialEntityID;
            sCo += " fix=" + bc.m_nFixAmount;
            sCo += " percentage=" + bc.m_nPercentageAmount;
            sCo += " in=" + co.m_dInPrice;
            sCo += " rev=" + co.m_dRevenuePrice;
            sCo += " out=" + co.m_dOutPrice;

            return sCo;
        }

        protected string FDRToString()
        {
            StringBuilder sRet = new StringBuilder();

            string row = "<breakDown>";
            sRet.Append(row);


            for (int i = 0; i < m_LDataRows.Count; i++)
            {

                FinancialDataRow fdr = m_LDataRows[i];

                row = string.Format("<row id=\"{0}\" entity_id =\"{1}\" Item_id=\"{2}\" User_Guid=\"{3}\" Type=\"{4}\" Amount=\"{5}\" Purchase_Date=\"{6}\" Country=\"{7}\" Subscription=\"{8}\" rel_contract=\"{9}\" Catalog_Price=\"{10}\" Actual_Price=\"{11}\" rel_coupon=\"{12}\" rel_pp=\"{13}\"/>",
                    i + 1, fdr.nEntityID, fdr.nItemID, fdr.sSiteUserGUID, fdr.nType, fdr.nAmount, fdr.dDate, fdr.sCountryName, fdr.nRelSub, fdr.nContractID, fdr.nCatPrice, fdr.nActPrice, fdr.nCouponID, fdr.nPrePaidID);

                sRet.Append(row);
            }

            sRet.Append("</breakDown>");

            return sRet.ToString();
        }


        private int CountTransaction(FinancialPurchaseObject fpo, Int32 nPaymentMethodEntityID, StartCountSince scs)
        {
            int countTransaction = 0;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select count(*) as counTransaction from billing_transactions where is_active=1 and status=1 and ";
                if (StartCountSince.Month == scs)
                {
                    DateTime startDate = new DateTime(m_dStartDate.Year, m_dStartDate.Month, 1);
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", startDate.ToString("yyyy-MM-dd HH:mm"));
                    selectQuery += "and";
                }
                else
                {
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                    selectQuery += "and";
                }
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "<>", 7); // Not a Gift
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_status", "=", 0);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER", "=", fpo.m_nBillingProvider);

                PaymentMethod pm = m_oBaseFinancial.GetPaymentMethod(nPaymentMethodEntityID);
                if (pm != null)
                {
                    if (pm.m_nBillingMethodID != 8) //PAYMENT METHODE - BILLING_METHOD == 8 , TAKE ALL TRANSACTION PER BILLING_PROVIDER
                    {
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_METHOD", "=", fpo.m_nBillingMethod);
                    }
                }
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "<", fpo.m_nId);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        countTransaction = Financial.Utils.GetIntSafeVal(ref selectQuery, "counTransaction", 0);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

            }
            catch (Exception ex)
            {
                log.Error("FinancialCalculator - CountTransaction - group_id = " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
                countTransaction = -1;
            }
            return countTransaction;
        }

    }





}
