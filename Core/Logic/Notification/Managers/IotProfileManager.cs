using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using System.Reflection;
using KLogMonitor;
using Core.Notification;
using Newtonsoft.Json;
using DAL;
using TVinciShared;
using ConfigurationManager;

namespace ApiLogic.Notification
{
    public class IotProfileManager : ICrudHandler<IotProfile, long>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<IotProfileManager> lazy = new Lazy<IotProfileManager>(() => new IotProfileManager());

        public static IotProfileManager Instance { get { return lazy.Value; } }

        private IotProfileManager() { }

        public Status Delete(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        private IotProfile FillMissingConfigurations(IotProfile oldConfig, IotProfile newConfig)
        {
            var updatingConfig = new IotProfile
            {
                AdapterUrl = newConfig.AdapterUrl ?? oldConfig.AdapterUrl,
                IotProfileAws = new IotProfileAws()
                {
                    AccessKeyId = newConfig.IotProfileAws?.AccessKeyId ?? oldConfig.IotProfileAws?.AccessKeyId,
                    SecretAccessKey = newConfig.IotProfileAws?.SecretAccessKey ?? oldConfig.IotProfileAws?.SecretAccessKey,
                    ClientId = newConfig.IotProfileAws?.ClientId ?? oldConfig.IotProfileAws?.ClientId,
                    IdentityPoolId = newConfig.IotProfileAws?.IdentityPoolId ?? oldConfig.IotProfileAws?.IdentityPoolId,
                    IotEndPoint = newConfig.IotProfileAws?.IotEndPoint ?? oldConfig.IotProfileAws?.IotEndPoint,
                    Region = newConfig.IotProfileAws?.Region ?? oldConfig.IotProfileAws?.Region,
                    UserPoolId = newConfig.IotProfileAws?.UserPoolId ?? oldConfig.IotProfileAws?.UserPoolId
                },
            };

            return updatingConfig;
        }

        public GenericResponse<IotProfile> Add(ContextData contextData)
        {
            var response = new GenericResponse<IotProfile>();
            var groupId = contextData.GroupId;

            try
            {
                var partnerSettings = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);

                if (partnerSettings == null)
                {
                    var error = $"Error while getting PartnerNotificationSettings for group: {groupId}.";
                    log.Error(error);
                    response.SetStatus(eResponseStatus.Error, error);
                    return response;
                }

                if (NotificationDal.GetIotProfile(groupId) != null)
                {
                    var error = $"Error: IotProfile already exists for group: {groupId}.";
                    log.Error(error);
                    response.SetStatus(eResponseStatus.AlreadyExist, error);
                    return response;
                }

                if (!IotManager.IsIotAllowed(partnerSettings))
                {
                    var error = $"Iot is not allowed for group: {groupId}.";
                    log.Error(error);
                    response.SetStatus(eResponseStatus.Error, error);
                    return response;
                }

                var iotProfile = new IotProfile
                {
                    AdapterUrl = ApplicationConfiguration.Current.IotAdapterConfiguration.AdapterUrl.Value
                };

                if (string.IsNullOrEmpty(iotProfile.AdapterUrl))
                {
                    log.Debug("No adapter Url was set in TCM");
                    response.SetStatus(eResponseStatus.AdapterUrlRequired, "Iot adapter url is missing");
                    return response;
                }

                var _request = new { GroupId = groupId.ToString() };
                var msResponse = IotManager.Instance.SendToAdapter<IotProfileAws>(groupId, IotAction.CREATE_ENVIRONMENT, _request, 
                    MethodType.Post, out int httpStatus, out bool hasConfig, iotProfile);

                if (httpStatus == 208)
                {
                    log.Debug($"Environment already exists for group: {groupId}");
                    response.SetStatus(eResponseStatus.AlreadyExist, "Environment already exists");
                    return response;
                }

                iotProfile.IotProfileAws = msResponse;
                iotProfile.IotProfileAws.UpdateDate = DateUtils.GetUtcUnixTimestampNow();

                var saved = SaveIotProfile(groupId, iotProfile);

                if (msResponse == null)
                {
                    var error = $"Error while Add Iot adapter profile for group: {groupId}.";
                    log.Error(error);
                    response.SetStatus(eResponseStatus.Error, error);
                    return response;
                }

                response.SetStatus(saved ? Status.Ok : Status.Error);
                response.Object = new IotProfile { AdapterUrl = iotProfile.AdapterUrl, IotProfileAws = msResponse };
            }
            catch (Exception ex)
            {
                log.Error($"Add profile failed, error: {ex.Message}");
            }

            return response;
        }

        public GenericResponse<IotProfile> Update(ContextData contextData, IotProfile iotProfile)
        {
            var response = new GenericResponse<IotProfile>();
            var groupId = contextData.GroupId;

            try
            {
                var partnerSettings = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);

                if (partnerSettings == null)
                {
                    var error = $"Error while getting PartnerNotificationSettings for group: {groupId}.";
                    log.Error(error);
                    response.SetStatus(eResponseStatus.Error, error);
                    return response;
                }

                if (!IotManager.IsIotAllowed(partnerSettings))
                {
                    var error = $"Iot is not allowed for group: {groupId}.";
                    log.Error(error);
                    response.SetStatus(eResponseStatus.Error, error);
                    return response;
                }

                var currentConfigurations = NotificationDal.GetIotProfile(contextData.GroupId);

                if (currentConfigurations == null)
                {
                    var error = $"Error: IotProfile doesn't exists for group: {contextData.GroupId}.";
                    log.Error(error);
                    response.SetStatus(eResponseStatus.NoConfigurationFound, error);
                    return response;
                }

                var newConfigurations = FillMissingConfigurations(currentConfigurations, iotProfile);

                newConfigurations.IotProfileAws.UpdateDate = DateUtils.GetUtcUnixTimestampNow();
                IotManager.Instance.InvalidateClientConfiguration(groupId);

                var saved = SaveIotProfile(groupId, newConfigurations);

                var msResponse = IotManager.Instance.SendToAdapter<IotProfileAws>(groupId, IotAction.ADD_CONFIG, iotProfile.IotProfileAws, MethodType.Put, out int httpStatus, out bool hasConfig, newConfigurations, true);

                if (!hasConfig)
                {
                    var update = IotManager.Instance.UpdateIotProfile(groupId, contextData);
                    if (update != null)
                    {
                        msResponse = IotManager.Instance.SendToAdapter<IotProfileAws>(groupId, IotAction.ADD_CONFIG, iotProfile.IotProfileAws, MethodType.Put, out httpStatus, out hasConfig, iotProfile, true);
                    }
                }

                if (msResponse == null)
                {
                    var error = $"Error while Updating Iot adapter profile for group: {groupId}.";
                    log.Error(error);
                    response.SetStatus(eResponseStatus.Error, error);
                    return response;
                }

                response.SetStatus(saved ? Status.Ok : Status.Error);
                response.Object = new IotProfile { AdapterUrl = iotProfile.AdapterUrl, IotProfileAws = msResponse };
            }
            catch (Exception ex)
            {
                log.Error($"Update profile failed, error: {ex.Message}");
            }

            return response;
        }

        public GenericResponse<IotProfile> GetIotConfiguration(ContextData contextData)
        {
            var response = new GenericResponse<IotProfile>();
            var groupId = contextData.GroupId;
            try
            {
                var partnerSettings = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
                if (!IotManager.IsIotAllowed(partnerSettings))
                {
                    log.Error($"Error while getting PartnerNotificationSettings for group: {groupId}.");
                    return response;
                }

                var iotProfile = NotificationDal.GetIotProfile(groupId);

                if (iotProfile == null)
                {
                    log.Error($"No Iot Profile for group: {groupId}.");
                    response.SetStatus(eResponseStatus.AdapterNotExists, $"No adapter configurations found for group: {groupId}");
                    return response;
                }

                var urlSuffix = $"groupId={groupId}&forClient={false}";

                var profileAws = IotManager.Instance.SendToAdapter<IotProfileAws>(groupId, IotAction.GET_IOT_CONFIGURATION, urlSuffix, MethodType.Get, out int httpStatus, out bool hasConfig);

                if (!hasConfig)
                {
                    var update = IotManager.Instance.UpdateIotProfile(groupId, contextData);
                    if (update != null)
                    {
                        profileAws = IotManager.Instance.SendToAdapter<IotProfileAws>(groupId, IotAction.GET_IOT_CONFIGURATION, urlSuffix, MethodType.Get, out httpStatus, out hasConfig);
                    }
                }

                response.Object = new IotProfile
                {
                    AdapterUrl = iotProfile.AdapterUrl,
                    IotProfileAws = profileAws
                };

                response.SetStatus(Status.Ok);
            }
            catch (Exception ex)
            {
                log.Error($"GetIotConfiguration failed, error: {ex.Message}");
                response.SetStatus(eResponseStatus.Error, "GetIotConfiguration failed");
            }

            return response;
        }

        private bool SaveIotProfile(int groupId, IotProfile msResponse)
        {
            if (!DAL.NotificationDal.SaveIotProfile(groupId, msResponse))
            {
                log.ErrorFormat($"Error while adding Iot profile. Iot response: {JsonConvert.SerializeObject(msResponse)}");
                return false;
            }
            return true;
        }

        public GenericResponse<IotProfile> Get(ContextData contextData, long id)
        {
            return GetIotConfiguration(contextData);
        }
    }
}
