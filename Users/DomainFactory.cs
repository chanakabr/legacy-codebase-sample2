using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;

namespace Users
{
    public static class DomainFactory
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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

            // check if user is valid
            User user = new User();
            if (!User.IsUserValid(nGroupID, nMasterUserGuid, ref user))
            {
                domain.m_DomainStatus = DomainStatus.Error;
                return domain;
            }

            // check if user is not already in another domain
            if (user.m_domianID != 0)
            {
                log.Error("Error - " + string.Format("User exists in other domain so it cannot be added to a new one. DomainStatus : {0} , G ID: {1} , D Name: {2} , Master: {3}, OtherDomain: {4}", domain.m_DomainStatus.ToString(), nGroupID, sDomainName, nMasterUserGuid, domain.m_nDomainID));

                domain.m_DomainStatus = DomainStatus.UserExistsInOtherDomains;
                return domain;
            }

            if (!string.IsNullOrEmpty(sCoGuid))
            {
                //Check if CoGuid already exists
                int nDomainID = DAL.DomainDal.GetDomainIDByCoGuid(sCoGuid, nGroupID);

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

            if (dom.m_DomainStatus == DomainStatus.DomainSuspended)
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

        internal static LimitationsManager GetDLM(int nGroupID, int nDomainLimitID)
        {
            LimitationsManager oLimitationsManager = null;
            try
            {
                DataSet ds = DomainDal.Get_GroupLimitsAndDeviceFamilies(nGroupID, nDomainLimitID);
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {

                    #region GroupLevel + DLM Level
                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0 &&
                        ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                    {
                        oLimitationsManager = new LimitationsManager();

                        DataRow drGroup = ds.Tables[0].Rows[0];
                        DataRow drDLM = ds.Tables[1].Rows[0];
                        if (drGroup != null && drDLM != null)
                        {
                            oLimitationsManager.domianLimitID = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "ID");
                            oLimitationsManager.DomainLimitName = ODBCWrapper.Utils.GetSafeStr(drDLM, "NAME");
                            int nConcurrencyGroupLevel = ODBCWrapper.Utils.GetIntSafeVal(drGroup, "GROUP_CONCURRENT_MAX_LIMIT");
                            oLimitationsManager.npvrQuotaInSecs = ODBCWrapper.Utils.GetIntSafeVal(drGroup, "npvr_quota_in_seconds");
                            int nConcurrencyDomainLevel = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "CONCURRENT_MAX_LIMIT");
                            oLimitationsManager.Frequency = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "freq_period_id");
                            oLimitationsManager.FrequencyDescription = Utils.GetMinPeriodDescription(oLimitationsManager.Frequency);
                            oLimitationsManager.Quantity = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "DEVICE_MAX_LIMIT");
                            oLimitationsManager.nUserLimit = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "USER_MAX_LIMIT");
                            oLimitationsManager.UserFrequency = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "user_freq_period_id");
                            oLimitationsManager.UserFrequencyDescrition = Utils.GetMinPeriodDescription(oLimitationsManager.UserFrequency);

                            oLimitationsManager.SetConcurrency(nConcurrencyDomainLevel, nConcurrencyGroupLevel);
                        }
                    }
                    #endregion

                    #region DeviceFamily
                    if (oLimitationsManager != null && ds.Tables.Count >= 4)
                    {
                        DataTable dt = ds.Tables[2];
                        DataTable dtSpecificLimits = ds.Tables[3];
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
                                dfl.Frequency = -1;

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
                                        else if (sLimitationType.ToLower() == "quantity")
                                        {
                                            dfl.quantity = nLimitationValue;
                                        }
                                        else if (sLimitationType.ToLower() == "frequency")
                                        {
                                            dfl.Frequency = nLimitationValue;
                                        }
                                    }
                                }
                                // if concurency / quntity is -1 take the value from the group itself.
                                if (dfl.concurrency == -1)
                                    dfl.concurrency = oLimitationsManager.Concurrency;
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

        public static void InitializeDLM(Domain domain)
        {
            domain.InitializeDLM();
        }
    }
}
