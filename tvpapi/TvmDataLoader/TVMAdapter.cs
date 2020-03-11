using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols;

namespace Tvinci.Data.TVMDataLoader
{
    public interface ITVMAdapter : ILoaderAdapter
    {        
        //void StoreProtocolRequest(string serializedRequest);
        IProtocol CreateProtocol();
    }

	[Serializable]
	public abstract class TVMAdapter<TSourceResult> : TVMAdapter<TSourceResult, TSourceResult>
	{
	
	}


    [Serializable]
    public abstract class TVMAdapter<TSourceResult, TAdapterResult> : LoaderAdapter<TSourceResult, TAdapterResult>, ISupportPaging, ITVMAdapter
    {

        //string m_request;

        //public string ProtocolRequest
        //{
        //    get
        //    {
        //        return m_request;
        //    }
        //}

        //void ITVMAdapter.StoreProtocolRequest(string request)
        //{
        //    m_request = request;
        //}

        IProtocol ITVMAdapter.CreateProtocol()
        {
            return CreateProtocol();
        }
        internal protected abstract IProtocol CreateProtocol();
                                    
        protected override ILoaderProvider GetProvider()
        {
            return new TVMProvider();
        }
                                        
        #region ISupportPaging Members       

        public int PageIndex
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "PageIndex", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "PageIndex", value);
            }
        }

        public int PageSize
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "PageSize", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "PageSize", value);
            }
        }
		
        #endregion

        public override bool IsPersist()
        {
            return true;
        }

        #region IPagingSupporting Members
        
        public virtual bool TryGetItemsCount(out long count)
        {
            count = base.GetItemsInSource();
            return true;
        }

        
        #endregion
    }
}
