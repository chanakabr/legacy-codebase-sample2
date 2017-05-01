using ApiObjects.Response;
using System.Collections.Generic;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/engagementAdapter/action")]
    public class EngagementAdapterController : ApiController
    {
        /// <summary>
        /// Returns all Engagement adapters for partner : id + name
        /// </summary>
        /// <remarks>       
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaEngagementAdapterListResponse List()
        {
            List<KalturaEngagementAdapter> list = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                list = ClientsManager.NotificationClient().GetEngagementAdapters(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            KalturaEngagementAdapterListResponse response = new KalturaEngagementAdapterListResponse()
            {
                EngagementAdapters = list,
                TotalCount = list.Count
            };

            return response;
        }

        /// <summary>
        /// Returns all Engagement adapters for partner : id + name
        /// </summary>
        /// <param name="id">Engagement adapter identifier</param>
        /// <remarks>       
        /// </remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaEngagementAdapter Get(int id)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                return ClientsManager.NotificationClient().GetEngagementAdapter(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Delete Engagement adapter by Engagement adapter id
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// engagement adapter identifier required = 8025, engagement adapter not exist = 8026,  action is not allowed = 5011
        /// </remarks>
        /// <param name="id">Engagement adapter identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.EngagementAdapterIdentifierRequired)]
        [Throws(eResponseStatus.EngagementAdapterNotExist)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        public bool Delete(int id)
        {
            bool response = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().DeleteEngagementAdapter(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new Engagement adapter for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// no engagement adapter to insert = 8028, name required = 5005, provider url required = 8033
        /// </remarks>
        /// <param name="engagementAdapter">Engagement adapter Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.NoEngagementAdapterToInsert)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.ProviderUrlRequired)]
        public KalturaEngagementAdapter Add(KalturaEngagementAdapter engagementAdapter)
        {
            KalturaEngagementAdapter response = null;

            if (string.IsNullOrWhiteSpace(engagementAdapter.AdapterUrl))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "adapterUrl");

            if (string.IsNullOrWhiteSpace(engagementAdapter.ProviderUrl))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "providerUrl");
            
            if (string.IsNullOrWhiteSpace(engagementAdapter.Name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().InsertEngagementAdapter(groupId, engagementAdapter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update Engagement adapter details
        /// </summary>
        /// <remarks>
        /// Possible status codes:   
        /// name required = 5005, engagement adapter identifier required = 8025, no engagement adapter to update = 8029, provider url required = 8033
        /// </remarks>
        /// <param name="id">Engagement adapter identifier</param>       
        /// <param name="engagementAdapter">Engagement adapter Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.EngagementAdapterIdentifierRequired)]
        [Throws(eResponseStatus.NoEngagementAdapterToUpdate)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.ProviderUrlRequired)]
        public KalturaEngagementAdapter Update(int id, KalturaEngagementAdapter engagementAdapter)
        {
            KalturaEngagementAdapter response = null;

            int groupId = KS.GetFromRequest().GroupId;
            engagementAdapter.Id = id;

            if (string.IsNullOrWhiteSpace(engagementAdapter.AdapterUrl))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "adapterUrl");

            if (string.IsNullOrWhiteSpace(engagementAdapter.ProviderUrl))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "providerUrl");

            if (string.IsNullOrWhiteSpace(engagementAdapter.Name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");

            try
            {
                // call client
                response = ClientsManager.NotificationClient().SetEngagementAdapter(groupId, engagementAdapter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

       
        /// <summary>
        /// Generate engagement adapter shared secret
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// engagement adapter identifier required = 8025, engagement adapter not exist = 8026
        /// </remarks>
        /// <param name="id">Engagement adapter identifier</param>
        [Route("generateSharedSecret"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.EngagementAdapterIdentifierRequired)]
        [Throws(eResponseStatus.EngagementAdapterNotExist)]
        public KalturaEngagementAdapter GenerateSharedSecret(int id)
        {
            KalturaEngagementAdapter response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().GenerateEngagementSharedSecret(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


    }
}