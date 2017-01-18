using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class DiscountModule : PriceCode
    {
        public DiscountModule(): base()
        {
            m_dPercent = 0;
            m_eTheRelationType = RelationTypes.And;
            m_dStartDate = new DateTime(2000, 1, 1);
            m_dEndDate = new DateTime(2099, 1, 1);
        }

        public bool Initialize(string sC, Price p, LanguageContainer[] sD, Int32 nPriceCodeID, double dDiscountPercent,
            RelationTypes eTheRelationType , DateTime dStartDate , DateTime dEndDate , WhenAlgo whenAlgo)
        {
            base.Initialize(sC, p, sD, nPriceCodeID);
            m_dPercent = dDiscountPercent;
            m_eTheRelationType = eTheRelationType;
            m_dStartDate = dStartDate;
            m_dEndDate = dEndDate;
            m_oWhenAlgo = whenAlgo;
            return true;
        }

        static public LanguageContainer[] GetDiscountCodeDescription(Int32 nPriceCodeID)
        {
            LanguageContainer[] theContainer = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select * from discount_code_descriptions with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("discount_code_id", "=", nPriceCodeID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        theContainer = new LanguageContainer[nCount];
                    Int32 nIndex = 0;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sLang = selectQuery.Table("query").DefaultView[i].Row["language_code3"].ToString();
                        string sVal = selectQuery.Table("query").DefaultView[i].Row["description"].ToString();
                        LanguageContainer t = new LanguageContainer();
                        t.Initialize(sLang, sVal);
                        theContainer[nIndex] = t;
                        nIndex++;
                    }
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return theContainer;
        }

        public bool Initialize(Int32 nDiscountID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select * from discount_codes with (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nDiscountID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        string sCode = selectQuery.Table("query").DefaultView[0].Row["CODE"].ToString();
                        double dPrice = double.Parse(selectQuery.Table("query").DefaultView[0].Row["PRICE"].ToString());
                        double dDiscountPer = double.Parse(selectQuery.Table("query").DefaultView[0].Row["DISCOUNT_PERCENT"].ToString());
                        RelationTypes oRelType = ((RelationTypes)(int.Parse(selectQuery.Table("query").DefaultView[0].Row["RELATION_TYPE"].ToString())));
                        Int32 nCurrencyCD = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CURRENCY_CD"].ToString());
                        Price p = new Price();
                        p.InitializeByCodeID(nCurrencyCD, dPrice);
                        DateTime dStart = new DateTime(2000, 1, 1);
                        DateTime dEnd = new DateTime(2099, 1, 1);
                        if (selectQuery.Table("query").DefaultView[0].Row["START_DATE"] != null &&
                            selectQuery.Table("query").DefaultView[0].Row["START_DATE"] != DBNull.Value)
                            dStart = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["START_DATE"]);

                        if (selectQuery.Table("query").DefaultView[0].Row["END_DATE"] != null &&
                            selectQuery.Table("query").DefaultView[0].Row["END_DATE"] != DBNull.Value)
                            dEnd = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["END_DATE"]);

                        WhenAlgoType oWhenAlgoType = (WhenAlgoType)(int.Parse(selectQuery.Table("query").DefaultView[0].Row["WHENALGO_TYPE"].ToString()));
                        Int32 nWhenAlgoTimes = int.Parse(selectQuery.Table("query").DefaultView[0].Row["WHENALGO_TIMES"].ToString());
                        WhenAlgo wa = new WhenAlgo();
                        wa.Initialize(oWhenAlgoType, nWhenAlgoTimes);

                        Initialize(sCode, p, GetDiscountCodeDescription(nDiscountID), nDiscountID, dDiscountPer, oRelType, dStart, dEnd, wa);

                    }
                }

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return true;
        }

        public double m_dPercent;
        public RelationTypes m_eTheRelationType;
        public DateTime m_dStartDate;
        public DateTime m_dEndDate;
        public WhenAlgo m_oWhenAlgo;
        public string alias;
    }
}
