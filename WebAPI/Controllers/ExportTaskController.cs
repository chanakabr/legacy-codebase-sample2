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
using WebAPI.Managers.Schema;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/exportTask/action")]
    [OldStandardAction("listOldStandard", "list")]
    [OldStandardAction("updateOldStandard", "update")]
    public class ExportTaskController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Adds a new bulk export task
        /// </summary>
        /// <param name="task">The task model to add</param>
        /// <remarks>
        /// Possible status codes:   
        /// Export notification URL required = 5017, Export frequency minimum value = 5018, Alias must be unique = 5019, Alias is required = 5020 
        /// </remarks>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaExportTask Add(KalturaExportTask task)
        {
            KalturaExportTask response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().AddBulkExportTask(groupId, task.Alias, task.Name, task.DataType, task.Filter, task.ExportType, task.getFrequency(), task.NotificationUrl,
                    task.VodTypes != null ? task.VodTypes.Select(vt => vt.value).ToList() : null, task.IsActive != null ? task.IsActive.Value : true);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Updates an existing bulk export task by task identifier
        /// </summary>
        /// <param name="id">The task id to update</param>
        /// <param name="task">The task model to update</param>
        /// <remarks>
        /// Possible status codes:   
        /// Export notification URL required = 5017, Export frequency minimum value = 5018, Alias must be unique = 5019
        /// </remarks>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaExportTask Update(long id, KalturaExportTask task)
        {
            KalturaExportTask response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (task.Id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "task id must be supplied");
            }

            try
            {
                response = ClientsManager.ApiClient().UpdateBulkExportTask(groupId, id, task.Alias, task.Name, task.DataType, task.Filter, task.ExportType, task.getFrequency(), task.NotificationUrl,
                    task.VodTypes != null ? task.VodTypes.Select(vt => vt.value).ToList() : null, task.IsActive);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Updates an existing bulk export task by task identifier
        /// </summary>
        /// <param name="task">The task model to update</param>
        /// <remarks>
        /// Possible status codes:   
        /// Export notification URL required = 5017, Export frequency minimum value = 5018, Alias must be unique = 5019
        /// </remarks>
        /// <returns></returns>
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool UpdateOldStandard(KalturaExportTask task)
        {
            int groupId = KS.GetFromRequest().GroupId;

            if (task.Id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "task id must be supplied");
            }

            try
            {
                ClientsManager.ApiClient().UpdateBulkExportTask(groupId, task.getId(), task.Alias, task.Name, task.DataType, task.Filter, task.ExportType, task.getFrequency(), task.NotificationUrl,
                    task.VodTypes != null ? task.VodTypes.Select(vt => vt.value).ToList() : null, task.IsActive);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Deletes an existing bulk export task by task identifier
        /// </summary>
        /// <param name="id">The identifier of the task to delete</param>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(long id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().DeleteBulkExportTask(groupId, id, null);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Gets an existing bulk export task by task identifier
        /// </summary>
        /// <param name="id">The identifier of the task to get</param>
        /// <returns></returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaExportTask Get(long id)
        {
            KalturaExportTask response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                var listRes = ClientsManager.ApiClient().GetBulkExportTasks(groupId, new long[] { id }, null, KalturaExportTaskOrderBy.CREATE_DATE_DESC);
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
        /// Returns bulk export tasks by tasks identifiers
        /// </summary>
        /// <param name="filter">Bulk export tasks filter</param>
        /// 
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaExportTaskListResponse List(KalturaExportTaskFilter filter = null)
        {
            KalturaExportTaskListResponse response = new KalturaExportTaskListResponse();

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                filter = new KalturaExportTaskFilter();
            }

            try
            {
                List<KalturaExportTask> objects = ClientsManager.ApiClient().GetBulkExportTasks(groupId, filter.getIdIn(), null, filter.OrderBy);
                response.Objects = objects;
                response.TotalCount = objects.Count;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns bulk export tasks by tasks identifiers
        /// </summary>
        /// <param name="filter">Bulk export tasks filter</param>
        /// 
        /// <returns></returns>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<KalturaExportTask> ListOldStandard(KalturaExportFilter filter = null)
        {
            List<KalturaExportTask> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                filter = new KalturaExportFilter();
            }

            try
            {
                response = ClientsManager.ApiClient().GetBulkExportTasks(groupId, filter.ids != null ? filter.ids.Select(id => id.value).ToArray() : null, null, filter.OrderBy);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}