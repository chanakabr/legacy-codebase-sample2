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

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/announcement/action")]
    [OldStandardAction("addOldStandard", "add")]
    [OldStandardAction("updateOldStandard", "update")]
    [OldStandardAction("listOldStandard", "list")]
    [OldStandardAction("enableSystemAnnouncements", "createAnnouncement")]
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
        [Throws(WebAPI.Managers.Models.StatusCode.TimeInPast)]
        [Throws(eResponseStatus.AnnouncementMessageTooLong)]
        [Throws(eResponseStatus.AnnouncementMessageIsEmpty)]
        [Throws(eResponseStatus.AnnouncementInvalidTimezone)]
        [Throws(eResponseStatus.FeatureDisabled)]
        public KalturaAnnouncement Add(KalturaAnnouncement announcement)
        {
            try
            {
                // validate announcement start date
                if (announcement.getStartTime() > 0 &&
                    announcement.StartTime < Utils.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow))
                {
                    log.ErrorFormat("start time have passed. given time: {0}", Utils.Utils.UnixTimeStampToDateTime((long)announcement.StartTime));
                    throw new BadRequestException(BadRequestException.TIME_ARGUMENT_IN_PAST, "KalturaAnnouncement.startTime");
                }

                int groupId = KS.GetFromRequest().GroupId;
                return ClientsManager.NotificationClient().AddAnnouncement(groupId, announcement);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Add a new future scheduled system announcement push notification
        /// </summary>
        /// <param name="announcement">The announcement to be added.
        /// timezone parameter should be taken from the 'name of timezone' from: https://msdn.microsoft.com/en-us/library/ms912391(v=winembedded.11).aspx
        /// Recipients values: All, LoggedIn, Guests</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: AnnouncementMessageTooLong = 8010, AnnouncementMessageIsEmpty = 8004
        /// AnnouncementInvalidTimezone = 8008, FeatureDisabled = 8009</remarks>
        [Route("addOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(WebAPI.Managers.Models.StatusCode.TimeInPast)]
        [Throws(eResponseStatus.AnnouncementMessageTooLong)]
        [Throws(eResponseStatus.AnnouncementMessageIsEmpty)]
        [Throws(eResponseStatus.AnnouncementInvalidTimezone)]
        [Throws(eResponseStatus.FeatureDisabled)]
        public bool AddOldStandard(KalturaAnnouncement announcement)
        {
            try
            {
                // validate announcement start date
                if (announcement.getStartTime() > 0 &&
                    announcement.StartTime < Utils.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow))
                {
                    log.ErrorFormat("start time have passed. given time: {0}", Utils.Utils.UnixTimeStampToDateTime((long)announcement.StartTime));
                    throw new BadRequestException(BadRequestException.TIME_ARGUMENT_IN_PAST, "KalturaAnnouncement.startTime");
                }

                int groupId = KS.GetFromRequest().GroupId;
                ClientsManager.NotificationClient().AddAnnouncement(groupId, announcement);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Update an existing future system announcement push notification. Announcement can only be updated only before sending
        /// </summary>
        /// <param name="announcementId">The announcement identifier</param>
        /// <param name="announcement">The announcement to update.
        /// timezone parameter should be taken from the 'name of timezone' from: https://msdn.microsoft.com/en-us/library/ms912391(v=winembedded.11).aspx
        /// Recipients values: All, LoggedIn, Guests</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: AnnouncementMessageTooLong = 8010, AnnouncementMessageIsEmpty = 8004, AnnouncementNotFound = 8006,
        /// AnnouncementUpdateNotAllowed = 8007, AnnouncementInvalidTimezone = 8008, FeatureDisabled = 8009</remarks>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(WebAPI.Managers.Models.StatusCode.TimeInPast)]
        [Throws(eResponseStatus.AnnouncementMessageTooLong)]
        [Throws(eResponseStatus.AnnouncementMessageIsEmpty)]
        [Throws(eResponseStatus.AnnouncementNotFound)]
        [Throws(eResponseStatus.AnnouncementUpdateNotAllowed)]
        [Throws(eResponseStatus.AnnouncementInvalidTimezone)]
        [Throws(eResponseStatus.FeatureDisabled)]
        public KalturaAnnouncement Update(int announcementId, KalturaAnnouncement announcement)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;

                // validate announcement start date
                if (announcement.getStartTime() > 0 &&
                    announcement.StartTime < Utils.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow))
                {
                    log.ErrorFormat("start time have passed. given time: {0}", Utils.Utils.UnixTimeStampToDateTime((long)announcement.StartTime));
                    throw new BadRequestException(BadRequestException.TIME_ARGUMENT_IN_PAST, "KalturaAnnouncement.startTime");
                }

                return ClientsManager.NotificationClient().UpdateAnnouncement(groupId, announcementId, announcement);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
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
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(WebAPI.Managers.Models.StatusCode.TimeInPast)]
        [Throws(eResponseStatus.AnnouncementMessageTooLong)]
        [Throws(eResponseStatus.AnnouncementMessageIsEmpty)]
        [Throws(eResponseStatus.AnnouncementNotFound)]
        [Throws(eResponseStatus.AnnouncementUpdateNotAllowed)]
        [Throws(eResponseStatus.AnnouncementInvalidTimezone)]
        [Throws(eResponseStatus.FeatureDisabled)]
        public bool UpdateOldStandard(KalturaAnnouncement announcement)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                
                // validate announcement start date
                if (announcement.getStartTime() > 0 &&
                    announcement.StartTime < Utils.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow))
                {
                    log.ErrorFormat("start time have passed. given time: {0}", Utils.Utils.UnixTimeStampToDateTime((long)announcement.StartTime));
                    throw new BadRequestException(BadRequestException.TIME_ARGUMENT_IN_PAST, "KalturaAnnouncement.startTime");
                }

                ClientsManager.NotificationClient().UpdateAnnouncement(groupId, announcement.Id.Value, announcement);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
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
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.AnnouncementNotFound)]
        [Throws(eResponseStatus.AnnouncementUpdateNotAllowed)]
        [Throws(eResponseStatus.FeatureDisabled)]
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
        [Throws(eResponseStatus.AnnouncementNotFound)]
        [Throws(eResponseStatus.FeatureDisabled)]
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
        /// Enable system announcements
        /// </summary>       
        /// <returns></returns>
        /// <remarks>Possible status codes: FeatureDisabled = 8009, FailCreateAnnouncement = 8011</remarks>
        [Route("enableSystemAnnouncements"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.FeatureDisabled)]
        [Throws(eResponseStatus.FailCreateAnnouncement)]
        public bool EnableSystemAnnouncements()
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
        [Throws(eResponseStatus.FeatureDisabled)]
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
        [Throws(eResponseStatus.FeatureDisabled)]
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