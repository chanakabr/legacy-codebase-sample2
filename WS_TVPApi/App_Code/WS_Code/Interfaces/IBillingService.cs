using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPPro.SiteManager.TvinciPlatform.Billing;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Context;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IBillingService
    {
        [OperationContract]
        AdyenBillingDetail GetLastBillingUserInfo(InitializationObject initObj, int billingMethod);

        [OperationContract]
        string GetClientMerchantSig(InitializationObject initObj, string sParamaters);
    }
}
