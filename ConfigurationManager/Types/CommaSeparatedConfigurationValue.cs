using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigurationManager
{
    public class CommaSeparatedConfigurationValue : StringConfigurationValue
    {
        HashSet<string> values = null;

        public CommaSeparatedConfigurationValue(string key) : base(key)
        {
            values = new HashSet<string>();
        }

        public CommaSeparatedConfigurationValue(string key, ConfigurationValue parent) : base(key, parent)
        {
            values = new HashSet<string>();
        }

        internal override bool Validate()
        {
            bool isValid = base.Validate();

            try
            {
                if (!string.IsNullOrEmpty(this.Value))
                {
                    string[] splitted = this.Value.Split(',');

                    foreach (var value in splitted)
                    {
                        values.Add(value);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Validating comma separted list failed because {ex}", ConfigurationValidationErrorLevel.Failure);
                isValid = false;
            }

            return isValid;
        }
        public bool ContainsValue(string value)
        {
            return values.Contains(value);
        }

        public List<string> Values
        {
            get
            {
                return new List<string>(values);
            }
        }
    }
}
