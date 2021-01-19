using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public partial class KalturaSmsAdapterProfileFilter : KalturaCrudFilter<KalturaSmsAdapterProfileOrderBy, SmsAdapterProfile>
    {
        public KalturaSmsAdapterProfileFilter()
        {

        }

        public override KalturaSmsAdapterProfileOrderBy GetDefaultOrderByValue()
        {
            return KalturaSmsAdapterProfileOrderBy.NONE;
        }

        public override GenericListResponse<SmsAdapterProfile> List(ContextData contextData, CorePager pager)
        {
            return new KalturaSmsAdapterProfile().List(contextData);
        }

        public override void Validate(ContextData contextData)
        {
        }
    }

    public enum KalturaSmsAdapterProfileOrderBy
    {
        NONE
    }
}
