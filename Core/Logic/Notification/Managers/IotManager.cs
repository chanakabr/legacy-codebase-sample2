using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ApiLogic.Api.Managers;
using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.EventBus;
using ApiObjects.Notification;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using Core.Catalog.CatalogManagement;
using Core.Domains;
using Core.Notification;
using Core.Notification.Adapters;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using TVinciShared;
using Module = Core.Domains.Module;

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
        string GetRegionTopicFormat(int groupId, EventType eventType, long regionId);
        IotProfileAws GetClientConfiguration(int groupId);
        void InvalidateClientConfiguration(int groupId);
    }

    public class IotManager : IIotManager
    {
        private static readonly Lazy<IotManager> IoTManagerLazy = new Lazy<IotManager>(
            () => new IotManager(), LazyThreadSafetyMode.PublicationOnly);
        private static readonly KLogger Logger = new KLogger(nameof(IotManager));
        private static readonly Lazy<HttpClient> HttpClientLazy = new Lazy<HttpClient>(
            () => HttpClientUtil.GetHttpClient(ApplicationConfiguration.Current.IotHttpClientConfiguration),
            LazyThreadSafetyMode.PublicationOnly);
        private readonly ThreadLocal<MD5> _md5 = new ThreadLocal<MD5>(MD5.Create);
        private readonly ICatalogManager _catalogManager;
        private readonly IRegionManager _regionManager;
        private readonly INotificationCache _notificationCache;
        private readonly INotificationDal _notificationDal;
        private readonly ILayeredCache _layeredCache;
        private readonly IDomainModule _domainModule;

        private const string REGISTER = "/api/IOT/RegisterDevice";
        private const string GET_IOT_CONFIGURATION = "/api/IOT/Configuration/List?";
        private const string ADD_TO_SHADOW = "/api/IOT/AddToShadow";
        private const string PUBLISH = "/api/IOT/Publish";
        private const string DELETE_DEVICE = "/api/IOT/DeleteDevice";
        public const string SYSTEM_ANNOUNCEMENT = "SystemAnnouncement";
        private const string ADD_CONFIG = "/api/IOT/Configuration/Update";
        private const string CREATE_ENVIRONMENT = "/api/IOT/Environment/Create";
        private const int TOPIC_PARTITIONS_COUNT = 10;
        public static IotManager Instance => IoTManagerLazy.Value;
        private static HttpClient HttpClient => HttpClientLazy.Value;

        private IotManager() : this(
            CatalogManager.Instance,
            RegionManager.Instance,
            NotificationCache.Instance(),
            NotificationDal.Instance,
            LayeredCache.Instance,
            Module.Instance)
        {
        }

        public IotManager(ICatalogManager catalogManager,
            IRegionManager regionManager,
            INotificationCache notificationCache,
            INotificationDal notificationDal,
            ILayeredCache layeredCache,
            IDomainModule domainModule)
        {
            _catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _notificationCache = notificationCache ?? throw new ArgumentNullException(nameof(notificationCache));
            _notificationDal = notificationDal ?? throw new ArgumentNullException(nameof(notificationDal));
            _layeredCache = layeredCache ?? throw new ArgumentNullException(nameof(layeredCache));
            _domainModule = domainModule ?? throw new ArgumentNullException(nameof(domainModule));
        }

        public GenericResponse<Iot> Register(ContextData contextData)
        {
            var response = new GenericResponse<Iot>();
            var groupId = contextData.GroupId;
            var udid = contextData.Udid;
            try
            {
                var partnerSettings = _notificationCache.GetPartnerNotificationSettings(groupId);

                if (!IsIotAllowed(partnerSettings))
                {
                    Logger.Error($"Error while getting PartnerNotificationSettings for group: {groupId}.");
                    return response;
                }

                response.Object = RegisterDevice(groupId, udid);

                if (response.Object == null)
                {
                    Logger.Error($"Error while registering udid: {udid} for group: {groupId}.");
                    return response;
                }

                var saved = SaveRegisteredDevice(groupId, udid, response.Object);

                response.SetStatus(saved ? Status.Ok : Status.Error);
            }
            catch (Exception ex)
            {
                Logger.Error($"Register failed, group: {groupId}, error: {ex.Message}", ex);
            }

            return response;
        }

        public static bool IsIotAllowed(NotificationPartnerSettingsResponse partnerSettings)
            => partnerSettings?.settings?.IsIotEnabled == true;

        private bool SaveRegisteredDevice(int groupId, string udid, Iot msResponse)
        {
            msResponse.Udid = msResponse.Udid ?? udid;
            msResponse.GroupId = msResponse.GroupId ?? groupId.ToString();

            if (_notificationDal.SaveRegisteredDevice(msResponse))
            {
                return true;
            }

            Logger.ErrorFormat($"Error while saving Iot device. Iot response: {JsonConvert.SerializeObject(msResponse)}");

            return false;
        }

        public string GetTopicFormat(int groupId, EventType eventType) => $"{groupId}/{eventType.ToString()}/{{0}}";

        public string GetRegionTopicFormat(int groupId, EventType eventType, long regionId)
            => $"{groupId}/{eventType.ToString()}/{regionId}/{{0}}";

        public GenericResponse<IotClientConfiguration> GetClientConfiguration(ContextData contextData)
        {
            var response = new GenericResponse<IotClientConfiguration>();
            var groupId = contextData.GroupId;
            try
            {
                var partnerSettings = _notificationCache.GetPartnerNotificationSettings(groupId);
                if (!IsIotAllowed(partnerSettings))
                {
                    Logger.Error($"Error while getting PartnerNotificationSettings for group: {groupId}.");
                    return response;
                }

                var groupConfig = GetClientConfigurationCache(groupId);
                var deviceConfig = new IotClientConfiguration
                {
                    AnnouncementTopic = groupConfig.AnnouncementTopic,
                    CognitoUserPool = groupConfig.CognitoUserPool,
                    CredentialsProvider = groupConfig.CredentialsProvider,
                    Json = groupConfig.Json,
                    Topics = GetTopicsForDevice(contextData, partnerSettings.settings)
                };
                response.Object = deviceConfig;

                response.SetStatus(Status.Ok);
            }
            catch (ConfigurationErrorsException)
            {
                Logger.Error($"No configurations for group: {groupId}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"GetIotClientConfiguration failed, error: {ex.Message}");
            }

            return response;
        }

        private List<string> GetTopicsForDevice(ContextData contextData, NotificationPartnerSettings settings)
        {
            var response = new List<string>();
            var epgTopicFormat = GetEpgTopicFormat(contextData, settings);
            var partitionNumber = HashUdid(contextData.Udid) % GetTopicPartitionsCount();
            if (!string.IsNullOrEmpty(epgTopicFormat)) // what if unrecognized device
            {
                response.Add(string.Format(epgTopicFormat, partitionNumber));
            }

            var lineupTopicFormat = GetLineupTopicFormat(contextData, settings);
            if (!string.IsNullOrEmpty(lineupTopicFormat))
            {
                response.Add(string.Format(lineupTopicFormat, partitionNumber));
            }

            return response;
        }

        private string GetEpgTopicFormat(ContextData contextData, NotificationPartnerSettings settings)
        {
            if (!settings.EpgNotification.Enabled)
            {
                return null;
            }

            if (!IsValidDeviceFamily(contextData, settings.EpgNotification.DeviceFamilyIds))
            {
                return null;
            }

            return !_catalogManager.IsRegionalizationEnabled(contextData.GroupId)
                ? GetTopicFormat(contextData.GroupId, EventType.epg_update)
                : GetRegionTopicFormat(contextData, EventType.epg_update);
        }

        private string GetLineupTopicFormat(ContextData contextData, NotificationPartnerSettings settings)
        {
            if (!settings.LineupNotification.Enabled)
            {
                return null;
            }

            return !_catalogManager.IsRegionalizationEnabled(contextData.GroupId)
                ? null
                : GetRegionTopicFormat(contextData, EventType.lineup_updated);
        }

        private string GetRegionTopicFormat(ContextData contextData, EventType eventType)
        {
            if (contextData.RegionId.HasValue)
            {
                return GetRegionTopicFormat(contextData.GroupId, eventType, contextData.RegionId.Value);
            }

            var defaultRegionId = _regionManager.GetDefaultRegionId(contextData.GroupId);
            if (!defaultRegionId.HasValue)
            {
                Logger.Error("Regionalization is enabled but default region is not set. EPG topic is not available.");

                return null;
            }

            return GetRegionTopicFormat(contextData.GroupId, eventType, defaultRegionId.Value);
        }

        private bool IsValidDeviceFamily(ContextData contextData, IReadOnlyCollection<int> deviceFamilyIds)
        {
            var deviceInfoResponse = _domainModule.GetDeviceInfo(contextData.GroupId, contextData.Udid, true);
            var familyId = deviceInfoResponse?.m_oDevice == null || deviceInfoResponse.m_oDevice.m_deviceFamilyID == 0
                ? default
                : deviceInfoResponse.m_oDevice.m_deviceFamilyID;

            return deviceFamilyIds.Count == 0 || deviceFamilyIds.Contains(familyId);
        }

        private int HashUdid(string udid)
        {
            var hashed = _md5.Value.ComputeHash(Encoding.UTF8.GetBytes(udid));
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
                var key = LayeredCacheKeys.GetGroupIotClientConfig(groupId);
                var invalidationKey = LayeredCacheKeys.GetGroupIotClientConfigInvalidationKey(groupId);
                var success = _layeredCache.Get(
                    key,
                    ref result,
                    GetClientConfiguration,
                    new Dictionary<string, object> { { nameof(groupId), groupId } },
                    groupId,
                    LayeredCacheConfigNames.GET_IOT_CLIENT_CONFIGURATION,
                    new List<string> { invalidationKey });

                if (!success || result == null)
                {
                    Logger.ErrorFormat($"Failed GetClientConfiguration, groupId: {groupId}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed GetGroupPermissionItemsDictionary, groupId: {groupId}", ex);
            }

            return result;
        }

        public void InvalidateClientConfiguration(int groupId)
        {
            string invalidationKey = LayeredCacheKeys.GetGroupIotClientConfigInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                Logger.ErrorFormat("Failed to set invalidation key on InvalidateClientConfiguration key = {0}", invalidationKey);
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

                        var iotProfile = _notificationDal.GetIotProfile(groupId.Value);
                        if (iotProfile == null)
                        {
                            Logger.Error($"Error while getting configurations for group: {groupId}.");
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
                Logger.Error(string.Format("GetClientConfiguration failed params : {0}", string.Join(";", funcParams.Keys)), ex);
                return Tuple.Create(result, false);
            }
        }

        public Status Delete(ContextData contextData, long id) => throw new NotImplementedException();

        public bool RemoveFromIot(int m_nGroupID, string udid)
        {
            try
            {
                var iotDevice = _notificationDal.GetRegisteredDevice(m_nGroupID.ToString(), udid);
                if (iotDevice == null)
                {
                    Logger.Error($"Device {udid} not found");
                    return false;
                }

                var response = DeleteDevice(m_nGroupID, iotDevice);

                if (!response)
                {
                    Logger.Error($"Failed removing udid: {udid} from group: {m_nGroupID}");
                    return false;
                }

                if (!_notificationDal.RemoveRegisteredDevice(m_nGroupID.ToString(), udid))
                {
                    Logger.Error($"Failed to delete iot document for device: {udid}, group: {m_nGroupID}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed removing udid: {udid} from group: {m_nGroupID} by exception: {ex.Message}");
                return false;
            }

            return true;
        }

        public GenericResponse<Iot> Get(ContextData contextData, long id) => throw new NotImplementedException();

        private string GetAdapterUrl(int groupId, IotAction action, IotProfile iotProfile = null)
        {
            iotProfile = iotProfile ?? _notificationDal.GetIotProfile(groupId);
            var adapterUrl = iotProfile?.AdapterUrl;

            if (string.IsNullOrEmpty(adapterUrl))
            {
                Logger.Error("Iot Notification URL wasn't found");
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
            var _random = new Random();
            while (counter < 3)
            {
                counter++;
                switch (method)
                {
                    case MethodType.Post:
                        content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                        response = HttpClient.PostAsync(url, content).Result;
                        if (IsServiceStatusDefined(response.StatusCode, out hasConfig)) { httpStatus = (int)response.StatusCode; return default; }
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                        {
                            httpStatus = (int)response.StatusCode;
                            return NotificationAdapter.ParseResponse<T>(response);
                        }
                        break;

                    case MethodType.Put:
                        content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                        response = HttpClient.PutAsync(url, content).Result;
                        if (IsServiceStatusDefined(response.StatusCode, out hasConfig)) { httpStatus = (int)response.StatusCode; return default; }
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                        {
                            httpStatus = (int)response.StatusCode;
                            return NotificationAdapter.ParseResponse<T>(response);
                        }
                        break;

                    case MethodType.Get:
                        var fullUrl = $"{url}{request}";
                        response = HttpClient.GetAsync(fullUrl).Result;
                        if (IsServiceStatusDefined(response.StatusCode, out hasConfig)) { httpStatus = (int)response.StatusCode; return default; }
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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
                        response = HttpClient.SendAsync(_request).Result;
                        if (IsServiceStatusDefined(response.StatusCode, out hasConfig)) { httpStatus = (int)response.StatusCode; return default; }
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                        {
                            httpStatus = (int)response.StatusCode;
                            return NotificationAdapter.ParseResponse<T>(response);
                        }
                        break;

                    default:
                        Logger.Error($"Invalid attempt to call method: {method}, url: {url} group: {groupId}");
                        httpStatus = -1;
                        return default;
                }
                Thread.Sleep(counter * _random.Next(0, 100));
            }
            httpStatus = -1;
            return default;
        }

        private bool IsServiceStatusDefined(HttpStatusCode status, out bool hasConfig)
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