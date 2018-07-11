using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Domains;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("householdLimitations")]
    public class HouseholdLimitationsController : IKalturaController
    {
        /// <summary>
        /// Get the limitation module by id
        /// </summary>
        /// <param name="id">Household limitations module identifier</param>
        /// <returns></returns>
        [Action("get")]
        [ApiAuthorize]
        static public KalturaHouseholdLimitations Get(int id)
        {
            KalturaHouseholdLimitations response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.DomainsClient().GetDomainLimitationModule(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}