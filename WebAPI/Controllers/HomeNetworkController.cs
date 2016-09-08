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
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/homeNetwork/action")]
    [OldStandardAction("listOldStandard", "list")]
    [OldStandardAction("updateOldStandard", "update")]
    public class HomeNetworkController : ApiController
    {
        /// <summary>
        /// Add a new home network to a household
        /// </summary>
        /// <param name="homeNetwork">Home network to add</param>
        /// <remarks>
        /// Possible status codes:
        /// Home network already exists = 1031, Home network limitation = 1032, External identifier is required = 6016
        /// </remarks>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [OldStandard("homeNetwork", "home_network")]
        [Throws(eResponseStatus.HomeNetworkAlreadyExists)]
        [Throws(eResponseStatus.HomeNetworkLimitation)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        public KalturaHomeNetwork Add(KalturaHomeNetwork homeNetwork)
        {
            KalturaHomeNetwork response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                response = ClientsManager.DomainsClient().AddDomainHomeNetwork(groupId, householdId, homeNetwork.ExternalId, homeNetwork.Name, homeNetwork.Description, homeNetwork.getIsActive());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Retrieve the household’s home networks
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaHomeNetworkListResponse List()
        {
            List<KalturaHomeNetwork> list = null;

            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                list = ClientsManager.DomainsClient().GetDomainHomeNetworks(groupId, householdId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaHomeNetworkListResponse() { Objects = list, TotalCount = list.Count };
        }

        /// <summary>
        /// Retrieve the household’s home networks
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<KalturaHomeNetwork> ListOldStandard()
        {
            List<KalturaHomeNetwork> response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                response = ClientsManager.DomainsClient().GetDomainHomeNetworks(groupId, householdId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update and existing home network for a household
        /// </summary>
        /// <param name="externalId">Home network identifier</param>
        /// <param name="homeNetwork">Home network to update</param>
        /// <remarks>
        /// Possible status codes:
        /// Home network does not exist = 1033, External identifier is required = 6016
        /// </remarks>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.HomeNetworkDoesNotExist)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        public KalturaHomeNetwork Update(string externalId, KalturaHomeNetwork homeNetwork)
        {
            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                return ClientsManager.DomainsClient().UpdateDomainHomeNetwork(groupId, householdId, externalId, homeNetwork.Name, homeNetwork.Description, homeNetwork.getIsActive());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Update and existing home network for a household
        /// </summary>
        /// <param name="home_network">Home network to update</param>
        /// <remarks>
        /// Possible status codes:
        /// Home network does not exist = 1033, External identifier is required = 6016
        /// </remarks>
        /// <returns></returns>
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.HomeNetworkDoesNotExist)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        public bool UpdateOldStandard(KalturaHomeNetwork home_network)
        {
            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                ClientsManager.DomainsClient().UpdateDomainHomeNetwork(groupId, householdId, home_network.ExternalId, home_network.Name, home_network.Description, home_network.getIsActive());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Delete household’s existing home network 
        /// </summary>
        /// <param name="externalId">The network to update</param>
        /// <remarks>
        /// Possible status codes:
        /// Home network does not exist = 1033, Home network frequency limitation = 1034, External identifier is required = 6016
        /// </remarks>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandard("externalId", "external_id")]
        [Throws(eResponseStatus.HomeNetworkDoesNotExist)]
        [Throws(eResponseStatus.HomeNetworkFrequency)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        public bool Delete(string externalId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                response = ClientsManager.DomainsClient().RemoveDomainHomeNetwork(groupId, householdId, externalId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}