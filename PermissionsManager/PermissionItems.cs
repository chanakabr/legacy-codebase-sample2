using ApiObjects.Roles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionsManager
{
    [JsonObject()]
    public class PermissionItems
    {
        [JsonProperty("name")]
        public string name;

        [JsonProperty("type")]
        public string type;

        [JsonProperty("permission_items")]
        public List<SlimPermissionItem> permissionItems;

        public PermissionItems()
        {
            permissionItems = new List<SlimPermissionItem>();
        }
    }

    [JsonObject()]
    public class SlimPermissionItem
    {
        [JsonProperty("permissions")]
        public HashSet<string> permissions;

        [JsonProperty("permission_item")]
        public PermissionItem permissionItem;

        public SlimPermissionItem(PermissionItem original)
        {
            this.permissionItem = original;
            permissions = new HashSet<string>();
        }
    }
}
