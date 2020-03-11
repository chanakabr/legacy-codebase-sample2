using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.DataLoader.PredefinedAdapters
{
    public interface ICustomAdapter : ILoaderAdapter
    {
        object ExtractResponse();

    }
    [Serializable]
    public abstract class CustomAdapter<TAdapterResult> : CustomAdapter<TAdapterResult, TAdapterResult>
    {

    }
    [Serializable]
    public abstract class CustomAdapter<TSourceResult, TAdapterResult> : LoaderAdapter<TSourceResult, TAdapterResult>, ICustomAdapter
    {
        object ICustomAdapter.ExtractResponse()
        {
            return CreateSourceResult();
        }

        protected abstract TSourceResult CreateSourceResult();

        protected sealed override TSourceResult PreCacheHandling(object retrievedData)
        {
            return (TSourceResult)retrievedData;
        }
                        
        protected override ILoaderProvider GetProvider()
        {
            return new CustomProvider();
        }

        public override bool IsPersist()
        {
            return true;
        }
    }
}
