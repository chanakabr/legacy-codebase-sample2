using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    [Serializable]
    public class DomainQuota
    {
        [JsonProperty("total")]
        public int Total { get; set; } // in seconds 
        
        [JsonProperty("used")]
        public int Used { get; set; } // in seconds 

        [JsonIgnore]
        public bool IsDefaultQuota { get; set; } // in seconds 

        public DomainQuota()
        {
        }
        public DomainQuota(int total)
        {
            this.Total = total;
            this.Used = 0;
            this.IsDefaultQuota = false;
        }
        public DomainQuota(int total, int used, bool isDefaultQuota)
        {
            this.Total = total;
            this.Used = used;
            this.IsDefaultQuota = isDefaultQuota;
        }

        public override string ToString()
        {
            return string.Format("Total:{0}, Used:{1}, IsDefaultQuota:{2}", this.Total, this.Used, this.IsDefaultQuota);
        }
    }
}
