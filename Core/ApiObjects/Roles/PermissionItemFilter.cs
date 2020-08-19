using ApiObjects.Base;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Text;


namespace ApiObjects.Roles
{
    public class PermissionItemFilter
    {
    }

    public class PermissionItemByIdInFilter : PermissionItemFilter
    {
        public List<long> IdIn { get; set; }
    }

    public class PermissionItemByApiActionFilter : PermissionItemFilter
    {
        public string Service { get; set; }

        public string Action { get; set; }
    }

    public class PermissionItemByArgumentFilter : PermissionItemByApiActionFilter
    {
        public string Parameter { get; set; }
    }

    public class PermissionItemByParameterFilter : PermissionItemFilter
    {
        public string Parameter { get; set; }

        public string Object { get; set; }
    }

}