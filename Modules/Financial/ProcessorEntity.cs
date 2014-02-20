using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;

namespace Financial
{
    public class ProcessorEntity
    {
        public Int32 m_nGroupID;
        public Int32 m_nBillingProcessorID;

        public string m_sName;

        public List<PaymentMethod> m_Payments;
        public List<Limitations> m_Limitations;

        public ProcessorEntity()
        {
            m_nGroupID = 0;
            m_nBillingProcessorID = 0;

            m_sName = string.Empty;

            m_Payments = new List<PaymentMethod>();
            m_Limitations = new List<Limitations>();
        }

        public void Initialize(Int32 nGroupID, Int32 nBillingProcessorID, string sName)
        {
            m_nGroupID = nGroupID;
            m_nBillingProcessorID = nBillingProcessorID;

            m_sName = sName;

            GetAllPayments();

            GetAllLimitations();
        }



        private void GetAllPayments()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from fr_financial_entities where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_type", "=", 4);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_processor_id", "=", m_nBillingProcessorID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {

                    Int32 nPaymentMethodID = 0;

                    string name = string.Empty;
                    string description = string.Empty;

                    Int32 nBillingMethodID = 0;

                    nPaymentMethodID = Utils.GetIntSafeVal(ref selectQuery, "id", i);
                    name = Utils.GetStrSafeVal(ref selectQuery, "name", i);
                    description = Utils.GetStrSafeVal(ref selectQuery, "description", i);

                    nBillingMethodID = Utils.GetIntSafeVal(ref selectQuery, "billing_method_id", i);

                    PaymentMethod pm = new PaymentMethod();
                    pm.Initialize(m_nGroupID, nPaymentMethodID, name, description, nBillingMethodID, m_nBillingProcessorID);

                    m_Payments.Add(pm);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        private void GetAllLimitations()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from fr_processors_limitations where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("processor_id", "=", m_nBillingProcessorID);
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

                    Int32 currencyCD = Utils.GetIntSafeVal(ref selectQuery, "Currency_CD", i);

                    DateTime startDate = Utils.GetDateSafeVal(ref selectQuery, "start_date", i);
                    DateTime endDate = Utils.GetDateSafeVal(ref selectQuery, "end_date", i);

                    double minFixPrice = Utils.GetDoubleSafeVal(ref selectQuery, "min_fix_price", i);
                    double maxFixPrice = Utils.GetDoubleSafeVal(ref selectQuery, "max_fix_price", i);

                    Limitations lim = new Limitations();
                    lim.Initialize(m_nGroupID, limitationID, name, description, 0, currencyCD,
                        startDate, endDate, minFixPrice, maxFixPrice);

                    m_Limitations.Add(lim);
                }
            }
            selectQuery.Finish();
            selectQuery = null; 
        }      
    }
}
