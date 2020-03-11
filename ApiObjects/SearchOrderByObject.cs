using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public enum OrderDiretion
    {
        Desc = 0,
        Asc = 1
    }

    public class SearchOrderByObject
    {
        public SearchOrderByObject()
        {
            m_nOrderNum = 0;
            m_eOrderBy = OrderDiretion.Asc;
            m_sOrderField = "";
        }

        public void Initialize(string sField, OrderDiretion eOrderBy, Int32 nOrderNum)
        {
            m_sOrderField = sField;
            m_eOrderBy = eOrderBy;
            m_nOrderNum = nOrderNum;
        }

        public OrderDiretion m_eOrderBy;
        public string m_sOrderField;
        public Int32 m_nOrderNum;
    }
}
