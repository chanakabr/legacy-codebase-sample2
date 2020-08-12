using ApiObjects;
using Newtonsoft.Json;
using System;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using ApiObjects.EventBus;
using KLogMonitor;

namespace WebAPI.EventNotifications
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class CampaignHandler : NotificationAction
    {
        [JsonProperty("campaign_id")]
        public long CampaignId { get; set; }

        public CampaignHandler() : base() { }

        internal override void Handle(EventManager.KalturaEvent kalturaEvent, KalturaNotification theObject)
        {
            KalturaObjectEvent objectEvent = kalturaEvent as KalturaObjectEvent;

            var serviceEvent = new CampaignUserEvent()
            {
                RequestId = KLogger.GetRequestId(),
                GroupId = kalturaEvent.PartnerId,
                CampaignId = this.CampaignId,
                EventObject = objectEvent.Object,
                DomainId = (int)Utils.HouseholdUtils.GetHouseholdIDByKS(kalturaEvent.PartnerId)
            };

            var publisher = EventBus.RabbitMQ.EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration();
            publisher.Publish(serviceEvent);
        }
    }
}