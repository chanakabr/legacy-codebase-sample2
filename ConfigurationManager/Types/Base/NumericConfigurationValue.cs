using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class NumericConfigurationValue : ConfigurationValue
    {
        public NumericConfigurationValue(string key) : base(key)
        {
        }

        public NumericConfigurationValue(string key, ConfigurationValue parent) : base(key, parent)
        {

        }

        internal override bool Validate()
        {
            try
            {
                if (this.ObjectValue == null)
                {
                    LogError("Missing value");

                    if (this.ShouldAllowEmpty)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                long value = Convert.ToInt64(this.ObjectValue);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);

                return false;
            }

            return true;
        }

        public long LongValue
        {
            get
            {
                return Convert.ToInt64(this.ObjectValue);
            }
        }

        public int IntValue
        {
            get
            {
                return Convert.ToInt32(this.ObjectValue);
            }
        }

        public double DoubleValue
        {
            get
            {
                return Convert.ToDouble(this.ObjectValue);
            }
        }
    }
}