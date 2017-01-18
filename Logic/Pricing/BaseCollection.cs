using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ApiObjects;
using System.Data;

namespace Core.Pricing
{
    [Serializable]
    public abstract class BaseCollection
    {
        protected Int32 m_nGroupID;

        protected static readonly string BASE_COLLECTION_LOG_FILE = "BaseCollection";

        protected BaseCollection() { }
        protected BaseCollection(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public int GroupID
        {
            get
            {
                return m_nGroupID;
            }
            protected set
            {
                m_nGroupID = value;
            }
        }

        public abstract Collection GetCollectionData(string sCollectionCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bGetAlsoUnActive);

        public abstract Collection[] GetCollectionsData(string[] oCollCodes, string sCountryCd, string sLanguageCode, string sDeviceName);
    }
}
