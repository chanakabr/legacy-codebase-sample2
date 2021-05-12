using ApiObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTO
{
    public class UnifiedChannelInfoDTO : UnifiedChannel
    {
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
