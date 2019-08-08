using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Context;
using TVPApiModule.Objects.Responses;
using Core.Users;

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
        DomainResponseObject AddUserToDomain(InitializationObject initObj, int masterUserGuid);

        [OperationContract]
        DomainResponseObject RemoveUserFromDomain(InitializationObject initObj, string userGuidToRemove);

        [OperationContract]
        DomainResponseObject RemoveDeviceFromDomain(InitializationObject initObj, int domainID, string sDeviceName, int iDeviceBrandID, string sUdid);

        [OperationContract]
        DomainResponseObject ChangeDeviceDomainStatus(InitializationObject initObj, bool bActive);

        [OperationContract]
        Domain GetDomainInfo(InitializationObject initObj);

        [OperationContract]
        DomainResponseObject SetDomainInfo(InitializationObject initObj, string sDomainName, string sDomainDescription);

        [OperationContract]
        DomainResponseObject AddDomain(InitializationObject initObj, string domainName, string domainDesc, int masterGuid);

        [OperationContract]
        DomainResponseObject AddDomainWithCoGuid(InitializationObject initObj, string domainName, string domainDesc, int masterGuid, string coGuid);

        [OperationContract]
        DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid);

        [OperationContract]
        int GetDomainIDByCoGuid(InitializationObject initObj, string coGuid);

        [OperationContract]
        DomainResponseObject SubmitAddUserToDomainRequest(InitializationObject initObj, string masterUsername);

        [OperationContract]
        string RemoveDomain(InitializationObject initObj);

        [OperationContract]
        int[] GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid);

        [OperationContract]
        bool SetDomainRestriction(InitializationObject initObj, int restriction);

        [OperationContract]
        DomainResponseObject SubmitAddDeviceToDomainRequest(InitializationObject initObj, string deviceName, int brandId);

        [OperationContract]
        DomainResponseObject ConfirmDeviceByDomainMaster(InitializationObject initObj, string udid, string masterUn, string token);

        [OperationContract]
        NetworkResponseObject AddHomeNetworkToDomain(InitializationObject initObj, string networkId, string networkName, string networkDesc);

        [OperationContract]
        NetworkResponseObject UpdateDomainHomeNetwork(InitializationObject initObj, string networkId, string networkName, string networkDesc, bool isActive);

        [OperationContract]
        NetworkResponseObject RemoveDomainHomeNetwork(InitializationObject initObj, string networkId);

        [OperationContract]
        HomeNetwork[] GetDomainHomeNetworks(InitializationObject initObj);

        [OperationContract]
        DomainResponseObject ChangeDomainMaster(InitializationObject initObj, int currentMasterID, int newMasterID);

        [OperationContract]
        DomainResponseObject ResetDomainFrequency(InitializationObject initObj, int frequencyType);

        [OperationContract]
        ClientResponseStatus SuspendDomain(InitializationObject initObj, int domainId);

        [OperationContract]
        ClientResponseStatus ResumeDomain(InitializationObject initObj, int domainId);

        [OperationContract]
        DomainLimitationModuleResponse GetDomainLimitationModule(InitializationObject initObj, int domainLimitationID);

        [OperationContract]
        ClientResponseStatus SetDomainRegion(InitializationObject initObj, int domain_id, string ext_region_id, string lookup_key);
    }
}