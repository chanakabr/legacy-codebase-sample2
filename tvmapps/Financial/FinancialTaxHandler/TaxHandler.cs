using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;


namespace FinancialTaxHandler
{
    public class TaxHandler : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        static protected object o = new object();
        public TaxHandler(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new TaxHandler(nTaskID, nIntervalInSec, sParameters);
        }

        static protected void UpdateTableSyncStatus(string sTable, Int32 nStatus, Int32 nRow, string sConnectionKey)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(sTable);
            updateQuery.SetConnectionKey(sConnectionKey);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("TAX_PROCESSING_STATUS", "=", nStatus);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nRow);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        protected override bool DoTheTaskInner()
        {
            return DoTheJob();
        }

        static protected FinancialUtils.FinancialPayment[] GetContractID(Int32 nGroupID, Int32 nCountryID, Int32 nDeviceID, Int32 nLangID,
            double dPaid, Int32 nCurrencyCodeID, Int32 nTransactionNumberThisMonth, double dTotalPaidThisMonth,
            DateTime dTransactionDate, bool bIsLicense, bool bIsSub, Int32 nEntityType, Int32 nBillingProcessorID, Int32 nBillingMethodID)
        {
            FinancialUtils.FinancialPayment[] ret = new FinancialUtils.FinancialPayment[0];
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select fe.id as 'fe_id',t.* from fr_financial_entity_contracts t,fr_financial_entities fe ";
            selectQuery += "where fe.id=t.FINANCIAL_ENTITY_ID and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("fe.entity_type", "=", nEntityType);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.group_id", "=", nGroupID);
            selectQuery += "and (";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.start_date", "<", dTransactionDate);
            selectQuery += " and (t.END_DATE is null or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.END_DATE", ">=", dTransactionDate);
            selectQuery += ")";
            selectQuery += ")";
            selectQuery += " and t.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.CURRENCY_CD", "=", nCurrencyCodeID);
            if (nBillingMethodID != 0 && nBillingProcessorID != 0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("fe.BILLING_METHOD_ID", "=", nBillingMethodID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("fe.BILLING_PROCESSOR_ID", "=", nBillingProcessorID);

            }
            if (bIsSub != bIsLicense)
            {
                if (bIsSub == true)
                    selectQuery += " and t.LICENSE_OR_SUB in (2,3)";
                if (bIsLicense == true)
                    selectQuery += " and t.LICENSE_OR_SUB in (1,3)";
            }
            //if (nTransactionNumberThisMonth > 0)
            //{
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.MIN_NUMBER_OF_TRANSACTIONS", ">=", nTransactionNumberThisMonth);
            selectQuery += " and (t.MAX_NUMBER_OF_TRANSACTIONS=0 or";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.MAX_NUMBER_OF_TRANSACTIONS", "<", nTransactionNumberThisMonth);
            selectQuery += ")";
            //}

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nContractID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    Int32 nFEID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["FE_ID"].ToString());
                    Int32 nCOUNTRIES_RULE_ID = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "COUNTRIES_RULE_ID", i);

                    bool bDoesRuleIncludeCountry = true;
                    if (nCountryID != 0)
                        bDoesRuleIncludeCountry = TVinciShared.PageUtils.DoesGeoBlockTypeIncludeCountry(nCOUNTRIES_RULE_ID, nGroupID);
                    if (bDoesRuleIncludeCountry == false)
                        continue;

                    //FinancialUtils.FinancialPayment thePayment = new FinancialUtils.FinancialPayment();
                    ret = (FinancialUtils.FinancialPayment[])(FinancialUtils.Utils.ResizeArray(ret, ret.Length + 1));
                    ret[ret.Length - 1] = new FinancialUtils.FinancialPayment();

                    double dFixPrice = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "FIX_PRICE", i);
                    double dPer = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "PER", i);
                    double dOR_PRICE = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "OR_PRICE", i);
                    //Int32 nLICENSE_OR_SUB = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "LICENSE_OR_SUB", i);
                    Int32 nIS_OR_PRICE_RELEVANT = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "IS_OR_PRICE_RELEVANT", i);
                    Int32 nLOWER_OR_HIGHER = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "LOWER_OR_HIGHER", i);
                    Int32 nOUT_OF_TYPE = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "OUT_OF_TYPE", i);
                    Int32 nCALC_ON_LEVEL = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "CALC_ON_LEVEL", i);
                    Int32 nLEVEL_NUM = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "LEVEL_NUM", i);
                    double dMIN_AMOUNT = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "MIN_AMOUNT", i);
                    double dMAX_AMOUNT = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "MAX_AMOUNT", i);

                    double dCalc = dFixPrice + ((double)((dPer * dPaid) / 100));
                    if (nIS_OR_PRICE_RELEVANT == 1)
                    {
                        if (nLOWER_OR_HIGHER == 1 && dOR_PRICE < dCalc)
                            dCalc = dOR_PRICE;
                        if (nLOWER_OR_HIGHER == 2 && dOR_PRICE > dCalc)
                            dCalc = dOR_PRICE;
                    }
                    if (dMIN_AMOUNT != 0.0 && dCalc > 0 && dCalc < dMIN_AMOUNT)
                        dCalc = dMIN_AMOUNT;
                    if (dMAX_AMOUNT > 0.0 && dCalc > 0 && dCalc > dMAX_AMOUNT)
                        dCalc = dMAX_AMOUNT;
                    double dFinalPrice = dCalc;

                    ret[ret.Length - 1].Initialize(nContractID, nFEID, dCalc, dFinalPrice, nCurrencyCodeID, "", nGroupID, dPaid, nBillingProcessorID, nBillingMethodID);
                    /*
                    double dAFF_REGISTER_FIX_PRICE = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "AFF_REGISTER_FIX_PRICE", i);
                    double dAFF_SUB_FIX_PRICE = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "AFF_SUB_FIX_PRICE", i);
                    double dAFF_SUB_PER = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "AFF_SUB_PER", i);
                    double dAFF_SUB_OR_PRICE = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "AFF_SUB_OR_PRICE", i);
                    Int32 nAFF_SUB_IS_OR_PRICE_RELEVANT = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "AFF_SUB_IS_OR_PRICE_RELEVANT", i);
                    Int32 nAFF_SUB_LOWER_OR_HIGHER = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "AFF_SUB_LOWER_OR_HIGHER", i);
                    Int32 nAFF_SUB_MAXIMUM_PAYMENTS = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "AFF_SUB_MAXIMUM_PAYMENTS", i);

                    double dAFF_PPV_FIX_PRICE = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "AFF_PPV_FIX_PRICE", i);
                    double dAFF_PPV_PER = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "AFF_PPV_PER", i);
                    double dAFF_PPV_OR_PRICE = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "AFF_PPV_OR_PRICE", i);
                    Int32 nAFF_PPV_IS_OR_PRICE_RELEVANT = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "AFF_PPV_IS_OR_PRICE_RELEVANT", i);
                    double dAFF_PPV_LOWER_OR_HIGHER = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "AFF_PPV_LOWER_OR_HIGHER", i);
                    Int32 nAFF_PPV_MAXIMUM_PAYMENTS = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "AFF_PPV_MAXIMUM_PAYMENTS", i);
                    Int32 nAFF_PERIOD_IN_MONTH = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "AFF_PERIOD_IN_MONTH", i);
                    */
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return ret;
        }

        static public Int32 GetCountOfMonthProcessorMethodTransactions(DateTime dTransactionDate, Int32 nBillingProcessorID,
            Int32 nBillingMethodID, string sCurrencyCode)
        {
            Int32 nCo = 0;
            dTransactionDate = dTransactionDate.AddMonths(-1);
            DateTime dStart = new DateTime(dTransactionDate.Year, dTransactionDate.Month, 1, 0, 0, 0).AddMonths(1);
            DateTime dEnd = dStart.AddMonths(1);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select count(*) as 'co' from billing_transactions_financial_payments (nolock) where  ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_DATE", ">=", dStart);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_DATE", "<", dEnd);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROCESSOR_ID", "=", nBillingProcessorID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_METHOD_ID", "=", nBillingMethodID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrencyCode);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCo = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nCo;
        }

        static protected void GetMethodAndProcessrByBILLING_PROVIDER(Int32 nBILLING_PROVIDER, ref Int32 nBILLING_METHOD_ID, ref Int32 nBILLING_PROCESSOR_ID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from lu_implementation_type where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nBILLING_PROVIDER);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nBILLING_METHOD_ID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["BILLING_METHOD_ID"].ToString());
                    nBILLING_PROCESSOR_ID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["BILLING_PROCESSOR_ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public void HandlePaymentMethods()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from billing_transactions (nolock) where BILLING_STATUS=0 and FINANCIAL_PROCESSING_STATUS=1";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nID = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "ID", i);
                    double dPaid = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "TOTAL_PRICE", i);
                    Int32 nGroupID = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "GROUP_ID", i);

                    string sCurrencyCode = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "CURRENCY_CODE", i);
                    string sCountryCode = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "COUNTRY_CODE", i);
                    string sDevice = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "DEVICE_NAME", i);
                    string sLanguage = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "LANGUAGE_CODE", i);
                    DateTime dTransactionDate = FinancialUtils.Utils.GetDateSafeVal(ref selectQuery, "CREATE_DATE", i);
                    Int32 nBillingProcessorID = 0;
                    Int32 nBillingMethodID = 0;
                    Int32 nBILLING_PROVIDER = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "BILLING_PROVIDER", i);
                    GetMethodAndProcessrByBILLING_PROVIDER(nBILLING_PROVIDER, ref nBillingMethodID, ref nBillingProcessorID);
                    Int32 nTransactionNumberThisMonth = GetCountOfMonthProcessorMethodTransactions(dTransactionDate, nBillingProcessorID, nBillingMethodID, sCurrencyCode);

                    Int32 nLangID = 0;
                    Int32 nDeviceID = 0;
                    Int32 nCurrencyCodeID = 0;
                    bool bIsLangMain = true;
                    Int32 nCountryID = FinancialUtils.Utils.GetCountryID(sCountryCode);
                    nCurrencyCodeID = FinancialUtils.Utils.GetCurrencyIDByCode(sCurrencyCode);
                    if (sDevice != "")
                        nDeviceID = TVinciShared.ProtocolsFuncs.GetDeviceIdFromName(sDevice, nGroupID);
                    if (sLanguage != "")
                        TVinciShared.ProtocolsFuncs.GetLangData(sLanguage, nGroupID, ref nLangID, ref bIsLangMain);
                    double dTotalPaidThisMonth = 0.0;

                    FinancialUtils.FinancialPayment[] thePayments = GetContractID(nGroupID, nCountryID, nDeviceID, nLangID, dPaid, nCurrencyCodeID,
                        nTransactionNumberThisMonth, dTotalPaidThisMonth, dTransactionDate, false, false, 4, nBillingProcessorID, nBillingMethodID);
                    Int32 nPaymentsCount = thePayments.Length;
                    for (int j = 0; j < nPaymentsCount; j++)
                    {
                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("billing_transactions_financial_payments");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", nID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TAX_PRICE", "=", thePayments[j].m_dFinalAmount);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FULL_TAX_PRICE", "=", thePayments[j].m_dFullAmount);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrencyCode);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", thePayments[j].m_nGroupID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_ID", "=", thePayments[j].m_nContractID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_PRICE", "=", thePayments[j].m_dBillingTransactionAmount);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FINANCIAL_ENTITY_ID", "=", thePayments[j].m_nEntityID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DISCOUNT_REASON", "=", thePayments[j].m_sDiscountReason);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FINAL_TAX_PER", "=", (double)(thePayments[j].m_dFinalAmount * 100 / thePayments[j].m_dBillingTransactionAmount));
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_TYPE_ID", "=", 4);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROCESSOR_ID", "=", thePayments[j].m_nBillingProcessorID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_METHOD_ID", "=", thePayments[j].m_nBillingMethodID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_DATE", "=", dTransactionDate);

                        insertQuery.Execute();
                        insertQuery.Finish();
                        insertQuery = null;
                    }
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("FINANCIAL_PROCESSING_STATUS", "=", 2);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public void HandleTax()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from billing_transactions (nolock) where BILLING_STATUS=0 and FINANCIAL_PROCESSING_STATUS=0";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nID = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "ID", i);
                    double dPaid = FinancialUtils.Utils.GetDoubleSafeVal(ref selectQuery, "TOTAL_PRICE", i);
                    Int32 nGroupID = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "GROUP_ID", i);
                    string sCurrencyCode = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "CURRENCY_CODE", i);
                    string sCountryCode = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "COUNTRY_CODE", i);
                    string sDevice = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "DEVICE_NAME", i);
                    string sLanguage = FinancialUtils.Utils.GetStrSafeVal(ref selectQuery, "LANGUAGE_CODE", i);
                    //Int32 nBillingProcessorID = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "BILLING_PROCESSOR_ID", i);
                    //Int32 nBillingMethodID = FinancialUtils.Utils.GetIntSafeVal(ref selectQuery, "BILLING_METHOD_ID", i);
                    DateTime dTransactionDate = FinancialUtils.Utils.GetDateSafeVal(ref selectQuery, "CREATE_DATE", i);
                    Int32 nLangID = 0;
                    Int32 nDeviceID = 0;
                    Int32 nCurrencyCodeID = 0;
                    bool bIsLangMain = true;
                    Int32 nCountryID = FinancialUtils.Utils.GetCountryID(sCountryCode);
                    nCurrencyCodeID = FinancialUtils.Utils.GetCurrencyIDByCode(sCurrencyCode);
                    if (sDevice != "")
                        nDeviceID = TVinciShared.ProtocolsFuncs.GetDeviceIdFromName(sDevice, nGroupID);
                    if (sLanguage != "")
                        TVinciShared.ProtocolsFuncs.GetLangData(sLanguage, nGroupID, ref nLangID, ref bIsLangMain);
                    Int32 nTransactionNumberThisMonth = 0;
                    double dTotalPaidThisMonth = 0.0;

                    FinancialUtils.FinancialPayment[] thePayments = GetContractID(nGroupID, nCountryID, nDeviceID, nLangID, dPaid, nCurrencyCodeID,
                        nTransactionNumberThisMonth, dTotalPaidThisMonth, dTransactionDate, false, false, 3, 0, 0);
                    Int32 nPaymentsCount = thePayments.Length;
                    for (int j = 0; j < nPaymentsCount; j++)
                    {
                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("billing_transactions_financial_payments");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", nID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TAX_PRICE", "=", thePayments[j].m_dFinalAmount);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FULL_TAX_PRICE", "=", thePayments[j].m_dFullAmount);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrencyCode);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", thePayments[j].m_nGroupID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_ID", "=", thePayments[j].m_nContractID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_PRICE", "=", thePayments[j].m_dBillingTransactionAmount);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FINANCIAL_ENTITY_ID", "=", thePayments[j].m_nEntityID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DISCOUNT_REASON", "=", thePayments[j].m_sDiscountReason);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FINAL_TAX_PER", "=", (double)(thePayments[j].m_dFinalAmount * 100 / thePayments[j].m_dBillingTransactionAmount));
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_TYPE_ID", "=", 3);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROCESSOR_ID", "=", thePayments[j].m_nBillingProcessorID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_METHOD_ID", "=", thePayments[j].m_nBillingMethodID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_DATE", "=", dTransactionDate);

                        insertQuery.Execute();
                        insertQuery.Finish();
                        insertQuery = null;
                    }
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("FINANCIAL_PROCESSING_STATUS", "=", 1);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }


        public bool DoTheJob()
        {
            try
            {
                lock (o)
                {
                    if (FinancialUtils.Utils.GetSyncStatus(6) == 1)
                        return true;
                    FinancialUtils.Utils.UpdateSyncStatus(1, 6);
                }

                HandleTax();
                HandlePaymentMethods();
                lock (o)
                {
                    FinancialUtils.Utils.UpdateSyncStatus(0, 6);
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Exception - On function: DoTheTaskInner ", ex);
                return false;
            }
        }
    }
}
