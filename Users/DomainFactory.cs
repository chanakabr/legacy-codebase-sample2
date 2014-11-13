using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public static class DomainFactory
    {

        public static Domain GetDomain(int nGroupID, int nDomainID, bool checkAddMonkey = false)
        {
            Domain d = new Domain(nDomainID);

            if (!d.Initialize(nGroupID, nDomainID))
            {
                d.m_DomainStatus = DomainStatus.Error;
                return d;
            }

            if (checkAddMonkey)
            {
                d = DomainFactory.CheckAddMonkey(d);
            }

            return d;
        }

        public static Domain GetDomain(int nGroupID, int nDomainID, int nSubGroupID)
        {
            Domain d = new Domain(nDomainID);

            if (!d.Initialize(nGroupID, nDomainID, nSubGroupID))
            {
                d.m_DomainStatus = DomainStatus.Error;
            }
            else
            {
                d = DomainFactory.CheckAddMonkey(d);
            }

            return d;
        }

        public static Domain GetDomain(string sName, string sDescription, int nGroupID, int nDomainID)
        {
            Domain d = new Domain(nDomainID);

            if (!d.Initialize(sName, sDescription, nGroupID, nDomainID))
            {
                d.m_DomainStatus = DomainStatus.Error;
            }
            else
            {
                d = DomainFactory.CheckAddMonkey(d);
            }

            return d;
        }

        public static Domain CreateDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID, string sCoGuid)
        {
            //Create new domain
            Domain domain = new Domain();


            if (!User.IsUserValid(nGroupID, nMasterUserGuid))
            {
                domain.m_DomainStatus = DomainStatus.Error;
                return domain;
            }

            if (!string.IsNullOrEmpty(sCoGuid))
            {
                //Check if CoGuid already exists
                int nDomainID = DAL.DomainDal.GetDomainIDByCoGuid(sCoGuid);

                if (nDomainID > 0)
                {
                    domain.m_DomainStatus = DomainStatus.DomainAlreadyExists;

                    return domain;  
                }
            }


            // Create new domain
            Domain oNewDomain = domain.CreateNewDomain(sDomainName, sDomainDescription, nGroupID, nMasterUserGuid, sCoGuid);
            oNewDomain = DomainFactory.CheckAddMonkey(oNewDomain);

            return oNewDomain;
        }


        public static Domain CheckAddMonkey(Domain dom)
        {
            if (dom == null)
            {
                return dom;
            }

            if (dom.m_DefaultUsersIDs != null && dom.m_DefaultUsersIDs.Count > 0)
            {
                return dom;
            }

            Domain resDomain = dom;
            int masterUserID = (resDomain.m_masterGUIDs != null && resDomain.m_masterGUIDs.Count > 0) ? resDomain.m_masterGUIDs[0] : 0;
            if (masterUserID <= 0)
            {
                resDomain.m_DomainStatus = DomainStatus.Error;
                return resDomain;
            }
                
            User masterUser = new User(resDomain.m_nGroupID, masterUserID);
            if (masterUser == null)
            {
                resDomain.m_DomainStatus = DomainStatus.Error;
                return resDomain;
            }

            if (masterUser != null)
            {
                try
                {
                    User monkeyUser = masterUser.Clone();

                    monkeyUser.m_sSiteGUID = string.Empty;
                    monkeyUser.m_oBasicData.m_sUserName = "{" + resDomain.m_nDomainID + "}_{Household}"; // (resDomain.m_nDomainID + "||" + Guid.NewGuid().ToString());
                    monkeyUser.m_oBasicData.m_sFacebookID = string.Empty;
                    monkeyUser.m_oBasicData.m_sFacebookImage = string.Empty;
                    monkeyUser.m_oBasicData.m_sFacebookToken = string.Empty;
                    monkeyUser.m_oBasicData.m_CoGuid = string.Empty;

                    int monkeyID = monkeyUser.Save(resDomain.m_nGroupID, true);

                    if ((monkeyID <= 0) || (string.IsNullOrEmpty(monkeyUser.m_sSiteGUID)))
                    {
                        resDomain.m_DomainStatus = DomainStatus.HouseholdUserFailed;
                    }
                    else
                    {
                        DomainResponseStatus addedMonkey = resDomain.AddUserToDomain(resDomain.m_nGroupID, resDomain.m_nDomainID, monkeyID, masterUserID, UserDomainType.Household);

                        if (addedMonkey != DomainResponseStatus.OK)
                        {
                            resDomain.m_DomainStatus = DomainStatus.HouseholdUserFailed;
                        }
                    }
                }
                catch { }
            }

            return resDomain;
        }

    }
}
