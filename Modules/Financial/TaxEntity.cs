using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;

namespace Financial
{
    public class TaxEntity
    {
        public Int32 m_nGroupID;
        public Int32 m_nTaxEntityID;

        public string m_sName;
        public string m_sDescription;

        public List<BaseContract> m_Contracts;

        public TaxEntity()
        {
            m_nGroupID = 0;
            m_nTaxEntityID = 0;

            m_sName = string.Empty;
            m_sDescription = string.Empty;

            m_Contracts = new List<BaseContract>();

        }

        public void Initialize(Int32 nGroupID, Int32 nTaxEntityID, string sName, string sDescription)
        {

            m_nGroupID = nGroupID;
            m_nTaxEntityID = nTaxEntityID;

            m_sName = sName;
            m_sDescription = sDescription;

            GetAllContracts();

        }

        private void GetAllContracts()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from fr_financial_entity_contracts where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("financial_entity_id", "=", m_nTaxEntityID);
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

                    BaseContract tc = new BaseContract();
                    tc.Initialize(m_nGroupID, nContractID, m_nTaxEntityID, name, description, currencyCD, ruleID,
                        startDate, endDate, fixAmount, percentageAmount, minAmount, maxAmount, 0, 0, 0, 0);

                    m_Contracts.Add(tc);

                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        public BaseContract findValidContract(double nPrice, Int32 nCurrenyCD, string sCountryName, DateTime dDate, RelatedTo eRT)
        {

            foreach (BaseContract contract in m_Contracts)
            {
                if (contract.IsContracatValid(nPrice, nCurrenyCD, sCountryName, dDate, eRT))
                {
                    return contract;
                }
            }

            return null;
        }

    }
}
