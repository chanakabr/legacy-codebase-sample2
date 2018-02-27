using ApiObjects.Notification;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using Synchronizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Core.Notification
{
    public class MailNotificationAdapterClient
    {
        #region Consts

        private const int STATUS_OK = 0;
        private const int STATUS_NO_CONFIGURATION_FOUND = 3;

        private const string PARAMETER_GROUP_ID = "group_id";
        private const string PARAMETER_ADAPTER = "adapter";

        #endregion

        #region Data members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static CouchbaseSynchronizer configurationSynchronizer;

        #endregion

        static MailNotificationAdapterClient()
        {
            configurationSynchronizer = new CouchbaseSynchronizer(100);
            configurationSynchronizer.SynchronizedAct += configurationSynchronizer_SynchronizedAct;
        }

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
                string signature = string.Concat(adapter.Id, adapter.Settings, groupId, unixTimestamp);

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
            announcementName = string.Format("{0}_{1}", announcementName, (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);

            string externalMailAnnouncementId = string.Empty;

            MailNotificationAdapter adapter = GetMailNotificationAdapter(groupId);
            if (adapter == null)
                return null;

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("Mail Notification URL wasn't found");
                return externalMailAnnouncementId;
            }

            try
            {
                //set unixTimestamp
                long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(adapter.Id, announcementName, unixTimestamp);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AnnouncementResponse adapterResponse = client.CreateAnnouncement(adapter.Id, announcementName, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null && adapterResponse.Status != null &&
                        adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                    {
                        #region Send Configuration if not found

                        string key = string.Format("MailNotification_Adapter_Locker_{0}", adapter.Id);

                        // Build dictionary for synchronized action
                        Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                        configurationSynchronizer.DoAction(key, parameters);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                        {
                            try
                            {
                                //call Adapter - after it is configured
                                adapterResponse = client.CreateAnnouncement(adapter.Id, announcementName, unixTimestamp,
                                    System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                            }
                            catch (Exception ex)
                            {
                                ReportAdapterError(adapter.Id, adapter.AdapterUrl, ex, "CreateAnnouncement");
                            }
                        }

                        #endregion
                    }

                    if (adapterResponse == null || string.IsNullOrEmpty(adapterResponse.AnnouncementExternalId))
                    {
                        log.ErrorFormat("Error while trying to create announcement. announcement Name: {0}", announcementName);
                    }
                    else
                    {
                        log.DebugFormat("successfully created announcement. announcement Name: {0}", announcementName);
                        externalMailAnnouncementId = adapterResponse.AnnouncementExternalId;
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

            if (adapter == null)
                return false;

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("mail Notification URL wasn't found");
                return false;
            }

            try
            {
                //set unixTimestamp
                long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(adapter.Id, externalAnnouncementId, unixTimestamp);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AdapterStatus adapterResponse = client.DeleteAnnouncement(adapter.Id, externalAnnouncementId, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null &&
                        adapterResponse.Code == STATUS_NO_CONFIGURATION_FOUND)
                    {
                        #region Send Configuration if not found

                        string key = string.Format("MailNotification_Adapter_Locker_{0}", adapter.Id);

                        // Build dictionary for synchronized action
                        Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                        configurationSynchronizer.DoAction(key, parameters);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                        {
                            try
                            {
                                //call Adapter - after it is configured
                                adapterResponse = client.DeleteAnnouncement(adapter.Id, externalAnnouncementId, unixTimestamp,
                                    System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                            }
                            catch (Exception ex)
                            {
                                ReportAdapterError(adapter.Id, adapter.AdapterUrl, ex, "DeleteAnnouncement");
                            }
                        }

                        #endregion
                    }

                    if (adapterResponse == null || adapterResponse.Code != (int)eResponseStatus.OK)
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

            if (adapter == null)
                return false;

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("Mail Notification URL wasn't found");
                return false;
            }

            try
            {
                string token = null;
                if (!Utils.CreateUserToken(groupId, userId, out token))
                {
                    log.ErrorFormat("Failed to create user token for userId: {0}", userId);
                    return false;
                }

                //set unixTimestamp
                long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(adapter.Id, userData.FirstName, userData.LastName, userData.Email, token,
                    announcementExternalIds != null ? string.Join("", announcementExternalIds) : string.Empty, unixTimestamp);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AnnouncementListResponse adapterResponse = client.Subscribe(adapter.Id, userData.FirstName, userData.LastName, userData.Email,
                        token, announcementExternalIds, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null && adapterResponse.Status != null &&
                        adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                    {
                        #region Send Configuration if not found

                        string key = string.Format("MailNotification_Adapter_Locker_{0}", adapter.Id);

                        // Build dictionary for synchronized action
                        Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                        configurationSynchronizer.DoAction(key, parameters);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                        {
                            try
                            {
                                //call Adapter - after it is configured
                                adapterResponse = client.Subscribe(adapter.Id, userData.FirstName, userData.LastName, userData.Email, token, announcementExternalIds, unixTimestamp,
                                    System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                            }
                            catch (Exception ex)
                            {
                                ReportAdapterError(adapter.Id, adapter.AdapterUrl, ex, "Subscribe");
                            }
                        }

                        #endregion
                    }

                    if (adapterResponse == null || adapterResponse.Status == null || adapterResponse.Status.Code != (int)eResponseStatus.OK)
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

            if (adapter == null)
                return false;

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("Mail Notification URL wasn't found");
                return false;
            }

            try
            {
                //set unixTimestamp
                long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(adapter.Id, userData.Email, announcementExternalIds != null ? string.Join("", announcementExternalIds) : string.Empty, unixTimestamp);


                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AnnouncementListResponse adapterResponse = client.UnSubscribe(adapter.Id, userData.Email, announcementExternalIds, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null && adapterResponse.Status != null &&
                        adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                    {
                        #region Send Configuration if not found

                        string key = string.Format("MailNotification_Adapter_Locker_{0}", adapter.Id);

                        // Build dictionary for synchronized action
                        Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                        configurationSynchronizer.DoAction(key, parameters);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                        {
                            try
                            {
                                //call Adapter - after it is configured
                                adapterResponse = client.UnSubscribe(adapter.Id, userData.Email, announcementExternalIds, unixTimestamp,
                                    System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                            }
                            catch (Exception ex)
                            {
                                ReportAdapterError(adapter.Id, adapter.AdapterUrl, ex, "UnSubscribe");
                            }
                        }

                        #endregion
                    }

                    if (adapterResponse == null || adapterResponse.Status == null || adapterResponse.Status.Code != (int)eResponseStatus.OK)
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

            if (adapter == null)
                return false;

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("Mail Notification URL wasn't found");
                return result;
            }

            try
            {
                //set unixTimestamp
                long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(adapter.Id, externalAnnouncementId, templateId, subject,
                    mergeVars != null ? string.Concat(mergeVars.Select(kv => string.Concat(kv.Key, kv.Value))) : string.Empty,
                    unixTimestamp);

                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AdapterStatus adapterResponse = client.Publish(adapter.Id, externalAnnouncementId, templateId, subject,
                        mergeVars != null ? mergeVars.Select(mv => new APILogic.MailNotificationsAdapterService.KeyValue() { Key = mv.Key, Value = mv.Value }).ToList() : null,
                        unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null &&
                        adapterResponse.Code == STATUS_NO_CONFIGURATION_FOUND)
                    {
                        #region Send Configuration if not found

                        string key = string.Format("MailNotification_Adapter_Locker_{0}", adapter.Id);

                        // Build dictionary for synchronized action
                        Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                        configurationSynchronizer.DoAction(key, parameters);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                        {
                            try
                            {
                                //call Adapter - after it is configured
                                adapterResponse = client.Publish(adapter.Id, externalAnnouncementId, templateId, subject,
                                    mergeVars != null ? mergeVars.Select(mv => new APILogic.MailNotificationsAdapterService.KeyValue() { Key = mv.Key, Value = mv.Value }).ToList() : null,
                                    unixTimestamp,
                                    System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                            }
                            catch (Exception ex)
                            {
                                ReportAdapterError(adapter.Id, adapter.AdapterUrl, ex, "Publish");
                            }
                        }

                        #endregion
                    }

                    if (adapterResponse == null || adapterResponse.Code != (int)eResponseStatus.OK)
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

            if (adapter == null)
                return false;

            // validate notification URL exists
            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                log.Error("Mail Notification URL wasn't found");
                return false;
            }

            string token;
            if (!Utils.CreateUserToken(groupId, userId, out token))
            {
                log.ErrorFormat("Failed to create user token for userId: {0}", userId);
                return false;
            }

            try
            {
                //set unixTimestamp
                long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(adapter.Id, userId, oldUserData.Email, NewUserData.Email, NewUserData.FirstName, NewUserData.LastName, token,
                     externalAnnouncementIds != null ? string.Join("", externalAnnouncementIds) : string.Empty, unixTimestamp);


                using (APILogic.MailNotificationsAdapterService.ServiceClient client = new APILogic.MailNotificationsAdapterService.ServiceClient(string.Empty, adapter.AdapterUrl))
                {
                    APILogic.MailNotificationsAdapterService.AdapterStatus adapterResponse = client.UpdateUser(adapter.Id, userId, oldUserData.Email,
                        NewUserData.Email, NewUserData.FirstName, NewUserData.LastName, token, externalAnnouncementIds, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null &&
                        adapterResponse.Code == STATUS_NO_CONFIGURATION_FOUND)
                    {
                        #region Send Configuration if not found

                        string key = string.Format("MailNotification_Adapter_Locker_{0}", adapter.Id);

                        // Build dictionary for synchronized action
                        Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                        configurationSynchronizer.DoAction(key, parameters);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                        {
                            try
                            {
                                //call Adapter - after it is configured
                                adapterResponse = client.UpdateUser(adapter.Id, userId, oldUserData.Email,
                                    NewUserData.Email, NewUserData.FirstName, NewUserData.LastName, token, externalAnnouncementIds, unixTimestamp,
                                    System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(adapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                            }
                            catch (Exception ex)
                            {
                                ReportAdapterError(adapter.Id, adapter.AdapterUrl, ex, "UpdateUser");
                            }
                        }

                        #endregion
                    }

                    if (adapterResponse == null || adapterResponse.Code != (int)eResponseStatus.OK)
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
                return null;
            }

            return mailNotificationAdapter;
        }

        private static bool configurationSynchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = false;

            if (parameters != null)
            {
                int partnerId = 0;
                MailNotificationAdapter adapter = null;

                if (parameters.ContainsKey(PARAMETER_GROUP_ID))
                {
                    partnerId = (int)parameters[PARAMETER_GROUP_ID];
                }

                if (parameters.ContainsKey(PARAMETER_ADAPTER))
                {
                    adapter = (MailNotificationAdapter)parameters[PARAMETER_ADAPTER];
                }

                // get the right configuration
                result = SendConfigurationToAdapter(partnerId, adapter);
            }

            return result;
        }
        private static void ReportAdapterError(long adapterId, string url, Exception ex, string action, bool throwException = true)
        {
            var previousTopic = HttpContext.Current.Items[Constants.TOPIC];
            HttpContext.Current.Items[Constants.TOPIC] = "C-DVR adapter";

            log.ErrorFormat("Failed communicating with adapter. Adapter identifier: {0}, Adapter URL: {1}, Adapter Api: {2}. Error: {3}",
                adapterId,
                url,
                action,
                ex);
            HttpContext.Current.Items[Constants.TOPIC] = previousTopic;

            if (throwException)
            {
                throw ex;
            }
        }

    }
}
