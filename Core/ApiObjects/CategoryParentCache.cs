using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace ApiObjects
{
    [Serializable]
    public class CategoryParentCache
    {
        #region Data Members
        
        [JsonProperty()]
        public long ParentId { get; set; }

        [JsonProperty()]
        public int Order { get; set; }

        [JsonProperty()]
        public long? VersionId { get; set; }

        #endregion

        #region Ctor

        public CategoryParentCache()
        {
            
        }

        #endregion
    }
}
