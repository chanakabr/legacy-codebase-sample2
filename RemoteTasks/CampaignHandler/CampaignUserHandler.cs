using ApiObjects.EventBus;
using CouchbaseManager;
using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ApiObjects.Base;
using ApiLogic.Users.Managers;

namespace CampaignHandler
{
    public class CampaignUserHandler : IServiceEventHandler<CampaignUserEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        //private readonly CouchbaseManager.CouchbaseManager _CouchbaseManager = null;

        public CampaignUserHandler()
        {
            //_CouchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
        }

        public Task Handle(CampaignUserEvent serviceEvent)
        {
            _Logger.Debug($"Starting CampaignUserHandler requestId:[{serviceEvent.RequestId}], Id:[{serviceEvent.Id}]");
            var contextData = new ContextData(serviceEvent.GroupId) { UserId = serviceEvent.UserId };
            //Get list of group campaigns
            var campaigns = CampaignManager.Instance.List(contextData, new object());
            throw new NotImplementedException();
        }
    }
}
