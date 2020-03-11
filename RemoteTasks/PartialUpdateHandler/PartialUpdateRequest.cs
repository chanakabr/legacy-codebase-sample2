using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using Newtonsoft.Json;

namespace PartialUpdateHandler
{
    public class PartialUpdateRequest
    {

        [JsonProperty("group_id")]
        public int GroupID
        {
            get;
            set;
        }

        [JsonProperty("assets")]
        public AssetsPartialUpdate Assets
        {
            get;
            set;
        }
    }
}
