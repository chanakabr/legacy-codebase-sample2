using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ODBCWrapper;
using System.Data;

namespace Tvinci.Data.DataLoader.PredefinedAdapters
{
    public interface IDatabaseAdapter : ILoaderAdapter
    {
        void ExecuteInitializeQuery(DataSetSelectQuery query);
    }

    [Serializable]
    public abstract class DatabaseAdapter<TAdapterResult> : DatabaseAdapter<TAdapterResult, TAdapterResult>
        where TAdapterResult : DataTable
    {

    }    
    [Serializable]
    public abstract class DatabaseAdapter<TSourceResult, TAdapterResult> : LoaderAdapter<TSourceResult, TAdapterResult>, IDatabaseAdapter
        where TSourceResult : DataTable
    {

        protected override TSourceResult PreCacheHandling(object retrievedData)
        {
            return (TSourceResult)retrievedData;
        }
        //protected override TAdapterResult PreCacheHandling(object retrievedData)
        //{
        //    throw new NotImplementedException();
        //}
        //protected override TExpectedResult PreCacheHandling(object retrievedData)
        //{
        //    return (TExpectedResult)retrievedData;
        //}
        
        public bool TryExtractRow(out DataRow row)
        {
            row = ExtractRow(false);

            return (row != null);
        }
        public DataRow ExtractRow(bool exceptionIfNull)
        {
            DataTable result = base.ExtractSource(eExecuteBehaivor.None);

            if (result == null)
            {
                if (!exceptionIfNull)
                {
                    return null;
                }
                else
                {
                    throw new Exception(string.Format("The adapter failed to return data table"));
                }
            }

            if (result.Rows.Count == 1)
            {
                return result.Rows[0];
            }
            else
            {
                if (!exceptionIfNull && result.Rows.Count == 0)
                {
                    return null;
                }
                else
                {
                    throw new Exception(string.Format("The adapter returned '{0}' rows. expected 1 {1}", result.Rows.Count, (exceptionIfNull) ? "." : " or 0."));
                }

            }
        }

        public const string OrderByToken = "{OrderBy}";


        #region Fields        
        [NonSerialized]
        protected ODBCWrapper.DataSetSelectQuery m_query;
        #endregion

        #region virtual methods
        protected virtual string SortByStatement()
        {
            return string.Empty;
        }

        protected abstract void InitializeQuery(DataSetSelectQuery query);
        #endregion

        #region Constructor              
        #endregion

        #region Override methods
        protected override ILoaderProvider GetProvider()
        {
            return new DatabaseProvider();
        }
        #endregion

        
        void IDatabaseAdapter.ExecuteInitializeQuery(DataSetSelectQuery query)
        {
            InitializeQuery(query);

            string sortBy = SortByStatement();

            if (!string.IsNullOrEmpty(sortBy))
            {
                query += string.Format(" Order by {0} ", sortBy);
            }
        }

        #region Public methods      

        //public string GetParsedStatement()
        //{
        //    string result = FormatStatement(Statement);

        //    if (string.IsNullOrEmpty(result))
        //    {
        //        throw new Exception("The member 'Statement' must contains value");
        //    }

        //    if (!string.IsNullOrEmpty(OrderByStatement))
        //    {
        //        result = Regex.Replace(result, OrderByToken, string.Format("order by {0}", OrderByStatement));
        //    }
        //    else
        //    {
        //        //result = Regex.Replace(result, OrderByToken, string.Format("order by {0}", ""));

        //        if (Regex.Match(result, OrderByToken) != null)
        //        {
        //            throw new Exception("");
        //        }
        //    }

        //    return result;
        //}
        #endregion

        public override bool IsPersist()
        {
            return true;
        }
    }
}
