using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace Financial
{


    public class BaseContract
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public Int32 m_nGroupID;
        public Int32 m_nContractID;
        public Int32 m_nFinancialEntityID;

        public string m_sName;
        public string m_sDescription;

        public Int32 m_nCurrencyCD;
        public Int32 m_nRuleID;

        public DateTime m_dStartDate;
        public DateTime m_dEndDate;

        public double m_nFixAmount;
        public double m_nPercentageAmount;

        public double m_nMinAmount;
        public double m_nMaxAmount;

        public Int32 m_nCalculatedOnLevel;
        public Int32 m_nLevel;

        public RelatedTo m_eRelatedTo;
        public CalculatedOn m_eCalculatedOn;

        public List<string> m_ValidCountries;

        public bool m_bUpdateCountries;

        //new member 
        public ContractRange m_cContractRange;


        public BaseContract()
        {
            m_nGroupID = 0;
            m_nContractID = 0;
            m_nFinancialEntityID = 0;

            m_sName = string.Empty;
            m_sDescription = string.Empty;

            m_nCurrencyCD = 0;
            m_nRuleID = 0;

            m_dStartDate = new DateTime();
            m_dEndDate = new DateTime();

            m_nFixAmount = 0;
            m_nPercentageAmount = 0;

            m_nMinAmount = 0;
            m_nMaxAmount = 0;

            m_eRelatedTo = RelatedTo.PPV;
            m_nCalculatedOnLevel = 0;
            m_nLevel = 0;

            m_eCalculatedOn = CalculatedOn.CataloguePrice;

            m_ValidCountries = new List<string>();

            m_bUpdateCountries = false;

            m_cContractRange = new ContractRange();
        }

        public void Initialize(Int32 nGID, Int32 nCID, Int32 nFEID, string sName, string sDescription,
            Int32 nCurID, Int32 nRID, DateTime dStart, DateTime dEnd,
            double nFixAmount, double nPercentageAmount, double nMinAmount, double nMaxAmount,
            RelatedTo eRT, Int32 nCOL, Int32 nLevel, CalculatedOn eCO)
        {
            m_nGroupID = nGID;
            m_nContractID = nCID;
            m_nFinancialEntityID = nFEID;

            m_sName = sName;
            m_sDescription = sDescription;

            m_nCurrencyCD = nCurID;
            m_nRuleID = nRID;

            m_dStartDate = dStart;
            m_dEndDate = dEnd;

            m_nFixAmount = nFixAmount;
            m_nPercentageAmount = nPercentageAmount;

            m_nMinAmount = nMinAmount;
            m_nMaxAmount = nMaxAmount;

            m_eRelatedTo = eRT;
            m_nCalculatedOnLevel = nCOL;
            m_nLevel = nLevel;

            m_eCalculatedOn = eCO;

            if (m_nRuleID == 0)
            {
                m_bUpdateCountries = true;
            }
        }


        public void Initialize(Int32 nGID, Int32 nCID, Int32 nFEID, string sName, string sDescription,
           Int32 nCurID, Int32 nRID, DateTime dStart, DateTime dEnd,
           double nFixAmount, double nPercentageAmount, double nMinAmount, double nMaxAmount,
           RelatedTo eRT, Int32 nCOL, Int32 nLevel, CalculatedOn eCO, Int32 nContractRangeId)
        {
            this.Initialize(nGID, nCID, nFEID, sName, sDescription, nCurID, nRID, dStart, dEnd, nFixAmount, nPercentageAmount, nMinAmount, nMaxAmount,
            eRT, nCOL, nLevel, eCO);

            m_cContractRange = null;

            if (nContractRangeId > 0)
            {
                m_cContractRange = GetContractRange(nContractRangeId, m_nGroupID);
            }

        }

        public virtual bool IsContracatValid(double dPricae, Int32 nCurrenyCD, string sCountryName, DateTime dDate, RelatedTo eRelatedTo)
        {
            bool validCountry = false;

            if (string.IsNullOrEmpty(sCountryName) || m_nRuleID == 0)
            {
                validCountry = true;
            }
            else
            {
                if (m_bUpdateCountries == false)
                {
                    GetValidCountries();
                    m_bUpdateCountries = true;
                }

                foreach (string country_name in m_ValidCountries)
                {
                    if (country_name == sCountryName)
                    {
                        validCountry = true;
                        break;
                    }
                }
            }
            return ((nCurrenyCD == m_nCurrencyCD && validCountry) && (dDate <= m_dEndDate && dDate >= m_dStartDate));
        }

        public virtual double Calculate(double dPrice)
        {
            double nCal = m_nFixAmount + (dPrice * (m_nPercentageAmount / 100));

            if (nCal < m_nMinAmount)
            {
                return m_nMinAmount;
            }

            if (m_nMaxAmount > 0 && nCal > m_nMaxAmount)
            {
                return m_nMaxAmount;
            }

            return nCal;
        }

        private void GetValidCountries()
        {

            Int32 nOnlyOrBut = 0;
            string sOnlyOrBut = ODBCWrapper.Utils.GetTableSingleVal("geo_block_types", "only_or_but", m_nRuleID).ToString();
            nOnlyOrBut = int.Parse(sOnlyOrBut);

            StringBuilder countries = new StringBuilder();
            countries.Append("(");

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select country_id from geo_block_types_countries where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("geo_block_type_id", "=", m_nRuleID);
            selectQuery += " and status = 1";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {

                    string countryId = selectQuery.Table("query").DefaultView[i].Row["country_id"].ToString();

                    if (i > 0)
                        countries.Append(",");
                    countries.Append(countryId);

                }
            }
            selectQuery.Finish();
            selectQuery = null;

            countries.Append(")");

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select country_name from countries where id ";
            if (nOnlyOrBut == 1)
            {
                selectQuery += "not ";
            }
            selectQuery += "in " + countries;

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {

                    string country_name = selectQuery.Table("query").DefaultView[i].Row["country_name"].ToString();

                    m_ValidCountries.Add(country_name);

                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        protected ContractRange GetContractRange(Int32 nContractRangeId, Int32 nGroupID)
        {
            ContractRange cR = new ContractRange();
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from  fr_financial_contracts_ranges where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nContractRangeId);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        cR.m_nContractRangeId = nContractRangeId;

                        cR.m_nValueRangeMin = Utils.GetIntSafeVal(ref selectQuery, "min", i);
                        cR.m_nValueRangeMax = Utils.GetIntSafeVal(ref selectQuery, "max", i);

                        cR.m_nValueRangeType = (ValueRangeType)Utils.GetIntSafeVal(ref selectQuery, "range_type", i);
                        cR.m_eStartCountSince = (StartCountSince)Utils.GetIntSafeVal(ref selectQuery, "start_from", i);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetContractRange GroupID=" + m_nGroupID + " exception: ", ex);
                return cR;
            }
            return cR;
        }
    }
}

