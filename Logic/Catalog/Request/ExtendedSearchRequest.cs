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
using Core.Catalog.Response;
using KLogMonitor;
using System.Reflection;
using Core.Catalog.Attributes;

namespace Core.Catalog.Request
{
    [DataContract]
    public class ExtendedSearchRequest : UnifiedSearchRequest
    {
        [DataMember]
        public List<string> ExtraReturnFields { get; set; }

        [DataMember]
        public bool ShouldUseSearchEndDate { get;  set; }

        public ExtendedSearchRequest()
            : base()
        {
        }

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
                ExtraReturnFields.Add("start_date");
                ExtraReturnFields.Add("end_date");
                return this.ExtraReturnFields.Distinct().ToList();
            }
            else
            {
                return new List<string>();
            }
        }

        internal override bool GetShouldUseSearchEndDate()
        {
            return ShouldUseSearchEndDate;
        }
    }
}