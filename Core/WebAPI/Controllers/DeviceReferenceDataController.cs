using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("deviceReferenceData")]
    public class DeviceReferenceDataController : IKalturaController
    {
        /// <summary>
        /// add DeviceReferenceData
        /// </summary>
        /// <param name="objectToAdd">DeviceReferenceData details</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AlreadyExist)]
        static public KalturaDeviceReferenceData Add(KalturaDeviceReferenceData objectToAdd)
        {
            var contextData = KS.GetContextData();

            // call to manager and get response
            KalturaDeviceReferenceData response;
            switch (objectToAdd)
            {
                case KalturaDeviceManufacturerInformation c: response = AddDeviceManufacturerInformation(contextData, c); break;
                default: throw new NotImplementedException($"Add for {objectToAdd.objectType} is not implemented");
            }

            return response;
        }

        private static KalturaDeviceManufacturerInformation AddDeviceManufacturerInformation(ContextData contextData, KalturaDeviceManufacturerInformation objectToAdd)
        {
            Func<DeviceManufacturerInformation, GenericResponse<DeviceManufacturerInformation>> addFunc = 
                (DeviceManufacturerInformation coreObject) => DeviceReferenceDataManager.Instance.Add(contextData, coreObject);
            var result = ClientUtils.GetResponseFromWS(objectToAdd, addFunc);
            return result;
        }

        /// <summary>
        /// Update existing DeviceReferenceData
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">id of DeviceReferenceData to update</param>
        /// <param name="objectToUpdate">DeviceReferenceData Object to update</param>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        static public KalturaDeviceReferenceData Update(long id, KalturaDeviceReferenceData objectToUpdate)
        {
            var contextData = KS.GetContextData();
            objectToUpdate.Id = id;

            // call to manager and get response
            KalturaDeviceReferenceData response;
            switch (objectToUpdate)
            {
                case KalturaDeviceManufacturerInformation c: response = UpdateDeviceManufacturerInformation(contextData, c); break;
                default: throw new NotImplementedException($"Update for {objectToUpdate.objectType} is not implemented");
            }

            return response;
        }

        private static KalturaDeviceManufacturerInformation UpdateDeviceManufacturerInformation(ContextData contextData, KalturaDeviceManufacturerInformation objectToUpdate)
        {
            Func<DeviceManufacturerInformation, GenericResponse<DeviceManufacturerInformation>> updateFance = 
                (DeviceManufacturerInformation coreObject) => DeviceReferenceDataManager.Instance.Update(contextData, coreObject);
            var result = ClientUtils.GetResponseFromWS(objectToUpdate, updateFance);
            return result;
        }

        /// <summary>
        /// Delete existing DeviceReferenceData
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">DeviceReferenceData identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        static public void Delete(long id)
        {
            var contextData = KS.GetContextData();
            Func<Status> deleteFunc = () => DeviceReferenceDataManager.Instance.Delete(contextData, id);
            ClientUtils.GetResponseStatusFromWS(deleteFunc);
        }

        /// <summary>
        /// Returns the list of available DeviceReferenceData
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaDeviceReferenceDataListResponse List(KalturaDeviceReferenceDataFilter filter, KalturaFilterPager pager = null)
        {
            var contextData = KS.GetContextData();

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }
            var corePager = AutoMapper.Mapper.Map<CorePager>(pager);

            KalturaGenericListResponse<KalturaDeviceReferenceData> result;
            switch (filter)
            {
                case KalturaDeviceManufacturersReferenceDataFilter f: result = ListByDeviceManufacturersReferenceDataFilter(contextData, corePager, f); break;    
                default: throw new Exceptions.ClientException((int)eResponseStatus.NotAllowed, "Filter error");
            }

            var response = new KalturaDeviceReferenceDataListResponse
            {
                Objects = result.Objects,
                TotalCount = result.TotalCount
            };

            return response;
        }

        private static KalturaGenericListResponse<KalturaDeviceReferenceData> ListByDeviceManufacturersReferenceDataFilter(ContextData contextData, CorePager pager, KalturaDeviceManufacturersReferenceDataFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<DeviceManufacturersReferenceDataFilter>(filter);

            Func<GenericListResponse<DeviceReferenceData>> listFunc = () =>
                DeviceReferenceDataManager.Instance.ListByManufacturer(contextData, coreFilter, pager);

            KalturaGenericListResponse<KalturaDeviceReferenceData> triggerCampaignResponse =
               ClientUtils.GetResponseListFromWS<KalturaDeviceReferenceData, DeviceReferenceData>(listFunc);

            KalturaGenericListResponse<KalturaDeviceReferenceData> response = new KalturaGenericListResponse<KalturaDeviceReferenceData>();
            response.Objects.AddRange(triggerCampaignResponse.Objects);
            response.TotalCount = triggerCampaignResponse.TotalCount;
            return response;
        }
    }
}
