using System;
using EventBus.Abstraction;
using Newtonsoft.Json;

namespace ApiObjects.DataMigrationEvents
{
    public enum eMigrationOperation
    {
        Create = 0,
        Update = 1,
        Delete = 2,
        // no Read as in migration we are not interested in read so no CRUD for you just CUD:)
    }

    public class BaseDataMigrationEvent : ServiceEvent
    {
        public override string EventKey => this.GetType().Name;

        [JsonProperty("partnerId")]
        public long PartnerId
        {
            get => GroupId;
            set => GroupId = (int) value;
        }
        
        public eMigrationOperation Operation { get; set; }
        
        
        
    }
}