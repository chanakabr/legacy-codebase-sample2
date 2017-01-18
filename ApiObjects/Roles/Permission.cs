using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    public class Permission
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<PermissionItem> PermissionItems { get; set; }
        public int GroupId { get; set; }
    }

    public class PermissionsResponse
    {
        public List<Permission> Permissions { get; set; }
        public ApiObjects.Response.Status Status { get; set; }
    }

    public class PermissionResponse
    {
        public Permission Permission { get; set; }
        public ApiObjects.Response.Status Status { get; set; }
    }
}
