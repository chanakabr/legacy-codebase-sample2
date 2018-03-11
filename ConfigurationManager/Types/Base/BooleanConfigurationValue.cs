using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class BooleanConfigurationValue : ConfigurationValue
    {
        public BooleanConfigurationValue(string key) : base(key)
        {
        }

        public BooleanConfigurationValue(string key, ConfigurationValue parent) : base(key, parent)
        {

        }

        internal override bool Validate()
        {
            try
            {
                if (this.ObjectValue == null)
                {
                    if (this.ShouldAllowEmpty)
                    {
                        return true;
                    }
                    else
                    {
                        LogError("Missing value");
                        return false;
                    }
                }

                bool value = Convert.ToBoolean(this.ObjectValue);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);

                return false;
            }

            return true;
        }

        public bool Value
        {
            get
            {
                return Convert.ToBoolean(this.ObjectValue);
            }
        }
    }
}