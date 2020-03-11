using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Financial
{
    public abstract class BaseContractsHolder
    {
        protected string m_sName;
        protected string m_sDescription;
        protected Int32 m_nGroupID;

        protected BaseContractsHolder(Int32 nGroupID, string sName, string sDescription)
        {
            m_nGroupID = nGroupID;
            m_sName = sName;
            m_sDescription = sDescription;
        }

        public void Initialize(Int32 nGroupID, string sName, string sDescription)
        {

        }
    }
}
