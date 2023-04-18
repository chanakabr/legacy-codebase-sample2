using ApiObjects;
using ApiObjects.Pricing;
using Core.Pricing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using APILogic;
using Google.Protobuf;

namespace Core.ConditionalAccess
{
    [Serializable]
    public class DomainEntitlements : DomainEntitlementsCache
    {
        [JsonProperty("DomainPpvEntitlements")]
        internal PPVEntitlements DomainPpvEntitlements { get; set; }

        [JsonProperty("DomainBundleEntitlements")]
        internal BundleEntitlements DomainBundleEntitlements { get; set; }

        [JsonProperty("PagoEntitlements")] public Dictionary<long, PagoEntitlement> PagoEntitlements { get; set; }

        public List<int> DomainSubscriptionsIds
        {
            get
            {
                if (DomainBundleEntitlements != null &&
                    DomainBundleEntitlements.SubscriptionsData != null &&
                    DomainBundleEntitlements.SubscriptionsData.Count > 0)
                {
                    return new List<int>(DomainBundleEntitlements.SubscriptionsData.Keys);
                }

                return null;
            }
        }

        /// <summary>
        /// represents all the domain entitlements
        /// </summary>        
        public DomainEntitlements()
        {
            DomainPpvEntitlements = new PPVEntitlements();
            DomainBundleEntitlements = new BundleEntitlements();
            PagoEntitlements = new Dictionary<long, PagoEntitlement>();
        }

        /// <summary>
        /// represents all the domain entitlements
        /// </summary>        
        public DomainEntitlements(DomainEntitlementsCache entitlementsCache)
        {
            DomainPpvEntitlements = new PPVEntitlements(entitlementsCache.DomainPpvEntitlements);
            DomainBundleEntitlements = new BundleEntitlements(entitlementsCache.DomainBundleEntitlements);
            PagoEntitlements = entitlementsCache.PagoEntitlements;
        }


        /// <summary>
        ///  represents the domain ppv entitlements object
        /// </summary>
        [Serializable]
        internal new class PPVEntitlements : DomainEntitlementsCache.PPVEntitlements
        {
            [JsonIgnore] public Dictionary<string, List<int>> MediaIdGroupFileTypeMapper { get; set; }

            [JsonIgnore] public Dictionary<int, HashSet<int>> MediaIdToMediaFiles { get; set; }

            public PPVEntitlements()
            {
                EntitlementsDictionary = new Dictionary<string, EntitlementObject>();
                MediaIdGroupFileTypeMapper = new Dictionary<string, List<int>>();
                MediaIdToMediaFiles = new Dictionary<int, HashSet<int>>();
            }

            public PPVEntitlements(DomainEntitlementsCache.PPVEntitlements entitlements)
            {
                EntitlementsDictionary = entitlements.EntitlementsDictionary?.ToDictionary(x => x.Key,
                    x => Extensions.Clone(x.Value));
                MediaIdGroupFileTypeMapper = new Dictionary<string, List<int>>();
                MediaIdToMediaFiles = new Dictionary<int, HashSet<int>>();
            }
        }


        /// <summary>
        /// represents the domain bundle (subscriptions and collections) entitlements object
        /// </summary>
        [Serializable]
        internal new class BundleEntitlements : DomainEntitlementsCache.BundleEntitlements
        {
            [JsonIgnore] public Dictionary<int, List<Subscription>> FileTypeIdToSubscriptionMappings { get; set; }
            [JsonIgnore] public Dictionary<int, List<Subscription>> ChannelsToSubscriptionMappings { get; set; }
            [JsonIgnore] public Dictionary<int, List<Collection>> ChannelsToCollectionsMappings { get; set; }
            [JsonIgnore] public Dictionary<int, Subscription> SubscriptionsData { get; set; }
            [JsonIgnore] public Dictionary<int, Collection> CollectionsData { get; set; }
            [JsonIgnore] public Dictionary<int, ProgramAssetGroupOffer> ProgramAssetGroupOffersData { get; set; }
            [JsonIgnore] public Dictionary<int, List<Collection>> FileTypeIdToCollectionMappings { get; set; }

            public BundleEntitlements()
            {
                EntitledSubscriptions = new Dictionary<string, Utils.UserBundlePurchase>();
                EntitledCollections = new Dictionary<string, Utils.UserBundlePurchase>();
                FileTypeIdToSubscriptionMappings = new Dictionary<int, List<Subscription>>();
                ChannelsToSubscriptionMappings = new Dictionary<int, List<Subscription>>();
                ChannelsToCollectionsMappings = new Dictionary<int, List<Collection>>();
                SubscriptionsData = new Dictionary<int, Subscription>();
                CollectionsData = new Dictionary<int, Collection>();
                ProgramAssetGroupOffersData = new Dictionary<int, ProgramAssetGroupOffer>();
                FileTypeIdToCollectionMappings = new Dictionary<int, List<Collection>>();
            }

            public BundleEntitlements(DomainEntitlementsCache.BundleEntitlements entitlement)
            {
                EntitledSubscriptions = entitlement.EntitledSubscriptions?.ToDictionary(x => x.Key,
                    x => Extensions.Clone(x.Value));
                EntitledCollections = entitlement.EntitledCollections?.ToDictionary(x => x.Key,
                    x => Extensions.Clone(x.Value));;
                FileTypeIdToSubscriptionMappings = new Dictionary<int, List<Subscription>>();
                ChannelsToSubscriptionMappings = new Dictionary<int, List<Subscription>>();
                ChannelsToCollectionsMappings = new Dictionary<int, List<Collection>>();
                SubscriptionsData = new Dictionary<int, Subscription>();
                CollectionsData = new Dictionary<int, Collection>();
                ProgramAssetGroupOffersData = new Dictionary<int, ProgramAssetGroupOffer>();
                FileTypeIdToCollectionMappings = new Dictionary<int, List<Collection>>();
            }
        }
    }

    [Serializable]
    public class DomainBundles : IDeepCloneable<DomainBundles>
    {
        [JsonProperty("EntitledSubscriptions")]
        public Dictionary<string, List<ConditionalAccess.Utils.UserBundlePurchaseWithSuspend>> EntitledSubscriptions
        {
            get;
            set;
        }

        [JsonProperty("EntitledCollections")]
        public Dictionary<string, List<ConditionalAccess.Utils.UserBundlePurchase>> EntitledCollections { get; set; }

        public DomainBundles()
        {
            EntitledSubscriptions = new Dictionary<string, List<Utils.UserBundlePurchaseWithSuspend>>();
            EntitledCollections = new Dictionary<string, List<Utils.UserBundlePurchase>>();
        }

        public DomainBundles(DomainBundles other)
        {
            EntitledSubscriptions = other.EntitledSubscriptions?.ToDictionary(x => x.Key,
                x => Extensions.Clone(x.Value));
            EntitledCollections = other.EntitledCollections?.ToDictionary(x => x.Key,
                x => Extensions.Clone(x.Value));
        }

        public DomainBundles Clone()
        {
            return new DomainBundles(this);
        }
    }
}