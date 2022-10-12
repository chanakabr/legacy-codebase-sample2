using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GrpcClientCommon;
using iot;
using phoenix;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;

namespace IotGrpcClientWrapper
{
    public interface IIotClient
    {
        void RegisterDevice(int groupId, long domainId, string udid);
        bool PublishAnnouncement(int groupId, string message);
        bool PublishPrivateMessage(int groupId, string message, string thingArn, string udid);

        GetClientConfigurationResponse GetClientConfiguration(int groupId, long domainId, int regionId,
            string udid);

        Task SendNotificationAsync(int groupId, string message, EventNotificationType eventType,
            List<int> regions);
    }
    public class IotClient : IIotClient
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());
        private readonly Iot.IotClient _client;

        private static readonly Lazy<IIotClient> LazyInstance =
            new Lazy<IIotClient>(() => new IotClient(), LazyThreadSafetyMode.PublicationOnly);

        public static readonly IIotClient Instance = LazyInstance.Value;

        private IotClient()
        {
            var address = ApplicationConfiguration.Current.MicroservicesClientConfiguration.Iot.Address.Value;
            var certFilePath =
                ApplicationConfiguration.Current.MicroservicesClientConfiguration.Iot.CertFilePath.Value;
            var retryCount = ApplicationConfiguration.Current.MicroservicesClientConfiguration.Iot.RetryCount.Value;
            _client = new Iot.IotClient(GrpcCommon.CreateChannel(address, certFilePath, retryCount));
        }

        public void RegisterDevice(int groupId, long domainId, string udid)
        {
            Logger.DebugFormat($"RegisterDevice groupId {groupId} domainId {domainId} udid {udid}");
            _client.RegisterDevice(new RegisterDeviceRequest()
            {
                GroupId = groupId,
                DomainId = domainId,
                Udid = udid
            });
        }

        public bool PublishAnnouncement(int groupId, string message)
        {
            Logger.DebugFormat($"PublishAnnouncement groupId {groupId} message {message.Substring(0, Math.Min(message.Length, 10))}");
            var response = _client.PublishAnnouncement(new PublishAnnouncementRequest()
            {
                GroupId = groupId,
                Message = message
            });
            return response.Value;
        }

        public bool PublishPrivateMessage(int groupId, string message, string thingArn, string udid)
        {
            Logger.DebugFormat($"PublishPrivateMessage groupId {groupId} message {message.Substring(0, Math.Min(message.Length, 10))} thingArn {thingArn} udid {udid}");
            var response =  _client.PublishPrivateMessage(new PublishPrivateMessageRequest()
            {
                GroupId = groupId,
                Message = message,
                ThingArn = thingArn,
                Udid = udid
            });
            return response.Value;
        }

        public GetClientConfigurationResponse GetClientConfiguration(int groupId, long domainId, int regionId,
            string udid)
        {
            return _client.GetClientConfiguration(new GetClientConfigurationRequest()
            {
                GroupId = groupId,
                DomainId = domainId,
                RegionId = regionId,
                Udid = udid
            });
        }
        
        public async Task SendNotificationAsync(int groupId, string message, EventNotificationType eventType, List<int> regions)
        {
            Logger.DebugFormat($"SendNotificationAsync groupId {groupId} message {message.Substring(0, Math.Min(message.Length, 10))}");
            await _client.SendNotificationAsync(new SendNotificationRequest()
            {
                GroupId = groupId,
                Message = message,
                EventType = eventType,
                Regions = { regions }
            });
        }
    }
}