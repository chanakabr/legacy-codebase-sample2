using ApiObjects.Notification;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                log.Error("Mail Notification URL wasn't found");
                return externalMailAnnouncementId;
            }

            try
            {
                long unixTimestamp;
                string signature;
                GetClientCallParamters(groupId, adapter, out unixTimestamp, out signature);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AnnouncementResponse response = client.CreateAnnouncement(adapter.Id, announcementName, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (response == null || string.IsNullOrEmpty(response.AnnouncementExternalId))
                    {
                        log.ErrorFormat("Error while trying to create announcement. announcement Name: {0}", announcementName);
                    }
                    else
                    {
                        log.DebugFormat("successfully created announcement. announcement Name: {0}", announcementName);
                        externalMailAnnouncementId = response.AnnouncementExternalId;
                    }
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

            MailNotificationAdapter adapter = GetMailNotificationAdapter(groupId);

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("mail Notification URL wasn't found");
                return false;
            }

            try
            {
                long unixTimestamp;
                string signature;
                GetClientCallParamters(groupId, adapter, out unixTimestamp, out signature);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AdapterStatus response = client.DeleteAnnouncement(adapter.Id, externalAnnouncementId, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (response == null || response.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Error while trying to delete announcement. announcement Name: {0}", externalAnnouncementId);
                    }
                    else
                    {
                        log.DebugFormat("successfully delete announcement. announcement Name: {0}", externalAnnouncementId);
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to delete announcement. announcement external ID: {0}, ex: {1}", externalAnnouncementId, ex);
            }

            return result;
        }

        public static bool SubscribeToAnnouncement(int groupId, List<string> announcementExternalIds, UserData userData, int userId)
        {
            bool result = false;

            MailNotificationAdapter adapter = GetMailNotificationAdapter(groupId);

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("Mail Notification URL wasn't found");
                return false;
            }

            try
            {
                long unixTimestamp;
                string signature;
                GetClientCallParamters(groupId, adapter, out unixTimestamp, out signature);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AnnouncementListResponse response = client.Subscribe(adapter.Id, userData.FirstName, userData.LastName, userData.Email
                        , announcementExternalIds, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (response == null || response.Status == null || response.Status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Error while trying to SubscribeToAnnouncement. adpaterId: {0}", adapter.Id);
                    }
                    else
                    {
                        log.DebugFormat("successfully SubscribeToAnnouncement. adpaterId: {0}", adapter.Id);
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to subscribe to announcement. announcementSubscriptions: {0}, ex: {1}", JsonConvert.SerializeObject(announcementExternalIds), ex);
            }

            return result;
        }

        public static bool UnSubscribeToAnnouncement(int groupId, List<string> announcementExternalIds, UserData userData, int userId)
        {
            bool result = false;

            MailNotificationAdapter adapter = GetMailNotificationAdapter(groupId);

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("Mail Notification URL wasn't found");
                return false;
            }

            try
            {
                long unixTimestamp;
                string signature;
                GetClientCallParamters(groupId, adapter, out unixTimestamp, out signature);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AnnouncementListResponse response = client.UnSubscribe(adapter.Id, userData.Email, announcementExternalIds, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (response == null || response.Status == null || response.Status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Error while trying to UnSubscribeToAnnouncement. adpaterId: {0}", adapter.Id);
                    }
                    else
                    {
                        log.DebugFormat("successfully UnSubscribeToAnnouncement. adpaterId: {0}", adapter.Id);
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to UnSubscribe to announcement. unsubscribeList: {0}, ex: {1}", JsonConvert.SerializeObject(announcementExternalIds), ex);
            }

            return result;
        }

        public static bool PublishToAnnouncement(int groupId, string externalAnnouncementId, string subject, List<KeyValuePair<string, string>> mergeVars, string templateId)
        {
            bool result = false;

            MailNotificationAdapter adapter = GetMailNotificationAdapter(groupId);

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("Mail Notification URL wasn't found");
                return result;
            }

            try
            {
                long unixTimestamp;
                string signature;
                GetClientCallParamters(groupId, adapter, out unixTimestamp, out signature);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AdapterStatus response = client.Publish(adapter.Id, externalAnnouncementId, templateId, subject, 
                        mergeVars != null ? mergeVars.Select(mv => new APILogic.MailNotificationsAdapterService.KeyValue() { Key = mv.Key, Value = mv.Value }).ToList() : null, 
                        unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (response == null || response.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Error while trying to PublishToAnnouncement. adpaterId: {0}", adapter.Id);
                    }
                    else
                    {
                        log.DebugFormat("successfully PublishToAnnouncement. adpaterId: {0}", adapter.Id);
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to PublishToAnnouncement. announcement external ID: {0}, mergeVars: {1} ex: {2}", externalAnnouncementId, mergeVars, ex);
            }

            return result;
        }

        public static bool UpdateUserData(int groupId, int userId, UserData oldUserData, UserData NewUserData, List<string> externalAnnouncementIds)
        {
            bool result = false;

            MailNotificationAdapter adapter = GetMailNotificationAdapter(groupId);

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("Mail Notification URL wasn't found");
                return false;
            }
            try
            {
                long unixTimestamp;
                string signature;
                GetClientCallParamters(groupId, adapter, out unixTimestamp, out signature);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AdapterStatus response = client.UpdateUser(adapter.Id, userId, oldUserData.Email,
                        NewUserData.Email, NewUserData.FirstName, NewUserData.LastName, externalAnnouncementIds, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (response == null || response.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Error while trying to UpdateUserData. adpeterId : {0}", adapter.Id);
                    }
                    else
                    {
                        log.DebugFormat("successfully UpdateUserData. adpeterId : {0}", adapter.Id);
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to UpdateUserData. adpeterId : {0}. ex: {1}", adapter.Id, ex);
            }
            
            return result;
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

        private static void GetClientCallParamters(int groupId, MailNotificationAdapter adapter, out long unixTimestamp, out string signature)
        {
            //set unixTimestamp
            unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //set signature
            signature = string.Concat(adapter.Id, adapter.ProviderUrl, adapter.Settings, groupId, unixTimestamp);
        }
    }
}
