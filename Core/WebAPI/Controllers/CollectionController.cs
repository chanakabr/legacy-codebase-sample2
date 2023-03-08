using ApiLogic.Pricing.Handlers;
using ApiObjects.Pricing;
using ApiObjects.Response;
using Core.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("collection")]
    public class CollectionController : IKalturaController
    {
        /// <summary>
        /// Returns a list of collections requested by Collection IDs or file identifier or coupon group identifier 
        /// </summary>
        /// <param name="filter">Filter request</param>
        /// <param name="pager">Page size and index</param>
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

            var contextData = KS.GetContextData();
            Func<GenericListResponse<Collection>> getListFunc;
            KalturaGenericListResponse<KalturaCollection> result = null;
            try
            {
                bool inactiveAssets = false;
                CollectionOrderBy orderBy = AutoMapper.Mapper.Map<CollectionOrderBy>(filter.OrderBy);

                if (filter.AlsoInactive.HasValue)
                {
                    long userId = Utils.Utils.GetUserIdFromKs();
                    bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, userId.ToString(), true);
                    inactiveAssets = isAllowedToViewInactiveAssets && filter.AlsoInactive.Value;
                }

                if (!string.IsNullOrEmpty(filter.CollectionIdIn))
                {
                    getListFunc = () =>
                    CollectionManager.Instance.GetCollectionsData(contextData, filter.getCollectionIdIn(), string.Empty, pager.GetRealPageIndex(), pager.PageSize.Value, false, 
                                                                  filter.CouponGroupIdEqual, inactiveAssets, orderBy);
                    result = ClientUtils.GetResponseListFromWS<KalturaCollection, Collection>(getListFunc);
                }
                else if (filter.MediaFileIdEqual.HasValue)
                {
                    IdsResponse collectionsIdsresult = CollectionManager.Instance.GetCollectionIdsContainingMediaFile(contextData.GroupId, 0, filter.MediaFileIdEqual.Value);

                    if (collectionsIdsresult == null)
                    {
                        throw new ClientException(StatusCode.Error);
                    }

                    List<int> collectionsIds = collectionsIdsresult.Ids;

                    // get collections
                    if (collectionsIds != null && collectionsIds.Count > 0)
                    {
                        getListFunc = () =>
                        CollectionManager.Instance.GetCollectionsData(contextData, collectionsIds.Select(id => id.ToString()).ToArray(), string.Empty, pager.GetRealPageIndex(), pager.PageSize.Value, false, filter.CouponGroupIdEqual);
                        result = ClientUtils.GetResponseListFromWS<KalturaCollection, Collection>(getListFunc);
                    }
                }
                else
                {
                    getListFunc = () =>
                       CollectionManager.Instance.GetCollectionsData(contextData, string.Empty, pager.GetRealPageIndex(), pager.PageSize.Value, false, filter.CouponGroupIdEqual, 
                       inactiveAssets, orderBy);
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
        /// Insert new collection for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="collection">collection object</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ExternalIdAlreadyExists)]
        [Throws(eResponseStatus.UsageModuleDoesNotExist)]
        [Throws(eResponseStatus.PriceDetailsDoesNotExist)]
        [Throws(eResponseStatus.ChannelDoesNotExist)]
        [Throws(eResponseStatus.CouponGroupNotExist)]
        [Throws(eResponseStatus.InvalidDiscountCode)]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        [Throws(eResponseStatus.InvalidFileTypes)]
        [Throws(eResponseStatus.AssetUserRuleDoesNotExists)]
        static public KalturaCollection Add(KalturaCollection collection)
        {
            KalturaCollection result = null;
            collection.ValidateForAdd();
            var contextData = KS.GetContextData();

            try
            {
                Func<CollectionInternal, GenericResponse<CollectionInternal>> insertCollectionFunc = (CollectionInternal collectionToInsert) =>
                      CollectionManager.Instance.Add(contextData, collectionToInsert);

                result = ClientUtils.GetResponseFromWS<KalturaCollection, CollectionInternal>(collection, insertCollectionFunc);

                if (result != null)
                {
                    Func<GenericResponse<Collection>> getFunc = () =>
                               CollectionManager.Instance.GetCollection(contextData.GroupId, long.Parse(result.Id), true);

                    result = ClientUtils.GetResponseFromWS<KalturaCollection, Collection>(getFunc);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Delete collection 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Collection id</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CollectionNotExist)]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        static public bool Delete(long id)
        {
            bool result = false;

            var contextData = KS.GetContextData();

            try
            {
                Status delete() => CollectionManager.Instance.Delete(contextData, id);

                result = ClientUtils.GetResponseStatusFromWS(delete);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Update Collection
        /// </summary>
        /// <param name="id">Collection id</param>
        /// <param name="collection">Collection</param>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ExternalIdAlreadyExists)]
        [Throws(eResponseStatus.UsageModuleDoesNotExist)]
        [Throws(eResponseStatus.PriceDetailsDoesNotExist)]
        [Throws(eResponseStatus.ChannelDoesNotExist)]
        [Throws(eResponseStatus.CouponGroupNotExist)]
        [Throws(eResponseStatus.InvalidDiscountCode)]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        [Throws(eResponseStatus.StartDateShouldBeLessThanEndDate)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.InvalidFileTypes)]
        [SchemeArgument("id", MinLong = 1)]
        static public KalturaCollection Update(long id, KalturaCollection collection)
        {
            KalturaCollection result = null;

            var contextData = KS.GetContextData();
            collection.ValidateForUpdate();

            try
            {
                collection.Id = id.ToString();

                Func<CollectionInternal, GenericResponse<CollectionInternal>> updateCollectionFunc = (CollectionInternal collectionToInsert) =>
                  CollectionManager.Instance.Update(contextData, collectionToInsert);

                result = ClientUtils.GetResponseFromWS<KalturaCollection, CollectionInternal>(collection, updateCollectionFunc);

                if (result != null)
                {

                    Func<GenericResponse<Collection>> getFunc = () =>
                               CollectionManager.Instance.GetCollection(contextData.GroupId, long.Parse(result.Id), true);

                    result = ClientUtils.GetResponseFromWS<KalturaCollection, Collection>(getFunc);
                }

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }
    }
}