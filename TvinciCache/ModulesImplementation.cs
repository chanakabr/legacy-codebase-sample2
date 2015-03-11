using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using TVinciShared;

namespace TvinciCache
{
    public class ModulesImplementation
    {
        private static object lck = new object();

        public static int GetModuleID(eWSModules eMainWSModule, int nGroupID, int nModuleID, string connectionKey = "")
        {
            int nImplID = 0;
            string key = string.Format("{0}_GetModuleID_{1}_{2}", eMainWSModule, nGroupID, nModuleID);            
             
            if (!WSCache.Instance.TryGet<int>(key, out nImplID))
            {
                lock (lck)
                {
                    if ((nImplID = WSCache.Instance.Get<int>(key)) == 0)
                    {
                        nImplID = WS_Utils.GetModuleImplID(nGroupID, nModuleID, connectionKey);
                        WSCache.Instance.Add(key, nImplID);
                    }
                }
            }

            return nImplID;
        }
    }
}
