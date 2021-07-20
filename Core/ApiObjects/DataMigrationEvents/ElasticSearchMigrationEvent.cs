using System;
using Newtonsoft.Json;
using EventBus.Abstraction;

namespace ApiObjects.DataMigrationEvents
{
    public sealed class ElasticSearchMigrationEvent : ServiceEvent
    {
        private string _eventKey;
        private const string EVENT_NAME_OVERRIDE = "OTT_MIGRATION_ELASTIC_SEARCH";
        private const string EVENT_KEY_PARTITION = "OTT_MIGRATION_ELASTIC_SEARCH_PARTITION";

        public ElasticSearchMigrationEvent(string eventNameTopic, int partnerId)
        {
            EventNameOverride = $"{EVENT_NAME_OVERRIDE}_{eventNameTopic}";
            _eventKey = EVENT_KEY_PARTITION;
            GroupId = partnerId;
        }
        
        [JsonProperty("parameters")]
        public object[] Parameters
        {
            get;
            set;
        }

        [JsonProperty("methodName")]
        public string MethodName
        {
            get;
            set;
        }
        
        
        public override string EventKey
        {
            get => _eventKey;
            set
            {
                if (value == null) throw new ArgumentNullException("Event key cannot be empty");
                _eventKey = $"{EVENT_KEY_PARTITION}_{value}_{GroupId}";
            }
        }
    }
}