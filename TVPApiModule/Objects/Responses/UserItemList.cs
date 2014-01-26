using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserItemList
    {
        public string site_guid { get; set; }

        public ItemObj[] item_obj { get; set; }

        public ListType list_type { get; set; }

        public ItemType item_type { get; set; }
    }
}
