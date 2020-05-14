using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.Catalog
{
    public class UnifiedChannel
    {
        public long Id { get; set; }

        public UnifiedChannelType Type { get; set; }


    }

    public partial class UnifiedChannelInfo : UnifiedChannel
    {
        public string Name { get; set; }
    }

    public enum UnifiedChannelType
    {
        Internal,
        External
    }
}
