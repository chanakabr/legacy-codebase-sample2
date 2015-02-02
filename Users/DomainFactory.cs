using DAL;
using System;
using System.Collections.Generic;
using System.Data;
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

        internal static LimitationsManager GetDLM(int nGroupID, int nDomainLimitID, DateTime dtLastActionDate)
        {
            LimitationsManager oLimitationsManager = null;
            try
            {
                DataSet ds = DomainDal.Get_GroupLimitsAndDeviceFamilies(nGroupID, nDomainLimitID);
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    oLimitationsManager = new LimitationsManager();

                    #region GroupLevel
                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow drGroup = ds.Tables[0].Rows[0];
                        if (drGroup != null)
                        {

                            int nConcurrencyDomainLevel = ODBCWrapper.Utils.GetIntSafeVal(drGroup, "CONCURRENT_MAX_LIMIT");
                            int nConcurrencyGroupLevel = ODBCWrapper.Utils.GetIntSafeVal(drGroup, "GROUP_CONCURRENT_MAX_LIMIT");

                            oLimitationsManager.SetConcurrency(nConcurrencyDomainLevel, nConcurrencyGroupLevel);
                            oLimitationsManager.Frequency = ODBCWrapper.Utils.GetIntSafeVal(drGroup, "freq_period_id");
                            oLimitationsManager.Quantity = ODBCWrapper.Utils.GetIntSafeVal(drGroup, "DEVICE_MAX_LIMIT");
                            oLimitationsManager.npvrQuotaInSecs = ODBCWrapper.Utils.GetIntSafeVal(drGroup, "npvr_quota_in_seconds");
                            oLimitationsManager.nUserLimit = ODBCWrapper.Utils.GetIntSafeVal(drGroup, "USER_MAX_LIMIT");

                            if (dtLastActionDate == null || dtLastActionDate.Equals(Utils.FICTIVE_DATE) || dtLastActionDate.Equals(DateTime.MinValue) || oLimitationsManager.Frequency == 0)
                                oLimitationsManager.NextActionFreqDate = DateTime.MinValue;
                            else
                                oLimitationsManager.NextActionFreqDate = Utils.GetEndDateTime(dtLastActionDate, oLimitationsManager.Frequency);

                        }
                    }
                    #endregion

                    #region DeviceFamily
                    if (ds.Tables.Count >= 3)
                    {
                        DataTable dt = ds.Tables[1];
                        DataTable dtSpecificLimits = ds.Tables[2];
                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            oLimitationsManager.lDeviceFamilyLimitations = new List<DeviceFamilyLimitations>();
                            DeviceFamilyLimitations dfl = new DeviceFamilyLimitations();
                            foreach (DataRow dr in dt.Rows)
                            {
                                dfl = new DeviceFamilyLimitations();
                                dfl.deviceFamily = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                                dfl.deviceFamilyName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                                dfl.concurrency = -1;
                                dfl.quantity = -1;

                                DataRow[] drSpecific = dtSpecificLimits.Select("device_family_id = " + dfl.deviceFamily);
                                foreach (DataRow drItem in drSpecific)
                                {
                                    string sLimitationType = ODBCWrapper.Utils.GetSafeStr(drItem, "description");
                                    int nLimitationValue = ODBCWrapper.Utils.GetIntSafeVal(drItem, "value", -1);

                                    if (dfl.deviceFamily > 0 && nLimitationValue > -1 && sLimitationType.Length > 0)
                                    {
                                        if (sLimitationType.ToLower() == "concurrency")
                                        {
                                            dfl.concurrency = nLimitationValue;
                                        }
                                        else
                                        {
                                            if (sLimitationType.ToLower() == "quantity")
                                            {
                                                dfl.quantity = nLimitationValue;
                                            }
                                        }
                                    }
                                }
                                // if concurency / quntity is -1 take the value from the group itself.
                                if (dfl.concurrency == -1)
                                    dfl.concurrency =  oLimitationsManager.Concurrency;
                                if (dfl.quantity == -1)
                                dfl.quantity = oLimitationsManager.Quantity;

                                oLimitationsManager.lDeviceFamilyLimitations.Add(dfl);
                            }
                        }
                    }
                    #endregion
                }
                    
                return oLimitationsManager;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
