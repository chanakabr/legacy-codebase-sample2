using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using Core.Notification;
using DAL;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using System;
using TVinciShared;

namespace ApiLogic.Notification
{
    public class IotProfileManager
    {
        private static readonly KLogger log = new KLogger(nameof(IotProfileManager));
        private static readonly Lazy<IotProfileManager> lazy = new Lazy<IotProfileManager>(()
            => new IotProfileManager(NotificationDal.Instance, NotificationCache.Instance(), IotManager.Instance));

        private readonly INotificationDal _notificationDal;
        private readonly INotificationCache _notificationCache;
        private readonly IIotManager _iotManager;

        public static IotProfileManager Instance => lazy.Value;

        private IotProfileManager(INotificationDal notificationDal, INotificationCache notificationCache, IIotManager iotManager)
        {
            _notificationDal = notificationDal ?? throw new ArgumentNullException(nameof(notificationDal));
            _notificationCache = notificationCache ?? throw new ArgumentNullException(nameof(notificationCache));
            _iotManager = iotManager ?? throw new ArgumentNullException(nameof(iotManager));
        }

        public Status Delete(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        private IotProfile FillMissingConfigurations(IotProfile oldConfig, IotProfile newConfig)
        {
            var updatingConfig = new IotProfile
            {
                AdapterUrl = newConfig.AdapterUrl ?? oldConfig.AdapterUrl,
                IotProfileAws = new IotProfileAws
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
                var partnerSettings = _notificationCache.GetPartnerNotificationSettings(groupId);

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

                iotProfile.IotProfileAws = _iotManager.CreateIotEnvironment(groupId, iotProfile);

                if (iotProfile.IotProfileAws == null)
                {
                    var error = $"Error while Add Iot adapter profile for group: {groupId}.";
                    log.Error(error);
                    response.SetStatus(eResponseStatus.Error, error);
                    return response;
                }

                iotProfile.IotProfileAws.UpdateDate = DateUtils.GetUtcUnixTimestampNow();

                response.SetStatus(Status.Ok);
                response.Object = new IotProfile { AdapterUrl = iotProfile.AdapterUrl, IotProfileAws = iotProfile.IotProfileAws };
            }
            catch (Exception ex)
            {
                log.Error($"Add profile failed, error: {ex.Message}", ex);
            }

            return response;
        }

        public GenericResponse<IotProfile> GetIotConfiguration(ContextData contextData)
        {
            var response = new GenericResponse<IotProfile>();
            var groupId = contextData.GroupId;
            try
            {
                var partnerSettings = _notificationCache.GetPartnerNotificationSettings(groupId);
                if (!IotManager.IsIotAllowed(partnerSettings))
                {
                    log.Error($"Error while getting PartnerNotificationSettings for group: {groupId}.");
                    return response;
                }

                var iotProfile = GetIotProfile(groupId);

                if (iotProfile == null)
                {
                    log.Error($"No Iot Profile for group: {groupId}.");
                    response.SetStatus(eResponseStatus.AdapterNotExists, $"No adapter configurations found for group: {groupId}");
                    return response;
                }

                _iotManager.InvalidateClientConfiguration(groupId);

                response.Object = new IotProfile
                {
                    AdapterUrl = iotProfile.AdapterUrl,
                    IotProfileAws = _iotManager.GetConfiguration(groupId)
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

        public GenericResponse<IotProfile> Get(ContextData contextData, long id)
        {
            return GetIotConfiguration(contextData);
        }

        internal IotProfile GetIotProfile(int groupId)
        {
            var result = IotManager.Instance.GetConfiguration(groupId);
            return new IotProfile { AdapterUrl = IotManager.Instance.GetTcmAdapterUrl(), IotProfileAws = result };
        }
    }
}
