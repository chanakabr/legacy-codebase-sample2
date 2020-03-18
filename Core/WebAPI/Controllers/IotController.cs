using ApiLogic.Notification;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using Tvinci.Core.DAL;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("iot")]
    public class IotController : KalturaCrudController<KalturaIot, KalturaIotListResponse, Iot, long, KalturaIotFilter, IotFilter>
    {
        /// <summary>
        /// Register IOT device
        /// </summary>
        /// <returns>Credentials for aws-sdk connection</returns>
        [Action("register")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.InternalConnectionIssue)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static GenericResponse<KalturaIot> Register()
        {
            var response = new GenericResponse<KalturaIot>();
            var contextData = KS.GetContextData();

            try
            {
                if (!ValidateRequest(contextData, response))
                {
                    return response; 
                }

                Func<GenericResponse<Iot>> coreFunc = () =>
                    IotManager.Instance.Register(contextData);

                response.Object = ClientUtils.GetResponseFromWS<KalturaIot, Iot>(coreFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get iot Client Configuration
        /// </summary>
        /// <returns></returns>
        [Action("getClientConfiguration")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.InternalConnectionIssue)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static GenericResponse<KalturaIotClientConfiguration> GetClientConfiguration()
        {
            var response = new GenericResponse<KalturaIotClientConfiguration>();
            var contextData = KS.GetContextData();

            try
            {
                if (!ValidateRequest(contextData, response))
                {
                    return response;
                }

                Func<GenericResponse<IotClientConfiguration>> coreFunc = () =>
                    IotManager.Instance.GetIotClientConfiguration(contextData);

                response.Object = ClientUtils.GetResponseFromWS<KalturaIotClientConfiguration, IotClientConfiguration>(coreFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        private static bool ValidateRequest<T>(ContextData contextData, GenericResponse<T> genericResponse)
        {
            var device = CatalogDAL.GetDomainDevices((int)contextData.DomainId);

            if (device == null || !device.ContainsKey(contextData.Udid))
            {
                genericResponse.SetStatus(eResponseStatus.DeviceNotInDomain);
                return false;
            }
            return true;
        }
    }
}