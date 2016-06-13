using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.SearchObjects
{
    /// <summary>
    /// Definitions of search by entitlement
    /// </summary>
    [Serializable]
    [DataContract]
    public class EntitlementSearchDefinitions
    {
        /// <summary>
        /// Should the search include free assets or not
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public bool shouldGetFreeAssets;

        /// <summary>
        /// Should the search include explicitly entitled (purchased, subscriptions) assets or not
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public bool shouldGetPurchasedAssets;

        /// <summary>
        /// List of free assets according to their type - will be filled if searching by entitlements
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public Dictionary<eAssetTypes, List<string>> freeAssets;

        /// <summary>
        /// List of assets that the user already paid for - will be filled if searching by entitlements
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public Dictionary<eAssetTypes, List<string>> entitledPaidForAssets;

        /// <summary>
        /// List of search objects that represent queries of subscriptions the user is entitled to
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public List<BaseSearchObject> subscriptionSearchObjects;

        /// <summary>
        /// Required file type of assets - this is relevant for free assets
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public List<int> fileTypes;

        /// <summary>
        /// List of epg channel IDs that the user is entitled to watch (either free or paid for)
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public List<int> epgChannelIds;
    }

    public enum eEntitlementSearchType
    {
        None,
        Free,
        Entitled,
        Both
    }
}
