using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;

namespace Financial
{
    public class ContentOwnerEntity
    {
        public Int32 m_nGroupID;
        public Int32 m_nContentOwnerEntityID;

        public string m_sName;
        public string m_sDescription;

        public List<Limitations> m_Limitations;

        public Int32 m_nIsRightHolder;

        public ContentOwnerEntity()
        {
            m_nGroupID = 0;
            m_nContentOwnerEntityID = 0;

            m_sName = string.Empty;
            m_sDescription = string.Empty;

            m_nIsRightHolder = 1;

            m_Limitations = new List<Limitations>();
        }

        public void Initialize(Int32 nGroupID, Int32 nContentOwnerEntityID, string sName, string sDescription, Int32 nIsRightHolder)
        {

            m_nGroupID = nGroupID;
            m_nContentOwnerEntityID = nContentOwnerEntityID;

            m_sName = sName;
            m_sDescription = sDescription;

            m_nIsRightHolder = nIsRightHolder;

            //GetAllLimitations();

        }

        private void GetAllLimitations()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from fr_financial_entity_limitations where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("financial_entity_id", "=", m_nContentOwnerEntityID);
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

                    m_Limitations.Add(lim);

                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
    }
}
