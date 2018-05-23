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
    public class Roles
    {
        [JsonProperty()]
        public List<SlimRole> roles = null;

        public Roles()
        {
            roles = new List<SlimRole>();
        }
    }

    [JsonObject()]
    public class SlimRole : Role
    {

        [JsonProperty("permissions")]
        public List<string> permissionsNames;

        public SlimRole(Role original)
        {
            base.Id = original.Id;
            base.GroupId = original.GroupId;
            base.Name = original.Name;
            base.Permissions = original.Permissions;

            if (this.Permissions != null)
            {
                this.permissionsNames = this.Permissions.Select(p => p.Name).ToList();
            }
        }
    }
}
