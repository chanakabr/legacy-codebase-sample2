using System;
using System.Collections.Generic;

namespace ApiObjects
{
    public class EpgEs
    {
        public ulong? EpgID { get; set; }
        
        public int? GroupId { get; set; }
        
        public int? ChannelId { get; set; }
        
        public bool? IsActive { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }

        public DateTime? CacheDate { get; set; }
        
        public DateTime? CreateDate { get; set; }
        
        public DateTime? UpdateDate { get; set; }
        
        public DateTime? SearchEndDate { get; set; }
        
        public string Crid { get; set; }
        
        public string EpgIdentifier { get; set; }
        
        public string ExternalId { get; set; }
        
        public string DocumentId { get; set; }
        
        public bool? IsAutoFill { get; set; }
        
        public int? EnableCDVR { get; set; }
        
        public int? EnableCatchUp { get; set; }
        
        public string Suppressed { get; set; }
        
        public long? LinearMediaId { get; set; }

        public DateTime? DateRouting { get; set; }
        
        public IDictionary<string, List<string>> Metas { get; set; }
        
        public IDictionary<string, List<string>> Tags { get; set; }

        public int[] Regions { get; set; }

        public EpgEs()
        {

        }

        public EpgEs(EpgPartial source)
        {
            this.Regions = source.Regions;
        }
    }
}