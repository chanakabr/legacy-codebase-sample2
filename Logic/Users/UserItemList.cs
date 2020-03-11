using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class ItemObj
    {
        public int item { get; set; }
        public int? orderNum { get; set; }
    }

    public class UserItemList
    {
        public string siteGuid { get; set; }
        public List<ItemObj> itemObj { get; set; }
        public ListType listType { get; set; }
        public ListItemType itemType { get; set; }
    }

    public class UserItemListsResponse
    {
        public List<UserItemList> UserItemLists { get; set; }
        public ApiObjects.Response.Status Status { get; set; }

    }
}
