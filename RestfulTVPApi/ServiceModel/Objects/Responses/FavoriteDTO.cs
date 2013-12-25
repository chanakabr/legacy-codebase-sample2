using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class FavoriteDTO
    {
        public string m_sDeviceUDID { get; set; }

        public string m_sType { get; set; }

        public string m_sItemCode { get; set; }

        public string m_sSiteUserGUID { get; set; }

        public DateTime m_dUpdateDate { get; set; }

        public string m_sExtraData { get; set; }

        public int m_nID { get; set; }

        public string m_sDeviceName { get; set; }

        public int m_nDomainID { get; set; }

        public int m_is_channel { get; set; }
    }
}