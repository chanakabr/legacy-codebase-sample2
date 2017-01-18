using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    public class Role
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<Permission> Permissions { get; set; }
        public int GroupId { get; set; }
    }

    public class RolesResponse
    {
        public List<Role> Roles { get; set; }
        public ApiObjects.Response.Status Status { get; set; }
    }
}
