using System.Collections.Generic;
using System.ServiceModel;
using ApiObjects;
using ApiObjects.Response;
using Core.Users;

namespace WebAPI.WebServices
{
    [ServiceContract(Namespace= "http://domains.tvinci.com/")]
    public interface IDomainsService
    {
        [OperationContract]
        DeviceResponse AddDevice(string sWSUserName, string sWSPassword, int nDomainID, string udid, string deviceName, int deviceBrandID);
        [OperationContract]
        DomainStatusResponse AddDeviceToDomain(string sWSUserName, string sWSPassword, int nDomainID, string udid, string deviceName, int deviceBrandID);
        [OperationContract]
        DomainStatusResponse AddDomain(string sWSUserName, string sWSPassword, string sDomainName, string sDomainDescription, int nMasterUserGuid);
        [OperationContract]
        HomeNetworkResponse AddDomainHomeNetwork(string sWSUsername, string sWSPassword, long lDomainID, string sNetworkID, string sNetworkName, string sNetworkDesc, bool isActive);
        [OperationContract]
        DomainStatusResponse AddDomainWithCoGuid(string sWSUserName, string sWSPassword, string sDomainName, string sDomainDescription, int nMasterUserGuid, string sCoGuid);
        [OperationContract]
        NetworkResponseObject AddHomeNetworkToDomain(string sWSUsername, string sWSPassword, long lDomainID, string sNetworkID, string sNetworkName, string sNetworkDesc);
        [OperationContract]
        DomainStatusResponse AddUserToDomain(string sWSUserName, string sWSPassword, int nDomainID, int nUserGuid, int nMasterUserGuid, bool bIsMaster);
        [OperationContract]
        DomainStatusResponse ChangeDeviceDomainStatus(string sWSUserName, string sWSPassword, int nDomainID, string deviceUDID, bool activate);
        [OperationContract]
        ChangeDLMObj ChangeDLM(string sWSUsername, string sWSPassword, int nDomainID, int nDlmID);
        [OperationContract]
        DomainResponseObject ChangeDomainMaster(string sWSUserName, string sWSPassword, int nDomainID, int nCurrentMasterID, int nNewMasterID);
        [OperationContract]
        DomainResponseObject ConfirmDeviceByDomainMaster(string sWSUserName, string sWSPassword, string sMasterUN, string sDeviceUDID, string sToken);
        [OperationContract]
        DeviceResponse GetDevice(string sWSUserName, string sWSPassword, string udid, int domainId, string userId, string ip);
        [OperationContract]
        List<Domain> GetDeviceDomains(string sWSUserName, string sWSPassword, string udid);
        [OperationContract]
        DeviceResponseObject GetDeviceInfo(string sWSUserName, string sWSPassword, string sID, bool bIsUDID);
        [OperationContract]
        DeviceRegistrationStatusResponse GetDeviceRegistrationStatus(string sWSUserName, string sWSPassword, string udid, int domainId);
        [OperationContract]
        DLMResponse GetDLM(string sWSUsername, string sWSPassword, int nDlmID);
        [OperationContract]
        DomainStatusResponse GetDomainByCoGuid(string sWSUserName, string sWSPassword, string sCoGuid);
        [OperationContract]
        DomainResponse GetDomainByUser(string sWSUserName, string sWSPassword, string siteGuid);
        [OperationContract]
        HomeNetworksResponse GetDomainHomeNetworks(string sWSUsername, string sWSPassword, long lDomainID);
        [OperationContract]
        int GetDomainIDByCoGuid(string sWSUserName, string sWSPassword, string sCoGuid);
        [OperationContract]
        int[] GetDomainIDsByOperatorCoGuid(string sWSUserName, string sWSPassword, string sOperatorCoGuid);
        [OperationContract]
        DomainResponse GetDomainInfo(string sWSUserName, string sWSPassword, int nDomainID);
        [OperationContract]
        List<string> GetDomainUserList(string sWSUserName, string sWSPassword, int nDomainID);
        [OperationContract]
        DevicePinResponse GetPINForDevice(string sWSUserName, string sWSPassword, string sDeviceUDID, int nBrandID);
        [OperationContract]
        DeviceResponse RegisterDeviceToDomainWithPIN(string sWSUserName, string sWSPassword, string sPID, int nDomainID, string sDeviceName);
        [OperationContract]
        DomainStatusResponse RemoveDeviceFromDomain(string sWSUserName, string sWSPassword, int nDomainID, string udid);
        [OperationContract]
        ApiObjects.Response.Status RemoveDLM(string sWSUsername, string sWSPassword, int nDlmID);
        [OperationContract]
        DomainResponseStatus RemoveDomain(string sWSUserName, string sWSPassword, int nDomainID, bool purge);
        [OperationContract]
        ApiObjects.Response.Status RemoveDomainByCoGuid(string sWSUserName, string sWSPassword, string coGuid, bool purge);
        [OperationContract]
        ApiObjects.Response.Status RemoveDomainById(string sWSUserName, string sWSPassword, int nDomainID, bool purge);
        [OperationContract]
        ApiObjects.Response.Status RemoveDomainHomeNetwork(string sWSUsername, string sWSPassword, long lDomainID, string sNetworkID);
        [OperationContract]
        DomainStatusResponse RemoveUserFromDomain(string sWSUserName, string sWSPassword, int nDomainID, string sUserGUID);
        [OperationContract]
        DomainResponseObject ResetDomain(string sWSUserName, string sWSPassword, int nDomainID);
        [OperationContract]
        DomainStatusResponse ResetDomainFrequency(string sWSUserName, string sWSPassword, int nDomainID, int nFrequencyType);
        [OperationContract]
        ApiObjects.Response.Status ResumeDomain(string sWSUserName, string sWSPassword, int nDomainID);
        [OperationContract]
        DeviceResponse SetDevice(string sWSUserName, string sWSPassword, string sDeviceUDID, string sDeviceName);
        [OperationContract]
        ApiObjects.Response.Status SetDeviceInfo(string sWSUserName, string sWSPassword, string sDeviceUDID, string sDeviceName);
        [OperationContract]
        HomeNetworkResponse SetDomainHomeNetwork(string sWSUsername, string sWSPassword, long lDomainID, string sNetworkID, string sNetworkName, string sNetworkDesc, bool bIsActive);
        [OperationContract]
        DomainStatusResponse SetDomainInfo(string sWSUserName, string sWSPassword, int nDomainID, string sDomainName, string sDomainDescription);
        [OperationContract]
        ApiObjects.Response.Status SetDomainRegion(string sWSUserName, string sWSPassword, int domainId, string extRegionId, string lookupKey);
        [OperationContract]
        bool SetDomainRestriction(string sWSUserName, string sWSPassword, int nDomainID, int nRestriction);
        [OperationContract]
        DeviceResponse SubmitAddDeviceToDomain(string sWSUserName, string sWSPassword, int domainID, string userID, string deviceUdid, string deviceName, int brandID);
        [OperationContract]
        DomainStatusResponse SubmitAddDeviceToDomainRequest(string sWSUserName, string sWSPassword, int nDomainID, int nUserID, string sDeviceUdid, string sDeviceName, int nBrandID);
        [OperationContract]
        DomainStatusResponse SubmitAddUserToDomainRequest(string sWSUserName, string sWSPassword, int nUserID, string sMasterUsername);
        [OperationContract]
        ApiObjects.Response.Status SuspendDomain(string sWSUserName, string sWSPassword, int nDomainID);
        [OperationContract]
        ApiObjects.Response.Status UpdateDomainHomeNetwork(string sWSUsername, string sWSPassword, long lDomainID, string sNetworkID, string sNetworkName, string sNetworkDesc, bool bIsActive);
        [OperationContract]
        ValidationResponseObject ValidateLimitationModule(string sWSUsername, string sWSPassword, string sUDID, int nDeviceBrandID, long lSiteGuid, long lDomainID, ValidationType eValidation, int nRuleID = 0, int nMediaConcurrencyLimit = 0, int nMediaID = 0);
        [OperationContract]
        ValidationResponseObject ValidateLimitationNpvr(string sWSUsername, string sWSPassword, string sUDID, int nDeviceBrandID, long lSiteGuid, long lDomainID, ValidationType eValidation, int nNpvrConcurrencyLimit = 0, string sNpvrID = null);
        [OperationContract]
        bool VerifyDRMDevice(string sWSUsername, string sWSPassword, string userId, string udid, string drmId);
    }
}