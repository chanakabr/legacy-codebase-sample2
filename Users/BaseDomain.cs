using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public abstract class BaseDomain
    {
        protected int m_nGroupID;

        protected BaseDomain() { }
        public BaseDomain(int nGroupID)
        {
            m_nGroupID = nGroupID;
            //m_bIsInitialized = false;
        }

        public abstract DomainResponseObject    AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID, string sCoGuid);

        public abstract DomainResponseObject    AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID);

        public abstract DomainResponseStatus    RemoveDomain(int nDomainID);

        public abstract DomainResponseObject    SetDomainInfo(int nDomainID, string sDomainName, int nGroupID, string sDomainDescription);

        //public abstract DomainResponseObject    AddUserToDomain(int nGroupID, int domainID, int nUserGuid, bool bIsMaster);
        public abstract DomainResponseObject    AddUserToDomain(int nGroupID, int domainID, int nUserGuid, int nMasterUserGuid, bool bIsMaster = false);

        public abstract DomainResponseObject    SubmitAddUserToDomainRequest(int nGroupID, int nUserGuid, string sMasterUsername);

        public abstract DomainResponseObject    RemoveUserFromDomain(int nGroupID, int sDomainID, int nUserGUID);

        public abstract Domain                  GetDomainInfo(int nDomainID, int nGroupID);

        public abstract DomainResponseObject    AddDeviceToDomain(int nGroupID, int domainID, string sUDID, string sDeviceName, int nBrandID);

        public abstract DomainResponseObject    ChangeDeviceDomainStatus(int nDomainID, string sDeviceUDID, bool bisEnable);

        public abstract DomainResponseObject    RemoveDeviceFromDomain(int nDomainID, string nDeviceUDID);

        public abstract DomainResponseObject    SubmitAddDeviceToDomainRequest(int nGroupID, int nDomainID, int nUserID, string sDeviceUdid, string sDeviceName, int nBrandID);

        public abstract DomainResponseObject    ConfirmDeviceByDomainMaster(string sMasterUN, string sUDID, string sToken);

        public abstract List<string>            GetDomainUserList(int nDomainID, int nGroupID);

        public abstract List<Domain>            GetDeviceDomains(string sUDID);

        public abstract DeviceResponseObject    RegisterDeviceToDomainWithPIN(int nGroupID, string sPIN, int nDomainID, string sDeviceName);

        public abstract DomainResponseObject    ResetDomain(int nDomainID);

        public abstract int                     GetDomainIDByCoGuid(string coGuid);

        public abstract DomainResponseObject    GetDomainByCoGuid(string coGuid, int nGroupID);
        //public abstract DomainResponseObject    SetDomainActive(int nDomainID, bool bActive);

        public abstract int[]                   GetDomainIDsByOperatorCoGuid(string sOperatorCoGuid);

        public abstract bool                    SetDomainRestriction(int nDomainID, DomainRestriction rest);

    }
}
