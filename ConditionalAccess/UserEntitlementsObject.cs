using ApiObjects;
using Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{

    public class UserEntitlementsObject
    {

        internal PPVEntitlements userPpvEntitlements { get; set; }
        internal BundleEntitlements userBundleEntitlements { get; set; }

        /// <summary>
        /// represents all the user entitlements
        /// </summary>
        public UserEntitlementsObject()
        {
            userPpvEntitlements = new PPVEntitlements();
            userBundleEntitlements = new BundleEntitlements();
        }

        /// <summary>
        ///  represents the user ppv entitlements object
        /// </summary>
        internal class PPVEntitlements
        {

            public Dictionary<string, EntitlementObject> EntitlementsDictionary { get; set; }
            public Dictionary<string, int> MediaIdGroupFileTypeMapper { get; set; }            

            public PPVEntitlements()
            {
                EntitlementsDictionary = null;
                MediaIdGroupFileTypeMapper = null;
            }

        }


        /// <summary>
        /// represents the user bundle (subscriptions and collections) entitlements object
        /// </summary>
        internal class BundleEntitlements
        {

            public Dictionary<string, ConditionalAccess.Utils.UserBundlePurchase> EntitledSubscriptions { get; set; }
            public Dictionary<string, ConditionalAccess.Utils.UserBundlePurchase> EntitledCollections { get; set; }
            public Dictionary<int, List<Subscription>> FileTypeIdToSubscriptionMappings  { get; set; }
            public Dictionary<int, List<Subscription>> ChannelsToSubscriptionMappings { get; set; }
            public Dictionary<int, List<Collection>> ChannelsToCollectionsMappings { get; set; }
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