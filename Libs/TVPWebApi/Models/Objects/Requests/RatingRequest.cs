using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPWebApi.Models.Objects
{
    public class RatingRequest : BaseRequest
    {
        public int media_type { get; set; }
        public int extra_val { get; set; }
    }
}