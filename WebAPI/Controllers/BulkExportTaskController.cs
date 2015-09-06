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
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/bulkExportTask/action")]
    public class BulkExportTaskController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Adds a new bulk export task
        /// </summary>
        /// <param name="external_key">External key for the task used to solicit an export using API</param>
        /// <param name="name">Task display name</param>
        /// <param name="data_type">The data type exported in this task</param>
        /// <param name="filter">Filter to apply on the export, utilize KSQL.</param>
        /// <param name="export_type">Type of export batch – full or incremental</param>
        /// <param name="frequency">How often the export should occur, reasonable minimum threshold should apply, configurable in minutes</param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates an existing bulk export task by identifier or by external key
        /// </summary>
        /// <param name="task">The task identifier or external key - depends on "by" parameter</param>
        /// <param name="by">Defines whether to update the task by identifier or external key</param>
        /// <param name="name">Task display name</param>
        /// <param name="data_type">The data type exported in this task</param>
        /// <param name="filter">Filter to apply on the export, utilize KSQL.</param>
        /// <param name="export_type">Type of export batch – full or incremental</param>
        /// <param name="frequency">How often the export should occur, reasonable minimum threshold should apply, configurable in minutes</param>
        /// <returns></returns>
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

        /// <summary>
        /// Deletes an existing bulk export task by identifier or by external key
        /// </summary>
        /// <param name="task">The task identifier or external key - depends on "by" parameter</param>
        /// <param name="by">Defines whether to delete the task by identifier or external key</param>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(string task, KalturaBulkExportReferenceBy by)
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

        /// <summary>
        /// Gets an existing bulk export task by identifier or by external key
        /// </summary>
        /// <param name="task">The task identifier or external key - depends on "by" parameter</param>
        /// <param name="by">Defines whether to get the task by identifier or external key</param>
        /// <returns></returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaBulkExportTask Get(string task, KalturaBulkExportReferenceBy by)
        {
            KalturaBulkExportTask response = null;

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
                            var listRes = ClientsManager.ApiClient().GetBulkExportTasks(groupId, new long[] { id }, null);
                            if (listRes != null && listRes.Count > 0)
                            {
                                response = listRes[0];
                            }
                        }
                        break;
                    case KalturaBulkExportReferenceBy.external_key:
                        {
                            var listRes = ClientsManager.ApiClient().GetBulkExportTasks(groupId, null, new string[] { task });
                            if (listRes != null && listRes.Count > 0)
                            {
                                response = listRes[0];
                            }
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

        /// <summary>
        /// Returns bulk export tasks by identifiers or by external keys
        /// </summary>
        /// <param name="filter">Bulk export tasks filter</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaBulkExportTask List(KalturaBulkExportFilter filter)
        {
            KalturaBulkExportTask response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                filter = new KalturaBulkExportFilter();
            }

            try
            {
                switch (filter.By)
                {
                    case KalturaBulkExportReferenceBy.id:
                        {
                            long[] ids;
                            try
                            {
                                ids = filter.Tasks.Select(t => long.Parse(t.value)).ToArray();
                            }
                            catch (Exception)
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "tasks must be numeric when reference by id");
                            }

                            var listRes = ClientsManager.ApiClient().GetBulkExportTasks(groupId, ids, null);
                            if (listRes != null && listRes.Count > 0)
                            {
                                response = listRes[0];
                            }
                        }
                        break;
                    case KalturaBulkExportReferenceBy.external_key:
                        {
                            var listRes = ClientsManager.ApiClient().GetBulkExportTasks(groupId, null, filter.Tasks.Select(t=>t.value).ToArray());
                            if (listRes != null && listRes.Count > 0)
                            {
                                response = listRes[0];
                            }
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