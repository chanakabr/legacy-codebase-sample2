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

        [OperationContract]
        DomainResponseObject AddDeviceToDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID);

        [OperationContract]
        DomainResponseObject AddUserToDomain(InitializationObject initObj, bool bMaster);

        [OperationContract]
        Domain RemoveUserFromDomain(InitializationObject initObj);

        [OperationContract]
        DomainResponseObject RemoveDeviceFromDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID);

        [OperationContract]
        DomainResponseObject ChangeDeviceDomainStatus(InitializationObject initObj, bool bActive);

        [OperationContract]
        Domain GetDomainInfo(InitializationObject initObj);

        [OperationContract]
        DomainResponseObject SetDomainInfo(InitializationObject initObj, string sDomainName, string sDomainDescription);

        [OperationContract]
        DomainResponseObject AddDomain(InitializationObject initObj, string domainName, string domainDesc, int masterGuid);
    }
}
