using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace ApiObjects
{
    public class RegionFilter
    {
        public List<int> RegionIds { get; set; }

        public List<string> ExternalIds { get; set; }

        public int ParentId { get; set; }

        public int LiveAssetId { get; set; }

        public RegionOrderBy orderBy { get; set; }
    }
}
