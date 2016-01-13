using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CachingProvider;
using DAL;
using TvinciCache;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using Tvinci.Core.DAL;
using System.Data;

namespace Catalog.Cache
{
    public class CatalogCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Constants
        private static readonly double DEFAULT_TIME_IN_CACHE_MINUTES = 60d; // 1 hours
        private static readonly double SHORT_IN_CACHE_MINUTES = 10d; // 10 minutes
        private static readonly string DEFAULT_CACHE_NAME = "CatalogCache";
        protected const string CACHE_KEY = "CATALOG";
        #endregion

        #region InnerCache properties
        private static object locker = new object();
        private ICachingService CacheService = null;
        private readonly double dCacheTT;
        private string sKeyCache = string.Empty;
        #endregion

        private static CatalogCache instance = null;


        private string GetCacheName()
        {
            string res = TVinciShared.WS_Utils.GetTcmConfigValue("CACHE_NAME");
            if (res.Length > 0)
                return res;
            return DEFAULT_CACHE_NAME;
        }

        private double GetDefaultCacheTimeInMinutes()
        {
            double res = 0d;
            string timeStr = TVinciShared.WS_Utils.GetTcmConfigValue("CACHE_TIME_IN_MINUTES");
            if (timeStr.Length > 0 && Double.TryParse(timeStr, out res) && res > 0)
                return res;
            return DEFAULT_TIME_IN_CACHE_MINUTES;
        }

        private void InitializeCachingService(string cacheName, double cachingTimeMinutes)
        {
            this.CacheService = new SingleInMemoryCache(cacheName, cachingTimeMinutes);
        }

        private CatalogCache()
        {
            dCacheTT = GetDefaultCacheTimeInMinutes();
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

        public List<Ratio> GetGroupRatios(int groupID)
        {
            List<Ratio> ratios = null;
            try
            {
                string sKey = "GroupRatios_" + groupID.ToString();
                ratios = Get<List<Ratio>>(sKey);

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
                                Height = Utils.GetIntSafeVal(row, "HEIGHT")
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

    }
}
