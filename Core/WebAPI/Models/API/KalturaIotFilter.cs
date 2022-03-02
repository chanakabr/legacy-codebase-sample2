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
    public partial class KalturaIotFilter : KalturaFilter<KalturaIotOrderBy>
    {
        public override KalturaIotOrderBy GetDefaultOrderByValue()
        {
            throw new NotImplementedException();
        }
    }
}
