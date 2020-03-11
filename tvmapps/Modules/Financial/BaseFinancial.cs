using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Financial
{

    public class BaseFinancial
    {
        public Int32 m_nGroupID;

        public List<ProcessorEntity> m_ProcessorsEntities;
        public List<TaxEntity> m_TaxEntities;

        public Hashtable m_hEntities;
        public Hashtable m_hFamilies;
        public Hashtable m_hContracts;

        public Dictionary<int, List<ContentOwnerContract>> m_dContractsLevels;

        public BaseFinancial(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;

            m_ProcessorsEntities = new List<ProcessorEntity>();
            m_TaxEntities = new List<TaxEntity>();

            m_hEntities = new Hashtable();
            m_hFamilies = new Hashtable();
            m_hContracts = new Hashtable();

            m_dContractsLevels = new Dictionary<int, List<ContentOwnerContract>>();
        }

        public void Initialize()
        {
            GetAllContentOwnerEntities();
            GetAllProcessorsEntities();
            GetAllTaxEntities();
        }

        private void GetAllProcessorsEntities()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select distinct lbp.value, lbp.description from fr_financial_entities t,lu_billing_processors lbp where ";
            selectQuery += "t.status<>2 and ";
            selectQuery += "lbp.value=t.BILLING_PROCESSOR_ID ";
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.entity_type", "=", 4);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.group_id", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {

                    Int32 processorID = Utils.GetIntSafeVal(ref selectQuery, "value", i);

                    string description = Utils.GetStrSafeVal(ref selectQuery, "description", i);

                    ProcessorEntity pe = new ProcessorEntity();
                    pe.Initialize(m_nGroupID, processorID, description);

                    m_ProcessorsEntities.Add(pe);

                }
            }
            selectQuery.Finish();
            selectQuery = null;


        }

        private void GetAllTaxEntities()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from fr_financial_entities where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_entity_id", "=", 0);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_type", "=", 3);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {

                    Int32 taxID = Utils.GetIntSafeVal(ref selectQuery, "id", i);

                    string name = Utils.GetStrSafeVal(ref selectQuery, "name", i);
                    string description = Utils.GetStrSafeVal(ref selectQuery, "description", i);

                    TaxEntity te = new TaxEntity();
                    te.Initialize(m_nGroupID, taxID, name, description);

                    m_TaxEntities.Add(te);

                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        public BaseContract GetValidTaxCotract(double dPrice, Int32 nCurrenyCD, string nRuleID, DateTime dDate, RelatedTo eRT)
        {
            List<BaseContract> contracts = new List<BaseContract>();

            foreach (TaxEntity tr in m_TaxEntities)
            {
                foreach (BaseContract bc in tr.m_Contracts)
                {
                    if (bc.IsContracatValid(dPrice, nCurrenyCD, nRuleID, dDate, eRT))
                    {
                        return bc;
                    }
                }

            }

            return null;
        }


        /*Get All Processor contracts that match to the values */
        public List<ProcessorContract> GetValidProcessorCotract(double dPrice, Int32 nCurrenyCD, string sCountryName, DateTime dDate, RelatedTo eRT, int nBillingMethod, int nBillingProvider)
        {
            List<BaseContract> contracts = new List<BaseContract>();
            List<ProcessorContract> cPcontracts = new List<ProcessorContract>();

            foreach (ProcessorEntity pe in m_ProcessorsEntities)
            {
                foreach (PaymentMethod pm in pe.m_Payments)
                {
                    //Check if PaymentMethod IsValid - depend on nBillingProvider (Adyen .....)
                    if (pm.IsPaymentMethodValid(nBillingMethod, nBillingProvider))
                    {
                        foreach (ProcessorContract pc in pm.m_Contracts)
                        {
                            if (pc.IsContracatValid(dPrice, nCurrenyCD, sCountryName, dDate, eRT))
                            {                                
                                cPcontracts.Add(pc);
                            }
                        }
                    }
                }
            }

            return cPcontracts;
        }


        public PaymentMethod GetPaymentMethod(Int32 nPaymentMethodEntityID)
        {
             foreach (ProcessorEntity pe in m_ProcessorsEntities)
            {
                foreach (PaymentMethod pm in pe.m_Payments)
                {
                    if (pm.m_nPaymentMethodEntityID == nPaymentMethodEntityID)
                    {
                        return pm;
                    }
                }
            }
             return null;
        }

        public ProcessorContract GetValidProcessorCotract(double dPrice, Int32 nCurrenyCD, string sCountryName, DateTime dDate, RelatedTo eRT)
        {
            List<BaseContract> contracts = new List<BaseContract>();

            foreach (ProcessorEntity pe in m_ProcessorsEntities)
            {
                foreach (PaymentMethod pm in pe.m_Payments)
                {
                    foreach (ProcessorContract pc in pm.m_Contracts)
                    {
                        if (pc.IsContracatValid(dPrice, nCurrenyCD, sCountryName, dDate, eRT))
                        {
                            return pc;
                        }
                    }
                    
                }
            }

            return null;
        }

        private void GetAllContentOwnerEntities()
        {
            Int32 nCount = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id, name, description, is_right_holder from fr_financial_entities where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_entity_id", "=", 0);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_type", "=", 1);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 contentOwnerID = Utils.GetIntSafeVal(ref selectQuery, "id", i);

                    string name = Utils.GetStrSafeVal(ref selectQuery, "name", i);
                    string description = Utils.GetStrSafeVal(ref selectQuery, "description", i);

                    Int32 nIsRightHolder = Utils.GetIntSafeVal(ref selectQuery, "is_right_holder", i);

                    ContentOwnerEntity coe = new ContentOwnerEntity();
                    coe.Initialize(m_nGroupID, contentOwnerID, name, description, nIsRightHolder);

                    m_hEntities.Add(contentOwnerID, coe);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nCount > 0)
            {
                string sRHEList = GetListForDB(m_hEntities);
                GetAllLimitations(sRHEList);

                nCount = 0;
                GetFamilies(sRHEList, ref nCount);
            }

            if (nCount > 0)
            {
                GetContracts(GetListForDB(m_hFamilies)); 
            }
        }

        private void GetFamilies(string sRHEList, ref Int32 nCount)
        {
            nCount = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id, name, description, parent_entity_id from fr_financial_entities where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_type", "=", 1);
            selectQuery += "and parent_entity_id in " + sRHEList;
            if (selectQuery.Execute("query", true) != null)
            {
                nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 contractFamilyID = Utils.GetIntSafeVal(ref selectQuery, "id", i);

                    string name = Utils.GetStrSafeVal(ref selectQuery, "name", i);
                    string description = Utils.GetStrSafeVal(ref selectQuery, "description", i);

                    Int32 parentID = Utils.GetIntSafeVal(ref selectQuery, "parent_entity_id", i);

                    ContractFamily cf = new ContractFamily();
                    cf.Initialize(m_nGroupID, contractFamilyID, parentID, name, description);

                    m_hFamilies.Add(contractFamilyID, cf);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        private void GetContracts(string sRHFList)
        {

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from fr_financial_entity_contracts where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += "and financial_entity_id in " + sRHFList;
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {

                    Int32 nContractID = Utils.GetIntSafeVal(ref selectQuery, "id", i);

                    Int32 nContractFamilyID = Utils.GetIntSafeVal(ref selectQuery, "FINANCIAL_ENTITY_ID", i);

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

                    Int32 relatedTo = Utils.GetIntSafeVal(ref selectQuery, "LICENSE_OR_SUB", i);
                    Int32 calculatedOnLevel = Utils.GetIntSafeVal(ref selectQuery, "CALC_ON_LEVEL", i);
                    Int32 level = Utils.GetIntSafeVal(ref selectQuery, "LEVEL_NUM", i);
                    Int32 calculatedOn = Utils.GetIntSafeVal(ref selectQuery, "OUT_OF_TYPE", i);

                    ContentOwnerContract bc = new ContentOwnerContract();
                    bc.Initialize(m_nGroupID, nContractID, nContractFamilyID, name, description, currencyCD, ruleID,
                        startDate, endDate, fixAmount, percentageAmount, minAmount, maxAmount,
                        (RelatedTo)relatedTo, calculatedOnLevel, level, (CalculatedOn)calculatedOn);

                    m_hContracts.Add(nContractID, bc);

                    if (!m_dContractsLevels.ContainsKey(level))
                    {
                        m_dContractsLevels.Add(level, new List<ContentOwnerContract>());
                    }
                    m_dContractsLevels[level].Add(bc);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }


        private void GetAllLimitations(string sRHEList)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from fr_financial_entity_limitations where is_active=1 and status=1 and ";
            selectQuery += "financial_entity_id in " + sRHEList;
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 limitationID = Utils.GetIntSafeVal(ref selectQuery, "id", i);

                    string name = Utils.GetStrSafeVal(ref selectQuery, "name", i);
                    string description = Utils.GetStrSafeVal(ref selectQuery, "description", i);

                    Int32 financialEntityID = Utils.GetIntSafeVal(ref selectQuery, "Financial_Entity_ID", i);
                    Int32 currencyCD = Utils.GetIntSafeVal(ref selectQuery, "Currency_CD", i);

                    DateTime startDate = Utils.GetDateSafeVal(ref selectQuery, "start_date", i);
                    DateTime endDate = Utils.GetDateSafeVal(ref selectQuery, "end_date", i);

                    double minFixPrice = Utils.GetDoubleSafeVal(ref selectQuery, "min_fix_price", i);
                    double maxFixPrice = Utils.GetDoubleSafeVal(ref selectQuery, "max_fix_price", i);

                    Limitations lim = new Limitations();
                    lim.Initialize(m_nGroupID, limitationID, name, description, financialEntityID, currencyCD,
                        startDate, endDate, minFixPrice, maxFixPrice);

                    if (m_hEntities.Contains(financialEntityID))
                        ((ContentOwnerEntity)m_hEntities[financialEntityID]).m_Limitations.Add(lim);

                }
            }
            selectQuery.Finish();
            selectQuery = null;



        }

        public ContentOwnerContract GetContractWithContractFamilyID(Int32 nCFID, double dPrice, Int32 nCurrenyCD, string sCountryName, DateTime dDate, RelatedTo eRT)
        {

            if (!m_hFamilies.Contains(nCFID))
                return null;


            foreach (DictionaryEntry de in m_hContracts)
            {
                ContentOwnerContract coc = (ContentOwnerContract)de.Value;
                
                if (coc.m_nFinancialEntityID == nCFID)
                {     
                    if (coc.IsContracatValid(dPrice, nCurrenyCD, sCountryName, dDate, eRT))
                        return coc;
                }
            }

            return null;
        }

        public List<ContentOwnerContract> GetValidCotractsForLevel(Int32 nLevel, double dPrice, Int32 nCurrenyCD, string sCountryName, DateTime dDate, RelatedTo eRT)
        {
            List<ContentOwnerContract> contracts = new List<ContentOwnerContract>();
            if (m_dContractsLevels.ContainsKey(nLevel))
            {
                foreach (ContentOwnerContract coc in m_dContractsLevels[nLevel])
                {
                    if (coc.IsContracatValid(dPrice, nCurrenyCD, sCountryName, dDate, eRT))
                    {
                        contracts.Add(coc);
                    }
                }
            }
            return contracts;
        }

        public List<ContentOwnerContract> GetValidCotractsForLevel_Old(Int32 nLevel, double dPrice, Int32 nCurrenyCD, string sCountryName, DateTime dDate, RelatedTo eRT)
        {

            List<ContentOwnerContract> contracts = new List<ContentOwnerContract>();

            foreach (DictionaryEntry de in m_hContracts)
            {
                ContentOwnerContract coc = (ContentOwnerContract)de.Value;

                if (coc.m_nLevel == nLevel && coc.IsContracatValid(dPrice, nCurrenyCD, sCountryName, dDate, eRT))
                    contracts.Add(coc);
            }

            return contracts;
        }


        private string GetListForDB(Hashtable hTable)
        {
            Int32 index = 0;

            StringBuilder sRet = new StringBuilder();
            sRet.Append("(");

            foreach (DictionaryEntry de in hTable)
            {
                if (index++ > 0)
                {
                    sRet.Append(",");
                }

                sRet.Append(de.Key.ToString());
            }
            sRet.Append(")");

            return sRet.ToString();
        }

        public bool IsContractOwnerIsRightHolder(ContentOwnerContract coc)
        {
            Int32 nIsRH = 0;
            try
            {
                ContractFamily cf = (ContractFamily)m_hFamilies[coc.m_nFinancialEntityID];
                ContentOwnerEntity coe = (ContentOwnerEntity)m_hEntities[cf.m_nParentEntityID];
                nIsRH = coe.m_nIsRightHolder;
            }
            catch
            {
                return false;
            }
            return (nIsRH == 1);
        }
    }
}
