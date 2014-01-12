using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class MediaContainer
    {
        public int m_nMediaID { get; set; }

        public int m_nMediaFileID { get; set; }

        public int m_nMaxUses { get; set; }

        public int m_nCurrentUses { get; set; }

        public DateTime m_dEndDate { get; set; }

        public DateTime m_dCurrentDate { get; set; }

        public DateTime m_dPurchaseDate { get; set; }

        public PaymentMethod m_purchaseMethod { get; set; }

        public string m_sDeviceUDID { get; set; }

        public string m_sDeviceName { get; set; }
    }
}
