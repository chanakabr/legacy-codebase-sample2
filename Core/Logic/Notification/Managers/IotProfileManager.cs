using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using System.Reflection;
using KLogMonitor;
using System.Configuration;
using Core.Notification;
using Newtonsoft.Json;
using DAL;
using TVinciShared;

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

        public GenericResponse<IotProfile> Update(ContextData contextData, IotProfile iotProfile)
        {
            var response = new GenericResponse<IotProfile>();
            var currentConfigurations = NotificationDal.GetIotProfile(contextData.GroupId);

            if (currentConfigurations == null)
            {
                var error = $"Error: IotProfile doesn't exists for group: {contextData.GroupId}.";
                log.Error(error);
                response.SetStatus(eResponseStatus.NoConfigurationFound, error);
                return response;
            }
            else
            {
                var newConfigurations = FillMissingConfigurations(currentConfigurations, iotProfile);
                return Add(contextData, newConfigurations, true);
            }
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
                    BrokerPort = newConfig.IotProfileAws?.BrokerPort == 0 ? oldConfig.IotProfileAws.BrokerPort : newConfig.IotProfileAws.BrokerPort,
                    CertificatePath = newConfig.IotProfileAws?.CertificatePath ?? oldConfig.IotProfileAws?.CertificatePath,
                    ClientId = newConfig.IotProfileAws?.ClientId ?? oldConfig.IotProfileAws?.ClientId,
                    IdentityPoolId = newConfig.IotProfileAws?.IdentityPoolId ?? oldConfig.IotProfileAws?.IdentityPoolId,
                    IotEndPoint = newConfig.IotProfileAws?.IotEndPoint ?? oldConfig.IotProfileAws?.IotEndPoint,
                    IotPolicyName = newConfig.IotProfileAws?.IotPolicyName ?? oldConfig.IotProfileAws?.IotPolicyName,
                    PfxPassword = newConfig.IotProfileAws?.PfxPassword ?? oldConfig.IotProfileAws?.PfxPassword,
                    PfxPath = newConfig.IotProfileAws?.PfxPath ?? oldConfig.IotProfileAws?.PfxPath,
                    Region = newConfig.IotProfileAws?.Region ?? oldConfig.IotProfileAws?.Region,
                    UserPoolId = newConfig.IotProfileAws?.UserPoolId ?? oldConfig.IotProfileAws?.UserPoolId
                },
            };

            return updatingConfig;
        }

        public GenericResponse<IotProfile> Add(ContextData contextData, IotProfile iotProfile, bool isUpdate = false)
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

                if (!isUpdate && NotificationDal.GetIotProfile(groupId) != null)
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

                iotProfile.IotProfileAws.UpdateDate = DateUtils.GetUtcUnixTimestampNow();

                var saved = SaveIotProfile(groupId, iotProfile);

                var msResponse = IotManager.Instance.SendToAdapter<IotProfileAws>(groupId, IotAction.ADD_CONFIG, iotProfile.IotProfileAws, MethodType.Put, out bool hasConfig, iotProfile, true);

                if (!hasConfig)
                {
                    var update = IotManager.Instance.UpdateIotProfile(groupId, contextData);
                    if (update != null)
                    {
                        msResponse = IotManager.Instance.SendToAdapter<IotProfileAws>(groupId, IotAction.ADD_CONFIG, iotProfile.IotProfileAws, MethodType.Put, out hasConfig, iotProfile, true);
                    }
                }

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
                var urlSuffix = $"groupId={groupId}&forClient={false}";

                var profileAws = IotManager.Instance.SendToAdapter<IotProfileAws>(groupId, IotAction.GET_IOT_CONFIGURATION, urlSuffix, MethodType.Get, out bool hasConfig);

                if (!hasConfig)
                {
                    var update = IotManager.Instance.UpdateIotProfile(groupId, contextData);
                    if (update != null)
                    {
                        profileAws = IotManager.Instance.SendToAdapter<IotProfileAws>(groupId, IotAction.GET_IOT_CONFIGURATION, urlSuffix, MethodType.Get, out hasConfig);
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
