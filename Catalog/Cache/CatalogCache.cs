using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CachingProvider;
using DAL;
using TvinciCache;

namespace Catalog.Cache
{
    public class CatalogCache
    {
        public static int GetParentGroup(int nGroupID)
        {
            int nParentGroup = 0;            
            try
            {
                string sKey = "ParentGroupCache_" + nGroupID.ToString();

                nParentGroup = WSCache.Instance.Get<int>(sKey);
                if (nParentGroup == 0)
                {
                    //GetParentGroup
                    nParentGroup = UtilsDal.GetParentGroupID(nGroupID);
                    bool bSet = WSCache.Instance.Add(sKey, nParentGroup);
                }
                return nParentGroup;
            }
            catch (Exception ex)
            {
                return nGroupID;
            }
        }
    }
}
