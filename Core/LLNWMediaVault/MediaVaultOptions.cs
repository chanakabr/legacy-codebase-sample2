using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.llnw.mediavault
{
    public class MediaVaultOptions
    {
        public int? StartTime { get; set; }
        public int? EndTime { get; set; }
        public string IPAddress { get; set; }
        public string Referrer { get; set; }
        public string PageURL { get; set; }

        public MediaVaultOptions()
        {
            IPAddress = "";
            Referrer = "";
            PageURL = "";
        }
    }
}
