using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class CeleryRoutingConfiguration : BaseConfig<CeleryRoutingConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.CeleryRoutingConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public CeleryRoutingDistributedTasksConfiguration distributedTasks = new CeleryRoutingDistributedTasksConfiguration();

    }
}
