using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class Item
    {
        public ItemType ItemType { get; set; }

        public int ItemId { get; set; }

        public int? OrderIndex { get; set; }

        public string UserId { get; set; }
    }

    public class UserItemsList
    {
        public List<Item> ItemsList { get; set; }

        public ListType ListType { get; set; }
    }

    public class UsersItemsListsResponse
    {
        public List<UserItemsList> UsersItemsLists { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}