using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public enum KalturaPartnerConfigurationType
    {
        DefaultPaymentGateway,
        EnablePaymentGatewaySelection,
        OSSAdapter,
        Concurrency,
        General,
        ObjectVirtualAsset,
        Commerce,
        Playback,
        Payment,
        Catalog,
        Security,
        Opc,
        Base,
        CustomFields,
        DefaultParentalSettings,
    }
}