using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using System.Reflection;
using KLogMonitor;
using System.Net.Http;
using System.Configuration;
using Core.Notification;
using Newtonsoft.Json;
using ApiObjects.Notification;
using DAL;
using TVinciShared;
using ConfigurationManager.Types;
using ConfigurationManager;
using System.Text;
using Core.Notification.Adapters;

namespace ApiLogic.Notification
{
    public class IotManager : ICrudHandler<Iot, long>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<IotManager> lazy = new Lazy<IotManager>(() => new IotManager());
        private static readonly HttpClient httpClient = HttpClientUtil.GetHttpClient(ApplicationConfiguration.Current.IotHttpClientConfiguration);
        private const string REGISTER = "/api/IOT/RegisterDevice";
        private const string GET_IOT_CONFIGURATION = "/api/IOT/Configuration/List?";
        public const string ADD_TO_SHADOW = "/api/IOT/AddToShadow";
        public const string PUBLISH = "/api/IOT/Publish";
        public const string DELETE_DEVICE = "/api/IOT/DeleteDevice";
        public const string SYSTEM_ANNOUNCEMENT = "SystemAnnouncement";
        public const string ADD_CONFIG = "/api/IOT/Configuration/Update";

        public static IotManager Instance { get { return lazy.Value; } }

        private IotManager() { }

        public GenericResponse<Iot> Register(ContextData contextData)
        {
            var response = new GenericResponse<Iot>();
            var groupId = contextData.GroupId;
            var udid = contextData.Udid;
            try
            {
                var partnerSettings = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);

                if (!IsIotAllowed(partnerSettings))
                {
                    log.Error($"Error while getting PartnerNotificationSettings for group: {groupId}.");
                    return response;
                }

                var _request = new { GroupId = groupId.ToString(), Udid = udid };

                var msResponse = SendToAdapter<Iot>(groupId, IotAction.REGISTER, _request, MethodType.Post, out bool hasConfig);

                if (!hasConfig)
                {
                    var update = UpdateIotProfile(groupId, contextData);
                    if (update != null)
                    {
                        msResponse = SendToAdapter<Iot>(groupId, IotAction.REGISTER, _request, MethodType.Post, out hasConfig);
                    }
                }

                if (msResponse == null)
                {
                    log.Error($"Error while registering udid: {udid} for group: {groupId}.");
                    return response;
                }

                var saved = SaveRegisteredDevice(groupId, udid, msResponse);

                response.SetStatus(saved ? Status.Ok : Status.Error);
                response.Object = msResponse;
            }
            catch (Exception ex)
            {
                log.Error($"Register failed, error: {ex.Message}");
            }

            return response;
        }

        public static bool IsIotAllowed(NotificationPartnerSettingsResponse partnerSettings)
        {
            return partnerSettings != null && partnerSettings.settings != null &&
                partnerSettings.settings.IsIotEnabled != null && partnerSettings.settings.IsIotEnabled.Value == true;
        }

        private bool SaveRegisteredDevice(int groupId, string udid, Iot msResponse)
        {
            msResponse.Udid = msResponse.Udid ?? udid;
            msResponse.GroupId = msResponse.GroupId ?? groupId.ToString();

            if (!DAL.NotificationDal.SaveRegisteredDevice(msResponse))
            {
                log.ErrorFormat($"Error while saving Iot device. Iot response: {Newtonsoft.Json.JsonConvert.SerializeObject(msResponse)}");
                return false;
            }
            return true;
        }

        public GenericResponse<IotClientConfiguration> GetClientConfiguration(ContextData contextData)
        {
            var response = new GenericResponse<IotClientConfiguration>();
            var groupId = contextData.GroupId;
            try
            {
                var partnerSettings = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
                if (!IsIotAllowed(partnerSettings))
                {
                    log.Error($"Error while getting PartnerNotificationSettings for group: {groupId}.");
                    return response;
                }

                var iotProfile = NotificationDal.GetIotProfile(groupId);
                if (iotProfile == null)
                {
                    log.Error($"Error while getting configurations for group: {groupId}.");
                    return response;
                }

                //TODO Matan - add cache layered cache
                var iotClientConfiguration = new IotClientConfiguration
                {
                    AnnouncementTopic = $"{groupId}/{SYSTEM_ANNOUNCEMENT}",
                    CredentialsProvider = new CredentialsProvider
                    {
                        CognitoIdentity = new CognitoIdentity
                        {
                            Default = new Default { PoolId = iotProfile.IotProfileAws.IdentityPoolId, Region = iotProfile.IotProfileAws.Region }
                        }
                    },
                    CognitoUserPool = new CognitoUserPool
                    {
                        Default = new Default { PoolId = iotProfile.IotProfileAws.UserPoolId, AppClientId = iotProfile.IotProfileAws.ClientId, Region = iotProfile.IotProfileAws.Region }
                    }
                };
                iotClientConfiguration.Json = iotClientConfiguration.ToString();

                response.Object = iotClientConfiguration;

                response.SetStatus(Status.Ok);
            }
            catch (ConfigurationErrorsException)
            {
                log.Error($"No configurations for group: {groupId}.");
            }
            catch (Exception ex)
            {
                log.Error($"GetIotClientConfiguration failed, error: {ex.Message}");
            }

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public bool RemoveFromIot(int m_nGroupID, string udid)
        {
            try
            {
                var iotDevice = DAL.NotificationDal.GetRegisteredDevice(m_nGroupID.ToString(), udid);
                if (iotDevice == null)
                {
                    log.Error($"Device {udid} not found");
                    return false;
                }
                var partnerSettings = NotificationCache.Instance().GetPartnerNotificationSettings(m_nGroupID);

                //Todo - Matan Create perm object
                var _request = new { GroupId = m_nGroupID.ToString(), Udid = udid, IdentityId = iotDevice.IdentityId };

                var msResponse = SendToAdapter<bool>(m_nGroupID, IotAction.DELETE_DEVICE, _request, MethodType.Delete, out bool hasConfig);

                if (!hasConfig)
                {
                    var update = UpdateIotProfile(m_nGroupID, new ContextData(m_nGroupID));
                    if (update != null)
                    {
                        msResponse = SendToAdapter<bool>(m_nGroupID, IotAction.DELETE_DEVICE, _request, MethodType.Delete, out hasConfig);
                    }
                }

                if (!msResponse)
                {
                    log.Error($"Failed removing udid: {udid} from group: {m_nGroupID}");
                    return false;
                }

                if (!DAL.NotificationDal.RemoveRegisteredDevice(m_nGroupID.ToString(), udid))
                {
                    log.Error($"Failed to delete iot document for device: {udid}, group: {m_nGroupID}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed removing udid: {udid} from group: {m_nGroupID} by exception: {ex.Message}");
                return false;
            }

            return true;
        }

        public GenericResponse<Iot> Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        private string GetAdapterUrl(int groupId, IotAction action, IotProfile iotProfile = null)
        {
            var adapterUrl = iotProfile != null ? iotProfile.AdapterUrl : NotificationDal.GetIotProfile(groupId).AdapterUrl;

            if (string.IsNullOrEmpty(adapterUrl))
            {
                log.Error("Iot Notification URL wasn't found");
                return string.Empty;
            }

            var configValue = string.Empty;
            switch (action)
            {
                case IotAction.REGISTER:
                    configValue = REGISTER;
                    break;
                case IotAction.GET_IOT_CONFIGURATION:
                    configValue = GET_IOT_CONFIGURATION;
                    break;
                case IotAction.ADD_TO_SHADOW:
                    configValue = ADD_TO_SHADOW;
                    break;
                case IotAction.PUBLISH:
                    configValue = PUBLISH;
                    break;
                case IotAction.DELETE_DEVICE:
                    configValue = DELETE_DEVICE;
                    break;
                case IotAction.SYSTEM_ANNOUNCEMENT:
                    configValue = SYSTEM_ANNOUNCEMENT;
                    break;
                case IotAction.ADD_CONFIG:
                    configValue = ADD_CONFIG;
                    break;
            }
            return $"{adapterUrl}{configValue}";
        }

        public T SendToAdapter<T>(int groupId, IotAction action, object request, MethodType method, out bool hasConfig, IotProfile iotProfile = null, bool groupNeeded = false)
        {
            var url = GetAdapterUrl(groupId, action, iotProfile);
            url = groupNeeded ? url + $"?groupId={groupId}" : string.Empty;
            StringContent content = null;
            hasConfig = true;
            switch (method)
            {
                case MethodType.Post:
                    content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var postResponse = httpClient.PostAsync(url, content).Result;
                    if (postResponse?.StatusCode == System.Net.HttpStatusCode.NoContent)
                        hasConfig = false;
                    return NotificationAdapter.ParseResponse<T>(postResponse);
                case MethodType.Put:
                    content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var putResponse = httpClient.PutAsync(url, content).Result;
                    if (putResponse?.StatusCode == System.Net.HttpStatusCode.NoContent)
                        hasConfig = false;
                    return NotificationAdapter.ParseResponse<T>(putResponse);
                case MethodType.Get:
                    var fullUrl = $"{url}{request}";
                    var _response = httpClient.GetAsync(fullUrl).Result;
                    if (_response?.StatusCode == System.Net.HttpStatusCode.NoContent)
                        hasConfig = false;
                    return NotificationAdapter.ParseResponse<T>(_response);
                case MethodType.Delete:
                    var _request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri(url),
                        Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")
                    };
                    var deleteResponse = httpClient.SendAsync(_request).Result;
                    return NotificationAdapter.ParseResponse<T>(deleteResponse);
                default:
                    log.Error($"Invalid attempt to call method: {method}, url: {url} group: {groupId}");
                    return default;
            }
        }

        public IotProfile UpdateIotProfile(int groupId, ContextData contextData)
        {
            log.Info($"No Configurations for group: {groupId} trying to reload from CB");
            var config = NotificationDal.GetIotProfile(groupId);
            if (config == null)
            {
                log.Error($"No config was found for group: {groupId}");
                return null;
            }
            return IotProfileManager.Instance.Update(contextData, config)?.Object;
        }
    }

    public enum IotAction
    {
        REGISTER, GET_IOT_CONFIGURATION, ADD_TO_SHADOW, PUBLISH, DELETE_DEVICE, SYSTEM_ANNOUNCEMENT, ADD_CONFIG
    }
    public enum MethodType
    {
        Post, Get, Delete, Put
    }
}
