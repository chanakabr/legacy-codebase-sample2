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
using ConfigurationManager;
using System.Text;
using Core.Notification.Adapters;
using System.Collections.Generic;
using CachingProvider.LayeredCache;
using System.Linq;
using System.Security.Cryptography;
using ApiObjects.EventBus;
using System.Threading;

namespace ApiLogic.Notification
{
    public interface IIotManager : ICrudHandler<Iot, long>
    {
        bool PublishIotMessage(int groupId, string message, string topic);
        bool AddToThingShadow(int groupId, string message, string thingArn, string udid);
        IotProfileAws CreateIotEnvironment(int groupId, IotProfile iotProfile);
        IotProfileAws UpdateIotEnvironment(int groupId, IotProfile newConfigurations);
        int GetTopicPartitionsCount();
        string GetTopicFormat(int groupId, EventType eventType);
    }

    public class IotManager : IIotManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<IotManager> lazy = new Lazy<IotManager>(() => new IotManager(), LazyThreadSafetyMode.PublicationOnly);
        private static readonly HttpClient httpClient = HttpClientUtil.GetHttpClient(ApplicationConfiguration.Current.IotHttpClientConfiguration);
        private readonly ThreadLocal<MD5> md5 = new ThreadLocal<MD5>(() => MD5.Create());

        private const string REGISTER = "/api/IOT/RegisterDevice";
        private const string GET_IOT_CONFIGURATION = "/api/IOT/Configuration/List?";
        public const string ADD_TO_SHADOW = "/api/IOT/AddToShadow";
        public const string PUBLISH = "/api/IOT/Publish";
        public const string DELETE_DEVICE = "/api/IOT/DeleteDevice";
        public const string SYSTEM_ANNOUNCEMENT = "SystemAnnouncement";
        public const string ADD_CONFIG = "/api/IOT/Configuration/Update";
        private const string CREATE_ENVIRONMENT = "/api/IOT/Environment/Create";
        private const int TOPIC_PARTITIONS_COUNT = 10;
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

                response.Object = RegisterDevice(groupId, udid);


                if (response.Object == null)
                {
                    log.Error($"Error while registering udid: {udid} for group: {groupId}.");
                    return response;
                }

                var saved = SaveRegisteredDevice(groupId, udid, response.Object);

                response.SetStatus(saved ? Status.Ok : Status.Error);
            }
            catch (Exception ex)
            {
                log.Error($"Register failed, group: {groupId}, error: {ex.Message}", ex);
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

        public string GetTopicFormat(int groupId, EventType eventType)
        {
            return $"{groupId}/{eventType.ToString()}/{{0}}";
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

                var groupConfig = GetClientConfigurationCache(groupId);
                var deviceConfig = new IotClientConfiguration
                {
                    AnnouncementTopic = groupConfig.AnnouncementTopic,
                    CognitoUserPool = groupConfig.CognitoUserPool,
                    CredentialsProvider = groupConfig.CredentialsProvider,
                    Json = groupConfig.Json,
                    Topics = GetTopicsForDevice(contextData, partnerSettings)
                };
                response.Object = deviceConfig;

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

        private List<string> GetTopicsForDevice(ContextData contextData, NotificationPartnerSettingsResponse ns)
        {
            var response = new List<string>();
            if (ValidateEpgTopicsAllowed(contextData, ns)) // what if unrecognized device
            {
                var partitionNumber = HashUdid(contextData.Udid) % GetTopicPartitionsCount();
                var topicFormat = GetTopicFormat(contextData.GroupId, EventType.epg_update);
                response.Add(string.Format(topicFormat, partitionNumber));
            }
            //TODO - Add other events if allowed?
            return response;
        }

        /// <summary>
        /// Check if epg update is allowed for the given device family Id
        /// </summary>
        /// <param name="contextData"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        private bool ValidateEpgTopicsAllowed(ContextData contextData, NotificationPartnerSettingsResponse ns)
        {
            var resp = Core.Domains.Module.GetDeviceInfo(contextData.GroupId, contextData.Udid, true);
            int familyId;
            if (resp?.m_oDevice == null || resp.m_oDevice.m_deviceFamilyID == 0)
            {
                familyId = 0;
            }
            else
            {
                familyId = resp.m_oDevice.m_deviceFamilyID;
            }
            return ns.settings.EpgNotification.Enabled &&
                (ns.settings.EpgNotification.DeviceFamilyIds.Count == 0 || ns.settings.EpgNotification.DeviceFamilyIds.Contains(familyId));
        }

        private int HashUdid(string udid)
        {
            var hashed = md5.Value.ComputeHash(Encoding.UTF8.GetBytes(udid));
            var result = BitConverter.ToInt32(hashed, 0);
            return Math.Abs(result);
        }

        public int GetTopicPartitionsCount()
        {
            return TOPIC_PARTITIONS_COUNT;
        }

        private IotClientConfiguration GetClientConfigurationCache(int groupId)
        {
            var result = new IotClientConfiguration();
            try
            {
                string key = LayeredCacheKeys.GetGroupIotClientConfig(groupId);
                string invalidationKey = LayeredCacheKeys.GetGroupIotClientConfigInvalidationKey(groupId);

                var success = LayeredCache.Instance.Get
                        (key, ref result, GetClientConfiguration, new Dictionary<string, object>() { { "groupId", groupId } },
                        groupId, LayeredCacheConfigNames.GET_IOT_CLIENT_CONFIGURATION, new List<string> { invalidationKey });

                if (!success || result == null)
                {
                    log.ErrorFormat("Failed GetClientConfiguration, groupId: {0}", groupId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupPermissionItemsDictionary, groupId: {0}", groupId), ex);
            }

            return result;
        }

        public void InvalidateClientConfiguration(int groupId)
        {
            string invalidationKey = LayeredCacheKeys.GetGroupIotClientConfigInvalidationKey(groupId);
            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set invalidation key on InvalidateClientConfiguration key = {0}", invalidationKey);
            }
        }

        private Tuple<IotClientConfiguration, bool> GetClientConfiguration(Dictionary<string, object> funcParams)
        {
            IotClientConfiguration result = null;
            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("groupId"))
                    {
                        int? groupId;
                        groupId = funcParams["groupId"] as int?;

                        if (!groupId.HasValue)
                        {
                            return Tuple.Create(result, false);
                        }

                        var iotProfile = NotificationDal.GetIotProfile(groupId.Value);
                        if (iotProfile == null)
                        {
                            log.Error($"Error while getting configurations for group: {groupId}.");
                            return Tuple.Create(result, false);
                        }

                        result = new IotClientConfiguration
                        {
                            AnnouncementTopic = $"{groupId}/{SYSTEM_ANNOUNCEMENT}",
                            CredentialsProvider = new CredentialsProvider
                            {
                                CognitoIdentity = new CognitoIdentity
                                {
                                    IotDefault = new IotDefault { PoolId = iotProfile.IotProfileAws.IdentityPoolId, Region = iotProfile.IotProfileAws.Region }
                                }
                            },
                            CognitoUserPool = new CognitoUserPool
                            {
                                IotDefault = new IotDefault { PoolId = iotProfile.IotProfileAws.UserPoolId, AppClientId = iotProfile.IotProfileAws.ClientId, Region = iotProfile.IotProfileAws.Region }
                            }
                        };

                        result.Json = result.ToString();

                        return Tuple.Create(result, true);
                    }
                }
                return Tuple.Create(result, false);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetClientConfiguration failed params : {0}", string.Join(";", funcParams.Keys)), ex);
                return Tuple.Create(result, false);
            }
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

                var response = DeleteDevice(m_nGroupID, iotDevice);

                if (!response)
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
                case IotAction.CREATE_ENVIRONMENT:
                    configValue = CREATE_ENVIRONMENT;
                    break;
            }
            return $"{adapterUrl}{configValue}";
        }

        private T SendToAdapter<T>(int groupId, IotAction action, object request, MethodType method, out int httpStatus, out bool hasConfig, IotProfile iotProfile = null, bool groupNeeded = false)
        {
            var url = GetAdapterUrl(groupId, action, iotProfile);
            if (groupNeeded)
                url += $"?groupId={groupId}";

            StringContent content = null;
            hasConfig = true;
            var counter = 0;
            HttpResponseMessage response;
            while (counter < 3)
            {
                counter++;
                switch (method)
                {
                    case MethodType.Post:
                        content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                        response = httpClient.PostAsync(url, content).Result;
                        if (IsServiceStatusDefined(response.StatusCode, out hasConfig)) { httpStatus = (int)response.StatusCode; return default; }
                        if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            httpStatus = (int)response.StatusCode;
                            return NotificationAdapter.ParseResponse<T>(response);
                        }
                        break;

                    case MethodType.Put:
                        content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                        response = httpClient.PutAsync(url, content).Result;
                        if (IsServiceStatusDefined(response.StatusCode, out hasConfig)) { httpStatus = (int)response.StatusCode; return default; }
                        if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            httpStatus = (int)response.StatusCode;
                            return NotificationAdapter.ParseResponse<T>(response);
                        }
                        break;

                    case MethodType.Get:
                        var fullUrl = $"{url}{request}";
                        response = httpClient.GetAsync(fullUrl).Result;
                        if (IsServiceStatusDefined(response.StatusCode, out hasConfig)) { httpStatus = (int)response.StatusCode; return default; }
                        if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            httpStatus = (int)response.StatusCode;
                            return NotificationAdapter.ParseResponse<T>(response);
                        }
                        break;

                    case MethodType.Delete:
                        var _request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Delete,
                            RequestUri = new Uri(url),
                            Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")
                        };
                        response = httpClient.SendAsync(_request).Result;
                        if (IsServiceStatusDefined(response.StatusCode, out hasConfig)) { httpStatus = (int)response.StatusCode; return default; }
                        if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            httpStatus = (int)response.StatusCode;
                            return NotificationAdapter.ParseResponse<T>(response);
                        }
                        break;

                    default:
                        log.Error($"Invalid attempt to call method: {method}, url: {url} group: {groupId}");
                        httpStatus = -1;
                        return default;
                }
            }
            httpStatus = -1;
            return default;
        }

        public IotProfile UpdateIotProfile(int groupId, ContextData contextData)
        {
            log.Info($"No Configurations for group: {groupId} trying to reload from CB");
            var config = NotificationDal.GetIotProfile(groupId);
            if (config == null)
            {
                log.Error($"No configurations were found for group: {groupId}");
                return null;
            }
            return IotProfileManager.Instance.Update(contextData, config)?.Object;
        }

        private bool IsServiceStatusDefined(System.Net.HttpStatusCode status, out bool hasConfig)
        {
            hasConfig = (int)status != 204;
            return new List<int> { 204 /*NoContent*/, 208 /*AlreadyReported*/ }.Contains((int)status);
        }


        #region commonIotRequests
        public bool PublishIotMessage(int groupId, string message, string topic)
        {
            var request = new { GroupId = groupId.ToString(), Message = @message, Topic = topic, ExternalAnnouncementId = "string" };
            var response = SendToAdapter<IotPublishResponse>(groupId, IotAction.PUBLISH, request, MethodType.Post, out int httpStatus, out bool hasConfig);
            return response != null && response.AdapterStatusCode == 0;
        }

        public bool AddToThingShadow(int groupId, string message, string thingArn, string udid)
        {
            var request = new { GroupId = groupId.ToString(), ThingArn = thingArn, Message = message, Udid = udid };
            var response = SendToAdapter<bool>(groupId, IotAction.ADD_TO_SHADOW, request, MethodType.Post, out int httpStatus, out bool hasConfig);
            return response;
        }

        private Iot RegisterDevice(int groupId, string udid)
        {
            var _request = new { GroupId = groupId.ToString(), Udid = udid };
            var response = SendToAdapter<Iot>(groupId, IotAction.REGISTER, _request, MethodType.Post, out int httpStatus, out bool hasConfig);
            return response;
        }

        private bool DeleteDevice(int groupId, Iot iot)
        {
            var _request = new { GroupId = groupId.ToString(), Udid = iot.Udid, IdentityId = iot.IdentityId };
            var response = SendToAdapter<bool>(groupId, IotAction.DELETE_DEVICE, _request, MethodType.Delete, out int httpStatus, out bool hasConfig);
            return response;
        }

        public IotProfileAws CreateIotEnvironment(int groupId, IotProfile iotProfile)
        {
            var _request = new { GroupId = groupId.ToString() };
            var response = SendToAdapter<IotProfileAws>(groupId, IotAction.CREATE_ENVIRONMENT, _request,
                MethodType.Post, out int httpStatus, out bool hasConfig, iotProfile);
            return response;
        }

        public IotProfileAws UpdateIotEnvironment(int groupId, IotProfile newConfigurations)
        {
            return SendToAdapter<IotProfileAws>(groupId, IotAction.ADD_CONFIG, newConfigurations.IotProfileAws, MethodType.Put, out int httpStatus,
                out bool hasConfig, newConfigurations, true);
        }

        /// <summary>
        /// Call the adapter and get the client configuration
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public IotProfileAws GetClientConfiguration(int groupId)
        {
            var urlSuffix = $"groupId={groupId}&forClient={false}";
            return SendToAdapter<IotProfileAws>(groupId, IotAction.GET_IOT_CONFIGURATION, urlSuffix, MethodType.Get, out int httpStatus, out bool hasConfig);
        }
        #endregion
    }

    public enum IotAction
    {
        REGISTER, GET_IOT_CONFIGURATION, ADD_TO_SHADOW, PUBLISH, DELETE_DEVICE, SYSTEM_ANNOUNCEMENT, ADD_CONFIG, CREATE_ENVIRONMENT
    }
    public enum MethodType
    {
        Post, Get, Delete, Put
    }
}