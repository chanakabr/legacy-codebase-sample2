using ApiObjects.Notification;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Notification
{
    public class MailNotificationAdapterClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static bool SendConfigurationToAdapter(int groupId, MailNotificationAdapter adapter)
        {
            try
            {
                if (adapter == null || string.IsNullOrEmpty(adapter.AdapterUrl))
                {
                    log.ErrorFormat("Adapter URL was not found. group ID: {0}, adapter: {1}",
                        groupId,
                        adapter != null ? JsonConvert.SerializeObject(adapter) : "null");
                    return false;
                }

                //set unixTimestamp
                long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(adapter.Id, adapter.ProviderUrl, adapter.Settings, groupId, unixTimestamp);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AdapterStatus adapterResponse = client.SetConfiguration(
                        adapter.Id, adapter.Settings, groupId, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null && adapterResponse.Code == (int)ApiObjects.Response.eResponseStatus.OK)
                    {
                        log.DebugFormat("Successfully set configuration of mail notification adapter. Result: AdapterID = {0}", adapter.Id);
                        return true;
                    }
                    else
                    {
                        log.ErrorFormat("Failed to set mail notification  Adapter configuration. Result: AdapterID = {0}, AdapterStatus = {1}",
                            adapter.Id, adapterResponse != null ? adapterResponse.Code.ToString() : "ERROR");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SendConfigurationToAdapter Failed: AdapterID = {0}, ex = {1}", adapter.Id, ex);
            }

            return false;
        }


        public static string CreateAnnouncement(int groupId, string announcementName)
        {
            announcementName = string.Format("{0}_{1}", Utils.Base64ForUrlEncode(announcementName), groupId);

            string externalMailAnnouncementId = string.Empty;

            MailNotificationAdapter adapter = GetMailNotificationAdapter(groupId);

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("Notification URL wasn't found");
                return externalMailAnnouncementId;
            }

            try
            {
                //set unixTimestamp
                long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(adapter.Id, adapter.ProviderUrl, adapter.Settings, groupId, unixTimestamp);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AnnouncementResponse response = client.CreateAnnouncement(adapter.Id, announcementName, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (response == null || string.IsNullOrEmpty(response.MailAnnouncementId))
                        log.ErrorFormat("Error while trying to create announcement. announcement Name: {0}", announcementName);
                    else
                        log.DebugFormat("successfully created announcement. announcement Name: {0}", announcementName);
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to create announcement. announcement Name: {0}, ex: {1}", announcementName, ex);
            }

            return externalMailAnnouncementId;
        }

        public static bool DeleteAnnouncement(int groupId, string externalAnnouncementId)
        {
            bool result = false;

            long adapterId = NotificationSettings.GetPartnerMailNotificationAdapterId(groupId);

            // validate notification URL exists
            if (string.IsNullOrEmpty(NotificationSettings.GetPushAdapterUrl(groupId)))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    // Anat 
                    //using (ServiceClient client = new ServiceClient())
                    //{
                    //    client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                    //    result = client.DeleteTopic(externalAnnouncementId);
                    //    if (!result)
                    //        log.ErrorFormat("Error while trying to delete announcement. announcement external ID: {0}", externalAnnouncementId);
                    //    else
                    //        log.DebugFormat("successfully deleted announcement. announcement external ID: {0}", externalAnnouncementId);
                    //}
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to delete announcement. announcement external ID: {0}, ex: {1}", externalAnnouncementId, ex);
                }
            }
            return result;
        }

        public static bool SubscribeToAnnouncement(int groupId, List<string> announcementExternalIds, UserData userData)
        {
            bool result = false;

            long adapterId = NotificationSettings.GetPartnerMailNotificationAdapterId(groupId);

            // validate notification URL exists
            if (string.IsNullOrEmpty("")) // Anat: get adapter URL
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    // Anat
                    //using (ServiceClient client = new ServiceClient())
                    //{
                    //    client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                    //    // fire request
                    //    result = client.SubscribeToTopic(announcementSubscriptions);

                    //    // validate response
                    //    if ((result == null ||
                    //        result.Count == 0) ||
                    //        result.Count != announcementSubscriptions.Count)
                    //    {
                    //        log.ErrorFormat("Error while trying to subscribe to announcement. announcementSubscriptions: {0}", JsonConvert.SerializeObject(announcementSubscriptions));
                    //    }
                    //    else
                    //    {
                    //        var failedSubscriptions = result.Where(x => string.IsNullOrEmpty(x.SubscriptionArnResult));
                    //        if (failedSubscriptions.Count() > 0)
                    //            log.ErrorFormat("Some of the subscription failed. failed subs: {0}", JsonConvert.SerializeObject(failedSubscriptions));
                    //        else
                    //            log.Debug("Announcement subscription passed");
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to subscribe to announcement. announcementSubscriptions: {0}, ex: {1}", JsonConvert.SerializeObject(announcementExternalIds), ex);
                }
            }
            return result;
        }

        public static bool UnSubscribeToAnnouncement(int groupId, List<string> announcementExternalIds, UserData userData)
        {
            bool result = false;

            long adapterId = NotificationSettings.GetPartnerMailNotificationAdapterId(groupId);

            // validate notification URL exists
            if (string.IsNullOrEmpty(NotificationSettings.GetPushAdapterUrl(groupId)))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    //Anat
                    //using (ServiceClient client = new ServiceClient())
                    //{
                    //    client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                    //    // fire request
                    //    result = client.UnSubscribeToTopic(announcementExternalIds);

                    //    // validate response
                    //    if ((result == null ||
                    //        result.Count == 0) ||
                    //        result.Count != announcementExternalIds.Count)
                    //    {
                    //        log.ErrorFormat("Error while trying to subscribe to unsubscribe announcements. unsubscribeList: {0}", JsonConvert.SerializeObject(announcementExternalIds));
                    //    }
                    //    else
                    //    {
                    //        var failedUnsubscriptions = result.Where(x => !x.Success);
                    //        if (failedUnsubscriptions.Count() > 0)
                    //            log.ErrorFormat("Some of the unsubscribe request failed. failed subs: {0}", JsonConvert.SerializeObject(failedUnsubscriptions));
                    //        else
                    //            log.Debug("Announcement unsubscribe passed");
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to unsubscribe to announcement. unsubscribeList: {0}, ex: {1}", JsonConvert.SerializeObject(announcementExternalIds), ex);
                }
            }
            return result;
        }

        public static string PublishToAnnouncement(int groupId, string externalAnnouncementId, string subject, List<KeyValuePair<string, string>> mergeVars)
        {
            string messageId = string.Empty;

            long adapterId = NotificationSettings.GetPartnerMailNotificationAdapterId(groupId);

            // validate notification URL exists
            if (string.IsNullOrEmpty("")) // Anat: get adapter URL
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    // Anat
                    //using (ServiceClient client = new ServiceClient())
                    //{
                    //    client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                    //    messageId = client.PublishToTopic(externalAnnouncementId, subject, message);
                    //    if (string.IsNullOrEmpty(messageId))
                    //        log.ErrorFormat("Error while trying to publish announcement. announcement external ID: {0}, message: {1}", externalAnnouncementId, message);
                    //    else
                    //        log.DebugFormat("successfully published announcement. announcement external ID: {0}, message: {1}", externalAnnouncementId, message);
                    //}
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to publish announcement. announcement external ID: {0}, mergeVars: {1} ex: {2}", externalAnnouncementId, mergeVars, ex);
                }
            }
            return messageId;
        }

        private static MailNotificationAdapter GetMailNotificationAdapter(int groupId)
        {
            MailNotificationAdapter mailNotificationAdapter = null;

            long adapterId = NotificationSettings.GetPartnerMailNotificationAdapterId(groupId);

            //get adapter
            mailNotificationAdapter = NotificationDal.GetMailNotificationAdapter(groupId, adapterId);

            if (mailNotificationAdapter == null || mailNotificationAdapter.Id <= 0)
            {
                log.ErrorFormat("failed. adapter not exists groupID = {0}", groupId);
            }

            return mailNotificationAdapter;
        }
    }
}
