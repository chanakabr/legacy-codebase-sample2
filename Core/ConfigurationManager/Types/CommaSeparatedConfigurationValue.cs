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
        }

        public CommaSeparatedConfigurationValue(string key, ConfigurationValue parent) : base(key, parent)
        {
            PopulateList();
        }

        private void PopulateList()
        {
            values = new HashSet<string>();

            if (!string.IsNullOrEmpty(this.Value))
            {
                string[] splitted = this.Value.Split(',');

                foreach (var value in splitted)
                {
                    values.Add(value);
                }
            }
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
