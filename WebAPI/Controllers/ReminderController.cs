using ApiObjects.Response;
using KLogMonitor;
using System.Reflection;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/reminder/action")]
    public class ReminderController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Add a new future reminder
        /// </summary>
        /// <param name="reminder">The reminder to be added.</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: InvalidAssetId = 4024, FeatureDisabled = 8009, FailCreateAnnouncement = 8011, UserAlreadySetReminder = 8023, PassedAsset = 8024</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(WebAPI.Managers.Models.StatusCode.TimeInPast)]
        [Throws(eResponseStatus.UserAlreadySetReminder)]
        [Throws(eResponseStatus.FailCreateAnnouncement)]
        [Throws(eResponseStatus.PassedAsset)]
        [Throws(eResponseStatus.InvalidAssetId)]
        [Throws(eResponseStatus.FeatureDisabled)]
        public KalturaReminder Add(KalturaReminder reminder)
        {
            KalturaReminder response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                if (reminder.GetType() == typeof(KalturaAssetReminder))
                {
                    KalturaAssetReminder kalturaAssetReminder = reminder as KalturaAssetReminder;
                    if (kalturaAssetReminder == null)
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "Type");
                    }

                    // validate referenceId
                    if (kalturaAssetReminder.AssetId <= 0)
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "AssetId");
                    }

                    // call client
                    return ClientsManager.NotificationClient().AddAssetReminder(groupId, userId, kalturaAssetReminder);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Delete a reminder. Reminder cannot be delete while being sent.
        /// </summary>
        /// <param name="id">Id of the reminder.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(long id)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                response = ClientsManager.NotificationClient().DeleteReminder(userId, groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>        
        /// Return a list of reminders with optional filter by KSQL.
        /// </summary>
        /// <param name="filter">Filter object</param>
        /// <param name="pager">Paging the request</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: SyntaxError = 4004, FeatureDisabled = 8009</remarks>        
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.FeatureDisabled)]
        public KalturaReminderListResponse List(KalturaReminderFilter filter, KalturaFilterPager pager = null)
        {
            KalturaReminderListResponse response = null;

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                response = ClientsManager.NotificationClient().GetReminders(groupId, userId, filter.KSql, pager.getPageSize(), pager.getPageIndex(), filter.OrderBy);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}