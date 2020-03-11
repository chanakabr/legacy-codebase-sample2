using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader.Protocols;
using Tvinci.Data.DataLoader;

namespace Tvinci.Data.TVMDataLoader
{
	public sealed class TVMDirectAdapter<TExpectedResult> : TVMAdapter<TExpectedResult>
	{
		IProtocol m_protocol;

        public override eCacheMode GetCacheMode()
        {
            return eCacheMode.Never;
        }

        protected override bool ShouldExtractFromCache(string cacheKey)
        {
            return false;
        }

        protected override bool ShouldStoreInCache(LoaderAdapterItem result)
        {
            return false;
        }

		public TVMDirectAdapter(IProtocol protocol)
		{
			m_protocol = protocol;
		}

        //public delegate bool TryGetItemsCountInSourceDelegate(object retrievedData, out long count);
        //private TryGetItemsCountInSourceDelegate m_tryGetMethod = null;

        //public TryGetItemsCountInSourceDelegate TryGetItemsCountMethod
        //{
        //    set
        //    {
        //        if (value != null)
        //        {
        //            base.ShouldExtractItemsCountInSource = true;
        //            m_tryGetMethod = value;
        //        }
        //        else
        //        {
        //            base.ShouldExtractItemsCountInSource = false;
        //            m_tryGetMethod = value;
        //        }
        //    }

        //}

        //protected override bool TryGetItemsCountInSource(object retrievedData, out long count)
        //{
        //    return TryGetItemsCountMethod(retrievedData, out count);            
        //}

		protected internal override IProtocol CreateProtocol()
		{
			return m_protocol;
		}

		public override bool IsPersist()
		{
			return false;
		}


        protected override TExpectedResult PreCacheHandling(object retrievedData)
        {
             return (TExpectedResult)retrievedData;
        }
        
        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{7FCCD6E3-02F3-4258-85CE-513140D670ED}"); }
        }
	}
}
