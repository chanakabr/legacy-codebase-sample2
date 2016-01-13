using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.SearchObjects
{
    /// <summary>
    /// Definitions of search by entitlement
    /// </summary>
    public class EntitlementSearchDefinitions
    {
        /// <summary>
        /// List of free assets according to their type - will be filled if searching by entitlements
        /// </summary>
        public Dictionary<eAssetTypes, List<string>> freeAssets;

        /// <summary>
        /// List of assets that the user already paid for - will be filled if searching by entitlements
        /// </summary>
        public Dictionary<eAssetTypes, List<string>> entitledPaidForAssets;

        /// <summary>
        /// List of search objects that represent queries of subscriptions the user is entitled to
        /// </summary>
        public List<BaseSearchObject> subscriptionSearchObjects;

        /// <summary>
        /// Required file type of assets - this is relevant for free assets
        /// </summary>
        public int fileType;

    }
}
