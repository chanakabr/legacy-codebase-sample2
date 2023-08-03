using AdapterControllers;
using AdapterControllers.CDVR;
using APILogic.Api.Managers;
using APILogic.ConditionalAccess;
using ApiObjects;
using ApiObjects.CDNAdapter;
using ApiObjects.ConditionalAccess;
using ApiObjects.Response;
using ApiObjects.Rules;
using ApiObjects.Segmentation;
using ApiObjects.TimeShiftedTv;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using Core.Api;
using Core.Api.Managers;
using Core.Catalog.Response;
using Core.Pricing;
using Core.Users;
using Core.Users.Cache;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ApiLogic;
using MoreLinq;
using TVinciShared;
using ApiObjects.MediaMarks;
using ApiLogic.Api.Managers;
using KalturaRequestContext;

namespace Core.ConditionalAccess
{
    public class PlaybackManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public const string PROGRAM_ID_KEY = "PROGRAM_ID";

        public static PlaybackContextResponse GetPlaybackContext(BaseConditionalAccess cas, int groupId, string userId, string assetId, eAssetTypes assetType,
            List<long> fileIds, StreamerType? streamerType, string mediaProtocol, PlayContextType context,
            string ip, string udid, out PlaybackContextOut playbackContextOut, UrlType urlType, string sourceType = null, Dictionary<string, string> adapterData = null)
        {
            PlaybackContextResponse response = new PlaybackContextResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
            };

            playbackContextOut = new PlaybackContextOut();
            BlockEntitlementType blockEntitlement = BlockEntitlementType.NO_BLOCK; // default value

            try
            {
                long domainId = 0;
                var validationStatus = Utils.ValidateUserAndDomain(groupId, userId, ref domainId);

                if (assetType == eAssetTypes.MEDIA && validationStatus.Code == (int)eResponseStatus.UserSuspended)
                {
                    // check permissions
                    bool permittedPpv = RolesPermissionsManager.Instance.IsPermittedPermission(groupId, userId, RolePermissions.PLAYBACK_PPV);
                    bool permittedSubscription = RolesPermissionsManager.Instance.IsPermittedPermission(groupId, userId, RolePermissions.PLAYBACK_SUBSCRIPTION);

                    if (!permittedPpv && !permittedSubscription)
                    {
                        blockEntitlement = BlockEntitlementType.BLOCK_ALL;
                    }
                    else if (!permittedPpv)
                    {
                        blockEntitlement = BlockEntitlementType.BLOCK_PPV;
                    }
                    else if (!permittedSubscription)
                    {
                        blockEntitlement = BlockEntitlementType.BLOCK_SUBSCRIPTION;
                    }
                }

                if (assetType == eAssetTypes.NPVR || assetType == eAssetTypes.EPG)
                {
                    if (validationStatus.Code == (int)eResponseStatus.UserSuspended || validationStatus.Code == (int)eResponseStatus.OK)
                    {
                        // check permissions
                        bool permittedEpg = RolesPermissionsManager.Instance.IsPermittedPermission(groupId, userId, RolePermissions.PLAYBACK_EPG);
                        bool permittedRecording = RolesPermissionsManager.Instance.IsPermittedPermission(groupId, userId, RolePermissions.PLAYBACK_RECORDING);
                        if ((assetType == eAssetTypes.NPVR && !permittedRecording) || (assetType == eAssetTypes.EPG && !permittedEpg))
                        {
                            if (validationStatus.Code == (int)eResponseStatus.UserSuspended)
                            {
                                return HandleUserSuspended(groupId, userId, assetId, assetType, response, domainId);
                            }
                            else
                            {
                                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NotAllowed, "Action not allowed");
                                return response;
                            }
                        }
                    }
                    else
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
                bool isExternalRecordingIgnoreMode = assetType == eAssetTypes.NPVR && TvinciCache.GroupsFeatures.GetGroupFeatureStatus(groupId, GroupFeature.EXTERNAL_RECORDINGS);
                if (assetType != eAssetTypes.MEDIA)
                {
                    Utils.ValidateDomain(groupId, (int)domainId, out Domain domain);
                    response.Status = Utils.GetMediaIdForAsset(groupId, assetId, assetType, userId, domain, udid, out mediaId, out recording, out program);
                }
                else
                {
                    mediaId = long.Parse(assetId);
                }

                // Allow to continue for external recording (and asset type = NPVR) since we may not be updated on them in real time
                if (response.Status.Code != (int)eResponseStatus.OK)
                {
                    if (isExternalRecordingIgnoreMode && fileIds != null && fileIds.Count > 0)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                        mediaId = Utils.GetMediaIdByFileId(groupId, (int)fileIds[0]);

                        long programId = 0;
                        // check if programId exist at adapterData
                        if (adapterData?.Count > 0 && adapterData.ContainsKey(PROGRAM_ID_KEY) && long.TryParse(adapterData[PROGRAM_ID_KEY], out programId))
                        {
                            if (programId > 0)
                            {
                                recording = new Recording() { EpgId = programId };
                            }
                        }
                    }
                    else
                    {
                        return response;
                    }
                }

                if (assetType == eAssetTypes.EPG && program != null)
                {
                    if (context == PlayContextType.StartOver)
                    {
                        response.Status = Utils.ValidateEpgForStartOver(program);

                        if (response.Status.Code != (int)eResponseStatus.OK)
                        {
                            return response;
                        }
                    }

                    var tstvSettings = Utils.GetTimeShiftedTvPartnerSettings(groupId);
                    response.Status = Utils.ValidateEpgForCatchUp(tstvSettings, program);

                    if (response.Status.Code != (int)eResponseStatus.OK)
                    {
                        return response;
                    }
                }

                MediaObj epgChannelLinearMedia = null;
                List<SlimAsset> assetsToCheck = null;

                // Recording
                if (assetType == eAssetTypes.NPVR)
                {
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

                    if (recording != null)
                    {
                        if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Items != null)
                        {
                            System.Web.HttpContext.Current.Items[RequestContextConstants.RECORDING_CONVERT_KEY] = recording.EpgId;
                        }
                        else
                        {
                            log.ErrorFormat("Error when trying to save epgId in httpContext key {0} for GetPlaybackContext on recording assetId {1}",
                                                RequestContextConstants.RECORDING_CONVERT_KEY, assetId);
                        }

                        assetsToCheck = new List<SlimAsset>() { new SlimAsset(recording.EpgId, recording.Id > 0 ? eAssetTypes.NPVR : eAssetTypes.EPG) };
                        if (mediaId > 0)
                        {
                            assetsToCheck.Add(new SlimAsset(mediaId, eAssetTypes.MEDIA));
                        }
                    }
                }
                else if (assetType == eAssetTypes.EPG && mediaId > 0)
                {
                    assetsToCheck = new List<SlimAsset>()
                    {
                        new SlimAsset(long.Parse(assetId), eAssetTypes.EPG),
                        new SlimAsset(mediaId, eAssetTypes.MEDIA)
                    };
                }
                else
                {
                    assetsToCheck = AssetRuleManager.GetAssetsForValidation(assetType, groupId, long.Parse(assetId));
                }

                AssetRule blockingRule;
                var networkRulesStatus = AssetRuleManager.CheckNetworkRules(assetsToCheck, groupId, ip, out blockingRule);
                if (!networkRulesStatus.IsOkStatusCode())
                {
                    response.Status = networkRulesStatus;
                    return response;
                }

                List<MediaFile> files = Utils.FilterMediaFilesForAsset(groupId, assetType, mediaId, streamerType, mediaProtocol, context, fileIds, false, sourceType);
                Dictionary<long, AdsControlData> assetFileIdsAds = new Dictionary<long, AdsControlData>();
                var isSuspended = false;
                if (files != null && files.Count > 0)
                {
                    MediaFileItemPricesContainer[] prices = null;
                    AdsControlData defaultAdsData = GetDomainAdsControl(groupId, domainId);

                    if (assetType == eAssetTypes.NPVR && (epgChannelLinearMedia == null || epgChannelLinearMedia.EnableRecordingPlaybackNonEntitledChannel))
                    {
                        assetFileIdsAds = files.ToDictionary(f => f.Id, f => new AdsControlData());
                    }
                    else
                    {
                        prices = cas.GetItemsPrices(files.Select(f => (int)f.Id).ToArray(), userId, string.Empty, true,
                            string.Empty, string.Empty, ip, null, blockEntitlement, false, true);

                        if (prices != null && prices.Length > 0)
                        {
                            foreach (MediaFileItemPricesContainer price in prices)
                            {
                                AdsControlData adsData = null;

                                // check permitted role
                                PriceReason priceReason = PriceReason.PPVPurchased;

                                if (!isSuspended && price.m_oItemPrices?.First()?.m_PriceReason == PriceReason.UserSuspended)
                                {
                                    isSuspended = true;
                                }

                                if (Utils.IsItemPurchased(price, ref priceReason))
                                {
                                    if (priceReason == PriceReason.PPVPurchased || priceReason == PriceReason.SubscriptionPurchased)
                                    {
                                        RolePermissions rolePermission = priceReason == PriceReason.PPVPurchased ? RolePermissions.PLAYBACK_PPV : RolePermissions.PLAYBACK_SUBSCRIPTION;
                                        if (!RolesPermissionsManager.Instance.IsPermittedPermission(groupId, userId, rolePermission))
                                        {
                                            continue;
                                        }

                                        if (priceReason == PriceReason.SubscriptionPurchased)
                                        {
                                            var subscriptionId = price.m_oItemPrices?.First()?.m_relevantSub?.m_sObjectCode;

                                            if (!string.IsNullOrEmpty(subscriptionId))
                                            {
                                                var status = api.HandleBlockingSegment<SegmentBlockPlaybackSubscriptionAction>(groupId, userId, udid, ip, (int)domainId, ObjectVirtualAssetInfoType.Subscription, subscriptionId);
                                                if (!status.IsOkStatusCode())
                                                {
                                                    response.Status = status;
                                                    return response;
                                                }
                                            }
                                        }
                                    }

                                    adsData = GetFileAdsDataFromBusinessModule(groupId, price, udid);
                                    if (adsData != null)
                                    {
                                        assetFileIdsAds.Add(price.m_nMediaFileID, adsData);
                                    }
                                    else
                                    {
                                        assetFileIdsAds.Add(price.m_nMediaFileID, defaultAdsData);
                                    }
                                }
                                if (Utils.IsFreeItem(price))
                                {
                                    assetFileIdsAds.Add(price.m_nMediaFileID, defaultAdsData);
                                }

                                playbackContextOut.MediaFileItemPrices = price;
                            }
                        }
                    }

                    var notPurchasedFiles = files.Where(x =>
                        !assetFileIdsAds.ContainsKey(x.Id)).ToList();
                    var hasFilesNotFromPago = assetFileIdsAds.Count > 0;

                    //Make sure we're not checking pago in case of purchased by different entitlement/free asset
                    if (!hasFilesNotFromPago)
                    {
                        playbackContextOut.PagoProgramAvailability = Utils.GetEntitledPagoWindow(groupId,
                            (int)domainId,
                            int.Parse(assetId), assetType, notPurchasedFiles, program);
                        playbackContextOut.PagoProgramAvailability?.FileIds?.ForEach(x =>
                            assetFileIdsAds.Add(x, defaultAdsData));
                    }

                    if (assetFileIdsAds.Count > 0)
                    {
                        ConcurrencyResponse concurrencyResponse = null;

                        // in case of direct Url no need for Concurrency check
                        if (context != PlayContextType.Download)
                        {
                            if (assetType == eAssetTypes.EPG)
                            {
                                concurrencyResponse = cas.CheckMediaConcurrency(userId, udid, prices, (int)mediaId, (int)domainId, long.Parse(assetId), ePlayType.EPG);
                            }
                            else if (assetType == eAssetTypes.NPVR)
                            {
                                concurrencyResponse = cas.CheckMediaConcurrency(userId, udid, prices, (int)mediaId, (int)domainId, recording != null ? recording.EpgId : -1, ePlayType.NPVR,
                                    assetId);
                            }
                            else
                            {
                                concurrencyResponse = cas.CheckMediaConcurrency(userId, udid, prices, int.Parse(assetId), (int)domainId, 0, ePlayType.MEDIA);
                            }

                            if (concurrencyResponse.Status != DomainResponseStatus.OK)
                            {
                                response.Status = Utils.ConcurrencyResponseToResponseStatus(concurrencyResponse.Status);
                                return response;
                            }

                            domainId = concurrencyResponse.Data.DomainId;
                            //if there's no file from other business module and still
                            //we got here, it means we got entitlement from pago
                            if (!hasFilesNotFromPago && playbackContextOut.PagoProgramAvailability?.FileIds?.Count > 0)
                            {
                                concurrencyResponse.Data.ProductId = (int)playbackContextOut.PagoProgramAvailability.PagoId;
                                concurrencyResponse.Data.ProductType = eTransactionType.ProgramAssetGroupOffer;
                            }
                        }
                        else
                        {
                            concurrencyResponse = new ConcurrencyResponse()
                            {
                                Data = new ApiObjects.MediaMarks.DevicePlayData()
                                {
                                    UDID = udid,
                                    DomainId = (int)domainId
                                }
                            };
                        }

                        response.ConcurrencyData = concurrencyResponse.Data;

                        if (response.ConcurrencyData != null)
                        {
                            log.DebugFormat("GetPlaybackContext - {0}", response.ConcurrencyData.ToString());
                        }

                        response.Files = files.Where(f => assetFileIdsAds.Keys.Contains(f.Id)).ToList();

                        foreach (MediaFile file in response.Files)
                        {
                            if (response.ConcurrencyData != null)
                            {
                                file.BusinessModuleDetails = new BusinessModuleDetails
                                {
                                    BusinessModuleId = response.ConcurrencyData.ProductId,
                                    BusinessModuleType = response.ConcurrencyData.ProductType
                                };
                            }

                            var assetFileAds = assetFileIdsAds[file.Id];
                            if (assetFileAds != null)
                            {
                                file.AdsParam = assetFileAds.AdsParam;
                                file.AdsPolicy = assetFileAds.AdsPolicy;
                            }

                            if (urlType == UrlType.direct)
                            {
                                // get adapter
                                bool isDefaultAdapter = false;
                                (PlayManifestResponse urlAdapterResponse, PlayManifestResponse altUrlAdapterResponse) urlAdapterResponse = (null, null);
                                if (!isExternalRecordingIgnoreMode)
                                {
                                    urlAdapterResponse = RetrievePlayManifestAdapterResponses(cas, groupId, userId, assetType, context, ip, udid, file, isDefaultAdapter, program, recording);
                                }

                                if (response.Status.Code == (int)eResponseStatus.OK)
                                {
                                    if (urlAdapterResponse.urlAdapterResponse != null)
                                    {
                                        file.DirectUrl = urlAdapterResponse.urlAdapterResponse.Url;
                                    }

                                    if (urlAdapterResponse.altUrlAdapterResponse != null)
                                    {
                                        file.AltDirectUrl = urlAdapterResponse.altUrlAdapterResponse.Url;
                                    }

                                    if (context != PlayContextType.Download)
                                    {
                                        // HandlePlayUses + DevicePlayData
                                        if (domainId > 0)
                                        {
                                            bool isLive = false;
                                            if (assetType == eAssetTypes.MEDIA && Utils.IsOpc(groupId))
                                            {
                                                string epgChannelId = APILogic.Api.Managers.EpgManager.GetEpgChannelId((int)mediaId, groupId);
                                                isLive = !string.IsNullOrEmpty(epgChannelId);
                                            }

                                            HandlePlayUsesAndDevicePlayData(cas, userId, domainId, (int)file.Id, ip, udid, playbackContextOut, concurrencyResponse != null ? concurrencyResponse.Data : null, isLive);
                                        }
                                    }
                                }
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

                    else if (isSuspended)
                    {
                        return HandleUserSuspended(groupId, userId, assetId, assetType, response, domainId);
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
                    log.DebugFormat("No files found for asset assetId = {0}, assetType = {1}, streamerType = {2}, protocols = {3}", assetId, assetType, streamerType, mediaProtocol);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoFilesFound, "No files found");
                    return response;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed to GetPlaybackContext for userId = {userId}, assetId = {assetId}, assetType = {assetType}, exception: {ex.ToString()}.", ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

        public static BookPlaybackSessionResponse BookPlaybackSession(BaseConditionalAccess cas, int groupId,
            string userId, string udid, string ip,
            string assetIdString, string mediaFileIdString, eAssetTypes assetType)
        {
            BookPlaybackSessionResponse response = new BookPlaybackSessionResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
            };

            try
            {
                long domainId = 0;
                var validationStatus = Utils.ValidateUserAndDomain(groupId, userId, ref domainId);

                if (validationStatus == null)
                {
                    response.Status = new ApiObjects.Response.Status(
                        (int)eResponseStatus.Error, "Error validating user and domain");
                    return response;
                }
                else if (validationStatus.Code != (int)eResponseStatus.OK)
                {
                    response.Status = new ApiObjects.Response.Status(validationStatus.Code, validationStatus.Message);
                    return response;
                }

                if (!int.TryParse(mediaFileIdString, out var mediaFileId))
                {
                    response.Status = new ApiObjects.Response.Status(
                        (int)eResponseStatus.InvalidParameters, "Media file Id cannot be parsed");
                    return response;
                }

                if (!long.TryParse(assetIdString, out var assetId))
                {
                    response.Status = new ApiObjects.Response.Status(
                        (int)eResponseStatus.InvalidParameters, "Asset Id cannot be parsed");
                    return response;
                }

                bool isFree = IsMediaFileFree(groupId, mediaFileId);

                ePlayType playType = ePlayType.UNKNOWN;
                long programId = 0;
                string npvrId = string.Empty;
                long mediaId = 0;

                if (assetType != eAssetTypes.MEDIA)
                {
                    Utils.ValidateDomain(groupId, (int)domainId, out Domain domain);
                    response.Status = Utils.GetMediaIdForAsset(groupId, assetIdString, assetType,
                        userId, domain, udid, out mediaId, out var recording, out _);

                    if (assetType == eAssetTypes.NPVR)
                    {
                        playType = ePlayType.NPVR;
                        programId = recording != null ? recording.EpgId : -1;
                        npvrId = assetIdString;
                    }
                    else
                    {
                        programId = assetId;
                        playType = ePlayType.EPG;
                    }
                }
                else
                {
                    mediaId = assetId;
                    playType = ePlayType.MEDIA;
                }

                if (!Utils.ValidateMediaFileForAsset(groupId, mediaId, assetType, mediaFileId))
                {
                    log.DebugFormat($"Invalid file/asset combination: assetId = {assetId}, fileId = {mediaFileIdString}, assetType = {assetType}");
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoFilesFound, "No files found");
                    return response;
                }

                // check if concurrency is validated + prepare device play data document
                var concurrencyResponse = CheckConcurrencyForBooking(
                    groupId, udid, userId, (int)mediaId, (int)domainId,
                    programId, playType, isFree, npvrId);

                if (concurrencyResponse.Status != DomainResponseStatus.OK)
                {
                    response.Status = Utils.ConcurrencyResponseToResponseStatus(concurrencyResponse.Status);
                    return response;
                }

                if (concurrencyResponse.Data != null)
                {
                    cas.InsertDevicePlayData(concurrencyResponse.Data, ApiObjects.Catalog.eExpirationTTL.Long, true);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed to BookPlaybackSession for userId = {userId}, " +
                    $"mediaFileId = {mediaFileIdString}, assetType = {assetType}, exception: {ex}.", ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

        /// <summary>
        ///  verifies if a specific media file has any module assigned to it or not
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="mediaFileId"></param>
        /// <returns></returns>
        public static bool IsMediaFileFree(int groupId, long mediaFileId)
        {
            bool isFree = false;
            MediaFilePPVContainer[] modules = Pricing.Module.GetPPVModuleListForMediaFilesWithExpiry(
                groupId, new int[] { (int)mediaFileId });

            // if we successfully got the modules CONTAINERS for this file - but there is no module assigned to this file, it's free
            if (modules != null && modules.Length > 0 &&
                (modules[0].m_oPPVModules == null || modules[0].m_oPPVModules.Length == 0))
            {
                isFree = true;
            }

            return isFree;
        }

        public static ConcurrencyResponse CheckConcurrencyForBooking(int groupId,
            string udid, string userIdString, int assetId, int domainId, long programId,
            ePlayType playType, bool isFree, string npvrId = "")
        {
            ConcurrencyResponse concurrencyResponse = new ConcurrencyResponse();

            concurrencyResponse.Data = new DevicePlayData()
            {
                UDID = udid,
                AssetId = assetId,
                DomainId = domainId,
                ProgramId = programId,
                playType = playType.ToString(),
                NpvrId = npvrId,
                IsFree = isFree,
                // the Playback Service shall set the Timestamp and CreatedAt fields
                // of the DevicePlay document to the current system time
                TimeStamp = DateUtils.GetUtcUnixTimestampNow(),
                CreatedAt = DateUtils.GetUtcUnixTimestampNow(),
            };

            if (Utils.IsAnonymousUser(userIdString))
            {
                return concurrencyResponse;
            }

            int.TryParse(userIdString, out var userId);
            concurrencyResponse.Data.UserId = userId;

            #region Validate domain
            DomainResponse domainResponse = null;

            if (domainId == 0)
            {
                domainResponse = Core.Domains.Module.GetDomainByUser(groupId, userIdString);
            }
            else
            {
                domainResponse = Core.Domains.Module.GetDomainInfo(groupId, domainId);
            }

            if (domainResponse != null && domainResponse.Domain != null)
            {
                domainId = domainResponse.Domain.m_nDomainID;
            }

            concurrencyResponse.Data.DomainId = domainId;
            #endregion

            // Get AssetRules
            concurrencyResponse.Data.AssetMediaConcurrencyRuleIds = Utils.GetAssetMediaRuleIds(groupId, assetId);
            concurrencyResponse.Data.AssetEpgConcurrencyRuleIds = Utils.GetAssetEpgRuleIds(groupId, assetId, ref programId);
            concurrencyResponse.Data.ProgramId = programId;

            bool shouldExcludeFreeContent = Api.api.GetShouldExcludeFreeContentFromConcurrency(groupId);

            // if the media file is free and we're excluding free content from concurrency limitations - don't check it
            if (!isFree || !shouldExcludeFreeContent)
            {
                // validate Concurrency for domain
                var validationResponse = Domains.Module.ValidateLimitationModule(
                    groupId, 0, ValidationType.Concurrency, concurrencyResponse.Data);

                if (validationResponse != null)
                {
                    log.DebugFormat("ValidateLimitationModule result: {0}", validationResponse.m_eStatus);
                    concurrencyResponse.Status = validationResponse.m_eStatus;
                }
            }

            return concurrencyResponse;
        }

        private static (PlayManifestResponse urlAdapterResponse, PlayManifestResponse altUrlAdapterResponse) RetrievePlayManifestAdapterResponses(
            BaseConditionalAccess cas,
            int groupId,
            string userId,
            eAssetTypes assetType,
            PlayContextType context,
            string ip,
            string udid,
            MediaFile file,
            bool isDefaultAdapter,
            EPGChannelProgrammeObject program,
            Recording recording)
        {
            var urlAdapterResponse = RetrievePlayManifestAdapterResponse(cas, groupId, userId, assetType, context, ip, udid, file, isDefaultAdapter, program, recording);
            var altUrlAdapterResponse = string.IsNullOrEmpty(file.AltUrl)
                ? null
                : RetrievePlayManifestAdapterResponse(cas, groupId, userId, assetType, context, ip, udid, file, isDefaultAdapter, program, recording, true);

            return (urlAdapterResponse, altUrlAdapterResponse);
        }

        private static PlayManifestResponse RetrievePlayManifestAdapterResponse(
            BaseConditionalAccess cas,
            int groupId,
            string userId,
            eAssetTypes assetType,
            PlayContextType context,
            string ip,
            string udid,
            MediaFile file,
            bool isDefaultAdapter,
            EPGChannelProgrammeObject program,
            Recording recording,
            bool isAltUrl = false)
        {
            var adapterResponse = Utils.GetRelevantCDN(groupId, !isAltUrl ? file.CdnId : file.AltCdnId, assetType, ref isDefaultAdapter);
            PlayManifestResponse playManifestResponse;
            switch (assetType)
            {
                case eAssetTypes.EPG:
                    playManifestResponse = GetEpgLicensedLink(cas, groupId, userId, program, file, udid, ip, adapterResponse, context, isAltUrl);
                    break;
                case eAssetTypes.NPVR:
                    playManifestResponse = GetRecordingLicensedLink(cas, groupId, userId, recording, file, udid, ip, adapterResponse);
                    break;
                case eAssetTypes.MEDIA:
                    playManifestResponse = GetMediaLicensedLink(cas, groupId, userId, file, udid, ip, adapterResponse, isAltUrl);
                    break;
                default:
                    playManifestResponse = null;
                    break;
            }

            return playManifestResponse;
        }

        public static void HandlePlayUsesAndDevicePlayData(BaseConditionalAccess cas, string userId, long domainId, int fileId, string ip, string udid,
            PlaybackContextOut playbackContextOutContainer, ApiObjects.MediaMarks.DevicePlayData devicePlayData, bool isLive)
        {
            var filePrice = playbackContextOutContainer.MediaFileItemPrices;
            if (Utils.IsItemPurchased(filePrice))
            {
                PlayUsesManager.HandlePlayUses(cas, filePrice, userId, fileId, ip, string.Empty, string.Empty, udid, string.Empty, domainId, cas.m_nGroupID, isLive);
            }
            else if (ApplicationConfiguration.Current.LicensedLinksCacheConfiguration.ShouldUseCache.Value)
            {
                var pagoAvailableWindow = playbackContextOutContainer.PagoProgramAvailability;
                if (pagoAvailableWindow != null && pagoAvailableWindow.IsValid())
                {
                    var now = DateTime.UtcNow;
                    var viewTime = (int)(pagoAvailableWindow.EndDate - pagoAvailableWindow.StartDate).TotalMinutes;
                    Utils.InsertOrSetCachedEntitlementResults(domainId, fileId,
                        new CachedEntitlementResults(viewTime, viewTime, now, false, false,
                            eTransactionType.ProgramAssetGroupOffer, now,
                            null, isLive));
                }
                // item must be free otherwise we wouldn't get this far
                else if(filePrice?.m_oItemPrices?.Length > 0)
                {
                    Utils.InsertOrSetCachedEntitlementResults(domainId, fileId,
                        new CachedEntitlementResults(0, 0, DateTime.UtcNow, true, false,
                            eTransactionType.PPV, null, filePrice.m_oItemPrices[0].m_dtEndDate, isLive));
                }
            }

            if (devicePlayData != null)
            {
                cas.InsertDevicePlayData(devicePlayData, ApiObjects.Catalog.eExpirationTTL.Long, false);
            }

            log.Debug("PlaybackManager.GetPlaybackContext - exec PlayUsesManager.HandlePlayUses and cas.InsertDevicePlayData methods");
        }

        private static PlaybackContextResponse HandleUserSuspended(int groupId, string userId, string assetId, eAssetTypes assetType, PlaybackContextResponse response, long domainId)
        {
            log.Debug($"User is Suspended. groupId = {groupId}, userId = {userId}, domainId = {domainId}, assetId = {assetId}, " +
                                        $"assetType = {assetType}");
            response.Status = RolesPermissionsManager.GetSuspentionStatus(groupId, Convert.ToInt32(domainId));
            if (response.Status.Args != null && response.Status.Args.Any())
            {
                response.Status.Message = response.Status.Args.Aggregate
                    (response.Status.Message, (result, s) => result.Replace('@' + s.key + '@', s.value));
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

        public static PlayManifestResponse GetPlayManifest(BaseConditionalAccess cas, int groupId, string userId, string assetId, eAssetTypes assetType,
                                                           long fileId, string ip, string udid, PlayContextType playContextType, bool isTokenizedUrl = false, bool isAltUrl = false)
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

                response.Status = Utils.GetMediaIdForAsset(groupId, assetId, assetType, userId, domain, udid, out mediaId, out recording, out program);
                if (response.Status.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Failed to get media ID for assetId = {0}, assetType = {1}", assetId, assetType);
                    return response;
                }

                List<MediaFile> files = Utils.FilterMediaFilesForAsset(groupId, assetType, mediaId, null, null, playContextType, new List<long>() { fileId }, true);

                if (files == null || files.Count == 0)
                {
                    log.ErrorFormat("Failed to get files for assetId = {0}, assetType = {1}, mediaId = {2}", assetId, assetType, mediaId);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoFilesFound, "No files found");
                    return response;
                }

                MediaFile file = files[0];

                PlaybackContextResponse playbackContextResponse = GetPlaybackContext(cas, groupId, userId, assetId, assetType, new List<long>() { fileId },
                                                                                     file.StreamerType.Value, file.Url.Substring(0, file.Url.IndexOf(':')), playContextType,
                                                                                     ip, udid, out var playbackContextOut, UrlType.playmanifest);

                if (playbackContextResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    response.Status = playbackContextResponse.Status;
                    return response;
                }

                if (!isTokenizedUrl)
                {
                    // get adapter
                    bool isDefaultAdapter = false;
                    CDNAdapterResponse adapterResponse = Utils.GetRelevantCDN(groupId, file.GetCdnId(isAltUrl), assetType, ref isDefaultAdapter);
                    switch (assetType)
                    {
                        case eAssetTypes.EPG:
                            response = GetEpgLicensedLink(cas, groupId, userId, program, file, udid, ip, adapterResponse, playContextType, isAltUrl);
                            break;
                        case eAssetTypes.NPVR:
                            response = GetRecordingLicensedLink(cas, groupId, userId, recording, file, udid, ip, adapterResponse);
                            break;
                        case eAssetTypes.MEDIA:
                            response = GetMediaLicensedLink(cas, groupId, userId, file, udid, ip, adapterResponse, isAltUrl);
                            break;
                    }
                }

                if (response.Status.Code == (int)eResponseStatus.OK && domainId > 0)
                {
                    bool isLive = false;
                    if (assetType == eAssetTypes.MEDIA)
                    {
                        string epgChannelId = APILogic.Api.Managers.EpgManager.GetEpgChannelId((int)mediaId, groupId);
                        isLive = !string.IsNullOrEmpty(epgChannelId);
                    }

                    // HandlePlayUses
                    HandlePlayUsesAndDevicePlayData(cas, userId, domainId, (int)file.Id, ip, udid, playbackContextOut, playbackContextResponse.ConcurrencyData, isLive);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in get GetAssetLicensedLink, userId = {0}, fileId = {1}, assetId = {2}, assetType = {3}", userId, fileId, assetId, assetType), ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }

        private static PlayManifestResponse GetMediaLicensedLink(BaseConditionalAccess cas, int groupId, string userId, MediaFile file, string udid, string ip, CDNAdapterResponse adapterResponse, bool isAltUrl = false)
        {
            PlayManifestResponse response = new PlayManifestResponse() { Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            var cdnId = file.GetCdnId(isAltUrl);
            var url = file.GetUrl(isAltUrl);
            try
            {
                if (adapterResponse.Adapter != null && !string.IsNullOrEmpty(adapterResponse.Adapter.AdapterUrl))
                {
                    var link = CDNAdapterController.GetInstance().GetVodLink(groupId, adapterResponse.Adapter.ID, userId, url, file.Type, (int)file.MediaId, (int)file.Id, ip);
                    response.Url = link != null ? link.Url : string.Empty;
                }
                else
                {
                    Dictionary<string, string> licensedLinkParams = Utils.GetLicensedLinkParamsDict(userId, file.Id.ToString(), url, ip, string.Empty, string.Empty, udid, string.Empty);
                    response.Url = cas.GetLicensedLink(cdnId, licensedLinkParams);
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
                    long domainId = UsersCache.Instance().GetDomainIdByUser(int.Parse(userId), groupId);
                    RecordResult recordResult = CdvrAdapterController.GetInstance().GetRecordingLinks(groupId, externalChannelId, recording.ExternalRecordingId, cdvrAdapter.ID, domainId);

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
                    response.Url = recordingLink.Url;
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    // main url
                    var link = CDNAdapterController.GetInstance().GetRecordingLink(groupId, adapterResponse.Adapter.ID, userId, recordingLink.Url, file.Type, recording.ExternalRecordingId, ip);

                    if (link != null && !string.IsNullOrEmpty(link.Url))
                    {
                        response.Url = link.Url;
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to GetRecordingLicensedLink for fileId = {0}, userId = {1}", file.Id, userId), ex);
            }

            return response;
        }

        private static PlayManifestResponse GetEpgLicensedLink(BaseConditionalAccess cas, int groupId, string userId, EPGChannelProgrammeObject program, MediaFile file,
                                                               string udid, string ip, CDNAdapterResponse adapterResponse, PlayContextType context, bool isAltUrl = false)
        {
            PlayManifestResponse response = new PlayManifestResponse() { Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            var cdnId = file.GetCdnId(isAltUrl);
            var url = file.GetUrl(isAltUrl);
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
                    var link = CDNAdapterController.GetInstance().GetEpgLink(groupId, adapterResponse.Adapter.ID, userId, url, file.Type, (int)program.EPG_ID,
                                                                            (int)file.MediaId, (int)file.Id, programStartTime.ToUtcUnixTimestampSeconds(), actionType, ip);
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
                    bool bIsDynamic = Utils.GetStreamingUrlType(cdnId, ref CdnStrID);
                    dURLParams.Add(ApiObjects.Epg.EpgLinkConstants.IS_DYNAMIC, bIsDynamic);
                    dURLParams.Add(ApiObjects.Epg.EpgLinkConstants.BASIC_LINK, url);

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

        public static AdsControlData GetDomainAdsControl(int groupId, long domainId)
        {
            AdsControlData adsData = null;

            DomainEntitlements domainEntitlements = null;
            if (Utils.TryGetDomainEntitlementsFromCache(groupId, (int)domainId, null, ref domainEntitlements))
            {
                // get domain subscriptions ordered by priority
                List<string> subscriptionIds = domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions.Select(x => x.Value.sBundleCode).ToList();
                if (subscriptionIds != null && subscriptionIds.Count > 0)
                {
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

        internal static AdsControlResponse GetAdsContext(BaseConditionalAccess cas, int groupId, string userId, string udid, string ip, string assetId, eAssetTypes assetType,
            List<long> fileIds, StreamerType? streamerType, string mediaProtocol, PlayContextType context)
        {
            AdsControlResponse response = new AdsControlResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
            };

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
                response.Status = Utils.GetMediaIdForAsset(groupId, assetId, assetType, userId, domain, udid, out mediaId, out recording, out program);
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

                List<MediaFile> files = Utils.FilterMediaFilesForAsset(groupId, assetType, mediaId, streamerType, mediaProtocol, context, fileIds);
                if (files != null && files.Count > 0)
                {
                    MediaFileItemPricesContainer[] prices = cas.GetItemsPrices(files.Select(f => (int)f.Id).ToArray(), userId, true, string.Empty, udid, ip);
                    if (prices != null && prices.Length > 0)
                    {
                        response.Sources = new List<AdsControlData>();
                        AdsControlData defaultAdsData = GetDomainAdsControl(groupId, domainId);

                        foreach (MediaFileItemPricesContainer price in prices)
                        {
                            AdsControlData adsData = null;
                            if (Utils.IsItemPurchased(price))
                            {
                                adsData = GetFileAdsDataFromBusinessModule(groupId, price, udid);
                                if (adsData == null)
                                {
                                    adsData = defaultAdsData;
                                }
                            }
                            else if (Utils.IsFreeItem(price))
                            {
                                adsData = defaultAdsData;
                            }

                            if (adsData == null)
                            {
                                adsData = new AdsControlData();
                            }

                            adsData.FileId = price.m_nMediaFileID;
                            adsData.FileType = files.Where(f => f.Id == price.m_nMediaFileID).FirstOrDefault().Type;

                            response.Sources.Add(Copy(adsData));
                        }
                    }
                }
                else
                {
                    log.DebugFormat("No files found for asset assetId = {0}, assetType = {1}, streamerType = {2}, protocols = {3}", assetId, assetType, streamerType, mediaProtocol);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoFilesFound, "No files found");
                    return response;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to GetAdsContext for userId = {0}, assetId = {1}, assetType = {2}", userId, assetId, assetType), ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }

        private static AdsControlData Copy(AdsControlData adsData)
        {
            return new AdsControlData
            {
                FileType = adsData.FileType,
                FileId = adsData.FileId,
                AdsPolicy = adsData.AdsPolicy,
                AdsParam = adsData.AdsParam
            };
        }

        public static PlaybackContextResponse GetPlaybackManifest(int groupId, string assetId, eAssetTypes assetType, List<long> fileIds,
                StreamerType? streamerType, string mediaProtocol, PlayContextType context, string sourceType = null, string userId = null, string udid = null)
        {
            PlaybackContextResponse response = new PlaybackContextResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
            };

            try
            {
                long mediaId;
                Recording recording = null;
                EPGChannelProgrammeObject program = null;
                bool isExternalRecordingIgnoreMode = assetType == eAssetTypes.NPVR && TvinciCache.GroupsFeatures.GetGroupFeatureStatus(groupId, GroupFeature.EXTERNAL_RECORDINGS);
                if (assetType != eAssetTypes.MEDIA)
                {
                    Domain domain = null;
                    if (int.TryParse(userId, out int user))
                    {
                        long domainId = UsersCache.Instance().GetDomainIdByUser(int.Parse(userId), groupId);
                        domain = DomainsCache.Instance().GetDomain((int)domainId, groupId);
                    }

                    response.Status = Utils.GetMediaIdForAsset(groupId, assetId, assetType, userId, domain, udid, out mediaId, out recording, out program);
                }
                else
                {
                    mediaId = long.Parse(assetId);
                }
                // Allow to continue for external recording (and asset type = NPVR) since we may not be updated on them in real time
                if (response.Status.Code != (int)eResponseStatus.OK)
                {
                    if (isExternalRecordingIgnoreMode && fileIds != null && fileIds.Count > 0)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                        mediaId = Utils.GetMediaIdByFileId(groupId, (int)fileIds[0]);
                    }
                    else
                    {
                        return response;
                    }
                }

                MediaObj epgChannelLinearMedia = null;

                // Recording
                if (assetType == eAssetTypes.NPVR)
                {
                    epgChannelLinearMedia = Utils.GetMediaById(groupId, (int)mediaId);

                    // get TSTV settings
                    var tstvSettings = Utils.GetTimeShiftedTvPartnerSettings(groupId);

                    // validate recording channel exists or the settings allow it to not exist
                    if (epgChannelLinearMedia == null && (!tstvSettings.IsRecordingPlaybackNonExistingChannelEnabled.HasValue || !tstvSettings.IsRecordingPlaybackNonExistingChannelEnabled.Value))
                    {
                        log.Error($"EPG channel does not exist and TSTV settings do not allow playback in this case. groupId = {groupId}, domainRecordingId = {assetId}, channelId = {recording.ChannelId}, recordingId = {recording.Id}");
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecordingPlaybackNotAllowedForNonExistingEpgChannel, "Recording playback is not allowed for non existing EPG channel");
                        return response;
                    }

                    if (recording != null)
                    {
                        if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Items != null)
                        {
                            System.Web.HttpContext.Current.Items[RequestContextConstants.RECORDING_CONVERT_KEY] = recording.EpgId;
                        }
                        else
                        {
                            log.ErrorFormat("Error when trying to save epgId in httpContext key {0} for GetPlaybackContext on recording assetId {1}",
                                RequestContextConstants.RECORDING_CONVERT_KEY, assetId);
                        }
                    }
                }

                List<MediaFile> files = Utils.FilterMediaFilesForAsset(groupId, assetType, mediaId, streamerType, mediaProtocol, context, fileIds, false, sourceType);
                if (files != null && files.Count > 0)
                {
                    response.Files = files;
                }
                else
                {
                    log.DebugFormat("No files found for asset assetId = {0}, assetType = {1}, streamerType = {2}, protocols = {3}", assetId, assetType, streamerType, mediaProtocol);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoFilesFound, "No files found");
                    return response;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed to GetPlaybackManifest for assetId = {assetId}, assetType = {assetType}", ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }
    }
}