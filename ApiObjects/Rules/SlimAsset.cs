using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects.Rules
{
    public class SlimAsset
    {
        public string Id { get; set; }
        public eAssetTypes Type { get; set; }

        public SlimAsset()
        {
        }

        public SlimAsset(string id, eAssetTypes assetTypes)
        {
            this.Id = id;
            this.Type = assetTypes;
        }
    }
}
