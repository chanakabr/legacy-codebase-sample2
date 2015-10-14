using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    [JsonObject(Id = "id")]
    public class RecommendationEngineBase
    {
        [DataMember]
        [JsonProperty()]
        public int ID
        {
            get;
            set;
        }

        [DataMember]
        [JsonProperty()]
        public string Name
        {
            get;
            set;
        }

        public RecommendationEngineBase()
        {
        }

        public RecommendationEngineBase(RecommendationEngineBase recommendationEngineBase)
        {
            this.ID = recommendationEngineBase.ID;
            this.Name = recommendationEngineBase.Name;
        }

        public RecommendationEngineBase(int id, string name, bool isDefault)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}
