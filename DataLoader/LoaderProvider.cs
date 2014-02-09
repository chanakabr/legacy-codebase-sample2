using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;

namespace Tvinci.Data.DataLoader
{
    public abstract class LoaderProvider<TAdapterInterface> : ILoaderProvider where TAdapterInterface : ILoaderAdapter
    {
        object ILoaderProvider.GetDataFromSource(ILoaderAdapter adapter)
        {
            return GetDataFromSource((TAdapterInterface)adapter);
        }

        #region ILoaderWorker Members

        public abstract object GetDataFromSource(TAdapterInterface adapter);
        
        #endregion
    }
}
