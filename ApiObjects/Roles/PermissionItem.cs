using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    [JsonObject()]
    public class PermissionItem
    {
        //[JsonIgnore()]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("is_excluded")]
        public bool IsExcluded { get; set; }

        public virtual string GetPermissionItemType()
        {
            return string.Empty;
        }

        public virtual string GetFileName()
        {
            return string.Empty;
        }
    }
}
