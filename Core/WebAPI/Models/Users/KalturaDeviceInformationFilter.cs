using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Password policy settings filter
    /// </summary>
    public partial class KalturaDeviceInformationFilter : KalturaCrudFilter<KalturaDeviceInformationOrderBy, DeviceInformation>
    {
        public KalturaDeviceInformationFilter() : base()
        {
        }

        public override KalturaDeviceInformationOrderBy GetDefaultOrderByValue()
        {
            return KalturaDeviceInformationOrderBy.NONE;
        }

        public override void Validate()
        {
        }

        public override GenericListResponse<DeviceInformation> List(ContextData contextData, CorePager pager)
        {
            //var coreFilter = AutoMapper.Mapper.Map<PasswordPolicyFilter>(this);
            return null;
        }
    }

    public enum KalturaDeviceInformationOrderBy
    {
        NONE
    }
}