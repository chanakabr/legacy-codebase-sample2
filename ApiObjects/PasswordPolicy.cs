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

        /// <summary>
        /// fill empty policies
        /// </summary>
        /// <param name="orgPolicy"></param>
        public void CompareAndFillPolicy(PasswordPolicy other)
        {
            //skip id
            this.Name = this.Name ?? other.Name;
            this.Complexities = this.Complexities ?? other.Complexities;
            this.Expiration = this.Expiration ?? other.Expiration;
            this.LockoutFailuresCount = this.LockoutFailuresCount ?? other.LockoutFailuresCount;
            this.HistoryCount = this.HistoryCount ?? other.HistoryCount;
            this.UserRoleIds =
                (this.UserRoleIds != null && this.UserRoleIds.Count > 0) ?
                this.UserRoleIds : other.UserRoleIds;

            //set null for 0 value
            if (this.Expiration == 0) this.Expiration = null;
            if (this.LockoutFailuresCount == 0) this.LockoutFailuresCount = null;
            if (this.HistoryCount == 0) this.HistoryCount = null;
        }
    }
}
