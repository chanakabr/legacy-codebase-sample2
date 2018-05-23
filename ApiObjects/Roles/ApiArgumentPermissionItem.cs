using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    [JsonObject()]
    public class ApiArgumentPermissionItem : PermissionItem
    {
        [JsonProperty("service")]
        public string Service { get; set; }
        [JsonProperty("action")]
        public string Action { get; set; }
        [JsonProperty("parameter")]
        public string Parameter { get; set; }

        public override string GetPermissionItemType()
        {
            return "controller";
        }

        public override string GetFileName()
        {
            if (string.IsNullOrEmpty(this.Service))
            {
                return string.Empty;
            }
            else
            {
                return string.Format("permission_item_controller_{0}", this.Service);
            }
        }
    }
}
