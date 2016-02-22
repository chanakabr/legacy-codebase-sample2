using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/announcement/action")]
    public class AnnouncementController : ApiController
    {
        /// <summary>
        /// Add a new future scheduled system announcment push notification
        /// </summary>
        /// <param name="announcement">The announcement to be added.
        /// timezone param should be taken from the 'name of timezone' from: https://msdn.microsoft.com/en-us/library/ms912391(v=winembedded.11).aspx </param>
        /// <returns></returns>
        /// <remarks>Possible status codes: AnnouncementMessageTooLong = 8010, AnnouncementMessageIsEmpty = 8004, AnnouncementInvalidStartTime = 8005,
        /// FeatureDisabled = 8009</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public int Add(KalturaAnnouncement announcement)
        {
            int response = 0;

            try
            {
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
        /// Update an existing future system annoucement push notification. Annoucement can only be updated only before sending
        /// </summary>
        /// <param name="announcement">The announcement to update.
        /// timezone param should be taken from the 'name of timezone' from: https://msdn.microsoft.com/en-us/library/ms912391(v=winembedded.11).aspx </param>
        /// <returns></returns>
        /// <remarks>Possible status codes: AnnouncementMessageTooLong = 8010, AnnouncementMessageIsEmpty = 8004, AnnouncementInvalidStartTime = 8005, AnnouncementNotFound = 8006,
        /// AnnouncementUpdateNotAllowed = 8007, FeatureDisabled = 8009</remarks>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(KalturaAnnouncement announcement)
        {
            bool response = false;
            
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
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
        /// <remarks>AnnouncementNotFound = 8006, AnnouncementUpdateNotAllowed = 8007</remarks>
        [Route("updateStatus"), HttpPost]
        [ApiAuthorize]
        public bool UpdateStatus(int id, bool status)
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
        /// Delete an exisitng annoucnent. Annoucment cannot be delete while being sent.
        /// </summary>
        /// <param name="id">Id of the announcement.</param>
        /// <returns></returns>
        /// <remarks>AnnouncementNotFound = 8006, AnnouncementUpdateNotAllowed = 8007</remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int id)
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
<<<<<<< HEAD
        /// create system announcment 
        /// </summary>       
        /// <returns></returns>
        /// <remarks>Possible status codes: FeatureDisabled = 8009, FailCreateAnnouncement = 8011</remarks>
        [Route("createannouncement"), HttpPost]
        [ApiAuthorize]
        public bool CreateAnnouncement()
        {
            bool response = false;
=======
        /// Lists all announcements in the system.
        /// </summary>
        /// <param name="pager">Paging the request</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaMessageAnnouncementListResponse List(KalturaFilterPager pager = null)
        {
            KalturaMessageAnnouncementListResponse response = null;

            if (pager == null)
                pager = new KalturaFilterPager();
>>>>>>> 8d5266bd9e02d1fa7a7e6119a060ea82c713c6d4

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
<<<<<<< HEAD
                response = ClientsManager.NotificationClient().CreateSystemAnnouncement(groupId);
=======
                response = ClientsManager.NotificationClient().GetAllAnnouncements(groupId, pager.PageSize, pager.PageIndex);
>>>>>>> 8d5266bd9e02d1fa7a7e6119a060ea82c713c6d4
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}