using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ConditionalAccess
{
  
    [Serializable]
    public class CachingDataResponse : ISerializable
    {
        #region Properties
        private string m_key;
        private object m_oVal;
        private DateTime m_dStart;
        private DateTime m_dLastUsed;
        private Int32 m_nUseCounter;
        private Int32 m_nMediaID;
        private Int32 m_nCacheSecs;
        private bool m_bToRenew;
        private System.Web.Caching.CacheItemPriority m_Priority;
        #endregion

#region Get/Set
        public String Key
        {
            get { return m_key; }
            set { m_key = value; }
        }
        public object OVal
        {
            get { return m_oVal; }
            set { m_oVal = value; }
        }
        public DateTime DStart
        {
            get { return m_dStart; }
            set { m_dStart = value; }
        }
        public DateTime DLastUsed
        {
            get { return m_dLastUsed; }
            set { m_dLastUsed = value; }
        }
        public Int32 NUseCounter
        {
            get { return m_nUseCounter; }
            set { m_nUseCounter = value; }
        }
        public Int32 NMediaID
        {
            get { return m_nMediaID; }
            set { m_nMediaID = value; }
        }
        public Int32 NCacheSecs
        {
            get { return m_nCacheSecs; }
            set { m_nCacheSecs = value; }
        }
        public bool BToRenew
        {
            get { return m_bToRenew; }
            set { m_bToRenew = value; }
        }

        public System.Web.Caching.CacheItemPriority Priority
        {
            get { return m_Priority; }
            set { m_Priority = value; }
        }       
#endregion
        #region Constructor
        public CachingDataResponse()
        {
            m_key = "";
            m_oVal = null;
            m_nMediaID = 0;
            m_bToRenew = false;
            m_dStart = DateTime.MinValue;
            m_dLastUsed = DateTime.MaxValue;
            m_nUseCounter = 0;
            m_nCacheSecs = 0;
            m_Priority = System.Web.Caching.CacheItemPriority.Default;
        }

        /*Constructor that get all the values and fill */       
        public CachingDataResponse(string key, object oVal, Int32 nMediaID, bool bToRenew, Int32 nCacheSecs, System.Web.Caching.CacheItemPriority priority)
        {
            m_key = key;
            m_oVal = oVal;
            m_nMediaID = nMediaID;
            m_bToRenew = bToRenew;
            m_dStart = DateTime.UtcNow;
            m_dLastUsed = DateTime.UtcNow;
            m_nUseCounter = 1;
            m_nCacheSecs = nCacheSecs;
            m_Priority = priority;
        }

        #endregion
        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
           // info.AddValue("m_oVal", "3333");
        }

        #endregion
    }
}
