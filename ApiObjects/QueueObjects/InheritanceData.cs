using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using Newtonsoft.Json;

namespace ApiObjects
{
    public enum InheritanceType
    {
        AssetStructMeta = 0,
        ParentUpdate = 1
    }

    public class InheritanceData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_asset_inharitance";

        public string Data;
        public InheritanceType Type;
        public long UserId;

        public InheritanceData(int groupId, InheritanceType type, string data, long userId) :
            base(Guid.NewGuid().ToString(), TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.Type = type;
            this.Data = data;
            this.UserId = userId;

            this.args = new List<object>()
            {
                groupId,
                userId,
                type,
                data,
                base.RequestId
            };
        }
    }

    [Serializable]
    public class InheritanceAssetStructMeta
    {
        [JsonProperty("AssetStructId")]
        public long AssetStructId;

        [JsonProperty("MetaId")]
        public long MetaId;
    }

    [Serializable]
    public class InheritanceParentUpdate
    {        
        [JsonProperty("AssetId")]
        public long AssetId;

        [JsonProperty("TopicsIds")]
        public List<long> TopicsIds;
    }
}
