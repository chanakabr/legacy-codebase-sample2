using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("ppv")]
    public class PpvController : IKalturaController
    {
        /// <summary>
        /// Returns ppv object by internal identifier
        /// </summary>
        /// <param name="id">ppv identifier</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: ModuleNotExists = 9016</remarks>     
        [Action("get")]
        [ApiAuthorize]
        static public KalturaPpv Get(long id)
        {
            KalturaPpv response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.PricingClient().GetPPVModuleData(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}