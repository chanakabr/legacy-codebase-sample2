using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class PPVModule
    {
        /// <remarks/>
        public PriceCode m_oPriceCode { get; set; }

        public UsageModule m_oUsageModule { get; set; }

        public DiscountModule m_oDiscountModule { get; set; }

        public CouponsGroup m_oCouponsGroup { get; set; }

        public LanguageContainer[] m_sDescription { get; set; }

        public string m_sObjectCode { get; set; }

        public string m_sObjectVirtualName { get; set; }

        public bool m_bSubscriptionOnly { get; set; }

        public int[] m_relatedFileTypes { get; set; }

        public string m_Product_Code{ get; set; }
    }
}
