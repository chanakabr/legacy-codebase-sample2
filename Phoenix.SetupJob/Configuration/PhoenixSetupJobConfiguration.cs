using System.Collections.Generic;

namespace Phoenix.SetupJob.Configuration
{
    public class PhoenixSetupJobConfiguration
    {
        public string KafkaConnectionString { get; set; }
        public string KronosServiceId { get; set; } = "kronos";
    }
}