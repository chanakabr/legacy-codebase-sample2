using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;

namespace Financial
{
    public class ContractFamily
    {
        public Int32 m_nGroupID;
        public Int32 m_nContractFamilyID;
        public Int32 m_nParentEntityID;

        public string m_sName;
        public string m_sDescription;

        public ContractFamily()
        {
            m_nGroupID = 0;
            m_nContractFamilyID = 0;
            m_nParentEntityID = 0;

            m_sName = string.Empty;
            m_sDescription = string.Empty;
        }

        public void Initialize(Int32 nGroupID, Int32 nContractFamilyID, Int32 nParentEntityID, string sName, string sDescription)
        {
            m_nGroupID = nGroupID;
            m_nContractFamilyID = nContractFamilyID;
            m_nParentEntityID = nParentEntityID;

            m_sName = sName;
            m_sDescription = sDescription;
        }
    }
}
