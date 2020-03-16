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
    public partial class KalturaIotFilter : KalturaCrudFilter<KalturaIotOrderBy, Iot>
    {
        public KalturaIotFilter()
        {

        }
        public override KalturaIotOrderBy GetDefaultOrderByValue()
        {
            throw new NotImplementedException();
        }

        public override GenericListResponse<Iot> List(ContextData contextData, CorePager pager)
        {
            throw new NotImplementedException();
        }

        public override void Validate()
        {
            throw new NotImplementedException();
        }

        public async Task RegisterDevice()
        {

        }
    }

    public enum KalturaIotOrderBy
    {
        NONE
    }
}
