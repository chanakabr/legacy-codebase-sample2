using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Iot settings filter
    /// </summary>
    public partial class KalturaIotProfileFilter : KalturaFilter<KalturaIotProfileOrderBy>
    {
        public override KalturaIotProfileOrderBy GetDefaultOrderByValue()
        {
            throw new NotImplementedException();
        }
    }
}
