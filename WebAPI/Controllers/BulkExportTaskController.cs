using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/bulkExportTask/action")]
    public class BulkExportTaskController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(string external_key, string name, KalturaExportDataType data_type, string filter, KalturaExportType export_type, long frequency)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().AddBulkExportTask(groupId, external_key, name, data_type, filter, export_type, frequency);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(string task, KalturaBulkExportReferenceBy by, string name, KalturaExportDataType data_type, string filter, KalturaExportType export_type, long frequency)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                switch (by)
                {
                    case KalturaBulkExportReferenceBy.id:
                        {
                            long id;
                            if (!long.TryParse(task, out id))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "task must be numeric when reference by id");
                            }
                            response = ClientsManager.ApiClient().UpdateBulkExportTask(groupId, id, null, name, data_type, filter, export_type, frequency);
                        }
                        break;
                    case KalturaBulkExportReferenceBy.external_key:
                        {
                            response = ClientsManager.ApiClient().UpdateBulkExportTask(groupId, 0, task, name, data_type, filter, export_type, frequency);
                        }
                        break;
                    default:
                        break;
                }
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Update(string task, KalturaBulkExportReferenceBy by)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                switch (by)
                {
                    case KalturaBulkExportReferenceBy.id:
                        {
                            long id;
                            if (!long.TryParse(task, out id))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "task must be numeric when reference by id");
                            }
                            response = ClientsManager.ApiClient().DeleteBulkExportTask(groupId, id, null);
                        }
                        break;
                    case KalturaBulkExportReferenceBy.external_key:
                        {
                            response = ClientsManager.ApiClient().DeleteBulkExportTask(groupId, 0, task);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}