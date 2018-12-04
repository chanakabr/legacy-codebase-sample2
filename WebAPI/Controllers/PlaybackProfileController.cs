using ApiObjects.Response;
using System.Collections.Generic;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.Notification;
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
        static public KalturaPlaybackProfileListResponse List(KalturaPlaybackProfileFilter filter)
        {
            KalturaPlaybackProfileListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (filter != null && filter.PlaybackProfileIdEqual.HasValue && filter.getPlaybackProfileId() > 0)
                {
                    //response = ClientsManager.ApiClient().GetPlaybackProfile(groupId, filter.getPlaybackProfileId());  //TODO anat check if needed ?
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

        ///// <summary>
        ///// Delete Playback adapter by Playback adapter id
        ///// </summary>
        ///// <remarks>
        ///// Possible status codes:       
        ///// playback adapter identifier required = 8025, playback adapter not exist = 8026,  action is not allowed = 5011
        ///// </remarks>
        ///// <param name="id">Playback adapter identifier</param>
        //[Action("delete")]
        //[ApiAuthorize]
        //[Throws(eResponseStatus.PlaybackProfileIdentifierRequired)]
        //[Throws(eResponseStatus.PlaybackProfileNotExist)]
        //[Throws(eResponseStatus.ActionIsNotAllowed)]
        //static public bool Delete(int id)
        //{
        //    bool response = false;

        //    int groupId = KS.GetFromRequest().GroupId;

        //    try
        //    {
        //        // call client
        //        response = ClientsManager.NotificationClient().DeletePlaybackProfile(groupId, id);
        //    }
        //    catch (ClientException ex)
        //    {
        //        ErrorUtils.HandleClientException(ex);
        //    }

        //    return response;
        //}

        /// <summary>
        /// Insert new Playback adapter for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// no playback adapter to insert = 8028, name required = 5005, provider url required = 8033
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

        ///// <summary>
        ///// Update Playback adapter details
        ///// </summary>
        ///// <remarks>
        ///// Possible status codes:   
        ///// name required = 5005, playback adapter identifier required = 8025, no playback adapter to update = 8029, provider url required = 8033
        ///// </remarks>
        ///// <param name="id">Playback adapter identifier</param>       
        ///// <param name="playbackProfile">Playback adapter Object</param>       
        //[Action("update")]
        //[ApiAuthorize]
        //[Throws(eResponseStatus.PlaybackProfileIdentifierRequired)]
        //[Throws(eResponseStatus.NoPlaybackProfileToUpdate)]
        //[Throws(eResponseStatus.NameRequired)]
        //[Throws(eResponseStatus.ProviderUrlRequired)]
        //static public KalturaPlaybackProfile Update(int id, KalturaPlaybackProfile playbackProfile)
        //{
        //    KalturaPlaybackProfile response = null;

        //    int groupId = KS.GetFromRequest().GroupId;
        //    playbackProfile.Id = id;

        //    if (string.IsNullOrWhiteSpace(playbackProfile.AdapterUrl))
        //        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "adapterUrl");

        //    if (string.IsNullOrWhiteSpace(playbackProfile.ProviderUrl))
        //        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "providerUrl");

        //    if (string.IsNullOrWhiteSpace(playbackProfile.Name))
        //        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");

        //    try
        //    {
        //        // call client
        //        response = ClientsManager.NotificationClient().SetPlaybackProfile(groupId, playbackProfile);
        //    }
        //    catch (ClientException ex)
        //    {
        //        ErrorUtils.HandleClientException(ex);
        //    }

        //    return response;
        //}


        ///// <summary>
        ///// Generate playback adapter shared secret
        ///// </summary>
        ///// <remarks>
        ///// Possible status codes:  
        ///// playback adapter identifier required = 8025, playback adapter not exist = 8026
        ///// </remarks>
        ///// <param name="id">Playback adapter identifier</param>
        //[Action("generateSharedSecret")]
        //[ApiAuthorize]
        //[ValidationException(SchemeValidationType.ACTION_NAME)]
        //[Throws(eResponseStatus.PlaybackProfileIdentifierRequired)]
        //[Throws(eResponseStatus.PlaybackProfileNotExist)]
        //static public KalturaPlaybackProfile GenerateSharedSecret(int id)
        //{
        //    KalturaPlaybackProfile response = null;

        //    int groupId = KS.GetFromRequest().GroupId;

        //    try
        //    {
        //        // call client
        //        response = ClientsManager.NotificationClient().GeneratePlaybackSharedSecret(groupId, id);
        //    }
        //    catch (ClientException ex)
        //    {
        //        ErrorUtils.HandleClientException(ex);
        //    }

        //    return response;
        //}

    }
}

