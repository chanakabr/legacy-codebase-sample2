using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using System.Net;
using System.Web;
using ApiObjects.Response;
using System.IO;
using System.Net.Http;
using ConfigurationManager;
using Newtonsoft.Json.Linq;
using Core.Catalog.CatalogManagement;
using Core.Catalog;

namespace PartialUpdateHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting partial update task. data={0}", data);

                PartialUpdateRequest request = JsonConvert.DeserializeObject<PartialUpdateRequest>(data);

                bool success = false;

                ElasticsearchWrapper wrapper = new ElasticsearchWrapper();

                success = wrapper.PartialUpdate(request.GroupID, request.Assets);

                if (!success)
                {
                    throw new Exception(string.Format(
                        "Partial update task on group id {0} did not finish successfully.", request.GroupID));
                }
                else
                {
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}
