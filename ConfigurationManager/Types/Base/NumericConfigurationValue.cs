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
            bool result = true;

            try
            {
                base.Validate();

                if (this.ObjectValue == null)
                {
                    ConfigurationValidationErrorLevel level = ConfigurationValidationErrorLevel.Optional;

                    // if mandatory - mark error as "failure" and fail validation (default is success)
                    if (!this.ShouldAllowEmpty)
                    {
                        level = ConfigurationValidationErrorLevel.Failure;
                        result = false;
                    }

                    LogError("Missing", level);
                }

                long value = Convert.ToInt64(this.ObjectValue);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ConfigurationValidationErrorLevel.Failure);
                result = false;
            }

            return result;
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