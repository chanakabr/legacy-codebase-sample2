using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiObjects;
using Core.Catalog;
using Ingest.Models;
using Ingest;
using ApiObjects.Catalog;
using System.ServiceModel;
using KLogMonitor;
using System.Reflection;
using Ingest.Clients.ClientManager;
using Ingest.Importers;

namespace IngetsNetCore
{
    public class IngestService : IService
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public BusinessModuleIngestResponse IngestBusinessModules(string username, string password, string xml)
        {
            // add the log topic
            KLogger.LogContextData[Constants.TOPIC] = "Business module ingest";

            BusinessModuleIngestResponse response = new BusinessModuleIngestResponse()
            {
                Status = new Status((int)StatusCodes.Error, StatusCodes.Error.ToString())
            };

            // get group id
            int groupId = ClientsManager.ApiClient().GetGroupIdByUsernamePassword(username, password);

            if (groupId > 0)
            {
                // import
                response = BusinessModulesImporter.Ingest(groupId, xml);
            }
            else
            {
                _Logger.ErrorFormat("IngestBusinessModules: Failed to get group id for username: {0}, password: {1}", username, password);
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
            _Logger.Topic = "EPGIngest";
            IngestResponse response = IngestController.IngestData(request, eIngestType.KalturaEpg);
            return response;
        }
    }
}
