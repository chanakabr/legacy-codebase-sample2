using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/licensedUrl/action")]
    public class LicensedUrlController : ApiController
    {
        /// <summary>
        /// Get the URL for playing an asset - EPG or media (not available for recording for now).
        /// </summary>
        /// <param name="assetId">Asset identifier - relevant only for asset_type = 'epg'</param>
        /// <param name="assetType">Asset type</param>
        /// <param name="baseUrl">Base URL for the licensed URLs</param>
        /// <param name="contentId">Identifier of the content to get the link for(file identifier)</param>
        /// <param name="startDate">The start date of the stream (epoch) - relevant only for asset_type = 'epg'</param>
        /// <param name="streamType">The stream type to get the URL for - relevant only for asset_type = 'epg'</param>
        /// <remarks>Possible status codes: Device not in household = 1003, Invalid base URL = 3004, Media concurrency limitation = 4000, Concurrency limitation = 4001, 
        /// Device type not allowed = 1002, Household suspended = 1009, User suspended = 2001, Service not allowed = 3003, Not entitled = 3032</remarks>
        [Route("getOldStandard"), HttpPost]
        [ApiAuthorize]
        [OldStandardAction("get")]
        [OldStandardArgument("assetType", "asset_type")]
        [OldStandardArgument("contentId", "content_id")]
        [OldStandardArgument("baseUrl", "base_url")]
        [OldStandardArgument("assetId", "asset_id")]
        [OldStandardArgument("startDate", "start_date")]
        [OldStandardArgument("streamType", "stream_type")]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Obsolete]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.InvalidBaseLink)]
        [Throws(eResponseStatus.MediaConcurrencyLimitation)]
        [Throws(eResponseStatus.ConcurrencyLimitation)]
        [Throws(eResponseStatus.DeviceTypeNotAllowed)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.ServiceNotAllowed)]
        [Throws(eResponseStatus.NotEntitled)]
        public KalturaLicensedUrl GetOldStandard(KalturaAssetType assetType, int contentId, string baseUrl, string assetId = null, long? startDate = null, KalturaStreamType? streamType = null)
        {
            KalturaLicensedUrl response = null;
            
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;
            string userId = ks.UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            try
            {
                switch (assetType)
                {
                    case KalturaAssetType.media:
                        response = ClientsManager.ConditionalAccessClient().GetLicensedLinks(groupId, userId, udid, contentId, baseUrl);
                        break;
                    case KalturaAssetType.epg:
                        {
                            if (!streamType.HasValue)
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "streamType");
                            }
                            if (!startDate.HasValue)
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "startDate");
                            }
                            if (string.IsNullOrEmpty(assetId))
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "assetId");
                            }
                            int epgId = 0;
                            if (!int.TryParse(assetId, out epgId))
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "assetId");
                            }
                            response = ClientsManager.ConditionalAccessClient().GetEPGLicensedLink(groupId, userId, udid, epgId, contentId, baseUrl, startDate.Value, streamType.Value);
                        }
                        break;
                    case KalturaAssetType.recording:
                        throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "assetType", "KalturaAssetType.recording");
                    default:
                        throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "assetType", "KalturaAssetType." + assetType.ToString());
                }
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get the URL for playing an asset - program, media or recording
        /// </summary>
        /// <param name="request">Licensed URL request parameters</param>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.InvalidBaseLink)]
        [Throws(eResponseStatus.MediaConcurrencyLimitation)]
        [Throws(eResponseStatus.ConcurrencyLimitation)]
        [Throws(eResponseStatus.DeviceTypeNotAllowed)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.ServiceNotAllowed)]
        [Throws(eResponseStatus.NotEntitled)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.ProgramDoesntExist)]
        [Throws(eResponseStatus.RecordingPlaybackNotAllowedForNonExistingEpgChannel)]
        [Throws(eResponseStatus.RecordingPlaybackNotAllowedForNotEntitledEpgChannel)]
        public KalturaLicensedUrl Get(KalturaLicensedUrlBaseRequest request)
        {
            KalturaLicensedUrl response = null;

            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;
            string userId = ks.UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            try
            {
                
                if (request is KalturaLicensedUrlEpgRequest) 
                {
                    KalturaLicensedUrlEpgRequest epgRequest = request as KalturaLicensedUrlEpgRequest;
                    epgRequest.Validate();

                    response = ClientsManager.ConditionalAccessClient().GetEPGLicensedLink(groupId, userId, udid, epgRequest.getEpgId(), epgRequest.ContentId, epgRequest.BaseUrl, epgRequest.StartDate, epgRequest.StreamType);
                }
                else if (request is KalturaLicensedUrlRecordingRequest) 
                {
                    KalturaLicensedUrlRecordingRequest recordingRequest = request as KalturaLicensedUrlRecordingRequest;

                    response = ClientsManager.ConditionalAccessClient().GetRecordingLicensedLink(groupId, userId, udid, recordingRequest.GetRecordingId(), recordingRequest.FileType);
                }
                else if (request is KalturaLicensedUrlMediaRequest)
                {
                    KalturaLicensedUrlMediaRequest mediaRequest = request as KalturaLicensedUrlMediaRequest;
                    mediaRequest.Validate();
                    response = ClientsManager.ConditionalAccessClient().GetLicensedLinks(groupId, userId, udid, mediaRequest.ContentId, mediaRequest.BaseUrl);
                }
                else
                {
                    throw new InternalServerErrorException();
                }
                
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}