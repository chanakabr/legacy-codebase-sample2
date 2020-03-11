using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog;
using Ingest.Clients.ClientManager;
using Ingest.Importers;
using Ingest.Models;
using KLogMonitor;
using System.Reflection;
using System.ServiceModel;

namespace Ingest
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]    
    public class Service : IService
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public BusinessModuleIngestResponse IngestBusinessModules(string username, string password, string xml)
        {
            // add the log topic
            OperationContext.Current.IncomingMessageProperties[Constants.TOPIC] = "Business module ingest";

            BusinessModuleIngestResponse response = new BusinessModuleIngestResponse()
            {
                Status = new Status((int)StatusCodes.Error, StatusCodes.Error.ToString())
            };

            // get group id
            int groupId =  ClientsManager.ApiClient().GetGroupIdByUsernamePassword(username, password);

            if (groupId > 0)
            {
                // import
                response = BusinessModulesImporter.Ingest(groupId, xml);
            }
            else
            {
                log.ErrorFormat("IngestBusinessModules: Failed to get group id for username: {0}, password: {1}", username, password);
            }
            return response;
        }

        public IngestResponse IngestTvinciData(IngestRequest request)
        {
            return IngestController.IngestData(request, eIngestType.Tvinci);
        }

        public IngestResponse IngestAdiData(IngestRequest request)
        {
            return IngestController.IngestData(request, eIngestType.Adi);
        }

        [ServiceKnownType(typeof(EpgIngestResponse))]
        public IngestResponse IngestKalturaEpg(IngestRequest request)
        {
            log.Topic = "EPGIngest";
            IngestResponse response = (IngestResponse)IngestController.IngestData(request, eIngestType.KalturaEpg);
            return response;
        }
    }
}
