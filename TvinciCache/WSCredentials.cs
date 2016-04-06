using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using TVinciShared;

namespace TvinciCache
{
    public class WSCredentials
    {
        private static object lckC = new object();
        private static object lckG = new object();

        public static Credentials GetWSCredentials(eWSModules eMainWSModule, int nGroupID, eWSModules eWSModule)
        {
            string key = string.Format("{0}_GetWSCredentials_{1}_{2}", eMainWSModule.ToString(), nGroupID, eWSModule.ToString());

            Credentials uc;

            if (!WSCache.Instance.TryGet<Credentials>(key, out uc))
            {
                lock (lckC)
                {
                    if (!WSCache.Instance.TryGet<Credentials>(key, out uc))
                    {
                        string sUN = string.Empty;
                        string sPass = string.Empty;

                        bool res = TVinciShared.WS_Utils.GetWSCredentials(nGroupID, eWSModule.ToString(), ref sUN, ref sPass);
                        if (res)
                        {
                            uc = new Credentials();
                            uc.m_sUsername = sUN;
                            uc.m_sPassword = sPass;
                            WSCache.Instance.Add(key, uc);
                        }
                        else
                        {
                            uc = new Credentials();
                        }
                    }
                }
            }

            return uc;
        }

        public static int GetGroupID(eWSModules eMainWSModule, Credentials uc)
        {
            string key = string.Format("{0}_GetGroupID_{1}_{2}", eMainWSModule.ToString(), uc.m_sUsername, uc.m_sPassword);

            int nGroupID = 0;

            if (!WSCache.Instance.TryGet<int>(key, out nGroupID))
            {
                lock (lckG)
                {
                    if (!WSCache.Instance.TryGet<int>(key, out nGroupID))
                    {
                        nGroupID = TVinciShared.WS_Utils.GetGroupID(eMainWSModule.ToString(), uc.m_sUsername, uc.m_sPassword);
                        if (nGroupID > 0)
                        {
                            WSCache.Instance.Add(key, nGroupID);
                        }
                    }
                }
            }

            return nGroupID;
        }
    }
}
