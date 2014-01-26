using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class PermittedMediaContainer
    {
        public int mediaID { get; set; }

        public int mediaFileID { get; set; }

        public int maxUses { get; set; }

        public int currentUses { get; set; }

        public DateTime endDate { get; set; }

        public DateTime currentDate { get; set; }

        public DateTime purchaseDate { get; set; }

        public PaymentMethod purchaseMethod { get; set; }

        public string deviceUDID { get; set; }

        public string deviceName { get; set; }
    }
}
