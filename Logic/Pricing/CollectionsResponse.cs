using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    public class CollectionsResponse
    {
        public Collection[] Collections { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
