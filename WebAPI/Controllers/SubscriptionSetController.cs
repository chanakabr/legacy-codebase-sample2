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
    [Service("subscriptionSet")]
    public class SubscriptionSetController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
       
        /// <summary>
        /// Returns a list of subscriptionSets requested by ids or subscription ids
        /// </summary>
        /// <param name="filter">SubscriptionSet filter</param>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaSubscriptionSetListResponse List(KalturaSubscriptionSetFilter filter = null)
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
                if (filter is KalturaSubscriptionDependencySetFilter)
                {
                    KalturaSubscriptionDependencySetFilter dfilter = (KalturaSubscriptionDependencySetFilter)filter;
                    if (!dfilter.TypeEqual.HasValue)
                    {
                        dfilter.TypeEqual = KalturaSubscriptionSetType.DEPENDENCY; // default for this filter type
                    }
                    // call client
                    if (!string.IsNullOrEmpty(dfilter.BaseSubscriptionIdIn))
                    {
                        response = ClientsManager.PricingClient().GetSubscriptionSetsBySBaseSubscriptionIds(groupId, dfilter.GetBaseSubscriptionIdContains(), dfilter.OrderBy, dfilter.TypeEqual);                        
                    }
                    else
                    {
                        response = ClientsManager.PricingClient().GetSubscriptionSets(groupId, dfilter.GetIdIn(), dfilter.OrderBy, filter.TypeEqual);
                    }
                }
                else if (!string.IsNullOrEmpty(filter.SubscriptionIdContains))
                {
                    // call client
                    response = ClientsManager.PricingClient().GetSubscriptionSetsBySubscriptionIds(groupId, filter.GetSubscriptionIdContains(), filter.OrderBy, filter.TypeEqual);  
                }
                else
                {
                    // call client
                    response = ClientsManager.PricingClient().GetSubscriptionSets(groupId, filter.GetIdIn(), filter.OrderBy, filter.TypeEqual);
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
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.SubscriptionAlreadyBelongsToAnotherSubscriptionSet)]
        [Throws(eResponseStatus.BaseSubscriptionAlreadyBelongsToAnotherSubscriptionSet)]
        static public KalturaSubscriptionSet Add(KalturaSubscriptionSet subscriptionSet)
        {
            KalturaSubscriptionSet response = null;
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(subscriptionSet.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            try
            {
                List<long> subscriptionIds = subscriptionSet.SubscriptionIds != null ? subscriptionSet.GetSubscriptionIds() : new List<long>();
                
                if (subscriptionSet is KalturaSubscriptionDependencySet)
                {
                    KalturaSubscriptionDependencySet dSubscriptionSet = (KalturaSubscriptionDependencySet)subscriptionSet;
                    if (!dSubscriptionSet.BaseSubscriptionId.HasValue)
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "BaseSubscriptionId");
                        if (! (dSubscriptionSet.BaseSubscriptionId.Value > 0))
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_STRING_CONTAINED_MIN_VALUE_CROSSED, "BaseSubscriptionId", 1);
                        }
                    }
                    
                    // call client
                    response = ClientsManager.PricingClient().AddSubscriptionDependencySet(groupId, dSubscriptionSet.Name, dSubscriptionSet.BaseSubscriptionId.Value, subscriptionIds);
                }
                else
                {
                    // call client
                    response = ClientsManager.PricingClient().AddSubscriptionSet(groupId, subscriptionSet.Name, subscriptionIds);
                }
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
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.SubscriptionAlreadyBelongsToAnotherSubscriptionSet)]
        [Throws(eResponseStatus.SubscriptionSetDoesNotExist)]
        [Throws(eResponseStatus.BaseSubscriptionAlreadyBelongsToAnotherSubscriptionSet)]
        [SchemeArgument("id", MinLong=1)]       
        static public KalturaSubscriptionSet Update(long id, KalturaSubscriptionSet subscriptionSet)
        {
            KalturaSubscriptionSet response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                List<long> subscriptionIds = new List<long>();
                bool shouldUpdateSubscriptionIds = subscriptionSet.SubscriptionIds != null;
                if (shouldUpdateSubscriptionIds && !string.IsNullOrEmpty(subscriptionSet.SubscriptionIds))
                {
                    subscriptionIds = subscriptionSet.GetSubscriptionIds();
                }

                if (subscriptionSet is KalturaSubscriptionDependencySet)
                {
                    KalturaSubscriptionDependencySet dSubscriptionSet = (KalturaSubscriptionDependencySet)subscriptionSet;    
                    
                    // call client
                    response = ClientsManager.PricingClient().UpdateSubscriptionDependencySet(groupId, id, dSubscriptionSet.Name, dSubscriptionSet.BaseSubscriptionId, subscriptionIds, shouldUpdateSubscriptionIds);
                }
                else
                {
                    // call client
                    response = ClientsManager.PricingClient().UpdateSubscriptionSet(groupId, id, subscriptionSet.Name, subscriptionIds, shouldUpdateSubscriptionIds);
                }
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
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.SubscriptionSetDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        static public bool Delete(long id)
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

        /// <summary>
        /// Get the subscriptionSet according to the Identifier
        /// </summary>
        /// <param name="id">SubscriptionSet Identifier</param>
        /// <returns></returns>
        [Action("get")]
        [ApiAuthorize]
        [Throws(eResponseStatus.SubscriptionSetDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        static public KalturaSubscriptionSet Get(long id)
        {
            KalturaSubscriptionSet response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.PricingClient().GetSubscriptionSet(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}