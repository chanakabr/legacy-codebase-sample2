using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for DynamicData
/// </summary>
/// 

namespace TVPApi
{
    //Holds dynamic data of objects (such as is in favorite, purchase status etc..)
    public class DynamicData
    {
        public bool IsFavorite { get; set; }
        public string Price { get; set; }
        public int MediaMark { get; set; }
        public PriceReason PriceType { get; set; }
        public bool Notification { get; set; }
        public DateTime ExpirationDate { get; set; }

        public DynamicData()
        {

        }
    }
}
