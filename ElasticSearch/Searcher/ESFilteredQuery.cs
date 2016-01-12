using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    /// <summary>
    /// Not to be confused with FilteredQuery!
    /// This is only the "query" part
    /// </summary>
    public class ESFilteredQuery : IESTerm
    {
        public QueryFilter Filter
        {
            get;
            set;
        }
        public IESTerm Query
        {
            get;
            set;
        }

        #region IESTerm Members

        public eTermType eType
        {
            get
            {
                return eTermType.BOOL_QUERY;
            }
        }

        public bool IsEmpty()
        {
            bool result = true;

            // I prefer writing the code parts separetly - simple is better

            if (this.Filter != null && !this.Filter.IsEmpty())
            {
                result = false;
            }

            if (this.Query != null && !this.Query.IsEmpty())
            {
                result = false;
            }

            return result;
        }

        #endregion

        public override string ToString()
        {
            if (IsEmpty())
            {
                return string.Empty;
            }

            StringBuilder filteredQuery = new StringBuilder();


            filteredQuery.Append("{ \"filtered\": {");

            List<string> parts = new List<string>();

            if (Query != null)
            {
                string sQuery = Query.ToString();
                if (!string.IsNullOrEmpty(sQuery))
                {
                    parts.Add(string.Format(" \"query\": {0}", sQuery));

                }
            }

            if (Filter != null)
            {
                string filterString = Filter.ToString();
                if (!string.IsNullOrEmpty(filterString))
                {
                    parts.Add(filterString);
                }
            }

            if (parts.Count > 0)
            {
                filteredQuery.Append(parts.Aggregate((current, next) => current + "," + next));
            }

            filteredQuery.Append("}");

            return filteredQuery.ToString();
        }
    }
}
