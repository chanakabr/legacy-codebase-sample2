using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class UserItemListDTO
    {
        public string siteGuid { get; set; }

        public ItemObjDTO[] itemObj { get; set; }

        public ListTypeDTO listType { get; set; }

        public ItemTypeDTO itemType { get; set; }
    }

    public class ItemObjDTO
    {
        public int item { get; set; }

        public int? orderNum { get; set; }
    }

    public enum ListTypeDTO
    {
        /// <remarks/>
        All,
        /// <remarks/>
        Watch,
        /// <remarks/>
        Purchase,
    }

    public enum ItemTypeDTO
    {
        /// <remarks/>
        All,
        /// <remarks/>
        Media,
    }
}