using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPPro.SiteManager.TvinciPlatform.Pricing;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.TvinciPlatform.Domains;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IDomainService
    {
        [OperationContract]
        DomainResponseObject ResetDomain(InitializationObject initObj);
    }
}
