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
            string stringValue = this.ObjectValue.ToString();

            if (string.IsNullOrEmpty(stringValue))
            {
                this.ObjectValue = false;
                LogError("Empty/null value for boolean value", ConfigurationValidationErrorLevel.Failure);
            }
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

                    if (!this.ShouldAllowEmpty)
                    {
                        result = false;
                        level = ConfigurationValidationErrorLevel.Failure;
                    }

                    LogError("Missing", level);
                }

                bool value = Convert.ToBoolean(this.ObjectValue);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ConfigurationValidationErrorLevel.Failure);

                result = false;
            }

            return result;
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