using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
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
        /// <param name="asset_id">Asset identifier - relevant only for asset_type = 'epg'</param>
        /// <param name="asset_type">Asset type</param>
        /// <param name="base_url">Base URL for the licensed URLs</param>
        /// <param name="content_id">Identifier of the content to get the link for(file identifier)</param>
        /// <param name="start_date">The start date of the stream (epoch) - relevant only for asset_type = 'epg'</param>
        /// <param name="stream_type">The stream type to get the URL for - relevant only for asset_type = 'epg'</param>
        /// <remarks>Possible status codes: Device not in household = 1003, Invalid base URL = 3004, Media concurrency limitation = 4000, Concurrency limitation = 4001, 
        /// Device type not allowed = 1002, Household suspended = 1009, User suspended = 2001, Service not allowed = 3003, Not entitled = 3032</remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaLicensedUrl Get(KalturaAssetType asset_type, int content_id, string base_url, string asset_id = null, long? start_date = null, KalturaStreamType? stream_type = null)
        {
            KalturaLicensedUrl response = null;
            
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;
            string userId = ks.UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            try
            {
                switch (asset_type)
                {
                    case KalturaAssetType.media:
                        response = ClientsManager.ConditionalAccessClient().GetLicensedLinks(groupId, userId, udid, content_id, base_url);
                        break;
                    case KalturaAssetType.epg:
                        {
                            if (!stream_type.HasValue)
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "stream_type cannot be null for epg");
                            }
                            if (!start_date.HasValue)
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "start_date cannot be null for epg");
                            }
                            if (string.IsNullOrEmpty(asset_id))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id cannot be empty for epg");
                            }
                            int epgId = 0;
                            if (!int.TryParse(asset_id, out epgId))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id must be a number for epg");
                            }
                            response = ClientsManager.ConditionalAccessClient().GetEPGLicensedLink(groupId, userId, udid, epgId, content_id, base_url, start_date.Value, stream_type.Value);
                        }
                        break;
                    case KalturaAssetType.recording:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Not implemented");
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Not implemented");
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