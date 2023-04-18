using ApiObjects;
using ApiObjects.Pricing;
using Core.GroupManagers;
using System;
using System.Transactions;
using APILogic;
using Google.Protobuf;

namespace Core.Pricing
{
    [Serializable]
    public class DiscountModule : PriceCode, IDeepCloneable<DiscountModule>
    {
        public DiscountModule(): base()
        {
            m_dPercent = 0;
            m_eTheRelationType = RelationTypes.And;
            m_dStartDate = new DateTime(2000, 1, 1);
            m_dEndDate = new DateTime(2099, 1, 1);
        }

        public DiscountModule(DiscountModule other) : base(other)
        {
            m_dPercent = other.m_dPercent;
            m_eTheRelationType = other.m_eTheRelationType;
            m_dStartDate = other.m_dStartDate;
            m_dEndDate = other.m_dEndDate;
            m_oWhenAlgo = Extensions.Clone(other.m_oWhenAlgo);
            alias = other.alias;
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

        public static LanguageContainer[] GetDiscountCodeDescription(Int32 nPriceCodeID)
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

        public double m_dPercent;
        public RelationTypes m_eTheRelationType;
        public DateTime m_dStartDate;
        public DateTime m_dEndDate;
        public WhenAlgo m_oWhenAlgo;
        public string alias;
        public DiscountModule Clone()
        {
            return new DiscountModule(this);
        }
    }
}
