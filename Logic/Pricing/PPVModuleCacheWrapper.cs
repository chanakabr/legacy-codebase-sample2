using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Core.Pricing
{
    /*
     * 1. This class uses a decorator in order to wrap the BasePPVModule class. Understand Decorator Design Pattern before you change anything.
     * 2. Its main functionality is to add caching mechanism to Pricing methods uses by the Conditional Access module.
     * 3. Methods not called by CAS do not cache their results right now (September 2014).
     * 
     */ 
    public class PPVModuleCacheWrapper : BasePPVModuleDecorator
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected static readonly string PPV_DATA_CACHE_NAME = "ppv_data";
        protected static readonly string PPV_CACHE_WRAPPER_LOG_FILE = "PPVModuleCacheWrapper";
        protected static readonly string PPV_MEDIA_FILE_CACHE_NAME = "ppv_media_file";
        protected static readonly string PPV_MEDIA_FILE_EXPIRY_CACHE_NAME = "ppv_media_file_expiry";

        public PPVModuleCacheWrapper(BasePPVModule originalBasePPVModule)
            : base(originalBasePPVModule)
        {

        }

        #region Public methods with cache support

        public override MediaFilePPVContainer[] GetPPVModuleListForMediaFilesWithExpiry(int[] nMediaFileIDs, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            if (nMediaFileIDs != null && nMediaFileIDs.Length > 0)
            {
                Dictionary<int, int> mediaFilesToIndexMapping = new Dictionary<int, int>();
                List<int> uncachedMediaFilesPPVModules = new List<int>();
                SortedSet<SortedMediaFilePPVContainer> set = new SortedSet<SortedMediaFilePPVContainer>();
                for (int i = 0; i < nMediaFileIDs.Length; i++)
                {
                    if (nMediaFileIDs[i] < 1 || mediaFilesToIndexMapping.ContainsKey(nMediaFileIDs[i]))
                        continue;
                    string cacheKey = GetPPVModuleListForMediaFilesCacheKey(nMediaFileIDs[i], true);
                    MediaFilePPVContainer temp = null;
                    if (PricingCache.TryGetMediaFilePPVContainer(cacheKey, out temp) && temp != null)
                    {
                        set.Add(new SortedMediaFilePPVContainer(temp, i));
                    }
                    else
                    {
                        uncachedMediaFilesPPVModules.Add(nMediaFileIDs[i]);
                        mediaFilesToIndexMapping.Add(nMediaFileIDs[i], i);
                    }
                } // for

                if (uncachedMediaFilesPPVModules.Count > 0)
                {
                    // fetch uncached data from the correct instance of BasePPVModule
                    MediaFilePPVContainer[] arrOfUncachedData = originalBasePPVModule.Get_PPVModuleForMediaFiles(uncachedMediaFilesPPVModules.ToArray(), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);                     
                    if (arrOfUncachedData != null && arrOfUncachedData.Length > 0)
                    {
                        for (int i = 0; i < arrOfUncachedData.Length; i++)
                        {
                            if (arrOfUncachedData[i] != null && arrOfUncachedData[i].m_nMediaFileID > 0
                                && mediaFilesToIndexMapping.ContainsKey(arrOfUncachedData[i].m_nMediaFileID))
                            {
                                string cacheKey = GetPPVModuleListForMediaFilesCacheKey(arrOfUncachedData[i].m_nMediaFileID, true);
                                if (!PricingCache.TryAddMediaFilePPVContainer(cacheKey, arrOfUncachedData[i]))
                                {
                                    PricingCache.LogCachingError("Failed to insert entry into cache. ", cacheKey, arrOfUncachedData[i],
                                        "GetPPVModuleListForMediaFilesWithExpiry", PPV_CACHE_WRAPPER_LOG_FILE);
                                }
                                set.Add(new SortedMediaFilePPVContainer(arrOfUncachedData[i], mediaFilesToIndexMapping[arrOfUncachedData[i].m_nMediaFileID]));
                            }
                        } // for
                    }
                }

                return set.Select((item) => item.MediaFilePPVContainerObj).ToArray<MediaFilePPVContainer>();
            }

            return null;
        }

        public override MediaFilePPVModule[] GetPPVModuleListForMediaFiles(int[] nMediaFileIDs, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            if (nMediaFileIDs != null && nMediaFileIDs.Length > 0)
            {
                Dictionary<int, int> mediaFilesToIndexMapping = new Dictionary<int, int>();
                List<int> uncachedMediaFilesPPVModules = new List<int>();
                SortedSet<SortedMediaFilePPVModule> set = new SortedSet<SortedMediaFilePPVModule>();
                for (int i = 0; i < nMediaFileIDs.Length; i++)
                {
                    if (nMediaFileIDs[i] < 1 || mediaFilesToIndexMapping.ContainsKey(nMediaFileIDs[i]))
                        continue;
                    string cacheKey = GetPPVModuleListForMediaFilesCacheKey(nMediaFileIDs[i], false);
                    MediaFilePPVModule temp = null;
                    if (PricingCache.TryGetMediaFilePPVModuleObj(cacheKey, out temp) && temp != null)
                    {
                        set.Add(new SortedMediaFilePPVModule(temp, i));
                    }
                    else
                    {
                        uncachedMediaFilesPPVModules.Add(nMediaFileIDs[i]);
                        mediaFilesToIndexMapping.Add(nMediaFileIDs[i], i);
                    }
                } // for

                if (uncachedMediaFilesPPVModules.Count > 0)
                {
                    MediaFilePPVModule[] arrOfUncachedData = originalBasePPVModule.GetPPVModuleListForMediaFiles(uncachedMediaFilesPPVModules.ToArray<int>(), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (arrOfUncachedData != null && arrOfUncachedData.Length > 0)
                    {
                        for (int i = 0; i < arrOfUncachedData.Length; i++)
                        {
                            if (arrOfUncachedData[i] != null && arrOfUncachedData[i].m_nMediaFileID > 0 &&
                                mediaFilesToIndexMapping.ContainsKey(arrOfUncachedData[i].m_nMediaFileID))
                            {
                                string cacheKey = GetPPVModuleListForMediaFilesCacheKey(arrOfUncachedData[i].m_nMediaFileID, false);
                                if (!PricingCache.TryAddMediaFilePPVModuleObj(cacheKey, arrOfUncachedData[i]))
                                {
                                    PricingCache.LogCachingError("failed to insert entry into cache. ", cacheKey,
                                        arrOfUncachedData[i], "GetPPVModuleListForMediaFiles", PPV_CACHE_WRAPPER_LOG_FILE);
                                }
                                set.Add(new SortedMediaFilePPVModule(arrOfUncachedData[i], mediaFilesToIndexMapping[arrOfUncachedData[i].m_nMediaFileID]));

                            }
                        } // for
                    }
                }

                return set.Select((item) => item.MediaFilePPVModuleObj).ToArray<MediaFilePPVModule>();
            }

            return null;
        }

        public override PPVModule GetPPVModuleData(string sPPVModuleCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PPVModule res = null;
            if (!string.IsNullOrEmpty(sPPVModuleCode))
            {
                string cacheKey = GetPPVDataCacheKey(sPPVModuleCode);
                PPVModule temp = null;
                if (PricingCache.TryGetPPVModule(cacheKey, out temp) && temp != null)
                    return temp;
                res = originalBasePPVModule.GetPPVModuleData(sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (res != null)
                {
                    if (!PricingCache.TryAddPPVModule(cacheKey, res))
                    {
                        PricingCache.LogCachingError("Failed to insert entry into cache. ", cacheKey,
                            res, "GetPPVModuleData", PPV_CACHE_WRAPPER_LOG_FILE);
                    }
                }
            }

            return res;
        }

        public override PPVModuleDataResponse GetPPVModuleDataResponse(string sPPVModuleCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PPVModuleDataResponse result = new PPVModuleDataResponse();
            try
            {
                result.PPVModule = GetPPVModuleData(sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (result.PPVModule != null)
                {
                    result.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    result.Status = new ApiObjects.Response.Status((int)eResponseStatus.ModuleNotExists, eResponseStatus.ModuleNotExists.ToString());
                }

            }
            catch (Exception ex)
            {
                result.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPPVModuleDataResponse. ");
                sb.Append(String.Concat(" GroupID: ", m_nGroupID));
                sb.Append(String.Concat(" PPVModuleCode: ", sPPVModuleCode));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            return result;
        }

        public override PPVModule[] GetPPVModulesData(string[] sPPVModuleCodes, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PPVModule[] ppvModules = null;
            if (sPPVModuleCodes != null && sPPVModuleCodes.Length > 0)
            {
                Dictionary<string, PPVModule> PPVModulesMapping = new Dictionary<string,PPVModule>();
                List<string> uncachedPPVs = new List<string>(sPPVModuleCodes);
                List<string> ppvModulesCacheKeys = GetPPVModulesCacheKey(sPPVModuleCodes);
                if (ppvModulesCacheKeys != null && ppvModulesCacheKeys.Count > 0)
                {
                    PPVModulesMapping = PricingCache.TryGetPPVmodules(ppvModulesCacheKeys);
                    if (PPVModulesMapping != null && PPVModulesMapping.Count != ppvModulesCacheKeys.Count)
                    {
                        foreach (string ppvModuleKey in PPVModulesMapping.Keys)
                        {
                            uncachedPPVs.Remove(ppvModuleKey);
                        }
                    }
                    else
                    {
                        PPVModulesMapping = new Dictionary<string, PPVModule>();
                    }
                }

                if (uncachedPPVs.Count > 0)
                {
                    PPVModule[] unCachedPPVModules = originalBasePPVModule.GetPPVModulesData(uncachedPPVs.ToArray(), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    if (unCachedPPVModules != null && unCachedPPVModules.Length > 0)
                    {
                        // add to mappings dictionary and try to add to cache

                        Dictionary<string, PPVModule> ppvModulesCachedKeyMappings = GetPPVModulesCacheKeyMappings(unCachedPPVModules.ToList());

                        if(ppvModulesCachedKeyMappings != null && ppvModulesCachedKeyMappings.Count > 0)
                        {
                            foreach (KeyValuePair<string, PPVModule> pair in ppvModulesCachedKeyMappings)
                            {
                                if (!PricingCache.TryAddPPVModule(pair.Key, pair.Value))
                                {
                                    PricingCache.LogCachingError("Failed to insert entry into cache. ", pair.Key, pair.Value, "GetPPVModulesData", PPV_CACHE_WRAPPER_LOG_FILE);
                                }

                                if (PPVModulesMapping != null && !PPVModulesMapping.ContainsKey(pair.Key))
                                {
                                    PPVModulesMapping.Add(pair.Key, pair.Value);
                                }
                            }
                        }
                    }
                }

                ppvModules = PPVModulesMapping.Values.ToArray();
            }

            return ppvModules;
        }

        #endregion

        #region Public methods with no cache support

        public override PPVModuleContainer[] GetPPVModuleListForAdmin(int nMediaFileID, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return this.originalBasePPVModule.GetPPVModuleListForAdmin(nMediaFileID, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }

        public override PPVModule[] GetPPVModuleList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return this.originalBasePPVModule.GetPPVModuleList(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }

        public override PPVModule[] GetPPVModuleShrinkList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return this.originalBasePPVModule.GetPPVModuleShrinkList(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }

        #endregion

        #region Private methods

        private string GetPPVModuleListForMediaFilesCacheKey(int mediaFileID, bool withExpiry)
        {
            return String.Concat(this.originalBasePPVModule.GroupID, "_", withExpiry ? PPV_MEDIA_FILE_EXPIRY_CACHE_NAME : PPV_MEDIA_FILE_CACHE_NAME, "_", mediaFileID);
        }
        private string GetPPVModuleListForMediaFilesCacheKey(long mediaFileID, bool withExpiry)
        {
            return String.Concat(this.originalBasePPVModule.GroupID, "_", withExpiry ? PPV_MEDIA_FILE_EXPIRY_CACHE_NAME : PPV_MEDIA_FILE_CACHE_NAME, "_", mediaFileID);
        }

        private string GetPPVDataCacheKey(string ppvModuleCode)
        {
            return String.Concat(this.originalBasePPVModule.GroupID, "_", PPV_DATA_CACHE_NAME, "_", ppvModuleCode);
        }

        private List<string> GetPPVModulesCacheKey(string[] ppvCodes)
        {
            List<string> cachedKeys = null;
            if (ppvCodes != null && ppvCodes.Length > 0)
            {
                cachedKeys = new List<string>();
                foreach (string ppvModuleCode in ppvCodes)
                {
                    cachedKeys.Add(GetPPVDataCacheKey(ppvModuleCode));
                }
            }

            return cachedKeys;
        }

        private Dictionary<string, PPVModule> GetPPVModulesCacheKeyMappings(List<PPVModule> ppvModules)
        {
            Dictionary<string, PPVModule> cachedKeyMappings = null;
            if (ppvModules != null && ppvModules.Count > 0)
            {
                cachedKeyMappings = new Dictionary<string, PPVModule>();
                foreach (PPVModule ppvModule in ppvModules)
                {
                    if (!string.IsNullOrEmpty(ppvModule.m_sObjectCode))
                    {
                        string key = GetPPVDataCacheKey(ppvModule.m_sObjectCode);
                        if (!cachedKeyMappings.ContainsKey(key))
                        {
                            cachedKeyMappings.Add(key, ppvModule);
                        }
                    }
                }
            }

            return cachedKeyMappings;
        }

        #endregion

        private class SortedMediaFilePPVContainer : IComparable<SortedMediaFilePPVContainer>
        {
            private int index;
            private MediaFilePPVContainer mfpc;

            public int Index
            {
                get
                {
                    return index;
                }
                private set
                {
                    index = value;
                }
            }

            public MediaFilePPVContainer MediaFilePPVContainerObj
            {
                get
                {
                    return mfpc;
                }
                private set
                {
                    mfpc = value;
                }
            }

            public SortedMediaFilePPVContainer(MediaFilePPVContainer mfpc, int index)
            {
                MediaFilePPVContainerObj = mfpc;
                Index = index;
            }

            public int CompareTo(SortedMediaFilePPVContainer other)
            {
                return Index.CompareTo(other.Index);
            }
        }

        private class SortedMediaFilePPVModule : IComparable<SortedMediaFilePPVModule>
        {
            private int index;
            private MediaFilePPVModule mfpm;

            public int Index
            {
                get
                {
                    return index;
                }
                private set
                {
                    index = value;
                }
            }

            public MediaFilePPVModule MediaFilePPVModuleObj
            {
                get
                {
                    return mfpm;
                }
                private set
                {
                    this.mfpm = value;
                }
            }

            public SortedMediaFilePPVModule(MediaFilePPVModule mfpm, int index)
            {
                this.index = index;
                this.mfpm = mfpm;
            }


            public int CompareTo(SortedMediaFilePPVModule other)
            {
                return this.Index.CompareTo(other.Index);
            }
        }

        public PPVModule ValidatePPVModuleForMediaFile(int groupID, int mediaFileID, long ppvModuleCode)
        {   
            PPVModule ppvModule = null;
            if (mediaFileID != 0)
            {
                // check that ppvmodule related to media file 
                DataRow dr = DAL.PricingDAL.Get_PPVModuleForMediaFile(mediaFileID, ppvModuleCode, groupID);
                if (dr != null)
                {
                    if (ppvModuleCode == 0)
                    {
                        ppvModuleCode = ODBCWrapper.Utils.GetLongSafeVal(dr, "ppmid");
                    }
                    // get the MediaFilePPVModule 
                    ppvModule = GetPPVModuleData(ppvModuleCode.ToString(), string.Empty, string.Empty, string.Empty);
                }
            }
            return ppvModule;
        }

        public override PPVModule[] GetPPVModulesDataByProductCodes(List<string> productCodes)
        {
            PPVModule[] ppvModules = null;
            if (productCodes != null && productCodes.Count > 0)
            {
                Dictionary<string, PPVModule> ppvModulesMapping = new Dictionary<string, PPVModule>();
                List<string> unfoundPPVModules = DAL.PricingDAL.Get_PPVsFromProductCodes(productCodes.Distinct().ToList(), originalBasePPVModule.GroupID).Keys.ToList();
                ppvModules = GetPPVModulesData(unfoundPPVModules.ToArray(), string.Empty, string.Empty, string.Empty);                
            }

            return ppvModules;
        }
    }
}