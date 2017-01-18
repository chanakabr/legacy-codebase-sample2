using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;
using ApiObjects;

namespace Core.Social
{
    public class FacebookManager
    {
        private Dictionary<int, FacebookConfig> m_FBCInstances = null;

        private static ReaderWriterLockSlim m_FBLocker = new ReaderWriterLockSlim();

        private static FacebookManager m_fbManager = null;

        private static object syncObj = new object();

        //Get instance by group ID and platform
        public static FacebookManager GetInstance
        {
            get
            {
                if (m_fbManager == null)
                {
                    lock (syncObj)
                    {
                        if (m_fbManager == null)
                        {
                            m_fbManager = new FacebookManager();
                        }
                    }
                }

                return m_fbManager;
            }
        }

        #region C'tor
        private FacebookManager()
        {
        }
        #endregion

        public FacebookConfig GetFacebookConfigInstance(int groupID)
        {
            FacebookConfig tempFBC = null;

            if (m_FBCInstances == null)
            {
                m_FBCInstances = new Dictionary<int, FacebookConfig>();
            }

            //If this is the first time a group ID is used - initialize a new manager and all relevent objects

            if (m_FBLocker.TryEnterWriteLock(1000))
            {
                try
                {
                    if (!m_FBCInstances.ContainsKey(groupID))
                    {
                        BaseSocialBL oSocialBL = BaseSocialBL.GetBaseSocialImpl(groupID) as BaseSocialBL;
                        FacebookConfig config = oSocialBL.GetFBConfig();
                        if (config != null && !string.IsNullOrEmpty(config.sFBKey))
                        {
                            m_FBCInstances.Add(groupID, oSocialBL.GetFBConfig());
                        }
                    }
                }
                catch
                {
                    //logger.ErrorFormat("GetSiteMapInstance->", ex);
                }
                finally
                {
                    m_FBLocker.ExitWriteLock();
                }
            }


            // If item already exist

            if (m_FBLocker.TryEnterReadLock(1000))
            {
                try
                {
                    m_FBCInstances.TryGetValue(groupID, out tempFBC);
                }
                catch 
                {
                    //logger.Error("GetSiteMapInstance->", ex);
                }
                finally
                {
                    m_FBLocker.ExitReadLock();
                }
            }

            return tempFBC;
        }
    }
}
