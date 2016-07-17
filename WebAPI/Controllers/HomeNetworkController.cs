using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/homeNetwork/action")]
    [OldStandardAction("listOldStandard", "list")]
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
        /// <param name="home_network">Home network to update</param>
        /// <remarks>
        /// Possible status codes:
        /// Home network does not exist = 1033, External identifier is required = 6016
        /// </remarks>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(KalturaHomeNetwork home_network)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                response = ClientsManager.DomainsClient().UpdateDomainHomeNetwork(groupId, householdId, home_network.ExternalId, home_network.Name, home_network.Description, home_network.getIsActive());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete household’s existing home network 
        /// </summary>
        /// <param name="external_id">The network to update</param>
        /// <remarks>
        /// Possible status codes:
        /// Home network does not exist = 1033, Home network frequency limitation = 1034, External identifier is required = 6016
        /// </remarks>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(string external_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                response = ClientsManager.DomainsClient().RemoveDomainHomeNetwork(groupId, householdId, external_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}