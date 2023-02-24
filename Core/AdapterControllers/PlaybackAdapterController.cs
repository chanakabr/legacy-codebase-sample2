using AdapterControllers.PlaybackAdapter;
using ApiObjects;
using ApiObjects.PlaybackAdapter;
using ApiObjects.Response;
using Phx.Lib.Log;
using Newtonsoft.Json;
using Synchronizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TVinciShared;

namespace AdapterControllers
{
    public class PlaybackAdapterController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int STATUS_OK = 0;
        private const string PARAMETER_GROUP_ID = "group_id";
        private const string PARAMETER_ADAPTER = "adapter";
        private const string LOCKER_STRING_FORMAT = "Playback_Adapter_Locker_{0}";
        private const int STATUS_NO_CONFIGURATION_FOUND = 3;
        private static PlaybackAdapterController instance;
        private static readonly object generalLocker = new object();
        private const int MAX_ALLOWED_DYNAMIC_DATA_PAIRS = 20;


        // Gets the singleton instance of the adapter controller
        public static PlaybackAdapterController GetInstance()
        {
            if (instance == null)
            {
                lock (generalLocker)
                {
                    if (instance == null)
                    {
                        instance = new PlaybackAdapterController();
                    }
                }
            }

            return instance;
        }

        private CouchbaseSynchronizer configurationSynchronizer;
        public PlaybackAdapterController()
        {
            configurationSynchronizer = new CouchbaseSynchronizer(100);
            configurationSynchronizer.SynchronizedAct += configurationSynchronizer_SynchronizedAct;
        }

        public ServiceClient GetPlaybackAdapterServiceClient(string adapterUrl)
        {
            log.Debug($"Constructing GetPlaybackAdapterServiceClient Client with url:[{adapterUrl}]");
            var SSOAdapaterServiceEndpointConfiguration = ServiceClient.EndpointConfiguration.BasicHttpBinding;
            var adapterClient = new ServiceClient(SSOAdapaterServiceEndpointConfiguration, adapterUrl);
            adapterClient.ConfigureServiceClient();

            return adapterClient;
        }

        public bool SendConfiguration(PlaybackProfile adapter, int partnerId)
        {
            bool result = false;

            if (adapter != null && !string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                var client = GetPlaybackAdapterServiceClient(adapter.AdapterUrl);

                //set unixTimestamp
                long unixTimestamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

                //set signature
                string signature = string.Empty;

                try
                {
                    PlaybackAdapter.AdapterStatus adapterResponse =
                        client.SetConfiguration(adapter.Id, adapter.Settings, partnerId, unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null)
                        log.DebugFormat("Playback Adapter Send Configuration Result = {0}", adapterResponse);
                    else
                        log.Debug("Adapter response is null");

                    if (adapterResponse != null && adapterResponse.Code == STATUS_OK)
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed ex = {0}, adapter id = {1}", ex, adapter != null ? adapter.Id : 0);
                }
            }

            return result;
        }

        public ApiObjects.PlaybackAdapter.PlaybackContext GetPlaybackContext(int groupId, PlaybackProfile adapter, string userId, string udid,
            string ip, ApiObjects.PlaybackAdapter.PlaybackContext kalturaPlaybackContext,
            ApiObjects.PlaybackAdapter.RequestPlaybackContextOptions kContextOptions)
        {
            PlaybackContext playbackContext = null;

            if (adapter == null)
            {
                throw new KalturaException(string.Format("playback adapter doesn't exist"), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("playback adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            var adapterClient = GetPlaybackAdapterServiceClient(adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
            string signature = string.Concat(adapter.Id, groupId, userId, udid, ip, unixTimestamp);

            try
            {

                AdapterControllers.PlaybackAdapter.AdapterPlaybackContext adapterPlaybackContext = ParsePlaybackContextToAdapterContext(kalturaPlaybackContext);
                List<KeyValue> playbackAdapterData = new List<KeyValue>();
                if (kContextOptions != null && kContextOptions.AdapterData?.Count > 0)
                {
                    foreach (KeyValuePair<string, string> pair in kContextOptions.AdapterData)
                    {
                        playbackAdapterData.Add(new KeyValue() { Key = pair.Key, Value = pair.Value });
                    }
                }

                PlaybackAdapter.AdapterPlaybackContextOptions contextOption = new PlaybackAdapter.AdapterPlaybackContextOptions()
                {
                    AdapterId = adapter.Id,
                    IP = ip,
                    PlaybackContext = adapterPlaybackContext,
                    PartnerId = groupId,
                    Udid = udid,
                    TimeStamp = unixTimestamp,
                    Signature = System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature))),
                    AdapterData = playbackAdapterData.ToList(),
                    UserId = userId
                };

                PlaybackAdapter.RequestPlaybackContextOptions requestPlaybackContextOptions = null;
                if (kContextOptions != null)
                {
                    requestPlaybackContextOptions = new PlaybackAdapter.RequestPlaybackContextOptions()
                    {
                        AdapterData = playbackAdapterData.ToList(),
                        AssetFileIds = kContextOptions.AssetFileIds,
                        AssetId = kContextOptions.AssetId,
                        AssetType = ParseAssetType(kContextOptions.AssetType),
                        Context = ParseContext(kContextOptions.Context),
                        MediaProtocol = kContextOptions.MediaProtocol,
                        StreamerType = kContextOptions.StreamerType,
                        UrlType = ParseUrlType(kContextOptions.UrlType)
                    };
                }

                PlaybackAdapter.PlaybackAdapterResponse adapterResponse = GetAdapterPlaybackContext(adapterClient, contextOption, requestPlaybackContextOptions);

                if (adapterResponse != null)
                {
                    log.DebugFormat("success. playback adapter response for GetAdapterPlaybackContext: status.code = {0},", adapterResponse.Status != null ? adapterResponse.Status.Code.ToString() : "null");
                    playbackContext = ParsePlaybackContextResponse(adapterResponse);
                }
                else
                {
                    log.Error("playback adapter response for GetAdapterPlaybackContext is null");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetPlaybackContext: adapterId = {0}. ex: {1}", adapter.Id, ex);
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }

            return playbackContext;
        }

        private bool configurationSynchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = false;

            if (parameters != null)
            {
                PlaybackProfile adapter = null;
                int groupId = 0;

                if (parameters.ContainsKey(PARAMETER_ADAPTER))
                {
                    adapter = (PlaybackProfile)parameters[PARAMETER_ADAPTER];
                }

                if (parameters.ContainsKey(PARAMETER_GROUP_ID))
                {
                    groupId = (int)parameters[PARAMETER_GROUP_ID];
                }

                result = this.SendConfiguration(adapter, groupId);
            }

            return result;
        }

        private static PlaybackAdapter.PlaybackAdapterResponse GetAdapterPlaybackContext(PlaybackAdapter.ServiceClient adapterClient, PlaybackAdapter.AdapterPlaybackContextOptions contextOptions,
            PlaybackAdapter.RequestPlaybackContextOptions requestPlaybackContextOptions)
        {
            PlaybackAdapter.PlaybackAdapterResponse adapterResponse;

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
            {
                log.Debug($"GetAdapterPlaybackContext " +
                    $"contextOptions: {JsonConvert.SerializeObject(contextOptions)}, " +
                    $"request: {JsonConvert.SerializeObject(requestPlaybackContextOptions)}");
                //call adapter
                adapterResponse = adapterClient.GetPlaybackContext(contextOptions, requestPlaybackContextOptions);
            }

            if (adapterResponse != null)
            {
                log.Debug($"GetAdapterPlaybackContext response: {JsonConvert.SerializeObject(adapterResponse)}");

                log.DebugFormat("success. playback adapter response for GetPlaybackContext: status.code = {0}",
                    adapterResponse.Status != null ? adapterResponse.Status.Code.ToString() : "null");
            }
            else
            {
                log.Error("playback adapter response for GetPlaybackContext is null");
            }

            return adapterResponse;
        }
        private static ApiObjects.PlaybackAdapter.PlaybackContext ParsePlaybackContextResponse(PlaybackAdapter.PlaybackAdapterResponse adapterResponse)
        {
            ApiObjects.PlaybackAdapter.PlaybackContext kalturaPlaybackContext = null;
            if (adapterResponse != null && adapterResponse.Status != null)
            {
                // If something went wrong in the adapter, throw relevant exception
                if (adapterResponse.Status.Code != (int)eResponseStatus.OK || adapterResponse.PlaybackContext == null)
                {
                    throw new KalturaException("Playback adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                }

                kalturaPlaybackContext = new ApiObjects.PlaybackAdapter.PlaybackContext();
                if (adapterResponse.PlaybackContext.Actions != null)
                {
                    kalturaPlaybackContext.Actions = adapterResponse.PlaybackContext.Actions.Select(x =>
                        new ApiObjects.PlaybackAdapter.RuleAction()
                        {
                            Description = x.Description,
                            Type = (ApiObjects.PlaybackAdapter.RuleActionType)x.Type
                        }).ToList();
                }

                if (adapterResponse.PlaybackContext.Messages != null)
                {
                    kalturaPlaybackContext.Messages = adapterResponse.PlaybackContext.Messages.Select(x =>
                        new ApiObjects.PlaybackAdapter.AccessControlMessage()
                        {
                            Code = x.Code,
                            Message = x.Message
                        }).ToList();
                }

                if (adapterResponse.PlaybackContext.Sources != null)
                {
                    kalturaPlaybackContext.Sources = adapterResponse.PlaybackContext.Sources.Select(x =>
                        new ApiObjects.PlaybackAdapter.PlaybackSource()
                        {
                            AssetId = x.AssetId,
                            Id = x.Id,
                            TypeId = x.TypeId,
                            Duration = x.Duration,
                            AdditionalData = x.AdditionalData,
                            AlternativeCdnAdapaterProfileId = x.AlternativeCdnAdapaterProfileId,
                            AltExternalId = x.AltExternalId,
                            AltStreamingCode = x.AltStreamingCode,
                            CatalogEndDate = x.CatalogEndDate,
                            CdnAdapaterProfileId = x.CdnAdapaterProfileId,
                            EndDate = x.EndDate,
                            ExternalId = x.ExternalId,
                            ExternalStoreId = x.ExternalStoreId,
                            FileSize = x.FileSize,
                            IsDefaultLanguage = x.IsDefaultLanguage,
                            Language = x.Language,
                            OrderNum = x.OrderNum,
                            OutputProtecationLevel = x.OutputProtecationLevel,
                            StartDate = x.StartDate,
                            Status = x.Status,
                            Url = x.Url,
                            AltUrl = x.AltUrl,
                            Drm = x.Drm == null ? null : ParseDrm(x.Drm),
                            Format = x.Format,
                            IsTokenized = x.IsTokenized,
                            Protocols = x.Protocols,
                            Type = x.Type,
                            BusinessModuleDetails = new BusinessModuleDetails
                            {
                                BusinessModuleId = x.BusinessModuleId,
                                BusinessModuleType = ConvertModuleType(x.BusinessModuleType)
                            },
                            Labels = x.Labels,
                            DynamicData = x.DynamicData?.ToDictionary(_ => _.Key, _ => _.Value.AsEnumerable())
                        }).ToList();
                }

                if (adapterResponse.PlaybackContext.Plugins != null)
                {
                    kalturaPlaybackContext.Plugins = ParsePlugins(adapterResponse.PlaybackContext.Plugins);
                }

                if (adapterResponse.PlaybackContext.PlaybackCaptions != null)
                {
                    kalturaPlaybackContext.PlaybackCaptions = adapterResponse.PlaybackContext.PlaybackCaptions.Select(x =>
                        new ApiObjects.PlaybackAdapter.CaptionPlaybackPluginData()
                        {
                            URL = x.URL,
                            Format = x.Format,
                            Label = x.Label,
                            Language = x.Language
                        }).ToList();
                }
            }

            return kalturaPlaybackContext;
        }

        private static eTransactionType? ConvertModuleType(TransactionType? businessModuleType)
        {
            if (!businessModuleType.HasValue)
            {
                return null;
            }
            switch (businessModuleType)
            {
                case TransactionType.Collection:
                    return eTransactionType.Collection;
                case TransactionType.PPV:
                    return eTransactionType.PPV;
                case TransactionType.Subscription:
                    return eTransactionType.Subscription;
                case TransactionType.PAGO:
                    return eTransactionType.ProgramAssetGroupOffer;
                default:
                    throw new KalturaException($"Unknown Transaction Type: {businessModuleType}", (int)eResponseStatus.Error);
            }
        }

        private static TransactionType? ConvertModuleType(eTransactionType? businessModuleType)
        {
            if (!businessModuleType.HasValue)
            {
                return null;
            }
            switch (businessModuleType)
            {
                case eTransactionType.Collection:
                    return TransactionType.Collection;
                case eTransactionType.PPV:
                    return TransactionType.PPV;
                case eTransactionType.Subscription:
                    return TransactionType.Subscription;
                case eTransactionType.ProgramAssetGroupOffer:
                    return TransactionType.PAGO;
                default:
                    throw new KalturaException($"Unknown Transaction Type: {businessModuleType}", (int)eResponseStatus.Error);
            }
        }

        private static List<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData> ParseDrm(List<PlaybackAdapter.DrmPlaybackPluginData> drms)
        {
            var drmPlaybackPluginDatas = new List<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData>();

            ApiObjects.PlaybackAdapter.DrmPlaybackPluginData drm;
            foreach (var item in drms)
            {
                if (item is PlaybackAdapter.FairPlayPlaybackPluginData)
                {
                    PlaybackAdapter.FairPlayPlaybackPluginData tmpDrm = item as PlaybackAdapter.FairPlayPlaybackPluginData;
                    drm = new ApiObjects.PlaybackAdapter.FairPlayPlaybackPluginData()
                    {
                        Certificate = tmpDrm.Certificate,
                    };
                }
                else if (item is PlaybackAdapter.CustomDrmPlaybackPluginData)
                {
                    PlaybackAdapter.CustomDrmPlaybackPluginData tmpDrm = item as PlaybackAdapter.CustomDrmPlaybackPluginData;
                    drm = new ApiObjects.PlaybackAdapter.CustomDrmPlaybackPluginData()
                    {
                        Data = tmpDrm.Data,
                    };
                }
                else
                {
                    drm = new ApiObjects.PlaybackAdapter.CustomDrmPlaybackPluginData();
                }

                drm.LicenseURL = item.LicenseURL;
                drm.Scheme = (ApiObjects.PlaybackAdapter.DrmSchemeName)item.Scheme;

                if (item?.DynamicData?.Count > 0)
                {
                    var isValidDynamicData = ValidateDrmDynamicData(item.DynamicData);
                    if (!isValidDynamicData.IsOkStatusCode())
                    {
                        throw new KalturaException(isValidDynamicData.Message, (int)eResponseStatus.Error);
                    }
                    drm.DynamicData = item.DynamicData.ToDictionary(x => x.Key, x => x.Value);
                }

                drmPlaybackPluginDatas.Add(drm);
            }

            return drmPlaybackPluginDatas;
        }

        private static Status ValidateDrmDynamicData(List<KeyValue> dynamicData = null)
        {
            var status = new Status();

            if (dynamicData?.Count > MAX_ALLOWED_DYNAMIC_DATA_PAIRS)
            {
                var error = $"DynamicData failed validation, exceeded maximum allowed amount of {MAX_ALLOWED_DYNAMIC_DATA_PAIRS}";
                log.Error(error);
                status.Set(eResponseStatus.Error, error);
            }
            else
            {
                status.Set(eResponseStatus.OK);
            }

            return status;
        }

        private static Status ValidateDrmDynamicData(Dictionary<string, string> dynamicData = null)
        {
            var status = new Status();

            if (dynamicData?.Count > MAX_ALLOWED_DYNAMIC_DATA_PAIRS)
            {
                var error = $"DynamicData failed validation, exceeded maximum allowed amount of {MAX_ALLOWED_DYNAMIC_DATA_PAIRS}";
                log.Error(error);
                status.Set(eResponseStatus.Error, error);
            }
            else
            {
                status.Set(eResponseStatus.OK);
            }

            return status;
        }

        private List<PlaybackAdapter.DrmPlaybackPluginData> ParseDrm(List<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData> drms)
        {
            List<PlaybackAdapter.DrmPlaybackPluginData> drmPlaybackPluginDatas = new List<PlaybackAdapter.DrmPlaybackPluginData>();

            PlaybackAdapter.DrmPlaybackPluginData drm;
            foreach (var item in drms)
            {
                if (item is ApiObjects.PlaybackAdapter.FairPlayPlaybackPluginData)
                {
                    var tmpDrm = item as ApiObjects.PlaybackAdapter.FairPlayPlaybackPluginData;
                    drm = new PlaybackAdapter.FairPlayPlaybackPluginData()
                    {
                        Certificate = tmpDrm.Certificate,
                    };
                }
                else if (item is ApiObjects.PlaybackAdapter.CustomDrmPlaybackPluginData)
                {
                    var tmpDrm = item as ApiObjects.PlaybackAdapter.CustomDrmPlaybackPluginData;
                    drm = new PlaybackAdapter.CustomDrmPlaybackPluginData()
                    {
                        Data = tmpDrm.Data,
                    };
                }
                else
                {
                    drm = new PlaybackAdapter.CustomDrmPlaybackPluginData();
                }

                drm.LicenseURL = item.LicenseURL;
                drm.Scheme = (PlaybackAdapter.DrmSchemeName)item.Scheme;

                if (item?.DynamicData?.Count > 0)
                {
                    var isValidDynamicData = ValidateDrmDynamicData(item.DynamicData);
                    if (!isValidDynamicData.IsOkStatusCode())
                    {
                        throw new KalturaException(isValidDynamicData.Message, (int)eResponseStatus.Error);
                    }
                    drm.DynamicData = item.DynamicData.Select(x => new KeyValue() { Key = x.Key, Value = x.Value })?.ToList();
                }

                drmPlaybackPluginDatas.Add(drm);
            }

            return drmPlaybackPluginDatas;
        }

        private PlaybackAdapter.AdapterPlaybackContext ParsePlaybackContextToAdapterContext(PlaybackContext kalturaPlaybackContext)
        {
            PlaybackAdapter.AdapterPlaybackContext playbackContext = null;
            if (kalturaPlaybackContext != null)
            {
                playbackContext = new PlaybackAdapter.AdapterPlaybackContext();
                if (kalturaPlaybackContext.Actions != null)
                {
                    playbackContext.Actions = kalturaPlaybackContext.Actions.Select(x =>
                         new PlaybackAdapter.RuleAction()
                         {
                             Description = x.Description,
                             Type = (PlaybackAdapter.RuleActionType)x.Type

                         }).ToList();
                }

                if (kalturaPlaybackContext.Messages != null)
                {
                    playbackContext.Messages = kalturaPlaybackContext.Messages.Select(x =>
                        new PlaybackAdapter.AccessControlMessage()
                        {
                            Code = x.Code,
                            Message = x.Message
                        }).ToList();
                }

                if (kalturaPlaybackContext.Sources != null)
                {
                    playbackContext.Sources = kalturaPlaybackContext.Sources.Select(x =>
                        new PlaybackAdapter.PlaybackSource()
                        {
                            AssetId = x.AssetId,
                            Id = x.Id,
                            TypeId = x.TypeId,
                            Duration = x.Duration,
                            AdditionalData = x.AdditionalData,
                            AlternativeCdnAdapaterProfileId = x.AlternativeCdnAdapaterProfileId,
                            AltExternalId = x.AltExternalId,
                            AltStreamingCode = x.AltStreamingCode,
                            CatalogEndDate = x.CatalogEndDate,
                            CdnAdapaterProfileId = x.CdnAdapaterProfileId,
                            EndDate = x.EndDate,
                            ExternalId = x.ExternalId,
                            ExternalStoreId = x.ExternalStoreId,
                            FileSize = x.FileSize,
                            IsDefaultLanguage = x.IsDefaultLanguage,
                            Language = x.Language,
                            OrderNum = x.OrderNum,
                            OutputProtecationLevel = x.OutputProtecationLevel,
                            StartDate = x.StartDate,
                            Status = x.Status,
                            AltUrl = x.AltUrl,
                            Url = x.Url,
                            Drm = x.Drm == null ? null : ParseDrm(x.Drm),
                            Format = x.Format,
                            IsTokenized = x.IsTokenized,
                            Protocols = x.Protocols,
                            Type = x.Type,
                            BusinessModuleId = x.BusinessModuleDetails?.BusinessModuleId,
                            BusinessModuleType = ConvertModuleType(x.BusinessModuleDetails?.BusinessModuleType),
                            Labels = x.Labels,
                            DynamicData = x.DynamicData?.Select(_ => new KeyListOfStrings { Key = _.Key, Value = _.Value.ToList() }).ToList()
                        }).ToList();
                }

                if (kalturaPlaybackContext.Plugins != null)
                {
                    playbackContext.Plugins = ParsePlugins(kalturaPlaybackContext.Plugins);
                }

                if (kalturaPlaybackContext.PlaybackCaptions != null)
                {
                    playbackContext.PlaybackCaptions = kalturaPlaybackContext.PlaybackCaptions.Select(x =>
                         new PlaybackAdapter.CaptionPlaybackPluginData()
                         {
                             URL = x.URL,
                             Format = x.Format,
                             Label = x.Label,
                             Language = x.Language
                         }).ToList();
                }
            }

            return playbackContext;
        }

        private List<PlaybackAdapter.PlaybackPluginData> ParsePlugins(List<ApiObjects.PlaybackAdapter.PlaybackPluginData> plugins)
        {
            List<PlaybackAdapter.PlaybackPluginData> playbackPluginDatas = new List<PlaybackAdapter.PlaybackPluginData>();
            PlaybackAdapter.PlaybackPluginData playbackPluginData;

            foreach (var item in plugins)
            {
                if (item is ApiObjects.PlaybackAdapter.BumperPlaybackPluginData)
                {
                    ApiObjects.PlaybackAdapter.BumperPlaybackPluginData tmp = item as ApiObjects.PlaybackAdapter.BumperPlaybackPluginData;
                    playbackPluginData = new PlaybackAdapter.BumperPlaybackPluginData()
                    {
                        StreamerType = tmp.StreamerType,
                        URL = tmp.URL
                    };
                }
                else
                {
                    playbackPluginData = new PlaybackAdapter.PlaybackPluginData();
                }

                playbackPluginDatas.Add(playbackPluginData);
            }

            return playbackPluginDatas;
        }

        private static List<ApiObjects.PlaybackAdapter.PlaybackPluginData> ParsePlugins(List<PlaybackAdapter.PlaybackPluginData> plugins)
        {
            List<ApiObjects.PlaybackAdapter.PlaybackPluginData> playbackPluginDatas = new List<ApiObjects.PlaybackAdapter.PlaybackPluginData>();
            ApiObjects.PlaybackAdapter.PlaybackPluginData playbackPluginData;

            foreach (var item in plugins)
            {
                if (item is PlaybackAdapter.BumperPlaybackPluginData)
                {
                    PlaybackAdapter.BumperPlaybackPluginData tmp = item as PlaybackAdapter.BumperPlaybackPluginData;
                    playbackPluginData = new ApiObjects.PlaybackAdapter.BumperPlaybackPluginData()
                    {
                        StreamerType = tmp.StreamerType,
                        URL = tmp.URL
                    };
                }
                else
                {
                    playbackPluginData = new ApiObjects.PlaybackAdapter.BumperPlaybackPluginData();
                }

                playbackPluginDatas.Add(playbackPluginData);
            }

            return playbackPluginDatas;
        }

        private AdapterUrlType ParseUrlType(UrlType urlType)
        {
            switch (urlType)
            {
                case UrlType.playmanifest:
                    return AdapterUrlType.PLAYMANIFEST;
                case UrlType.direct:
                    return AdapterUrlType.DIRECT;
                default:
                    throw new KalturaException("Unknown UrlType value", (int)eResponseStatus.Error);
            }
        }

        private AdapterPlaybackContextType? ParseContext(PlayContextType? context)
        {
            if (context.HasValue)
            {
                switch (context.Value)
                {
                    case PlayContextType.CatchUp:
                        return AdapterPlaybackContextType.CATCHUP;
                    case PlayContextType.Download:
                        return AdapterPlaybackContextType.DOWNLOAD;
                    case PlayContextType.Playback:
                        return AdapterPlaybackContextType.PLAYBACK;
                    case PlayContextType.StartOver:
                        return AdapterPlaybackContextType.START_OVER;
                    case PlayContextType.Trailer:
                        return AdapterPlaybackContextType.TRAILER;
                    default:
                        throw new KalturaException("Unknown PlayContextType value", (int)eResponseStatus.Error);
                }
            }

            return null;
        }

        private AdapterAssetType ParseAssetType(eAssetTypes assetType)
        {
            switch (assetType)
            {
                case eAssetTypes.EPG:
                    return AdapterAssetType.epg;
                case eAssetTypes.NPVR:
                    return AdapterAssetType.recording;
                case eAssetTypes.MEDIA:
                    return AdapterAssetType.media;
                case eAssetTypes.UNKNOWN:
                default:
                    throw new KalturaException("Unknown eAssetTypes value", (int)eResponseStatus.Error);
            }
        }

        public PlaybackContext GetPlaybackManifest(int groupId, PlaybackProfile adapter, ApiObjects.PlaybackAdapter.PlaybackContext kalturaPlaybackContext,
            ApiObjects.PlaybackAdapter.RequestPlaybackContextOptions kContextOptions, string userId, string udid, string ip)
        {
            PlaybackContext playbackContext = null;

            if (adapter == null)
            {
                throw new KalturaException(string.Format("playback adapter doesn't exist"), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("playback adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            var adapterClient = GetPlaybackAdapterServiceClient(adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
            string signature = string.Concat(adapter.Id, groupId, string.Empty, string.Empty, string.Empty, unixTimestamp); //TODO anat: ask Ira

            try
            {

                AdapterControllers.PlaybackAdapter.AdapterPlaybackContext adapterPlaybackContext = ParsePlaybackContextToAdapterContext(kalturaPlaybackContext);
                List<KeyValue> playbackAdapterData = new List<KeyValue>();
                if (kContextOptions != null && kContextOptions.AdapterData?.Count > 0)
                {
                    foreach (KeyValuePair<string, string> pair in kContextOptions.AdapterData)
                    {
                        playbackAdapterData.Add(new KeyValue() { Key = pair.Key, Value = pair.Value });
                    }
                }

                PlaybackAdapter.AdapterPlaybackContextOptions contextOption = new PlaybackAdapter.AdapterPlaybackContextOptions()
                {
                    AdapterId = adapter.Id,
                    PlaybackContext = adapterPlaybackContext,
                    PartnerId = groupId,
                    TimeStamp = unixTimestamp,
                    Signature = System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature))),
                    AdapterData = playbackAdapterData.ToList(),
                    UserId = userId,
                    Udid = udid,
                    IP = ip
                };

                PlaybackAdapter.RequestPlaybackContextOptions requestPlaybackContextOptions = null;
                if (kContextOptions != null)
                {
                    requestPlaybackContextOptions = new PlaybackAdapter.RequestPlaybackContextOptions()
                    {
                        AdapterData = playbackAdapterData.ToList(),
                        AssetFileIds = kContextOptions.AssetFileIds,
                        AssetId = kContextOptions.AssetId,
                        AssetType = ParseAssetType(kContextOptions.AssetType),
                        Context = ParseContext(kContextOptions.Context),
                        MediaProtocol = kContextOptions.MediaProtocol,
                        StreamerType = kContextOptions.StreamerType,
                        UrlType = ParseUrlType(kContextOptions.UrlType)
                    };
                }

                PlaybackAdapter.PlaybackAdapterResponse adapterResponse = GetAdapterPlaybackManifest(adapterClient, contextOption, requestPlaybackContextOptions);

                if (adapterResponse != null)
                {
                    log.DebugFormat("success. playback adapter response for GetAdapterPlaybackContext: status.code = {0},", adapterResponse.Status != null ? adapterResponse.Status.Code.ToString() : "null");
                    playbackContext = ParsePlaybackContextResponse(adapterResponse);
                }
                else
                {
                    log.Error("GetPlaybackManifest. playback adapter response for GetAdapterPlaybackContext is null");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetPlaybackManifest: adapterId = {0}. ex: {1}", adapter.Id, ex);
                throw new KalturaException("Adapter failed completing request. GetPlaybackManifest", (int)eResponseStatus.AdapterAppFailure);
            }

            return playbackContext;
        }

        private static PlaybackAdapter.PlaybackAdapterResponse GetAdapterPlaybackManifest(PlaybackAdapter.ServiceClient adapterClient, PlaybackAdapter.AdapterPlaybackContextOptions contextOptions,
            PlaybackAdapter.RequestPlaybackContextOptions requestPlaybackContextOptions)
        {
            PlaybackAdapter.PlaybackAdapterResponse adapterResponse;

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
            {
                //call adapter
                adapterResponse = adapterClient.GetPlaybackManifest(contextOptions, requestPlaybackContextOptions);
            }

            if (adapterResponse != null)
            {
                log.DebugFormat("success. playback adapter response for GetPlaybackManifest: status.code = {0}",
                    adapterResponse.Status != null ? adapterResponse.Status.Code.ToString() : "null");
            }
            else
            {
                log.Error("playback adapter response for GetPlaybackManifest is null");
            }

            return adapterResponse;
        }
    }
}