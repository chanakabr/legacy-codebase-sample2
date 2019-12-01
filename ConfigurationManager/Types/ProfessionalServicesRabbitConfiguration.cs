using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class ProfessionalServicesRabbitConfiguration : BaseRabbitConfiguration
    {

        public override string TcmKey => TcmObjectKeys.ProfessionalServicesRabbitConfiguration;

        public override string[] TcmPath => new string[] { TcmObjectKeys.MainRabbitConfiguration, TcmKey };

        public new BaseValue<string> RoutingKey = new BaseValue<string>("routingKey", "CDR_NOTIFICATION");
        public BaseValue<string> Task = new BaseValue<string>("task", "distributed_tasks.cdr_notification");

    }
}
