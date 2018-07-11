using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("unifiedPayment")]
    public class UnifiedPaymentController : IKalturaController
    {
        /// <summary>
        /// Returns the data about the next renewal 
        /// </summary>                
        /// <param name="id">Unified payment ID</param>
        [Action("getNextRenewal")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        static public KalturaUnifiedPaymentRenewal GetNextRenewal(int id)
        {
            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                return ClientsManager.ConditionalAccessClient().GetUnifiedPaymentNextRenewal(groupId, householdId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return null;
        }
    }
}