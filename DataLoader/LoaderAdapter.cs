using System;
using System.Linq;
using System.Data;
using System.Configuration;
using KLogMonitor;
using System.Reflection;
using ConfigurationManager;

namespace Tvinci.Data.DataLoader
{

    [Flags]
    public enum eExecuteBehaivor
    {
        None = 0,
        ForceRetrieve = 2
    }


    public class LoaderAdapterItem
    {
        public object Item { get; set; }
        public long ItemsCount { get; set; }
        public bool HasItemsCount { get; set; }

        public LoaderAdapterItem()
        {
            Item = null;
            ItemsCount = 0;
            HasItemsCount = false;
        }
    }

    public enum eParameterType
    {
        Retrieve,
        Filter
    }

    public interface ICustomParameterType
    {
        object[] GetPropertiesValue();
    }

    [Serializable]
    public abstract class LoaderAdapter<TAdapterResult> : LoaderAdapter<TAdapterResult, TAdapterResult>
    {


    }

    public static class LoaderAdapterManager
    {
        public delegate bool ForceDataRetrieveDelegate();

        public delegate object GetLanguageIDDelegate();

        public static ForceDataRetrieveDelegate ForceDataRetrieveMethod { get; set; }
        public static GetLanguageIDDelegate GetLanguageIDMethod { get; set; }

        static LoaderAdapterManager()
        {
            GetLanguageIDMethod = LoaderAdapterManager.DummyGetLanguageID;
        }

        private static object DummyGetLanguageID()
        {
            return null;
        }
    }
    [Serializable]
    public abstract class LoaderAdapter<TSourceResult, TAdapterResult> : ILoaderAdapter
    {


        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private object LanguageID
        {
            get
            {
                return Parameters.GetParameter<object>(eParameterType.Retrieve, "LanguageID", null);
            }
            set
            {
                Parameters.SetParameter<object>(eParameterType.Retrieve, "LanguageID", value);

            }
        }

        protected LoaderParameters Parameters = new LoaderParameters();

        int m_loaderIdentifier;

        public string FlashVarsFileFormat;
        public string FlashVarsSubFileFormat;

        public LoaderAdapter()
        {
            m_loaderIdentifier = UniqueIdentifier.GetHashCode();
            LanguageID = LoaderAdapterManager.GetLanguageIDMethod();
        }

        [NonSerialized]
        LoaderAdapterItem m_currentExecuteItem = null;

        protected TSourceResult ExtractSource(eExecuteBehaivor behaivor)
        {
            m_currentExecuteItem = getSourceResult(behaivor);

            if (m_currentExecuteItem == null || m_currentExecuteItem.Item == null)
            {
                return default(TSourceResult);
            }
            if (m_currentExecuteItem.Item is TSourceResult)
            {
                return (TSourceResult)m_currentExecuteItem.Item;
            }
            else
            {
                throw new Exception(string.Format("Expected result of type '{0}'", typeof(TSourceResult)));
            }
        }


        public virtual TAdapterResult Execute()
        {
            return Execute(eExecuteBehaivor.None);
        }

        public TAdapterResult Execute(eExecuteBehaivor behaivor)
        {
            object result = execute(behaivor);

            if (result == null)
            {
                return default(TAdapterResult);
            }
            else if (result is TAdapterResult)
            {
                return (TAdapterResult)result;
            }
            else
            {
                throw new Exception(string.Format("Expected result of type '{0}'", typeof(TAdapterResult)));
            }
        }


        protected virtual ILoaderCache GetCustomDataCaching()
        {
            return null;
        }

        [NonSerialized]
        ILoaderCache m_dataCaching = null;

        #region Abstract / virtual methods


        public virtual eCacheMode GetCacheMode()
        {
            return eCacheMode.Application;
        }

        protected abstract ILoaderProvider GetProvider();

        protected virtual void PreExecute()
        {
            // no implementation needed by design
        }

        protected virtual void Validate()
        {
            return;
        }
        #endregion

        #region Private methods
        private eCacheAction initializeCache(out string cacheKey, eExecuteBehaivor behaivor)
        {
            cacheKey = GetRequestUniqueKey();

            if (LoaderAdapterManager.ForceDataRetrieveMethod != null && LoaderAdapterManager.ForceDataRetrieveMethod())
            {
                // TODO - must change method name!!!!!
                m_dataCaching = RequestCache.Current;
                return eCacheAction.GetFromCache | eCacheAction.StoreInCache;
            }

            switch (GetCacheMode())
            {
                case eCacheMode.Application:
                    m_dataCaching = LoaderCacheLite.Current;
                    break;
                case eCacheMode.Session:
                    m_dataCaching = SessionCache.Current;
                    break;
                case eCacheMode.Custom:
                    m_dataCaching = GetCustomDataCaching();
                    break;
                case eCacheMode.Never:
                    m_dataCaching = RequestCache.Current;
                    break;
                default:
                    throw new NotSupportedException();
            }



            if (m_dataCaching != null)
            {
                eCacheAction result = eCacheAction.GetFromCache | eCacheAction.StoreInCache;

                if ((behaivor & eExecuteBehaivor.ForceRetrieve) == eExecuteBehaivor.ForceRetrieve || !ShouldExtractFromCache(cacheKey))
                {
                    result &= ~eCacheAction.GetFromCache;
                }

                return result;
            }

            return eCacheAction.None;
        }
        #endregion

        static LoaderAdapter()
        {
            string value = System.Configuration.ConfigurationManager.AppSettings["Tvinci.DataLoader.LoaderAdapter.BackwardCompotability"];
            shouldsupportBackwardCompetability = string.IsNullOrEmpty(value) ? false : (value.ToLower() == "true");
        }

        private static readonly bool shouldsupportBackwardCompetability;

        #region ILoaderAdapter Members
        public abstract bool IsPersist();

        LoaderAdapterItem getSourceResult(eExecuteBehaivor behaivor)
        {
            LoaderAdapterItem result = null;
            PreExecute();

            // validate the parameters
            Validate();

            string cacheKey;
            eCacheAction cacheAction = initializeCache(out cacheKey, behaivor);
            if ((cacheAction & eCacheAction.GetFromCache) == eCacheAction.GetFromCache)
            {
                if (m_dataCaching.TryGetData<LoaderAdapterItem>(cacheKey, out result))
                {
                    if (result.Item != null)
                    {
                        if (!(result.Item is TSourceResult))
                        {
                            if (shouldsupportBackwardCompetability)
                            {
                                result = null;
                            }
                            else
                            {
                                string message = string.Format("loader expected source of type '{0}'. actual type returned '{1}' (RequestKey '{2}').", typeof(TSourceResult).FullName, result.Item.GetType().FullName, cacheKey);
                                logger.Error(message);
                                throw new Exception(message);
                            }
                        }
                    }
                    else
                    {
                        if (shouldsupportBackwardCompetability)
                        {
                            result = null;
                        }
                    }
                }
            }

            // check if can get value from cache
            if (result == null)
            {
                // get value from source
                object sourceData = GetProvider().GetDataFromSource(this);

                if (sourceData == null && shouldsupportBackwardCompetability)
                {
                    logger.WarnFormat("The loader source returned with null value. cache key '{0}'. The loader will try to load data from source again next time being executed", cacheKey);
                }

                result = new LoaderAdapterItem();
                result.Item = PreCacheHandling(sourceData);

                if (sourceData != null)
                {
                    if (ShouldExtractItemsCountInSource)
                    {
                        long tempCount;
                        if (TryGetItemsCountInSource(sourceData, out tempCount))
                        {
                            result.ItemsCount = tempCount;
                            result.HasItemsCount = true;
                        }
                    }
                }
                else
                {
                    result.ItemsCount = 0;
                    result.HasItemsCount = true;
                }

                // update the cache for next time
                if ((cacheAction & eCacheAction.StoreInCache) == eCacheAction.StoreInCache)
                {
                    if (ShouldStoreInCache(result))
                    {
                        m_dataCaching.AddData(GetRequestUniqueKey(), result, new string[] { }, CustomCacheDuration());
                    }

                }
            }

            return result;
        }

        protected virtual int CustomCacheDuration()
        {
            return 0;
        }

        protected string GetRequestUniqueKey()
        {
            return string.Format("{0};{1}", m_loaderIdentifier, Parameters.GetUniqueKey());

        }

        /// <summary>
        /// Allows executing logic to determine whether to store the result in cache. 
        /// Can be used to prevent storing null as value in cache
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual bool ShouldStoreInCache(LoaderAdapterItem result)
        {
            return true;
        }



        protected virtual bool ShouldExtractFromCache(string cacheKey)
        {
            return true;
        }

        public delegate bool TryGetCountDelegate(object retrievedData, out long count);

        private TryGetCountDelegate m_tryGetItemsCountMethod;

        public TryGetCountDelegate TryGetItemsCountMethod
        {
            set
            {
                ShouldExtractItemsCountInSource = (value != null);
                m_tryGetItemsCountMethod = value;
            }
        }

        protected virtual bool TryGetItemsCountInSource(object retrievedData, out long count)
        {
            if (m_tryGetItemsCountMethod != null)
            {
                return m_tryGetItemsCountMethod(retrievedData, out count);
            }
            else
            {
                count = 0;
                return false;
            }
        }

        public bool m_shouldExtractItemsCountInSource = false;

        public virtual bool ShouldExtractItemsCountInSource
        {
            get { return m_shouldExtractItemsCountInSource; }
            private set
            {
                m_shouldExtractItemsCountInSource = value;
            }
        }


        object execute(eExecuteBehaivor behaivor)
        {
            object result = null;
            TSourceResult sourceData = ExtractSource(behaivor);

            resultFormattedByDerived = true;
            TAdapterResult tempResult = FormatResults(sourceData);

            if (typeof(TSourceResult) != typeof(TAdapterResult) && !resultFormattedByDerived)
            {
                throw new Exception("When the adapter result type and the source result type not match, the method 'FormatResults' must be overriden by derived adapter!");
            }

            if (resultFormattedByDerived)
            {
                result = tempResult;
            }
            else
            {
                result = sourceData;
            }

            m_lastExecuteResult = result;

            return result;

        }

        object ILoaderAdapter.Execute(eExecuteBehaivor behaivor)
        {
            
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return BCExecute(behaivor);
            }
            else
            {
                return execute(behaivor);
            }
        }

        public virtual object BCExecute(eExecuteBehaivor behaivor)
        {
            return execute(behaivor);
        }

        object ILoaderAdapter.Execute()
        {
            
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return BCExecute(eExecuteBehaivor.None);
            }
            else
            {
                return execute(eExecuteBehaivor.None);
            }
        }

        protected abstract Guid UniqueIdentifier { get; }

        protected abstract TSourceResult PreCacheHandling(object retrievedData);

        private bool resultFormattedByDerived;
        protected virtual TAdapterResult FormatResults(TSourceResult originalObject)
        {
            resultFormattedByDerived = false;
            return default(TAdapterResult);
        }

        #endregion

        protected long GetItemsInSource()
        {
            if (m_currentExecuteItem == null)
            {
                return 0;
            }
            else if (!m_currentExecuteItem.HasItemsCount)
            {
                throw new Exception("Failed to extract items count in source");
            }
            else
            {
                return m_currentExecuteItem.ItemsCount;
            }
        }

        #region ILoaderAdapter Members

        [NonSerialized]
        object m_lastExecuteResult = null;

        public object LastExecuteResult
        {
            get { return m_lastExecuteResult; }
        }

        #endregion
    }
}



//#region ISerializable Members

//       protected LoaderAdapter(SerializationInfo info, StreamingContext context)
//       {
//           Parameters = (LoaderParameters)info.GetValue("parameters", typeof(LoaderParameters));
//       }
//       public void GetObjectData(SerializationInfo info, StreamingContext context)
//       {
//           info.AddValue("parameters", Parameters);

//       }

//       #endregion
