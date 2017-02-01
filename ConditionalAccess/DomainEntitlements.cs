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
                EntitlementsDictionary = null;
                MediaIdGroupFileTypeMapper = null;
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
            [JsonProperty("FileTypeIdToSubscriptionMappings")]
            public Dictionary<int, List<Subscription>> FileTypeIdToSubscriptionMappings { get; set; }
            [JsonProperty("ChannelsToSubscriptionMappings")]
            public Dictionary<int, List<Subscription>> ChannelsToSubscriptionMappings { get; set; }
            [JsonProperty("ChannelsToCollectionsMappings")]
            public Dictionary<int, List<Collection>> ChannelsToCollectionsMappings { get; set; }       
            public Dictionary<string, ConditionalAccess.Utils.UserBundlePurchase> EntitledCollections { get; set; }     
            public Dictionary<int, Subscription> SubscriptionsData { get; set; }            
            public Dictionary<int, Collection> CollectionsData { get; set; }

            public BundleEntitlements()
            {
                EntitledSubscriptions = null;
                EntitledCollections = null;
                FileTypeIdToSubscriptionMappings = null;
                ChannelsToSubscriptionMappings = null;
                ChannelsToCollectionsMappings = null;
                SubscriptionsData = null;
                CollectionsData = null;                
            }
        }


    }
}