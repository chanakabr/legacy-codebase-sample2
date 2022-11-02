using System;
using System.Text;
using System.Threading;
using Google.Protobuf;
using Microsoft.Extensions.Logging.Abstractions;
using OTT.Service.Kronos;
using Phx.Lib.Appconfig;


namespace ApiLogic.kronos
{
    public class KronosClient
    {
        private const string ClientId = "phoenix";
        private const string kronosServiceId  = "kronos";
        private static readonly Lazy<KronosClient> LazyInstance =
            new Lazy<KronosClient>(() => new KronosClient(), LazyThreadSafetyMode.PublicationOnly);

        public static readonly KronosClient Instance = LazyInstance.Value;
        private KronosAsyncClient _kronosClient;
        public KronosClient()
        {
            var tcmConfig = ApplicationConfiguration.Current.KafkaClientConfiguration;
            _kronosClient = new KronosAsyncClient(ClientId, kronosServiceId, tcmConfig.BootstrapServers.Value, NullLoggerFactory.Instance, new PhoenixTracingProvider());
        }

        public void ScheduledTask(int partnerId, string qualifiedName, long eta, string body, string category, long timeout)
        {
            var scheduleTaskReq = new ScheduleTaskRequest
            {
                PartnerId = partnerId,
                TaskData = new TaskData
                {
                    Sender = ClientId,
                    Category = category,
                    QualifiedName = qualifiedName,
                    ETA = eta,
                    Body = ByteString.CopyFrom(body, Encoding.UTF8),
                    TimeoutSeconds = timeout,
                } 
            };
            
            _kronosClient.ScheduleTask(scheduleTaskReq);
        }
    }
    
    internal class PhoenixTracingProvider : ITracingProvider
    {
        public string GetTraceId() =>  Guid.NewGuid().ToString();
    }
}