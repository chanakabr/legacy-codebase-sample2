using ApiLogic.Notification;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using Core.Api;
using System;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("iot")]
    public class IotController : KalturaCrudController<KalturaIot, KalturaIotListResponse, Iot, long, KalturaIotFilter>
    {
        /// <summary>
        /// Register IOT device
        /// </summary>
        /// <returns>Credentials for aws-sdk connection</returns>
        [Action("register")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static KalturaIot Register()
        {
            KalturaIot response = null;
            var contextData = KS.GetContextData();

            try
            {
                ValidateRequest(contextData);

                Func<GenericResponse<Iot>> coreFunc = () =>
                    IotManager.Instance.Register(contextData);

                response = ClientUtils.GetResponseFromWS<KalturaIot, Iot>(coreFunc);
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
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static KalturaIotClientConfiguration GetClientConfiguration()
        {
            KalturaIotClientConfiguration response = null;
            var contextData = KS.GetContextData();

            try
            {
                ValidateRequest(contextData);

                Func<GenericResponse<IotClientConfiguration>> coreFunc = () =>
                    IotManager.Instance.GetClientConfiguration(contextData);

                response = ClientUtils.GetResponseFromWS<KalturaIotClientConfiguration, IotClientConfiguration>(coreFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        private static void ValidateRequest(ContextData contextData)
        {
            var devices = api.Instance.GetDomainDevices((int)contextData.DomainId, contextData.GroupId);

            if (devices == null || !devices.ContainsKey(contextData.Udid))
            {
                throw new BadRequestException(BadRequestException.INVALID_UDID, contextData.Udid);
            }
        }
    }
}