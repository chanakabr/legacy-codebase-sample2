using ApiObjects.Base;
using System.Collections.Generic;

namespace ApiObjects
{
    public class PasswordPolicyFilter : ICrudFilter
    {
        public List<long> RoleIdsIn { get; set; }
    }
}
