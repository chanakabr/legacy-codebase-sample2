using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class TvinciDomain : BaseDomain
    {
        protected TvinciDomain()
        {
        }

        public TvinciDomain(int groupID)
            : base(groupID)
        {
        }

        //Override Methods
        public override int GetDomainIDByCoGuid(string coGuid)
        {
            int domainID = DAL.DomainDal.GetDomainIDByCoGuid(coGuid);

            return domainID;
        }

        public override DomainResponseObject GetDomainByCoGuid(string coGuid, int nGroupID)
        {
            // Create new response
            DomainResponseObject oDomainResponseObject;

            int nDomainID = GetDomainIDByCoGuid(coGuid);

            if (nDomainID <= 0)
            {
                oDomainResponseObject = new DomainResponseObject(null, DomainResponseStatus.DomainNotExists);

                return oDomainResponseObject;
            }

            Domain domain = DomainFactory.GetDomain(nGroupID, nDomainID);

            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);

            return oDomainResponseObject;
        }

        protected override Domain DomainInitializer(int nGroupID, int nDomainID)
        {
            return DomainFactory.GetDomain(nGroupID, nDomainID);
        }

        public override DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID, string sCoGuid)
        {

            Domain domain = DomainFactory.CreateDomain(sDomainName.Trim(), sDomainDescription.Trim(), nMasterUserGuid, nGroupID, sCoGuid);

            DomainResponseObject oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.UnKnown);

            if (domain.m_DomainStatus != DomainStatus.OK)
            {
                if (domain.m_DomainStatus == DomainStatus.DomainAlreadyExists)
                {
                    oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.DomainAlreadyExists);
                }
                else if (domain.m_DomainStatus == DomainStatus.HouseholdUserFailed)
                {
                    oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.HouseholdUserFailed);
                }
                else if (domain.m_DomainStatus == DomainStatus.Error)
                {
                    oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
                }

                return oDomainResponseObject;
            }

            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);

            return oDomainResponseObject;
        }

        public override DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID)
        {
            return AddDomain(sDomainName, sDomainDescription, nMasterUserGuid, nGroupID, "");
        }

        /// <summary>
        /// SubmitAddUserToDomainRequest
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="nUserGuid"></param>
        /// <param name="sMasterUsername"></param>
        /// <returns></returns>
        public override DomainResponseObject SubmitAddUserToDomainRequest(int nGroupID, int nUserGuid, string sMasterUsername)
        {
            //New domain
            Domain domain = new Domain();
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Check if UserGuid is valid
            if (!User.IsUserValid(nGroupID, nUserGuid))
            {
                domain.m_DomainStatus = DomainStatus.Error;
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
            }

            oDomainResponseObject = domain.SubmitAddUserToDomainRequest(nGroupID, nUserGuid, sMasterUsername);

            return oDomainResponseObject;
        }

        protected override NetworkResponseObject RemoveDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks,
            int numOfActiveNetworks, int frequency, HomeNetwork candidate,
            HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref NetworkResponseObject res)
        {
            if (IsSatisfiesFrequencyConstraint(dtLastDeactivationDate, frequency) || !existingNetwork.IsActive)
            {
                // we can remove it if the home network either satisfies the frequency constraint or the request
                // is to remove a de-activated home network
                if (DomainDal.Update_HomeNetworkWithDeactivationDate(lDomainID, existingNetwork.UID, m_nGroupID, existingNetwork.Name, existingNetwork.Description, false))
                {
                    res.eReason = NetworkResponseStatus.OK;
                    res.bSuccess = true;
                }
                else
                {
                    // failed to update db. log and return err

                    Logger.Logger.Log("RemoveDomainHomeNetworkInner", GetUpdateHomeNetworkErrMsg("Failed to delete in DB. ", lDomainID, existingNetwork, frequency, numOfAllowedNetworks, numOfActiveNetworks), "TvinciDomain");

                    res.eReason = NetworkResponseStatus.Error;
                    res.bSuccess = false;
                }
            }
            else
            {
                // does not satisfy the frequency constraint. return frequency err
                res.eReason = NetworkResponseStatus.FrequencyLimitation;
                res.bSuccess = false;
            }

            return res;
        }

        protected override NetworkResponseObject UpdateDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks, int numOfActiveNetworks,
            int frequency, HomeNetwork candidate, HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref NetworkResponseObject res)
        {
            if (candidate.IsActive == existingNetwork.IsActive)
            {
                // no changes of network activeness. just update in DB name and desc
                if (DomainDal.Update_HomeNetworkWithoutDeactivationDate(lDomainID, candidate.UID, m_nGroupID, candidate.Name, candidate.Description, candidate.IsActive))
                {
                    res.eReason = NetworkResponseStatus.OK;
                    res.bSuccess = true;
                }
                else
                {
                    // failed to update in db. log and return error
                    Logger.Logger.Log("UpdateDomainHomeNetworkInner", GetUpdateHomeNetworkErrMsg("DB failed to update. In if candidate.IsActive == existingHomeNetwork.IsActive", lDomainID, candidate, frequency, numOfAllowedNetworks, numOfActiveNetworks), "TvinciDomain");

                    res.eReason = NetworkResponseStatus.Error;
                    res.bSuccess = false;
                }
            }
            else
            {
                if (candidate.IsActive)
                {
                    // a request to activate the home network.
                    // check if violates the quantity constraint
                    if (IsSatisfiesQuantityConstraint(numOfAllowedNetworks, numOfActiveNetworks))
                    {
                        // we can activate the home network. update data in db.
                        if (DomainDal.Update_HomeNetworkWithoutDeactivationDate(lDomainID, candidate.UID, m_nGroupID, candidate.Name, candidate.Description, candidate.IsActive))
                        {
                            res.eReason = NetworkResponseStatus.OK;
                            res.bSuccess = true;
                        }
                        else
                        {
                            // failed to update db. log and return error
                            Logger.Logger.Log("UpdateDomainHomeNetworkInner", GetUpdateHomeNetworkErrMsg("DB failed to update", lDomainID, candidate, frequency, numOfAllowedNetworks, numOfActiveNetworks), "TvinciDomain");

                            res.eReason = NetworkResponseStatus.Error;
                            res.bSuccess = false;
                        }

                    }
                    else
                    {
                        // we cannot activate the home network. return quantity error

                        res.eReason = NetworkResponseStatus.QuantityLimitation;
                        res.bSuccess = false;
                    }
                }
                else
                {
                    // the request is to de-activate the home network
                    // check if violates the frequency constraint
                    if (IsSatisfiesFrequencyConstraint(dtLastDeactivationDate, frequency))
                    {
                        // satsfies the frequency constraint. update data in db.
                        if (DomainDal.Update_HomeNetworkWithDeactivationDate(lDomainID, candidate.UID, m_nGroupID, candidate.Name, candidate.Description, true))
                        {
                            res.eReason = NetworkResponseStatus.OK;
                            res.bSuccess = true;
                        }
                        else
                        {
                            // failed to update data in db. log and return error
                            Logger.Logger.Log("UpdateDomainHomeNetworkInner", GetUpdateHomeNetworkErrMsg("DB failed to update", lDomainID, candidate, frequency, numOfAllowedNetworks, numOfActiveNetworks), "TvinciDomain");

                            res.eReason = NetworkResponseStatus.Error;
                            res.bSuccess = false;
                        }
                    }
                    else
                    {
                        // does not satisfy the frequency constraint. return frequency error
                        res.eReason = NetworkResponseStatus.FrequencyLimitation;
                        res.bSuccess = false;
                    }
                }
            }

            return res;
        }
    }
}
