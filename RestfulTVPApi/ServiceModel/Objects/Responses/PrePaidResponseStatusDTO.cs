using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public enum PrePaidResponseStatusDTO
    {

        /// <remarks/>
        Success,

        /// <remarks/>
        Fail,

        /// <remarks/>
        UnKnown,

        /// <remarks/>
        PriceNotCorrect,

        /// <remarks/>
        UnKnownUser,

        /// <remarks/>
        UnKnownPPVModule,

        /// <remarks/>
        UnKnownPPModule,

        /// <remarks/>
        NoCredit,
    }
}