using ApiObjects;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OPC_Migration
{

    public static class Utils
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string assetsPageSizeFromConfig = System.Configuration.ConfigurationManager.AppSettings["ASSETS_PAGE_SIZE"];
        private static readonly string bulkIdsUpdateSizeFromConfig = System.Configuration.ConfigurationManager.AppSettings["BULK_IDS_UPDATE_SIZE"];

        private const int DEFAULT_ASSETS_PAGE_SIZE = 1000;
        private const int DEFAULT_BULK_IDS_UPDATE_SIZE = 1000;
        public const long UPDATING_USER_ID = 999999;
        public const string META_DOUBLE_SUFFIX = "_DOUBLE";
        public const string META_BOOL_SUFFIX = "_BOOL";
        public const string META_STR_SUFFIX = "_STR";
        public const string META_DATE_PREFIX = "date";
        public const string IS_RELATED_PREFIX = "IS_";
        public const string IS_RELATED_SUFFIX = "_RELATED";
        public const string IS_RELATED_TAG_COLUMN = "IS_RELATED";
        public const string PROGRAM_ASSET_STRUCT = "Program";

        public static bool ClearAllCaches(int groupId)
        {
            bool result = true;
            try
            {
                result = ClearInMemoryCache() && ClearLayeredCache(groupId);
            }
            catch (Exception ex)
            {
                log.Error("Failed ClearAllCaches", ex);
                result = false;
            }

            return result;
        }

        public static bool ClearInMemoryCache()
        {
            bool result = true;
            try
            {
                CachingManager.CachingManager.RemoveFromCache("");
                TvinciCache.WSCache.ClearAll();
                Core.Catalog.Cache.CatalogCache.ClearAll();                
            }
            catch (Exception ex)
            {
                log.Error("Failed ClearInMemoryCache", ex);
                result = false;
            }

            return result;
        }

        public static bool ClearLayeredCache(int groupId)
        {
            bool result = true;
            try
            {
                result = CachingProvider.LayeredCache.LayeredCache.Instance.IncrementLayeredCacheGroupConfigVersion(groupId);
            }
            catch (Exception ex)
            {
                log.Error("Failed ClearLayeredCache caches", ex);
                result = false;
            }

            return result;
        }

        public static string GetSignature(string signString)
        {
            string retVal;
            //Get key from DB
            string hmacSecret = ApplicationConfiguration.Current.WebServicesConfiguration.Catalog.SignatureKey.Value;
            // The HMAC secret as configured in the skin
            // Values are always transferred using UTF-8 encoding
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Calculate the HMAC
            // signingString is the SignString from the request
            System.Security.Cryptography.HMACSHA1 myhmacsha1 = new System.Security.Cryptography.HMACSHA1(encoding.GetBytes(hmacSecret));
            retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signString)));
            myhmacsha1.Clear();
            return retVal;
        }

        public static MetaType GetTopicType(string metaType)
        {
            MetaType result = MetaType.All;
            if (metaType.Contains(META_STR_SUFFIX))
            {
                result = MetaType.MultilingualString;
            }
            else if (metaType.Contains(META_BOOL_SUFFIX))
            {
                result = MetaType.Bool;
            }
            else if (metaType.Contains(META_DOUBLE_SUFFIX))
            {
                result = MetaType.Number;
            }
            else if (metaType.StartsWith(META_DATE_PREFIX))
            {
                result = MetaType.DateTime;
            }
            else
            {
                log.ErrorFormat("Invalid metaType: {0}", metaType);
            }

            return result;
        }

        public static bool CheckIfMetaIsSearchRelated(string metaType, DataRow groupDr)
        {
            bool result = false;
            try
            {
                string columnName = string.Format("{0}{1}{2}", IS_RELATED_PREFIX, metaType, IS_RELATED_SUFFIX);
                result = ODBCWrapper.Utils.GetIntSafeVal(groupDr, columnName, 0) == 1;
            }
            catch (Exception ex)
            {
                log.Error("Failed CheckIfMetaIsSearchRelated", ex);
            }

            return result;
        }

        public static int GetAssetsPageSize()
        {
            int result = 0;
            if (string.IsNullOrEmpty(assetsPageSizeFromConfig) || !int.TryParse(assetsPageSizeFromConfig, out result) || result <= DEFAULT_ASSETS_PAGE_SIZE)
            {
                result = DEFAULT_ASSETS_PAGE_SIZE;
            }

            return result;
        }

        public static int GetBulkIdsUpdateSize()
        {
            int result = 0;
            if (string.IsNullOrEmpty(bulkIdsUpdateSizeFromConfig) || !int.TryParse(bulkIdsUpdateSizeFromConfig, out result) || result <= DEFAULT_BULK_IDS_UPDATE_SIZE)
            {
                result = DEFAULT_BULK_IDS_UPDATE_SIZE;
            }

            return result;
        }

        public static void TrimEndSpaceFromGroup(GroupsCacheManager.Group group)
        {
            List<KeyValuePair<int, string>> tagsToRemoveSpaces = group.m_oGroupTags.Where(x => x.Value.EndsWith(" ")).ToList();
            if (tagsToRemoveSpaces?.Count > 0)
            {
                foreach (KeyValuePair<int, string> tagToUpdate in tagsToRemoveSpaces)
                {
                    group.m_oGroupTags[tagToUpdate.Key] = tagToUpdate.Value.TrimEnd();
                }
            }

            Dictionary<int, Dictionary<string, string>> metasToRemoveSpaces = new Dictionary<int, Dictionary<string, string>>();
            foreach (KeyValuePair<int, Dictionary<string, string>> groupMetas in group.m_oMetasValuesByGroupId)
            {
                List<KeyValuePair<string, string>> groupMetasToRemoveSpaces = groupMetas.Value.Where(x => x.Value.EndsWith(" ")).ToList();
                if (groupMetasToRemoveSpaces?.Count > 0)
                {
                    metasToRemoveSpaces.Add(groupMetas.Key, new Dictionary<string, string>());
                    foreach (KeyValuePair<string, string> metaToUpdate in groupMetasToRemoveSpaces)
                    {
                        string trimmedValue = metaToUpdate.Value.TrimEnd();
                        metasToRemoveSpaces[groupMetas.Key][metaToUpdate.Key] = trimmedValue;
                    }
                }
            }

            if (metasToRemoveSpaces?.Count > 0)
            {
                foreach (KeyValuePair<int, Dictionary<string, string>> valuesToUpdate in metasToRemoveSpaces)
                {
                    foreach (KeyValuePair<string, string> metasToUpdate in valuesToUpdate.Value)
                    {
                        group.m_oMetasValuesByGroupId[valuesToUpdate.Key][metasToUpdate.Key] = metasToUpdate.Value;
                    }
                }
            }

            List<string> epgsMetas = group.m_oEpgGroupSettings.MetasDisplayName.Where(x => x.EndsWith(" ")).ToList();
            if (epgsMetas?.Count > 0)
            {
                foreach (string epgsMetasToUpdate in epgsMetas)
                {
                    group.m_oEpgGroupSettings.MetasDisplayName.Remove(epgsMetasToUpdate);
                    group.m_oEpgGroupSettings.MetasDisplayName.Add(epgsMetasToUpdate.TrimEnd());
                }
            }

            List<string> epgsTags = group.m_oEpgGroupSettings.TagsDisplayName.Where(x => x.EndsWith(" ")).ToList();
            if (epgsTags?.Count > 0)
            {
                foreach (string epgsTagsToUpdate in epgsTags)
                {
                    group.m_oEpgGroupSettings.TagsDisplayName.Remove(epgsTagsToUpdate);
                    group.m_oEpgGroupSettings.TagsDisplayName.Add(epgsTagsToUpdate.TrimEnd());
                }
            }
        }

    }
}
