using ApiObjects;
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
        public List<FilePermission> permissions;

        public Permissions()
        {
            permissions = new List<FilePermission>();
        }
    }

    public class FilePermission
    {
        [JsonIgnore()]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonIgnore()]
        public List<PermissionItem> PermissionItems { get; set; }

        [JsonProperty("group_id")]
        [JsonIgnore()]
        public int GroupId { get; set; }
        
        [JsonProperty("users_group")]
        public string UsersGroup { get; set; }

        [JsonProperty("friendly_name")]
        public string FriendlyName { get; set; }

        [JsonProperty("depends_on_permission_names")]
        public string DependsOnPermissionNames { get; set; }

        [JsonProperty("type")]
        public ePermissionType Type { get; set; }

        public FilePermission(GroupPermission original)
        {
            if (original != null)
            {
                this.Id = original.Id;
                this.Name = original.Name;
                this.PermissionItems = original.PermissionItems;
                this.GroupId = original.GroupId;
                this.UsersGroup = original.UsersGroup;
                this.FriendlyName = original.FriendlyName;
                this.DependsOnPermissionNames = original.DependsOnPermissionNames;
                this.Type = original.Type;
            }
        }
    }
}
