using ApiObjects.Base;
using System.Collections.Generic;

namespace ApiObjects
{
    public class PasswordPolicy : ICrudHandeledObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public HashSet<long> UserRoleIds { get; set; }
        public int? HistoryCount { get; set; }
        public int? Expiration { get; set; }
        public List<PasswordRegex> Complexities { get; set; }
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
        public bool CompareAndFill(PasswordPolicy oldPasswordPolicy)
        {
            var needToUpdate = false;

            if (string.IsNullOrEmpty(this.Name)) //to do fill as name
            {
                this.Name = oldPasswordPolicy.Name;
            }
            else
            {
                needToUpdate = true;
            }

            if (this.UserRoleIds == null)
            {
                this.UserRoleIds = oldPasswordPolicy.UserRoleIds;
            }
            else
            {
                needToUpdate = true;
            }

            if (!this.HistoryCount.HasValue)
            {
                this.HistoryCount = oldPasswordPolicy.HistoryCount;
            }
            else
            {
                needToUpdate = true;
            }

            if (!this.Expiration.HasValue)
            {
                this.Expiration = oldPasswordPolicy.Expiration;
            }
            else
            {
                needToUpdate = true;
            }

            if (this.Complexities == null)
            {
                this.Complexities = oldPasswordPolicy.Complexities;
            }
            else
            {
                needToUpdate = true;
            }
            
            if (!this.LockoutFailuresCount.HasValue)
            {
                this.LockoutFailuresCount = oldPasswordPolicy.LockoutFailuresCount;
            }
            else
            {
                needToUpdate = true;
            }
            
            return needToUpdate;
        }
    }
}
