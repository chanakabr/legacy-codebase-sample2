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
    public partial class KalturaDeviceReferenceDataFilter : KalturaCrudFilter<KalturaDeviceReferenceDataOrderBy, DeviceReferenceData>
    {
        public KalturaDeviceReferenceDataFilter() : base()
        {
        }

        public override KalturaDeviceReferenceDataOrderBy GetDefaultOrderByValue()
        {
            return KalturaDeviceReferenceDataOrderBy.NONE;
        }

        public override void Validate()
        {
        }

        public override GenericListResponse<DeviceReferenceData> List(ContextData contextData, CorePager pager)
        {
            //var reponse = DeviceInformationManager.Instance.GetModels(contextData.GroupId);
            //var reponse = DeviceInformationManager.Instance.GetManufacturers(contextData.GroupId);
            return null;
        }
    }

    public enum KalturaDeviceReferenceDataOrderBy
    {
        NONE
    }
}