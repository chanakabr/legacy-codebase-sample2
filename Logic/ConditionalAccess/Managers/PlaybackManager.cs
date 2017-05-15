using AdapterControllers;
using AdapterControllers.CDVR;
using APILogic.ConditionalAccess;
using ApiObjects;
using ApiObjects.CDNAdapter;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using CachingProvider.LayeredCache;
using Core.Catalog.Response;
using Core.Pricing;
using Core.Users;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.ConditionalAccess
{
    public class PlaybackManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static PlaybackContextResponse GetPlaybackContext(BaseConditionalAccess cas, int groupId, string userId, string assetId, eAssetTypes assetType, List<long> fileIds, StreamerType? streamerType, string mediaProtocol,
            PlayContextType context, string ip, string udid, out MediaFileItemPricesContainer filePrice)
        {
            PlaybackContextResponse response = new PlaybackContextResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
            };

            filePrice = null;

            try
            {
                Domain domain = null;
                long domainId = 0;
                ApiObjects.Response.Status validationStatus = Utils.ValidateUserAndDomain(groupId, userId, ref domainId, out domain);

                if (assetType == eAssetTypes.NPVR || assetType == eAssetTypes.EPG)
                {
                    if (validationStatus.Code != (int)eResponseStatus.OK)
                    {
                        log.DebugFormat("User or domain not valid, groupId = {0}, userId: {1}, domainId = {2}", groupId, userId, domainId);
                        response.Status = new ApiObjects.Response.Status(validationStatus.Code, validationStatus.Message);
                        return response;
                    }
                }

                // EPG
                if (assetType == eAssetTypes.EPG)
                {
                    // services
                    PlayContextType? allowedContext = cas.FilterNotAllowedServices(domainId, context);
                    if (!allowedContext.HasValue)
                    {
                        log.DebugFormat("Service for domainId = {0}", domainId);
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ServiceNotAllowed, "Service not allowed");
                        return response;
                    }
                }

                if (assetType == eAssetTypes.NPVR && !cas.IsServiceAllowed((int)domainId, eService.NPVR))
                {
                    log.DebugFormat("Premium Service not allowed, DomainID: {0}, UserID: {1}, Service: {2}", domainId, userId, eService.NPVR.ToString());
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ServiceNotAllowed, "Service not allowed");
                    return response;
                }

                long mediaId;
                Recording recording = null;
                EPGChannelProgrammeObject program = null;
                response.Status = Utils.GetMediaIdForAsset(groupId, assetId, assetType, userId, domain, udid, out mediaId, out  recording, out program);
                if (response.Status.Code != (int)eResponseStatus.OK)
                {
                    return response;
                }

                MediaObj epgChannelLinearMedia = null;
                // Recording
                if (assetType == eAssetTypes.NPVR)
                {
                    long longAssetId = 0;
                    long.TryParse(assetId, out longAssetId);
                    response.Status = Utils.ValidateRecording(groupId, domain, udid, userId, longAssetId, ref recording);
                    if (response.Status.Code != (int)eResponseStatus.OK)
                    {
                        return response;
                    }

                    epgChannelLinearMedia = Utils.GetMediaById(groupId, (int)mediaId);

                    // get TSTV settings
                    var tstvSettings = Utils.GetTimeShiftedTvPartnerSettings(groupId);

                    // validate recording channel exists or the settings allow it to not exist
                    if (epgChannelLinearMedia == null && (!tstvSettings.IsRecordingPlaybackNonExistingChannelEnabled.HasValue || !tstvSettings.IsRecordingPlaybackNonExistingChannelEnabled.Value))
                    {
                        log.ErrorFormat("EPG channel does not exist and TSTV settings do not allow playback in this case. groupId = {0}, userId = {1}, domainId = {2}, domainRecordingId = {3}, channelId = {4}, recordingId = {5}",
                            groupId, userId, domainId, assetId, recording.ChannelId, recording.Id);
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecordingPlaybackNotAllowedForNonExistingEpgChannel, "Recording playback is not allowed for non existing EPG channel");
                        return response;
                    }
                }

                List<MediaFile> files = Utils.FilterMediaFilesForAsset(groupId, assetId, assetType, mediaId, streamerType, mediaProtocol, context, fileIds);
                Dictionary<long, AdsControlData> assetFileIdsAds = new Dictionary<long, AdsControlData>();

                if (files != null && files.Count > 0)
                {
                    MediaFileItemPricesContainer[] prices = null;

                    if (assetType == eAssetTypes.NPVR && (epgChannelLinearMedia == null || epgChannelLinearMedia.EnableRecordingPlaybackNonEntitledChannel))
                    {
                        assetFileIdsAds = files.ToDictionary(f => f.Id, f => new AdsControlData());
                    }
                    else
                    {
                        prices = cas.GetItemsPrices(files.Select(f => (int)f.Id).ToArray(), userId, true, string.Empty, string.Empty, string.Empty);
                        if (prices != null && prices.Length > 0)
                        {
                            AdsControlData adsData;
                            foreach (MediaFileItemPricesContainer price in prices)
                            {
                                adsData = null;

                                if (Utils.IsItemPurchased(price))
                                {
                                    adsData = GetFileAdsDataFromBusinessModule(groupId, price, udid);
                                    if (adsData != null)
                                    {
                                        assetFileIdsAds.Add(price.m_nMediaFileID, adsData);
                                    }
                                    else
                                    {
                                        assetFileIdsAds.Add(price.m_nMediaFileID, GetDomainAdsControl(groupId, domainId));
                                    }
                                }
                                if (Utils.IsFreeItem(price))
                                {
                                    assetFileIdsAds.Add(price.m_nMediaFileID, GetDomainAdsControl(groupId, domainId));
                                }

                                filePrice = price;
                            }
                        }
                    }

                    if (assetFileIdsAds.Count > 0)
                    {
                        int domainID = 0;
                        List<int> ruleIds = new List<int>();
                        DomainResponseStatus mediaConcurrencyResponse = cas.CheckMediaConcurrency(userId, (int)assetFileIdsAds.First().Key, udid, prices, int.Parse(assetId), ip, ref ruleIds, ref domainID);
                        if (mediaConcurrencyResponse != DomainResponseStatus.OK)
                        {
                            response.Status = Utils.ConcurrencyResponseToResponseStatus(mediaConcurrencyResponse);
                            return response;
                        }

                        response.Files = files.Where(f => assetFileIdsAds.Keys.Contains(f.Id)).ToList();
                        foreach (MediaFile file in response.Files)
                        {
                            var assetFileAds = assetFileIdsAds[file.Id];
                            if (assetFileAds != null)
                            {
                                file.AdsParam = assetFileAds.AdsParam;
                                file.AdsPolicy = assetFileAds.AdsPolicy;
                            }
                        }
                    }
                    else if (assetType == eAssetTypes.NPVR)
                    {
                        log.DebugFormat("User is not entitled for the EPG and TSTV settings do not allow playback. groupId = {0}, userId = {1}, domainId = {2}, domainRecordingId = {3}, epgId = {4}, recordingId = {5}",
                        groupId, userId, domainId, assetId, recording.EpgId, recording.Id);
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecordingPlaybackNotAllowedForNotEntitledEpgChannel, "Recording playback is not allowed for not entitled EPG channel");
                        return response;
                    }
                    else
                    {
                        log.DebugFormat("User is not entitled. groupId = {0}, userId = {1}, domainId = {2}, assetId = {3}, assetType = {4}",
                        groupId, userId, domainId, assetId, assetType);
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NotEntitled, "Not entitled");
                        return response;
                    }
                }
                else
                {
                    log.DebugFormat("No files found for asset assetId = {0}, assetType = {1}, streamerType = {2}, protocols = {3}", userId, assetId, assetType, streamerType, mediaProtocol);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoFilesFound, "No files found");
                    return response;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to GetPlaybackContext for userId = {0}, assetId = {1}, assetType = {2}", userId, assetId, assetType), ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

        private static AdsControlData GetFileAdsDataFromBusinessModule(int groupId, MediaFileItemPricesContainer price, string udid)
        {
            AdsControlData adsData = null;

            if (price.m_oItemPrices != null && price.m_oItemPrices.Length > 0)
            {
                Subscription subscription = null;
                ItemPriceContainer itemPrice = price.m_oItemPrices[0];
                if ((subscription = price.m_oItemPrices[0].m_relevantSub) != null) 
                {
                    if (subscription.m_lServices != null && subscription.m_lServices.Where(s => s.ID == (int)eService.AdsControl).FirstOrDefault() != null && subscription.AdsPolicy != null)
                    {
                        adsData = new AdsControlData()
                        {
                            AdsPolicy = subscription.AdsPolicy,
                            AdsParam = subscription.AdsParam,
                        };
                    }
                }
                else
                {
                    PPVModule ppv = Core.Pricing.Module.GetPPVModuleData(groupId, price.m_oItemPrices[0].m_sPPVModuleCode, string.Empty, string.Empty, udid);
                    if (ppv != null && ppv.AdsPolicy != null)
                    {
                        adsData = new AdsControlData()
                        {
                            AdsPolicy = ppv.AdsPolicy,
                            AdsParam = ppv.AdsParam,
                        };
                    }
                }
            }

            return adsData;
        }

        public static PlayManifestResponse GetPlayManifest(BaseConditionalAccess cas, int groupId, string userId, string assetId, eAssetTypes assetType, long fileId, string ip, string udid, PlayContextType playContextType)
        {
            PlayManifestResponse response = new PlayManifestResponse() { Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };
            try
            {
                long mediaId;
                Recording recording = null;
                EPGChannelProgrammeObject program = null;
                Domain domain = null;
                long domainId = 0;

                ApiObjects.Response.Status validationStatus = Utils.ValidateUserAndDomain(groupId, userId, ref domainId, out domain);

                if (assetType != eAssetTypes.MEDIA && validationStatus.Code != (int)eResponseStatus.OK)
                {
                    log.DebugFormat("User or domain not valid, groupId = {0}, userId: {1}, domainId = {2}", groupId, userId, domainId);
                    response.Status = new ApiObjects.Response.Status(validationStatus.Code, validationStatus.Message);
                    return response;
                }

                response.Status = Utils.GetMediaIdForAsset(groupId, assetId, assetType, userId, domain, udid, out mediaId, out  recording, out program);
                if (response.Status.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Failed to get media ID for assetId = {0}, assetType = {1}", assetId, assetType);
                    return response;
                }

                List<MediaFile> files = Utils.FilterMediaFilesForAsset(groupId, assetId, assetType, mediaId, null, null, playContextType, new List<long>() { fileId }, true);

                if (files == null || files.Count == 0)
                {
                    log.ErrorFormat("Failed to get files for assetId = {0}, assetType = {1}, mediaId = {2}", assetId, assetType, mediaId);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoFilesFound, "No files found");
                    return response;
                }

                MediaFile file = files[0];
                MediaFileItemPricesContainer price;
                PlaybackContextResponse playbackContextResponse = GetPlaybackContext(cas, groupId, userId, assetId, assetType, new List<long>() { fileId }, file.StreamerType.Value,
                    file.Url.Substring(0, file.Url.IndexOf(':')), playContextType, ip, udid, out price);
                if (playbackContextResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    response.Status = playbackContextResponse.Status;
                    return response;
                }

                // get adapter
                bool isDefaultAdapter = false;
                CDNAdapterResponse adapterResponse = Utils.GetRelevantCDN(groupId, file.CdnId, assetType, ref isDefaultAdapter);

                int assetIdInt = int.Parse(assetId);

                switch (assetType)
                {
                    case eAssetTypes.EPG:
                        response = GetEpgLicensedLink(cas, groupId, userId, program, file, udid, ip, adapterResponse, playContextType);
                        break;
                    case eAssetTypes.NPVR:
                        response = GetRecordingLicensedLink(cas, groupId, userId, recording, file, udid, ip, adapterResponse);
                        break;
                    case eAssetTypes.MEDIA:
                        response = GetMediaLicensedLink(cas, groupId, userId, file, udid, ip, adapterResponse);
                        break;
                    default:
                        break;
                }

                if (response.Status.Code == (int)eResponseStatus.OK)
                {
                    // HandlePlayUses
                    if (domainId > 0 && Utils.IsItemPurchased(price))
                    {
                        PlayUsesManager.HandlePlayUses(cas, price, userId, (int)file.Id, ip, string.Empty, string.Empty, udid, string.Empty, domainId, groupId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in get GetAssetLicensedLink, userId = {0}, fileId = {1}, assetId = {2}, assetType = {3}", userId, fileId, assetId, assetType), ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }

        private static PlayManifestResponse GetMediaLicensedLink(BaseConditionalAccess cas, int groupId, string userId, MediaFile file, string udid, string ip, CDNAdapterResponse adapterResponse)
        {
            PlayManifestResponse response = new PlayManifestResponse() { Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            try
            {
                if (adapterResponse.Adapter != null && !string.IsNullOrEmpty(adapterResponse.Adapter.AdapterUrl))
                {
                    var link = CDNAdapterController.GetInstance().GetVodLink(groupId, adapterResponse.Adapter.ID, userId, file.Url, file.Type, (int)file.MediaId, (int)file.Id, ip);
                    response.Url = link != null ? link.Url : string.Empty;
                }
                else
                {
                    Dictionary<string, string> licensedLinkParams = Utils.GetLicensedLinkParamsDict(userId, file.Id.ToString(), file.Url, ip, string.Empty, string.Empty, udid, string.Empty);
                    response.Url = cas.GetLicensedLink(file.CdnId, licensedLinkParams);
                }

                if (!string.IsNullOrEmpty(response.Url))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to GetMediaLicensedLink for fileId = {0}, userId = {1}", file.Id, userId), ex);
            }
            return response;
        }

        private static PlayManifestResponse GetRecordingLicensedLink(BaseConditionalAccess cas, int groupId, string userId, Recording recording, MediaFile file, string udid, string ip, CDNAdapterResponse adapterResponse)
        {
            PlayManifestResponse response = new PlayManifestResponse() { Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            try
            {
                int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);
                CDVRAdapter cdvrAdapter = ConditionalAccessDAL.GetCDVRAdapter(groupId, adapterId);
                RecordingLink recordingLink = null;
                if (cdvrAdapter != null && cdvrAdapter.DynamicLinksSupport)
                {
                    // get the link from the CDVR adapter
                    string externalChannelId = Tvinci.Core.DAL.CatalogDAL.GetEPGChannelCDVRId(groupId, recording.ChannelId);

                    RecordResult recordResult = CdvrAdapterController.GetInstance().GetRecordingLinks(groupId, externalChannelId, recording.ExternalRecordingId, cdvrAdapter.ID);

                    if (recordResult == null || recordResult.Links == null || recordResult.Links.Count == 0)
                    {
                        log.ErrorFormat("Failed to get recording links dynamically from CDVR adapter. adapterId = {0}, groupId = {1}, userId = {2}, domainRecordingId = {3}, externalRecordingId = {4}, recordingId = {5}",
                            cdvrAdapter.ID, groupId, userId, recording.Id, recording.ExternalRecordingId, recording.Id);
                        return response;
                    }

                    recordingLink = recordResult.Links.Where(rl => rl.FileType == file.Type).FirstOrDefault();
                }
                else
                {
                    // get the link for the recording with the given udid brand
                    recordingLink = RecordingsDAL.GetRecordingLinkByFileType(groupId, recording.Id, file.Type);
                }

                if (recordingLink == null || string.IsNullOrEmpty(recordingLink.Url))
                {
                    log.ErrorFormat("Recording link was not found for fileType = {0}. groupId = {1}, userId = {2}, domainRecordingId = {3}, udid = {4}, recordingId = {5}",
                        file.Type, groupId, userId, recording.Id, udid, recording.Id);
                    return response;
                }

                if (adapterResponse == null || adapterResponse.Adapter == null || adapterResponse.Status.Code != (int)eResponseStatus.OK || string.IsNullOrEmpty(adapterResponse.Adapter.AdapterUrl))
                {
                    log.ErrorFormat("failed to get CDN adapter for recordings for groupId = {0}. userId = {1}, domainRecordingId = {2}, recordingId = {3}",
                        groupId, userId, recording.Id, recording.Id);
                    return response;
                }

                // main url
                var link = CDNAdapterController.GetInstance().GetRecordingLink(groupId, adapterResponse.Adapter.ID, userId, recordingLink.Url, file.Type, recording.ExternalRecordingId, ip);

                if (link != null && !string.IsNullOrEmpty(link.Url))
                {
                    response.Url = link.Url;
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to GetRecordingLicensedLink for fileId = {0}, userId = {1}", file.Id, userId), ex);
            }

            return response;
        }

        private static PlayManifestResponse GetEpgLicensedLink(BaseConditionalAccess cas, int groupId, string userId, EPGChannelProgrammeObject program, MediaFile file, string udid, string ip, CDNAdapterResponse adapterResponse, PlayContextType context)
        {
            PlayManifestResponse response = new PlayManifestResponse() { Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            try
            {
                DateTime programEndTime = DateTime.ParseExact(program.END_DATE, "dd/MM/yyyy HH:mm:ss", null);
                DateTime programStartTime = DateTime.ParseExact(program.START_DATE, "dd/MM/yyyy HH:mm:ss", null);

                eEPGFormatType formatType = Utils.GetEpgFormatTypeByPlayContextType(context);

                // if adapter response is not null and is adapter (has an adapter url) - call the adapter
                if (adapterResponse.Adapter != null && !string.IsNullOrEmpty(adapterResponse.Adapter.AdapterUrl))
                {
                    int actionType = Utils.MapActionTypeForAdapter(formatType);

                    // main url
                    var link = CDNAdapterController.GetInstance().GetEpgLink(groupId, adapterResponse.Adapter.ID, userId, file.Url, file.Type, (int)program.EPG_ID, (int)file.MediaId, (int)file.Id,
                        TVinciShared.DateUtils.DateTimeToUnixTimestamp(programStartTime), actionType, ip);
                    response.Url = link != null ? link.Url : null;
                }
                else
                {
                    Dictionary<string, object> dURLParams = new Dictionary<string, object>();
                    dURLParams.Add(ApiObjects.Epg.EpgLinkConstants.PROGRAM_END, programEndTime);
                    dURLParams.Add(ApiObjects.Epg.EpgLinkConstants.EPG_FORMAT_TYPE, formatType);
                    if (formatType == eEPGFormatType.Catchup || formatType == eEPGFormatType.StartOver)
                    {
                        dURLParams.Add(ApiObjects.Epg.EpgLinkConstants.PROGRAM_START, programStartTime);
                    }
                    string CdnStrID = string.Empty;
                    bool bIsDynamic = Utils.GetStreamingUrlType(file.CdnId, ref CdnStrID);
                    dURLParams.Add(ApiObjects.Epg.EpgLinkConstants.IS_DYNAMIC, bIsDynamic);
                    dURLParams.Add(ApiObjects.Epg.EpgLinkConstants.BASIC_LINK, file.Url);

                    StreamingProvider.ILSProvider provider = StreamingProvider.LSProviderFactory.GetLSProvidernstance(CdnStrID);
                    if (provider != null)
                    {
                        string liveUrl = provider.GenerateEPGLink(dURLParams);
                        response.Url = !string.IsNullOrEmpty(liveUrl) ? liveUrl : null;
                    }
                }

                if (!string.IsNullOrEmpty(response.Url))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to GetEpgLicensedLink for fileId = {0}, userId = {1}", file.Id, userId), ex);
            }

            return response;
        }

        internal static AdsControlData GetDomainAdsControl(int groupId, long domainId)
        {
            AdsControlData adsData = null;

            DomainEntitlements domainEntitlements = null;
            if (Utils.TryGetDomainEntitlementsFromCache(groupId, (int)domainId, null, ref domainEntitlements))
            {
                // get domain subscriptions ordered by priority
                List<string> subscriptionIds = domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions.Select(x => x.Value.sBundleCode).ToList();
                Subscription[] subs = Utils.GetSubscriptionsDataWithCaching(subscriptionIds, groupId);
                if (subs != null && subs.Length > 0)
                {
                    subs = subs.OrderBy(s => s.m_Priority).ToArray();
                    foreach (var sub in subs)
                    {
                        // find one with policy
                        if (sub.m_lServices != null && sub.m_lServices.Where(s => s.ID == (int)eService.AdsControl).FirstOrDefault() != null &&
                            sub.AdsPolicy != null)
                        {
                            adsData = new AdsControlData()
                            {
                                AdsParam = sub.AdsParam,
                                AdsPolicy = sub.AdsPolicy
                            };

                            break;
                        }
                    }
                }
            }

            // if no subscription found get group default
            if (adsData == null)
            {
                string key = LayeredCacheKeys.GetGroupAdsControlKey(groupId);

                if (!LayeredCache.Instance.Get<AdsControlData>(key, ref adsData, Pricing.Utils.GetGetGroupAdsControl, new Dictionary<string, object>() { 
                                                       { "groupId", groupId } }, groupId, LayeredCacheConfigNames.GET_GROUP_ADS_CONTROL_CACHE_CONFIG_NAME, new List<string>() {
                                                       LayeredCacheKeys.GetPricingSettingsInvalidationKey(groupId) }))
                {
                    log.Error("Failed to get group ads control data");
                }

            }

            //if no such values return empty
            return adsData;
        }
    }
}
