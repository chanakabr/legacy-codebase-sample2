using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Response;

namespace ApiObjects
{
    public class RegionsResponse
    {
        public List<Region> Regions { get; set; }

        public Status Status { get; set; }
    }
}
