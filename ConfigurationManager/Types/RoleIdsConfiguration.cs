using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class RoleIdsConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue MasterRoleId;
        public NumericConfigurationValue UserRoleId;

        public RoleIdsConfiguration(string key) : base(key)
        {
            MasterRoleId = new NumericConfigurationValue("master_role_id", this)
            {
                DefaultValue = 2
            };
            UserRoleId = new NumericConfigurationValue("user_role_id", this)
            {
                DefaultValue = 1
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= MasterRoleId.Validate();
            result &= UserRoleId.Validate();

            return result;
        }
    }
}