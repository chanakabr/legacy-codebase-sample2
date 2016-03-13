using Ingest.Clients.ClientManager;
using Ingest.Importers;
using Ingest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Ingest
{
    public class Service : IService
    {
        public BusinessModuleIngestResponse IngestBusinessModules(string username, string password, string xml)
        {
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

            return response;
        }
    }
}
