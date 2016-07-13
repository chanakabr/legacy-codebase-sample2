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

namespace Catalog.Request
{
    [DataContract]
    public class ExtendedSearchRequest : UnifiedSearchRequest
    {
        [DataMember]
        public List<string> ExtraReturnFields { get; set; }

        public ExtendedSearchRequest(int nPageSize, int nPageIndex, int nGroupID, string sSignature, string sSignString,
            OrderObj order,
            List<int> types,
            string filterQuery,
            string nameAndDescription,
            BooleanPhraseNode filterTree = null)
            : base(nPageSize, nPageIndex, nGroupID, sSignature, sSignString, order, types, filterQuery, nameAndDescription, filterTree)
        {
        }

        internal override List<string> GetExtraReturnFields()
        {
            if (this.ExtraReturnFields != null)
            {
                return this.ExtraReturnFields;
            }
            else
            {
                return new List<string>();
            }
        }
    }
}