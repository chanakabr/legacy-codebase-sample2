using ApiObjects;
using Newtonsoft.Json;
using Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    [Serializable]
    public class DomainEntitlements
    {

        [JsonProperty("DomainPpvEntitlements")]
        internal PPVEntitlements DomainPpvEntitlements { get; set; }
        [JsonProperty("DomainBundleEntitlements")]
        internal BundleEntitlements DomainBundleEntitlements { get; set; }

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
            [JsonProperty("MediaIdGroupFileTypeMapper")]
            public Dictionary<string, int> MediaIdGroupFileTypeMapper { get; set; }            

            public PPVEntitlements()
            {
                EntitlementsDictionary = new Dictionary<string,EntitlementObject>();
                MediaIdGroupFileTypeMapper = new Dictionary<string,int>();
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
            [JsonIgnore]
            public Dictionary<int, List<Subscription>> FileTypeIdToSubscriptionMappings { get; set; }
            [JsonIgnore]
            public Dictionary<int, List<Subscription>> ChannelsToSubscriptionMappings { get; set; }
            [JsonIgnore]
            public Dictionary<int, List<Collection>> ChannelsToCollectionsMappings { get; set; }
            [JsonProperty("EntitledCollections")]
            public Dictionary<string, ConditionalAccess.Utils.UserBundlePurchase> EntitledCollections { get; set; }
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
}