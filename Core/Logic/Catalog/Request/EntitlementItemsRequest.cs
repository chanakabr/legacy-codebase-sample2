using ApiObjects;
using System.Collections.Generic;

namespace ApiLogic.Catalog.Request
{
    public class EntitlementItemsRequest
    {
        public int GroupId;
        public List<int> lUsersIDs;
        public bool isExpired;
        public int domainID;
        public bool shouldCheckByDomain;
        public int pageSize;
        public int pageIndex;
        public EntitlementOrderBy orderBy;
        public long? shopUserId;

        public EntitlementItemsRequest()
        {
            domainID = 0;
            shouldCheckByDomain = true;
            pageSize = 500;
            pageIndex = 0;
            orderBy = EntitlementOrderBy.PurchaseDateAsc;
            shopUserId = null;
        }

        public EntitlementItemsRequest(int groupId,  List<int> lUsersIDs, bool isExpired, int domainID = 0, bool shouldCheckByDomain = true, int pageSize = 500, int pageIndex = 0, EntitlementOrderBy orderBy = EntitlementOrderBy.PurchaseDateAsc, long? shopUserId = null)
            : base()
        {
            GroupId = groupId;
            this.lUsersIDs = lUsersIDs;
            this.isExpired = isExpired;
            this.domainID = domainID;
            this.shouldCheckByDomain = shouldCheckByDomain;
            this.pageSize = pageSize;
            this.pageIndex = pageIndex;
            this.orderBy = orderBy;
            this.shopUserId = shopUserId;
        }
    }
}
