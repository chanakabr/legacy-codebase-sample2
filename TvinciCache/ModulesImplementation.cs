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
                    if (!WSCache.Instance.TryGet<int>(key, out nImplID))
                    {
                        nImplID = WS_Utils.GetModuleImplID(nGroupID, nModuleID, connectionKey);
                        WSCache.Instance.Add(key, nImplID);
                    }
                }
            }

            return nImplID;
        }

        public static int GetModuleIDAndName(eWSModules eMainWSModule, int nGroupID, int nModuleID, string connectionKey = "")
        {
            int nImplID = 0;

            string key = string.Format("{0}_GetModuleID_{1}_{2}", eMainWSModule, nGroupID, nModuleID);

            if ((nImplID = WSCache.Instance.Get<int>(key)) == 0)
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

        public static string GetModuleName(eWSModules eMainWSModule, int nGroupID, int nModuleID, int operatorId = -1)
        {
            string moduleName = string.Empty;

            string key = string.Format("{0}_GetModuleName_{1}_{2}", eMainWSModule, nGroupID, nModuleID);

            if (string.IsNullOrEmpty(moduleName = WSCache.Instance.Get<string>(key)))
            {
                lock (lck)
                {
                    if (string.IsNullOrEmpty(moduleName = WSCache.Instance.Get<string>(key)))
                    {
                        moduleName = WS_Utils.GetModuleImplName(nGroupID, nModuleID, operatorId);

                        if (!string.IsNullOrEmpty(moduleName))
                            WSCache.Instance.Add(key, moduleName);
                    }
                }
            }

            return moduleName;
        }
    }
}
