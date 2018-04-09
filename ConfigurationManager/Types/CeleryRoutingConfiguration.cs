using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class CeleryRoutingConfiguration : StringConfigurationValue
    {
        private JObject json;
        public CeleryRoutingDistributedTasksConfiguration distributedTasks;

        public CeleryRoutingConfiguration(string key) : base(key)
        {
            if (!string.IsNullOrEmpty(this.Value))
            {
                json = JObject.Parse(this.Value);
            }

            distributedTasks = new ConfigurationManager.CeleryRoutingDistributedTasksConfiguration("distributed_tasks", this)
            {
                ShouldAllowEmpty = true
            };
        }

        internal override bool Validate()
        {
            bool result = base.Validate();

            result &= distributedTasks.Validate();

            return result;
        }

        public string GetHandler(string path)
        {
            string result = string.Empty;

            if (json != null)
            {
                var tempToken = json.SelectToken(path);

                if (tempToken != null)
                {
                    result = tempToken.Value<string>();
                }
            }

            return result;
        }
    }
}
