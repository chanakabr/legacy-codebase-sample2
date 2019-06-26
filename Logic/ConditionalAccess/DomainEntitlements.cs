using ApiObjects;
using Core.Pricing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    [Serializable]
    public class DomainEntitlements
    {

        [JsonProperty("DomainPpvEntitlements")]
        internal PPVEntitlements DomainPpvEntitlements { get; set; }
        [JsonProperty("DomainBundleEntitlements")]
        internal BundleEntitlements DomainBundleEntitlements { get; set; }

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
        }


        /// <summary>
        ///  represents the domain ppv entitlements object
        /// </summary>
        [Serializable]
        internal class PPVEntitlements
        {
            [JsonProperty("EntitlementsDictionary")]
            public Dictionary<string, EntitlementObject> EntitlementsDictionary { get; set; }
            
            [JsonIgnore]
            public Dictionary<string, List<int>> MediaIdGroupFileTypeMapper { get; set; }
            
            [JsonIgnore]
            public Dictionary<int, HashSet<int>> MediaIdToMediaFiles { get; set; }  

            public PPVEntitlements()
            {
                EntitlementsDictionary = new Dictionary<string,EntitlementObject>();
                MediaIdGroupFileTypeMapper = new Dictionary<string, List<int>>();
                MediaIdToMediaFiles = new Dictionary<int, HashSet<int>>();
            }

        }


        /// <summary>
        /// represents the domain bundle (subscriptions and collections) entitlements object
        /// </summary>
        [Serializable]
        internal class BundleEntitlements
        {
            [JsonProperty("EntitledSubscriptions")]
            public Dictionary<string, ConditionalAccess.Utils.UserBundlePurchase> EntitledSubscriptions { get; set; }
            [JsonProperty("EntitledCollections")]
            public Dictionary<string, ConditionalAccess.Utils.UserBundlePurchase> EntitledCollections { get; set; }

            [JsonIgnore]
            public Dictionary<int, List<Subscription>> FileTypeIdToSubscriptionMappings { get; set; }
            [JsonIgnore]
            public Dictionary<int, List<Subscription>> ChannelsToSubscriptionMappings { get; set; }
            [JsonIgnore]
            public Dictionary<int, List<Collection>> ChannelsToCollectionsMappings { get; set; }
            [JsonIgnore]
            public Dictionary<int, Subscription> SubscriptionsData { get; set; }
            [JsonIgnore]
            public Dictionary<int, Collection> CollectionsData { get; set; }

            public BundleEntitlements()
            {
                EntitledSubscriptions = new Dictionary<string,Utils.UserBundlePurchase>();
                EntitledCollections = new Dictionary<string,Utils.UserBundlePurchase>();
                FileTypeIdToSubscriptionMappings = new Dictionary<int,List<Subscription>>();
                ChannelsToSubscriptionMappings = new Dictionary<int,List<Subscription>>();
                ChannelsToCollectionsMappings = new Dictionary<int,List<Collection>>();
                SubscriptionsData = new Dictionary<int,Subscription>();
                CollectionsData = new Dictionary<int,Collection>();                
            }
        }
    }

    [Serializable]
    public class DomainBundles
    {
        [JsonProperty("EntitledSubscriptions")]
        public Dictionary<string, List<ConditionalAccess.Utils.UserBundlePurchaseWithSuspend>> EntitledSubscriptions { get; set; }
        [JsonProperty("EntitledCollections")]
        public Dictionary<string, List<ConditionalAccess.Utils.UserBundlePurchase>> EntitledCollections { get; set; }

        public DomainBundles()
        {
            EntitledSubscriptions = new Dictionary<string, List<Utils.UserBundlePurchaseWithSuspend>>();
            EntitledCollections = new Dictionary<string, List<Utils.UserBundlePurchase>>();
        }
    }
}