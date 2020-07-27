using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using CachingProvider.LayeredCache;

namespace ApiLogic.Users.Managers
{
    public class DeviceInformationManager : ICrudHandler<DeviceInformation, long>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<DeviceInformationManager> lazy = new Lazy<DeviceInformationManager>(() => new DeviceInformationManager());
        public static DeviceInformationManager Instance { get { return lazy.Value; } }

        private DeviceInformationManager() { }

        public Status Delete(ContextData contextData, long id)
        {
            return new Status(eResponseStatus.OK);
        }

        public GenericResponse<DeviceInformation> Get(ContextData contextData, long id)
        {
            return new GenericResponse<DeviceInformation>();
        }

        public GenericResponse<DeviceInformation> Add<T>(ContextData contextData, T coreObject) where T : DeviceInformation
        {
            var response = new GenericResponse<DeviceInformation>();
            var groupId = contextData.GroupId;
            var updaterId = contextData.UserId;
            try
            {
                var validationKey = string.Empty;
                if (coreObject is DeviceModelInformation)
                {
                    response = DAL.UsersDal.InsertDeviceModelInformation
                        (groupId, updaterId, coreObject as DeviceModelInformation);
                    validationKey = LayeredCacheKeys.GetDeviceModelInformationInvalidationKey(groupId);
                }
                else if (coreObject is DeviceManufacturerInformation)
                {
                    response = DAL.UsersDal.InsertDeviceManufacturerInformation
                        (groupId, updaterId, coreObject as DeviceManufacturerInformation);
                    validationKey = LayeredCacheKeys.GetDeviceManufacturerInformationInvalidationKey(groupId);
                }

                if (response.IsOkStatusCode())
                    LayeredCache.Instance.SetInvalidationKey(validationKey);
                else
                {
                    log.Info($"Cannot insert device information from type: {coreObject.GetType().Name}");
                    response.SetStatus(eResponseStatus.Error, "Cannot insert device information");
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in Add. contextData: {contextData}, " +
                    $"DeviceInformation: {Newtonsoft.Json.JsonConvert.SerializeObject(coreObject)}", ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public GenericResponse<DeviceInformation> Update<T>(ContextData contextData, T coreObject) where T : DeviceInformation
        {
            return new GenericResponse<DeviceInformation>();
        }
    }
}
