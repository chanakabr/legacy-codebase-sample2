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
    [RoutePrefix("_service/exportTask/action")]
    public class ExportTaskController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Adds a new bulk export task
        /// </summary>
        /// <param name="task">The task model to add</param>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(KalturaExportTask task)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().AddBulkExportTask(groupId, task.ExternalKey, task.Name, task.DataType, task.Filter, task.ExportType, task.Frequency);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Updates an existing bulk export task by external key
        /// </summary>
        /// <param name="task">The task model to update</param>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(KalturaExportTask task)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().UpdateBulkExportTask(groupId, 0, task.ExternalKey, task.Name, task.DataType, task.Filter, task.ExportType, task.Frequency);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Deletes an existing bulk export task by external key
        /// </summary>
        /// <param name="external_key">The external key of the task to delete</param>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(string external_key)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().DeleteBulkExportTask(groupId, 0, external_key);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Gets an existing bulk export task by external key
        /// </summary>
        /// <param name="external_key">The external key of the task to get</param>
        /// <returns></returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaExportTask Get(string external_key)
        {
            KalturaExportTask response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                var listRes = ClientsManager.ApiClient().GetBulkExportTasks(groupId, null, new string[] { external_key });
                if (listRes != null && listRes.Count > 0)
                {
                    response = listRes[0];
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns bulk export tasks by external keys
        /// </summary>
        /// <param name="filter">Bulk export tasks filter</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<KalturaExportTask> List(KalturaExportFilter filter = null)
        {
            List<KalturaExportTask> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                filter = new KalturaExportFilter();
            }

            try
            {
                response = ClientsManager.ApiClient().GetBulkExportTasks(groupId, null, filter.ExternalKeys != null ? filter.ExternalKeys.Select(ek => ek.value).ToArray() : null);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}