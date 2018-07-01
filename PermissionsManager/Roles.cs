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
        public List<FileRole> roles = null;

        public Roles()
        {
            roles = new List<FileRole>();
        }
    }

    [JsonObject()]
    public class FileRole
    {
        [JsonIgnore()]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonIgnore()]
        public List<Permission> Permissions { get; set; }

        [JsonProperty("group_id")]
        [JsonIgnore()]
        public int GroupId { get; set; }

        [JsonProperty("permissions")]
        public List<string> permissionsNames;

        [JsonProperty("excluded_permissions")]
        public List<string> excludedPermissionsNames;

        public FileRole()
        {

        }

        public FileRole(Role original)
        {
            if (original != null)
            {
                this.Id = original.Id;
                this.GroupId = original.GroupId;
                this.Name = original.Name;
                this.Permissions = original.Permissions;

                if (this.Permissions != null)
                {
                    this.permissionsNames = this.Permissions.Where(p => !p.isExcluded).Select(p => p.Name).ToList();
                    this.excludedPermissionsNames = this.Permissions.Where(p => p.isExcluded).Select(p => p.Name).ToList();
                }
            }
        }
    }
}
