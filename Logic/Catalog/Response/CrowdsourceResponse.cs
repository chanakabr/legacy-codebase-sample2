using ApiObjects.CrowdsourceItems.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Response
{
    [DataContract]
    public class CrowdsourceResponse : BaseResponse
    {
        [DataMember]
        public List<BaseCrowdsourceItem> CrowdsourceItems { get; set; }

        public CrowdsourceResponse()
        {
            CrowdsourceItems = null;
        }
    }
}
