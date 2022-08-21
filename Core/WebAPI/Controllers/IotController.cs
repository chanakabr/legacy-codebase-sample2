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
    public class IotController : IKalturaController
    {
        /// <summary>
        /// Register IOT device
        /// </summary>
        /// <returns>boolean for processing the registration request</returns>
        [Action("register")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        public static bool Register()
        {
            throw new NotImplementedException("iot.Register should be used only by phoenix rest proxy");
        }

        /// <summary>
        /// Get iot Client Configuration
        /// </summary>
        /// <returns></returns>
        [Action("getClientConfiguration")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        public static KalturaIotClientConfiguration GetClientConfiguration()
        {
            throw new NotImplementedException("iot.GetClientConfiguration should be used only by phoenix rest proxy");
        }
    }
}