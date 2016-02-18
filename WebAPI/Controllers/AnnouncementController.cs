using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Notifications;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/announcements/action")]
    public class AnnouncementController : ApiController
    {
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
    }
}