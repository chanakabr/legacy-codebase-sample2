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
using WebAPI.Managers.Schema;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/announcement/action")]
    [OldStandardAction("listOldStandard", "list")]
    public class AnnouncementController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Add a new future scheduled system announcement push notification
        /// </summary>
        /// <param name="announcement">The announcement to be added.
        /// timezone parameter should be taken from the 'name of timezone' from: https://msdn.microsoft.com/en-us/library/ms912391(v=winembedded.11).aspx
        /// Recipients values: All, LoggedIn, Guests</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: AnnouncementMessageTooLong = 8010, AnnouncementMessageIsEmpty = 8004
        /// AnnouncementInvalidTimezone = 8008, FeatureDisabled = 8009</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(KalturaAnnouncement announcement)
        {
            bool response = false;

            try
            {
                // validate announcement is not empty
                if (announcement == null)
                {
                    log.Error("announcement object is empty");
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "announcement object is empty");
                }

                // validate announcement start date
                if (announcement.getStartTime() > 0 &&
                    announcement.StartTime < Utils.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow))
                {
                    log.ErrorFormat("start time have passed. given time: {0}", Utils.Utils.UnixTimeStampToDateTime((long)announcement.StartTime));
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "start time have passed");
                }

                int groupId = KS.GetFromRequest().GroupId;
                response = ClientsManager.NotificationClient().AddAnnouncement(groupId, announcement);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update an existing future system announcement push notification. Announcement can only be updated only before sending
        /// </summary>
        /// <param name="announcement">The announcement to update.
        /// timezone parameter should be taken from the 'name of timezone' from: https://msdn.microsoft.com/en-us/library/ms912391(v=winembedded.11).aspx
        /// Recipients values: All, LoggedIn, Guests</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: AnnouncementMessageTooLong = 8010, AnnouncementMessageIsEmpty = 8004, AnnouncementNotFound = 8006,
        /// AnnouncementUpdateNotAllowed = 8007, AnnouncementInvalidTimezone = 8008, FeatureDisabled = 8009</remarks>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(KalturaAnnouncement announcement)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;

                // validate announcement is not empty
                if (announcement == null)
                {
                    log.Error("announcement object is empty");
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "announcement object is empty");
                }

                // validate announcement start date
                if (announcement.getStartTime() > 0 &&
                    announcement.StartTime < Utils.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow))
                {
                    log.ErrorFormat("start time have passed. given time: {0}", Utils.Utils.UnixTimeStampToDateTime((long)announcement.StartTime));
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "start time have passed");
                }

                response = ClientsManager.NotificationClient().UpdateAnnouncement(groupId, announcement);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update a system announcement status
        /// </summary>
        /// <param name="id">Id of the announcement.</param>
        /// <param name="status">Status to update to.</param>
        /// <returns></returns>
        /// <remarks>AnnouncementNotFound = 8006, AnnouncementUpdateNotAllowed = 8007, FeatureDisabled = 8009</remarks>
        [Route("updateStatus"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public bool UpdateStatus(long id, bool status)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                response = ClientsManager.NotificationClient().UpdateAnnouncementStatus(groupId, id, status);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete an existing announcing. Announcement cannot be delete while being sent.
        /// </summary>
        /// <param name="id">Id of the announcement.</param>
        /// <returns></returns>
        /// <remarks>AnnouncementNotFound = 8006, FeatureDisabled = 8009</remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(long id)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                response = ClientsManager.NotificationClient().DeleteAnnouncement(groupId, id);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// create system announcement 
        /// </summary>       
        /// <returns></returns>
        /// <remarks>Possible status codes: FeatureDisabled = 8009, FailCreateAnnouncement = 8011</remarks>
        [Route("createAnnouncement"), HttpPost]
        [ApiAuthorize]
        public bool CreateAnnouncement()
        {
            bool response = false;
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                response = ClientsManager.NotificationClient().CreateSystemAnnouncement(groupId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;

        }

        /// <summary>
        /// Lists all announcements in the system.
        /// </summary>
        /// <param name="filter">Filter object</param>
        /// <param name="pager">Paging the request</param>
        /// <returns></returns>
        /// <remarks>FeatureDisabled = 8009</remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAnnouncementListResponse List(KalturaAnnouncementFilter filter, KalturaFilterPager pager = null)
        {
            KalturaAnnouncementListResponse response = null;

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                response = ClientsManager.NotificationClient().GetAnnouncements(groupId, pager.getPageSize(), pager.getPageIndex());
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Lists all announcements in the system.
        /// </summary>
        /// <param name="pager">Paging the request</param>
        /// <returns></returns>
        /// <remarks>FeatureDisabled = 8009</remarks>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaMessageAnnouncementListResponse ListOldStandard(KalturaFilterPager pager = null)
        {
            KalturaMessageAnnouncementListResponse response = null;

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                response = ClientsManager.NotificationClient().GetAllAnnouncements(groupId, pager.getPageSize(), pager.getPageIndex());
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}