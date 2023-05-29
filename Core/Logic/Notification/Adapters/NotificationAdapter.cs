using APILogic.AmazonSnsAdapter;
using Phx.Lib.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel;
using TVinciShared;
using ApiLogic.Notification;
using System.Configuration;
using Phx.Lib.Appconfig;

namespace Core.Notification.Adapters
{
    public class NotificationAdapter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static ServiceClient GetAmazonSnsServiceClient(string url)
        {
            var client = new ServiceClient(ServiceClient.EndpointConfiguration.BasicHttpBinding_IService, url);
            client.ConfigureServiceClient(ApplicationConfiguration.Current.AdaptersClientConfiguration.AmazonSnsAdapter);
            return client;
        }


        public static T ParseResponse<T>(HttpResponseMessage httpResponse)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new ConfigurationErrorsException($"No configurations");
                }

                return JsonConvert.DeserializeObject<T>(httpResponse.Content.ReadAsStringAsync().Result);
            }

            log.Error($"Failed to parse http response, status: {httpResponse.StatusCode}, response object: {JsonConvert.SerializeObject(httpResponse)}");
            return default;
        }


        public static string CreateAnnouncement(int groupId, string announcementName, bool isUniqueName = false)
        {
            // create Amazon topic name (without special characters + announcemenId + GID)
            if (isUniqueName)
            {
                announcementName = string.Format("{0}_{1}_{2}", Utils.Base64ForUrlEncode(announcementName), Guid.NewGuid().ToString(), groupId);
            }
            else
            {
                announcementName = string.Format("{0}_{1}", Utils.Base64ForUrlEncode(announcementName), groupId);
            }

            string externalAnnouncementId = string.Empty;

            // validate notification URL exists
            var pushAdapterUrl = NotificationSettings.GetPushAdapterUrl(groupId);
            if (string.IsNullOrEmpty(pushAdapterUrl))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (var client = GetAmazonSnsServiceClient(pushAdapterUrl))
                    {

                        externalAnnouncementId = client.CreateTopicAsync(announcementName).ExecuteAndWait();
                        if (string.IsNullOrEmpty(externalAnnouncementId))
                            log.ErrorFormat("Error while trying to create announcement. announcement Name: {0}, isUniqueName: {1}", announcementName, isUniqueName);
                        else
                            log.DebugFormat("successfully created announcement. announcement Name: {0}, isUniqueName: {1}", announcementName, isUniqueName);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to create announcement. announcement Name: {0}, isUniqueName: {1}, ex: {2}", announcementName, isUniqueName, ex);
                }
            }
            return externalAnnouncementId;
        }

        public static bool DeleteAnnouncement(int groupId, string externalAnnouncementId)
        {
            bool result = false;

            // validate notification URL exists
            var pushAdapterUrl = NotificationSettings.GetPushAdapterUrl(groupId);
            if (string.IsNullOrEmpty(pushAdapterUrl))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (var client = GetAmazonSnsServiceClient(pushAdapterUrl))
                    {

                        result = client.DeleteTopicAsync(externalAnnouncementId).ExecuteAndWait();
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
            var pushAdapterUrl = NotificationSettings.GetPushAdapterUrl(groupId);
            if (string.IsNullOrEmpty(pushAdapterUrl))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (var client = GetAmazonSnsServiceClient(pushAdapterUrl))
                    {

                        // fire request
                        result = client.SubscribeToTopicAsync(announcementSubscriptions).ExecuteAndWait();

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
            var pushAdapterUrl = NotificationSettings.GetPushAdapterUrl(groupId);
            if (string.IsNullOrEmpty(pushAdapterUrl))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (var client = GetAmazonSnsServiceClient(pushAdapterUrl))
                    {
                        // fire request
                        result = client.UnSubscribeToTopicAsync(unsubscribeList).ExecuteAndWait();

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

        public static string PublishToAnnouncement(int groupId, string externalAnnouncementId, string subject, MessageData message)
        {
            string messageId = string.Empty;

            // validate notification URL exists
            var pushAdapterUrl = NotificationSettings.GetPushAdapterUrl(groupId);
            if (string.IsNullOrEmpty(pushAdapterUrl))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (var client = GetAmazonSnsServiceClient(pushAdapterUrl))
                    {

                        messageId = client.PublishToTopicAsync(externalAnnouncementId, subject, message).ExecuteAndWait();
                        if (string.IsNullOrEmpty(messageId))
                            log.ErrorFormat("Error while trying to publish announcement. announcement external ID: {0}, message: {1}", externalAnnouncementId, message);
                        else
                            log.DebugFormat("successfully published announcement. announcement external ID: {0}, messageId: {1}", externalAnnouncementId, messageId);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to publish announcement. announcement external ID: {0}, message: {1} ex: {2}", externalAnnouncementId, message, ex);
                }
            }
            return messageId;
        }

        /// <summary>
        /// Publish message via IOT
        /// </summary>
        /// <param name="groupId">group Id</param>
        /// <param name="externalAnnouncementId">optional (log)</param>
        /// <param name="message">Message body</param>
        /// <param name="topic">With groupId prefix</param>
        /// <returns></returns>
        public static bool IotPublishAnnouncement(int groupId, string message)
        {
            log.DebugFormat($"IotPublishAnnouncement message {message.Substring(0, Math.Min(message.Length, 10))}");
            try
            {
                return IotGrpcClientWrapper.IotClient.Instance.PublishAnnouncement(groupId, message);
            }
            catch (Exception ex)
            {
                log.Error($"Error while trying to publish announcement. message: {message} ex: {ex}");
            }

            return false;
        }

        public static bool AddPrivateMessageToShadowIot(int groupId, string message, string thingArn, string udid)
        {
            var response = false;

            try
            {
                response = IotGrpcClientWrapper.IotClient.Instance.PublishPrivateMessage(groupId, message, thingArn,udid);

                if (!response)
                    log.Error($"Error while trying to add message to thing shadow. group: {groupId}, message: {message}, thing: {thingArn}");
                else
                    log.Debug($"successfully added message to thing shadow");
            }
            catch (Exception ex)
            {
                log.Error($"Error while trying to publish announcement. message: {message} ex: {ex}");
            }

            return response;
        }

        public static List<WSEndPointPublishDataResult> PublishToEndPoint(int groupId, WSEndPointPublishData publishData)
        {
            List<WSEndPointPublishDataResult> publishResult = new List<WSEndPointPublishDataResult>();

            // validate notification URL exists
            var pushAdapterUrl = NotificationSettings.GetPushAdapterUrl(groupId);
            if (string.IsNullOrEmpty(pushAdapterUrl))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (var client = GetAmazonSnsServiceClient(pushAdapterUrl))
                    {

                        publishResult = client.PublishToEndPointAsync(publishData).ExecuteAndWait();
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
            var pushAdapterUrl = NotificationSettings.GetPushAdapterUrl(groupId);
            if (string.IsNullOrEmpty(pushAdapterUrl))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (var client = GetAmazonSnsServiceClient(pushAdapterUrl))
                    {

                        deviceRegistrationResult = client.RegisterDeviceToApplicationAsync(deviceRegistration).ExecuteAndWait();
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
            var pushAdapterUrl = NotificationSettings.GetPushAdapterUrl(groupId);
            if (string.IsNullOrEmpty(pushAdapterUrl))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (var client = GetAmazonSnsServiceClient(pushAdapterUrl))
                    {

                        // fire request
                        result = client
                            .RegisterDeviceToApplicationAndTopicAsync(deviceAppRegistration, announcementToSubscribe, announcementToUnsubscribe)
                            .ExecuteAndWait();

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

        public static bool SendSms(int groupId, string message, string phoneNumber)
        {
            bool success = false;

            // validate notification URL exists
            var pushAdapterUrl = NotificationSettings.GetPushAdapterUrl(groupId);
            if (string.IsNullOrEmpty(pushAdapterUrl))
                log.Error("Notification URL wasn't found");
            else
            {
                try
                {
                    using (var client = GetAmazonSnsServiceClient(pushAdapterUrl))
                    {
                        client.Endpoint.Address = new EndpointAddress(NotificationSettings.GetPushAdapterUrl(groupId));

                        success = client.SendSmsAsync(message, phoneNumber).ExecuteAndWait();

                        log.DebugFormat("Send Sms to phoneNumber: {0}, success: {1}, message: {2}", phoneNumber, success, message);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to send SMS . ex {0}", ex);
                }
            }
            return success;
        }
    }
}
