using ApiObjects.Base;
using System.Collections.Generic;

namespace ApiObjects
{
    public class PasswordPolicy : ICrudHandeledObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<long> UserRoleIds { get; set; }
        public int? HistoryCount { get; set; }
        public int? Expiration { get; set; }
        public List<RegexObject> Complexities { get; set; }
        public int? LockoutFailuresCount { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is PasswordPolicy passwordPolicy))
                return false;

            return passwordPolicy.Id == this.Id;
        }
    }
}
