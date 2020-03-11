using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("playbackProfile")]
    public class PlaybackProfileController : IKalturaController
    {
        /// <summary>
        /// Returns all playback profiles for partner : id + name
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <remarks>       
        /// </remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaPlaybackProfileListResponse List(KalturaPlaybackProfileFilter filter = null)
        {
            KalturaPlaybackProfileListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (filter != null && filter.PlaybackProfileIdEqual.HasValue && filter.getPlaybackProfileId() > 0)
                {
                    response = ClientsManager.ApiClient().GetPlaybackProfile(groupId, (long)filter.getPlaybackProfileId());  
                }
                else
                {
                    response = ClientsManager.ApiClient().GetPlaybackProfiles(groupId);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete Playback adapter by Playback adapter id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Playback adapter identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AdapterIdentifierRequired)]
        [Throws(eResponseStatus.AdapterNotExists)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        static public bool Delete(int id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userId = KS.GetFromRequest().UserId;

                // call client
                response = ClientsManager.ApiClient().DeletePlaybackProfile(groupId, userId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new Playback adapter for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="playbackProfile">Playback adapter Object</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.NoAdapterToInsert)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        static public KalturaPlaybackProfile Add(KalturaPlaybackProfile playbackProfile)
        {
            KalturaPlaybackProfile response = null;

            if (string.IsNullOrWhiteSpace(playbackProfile.AdapterUrl))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "adapterUrl");

            if (string.IsNullOrWhiteSpace(playbackProfile.Name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");

            if (string.IsNullOrWhiteSpace(playbackProfile.SystemName))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userId = KS.GetFromRequest().UserId;

                // call client
                response = ClientsManager.ApiClient().InsertPlaybackProfile(groupId, userId, playbackProfile);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update Playback adapter details
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Playback adapter identifier</param>       
        /// <param name="playbackProfile">Playback adapter Object</param>       
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AdapterIdentifierRequired)]
        [Throws(eResponseStatus.AdapterNotExists)]
        [Throws(eResponseStatus.NameRequired)]
        static public KalturaPlaybackProfile Update(int id, KalturaPlaybackProfile playbackProfile)
        {
            KalturaPlaybackProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;
            playbackProfile.Id = id;

            if (string.IsNullOrWhiteSpace(playbackProfile.AdapterUrl))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "adapterUrl");
            
            if (string.IsNullOrWhiteSpace(playbackProfile.Name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");

            try
            {
                string userId = KS.GetFromRequest().UserId;
                // call client
                response = ClientsManager.ApiClient().SetPlaybackProfile(groupId, userId, playbackProfile);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


        /// <summary>
        /// Generate playback adapter shared secret
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Playback adapter identifier</param>
        [Action("generateSharedSecret")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.AdapterIdentifierRequired)]
        [Throws(eResponseStatus.AdapterNotExists)]
        static public KalturaPlaybackProfile GenerateSharedSecret(int id)
        {
            KalturaPlaybackProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GeneratePlaybackAdapterSharedSecret(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}

