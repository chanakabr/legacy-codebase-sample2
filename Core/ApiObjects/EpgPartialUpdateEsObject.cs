using System;

namespace ApiObjects
{
    public class EpgPartialUpdateEsObject
    {
        public ulong EpgId { get; set; }

        public DateTime StartDate { get; set; }

        public string Language { get; set; }

        public EpgEs EpgPartial { get; set; }
    }
}
