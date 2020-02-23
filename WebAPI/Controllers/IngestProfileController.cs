using ApiObjects.Response;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("IngestProfile")]
    public class IngestProfileController : IKalturaController
    {
        /// <summary>
        /// Returns all ingest profiles for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaIngestProfileListResponse List()
        {
            var response = new KalturaIngestProfileListResponse();
            var groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().GetIngestProfiles(groupId);
                return response;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete ingest profiles by ingest profiles id
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// ingest profile not exist = 5048
        /// </remarks>
        /// <param name="ingestProfileId">ingest profile Identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.IngestProfileNotExists)]
        static public bool Delete(int ingestProfileId)
        {
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId;

            try
            {
                var response = ClientsManager.ApiClient().DeleteIngestProfiles(groupId, ingestProfileId, int.Parse(userId));
                return response;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return false;
        }

        /// <summary>
        /// Insert new ingest profile for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///   External identifier is required = 6016, Name is required = 5005, Shared secret is required = 5006, External identifier must be unique = 6040, No ingest profile to insert = 5049
        /// </remarks>
        /// <param name="ingestProfile">ingest profile Object to be added</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.SharedSecretRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        [Throws(eResponseStatus.NoIngestProfileToInsert)]
        static public KalturaIngestProfile Add(KalturaIngestProfile ingestProfile)
        {
            KalturaIngestProfile response = null;
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId;

            try
            {
                response = ClientsManager.ApiClient().InsertIngestProfile(groupId, ingestProfile, int.Parse(userId));
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update ingest profile details
        /// </summary>
        /// <param name="ingestProfileId">ingest profile Identifier</param> 
        /// <param name="ingestProfile">ingest profile Object</param>       
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.IngestProfileIdRequired)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        static public KalturaIngestProfile Update(int ingestProfileId, KalturaIngestProfile ingestProfile)
        {
            KalturaIngestProfile response = null;
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId;
            try
            {
                response = ClientsManager.ApiClient().UpdateIngestProfile(groupId, ingestProfileId, ingestProfile, int.Parse(userId));
                return response;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

    }
}