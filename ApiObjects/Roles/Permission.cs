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
}
