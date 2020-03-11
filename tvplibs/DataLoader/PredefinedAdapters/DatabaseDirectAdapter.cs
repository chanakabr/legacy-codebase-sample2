using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Tvinci.Data.DataLoader.PredefinedAdapters
{            
    public class DatabaseDirectAdapter<TExpectedResult> : DatabaseAdapter<TExpectedResult>
        where TExpectedResult : DataTable
    {
        public delegate TExpectedResult ParseResponseDelegate(object retrievedData);

        public static void Execute(IDatabaseAdapter adapter)
        {
            adapter.Execute();
        }
        public override bool IsPersist()
        {
            return false;
        }

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
        
        public delegate void InitializeQueryDelegate(ODBCWrapper.DataSetSelectQuery query);
        public ParseResponseDelegate ParseResponseMethod = null;        
                
        InitializeQueryDelegate m_initializeQueryMethod;        
        private DataTable m_mergeToTable;

        protected override Guid UniqueIdentifier
        {
            get { return m_guid; }
        }

        Guid m_guid = Guid.NewGuid();

        public DatabaseDirectAdapter(InitializeQueryDelegate initializeQueryMethod)
            : this(initializeQueryMethod, null)
        {
            
        }

        public DatabaseDirectAdapter(InitializeQueryDelegate initializeQueryMethod, TExpectedResult mergeToTable)
            : this(initializeQueryMethod, mergeToTable, null)
        {            
        }

        public DatabaseDirectAdapter(InitializeQueryDelegate initializeQueryMethod, TExpectedResult mergeToTable, ParseResponseDelegate parseResponseMethod)          
        {
            ParseResponseMethod = parseResponseMethod;
            m_initializeQueryMethod = initializeQueryMethod;
            m_mergeToTable = mergeToTable;
        }

        protected override void InitializeQuery(ODBCWrapper.DataSetSelectQuery query)
        {
            m_initializeQueryMethod(query);
        }
        
        protected override TExpectedResult PreCacheHandling(object retrievedData)
        {
            DataTable requestedObject = retrievedData as DataTable;

            if (requestedObject == null)
            {
                throw new Exception("Expected object of type 'DataTable'");
            }

            if (ParseResponseMethod != null)
            {
                requestedObject = ParseResponseMethod(retrievedData);
            }

            if (m_mergeToTable != null)
            {                
                try
                {
                    m_mergeToTable.Merge(requestedObject);
                        requestedObject = m_mergeToTable;                                            
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Failed to merge data with typed dataset of type '{0}'. see inner exception for details", m_mergeToTable.GetType().Name),ex);                    
                }                
            }

            return (TExpectedResult)requestedObject;
        }        
    }

    public sealed class DatabaseDirectAdapter : DatabaseDirectAdapter<DataTable>
    {

        public DatabaseDirectAdapter(InitializeQueryDelegate initializeQueryMethod)
            : base(initializeQueryMethod)
        {
        }

        public DatabaseDirectAdapter(InitializeQueryDelegate initializeQueryMethod, DataTable mergeToTable)
            : base (initializeQueryMethod,mergeToTable)
        {            
        }

        public DatabaseDirectAdapter(InitializeQueryDelegate initializeQueryMethod, DataTable mergeToTable, ParseResponseDelegate parseResponseMethod)          
            : base (initializeQueryMethod,mergeToTable,parseResponseMethod)
        {
            
        }
    }

	//public class DatabaseDirectAdapter<TExpectedResult, TAdditionalData> : DatabaseAdapter<TExpectedResult>
	//    where TExpectedResult : DataTable
	//{
	//    public delegate void ParseResponseDelegate(object retrievedData,TAdditionalData additional);

	//    TAdditionalData m_additionalData;
	//    private DataTable m_mergeToTable;

	//    public delegate void InitializeQueryDelegate(ODBCWrapper.DataSetSelectQuery query, TAdditionalData additionalData);

	//    InitializeQueryDelegate m_initializeMethod;

	//    public DatabaseDirectAdapter(InitializeQueryDelegate initializeQueryMethod, TAdditionalData additionalData, DataTable mergeToTable)
	//        : this(initializeQueryMethod, additionalData, mergeToTable, null)
	//    {
	//    }

	//    public DatabaseDirectAdapter(InitializeQueryDelegate initializeQueryMethod, TAdditionalData additionalData, DataTable mergeToTable, ParseResponseDelegate parseResponseMethod)
	//    {
	//        base.ParseResponseMethod = parseResponseMethod;
	//        m_additionalData = additionalData;
	//        m_initializeMethod = initializeQueryMethod;
	//        m_mergeToTable = mergeToTable;
	//    }

	//    protected override void InitializeQuery(ODBCWrapper.DataSetSelectQuery query)
	//    {
	//        m_initializeMethod(query, m_additionalData);
	//    }

	//    protected override TExpectedResult ParseResponse(object retrievedData)
	//    {
	//        DataTable requestedObject = retrievedData as DataTable;

	//        if (requestedObject == null)
	//        {
	//            throw new Exception("Expected object of type 'DataTable'");
	//        }

	//        if (m_mergeToTable != null)
	//        {
	//            try
	//            {
	//                m_mergeToTable.Merge(requestedObject);
	//                requestedObject = m_mergeToTable;
	//            }
	//            catch (Exception ex)
	//            {
	//                throw new Exception(string.Format("Failed to merge data with typed dataset of type '{0}'. see inner exception for details", m_mergeToTable.GetType().Name), ex);
	//            }
	//        }

	//        return (TExpectedResult) requestedObject;

	//    }
	//}
}
