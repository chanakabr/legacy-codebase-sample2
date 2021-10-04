using ApiObjects.Base;
using ApiObjects.Pricing.Dto;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Pricing;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Pricing.Handlers
{
    public class CollectionManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<CollectionManager> lazy = new Lazy<CollectionManager>(() => new CollectionManager(PricingDAL.Instance), LazyThreadSafetyMode.PublicationOnly);

        private readonly ICollectionRepository _repository;

        public static CollectionManager Instance => lazy.Value;

        public CollectionManager(ICollectionRepository repository)
        {
            _repository = repository;
        }

        public GenericResponse<Collection> Add(ContextData contextData, Collection collection)
        {
            var response = new GenericResponse<Collection>();
            List<long> channelIds = collection.m_sCodes.Select(t => long.Parse(t.m_sCode)).ToList();

            List<SubscriptionCouponGroupDTO> couponsGroups = ConvertToDtos(collection.CouponsGroups);

            long id = _repository.Insert_Collection(contextData.GroupId, collection.m_oCollectionPriceCode.m_nObjectID,
                collection.m_oDiscountModule.m_nObjectID, collection.m_oCollectionUsageModule.m_nObjectID, collection.m_dStartDate, collection.m_dEndDate,
                collection.CouponsGroups.Count > 0 ? collection.CouponsGroups[0].m_sGroupCode : "0",
                contextData.UserId.Value, collection.m_sDescription, collection.m_sName, channelIds, couponsGroups, collection.ExternalProductCodes);

            if (id == 0)
            {
                log.Error($"Error while ADD Collection. contextData: {contextData.ToString()}.");
                return response;
            }

            SetCollectionInvalidation(contextData.GroupId, id);
            collection.m_CollectionCode = id.ToString();
            response.Object = collection;
            response.Status.Set(eResponseStatus.OK);

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            try
            {
                if (!_repository.IsCollectionExists(contextData.GroupId, id))
                {
                    result.Set(eResponseStatus.CollectionNotExist, $"Collection {id} does not exist");
                    return result;
                }

                if (!_repository.DeleteCollection(contextData.GroupId, id, contextData.UserId.Value))
                {
                    log.Error($"Error while Collection. contextData: {contextData.ToString()}.");
                    result.Set(eResponseStatus.Error);
                    return result;
                }

                SetCollectionInvalidation(contextData.GroupId, id);
                result.Set(eResponseStatus.OK);
            }

            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in  delete Collection. contextData:{contextData.ToString()}, id:{id}.", ex);
            }

            return result;
        }

        private static List<SubscriptionCouponGroupDTO> ConvertToDtos(List<SubscriptionCouponGroup> subscriptionCouponGroups)
        {
            return subscriptionCouponGroups.Select(s => new SubscriptionCouponGroupDTO(s.m_sGroupCode,
                s.startDate, s.endDate)).ToList();
        }

        private void SetCollectionInvalidation(int groupId, long collectionId)
        {
            // invalidation keys
            string invalidationKey = LayeredCacheKeys.GetCollectionsIdsInvalidationKey(groupId);
            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set invalidation key for group Collection. key = {0}", invalidationKey);
            }
        }

        public GenericListResponse<Collection> GetCollectionsData(int nGroupID, string[] oCollCodes, string sCountryCd2, string sLanguageCode3, string sDeviceName, int pageIndex = 0, int pageSize = 30, bool shouldIgnorePaging = true, int? couponGroupIdEqual = null)
        {
            GenericListResponse<Collection> response = new GenericListResponse<Collection>();

            BaseCollection t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                if (!shouldIgnorePaging)
                {
                    int startIndexOnList = pageIndex * pageSize;
                    int rangeToGetFromList = (startIndexOnList + pageSize) > oCollCodes.Length ? (oCollCodes.Length - startIndexOnList) > 0 ? (oCollCodes.Length - startIndexOnList) : 0 : pageSize;
                    if (rangeToGetFromList > 0)
                    {
                        oCollCodes = oCollCodes.Skip(startIndexOnList).Take(rangeToGetFromList).ToArray();
                    }
                }

                response.Objects = (new CollectionCacheWrapper(t)).GetCollectionsData(oCollCodes, sCountryCd2, sLanguageCode3, sDeviceName, couponGroupIdEqual).Collections.ToList();
                response.TotalItems = response.Objects != null ? response.Objects.Count : 0;
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        public GenericListResponse<Collection> GetCollectionsData(int groupId, string country, string language, string udid, int pageIndex, int pageSize, bool shouldIgnorePaging, int? couponGroupIdEqual = null)
        {
            // get group's CollectionIds
            HashSet<long> collCodes = PricingCache.GetCollectionsIds(groupId);

            if (collCodes == null)
            {
                return null;
            }

            return GetCollectionsData(groupId, collCodes.Select(x => x.ToString()).ToArray(), country, language, udid, pageIndex, pageSize, shouldIgnorePaging, couponGroupIdEqual);
        }

        public IdsResponse GetCollectionIdsContainingMediaFile(int groupId, int mediaId, int mediaFileID)
        {
            BaseCollection t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                return (new CollectionCacheWrapper(t)).GetCollectionIdsContainingMediaFile(mediaId, mediaFileID);
            }

            return null;
        }
    }
}