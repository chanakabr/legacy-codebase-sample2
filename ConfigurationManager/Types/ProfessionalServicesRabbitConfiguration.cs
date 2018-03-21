using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class ProfessionalServicesRabbitConfiguration : BaseRabbitConfiguration
    {
        public StringConfigurationValue Task;

        public ProfessionalServicesRabbitConfiguration(string key) : base(key)
        {
            this.Initialize();
        }

        public ProfessionalServicesRabbitConfiguration(string key, ConfigurationValue parent) : base(key, parent)
        {
            this.Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();

            Task = new ConfigurationManager.StringConfigurationValue("task", this)
            {
                DefaultValue = "distributed_tasks.cdr_notification"
            };
        }

        internal override bool Validate()
        {
            bool result = base.Validate();

            result &= this.Task.Validate();

            return result;
        }
    }
}
