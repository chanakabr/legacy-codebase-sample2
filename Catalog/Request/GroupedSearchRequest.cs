using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Catalog.Response;
using KLogMonitor;
using System.Reflection;
using Catalog.Attributes;
using Catalog.Cache;

namespace Catalog.Request
{
    [LogTopic("UnifiedSearch")]
    [DataContract]
    public class GroupedSearchRequest : UnifiedSearchRequest, IRequestImp
    {
        #region Data Members
        
        #endregion

        #region Ctor

        public GroupedSearchRequest(int pageSize, int pageIndex, int groupID, string signature, string signString,
            OrderObj order,
            List<int> types,
            string filterQuery,
            string nameAndDescription,
            BooleanPhraseNode filterTree = null,
            List<string> groupBy = null)
            : base(pageSize, pageIndex, groupID, signature, signString, order, types, filterQuery, nameAndDescription, filterTree)
        {
            this.groupBy = groupBy;
        }

        #endregion
    }
}
