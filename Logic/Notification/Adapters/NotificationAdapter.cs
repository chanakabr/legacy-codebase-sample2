using APILogic.AmazonSnsAdapter;
using KLogMonitor;
using Newtonsoft.Json;
using Core.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace Core.Notification.Adapters
{
    public class NotificationAdapter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static string CreateAnnouncement(int groupId, string announcementName)
        {
            // create Amazon topic name (without special characters + GID)
            announcementName = string.Format("{0}_{1}", Utils.Base64ForUrlEncode(announcementName), groupId);

            string externalAnnouncementId = string.Empty;

            // validate notification URL exists
            if (string.IsNullOrEmpty(NotificationSettings.GetPushAdapterUrl(groupId)))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (ServiceClient client = new ServiceClient())
                    {
                        client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                        externalAnnouncementId = client.CreateTopic(announcementName);
                        if (string.IsNullOrEmpty(externalAnnouncementId))
                            log.ErrorFormat("Error while trying to create announcement. announcement Name: {0}", announcementName);
                        else
                            log.DebugFormat("successfully created announcement. announcement Name: {0}", announcementName);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to create announcement. announcement Name: {0}, ex: {1}", announcementName, ex);
                }
            }
            return externalAnnouncementId;
        }

        public static bool DeleteAnnouncement(int groupId, string externalAnnouncementId)
        {
            bool result = false;

            // validate notification URL exists
            if (string.IsNullOrEmpty(NotificationSettings.GetPushAdapterUrl(groupId)))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (ServiceClient client = new ServiceClient())
                    {
                        client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                        result = client.DeleteTopic(externalAnnouncementId);
                        if (!result)
                            log.ErrorFormat("Error while trying to delete announcement. announcement external ID: {0}", externalAnnouncementId);
                        else
                            log.DebugFormat("successfully deleted announcement. announcement external ID: {0}", externalAnnouncementId);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to delete announcement. announcement external ID: {0}, ex: {1}", externalAnnouncementId, ex);
                }
            }
            return result;
        }

        public static List<AnnouncementSubscriptionData> SubscribeToAnnouncement(int groupId, List<AnnouncementSubscriptionData> announcementSubscriptions)
        {
            List<AnnouncementSubscriptionData> result = null;

            // validate notification URL exists
            if (string.IsNullOrEmpty(NotificationSettings.GetPushAdapterUrl(groupId)))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (ServiceClient client = new ServiceClient())
                    {
                        client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                        // fire request
                        result = client.SubscribeToTopic(announcementSubscriptions);

                        // validate response
                        if ((result == null ||
                            result.Count == 0) ||
                            result.Count != announcementSubscriptions.Count)
                        {
                            log.ErrorFormat("Error while trying to subscribe to announcement. announcementSubscriptions: {0}", JsonConvert.SerializeObject(announcementSubscriptions));
                        }
                        else
                        {
                            var failedSubscriptions = result.Where(x => string.IsNullOrEmpty(x.SubscriptionArnResult));
                            if (failedSubscriptions.Count() > 0)
                                log.ErrorFormat("Some of the subscription failed. failed subs: {0}", JsonConvert.SerializeObject(failedSubscriptions));
                            else
                                log.Debug("Announcement subscription passed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to subscribe to announcement. announcementSubscriptions: {0}, ex: {1}", JsonConvert.SerializeObject(announcementSubscriptions), ex);
                }
            }
            return result;
        }

        public static List<UnSubscribe> UnSubscribeToAnnouncement(int groupId, List<UnSubscribe> unsubscribeList)
        {
            List<UnSubscribe> result = null;

            // validate notification URL exists
            if (string.IsNullOrEmpty(NotificationSettings.GetPushAdapterUrl(groupId)))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (ServiceClient client = new ServiceClient())
                    {
                        client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                        // fire request
                        result = client.UnSubscribeToTopic(unsubscribeList);

                        // validate response
                        if ((result == null ||
                            result.Count == 0) ||
                            result.Count != unsubscribeList.Count)
                        {
                            log.ErrorFormat("Error while trying to subscribe to unsubscribe announcements. unsubscribeList: {0}", JsonConvert.SerializeObject(unsubscribeList));
                        }
                        else
                        {
                            var failedUnsubscriptions = result.Where(x => !x.Success);
                            if (failedUnsubscriptions.Count() > 0)
                                log.ErrorFormat("Some of the unsubscribe request failed. failed subs: {0}", JsonConvert.SerializeObject(failedUnsubscriptions));
                            else
                                log.Debug("Announcement unsubscribe passed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to unsubscribe to announcement. unsubscribeList: {0}, ex: {1}", JsonConvert.SerializeObject(unsubscribeList), ex);
                }
            }
            return result;
        }

        public static string PublishToAnnouncement(int groupId,string externalAnnouncementId, string subject, MessageData message)
        {
            string messageId = string.Empty;

            // validate notification URL exists
            if (string.IsNullOrEmpty(NotificationSettings.GetPushAdapterUrl(groupId)))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (ServiceClient client = new ServiceClient())
                    {
                        client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                        messageId = client.PublishToTopic(externalAnnouncementId, subject, message);
                        if (string.IsNullOrEmpty(messageId))
                            log.ErrorFormat("Error while trying to publish announcement. announcement external ID: {0}, message: {1}", externalAnnouncementId, message);
                        else
                            log.DebugFormat("successfully published announcement. announcement external ID: {0}, message: {1}", externalAnnouncementId, message);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to publish announcement. announcement external ID: {0}, message: {1} ex: {2}", externalAnnouncementId, message, ex);
                }
            }
            return messageId;
        }

        public static List<WSEndPointPublishDataResult> PublishToEndPoint(int groupId,WSEndPointPublishData publishData)
        {
            List<WSEndPointPublishDataResult> publishResult = new List<WSEndPointPublishDataResult>();

            // validate notification URL exists
            if (string.IsNullOrEmpty(NotificationSettings.GetPushAdapterUrl(groupId)))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (ServiceClient client = new ServiceClient())
                    {
                        client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                        publishResult = client.PublishToEndPoint(publishData);
                        if (publishResult == null ||
                            publishResult.Count == 0 ||
                            publishResult.Count != publishData.EndPoints.Count)
                        {
                            log.ErrorFormat("Error while trying to publish to endpoint. message: {0}, endPoints: {1}",
                                publishData != null && publishData.Message != null ? JsonConvert.SerializeObject(publishData.Message) : string.Empty,
                                publishData != null && publishData.EndPoints != null ? JsonConvert.SerializeObject(publishData.EndPoints) : string.Empty);
                        }
                        else
                        {
                            var failedPublish = publishResult.Where(x => string.IsNullOrEmpty(x.ResultMessageId));
                            if (failedPublish.Count() > 0)
                                log.ErrorFormat("Some of the publish requests failed. failed endpoints: {0}", JsonConvert.SerializeObject(failedPublish));
                            else
                                log.Debug("publish to endpoints passed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to publish to endpoint. ex {0}", ex);
                }
            }
            return publishResult;
        }

        public static DeviceAppRegistration RegisterDeviceToApplication(int groupId, DeviceAppRegistration deviceRegistration)
        {
            DeviceAppRegistration deviceRegistrationResult = null;

            // validate notification URL exists
            if (string.IsNullOrEmpty(NotificationSettings.GetPushAdapterUrl(groupId)))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (ServiceClient client = new ServiceClient())
                    {
                        client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                        deviceRegistrationResult = client.RegisterDeviceToApplication(deviceRegistration);
                        if (deviceRegistrationResult == null || string.IsNullOrEmpty(deviceRegistrationResult.PlatformApplicationArn))
                        {
                            log.ErrorFormat("Error while trying to register device to application. deviceRegistration: {0}",
                                JsonConvert.SerializeObject(deviceRegistration));
                        }
                        else
                            log.Debug("device registration passed successfully");
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to register device to application. deviceRegistration: {0}, ex: {1}",
                        JsonConvert.SerializeObject(deviceRegistration),
                        ex);
                }
            }
            return deviceRegistrationResult;
        }

        public static DeviceAppRegistrationAnnouncementResult RegisterDeviceToApplicationAndAnnouncement(int groupId, DeviceAppRegistration deviceAppRegistration, List<AnnouncementSubscriptionData> announcementToSubscribe, List<UnSubscribe> announcementToUnsubscribe)
        {
            DeviceAppRegistrationAnnouncementResult result = new DeviceAppRegistrationAnnouncementResult();

            string logString = string.Format("Method input data - deviceAppRegistration: {0}, announcementToSubscribe: {1}, announcementToUnsubscribe: {2}",
                    deviceAppRegistration != null ? JsonConvert.SerializeObject(deviceAppRegistration) : string.Empty,
                    announcementToSubscribe != null ? JsonConvert.SerializeObject(announcementToSubscribe) : string.Empty,
                    announcementToUnsubscribe != null ? JsonConvert.SerializeObject(announcementToUnsubscribe) : string.Empty);

            // validate notification URL exists
            if (string.IsNullOrEmpty(NotificationSettings.GetPushAdapterUrl(groupId)))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (ServiceClient client = new ServiceClient())
                    {
                        client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                        // fire request
                        result = client.RegisterDeviceToApplicationAndTopic(deviceAppRegistration, announcementToSubscribe, announcementToUnsubscribe);

                        if (result == null)
                        {
                            log.ErrorFormat("Error while trying to register + subscribe + unsubscribe notification. {0}", logString);
                            return result;
                        }

                        // validate device registration result
                        if (deviceAppRegistration != null && string.IsNullOrEmpty(result.EndPointArn))
                        {
                            log.ErrorFormat("Error while trying to register device. {0}", logString);
                            return null;
                        }

                        // validate subscription result
                        if (announcementToSubscribe != null &&
                            announcementToSubscribe.Count > 0 &&
                            (result.AnnounsmentsSubscriptions == null ||
                            result.AnnounsmentsSubscriptions.Count == 0))
                        {
                            log.ErrorFormat("Error while trying to subscribe announcement (registration to device + subscribe to announcements flow). {0}", logString);
                        }

                        // validate unsubscribe result
                        if (announcementToUnsubscribe != null &&
                            announcementToUnsubscribe.Count > 0 &&
                            (result.AnnounsmentsCancelledSubscriptions == null ||
                            result.AnnounsmentsCancelledSubscriptions.Count == 0))
                        {
                            log.ErrorFormat("Error while trying to unsubscribe topic (registration to device + cancel subscription flow). {0}", logString);
                        }

                        log.Debug("device registration and announcement subscription passed");
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to register device to application and subscribe to announcements. {0} ex: {1}", logString, ex);
                }
            }
            return result;
        }
    }
}
