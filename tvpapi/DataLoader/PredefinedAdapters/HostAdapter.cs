using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.DataLoader.PredefinedAdapters
{
    //[Serializable]
    //public class HostAdapter: LoaderAdapter
    //{
    //    [NonSerialized]
    //    object m_AdapterResult = null;

    //    private object AdapterResult
    //    {
    //        get
    //        {
    //            return m_AdapterResult;
    //        }
    //        set
    //        {
    //            m_AdapterResult = value;
    //        }
    //    }

    //    [NonSerialized]
    //    bool m_AdapterExecuted = false;
        
    //    public LoaderAdapter Adapter { get; private set; }

    //    public HostAdapter(LoaderAdapter adapter)
    //    {
    //        Adapter = adapter;
    //    }

    //    public TResult GetResult<TResult>()
    //    {
    //        if (!m_AdapterExecuted)
    //        {
    //            try
    //            {
    //                m_AdapterResult = Adapter.Execute<TResult>();
    //                return (TResult)m_AdapterResult;            
    //            }
    //            catch (Exception)
    //            {
    //                m_AdapterResult = null;
    //                throw;
    //            }
    //            finally
    //            {
    //                m_AdapterExecuted = true;
    //            }
    //        }
    //        else
    //        {
    //            if (m_AdapterResult == null)
    //            {
    //                throw new Exception("Cannot create results. the adapter failed to execute in previous attempts. Read log for further information");
    //            }

    //            return (TResult)m_AdapterResult;            
    //        }                        
    //    }

    //    protected override ILoaderProvider GetProvider()
    //    {
    //        return new HostProvider();            
    //    }
    //}
}
