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
    public class Permissions
    {
        [JsonProperty()]
        public List<GroupPermission> permissions;

        public Permissions()
        {
            permissions = new List<GroupPermission>();
        }
    }
}
