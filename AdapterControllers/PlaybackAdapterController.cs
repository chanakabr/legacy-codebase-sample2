using AdapterControllers.PlaybackAdapter;
using ApiObjects;
using ApiObjects.PlaybackAdapter;
using ApiObjects.Response;
using KLogMonitor;
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

        public bool SendConfiguration(PlaybackProfile adapter, int partnerId)
        {
            bool result = false;

            if (adapter != null && !string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                PlaybackAdapter.ServiceClient client = new PlaybackAdapter.ServiceClient(string.Empty, adapter.AdapterUrl);

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
            string ip, ApiObjects.PlaybackAdapter.PlaybackContext kalturaPlaybackContext)
        {
            ApiObjects.PlaybackAdapter.PlaybackContext playbackContext = null;

            if (adapter == null)
            {
                throw new KalturaException(string.Format("playback adapter doesn't exist"), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("playback adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            PlaybackAdapter.ServiceClient adapterClient = new PlaybackAdapter.ServiceClient();
            adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
            string signature = string.Concat(adapter.Id, groupId, userId, udid, ip, unixTimestamp);

            try
            {

                AdapterControllers.PlaybackAdapter.AdapterPlaybackContext adapterPlaybackContext = ParsePlaybackContextToAdapterContext(kalturaPlaybackContext);

                PlaybackAdapter.AdapterPlaybackContextOptions contextOption = new PlaybackAdapter.AdapterPlaybackContextOptions()
                {
                    AdapterId = adapter.Id,
                    IP = ip,
                    PlaybackContext = adapterPlaybackContext,
                    PartnerId = groupId,
                    Udid = udid,
                    TimeStamp = unixTimestamp,
                    Signature = System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                };

                PlaybackAdapter.PlaybackAdapterResponse adapterResponse = GetAdapterPlaybackContext(adapterClient, contextOption);

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

        private static PlaybackAdapter.PlaybackAdapterResponse GetAdapterPlaybackContext(PlaybackAdapter.ServiceClient adapterClient, PlaybackAdapter.AdapterPlaybackContextOptions contextOptions)
        {
            PlaybackAdapter.PlaybackAdapterResponse adapterResponse;

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
            {
                //call adapter
                adapterResponse = adapterClient.GetPlaybackContext(contextOptions);
            }

            if (adapterResponse != null)
            {
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
                            Drm = x.Drm == null ? null : ParseDrm(x.Drm),
                            Format = x.Format,
                            IsTokenized = x.IsTokenized,
                            Protocols = x.Protocols
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

        private static List<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData> ParseDrm(PlaybackAdapter.DrmPlaybackPluginData[] drms)
        {
            List<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData> drmPlaybackPluginDatas = new List<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData>();

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

                drmPlaybackPluginDatas.Add(drm);
            }

            return drmPlaybackPluginDatas;
        }

        private PlaybackAdapter.DrmPlaybackPluginData[] ParseDrm(List<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData> drms)
        {
            List<PlaybackAdapter.DrmPlaybackPluginData> drmPlaybackPluginDatas = new List<PlaybackAdapter.DrmPlaybackPluginData>();

            PlaybackAdapter.DrmPlaybackPluginData drm;
            foreach (var item in drms)
            {
                if (item is ApiObjects.PlaybackAdapter.FairPlayPlaybackPluginData)
                {
                    ApiObjects.PlaybackAdapter.FairPlayPlaybackPluginData tmpDrm = item as ApiObjects.PlaybackAdapter.FairPlayPlaybackPluginData;
                    drm = new PlaybackAdapter.FairPlayPlaybackPluginData()
                    {
                        Certificate = tmpDrm.Certificate,
                    };
                }
                else if (item is ApiObjects.PlaybackAdapter.CustomDrmPlaybackPluginData)
                {
                    ApiObjects.PlaybackAdapter.CustomDrmPlaybackPluginData tmpDrm = item as ApiObjects.PlaybackAdapter.CustomDrmPlaybackPluginData;
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

                drmPlaybackPluginDatas.Add(drm);
            }

            return drmPlaybackPluginDatas.ToArray();
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

                         }).ToArray();
                }

                if (kalturaPlaybackContext.Messages != null)
                {
                    playbackContext.Messages = kalturaPlaybackContext.Messages.Select(x =>
                        new PlaybackAdapter.AccessControlMessage()
                        {
                            Code = x.Code,
                            Message = x.Message
                        }).ToArray();
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
                            Url = x.Url,
                            Drm = x.Drm == null ? null : ParseDrm(x.Drm),
                            Format = x.Format,
                            IsTokenized = x.IsTokenized,
                            Protocols = x.Protocols
                        }).ToArray();
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
                         }).ToArray();
                }
            }

            return playbackContext;
        }

        private PlaybackAdapter.PlaybackPluginData[] ParsePlugins(List<ApiObjects.PlaybackAdapter.PlaybackPluginData> plugins)
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

            return playbackPluginDatas.ToArray();
        }

        private static List<ApiObjects.PlaybackAdapter.PlaybackPluginData> ParsePlugins(PlaybackAdapter.PlaybackPluginData[] plugins)
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
    }
}