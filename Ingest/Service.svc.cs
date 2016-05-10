using Ingest.Clients.ClientManager;
using Ingest.Importers;
using Ingest.Models;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Ingest
{
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
    }
}
