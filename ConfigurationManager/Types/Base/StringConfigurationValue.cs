using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class StringConfigurationValue : ConfigurationValue
    {
        public StringConfigurationValue(string key) : base(key)
        {
        }

        public StringConfigurationValue(string key, ConfigurationValue parent) : base(key, parent)
        {

        }

        internal override bool Validate()
        {
            try
            {
                string value = Convert.ToString(this.ObjectValue);

                if (string.IsNullOrEmpty(value))
                {
                    LogError("key is missing");

                    if (!this.ShouldAllowEmpty)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);

                return false;
            }

            return true;
        }

        public string Value
        {
            get
            {
                return Convert.ToString(this.ObjectValue);
            }
        }
    }
}