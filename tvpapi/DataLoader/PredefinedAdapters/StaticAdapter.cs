using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Tvinci.Data.DataLoader.PredefinedAdapters
{
    public interface IStaticAdapter : ILoaderAdapter
    {
        object Data { get;  }
    }

    [Serializable]
    public class StaticAdapter : LoaderAdapter<object>, IStaticAdapter
    {        
        [NonSerialized]
        private object m_data;

        object IStaticAdapter.Data
        {
            get
            {
                return m_data;
            }            
        }
        
        public StaticAdapter(object data)
        {
            m_data = data;
        }

        protected override bool ShouldStoreInCache(LoaderAdapterItem result)
        {
            return false;
        }
        protected override bool ShouldExtractFromCache(string cacheKey)
        {
            return false;
        }
        protected override ILoaderProvider GetProvider()
        {
            return StaticProvider.Instance;
        }

        public override bool IsPersist()
        {
            return false;
        }

        protected override object PreCacheHandling(object retrievedData)
        {
            return retrievedData;
        }

        public sealed override eCacheMode GetCacheMode()
        {
            return eCacheMode.Never;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{34F9E01D-0E94-41fd-A694-98EFE6A66A27}"); }
        }
    }
}
