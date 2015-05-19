using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace Tvinci.Data.Loaders
{
    public class CacheKey
    {
        public string ID { get; set; }
        public DateTime UpdateDate { get; set; }

        public CacheKey()
        {
        }

        public CacheKey(string id, DateTime updateDate)
        {
            ID = id;
            UpdateDate = updateDate;
        }
    }
}
