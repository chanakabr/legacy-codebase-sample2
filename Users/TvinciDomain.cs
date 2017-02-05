using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Users.Cache;
using KLogMonitor;
using System.Reflection;

namespace Users
{
    public class TvinciDomain : BaseDomain
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected TvinciDomain()
        {
        }

        public TvinciDomain(int groupID)
            : base(groupID)
        {
        }

        //Override Methods - get domainID from DB
        public override int GetDomainIDByCoGuid(string coGuid)
        {
            int domainID = DAL.DomainDal.GetDomainIDByCoGuid(coGuid, m_nGroupID);

            return domainID;
        }

        //Override Methods - get domainID from DB - then - try to get it from Cache
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

            // get domain by domain id from Cache 
            DomainsCache oDomainCache = DomainsCache.Instance();
            Domain domain = oDomainCache.GetDomain(nDomainID, m_nGroupID);
            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);

            return oDomainResponseObject;
        }

        /*protected override Domain DomainInitializer(int nGroupID, int nDomainID)
        {
            // get domain by domain id from Cache 
            DomainCache oDomainCache = DomainCache.Instance();
            Domain domain = oDomainCache.GetDomain(nGroupID, nDomainID, false);
            return domain;
        }*/

        protected override Domain DomainInitializer(int nGroupID, int nDomainID, bool bCache = true)
        {
            // get domain by domain id from Cache 
            DomainsCache oDomainCache = DomainsCache.Instance();

            Domain domain = oDomainCache.GetDomain(nDomainID, nGroupID, bCache);

            return domain;
        }

        public override DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID, string sCoGuid)
        {
            DomainResponseObject oDomainResponseObject = new DomainResponseObject(null, DomainResponseStatus.Error);

            try
            {
                // create domain
                Domain domain = DomainFactory.CreateDomain(sDomainName.Trim(), sDomainDescription.Trim(), nMasterUserGuid, nGroupID, sCoGuid);

                if (domain != null)
                {
                    // check status
                    switch (domain.m_DomainStatus)
                    {
                        case DomainStatus.OK: // add domain to Cache
                        case DomainStatus.DomainCreatedWithoutNPVRAccount:
                            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);
                            DomainsCache.Instance().InsertDomain(domain);

                            // set user role to master 
                            long roleId;
                            if (long.TryParse(Utils.GetTcmConfigValue("master_role_id"), out roleId) && DAL.UsersDal.Insert_UserRole(m_nGroupID, nMasterUserGuid.ToString(), roleId, true) > 0)
                            {
                                // add invalidation key for user roles cache
                                string invalidationKey = UtilsDal.GetAddRoleInvalidationKey(nMasterUserGuid.ToString());
                                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                                {
                                    log.ErrorFormat("Failed to set invalidation key on AddDomain key = {0}", invalidationKey);
                                }
                            }
                            break;
                        case DomainStatus.UserExistsInOtherDomains:
                            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.UserExistsInOtherDomains);
                            break;
                        case DomainStatus.DomainAlreadyExists:
                            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.DomainAlreadyExists);
                            break;
                        case DomainStatus.HouseholdUserFailed:
                            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.HouseholdUserFailed);
                            break;
                        case DomainStatus.Error:
                            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
                            break;
                        default:
                            log.Error("Error - " + string.Format("Flow not recognized for DomainStatus: {0} , G ID: {1} , D Name: {2} , Master: {3}", domain.m_DomainStatus.ToString(), nGroupID, sDomainName, nMasterUserGuid));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at AddDomain. ");
                sb.Append(String.Concat(" D Name: ", sDomainName));
                sb.Append(String.Concat(" D Desc: ", sDomainDescription));
                sb.Append(String.Concat(" Master Site Guid: ", nMasterUserGuid));
                sb.Append(String.Concat(" G ID: ", nGroupID));
                sb.Append(String.Concat(" CoGuid: ", sCoGuid));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
            }

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

        protected override ApiObjects.Response.Status RemoveDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks,
            int numOfActiveNetworks, int frequency, HomeNetwork candidate,
            HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref ApiObjects.Response.Status res)
        {
            if (IsSatisfiesFrequencyConstraint(dtLastDeactivationDate, frequency) || !existingNetwork.IsActive)
            {
                // we can remove it if the home network either satisfies the frequency constraint or the request
                // is to remove a de-activated home network
                if (DomainDal.Update_HomeNetworkWithDeactivationDate(lDomainID, existingNetwork.UID, m_nGroupID, existingNetwork.Name, existingNetwork.Description, false) != null)
                {
                    res = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, ApiObjects.Response.eResponseStatus.OK.ToString());
                }
                else
                {
                    // failed to update db. log and return err

                    log.Error("RemoveDomainHomeNetworkInner - " + GetUpdateHomeNetworkErrMsg("Failed to delete in DB. ", lDomainID, existingNetwork, frequency, numOfAllowedNetworks, numOfActiveNetworks));

                    res = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                }
            }
            else
            {
                // does not satisfy the frequency constraint. return frequency err
                res = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.HomeNetworkFrequency, "Home network frequency limitation");
            }

            return res;
        }

        protected override HomeNetwork UpdateDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks, int numOfActiveNetworks,
            int frequency, HomeNetwork candidate, HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref ApiObjects.Response.Status res)
        {
            HomeNetwork homeNetwork = null;
            if (candidate.IsActive == existingNetwork.IsActive)
            {
                // no changes of network activeness. just update in DB name and desc
                homeNetwork = Utils.Update_HomeNetworkWithoutDeactivationDate(lDomainID, candidate.UID, m_nGroupID, candidate.Name, candidate.Description, candidate.IsActive);
                if (homeNetwork != null)
                {
                    res = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, ApiObjects.Response.eResponseStatus.OK.ToString());
                }
                else
                {
                    // failed to update in db. log and return error
                    log.Error("UpdateDomainHomeNetworkInner - " + GetUpdateHomeNetworkErrMsg("DB failed to update. In if candidate.IsActive == existingHomeNetwork.IsActive", lDomainID, candidate, frequency, numOfAllowedNetworks, numOfActiveNetworks));
                    res = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
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
                        homeNetwork = Utils.Update_HomeNetworkWithoutDeactivationDate(lDomainID, candidate.UID, m_nGroupID, candidate.Name, candidate.Description, candidate.IsActive);
                        if (homeNetwork != null)
                        {
                            res = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, ApiObjects.Response.eResponseStatus.OK.ToString());
                        }
                        else
                        {
                            // failed to update db. log and return error
                            log.Error("UpdateDomainHomeNetworkInner - " + GetUpdateHomeNetworkErrMsg("DB failed to update", lDomainID, candidate, frequency, numOfAllowedNetworks, numOfActiveNetworks));
                            res = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                        }

                    }
                    else
                    {
                        // we cannot activate the home network. return quantity error
                        res = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.HomeNetworkLimitation, "Home networks exceeded limit");
                    }
                }
                else
                {
                    // the request is to de-activate the home network
                    // check if violates the frequency constraint
                    if (IsSatisfiesFrequencyConstraint(dtLastDeactivationDate, frequency))
                    {
                        // satsfies the frequency constraint. update data in db.
                        homeNetwork = Utils.Update_HomeNetworkWithDeactivationDate(lDomainID, candidate.UID, m_nGroupID, candidate.Name, candidate.Description, true);
                        if (homeNetwork != null)
                        {
                            res = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, ApiObjects.Response.eResponseStatus.OK.ToString());
                        }
                        else
                        {
                            // failed to update data in db. log and return error
                            log.Error("UpdateDomainHomeNetworkInner - " + GetUpdateHomeNetworkErrMsg("DB failed to update", lDomainID, candidate, frequency, numOfAllowedNetworks, numOfActiveNetworks));
                            res = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());

                        }
                    }
                    else
                    {
                        // does not satisfy the frequency constraint. return frequency error
                        res = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.HomeNetworkFrequency, "Home network frequency limitation");
                    }
                }
            }

            return homeNetwork;
        }



    }
}
