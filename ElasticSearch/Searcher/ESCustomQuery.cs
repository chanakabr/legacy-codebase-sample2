using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    public class ESCustomQuery : IESTerm
    {
        public string CustomQuery;

        #region Ctor

        public ESCustomQuery(string customQuery)
        {
            this.CustomQuery = customQuery;
        }

        #endregion

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
            return string.IsNullOrEmpty(CustomQuery);
        }

        #endregion

        public override string ToString()
        {
            return CustomQuery;
        }
    }
}
