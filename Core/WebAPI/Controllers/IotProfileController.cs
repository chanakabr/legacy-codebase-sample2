using ApiLogic.Notification;
using ApiObjects;
using ApiObjects.Response;
using System;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;

namespace WebAPI.Controllers
{
    [Service("iotProfile")]
    public class IotProfileController : IKalturaController
    {
        /// <summary>
        /// Add new KalturaIotProfile
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="objectToAdd">KalturaIotProfile Object to add</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AlreadyExist)]
        static public KalturaIotProfile Add(KalturaIotProfile objectToAdd)
        {
            var contextData = KS.GetContextData();
            Func<GenericResponse<IotProfile>> addFunc = () => IotProfileManager.Instance.Add(contextData);
            var response = ClientUtils.GetResponseFromWS<KalturaIotProfile, IotProfile>(addFunc);
            return response;
        }

        /// <summary>
        /// Get existing KalturaIotProfile
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">KalturaIotProfile identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.AdapterNotExists)]
        static public KalturaIotProfile Get(long id)
        {
            var contextData = KS.GetContextData();
            Func<GenericResponse<IotProfile>> coreFunc = () => IotProfileManager.Instance.Get(contextData, contextData.GroupId);
            var response = ClientUtils.GetResponseFromWS<KalturaIotProfile, IotProfile>(coreFunc);
            return response;
        }

        /// <summary>
        /// Update existing KalturaIotProfile
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">id of KalturaIotProfile to update</param>
        /// <param name="objectToUpdate">KalturaIotProfile Object to update</param>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.NoConfigurationFound)]
        static public KalturaIotProfile Update(long id, KalturaIotProfile objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
