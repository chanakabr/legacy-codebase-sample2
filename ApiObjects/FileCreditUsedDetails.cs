using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    [Serializable]
    public class FileCreditUsedDetails
    {

        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("ProductCode")]
        public string ProductCode { get; set; }

        [JsonProperty("DateUsed")]
        public long DateUsed { get; set; }

        public FileCreditUsedDetails()
        {
            Id = 0;
            ProductCode = string.Empty;
            DateUsed = 0;
        }

    }    
}
