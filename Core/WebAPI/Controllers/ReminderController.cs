using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("reminder")]
    public class ReminderController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Add a new future reminder
        /// </summary>
        /// <param name="reminder">The reminder to be added.</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: InvalidAssetId = 4024, FeatureDisabled = 8009, FailCreateAnnouncement = 8011, UserAlreadySetReminder = 8023, PassedAsset = 8024</remarks>
        [Action("add")]
        [ApiAuthorize]
        [Throws(StatusCode.UserIDInvalid)]
        [Throws(eResponseStatus.UserAlreadySetReminder)]
        [Throws(eResponseStatus.FailCreateAnnouncement)]
        [Throws(eResponseStatus.PassedAsset)]
        [Throws(eResponseStatus.InvalidAssetId)]
        [Throws(eResponseStatus.FeatureDisabled)]
        static public KalturaReminder Add(KalturaReminder reminder)
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
                else if (reminder.GetType() == typeof(KalturaSeriesReminder))
                {
                    // call client
                    return ClientsManager.NotificationClient().AddSeriesReminder(groupId, userId, reminder as KalturaSeriesReminder);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "objectType");
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
        /// <param name="type">Reminder type.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [Action("delete")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.ReminderNotFound)]
        static public bool Delete(long id, KalturaReminderType type)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                response = ClientsManager.NotificationClient().DeleteReminder(int.Parse(userId), groupId, id, type);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete a reminder. Reminder cannot be delete while being sent.
        /// </summary>
        /// <param name="id">Id of the reminder.</param>
        /// <param name="type">Reminder type.</param>
        /// <param name="token">User's token identifier</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [Action("deleteWithToken")]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.InvalidToken)]
        [Throws(eResponseStatus.ReminderNotFound)]
        static public void DeleteWithToken(long id, KalturaReminderType type, string token, int partnerId)
        {
            try
            {
                int userId = ClientsManager.NotificationClient().GetUserIdByToken(partnerId, token);

                ClientsManager.NotificationClient().DeleteReminder(userId, partnerId, id, type);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>        
        /// Return a list of reminders with optional filter by KSQL.
        /// </summary>
        /// <param name="filter">Filter object</param>
        /// <param name="pager">Paging the request</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(StatusCode.UserIDInvalid)]
        [Throws(eResponseStatus.FeatureDisabled)]
        static public KalturaReminderListResponse List<T>(KalturaReminderFilter<T> filter, KalturaFilterPager pager = null) where T : struct, IComparable, IFormattable, IConvertible
        {
            KalturaReminderListResponse response = null;

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                if (filter is KalturaSeasonsReminderFilter)
                {
                    KalturaSeasonsReminderFilter seasonsReminderFilter = filter as KalturaSeasonsReminderFilter;
                    response = ClientsManager.NotificationClient().GetSeriesReminders(groupId, userId, new List<string>() { seasonsReminderFilter.SeriesIdEqual },
                        seasonsReminderFilter.GetSeasonNumberIn(), seasonsReminderFilter.EpgChannelIdEqual, pager.PageSize.Value, pager.GetRealPageIndex());
                }
                else if (filter is KalturaSeriesReminderFilter)
                {
                    KalturaSeriesReminderFilter seriesReminderFilter = filter as KalturaSeriesReminderFilter;
                    response = ClientsManager.NotificationClient().GetSeriesReminders(groupId, userId, seriesReminderFilter.GetSeriesIdIn(), null, seriesReminderFilter.EpgChannelIdEqual,
                        pager.PageSize.Value, pager.GetRealPageIndex());
                }
                else if (filter is KalturaAssetReminderFilter || filter is KalturaReminderFilter<KalturaAssetReminderOrderBy>)
                {
                    KalturaAssetReminderFilter assetReminderFilter = filter as KalturaAssetReminderFilter;
                    response = ClientsManager.NotificationClient().GetReminders(groupId, userId, filter.KSql, pager.PageSize.Value, pager.GetRealPageIndex(), assetReminderFilter.OrderBy);
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