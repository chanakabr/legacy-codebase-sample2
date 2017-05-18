using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Models.API;
using WebAPI.Models.Pricing;
using WebAPI.Utils;
using WebAPI.Managers.Models;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;
using ApiObjects.Response;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/subscriptionSet/action")]
    public class SubscriptionSetController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
       
        /// <summary>
        /// Returns a list of subscriptionSets requested by ids or subscription ids
        /// </summary>
        /// <param name="filter">SubscriptionSet filter</param>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaSubscriptionSetListResponse List(KalturaSubscriptionSetFilter filter = null)
        {
            if (filter == null)
            {
                filter = new KalturaSubscriptionSetFilter();
            }
            else
            {
                filter.Validate();
            }

            KalturaSubscriptionSetListResponse response = new KalturaSubscriptionSetListResponse();            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (!string.IsNullOrEmpty(filter.SubscriptionIdContains))
                {
                    // call client
                    response = ClientsManager.PricingClient().GetSubscriptionSetsBySubscriptionIds(groupId, filter.GetSubscriptionIdContains(), filter.OrderBy);  
                }
                else
                {
                    // call client
                    response = ClientsManager.PricingClient().GetSubscriptionSets(groupId, filter.GetIdIn(), filter.OrderBy);
                }
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add a new subscriptionSet
        /// </summary>
        /// <param name="subscriptionSet">SubscriptionSet Object</param>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.SubscriptionAlreadyBelongsToAnotherSubscriptionSet)]        
        public KalturaSubscriptionSet Add(KalturaSubscriptionSet subscriptionSet)
        {
            KalturaSubscriptionSet response = null;
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(subscriptionSet.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            try
            {
                List<long> subscriptionIds = subscriptionSet.SubscriptionIds != null ? subscriptionSet.SubscriptionIds.Select(x => x.value).Distinct().ToList() : new List<long>();
                // call client
                response = ClientsManager.PricingClient().AddSubscriptionSet(groupId, subscriptionSet.Name, subscriptionIds);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update the subscriptionSet
        /// </summary>
        /// <param name="id">SubscriptionSet Identifier</param>
        /// <param name="subscriptionSet">SubscriptionSet Object</param>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.SubscriptionAlreadyBelongsToAnotherSubscriptionSet)]
        [Throws(eResponseStatus.SubscriptionSetDoesNotExist)]
        [SchemeArgument("id", MinLong=1)]
        public KalturaSubscriptionSet Update(long id, KalturaSubscriptionSet subscriptionSet)
        {
            KalturaSubscriptionSet response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                bool shouldUpdateSubscriptionIds = subscriptionSet.SubscriptionIds != null;
                List<long> subscriptionIds = new List<long>();
                if (shouldUpdateSubscriptionIds)
                {
                    subscriptionIds = subscriptionSet.SubscriptionIds.Select(x => x.value).Distinct().ToList();
                }
                
                // call client
                response = ClientsManager.PricingClient().UpdateSubscriptionSet(groupId, id, subscriptionSet.Name, subscriptionIds, shouldUpdateSubscriptionIds);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete a subscriptionSet
        /// </summary>
        /// <param name="id">SubscriptionSet Identifier</param>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.SubscriptionSetDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        public bool Delete(long id)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {                
                // call client
                result = ClientsManager.PricingClient().DeleteSubscriptionSet(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        ///// <summary>
        ///// Get the subscriptionSet according to the Identifier
        ///// </summary>
        ///// <param name="id">SubscriptionSet Identifier</param>
        ///// <returns></returns>
        //[Route("get"), HttpPost]
        //[ApiAuthorize]
        //[Throws(eResponseStatus.SubscriptionSetDoesNotExist)]
        //[SchemeArgument("id", MinLong = 1)]
        //public KalturaSubscriptionSet Get(long id)
        //{
        //    KalturaSubscriptionSet response = null;
        //    int groupId = KS.GetFromRequest().GroupId;

        //    try
        //    {
        //        // call client
        //        response = ClientsManager.PricingClient().GetSubscriptionSet(groupId, id);
        //    }
        //    catch (ClientException ex)
        //    {
        //        ErrorUtils.HandleClientException(ex);
        //    }

        //    return response;
        //}

    }
}