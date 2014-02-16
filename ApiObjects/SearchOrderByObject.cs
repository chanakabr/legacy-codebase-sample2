using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public enum OrderBy
    {
        Desc = 0,
        Asc = 1
    }

    public class SearchOrderByObject
    {
        public SearchOrderByObject()
        {
            m_nOrderNum = 0;
            m_eOrderBy = OrderBy.Asc;
            m_sOrderField = "";
        }

        public void Initialize(string sField, OrderBy eOrderBy, Int32 nOrderNum)
        {
            m_sOrderField = sField;
            m_eOrderBy = eOrderBy;
            m_nOrderNum = nOrderNum;
        }

        public OrderBy m_eOrderBy;
        public string m_sOrderField;
        public Int32 m_nOrderNum;
    }
}
