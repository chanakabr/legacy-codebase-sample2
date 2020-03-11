using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class BooleanConfigurationValue : ConfigurationValue
    {
        private bool isEmpty = false;

        public BooleanConfigurationValue(string key) : base(key)
        {
        }

        public BooleanConfigurationValue(string key, ConfigurationValue parent) : base(key, parent)
        {
            isEmpty = false;

            if (this.ObjectValue == null)
            {
                isEmpty = true;
            }
            else
            {
                try
                {
                    string stringValue = Convert.ToString(this.ObjectValue);

                    if (string.IsNullOrEmpty(stringValue))
                    {
                        isEmpty = true;
                    }
                }
                catch
                {
                    isEmpty = true;
                }
            }

            if (isEmpty)
            {
                this.ObjectValue = false;

                if (!ShouldAllowEmpty)
                {
                    LogError("Empty/null value for boolean value", ConfigurationValidationErrorLevel.Failure);
                }
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

        public override void LoadDefault()
        {
            base.LoadDefault();

            if (isEmpty && this.DefaultValue != null)
            {
                this.ObjectValue = this.DefaultValue;
            }
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