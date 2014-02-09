using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPWebApi.Models
{
    public class FavoriteRequest
    {
        public int media_id { get; set; }
        public int media_type { get; set; }
        public int extra_val { get; set; }
    }
}