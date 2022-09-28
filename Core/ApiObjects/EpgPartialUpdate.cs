using System;

namespace ApiObjects
{
    public class EpgPartialUpdate
    {
        public ulong EpgId { get; set; }
        public string DocumentId { get; set; }

        public DateTime StartDate { get; set; }

        public string Language { get; set; }

        public EpgPartial EpgPartial { get; set; }
    }

    public class EpgPartial
    {
        public int[] Regions { get; set; }
    }
}
