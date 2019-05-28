using ApiObjects.Notification;
using ApiObjects.Response;
using ConfigurationManager;
using KLogMonitor;
using System;
using System.Reflection;

namespace TvinciImporter
{
    public class NotificationHelper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static private bool UpdateNotificationRequest(int groupid, int nMediaID)
        {
            bool bUpdate = false;
            try
            {
                //Call Notifications WCF service
                string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Notification.URL.Value;
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupid);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);
                bUpdate = service.AddNotificationRequest(sWSUserName, sWSPass, string.Empty, NotificationTriggerType.FollowUpByTag, nMediaID);
            }
            catch (Exception ex)
            {
                log.Error("Exception (UpdateNotificationRequest) - " + string.Format("Media:{0}, groupID:{1}, ex:{2}", nMediaID, groupid, ex.Message), ex);
                return false;
            }
            return bUpdate;
        }

        static public ApiObjects.Response.Status SetMessageTemplate(int groupID, ref ApiObjects.Notification.MessageTemplate messageTemplate)
        {
            ApiObjects.Notification.MessageTemplateResponse response = null;
            try
            {
                //Call Notifications WCF service
                string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Notification.URL.Value;
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupID);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);

                ApiObjects.Notification.MessageTemplate wcfMessageTemplate = new ApiObjects.Notification.MessageTemplate()
                {
                    TemplateType = messageTemplate.TemplateType,
                    Message = messageTemplate.Message,
                    Sound = messageTemplate.Sound,
                    Action = messageTemplate.Action,
                    URL = messageTemplate.URL,
                    Id = messageTemplate.Id,
                    DateFormat = messageTemplate.DateFormat,
                    MailSubject = messageTemplate.MailSubject,
                    MailTemplate = messageTemplate.MailTemplate,
                    RatioId = messageTemplate.RatioId
                };

                response = service.SetMessageTemplate(sWSUserName, sWSPass, wcfMessageTemplate);
                if (response != null && response.Status.Code == (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    messageTemplate = new ApiObjects.Notification.MessageTemplate()
                    {
                        Id = response.MessageTemplate.Id,
                        Message = response.MessageTemplate.Message,
                        DateFormat = response.MessageTemplate.DateFormat,
                        TemplateType = response.MessageTemplate.TemplateType,
                        MailSubject = response.MessageTemplate.MailSubject,
                        MailTemplate = response.MessageTemplate.MailTemplate,
                        Action = response.MessageTemplate.Action,
                        Sound = response.MessageTemplate.Sound,
                        URL = response.MessageTemplate.URL,
                        RatioId = response.MessageTemplate.RatioId
                    };
                }
                return response.Status;
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Error while call service.SetMessageTemplate ex:{0}", exc);
                return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
        }

        public static Status AddEngagement(int groupId, ref Engagement engagement)
        {
            ApiObjects.Notification.EngagementResponse response = null;
            try
            {
                //Call Notifications WCF service
                string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Notification.URL.Value;
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);

                ApiObjects.Notification.Engagement wcfEngagement = new ApiObjects.Notification.Engagement()
                {
                    AdapterDynamicData = engagement.AdapterDynamicData,
                    AdapterId = engagement.AdapterId,
                    EngagementType = engagement.EngagementType,
                    IntervalSeconds = engagement.IntervalSeconds,
                    SendTime = engagement.SendTime,
                    TotalNumberOfRecipients = engagement.TotalNumberOfRecipients,
                    UserList = engagement.UserList,
                    CouponGroupId = engagement.CouponGroupId
                };

                response = service.AddEngagement(sWSUserName, sWSPass, wcfEngagement);
                return response.Status;
            }
            catch
            {
                return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
        }

        public static Status SetEngagementAdapterConfiguration(int groupId, int engagementAdapterId)
        {
            try
            {
                //Call Notifications WCF service
                string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Notification.URL.Value;
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);

                return service.SetEngagementAdapterConfiguration(sWSUserName, sWSPass, engagementAdapterId);
            }
            catch
            {
                return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
        }

        public static Status SetMailNotificationsAdapterConfiguration(int groupId, int adapterId)
        {
            try
            {
                //Call Notifications WCF service
                string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Notification.URL.Value;
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(groupId, "", "notifications", sIP, ref sWSUserName, ref sWSPass);

                return service.SetMailNotificationsAdapterConfiguration(sWSUserName, sWSPass, adapterId);
            }
            catch
            {
                return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
        }

        private static Notification_WCF.NotificationServiceClient CreateNotificationServiceClient(int groupId)
        {
            //Call Notifications WCF service
            string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Notification.URL.Value;
            Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
            if (!string.IsNullOrEmpty(sWSURL))
                service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

            return service;
        }

        public static TopicNotification[] GetGroupTopicNotifications(int groupId)
        {
            TopicNotification[] topics = null;
            try
            {
                //Call Notifications WCF service                
                Notification_WCF.NotificationServiceClient service = CreateNotificationServiceClient(groupId);
                topics = service.GetGroupTopicNotifications(groupId, true);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception (GetGroupTopicNotifications) - groupId:{0}. ex:{1}", groupId, ex);
            }

            return topics;
        }

        public static TopicNotification AddTopicNotification(int groupId, TopicNotification topicNotification, long userId)
        {
            TopicNotification topic = null;
            try
            {
                //Call Notifications WCF service                
                Notification_WCF.NotificationServiceClient service = CreateNotificationServiceClient(groupId);
                topic = service.AddTopicNotification(groupId, topicNotification, userId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception (AddTopicNotification) - groupId:{0}. ex:{1}", groupId, ex);
            }

            return topic;
        }

        public static TopicNotification UpdateTopicNotification(int groupId, TopicNotification topicNotification, long userId)
        {
            TopicNotification topic = null;
            try
            {
                //Call Notifications WCF service                
                Notification_WCF.NotificationServiceClient service = CreateNotificationServiceClient(groupId);
                topic = service.UpdateTopicNotification(groupId, topicNotification, userId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception (AddTopicNotification) - groupId:{0}. ex:{1}", groupId, ex);
            }

            return topic;
        }

        public static TopicNotificationMessage AddTopicNotificationMessage(int groupId, TopicNotificationMessage topicNotificationMessage, long userId)
        {
            TopicNotificationMessage topicMessage = null;
            try
            {
                //Call Notifications WCF service                
                Notification_WCF.NotificationServiceClient service = CreateNotificationServiceClient(groupId);
                topicMessage = service.AddTopicNotificationMessage(groupId, topicNotificationMessage, userId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception (AddTopicNotificationMessage) - groupId:{0}. ex:{1}", groupId, ex);
            }

            return topicMessage;
        }

        public static TopicNotificationMessage UpdateTopicNotificationMessage(int groupId, TopicNotificationMessage topicNotificationMessage, long userId)
        {
            TopicNotificationMessage topicMessage = null;
            try
            {
                //Call Notifications WCF service                
                Notification_WCF.NotificationServiceClient service = CreateNotificationServiceClient(groupId);
                topicMessage = service.UpdateTopicNotificationMessage(groupId, topicNotificationMessage, userId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception (UpdateTopicNotificationMessage) - groupId:{0}. ex:{1}", groupId, ex);
            }

            return topicMessage;
        }

        public static TopicNotificationMessage[] GetTopicNotificationMessages(int groupId, long topicNotificationId, int pageSize, int pageIndex)
        {
            TopicNotificationMessage[] topicMessages = null;
            try
            {
                //Call Notifications WCF service                
                Notification_WCF.NotificationServiceClient service = CreateNotificationServiceClient(groupId);
                topicMessages = service.GetTopicNotificationMessages(groupId, topicNotificationId, pageSize, pageIndex);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception (GetTopicNotificationMessages) - groupId:{0}. ex:{1}", groupId, ex);
            }

            return topicMessages;
        }

        public static TopicNotification GetTopicNotification(int groupId, long topicNotificationId)
        {
            TopicNotification topic = null;
            try
            {
                //Call Notifications WCF service                
                Notification_WCF.NotificationServiceClient service = CreateNotificationServiceClient(groupId);
                topic = service.GetTopicNotification(groupId, topicNotificationId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception (GetTopicNotification) - groupId:{0}. ex:{1}", groupId, ex);
            }

            return topic;
        }
    }
}