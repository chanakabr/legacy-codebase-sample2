using Newtonsoft.Json;
using System;

namespace DAL.DTO
{
    public class UnifiedChannelDTO
    {
        public long Id { get; set; }

        public UnifiedChannelTypeDTO Type { get; set; }
    }

    public enum UnifiedChannelTypeDTO
    {
        Internal,
        External
    }
}