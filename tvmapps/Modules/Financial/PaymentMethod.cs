using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;

namespace Financial
{
    public class PaymentMethod
    {
        public Int32 m_nGroupID;
        public Int32 m_nPaymentMethodEntityID;

        public string m_sName;
        public string m_sDescription;

        public Int32 m_nBillingMethodID;
        public Int32 m_nBillingProcessorID;

        public bool m_bMustBePaid;

        public List<ProcessorContract> m_Contracts;

        public PaymentMethod()
        {
            m_nGroupID = 0;
            m_nPaymentMethodEntityID = 0;

            m_sName = string.Empty;
            m_sDescription = string.Empty;

            m_nBillingMethodID = 0;
            m_nBillingProcessorID = 0;         

            m_Contracts = new List<ProcessorContract>();
        }

        public void Initialize(Int32 nGroupID, Int32 nPaymentMethodEntityID, string sName, string sDescription, Int32 nBillingMethodID, Int32 nBillingProcessorID)
        {

            m_nGroupID = nGroupID;
            m_nPaymentMethodEntityID = nPaymentMethodEntityID;

            m_sName = sName;
            m_sDescription = sDescription;

            m_nBillingMethodID = nBillingMethodID;
            m_nBillingProcessorID = nBillingProcessorID;

            GetAllContracts();
        }

        public void Initialize(Int32 nGroupID, Int32 nPaymentMethodEntityID, string sName, string sDescription, Int32 nBillingMethodID, Int32 nBillingProcessorID, bool bMustBePaid)
        {

            Initialize(nGroupID, nPaymentMethodEntityID, sName, sDescription, nBillingMethodID, nBillingProcessorID);
            m_bMustBePaid = bMustBePaid;

        }

        private void GetAllContracts()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from fr_financial_entity_contracts where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("financial_entity_id", "=", m_nPaymentMethodEntityID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nContractID = Utils.GetIntSafeVal(ref selectQuery, "id", i);

                    string name = Utils.GetStrSafeVal(ref selectQuery, "name", i);
                    string description = Utils.GetStrSafeVal(ref selectQuery, "description", i);

                    Int32 currencyCD = Utils.GetIntSafeVal(ref selectQuery, "Currency_CD", i);
                    Int32 ruleID = Utils.GetIntSafeVal(ref selectQuery, "COUNTRIES_RULE_ID", i);

                    DateTime startDate = Utils.GetDateSafeVal(ref selectQuery, "start_date", i);
                    DateTime endDate = Utils.GetDateSafeVal(ref selectQuery, "end_date", i);

                    double fixAmount = Utils.GetDoubleSafeVal(ref selectQuery, "FIX_PRICE", i);
                    double percentageAmount = Utils.GetDoubleSafeVal(ref selectQuery, "PER", i);

                    double minAmount = Utils.GetDoubleSafeVal(ref selectQuery, "MIN_AMOUNT", i);
                    double maxAmount = Utils.GetDoubleSafeVal(ref selectQuery, "MAX_AMOUNT", i);

                    Int32 minPerMonth = Utils.GetIntSafeVal(ref selectQuery, "MIN_NUMBER_OF_TRANSACTIONS", i);
                    Int32 maxPerMonth = Utils.GetIntSafeVal(ref selectQuery, "MAX_NUMBER_OF_TRANSACTIONS", i);

                    // contractRangeId to each processor contract
                   Int32 contractRangeId = Utils.GetIntSafeVal(ref selectQuery, "CONTRACT_RANGE_ID", i);
                   
                    ProcessorContract pc = new ProcessorContract();
                    pc.Initialize(m_nGroupID, nContractID, m_nPaymentMethodEntityID, name, description, currencyCD, ruleID,
                        startDate, endDate, fixAmount, percentageAmount, minAmount, maxAmount, 0, 0, 0, 0, minPerMonth, maxPerMonth, contractRangeId, m_nPaymentMethodEntityID);                                      
                    
                    m_Contracts.Add(pc);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        // sPaymentMethodValid if it MustBePaid or match nBillingMethod && nBillingProvider
        public bool IsPaymentMethodValid(int nBillingMethod, int nBillingProvider)
        {
            if (nBillingProvider == m_nBillingProcessorID)
            {
                if (nBillingMethod == m_nBillingMethodID)
                {
                    return true;
                }

                if (m_nBillingMethodID == 8) // TODO : change 7 to 8 = All (must be paid) TODO LIAT: add values to [lu_billing_methods] 8 = all 
                {
                    return true;
                }
            }

            return false;
        }
           


    }
}
