using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;
using WebAPI.Utils;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;
using WebAPI.Clients;
using Core.Pricing;
using ApiObjects.Response;
using ApiLogic.Pricing.Handlers;

namespace WebAPI.Controllers
{
    [Service("collection")]
    public class CollectionController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns a list of subscriptions requested by Subscription ID or file ID
        /// </summary>
        /// <param name="filter">Filter request</param>
        /// <param name="pager">Page size and index</param>
        /// <remarks>Possible status codes:      
        ///   </remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaCollectionListResponse List(KalturaCollectionFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaCollectionListResponse response = new KalturaCollectionListResponse();

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            if (filter == null)
            {
                filter = new KalturaCollectionFilter();
            }
            else
            {
                filter.Validate();
            }
            
            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();
            Func<GenericListResponse<Collection>> getListFunc;
            KalturaGenericListResponse<KalturaCollection> result = null;
            try
            {
                if (!string.IsNullOrEmpty(filter.CollectionIdIn))
                {
                    getListFunc = () =>
                    CollectionManager.Instance.GetCollectionsData(groupId, filter.getCollectionIdIn(), string.Empty, language, udid, pager.getPageIndex(), pager.PageSize.Value, false, filter.CouponGroupIdEqual);
                    result = ClientUtils.GetResponseListFromWS<KalturaCollection, Collection>(getListFunc);
                }
                else if (filter.MediaFileIdEqual.HasValue)
                {
                    IdsResponse collectionsIdsresult = CollectionManager.Instance.GetCollectionIdsContainingMediaFile(groupId, 0, filter.MediaFileIdEqual.Value);

                    if (collectionsIdsresult == null)
                    {
                        throw new ClientException(StatusCode.Error);
                    }

                    List<int> collectionsIds = collectionsIdsresult.Ids;

                    // get collections
                    if (collectionsIds != null && collectionsIds.Count > 0)
                    {
                        getListFunc = () =>
                        CollectionManager.Instance.GetCollectionsData(groupId, collectionsIds.Select(id => id.ToString()).ToArray(), string.Empty, language, udid, pager.getPageIndex(), pager.PageSize.Value, false, filter.CouponGroupIdEqual);
                        result = ClientUtils.GetResponseListFromWS<KalturaCollection, Collection>(getListFunc);
                    }
                }
                else
                {
                    getListFunc = () =>
                       CollectionManager.Instance.GetCollectionsData(groupId, string.Empty, language, udid, pager.getPageIndex(), pager.PageSize.Value, false, filter.CouponGroupIdEqual);
                    result = ClientUtils.GetResponseListFromWS<KalturaCollection, Collection>(getListFunc);
                }

                if (result != null)
                {
                    response.Collections = result.Objects;
                    response.TotalCount = result.TotalCount;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Internal API !!! Insert new collection for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="collection">collection object</param>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaCollection Add(KalturaCollection collection)
        {
            
            KalturaCollection result = null;
            collection.ValidateForAdd();
            var contextData = KS.GetContextData();

            Func<Collection, GenericResponse<Collection>> insertCollectionFunc = (Collection collectionToInsert) =>
                      CollectionManager.Instance.Add(contextData, collectionToInsert);

            result = ClientUtils.GetResponseFromWS<KalturaCollection, Collection>(collection, insertCollectionFunc);

            return result;
        }

        /// <summary>
        /// Internal API !!! Delete collection 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Collection id</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CollectionNotExist)]
        static public bool Delete(long id)
        {
            bool result = false;

            var contextData = KS.GetContextData();

            try
            {
                Func<Status> delete = () => CollectionManager.Instance.Delete(contextData, id);

                result = ClientUtils.GetResponseStatusFromWS(delete);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }
    }
}