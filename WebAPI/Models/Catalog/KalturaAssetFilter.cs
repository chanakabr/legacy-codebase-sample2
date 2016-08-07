using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public abstract class KalturaAssetFilter : KalturaFilter<KalturaAssetOrderBy>
    {
        public override KalturaAssetOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetOrderBy.RELEVANCY_DESC;
        }       

        internal virtual void Validate()
        {            
        }
    }
}



