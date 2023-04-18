using ApiObjects;
using ApiObjects.Pricing;
using Core.Pricing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using APILogic;
using MoreLinq;

namespace Core.ConditionalAccess
{
    [Serializable]
    public class DomainEntitlementsCache
    {
        [JsonProperty("DomainPpvEntitlements")]
        internal PPVEntitlements DomainPpvEntitlements { get; set; }
        [JsonProperty("DomainBundleEntitlements")]
        internal BundleEntitlements DomainBundleEntitlements { get; set; }
        [JsonProperty("PagoEntitlements")]
        public Dictionary<long, PagoEntitlement> PagoEntitlements { get; set; }

        /// <summary>
        /// represents all the domain entitlements
        /// </summary>        
        public DomainEntitlementsCache()
        {
            DomainPpvEntitlements = new PPVEntitlements();
            DomainBundleEntitlements = new BundleEntitlements();
            PagoEntitlements = new Dictionary<long, PagoEntitlement>();
        }


        /// <summary>
        ///  represents the domain ppv entitlements object
        /// </summary>
        [Serializable]
        internal class PPVEntitlements
        {
            [JsonProperty("EntitlementsDictionary")]
            public Dictionary<string, EntitlementObject> EntitlementsDictionary { get; set; }
            public PPVEntitlements()
            {
                EntitlementsDictionary = new Dictionary<string,EntitlementObject>();
            }
            public PPVEntitlements(PPVEntitlements other)
            {
                EntitlementsDictionary = other.EntitlementsDictionary?.ToDictionary(x => x.Key,
                    x => Extensions.Clone(x.Value)); 
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

            public BundleEntitlements()
            {
                EntitledSubscriptions = new Dictionary<string,Utils.UserBundlePurchase>();
                EntitledCollections = new Dictionary<string,Utils.UserBundlePurchase>();
            }
        }
    }
}