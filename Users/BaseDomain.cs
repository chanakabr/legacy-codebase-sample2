using DAL;
using System;
using System.Collections.Generic;
using System.Data;
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
            
        }

        public abstract DomainResponseObject    AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID, string sCoGuid);

        public abstract DomainResponseObject    AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID);

        public abstract DomainResponseStatus    RemoveDomain(int nDomainID);

        public abstract DomainResponseObject    SetDomainInfo(int nDomainID, string sDomainName, int nGroupID, string sDomainDescription);

        
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
        

        public abstract int[]                   GetDomainIDsByOperatorCoGuid(string sOperatorCoGuid);

        public abstract bool                    SetDomainRestriction(int nDomainID, DomainRestriction rest);

        public virtual List<HomeNetwork> GetDomainHomeNetworks(long lDomainID)
        {
            return Utils.GetHomeNetworksOfDomain(lDomainID, m_nGroupID);
        }

        public virtual NetworkResponseObject AddHomeNetworkToDomain(long lDomainID, string sNetworkID,
            string sNetworkName, string sNetworkDesc)
        {
            NetworkResponseObject res = new NetworkResponseObject(false, NetworkResponseStatus.Error);
            List<HomeNetwork> lstOfHomeNetworks = null;
            int numOfAllowedNetworks = 0;
            int numOfActiveNetworks = 0;

            // check network id is valid
            if (IsHomeNetworkInputInvalid(lDomainID, sNetworkID))
            {
                res.eReason = NetworkResponseStatus.InvalidInput;
                res.bSuccess = false;
                return res;
            }

            HomeNetwork candidate = new HomeNetwork(sNetworkName, sNetworkID, sNetworkDesc, DateTime.UtcNow, true);
            DataTable dt = null;
            if (!DomainDal.Get_ProximityDetectionDataForInsertion(m_nGroupID, lDomainID, ref numOfAllowedNetworks, ref dt))
            {
                // failed to extract data from DB.
                // log and return err

                Logger.Logger.Log("AddHomeNetworkToDomain", GetUpdateHomeNetworkErrMsg("Failed to extract data from DB", lDomainID, candidate, 0, numOfAllowedNetworks, numOfActiveNetworks), "TvinciDomain");
                res.eReason = NetworkResponseStatus.Error;
                res.bSuccess = false;

                return res;
            }

            GetListOfExistingHomeNetworksForInsertion(dt, out lstOfHomeNetworks, out numOfActiveNetworks);


            // check if network already exists
            if (lstOfHomeNetworks.Contains(candidate))
            {
                res.eReason = NetworkResponseStatus.NetworkExists;
                res.bSuccess = false;

                return res;
            }

            // check quantity limitation
            if (!IsSatisfiesQuantityConstraint(numOfAllowedNetworks, numOfActiveNetworks))
            {
                res.eReason = NetworkResponseStatus.QuantityLimitation;
                res.bSuccess = false;

                return res;
            }

            // all validations pass, add new home network to domain
            if (!DomainDal.Insert_NewHomeNetwork(m_nGroupID, candidate.UID, lDomainID, candidate.Name, candidate.Description, candidate.IsActive, candidate.CreateDate))
            {
                // failed to insert
                res.eReason = NetworkResponseStatus.Error;
                res.bSuccess = false;

                // log
                Logger.Logger.Log("AddHomeNetworkToDomain", String.Concat("Failed to add to domain: ", lDomainID, " the home network: ", candidate.ToString()), "TvinciDomains");

            }
            else
            {
                res.eReason = NetworkResponseStatus.OK;
                res.bSuccess = true;
            }

            return res;
        }

        public virtual NetworkResponseObject UpdateDomainHomeNetwork(long lDomainID, string sNetworkID,
            string sNetworkName, string sNetworkDesc, bool bIsActive)
        {
            NetworkResponseObject res = null;
            HomeNetwork candidate = null;
            HomeNetwork existingNetwork = null;
            int numOfAllowedNetworks = 0;
            int numOfActiveNetworks = 0;
            int frequency = 0;
            DateTime dtLastDeactivationDate = DateTime.MinValue;
            if (!UpdateRemoveHomeNetworkCommon(lDomainID, sNetworkID, sNetworkName, sNetworkDesc, bIsActive, out res,
                ref candidate, ref existingNetwork, ref numOfAllowedNetworks, ref numOfActiveNetworks, ref frequency, ref dtLastDeactivationDate))
            {
                return res;
            }
            return UpdateDomainHomeNetworkInner(lDomainID, numOfAllowedNetworks, numOfActiveNetworks, frequency,
                candidate, existingNetwork, dtLastDeactivationDate, ref res);
        }


        public virtual NetworkResponseObject RemoveDomainHomeNetwork(long lDomainID, string sNetworkID)
        {
            NetworkResponseObject res = null;
            HomeNetwork candidate = null;
            HomeNetwork existingNetwork = null;
            int numOfAllowedNetworks = 0;
            int numOfActiveNetworks = 0;
            int frequency = 0;
            DateTime dtLastDeactivationDate = DateTime.MinValue;
            if (!UpdateRemoveHomeNetworkCommon(lDomainID, sNetworkID, string.Empty, string.Empty, false, out res,
                ref candidate, ref existingNetwork, ref numOfAllowedNetworks, ref numOfActiveNetworks, ref frequency, ref dtLastDeactivationDate))
            {
                return res;
            }

            return RemoveDomainHomeNetworkInner(lDomainID, numOfAllowedNetworks, numOfActiveNetworks, frequency,
                candidate, existingNetwork, dtLastDeactivationDate, ref res);
        }

        #region Protected abstract

        protected abstract NetworkResponseObject RemoveDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks,
            int numOfActiveNetworks, int frequency, HomeNetwork candidate,
            HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref NetworkResponseObject res);

        protected abstract NetworkResponseObject UpdateDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks, int numOfActiveNetworks,
            int frequency, HomeNetwork candidate, HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref NetworkResponseObject res);

        #endregion

        #region Protected implemented

        protected bool IsSatisfiesFrequencyConstraint(DateTime dtLastDeactivationDate, int frequency)
        {
            DateTime dt = Utils.GetEndDateTime(dtLastDeactivationDate, frequency);

            return dt < DateTime.UtcNow;
        }

        protected bool IsSatisfiesQuantityConstraint(int numOfAllowedNetworks, int numOfActiveNetworks)
        {
            return numOfAllowedNetworks > numOfActiveNetworks;
        }

        protected string GetUpdateHomeNetworkErrMsg(string errMsg, long lDomainID, HomeNetwork candidate, int frequency, int numOfAllowedHomeNetworks, int numOfActiveHomeNetworks)
        {
            StringBuilder sb = new StringBuilder(errMsg);
            sb.Append(String.Concat(" Domain ID: ", lDomainID));
            sb.Append(String.Concat(". Frequency: ", frequency));
            sb.Append(String.Concat(". Num of allowed home networks: ", numOfAllowedHomeNetworks));
            sb.Append(String.Concat(". Num of activate home networks: ", numOfActiveHomeNetworks));
            sb.Append(String.Concat(". Home network candidate: ", candidate.ToString()));

            return sb.ToString();
        }

        protected HomeNetwork GetHomeNetworkFromList(List<HomeNetwork> lstOfHomeNetworks, HomeNetwork hn)
        {
            foreach (HomeNetwork iter in lstOfHomeNetworks)
            {
                if (iter.Equals(hn))
                    return iter;
            }
            return null;
        }

        protected void GetListOfExistingHomeNetworksForUpdating(DataTable dt, out List<HomeNetwork> lst, out int numOfActiveNetworks)
        {
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                int length = dt.Rows.Count;
                numOfActiveNetworks = 0;

                lst = new List<HomeNetwork>(length);

                for (int i = 0; i < length; i++)
                {
                    HomeNetwork hn = new HomeNetwork();
                    hn.UID = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["NETWORK_ID"]);
                    hn.IsActive = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["IS_ACTIVE"]) != 0;
                    if (hn.IsActive)
                        numOfActiveNetworks++;
                    hn.Name = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["NAME"]);
                    hn.Description = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["DESCRIPTION"]);

                    lst.Add(hn);

                }

            }
            else
            {
                lst = new List<HomeNetwork>(0);
                numOfActiveNetworks = 0;
            }

        }

        protected bool IsHomeNetworkInputInvalid(long lDomainID, string sNetworkID)
        {
            return lDomainID < 1 || string.IsNullOrEmpty(sNetworkID);
        }

        protected void GetListOfExistingHomeNetworksForInsertion(DataTable dt, out List<HomeNetwork> lst, out int numOfActiveNetworks)
        {
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                int length = dt.Rows.Count;
                lst = new List<HomeNetwork>(length);

                numOfActiveNetworks = 0;

                for (int i = 0; i < length; i++)
                {
                    HomeNetwork hn = new HomeNetwork();
                    hn.UID = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["NETWORK_ID"]);
                    if (hn.UID.Length == 0)
                        continue;
                    hn.IsActive = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["IS_ACTIVE"]) != 0;
                    if (hn.IsActive)
                        numOfActiveNetworks++;
                    lst.Add(hn);
                }

            }
            else
            {
                lst = new List<HomeNetwork>(0);
                numOfActiveNetworks = 0;
            }
        }

        #endregion

        #region Private methods
        private bool UpdateRemoveHomeNetworkCommon(long lDomainID, string sNetworkID, string sNetworkName, string sNetworkDesc, bool bIsActive,
            out NetworkResponseObject res, ref HomeNetwork nullifiedCandidate, ref HomeNetwork nullifiedExistingNetwork,
            ref int numOfAllowedNetworks, ref int numOfActiveNetworks, ref int frequency, ref DateTime dtLastDeactivationDate)
        {
            bool retVal = true;
            res = new NetworkResponseObject(false, NetworkResponseStatus.Error);
            List<HomeNetwork> lstOfHomeNetworks = null;
            DataTable dt = null;
            if (IsHomeNetworkInputInvalid(lDomainID, sNetworkID))
            {
                res.eReason = NetworkResponseStatus.InvalidInput;
                res.bSuccess = false;
                retVal = false;
                return retVal;
            }

            nullifiedCandidate = new HomeNetwork(sNetworkName, sNetworkID, sNetworkDesc, DateTime.UtcNow, bIsActive);

            if (!DomainDal.Get_ProximityDetectionDataForUpdating(m_nGroupID, lDomainID, sNetworkID, ref numOfAllowedNetworks, ref frequency, ref dtLastDeactivationDate, ref dt))
            {
                // failed to extract data from db. log and return err
                Logger.Logger.Log("UpdateRemoveHomeNetworkCommon", GetUpdateHomeNetworkErrMsg("DomainDal.Get_ProximityDetectionDataForUpdating failed.", lDomainID, nullifiedCandidate, frequency, numOfAllowedNetworks, numOfActiveNetworks), "BaseDomain");

                res.eReason = NetworkResponseStatus.Error;
                res.bSuccess = false;
                retVal = false;
                return retVal;
            }

            GetListOfExistingHomeNetworksForUpdating(dt, out lstOfHomeNetworks, out numOfActiveNetworks);

            nullifiedExistingNetwork = GetHomeNetworkFromList(lstOfHomeNetworks, nullifiedCandidate);

            if (nullifiedExistingNetwork == null)
            {
                res.eReason = NetworkResponseStatus.NetworkDoesNotExist;
                res.bSuccess = false;
                retVal = false;

                return retVal;
            }

            return retVal;
        }

        #endregion

    }
}
