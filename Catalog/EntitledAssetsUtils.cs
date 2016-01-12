using ApiObjects;
using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catalog
{
    public class EntitledAssetsUtils
    {
        public static List<BaseSearchObject> GetUserSubscriptionSearchObjects(int groupId, string siteGuid)
        {
            List<BaseSearchObject> result = new List<BaseSearchObject>();

            return result;
        }

        internal static Dictionary<eAssetTypes, List<string>> GetFreeAssets(int groupId, string siteGuid)
        {
            throw new NotImplementedException();
        }

        internal static Dictionary<eAssetTypes, List<string>> GetUserPPVAssets(int groupId, string siteGuid)
        {
            throw new NotImplementedException();
        }
    }
}
