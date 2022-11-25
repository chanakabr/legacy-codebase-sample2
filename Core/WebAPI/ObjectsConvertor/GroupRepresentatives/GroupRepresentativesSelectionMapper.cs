using System;
using System.Threading;
using ApiObjects.SearchObjects;
using ApiObjects.SearchObjects.GroupRepresentatives;
using WebAPI.Models.Catalog.GroupRepresentatives;
using WebAPI.ObjectsConvertor.Ordering;

namespace WebAPI.ObjectsConvertor.GroupRepresentatives
{
    public class GroupRepresentativesSelectionMapper : IGroupRepresentativesSelectionMapper
    {
        private static readonly Lazy<IGroupRepresentativesSelectionMapper> LazyInstance = new Lazy<IGroupRepresentativesSelectionMapper>(
            () => new GroupRepresentativesSelectionMapper(KalturaOrderMapper.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IGroupRepresentativesSelectionMapper Instance => LazyInstance.Value;

        private readonly IKalturaOrderMapper _orderMapper;

        public GroupRepresentativesSelectionMapper(IKalturaOrderMapper orderMapper)
        {
            _orderMapper = orderMapper;
        }

        public UnmatchedItemsPolicy MapToUnmatchedItemsPolicy(KalturaUnmatchedItemsPolicy? policy)
        {
            switch (policy)
            {
                case KalturaUnmatchedItemsPolicy.GROUP:
                    return UnmatchedItemsPolicy.Group;
                case KalturaUnmatchedItemsPolicy.INCLUDE_AND_MERGE:
                    return UnmatchedItemsPolicy.IncludeAndMerge;
                default:
                    return UnmatchedItemsPolicy.Omit;
            }
        }

        public RepresentativeSelectionPolicy MapToRepresentativeSelectionPolicy(KalturaRepresentativeSelectionPolicy policy)
        {
            var defaultPolicy = new TopAssetRsp
            {
                OrderingParameters = new[] { new AssetOrder { Field = OrderBy.CREATE_DATE, Direction = OrderDir.DESC } }
            };

            if (policy == null)
            {
                return defaultPolicy;
            }

            if (policy is KalturaTopSubscriptionEntitledRsp topEntitledRsp)
            {
                return new TopSubscriptionEntitledRsp
                {
                    OrderingParameters = _orderMapper.MapParameters(new[] { topEntitledRsp.OrderBy }, OrderBy.CREATE_DATE)
                };
            }

            if (policy is KalturaTopRsp topRsp)
            {
                return new TopAssetRsp
                {
                    OrderingParameters = _orderMapper.MapParameters(new[] { topRsp.OrderBy }, OrderBy.CREATE_DATE)
                };
            }

            return defaultPolicy;
        }
    }
}