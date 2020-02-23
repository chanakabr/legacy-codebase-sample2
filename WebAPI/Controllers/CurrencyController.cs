using KLogMonitor;
using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("currency")]
    public class CurrencyController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get the list of currencies for the partner with option to filter by currency codes
        /// </summary>
        /// <param name="filter">currency filter</param>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaCurrencyListResponse List(KalturaCurrencyFilter filter)
        {
            KalturaCurrencyListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            filter.Validate();

            try
            {
                if (filter.ExcludePartner.HasValue && filter.ExcludePartner.Value)
                {
                    response = ClientsManager.ApiClient().GetAllCurrencies(groupId, filter.OrderBy);
                }
                else
                {
                    response = ClientsManager.ApiClient().GetCurrencyList(groupId, filter.GetCodeIn(), filter.OrderBy);
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