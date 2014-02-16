using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVinciShared
{
    public class UMObj : IComparable
    {
        public string m_id;
        public string m_title;
        public string m_description;
        public bool m_isBelongToSub;
        public int m_orderNum;

        public UMObj(string id, string title, string desc, bool isBelong, int orderNum)
        {
            m_id = id;
            m_title = title;
            m_description = desc;
            m_isBelongToSub = isBelong;
            m_orderNum = orderNum;
        }

        public int CompareTo(object obj)
        {
            int retVal = -1;
            if (obj is UMObj)
            {
                UMObj otherObj = obj as UMObj;
                return (this.m_orderNum.CompareTo(otherObj.m_orderNum));
            }
            return retVal;
        }
    }
}
