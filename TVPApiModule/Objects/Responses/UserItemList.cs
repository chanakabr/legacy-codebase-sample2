using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserItemList
    {
        public string siteGuid { get; set; }

        public ItemObj[] itemObj { get; set; }

        public ListType listType { get; set; }

        public ItemType itemType { get; set; }
    }
}
