using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class MediaContainerDTO
    {
        public int m_nMediaID { get; set; }

        public int m_nMediaFileID { get; set; }

        public int m_nMaxUses { get; set; }

        public int m_nCurrentUses { get; set; }

        public DateTime m_dEndDate { get; set; }

        public DateTime m_dCurrentDate { get; set; }

        public DateTime m_dPurchaseDate { get; set; }

        public PaymentMethodDTO m_purchaseMethod { get; set; }

        public string m_sDeviceUDID { get; set; }

        public string m_sDeviceName { get; set; }
    }
}