using System;

namespace Core.Pricing
{
    public class AssetFilePpv
    {
        public long AssetFileId { get; set; }

        public long PpvModuleId { get; set; }

        public DateTime? StartDate{ get; set; }

        public DateTime? EndDate{ get; set; }
    }
}
