using ApiLogic.Users.Managers;
using ApiObjects.Response;
using ApiObjects.Segmentation;
using System;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Segmentation;

namespace WebAPI.Controllers
{
    [Service("householdSegment")]       
    public class HouseholdSegmentController : IKalturaController
    {
        /// <summary>
        /// householdSegment add
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="objectToAdd">householdSegment details</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.HouseholdRequired)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.ObjectNotExist)]
        static public KalturaHouseholdSegment Add(KalturaHouseholdSegment objectToAdd)
        {
            var contextData = KS.GetContextData();
            Func<HouseholdSegment, GenericResponse<HouseholdSegment>> addFunc = (HouseholdSegment coreObject) =>
               HouseholdSegmentManager.Instance.Add(contextData, coreObject);
            var response = ClientUtils.GetResponseFromWS(objectToAdd, addFunc);
            return response;
        }

        /// <summary>
        /// Remove segment from household
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Segment identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.HouseholdRequired)]
        [Throws(eResponseStatus.ObjectNotExist)]
        static public void Delete(long id)
        {
            var contextData = KS.GetContextData();
            Func<Status> deleteFunc = () => HouseholdSegmentManager.Instance.Delete(contextData, id);
            ClientUtils.GetResponseStatusFromWS(deleteFunc);
        }

        /// <summary>
        /// Gets all HouseholdSegment items for a household
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaHouseholdSegmentListResponse List(KalturaHouseholdSegmentFilter filter = null)
        {
            var contextData = KS.GetContextData();
            if (filter == null)
            {
                filter = new KalturaHouseholdSegmentFilter();
            }
            var coreFilter = AutoMapper.Mapper.Map<HouseholdSegmentFilter>(filter);

            Func<GenericListResponse<HouseholdSegment>> listFunc = () =>
                HouseholdSegmentManager.Instance.List(contextData, coreFilter);

            KalturaGenericListResponse<KalturaHouseholdSegment> result =
               ClientUtils.GetResponseListFromWS<KalturaHouseholdSegment, HouseholdSegment>(listFunc);

            var response = new KalturaHouseholdSegmentListResponse
            {
                Objects = result.Objects,
                TotalCount = result.TotalCount
            };

            return response;
        }
    }
}