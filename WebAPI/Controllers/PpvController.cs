using System;
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

        /// <summary>
        /// Returns all ppv objects
        /// </summary>  
        /// <param name="filter">Filter parameters for filtering out the result</param>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaPpvListResponse List(KalturaPpvFilter filter = null)
        {
            KalturaPpvListResponse response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;

                if (filter == null)
                    filter = new KalturaPpvFilter();

                if( filter.GetIdIn().Count > 0)
                { 
                    // call client                
                    response = ClientsManager.PricingClient().GetPPVModulesData(groupId, filter.GetIdIn(), filter.OrderBy);
                }
                else
                {
                    // call client                
                    response = ClientsManager.PricingClient().GetPPVModulesData(groupId, filter.OrderBy);
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