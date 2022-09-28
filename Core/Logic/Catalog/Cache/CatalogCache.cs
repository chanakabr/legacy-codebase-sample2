using ApiObjects;
using ApiObjects.Catalog;
using CachingProvider;
using Phx.Lib.Appconfig;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace Core.Catalog.Cache
{
    public interface ICatalogCache
    {
        Dictionary<string, LinearChannelSettings> GetLinearChannelSettings(int groupID, List<string> keys);
        int GetParentGroup(int groupId);
    }

    public class CatalogCache : ICatalogCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Constants
        private static readonly uint DEFAULT_TIME_IN_CACHE_SECONDS = 3600; // 1 hours
        private static readonly double SHORT_IN_CACHE_MINUTES = 10d; // 10 minutes
        private static readonly string DEFAULT_CACHE_NAME = "CatalogCache";
        protected const string CACHE_KEY = "CATALOG";

        #endregion

        #region InnerCache properties
        private static object lck = new object();
        private static object locker = new object();
        private ICachingService CacheService = null;
        private readonly uint dCacheTT;
        private string sKeyCache = string.Empty;
        #endregion

        private static CatalogCache instance = null;


        private string GetCacheName()
        {
            string res = ApplicationConfiguration.Current.CatalogCacheConfiguration.Name.Value;
            if (res.Length > 0)
                return res;
            return DEFAULT_CACHE_NAME;
        }

        private uint GetDefaultCacheTimeInSeconds()
        {
            uint result = (uint)ApplicationConfiguration.Current.CatalogCacheConfiguration.TTLSeconds.Value;

            if (result <= 0)
            {
                result = DEFAULT_TIME_IN_CACHE_SECONDS;
            }

            return result;
        }

        private void InitializeCachingService(string cacheName, uint expirationInSeconds)
        {
            this.CacheService = SingleInMemoryCache.GetInstance(InMemoryCacheType.General, expirationInSeconds);
        }

        public CatalogCache()
        {
            dCacheTT = GetDefaultCacheTimeInSeconds();
            InitializeCachingService(GetCacheName(), dCacheTT);
            sKeyCache = CACHE_KEY; // the key for cache in the inner memory start with CACHE_KEY preffix
        }

        public static CatalogCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new CatalogCache();
                    }
                }
            }
            return instance;
        }

        public int GetParentGroup(int nGroupID)
        {
            int nParentGroup = 0;
            try
            {
                string sKey = "ParentGroupCache_" + nGroupID.ToString();
                object oParent = Get(sKey);
                if (oParent != null)
                {
                    nParentGroup = (int)oParent;
                }
                if (nParentGroup == 0)
                {
                    //GetParentGroup
                    nParentGroup = UtilsDal.GetParentGroupID(nGroupID);
                    bool bSet = Set(sKey, nParentGroup);
                }
                return nParentGroup;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return nGroupID;
            }
        }

        public List<ApiObjects.Ratio> GetGroupRatios(int groupID)
        {
            List<ApiObjects.Ratio> ratios = null;
            try
            {
                string sKey = "GroupRatios_" + groupID.ToString();
                ratios = Get<List<ApiObjects.Ratio>>(sKey);

                if (ratios == null || ratios.Count == 0)
                {
                    // get from DB
                    ratios = CatalogDAL.GetGroupRatios(groupID);

                    if (ratios != null && ratios.Count > 0)
                        Set(sKey, ratios);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting group ratios. GID {0}, ex: {1}", groupID, ex);
            }
            return ratios;
        }

        public List<PicData> GetDefaultImages(int groupID)
        {
            List<PicData> defaultPictures = null;
            try
            {
                string sKey = "GroupDefaultImages_" + groupID.ToString();
                defaultPictures = Get<List<PicData>>(sKey);

                if (defaultPictures == null || defaultPictures.Count == 0)
                {
                    DataRowCollection picsDataRows = CatalogDAL.GetPicsTableData(groupID, eAssetImageType.DefaultPic);

                    if (picsDataRows == null)
                        return null;
                    else
                    {
                        defaultPictures = new List<PicData>();
                        foreach (DataRow row in picsDataRows)
                        {
                            defaultPictures.Add(new PicData()
                            {
                                RatioId = Utils.GetIntSafeVal(row, "RATIO_ID"),
                                Version = Utils.GetIntSafeVal(row, "VERSION"),
                                BaseUrl = Utils.GetStrSafeVal(row, "BASE_URL"),
                                PicId = Utils.GetLongSafeVal(row, "ID"),
                                Ratio = Utils.GetStrSafeVal(row, "RATIO"),
                                GroupId = Utils.GetIntSafeVal(row, "GROUP_ID")
                            });
                        }
                    }

                    if (defaultPictures == null)
                        Set(sKey, new List<PicData>(), SHORT_IN_CACHE_MINUTES);
                    else
                        Set(sKey, defaultPictures);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting group default images. GID {0}, ex: {1}", groupID, ex);
            }
            return defaultPictures;
        }

        public List<PicSize> GetGroupPicSizes(int groupID)
        {
            List<PicSize> picSizes = null;
            try
            {
                string sKey = "GroupPicSizes_" + groupID.ToString();
                picSizes = Get<List<PicSize>>(sKey);

                if (picSizes == null || picSizes.Count == 0)
                {
                    DataRowCollection picsSizeRows = CatalogDAL.GetGroupPicSizesTableData(groupID);

                    if (picsSizeRows == null)
                        return null;
                    else
                    {
                        picSizes = new List<PicSize>();
                        foreach (DataRow row in picsSizeRows)
                        {
                            picSizes.Add(new PicSize()
                            {
                                RatioId = Utils.GetIntSafeVal(row, "RATIO_ID"),
                                Width = Utils.GetIntSafeVal(row, "WIDTH"),
                                Height = Utils.GetIntSafeVal(row, "HEIGHT"),
                                Id = ODBCWrapper.Utils.GetLongSafeVal(row, "ID")
                            });
                        }
                    }

                    if (picSizes == null)
                        Set(sKey, new List<PicSize>(), SHORT_IN_CACHE_MINUTES);
                    else
                        Set(sKey, picSizes);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting group picture sizes. GID {0}, ex: {1}", groupID, ex);
            }
            return picSizes;
        }

        public Dictionary<long, List<PicSize>> GetGroupRatioIdToPicSizeMapping(int groupID)
        {
            Dictionary<long, List<PicSize>> groupRatioIdToPicSize = null;
            try
            {
                string sKey = "GroupRatioIdToPicSizeMapping_" + groupID.ToString();
                groupRatioIdToPicSize = Get<Dictionary<long, List<PicSize>>>(sKey);

                if (groupRatioIdToPicSize == null)
                {
                    groupRatioIdToPicSize = new Dictionary<long, List<PicSize>>();
                    DataRowCollection picsSizeRows = CatalogDAL.GetGroupPicSizesTableData(groupID);
                    if (picsSizeRows == null)
                    {
                        return groupRatioIdToPicSize;
                    }

                    foreach (DataRow row in picsSizeRows)
                    {
                        var picSize = new PicSize()
                        {
                            RatioId = Utils.GetIntSafeVal(row, "RATIO_ID"),
                            Width = Utils.GetIntSafeVal(row, "WIDTH"),
                            Height = Utils.GetIntSafeVal(row, "HEIGHT"),
                            Id = ODBCWrapper.Utils.GetLongSafeVal(row, "ID")
                        };

                        if (groupRatioIdToPicSize.ContainsKey(picSize.RatioId))
                        {
                            groupRatioIdToPicSize[picSize.RatioId].Add(picSize);
                        }
                        else
                        {
                            groupRatioIdToPicSize.Add(picSize.RatioId, new List<PicSize>() { picSize });
                        }
                    }

                    Set(sKey, groupRatioIdToPicSize);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in GetGroupRatioIdToPicSizeMapping. groupID:{0}.", groupID), ex);
            }

            return groupRatioIdToPicSize;
        }

        public object Get(string sKey)
        {
            sKey = string.Format("{0}{1}", sKeyCache, sKey);
            BaseModuleCache bModule = CacheService.Get(sKey);
            if (bModule != null)
                return bModule.result;

            return null;
        }

        public T Get<T>(string sKey) where T : class
        {
            sKey = string.Format("{0}{1}", sKeyCache, sKey);
            return CacheService.Get<T>(sKey);
        }

        public bool Set(string sKey, object oValue)
        {
            return Set(sKey, oValue, dCacheTT);
        }

        public bool Set(string sKey, object oValue, double dCacheTime)
        {
            sKey = string.Format("{0}{1}", sKeyCache, sKey);
            BaseModuleCache bModule = new BaseModuleCache(oValue);
            return CacheService.Set(sKey, bModule, dCacheTime);
        }

        public Dictionary<string, LinearChannelSettings> GetLinearChannelSettings(int groupID, List<string> keys)
        {
            var linearChannelSettings = new Dictionary<string, LinearChannelSettings>();
            try
            {
                List<int> missingsKeys = null;
                LinearChannelSettings linear = null;
                List<string> fullKeys = keys.Distinct().Select(k => (string.Format("LinearChannelSettings_{0}_{1}", groupID, k))).ToList();
                Dictionary<string, object> values = GetValues(fullKeys);
                if (values != null && values.Count > 0)
                {
                    foreach (KeyValuePair<string, object> pair in values)
                    {
                        if (!string.IsNullOrEmpty(pair.Key) && pair.Value != null)
                        {
                            linear = (LinearChannelSettings)pair.Value;
                            if (linear != null)
                            {
                                linearChannelSettings.Add(linear.ChannelID, linear);
                            }
                        }
                    }
                    // complete missing channel => get list of missings channels
                    missingsKeys = keys.Except(linearChannelSettings.Keys).Select(k => int.Parse(k)).ToList<int>();
                }
                else
                {
                    missingsKeys = keys.Select(k => int.Parse(k)).ToList<int>();
                }

                if (missingsKeys != null && missingsKeys.Count > 0)
                {
                    //get from DB
                    var linearChannelSettingsFromDb = CatalogDAL.Instance.GetLinearChannelSettings(groupID, missingsKeys);
                    foreach (var linearFromDb in linearChannelSettingsFromDb)
                    {
                        string sKey = string.Format("LinearChannelSettings_{0}_{1}", groupID, linearFromDb.ChannelID);
                        if (linearFromDb != null && !linearChannelSettings.ContainsKey(linearFromDb.ChannelID))
                        {
                            linearChannelSettings.Add(linearFromDb.ChannelID, linearFromDb);
                            Set(sKey, linearFromDb);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error whileGetLinearChannelSettings. groupID {0}, ex: {1}", groupID, ex);
                return null;
            }
            return linearChannelSettings;
        }

        private Dictionary<string, object> GetValues(List<string> keys)
        {
            Dictionary<string, object> values = null;
            if (keys != null && keys.Count > 0)
            {
                keys = keys.Select(k => (string.Format("{0}{1}", sKeyCache, k))).ToList();
                values = this.CacheService.GetValues(keys) as Dictionary<string, object>;
            }

            return values;
        }

        public Dictionary<int, string> GetGroupWatchPermissionsTypes(int groupID)
        {
            Dictionary<int, string> watchPermissionsTypes = null;
            try
            {
                string sKey = "GroupWatchPermissionsTypes_" + groupID.ToString();
                watchPermissionsTypes = Get<Dictionary<int, string>>(sKey);

                if (watchPermissionsTypes == null || watchPermissionsTypes.Count == 0)
                {
                    DataTable dataTable = CatalogDAL.GetGroupWatchPermissionsTypes(groupID);

                    if (dataTable == null || dataTable.Rows?.Count == 0)
                        return null;
                    else
                    {
                        watchPermissionsTypes = new Dictionary<int, string>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            watchPermissionsTypes.Add(Utils.GetIntSafeVal(row, "ID"), Utils.GetStrSafeVal(row, "NAME"));
                        }
                    }

                    if (watchPermissionsTypes == null)
                        Set(sKey, new Dictionary<int, string>(), SHORT_IN_CACHE_MINUTES);
                    else
                        Set(sKey, watchPermissionsTypes);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting group watch permissions types. GID {0}, ex: {1}", groupID, ex);
            }
            return watchPermissionsTypes;
        }

        internal Dictionary<int, string> GetMediaQualities()
        {
            Dictionary<int, string> mediaQualities = null;
            try
            {
                string sKey = "MediaQualities_0";
                mediaQualities = Get<Dictionary<int, string>>(sKey);

                if (mediaQualities == null || mediaQualities.Count == 0)
                {
                    DataTable dataTable = CatalogDAL.GetMediaQualities();

                    if (dataTable == null || dataTable.Rows.Count == 0)
                        return null;
                    else
                    {
                        mediaQualities = new Dictionary<int, string>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            mediaQualities.Add(Utils.GetIntSafeVal(row, "ID"), Utils.GetStrSafeVal(row, "NAME"));
                        }
                    }

                    if (mediaQualities == null)
                        Set(sKey, new Dictionary<int, string>(), SHORT_IN_CACHE_MINUTES);
                    else
                        Set(sKey, mediaQualities);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting mediaQualities. ex: {0}", ex);
            }
            return mediaQualities;
        }

        public static void ClearAll()
        {
            if (instance != null)
            {
                instance = null;
            }
        }

    }
}
