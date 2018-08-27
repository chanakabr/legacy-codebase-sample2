using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using KLogMonitor;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
using WebAPI.Utils;
using ApiObjects.Response;
using WebAPI.Models.Segmentation;

namespace WebAPI.Controllers
{
    [Service("segmentationType")]
    public class SegmentationTypeController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="segmentationType">.</param>
        /// <returns>.</returns>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaSegmentationType Add(KalturaSegmentationType segmentationType)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                return ClientsManager.ApiClient().AddSegmentationType(groupId, segmentationType);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="segmentationTypeId">.</param>
        /// <param name="segmentationType">.</param>
        /// <returns></returns>
        [Action("update")]
        [ApiAuthorize]
        static public KalturaSegmentationType Update(int segmentationTypeId, KalturaSegmentationType segmentationType)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                return ClientsManager.ApiClient().UpdateSegmentationType(groupId, segmentationType);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="id">.</param>
        /// <returns>.</returns>
        [Action("delete")]
        [ApiAuthorize]
        static public bool Delete(long id)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;

                response = ClientsManager.
                    ApiClient().DeleteSegmentationType(groupId, id);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        
        /// <summary>
        /// ...
        /// </summary>
        /// <param name="filter">.</param>
        /// <param name="pager">.</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaSegmentationTypeListResponse List(KalturaSegmentationTypeFilter filter, KalturaFilterPager pager = null)
        {
            KalturaSegmentationTypeListResponse response = null;

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                response = ClientsManager.
                    ApiClient().ListSegmentationTypes(groupId, pager.getPageIndex(), pager.getPageSize());
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        
    }
}