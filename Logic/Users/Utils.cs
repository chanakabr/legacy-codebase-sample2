using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using DAL;
using ApiObjects;

using System.Reflection;

using System.Security.Principal;
using System.Security.AccessControl;
using KLogMonitor;
using ApiObjects.Response;
using QueueWrapper.Queues.QueueObjects;
using TVinciShared;
using System.Xml;
using System.IO;
using System.Net;
using ApiObjects.Notification;
using System.Threading.Tasks;
using System.Web;
using System.ServiceModel;
using Core.Users.Cache;

namespace Core.Users
{
    public class Utils
    {
        private static string USERS_CONNECTION = "users_connection_string";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public const int USER_COGUID_LENGTH = 15;
        internal static readonly DateTime FICTIVE_DATE = new DateTime(2000, 1, 1); // fictive date. must match with the       
        internal static readonly int CONCURRENCY_MILLISEC_THRESHOLD = 65000; // default result of GetDateSafeVal in ODBCWrapper.Utils
        protected const string ROUTING_KEY_INITIATE_NOTIFICATION_ACTION = "PROCESS_INITIATE_NOTIFICATION_ACTION";


        static public Int32 GetGroupID(string sWSUserName, string sPass)
        {
            Credentials oCredentials = new Credentials(sWSUserName, sPass);
            Int32 nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.USERS, oCredentials);
            if (nGroupID == 0)
                log.Debug("WS ignored - eWSModules: eWSModules.USERS " + " UN: " + sWSUserName + " Pass: " + sPass);

            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sPass, string sFunctionName, ref BaseUsers baseUser)
        {
            Credentials oCredentials = new Credentials(sWSUserName, sPass);
            Int32 nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.USERS, oCredentials);
            if (nGroupID != 0)
            {
                Utils.GetBaseImpl(ref baseUser, nGroupID);
            }
            else
            {
                log.Debug("WS ignored - eWSModules: eWSModules.USERS " + " UN: " + sWSUserName + " Pass: " + sPass);
            }
            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sPass, string sFunctionName, ref KalturaBaseUsers user, int operatorId = -1)
        {
            Credentials oCredentials = new Credentials(sWSUserName, sPass);
            Int32 nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.USERS, oCredentials);

            if (nGroupID != 0)
                Utils.GetBaseImpl(ref user, nGroupID, operatorId);
            else
                log.Debug("WS ignored - eWSModules: eWSModules.USERS " + " UN: " + sWSUserName + " Pass: " + sPass);

            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sPass, string sFunctionName, ref BaseDomain t)
        {
            Credentials oCredentials = new Credentials(sWSUserName, sPass);
            Int32 nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.DOMAINS, oCredentials);

            if (nGroupID != 0)
                Utils.GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - eWSModules: eWSModules.DOMAINS " + " UN: " + sWSUserName + " Pass: " + sPass);

            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sPass, string sFunctionName, ref BaseDevice t)
        {
            Credentials oCredentials = new Credentials(sWSUserName, sPass);
            Int32 nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.DOMAINS, oCredentials);
            if (nGroupID != 0)
                Utils.GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - eWSModules: eWSModules.DOMAINS " + " UN: " + sWSUserName + " Pass: " + sPass);
            return nGroupID;
        }

        static public void GetBaseImpl(ref BaseUsers t, Int32 nGroupID)
        {
            int nImplID = TvinciCache.ModulesImplementation.GetModuleID(eWSModules.USERS, nGroupID, (int)ImplementationsModules.Users, USERS_CONNECTION);

            switch (nImplID)
            {
                case 1:
                    t = new TvinciUsers(nGroupID);
                    break;
                case 3:
                    t = new SSOUsers(nGroupID, 0);
                    break;
                case 4:
                    t = new MediaCorpUsers(nGroupID, -1);
                    break;
                case 6:
                    t = new EutelsatUsers(nGroupID);
                    break;
                default:
                    break;
            }
        }

        static public void GetBaseImpl(ref KalturaBaseUsers user, Int32 nGroupID, int operatorId = -1, string className = "User")
        {
            try
            {
                string moduleName = TvinciCache.ModulesImplementation.GetModuleName(eWSModules.USERS, nGroupID, (int)ImplementationsModules.Users, USERS_CONNECTION, operatorId);

                if (String.IsNullOrEmpty(moduleName))
                    user = new KalturaUsers(nGroupID);
                else
                {
                    // load user assembly
                    string usersAssemblyLocation = Utils.GetTcmConfigValue("USERS_ASSEMBLY_LOCATION");
                    Assembly userAssembly = Assembly.LoadFrom(string.Format(@"{0}{1}.dll", usersAssemblyLocation.EndsWith("\\") ? usersAssemblyLocation :
                        usersAssemblyLocation + "\\", moduleName));

                    // get user class 
                    Type userType = userAssembly.GetType(string.Format("{0}.{1}", moduleName, className));

                    if (operatorId == -1)
                    {
                        // regular user - constructor receives a single parameter
                        user = (KalturaUsers)Activator.CreateInstance(userType, nGroupID);
                    }
                    else
                    {
                        // SSO user - constructor receives 2 parameters
                        user = (KalturaUsers)Activator.CreateInstance(userType, nGroupID, operatorId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetBaseUsersImplModuleName Error - " + string.Format(" Error while trying to get user implementation for group ID: {0} error: {1}", nGroupID, ex.Message), ex);
            }
        }

        static public void GetBaseImpl(ref BaseEncrypter t, Int32 nGroupID)
        {
            int nImplID = 0;

            string key = string.Format("users_GetBaseEncrypterImpl_{0}", nGroupID);
            bool bRes = UsersCache.GetItem<int>(key, out nImplID);

            if (!bRes)
            {
                DataRow dr = DAL.UtilsDal.GetEncrypterData(nGroupID, "USERS_CONNECTION_STRING");

                if (dr != null)
                {
                    nImplID = ODBCWrapper.Utils.GetIntSafeVal(dr["ENCRYPTER_IMPLEMENTATION"]);
                    if (nImplID > 0)
                    {
                        UsersCache.AddItem(key, nImplID);
                    }
                }
            }

            switch (nImplID)
            {
                case 1:
                    t = new MD5Encrypter(nGroupID);
                    break;
                case 2:
                    t = new SHA1Encrypter(nGroupID);
                    break;
                case 3:
                    t = new SHA256Encrypter(nGroupID);
                    break;
                case 4:
                    t = new SHA384Encrypter(nGroupID);
                    break;
                default:
                    break;
            }

        }

        static public string GetTcmConfigValue(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        static public void GetBaseImpl(ref BaseDomain t, Int32 nGroupID)
        {
            int nImplID = TvinciCache.ModulesImplementation.GetModuleID(eWSModules.DOMAINS, nGroupID, (int)ImplementationsModules.Domains, USERS_CONNECTION);

            switch (nImplID)
            {
                case 1:
                    t = new TvinciDomain(nGroupID);
                    break;
                case 2:
                    t = new EutelsatDomain(nGroupID);
                    break;
                default:
                    break;
            }
        }

        static public void GetBaseImpl(ref BaseDevice t, Int32 nGroupID)
        {
            int nImplID = TvinciCache.ModulesImplementation.GetModuleID(eWSModules.USERS, nGroupID, (int)ImplementationsModules.Domains, USERS_CONNECTION);
            switch (nImplID)
            {
                case 1:
                case 2:
                    t = new TvinciDevice(nGroupID);
                    break;
                default:
                    break;
            }
        }

        static public BaseNewsLetterImpl GetBaseImpl(string apiKey, string listID, int implID)
        {
            BaseNewsLetterImpl retVal = null;
            if (implID == 1)
            {
                retVal = new MCNewsLetterImpl(apiKey, listID);
            }
            return retVal;
        }

        static public Country[] GetCountryList()
        {
            Country[] ret = null;

            string key = "users_GetCountryList";
            List<int> lCountryIDs;
            List<Country> lCountry;
            bool bRes = UsersCache.GetItem<List<Country>>(key, out  lCountry);
            if (!bRes)
            {
                Country oCountry;
                lCountry = new List<Country>();
                lCountryIDs = DAL.UtilsDal.GetAllCountries();
                foreach (int nCountryID in lCountryIDs)
                {
                    oCountry = new Country();
                    oCountry.Initialize(nCountryID);
                    lCountry.Add(oCountry);
                }

                UsersCache.AddItem(key, lCountry);
            }
            if (lCountry != null && lCountry.Count > 0)
            {
                ret = lCountry.ToArray();
            }
            return ret;
        }

        static public State[] GetStateList(Int32 nCountryID)
        {
            State[] ret = null;
            List<State> lState;
            string key = string.Format("users_GetStateList_{0}", nCountryID);
            bool bRes = UsersCache.GetItem<List<State>>(key, out lState);
            if (!bRes)
            {
                List<int> lStateIDs = DAL.UtilsDal.GetStatesByCountry(nCountryID);
                if (lStateIDs != null && lStateIDs.Count > 0)
                {
                    lState = new List<State>();
                    foreach (int nStateID in lStateIDs)
                    {
                        State oState = new State();
                        oState.Initialize(nStateID);
                        lState.Add(oState);
                    }
                    UsersCache.AddItem(key, lState);
                }
            }
            if (lState != null && lState.Count > 0)
            {
                ret = lState.ToArray();
            }

            return ret;
        }

        static public Country GetIPCountry2(string sIP)
        {
            Int32 nCountry = 0;
            string[] splited = sIP.Split('.');

            Int64 nIPVal = Int64.Parse(splited[3]) + Int64.Parse(splited[2]) * 256 + Int64.Parse(splited[1]) * 256 * 256 + Int64.Parse(splited[0]) * 256 * 256 * 256;
            //Int32 nID = 0;
            nCountry = DAL.UtilsDal.GetCountryIDFromIP(nIPVal);

            if (nCountry == 0)
            {
                return null;
            }

            Country ret = new Country();
            ret.Initialize(nCountry);
            return ret;
        }

        static public DateTime GetEndDateTime(DateTime dBase, Int32 nVal)
        {
            DateTime dRet = dBase;
            if (nVal == 1111111)
                dRet = dRet.AddMonths(1);
            else if (nVal == 2222222)
                dRet = dRet.AddMonths(2);
            else if (nVal == 3333333)
                dRet = dRet.AddMonths(3);
            else if (nVal == 4444444)
                dRet = dRet.AddMonths(4);
            else if (nVal == 5555555)
                dRet = dRet.AddMonths(5);
            else if (nVal == 6666666)
                dRet = dRet.AddMonths(6);
            else if (nVal == 9999999)
                dRet = dRet.AddMonths(9);
            else if (nVal == 11111111)
                dRet = dRet.AddYears(1);
            else if (nVal == 22222222)
                dRet = dRet.AddYears(2);
            else if (nVal == 33333333)
                dRet = dRet.AddYears(3);
            else if (nVal == 44444444)
                dRet = dRet.AddYears(4);
            else if (nVal == 55555555)
                dRet = dRet.AddYears(5);
            else if (nVal == 100000000)
                dRet = dRet.AddYears(10);
            else
                dRet = dRet.AddMinutes(nVal);
            return dRet;
        }

        static public BaseMailImpl GetBaseImpl(int nGroupID, int nRuleID, int nImpID)
        {
            BaseMailImpl retVal = null;

            switch (nImpID)
            {
                case 1:
                    retVal = new MCMailImpl(nGroupID, nRuleID);
                    break;

                default:
                    break;
            }

            return retVal;
        }

        static public bool SendMailTemplate(MailRequestObj request)
        {
            bool retVal = false;
            Mailer.IMailer mailer = Mailer.MailFactory.GetMailer(Mailer.MailImplementors.MCMailer);
            retVal = mailer.SendMailTemplate(request);
            return retVal;
        }

        static public bool SendMail(int nGroupID, MailRequestObj request)
        {
            return SendMailTemplate(request);
        }

        static public bool GetUserOperatorAndHouseholdIDs(int nGroupID, string sCoGuid, ref int nOperatorID, ref string sOperatorCoGuid, ref int nOperatorGroupID, ref int nHouseholdID)
        {
            if (string.IsNullOrEmpty(sCoGuid))
            {
                return false;
            }

            if (sCoGuid.Length != USER_COGUID_LENGTH)
            {
                return false;
            }

            try
            {
                // IPNO ID - First 8 characters; HouseHold ID - last 7 characters
                sOperatorCoGuid = sCoGuid.Substring(0, 8);
                nHouseholdID = int.Parse(sCoGuid.Substring(8, 7));

                nOperatorGroupID = DAL.UtilsDal.GetOperatorGroupID(nGroupID, sOperatorCoGuid, ref nOperatorID);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("GetUserOperatorAndHouseholdIDs. Exception. ");
                sb.Append(String.Concat("Group ID: ", nGroupID));
                sb.Append(String.Concat(" CoGuid: ", sCoGuid));
                sb.Append(String.Concat(" Msg: ", ex.Message));
                sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));

                log.Debug("GetUserOperatorAndHouseholdIDs - " + sb.ToString());

                return false;
            }

            return true;
        }

        static public bool SetPassword(string sPassword, ref UserBasicData oBasicData, int nGroupID)
        {
            if (sPassword.Length > 0)
            {
                // check if we need to encrypt the password
                BaseEncrypter encrypter = null;

                Utils.GetBaseImpl(ref encrypter, nGroupID);
                // if encrypter is null the group does not have an encrypter support
                if (encrypter != null)
                {
                    string sEncryptedPassword = string.Empty;
                    string sSalt = string.Empty;

                    encrypter.GenerateEncryptPassword(sPassword, ref sEncryptedPassword, ref sSalt);

                    oBasicData.m_sPassword = sEncryptedPassword;
                    oBasicData.m_sSalt = sSalt;
                }
                else
                {
                    oBasicData.m_sPassword = sPassword;
                    oBasicData.m_sSalt = string.Empty;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        internal static List<HomeNetwork> GetHomeNetworksOfDomain(long lDomainID, int nGroupID, bool bCache = false)
        {
            List<HomeNetwork> res = null;
            DomainsCache oDomainCache = DomainsCache.Instance();

            if (bCache)
            {
                int nDomainID = (int)lDomainID;
                // need to get Domain from cache                 
                Domain oDomain = oDomainCache.GetDomain(nDomainID, nGroupID);
                if (oDomain != null && oDomain.m_homeNetworks != null && oDomain.m_homeNetworks.Count > 0)
                {
                    res = oDomain.m_homeNetworks;
                }
                if (res != null)
                {
                    return res;
                }
                bCache = false; // need to go get data from DB
            }
            if (!bCache)
            {
                // res from Cache return null - go to get details from DB
                DataTable dt = DomainDal.Get_DomainHomeNetworks(lDomainID, nGroupID);

                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    int length = dt.Rows.Count;
                    res = new List<HomeNetwork>(length);
                    for (int i = 0; i < length; i++)
                    {
                        HomeNetwork hn = new HomeNetwork();
                        hn.UID = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["NETWORK_ID"]);
                        if (string.IsNullOrEmpty(hn.UID))
                            continue;
                        hn.Name = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["NAME"]);
                        hn.Description = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["DESCRIPTION"]);
                        hn.IsActive = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["IS_ACTIVE"]) != 0;
                        hn.CreateDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[i]["CREATE_DATE"]);

                        res.Add(hn);
                    }
                    // remove current domain from cache
                    oDomainCache.RemoveDomain((int)lDomainID);
                }
                else
                {
                    res = new List<HomeNetwork>(0);
                }
            }
            return res;
        }

        internal static HomeNetwork Update_HomeNetworkWithoutDeactivationDate(long lDomainID, string sNetworkID, int nGroupID, string sName,
            string sDesc, bool bIsActive)
        {
            DataRow row = DomainDal.Update_HomeNetworkWithoutDeactivationDate(lDomainID, sNetworkID, nGroupID, sName, sDesc, bIsActive);
            HomeNetwork homeNetwork = new HomeNetwork()
            {
                UID = ODBCWrapper.Utils.GetSafeStr(row["NETWORK_ID"]),
                Name = ODBCWrapper.Utils.GetSafeStr(row["NAME"]),
                Description = ODBCWrapper.Utils.GetSafeStr(row["DESCRIPTION"]),
                IsActive = ODBCWrapper.Utils.GetIntSafeVal(row["IS_ACTIVE"]) != 0,
                CreateDate = ODBCWrapper.Utils.GetDateSafeVal(row["CREATE_DATE"])
            };

            return homeNetwork;
        }

        internal static HomeNetwork Update_HomeNetworkWithDeactivationDate(long lDomainID, string sNetworkID, int nGroupID, string sName,
            string sDesc, bool bTrueForDeactivationFalseForDeletion)
        {
            DataRow row = DomainDal.Update_HomeNetworkWithDeactivationDate(lDomainID, sNetworkID, nGroupID, sName, sDesc, bTrueForDeactivationFalseForDeletion);
            HomeNetwork homeNetwork = new HomeNetwork()
            {
                UID = ODBCWrapper.Utils.GetSafeStr(row["NETWORK_ID"]),
                Name = ODBCWrapper.Utils.GetSafeStr(row["NAME"]),
                Description = ODBCWrapper.Utils.GetSafeStr(row["DESCRIPTION"]),
                IsActive = ODBCWrapper.Utils.GetIntSafeVal(row["IS_ACTIVE"]) != 0,
                CreateDate = ODBCWrapper.Utils.GetDateSafeVal(row["CREATE_DATE"])
            };

            return homeNetwork;
        }


        static public bool IsGroupIDContainedInConfig(long lGroupID, string sKey, char cSeperator)
        {
            bool res = false;
            string rawStrFromConfig = GetTcmConfigValue(sKey);
            if (rawStrFromConfig.Length > 0)
            {
                string[] strArrOfIDs = rawStrFromConfig.Split(cSeperator);
                if (strArrOfIDs != null && strArrOfIDs.Length > 0)
                {
                    List<long> listOfIDs = strArrOfIDs.Select(s =>
                    {
                        long l = 0;
                        if (Int64.TryParse(s, out l))
                            return l;
                        return 0;
                    }).ToList();

                    res = listOfIDs.Contains(lGroupID);
                }
            }

            return res;
        }

        internal static MutexSecurity CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            MutexSecurity mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.Delete, AccessControlType.Deny));

            return mutexSecurity;
        }

        public static string GetMinPeriodDescription(int id)
        {
            string res = null;
            Dictionary<string, string> minPeriods;
            if (CachingManager.CachingManager.Exist("MinPeriods"))
            {
                minPeriods = CachingManager.CachingManager.GetCachedData("MinPeriods") as Dictionary<string, string>;
            }
            else
            {
                minPeriods = Tvinci.Core.DAL.CatalogDAL.GetMinPeriods();
                if (minPeriods != null)
                    CachingManager.CachingManager.SetCachedData("MinPeriods", minPeriods, 604800, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }

            if (minPeriods != null)
                minPeriods.TryGetValue(id.ToString(), out res);

            return res;
        }

        static public void GetContentInfo(ref string subject, string key, Dictionary<string, string> info)
        {
            if (info.ContainsKey(key))
            {
                subject = info[key];
            }
            else if (info.ContainsKey(key.ToLower()))
            {
                subject = info[key.ToLower()];
            }
        }

        internal static string DateToFilename(DateTime dateTime)
        {
            return
                (string.Format("{0:dd-MM-yyyy_hh-mm-ss}", dateTime));
        }

        public static eResponseStatus ConvertResponseStatus(ResponseStatus status)
        {
            eResponseStatus result;

            switch (status)
            {
                case ResponseStatus.OK:
                    result = eResponseStatus.OK;
                    break;
                case ResponseStatus.UserExists:
                    result = eResponseStatus.UserExists;
                    break;
                case ResponseStatus.UserDoesNotExist:
                    result = eResponseStatus.UserDoesNotExist;
                    break;
                case ResponseStatus.WrongPasswordOrUserName:
                    result = eResponseStatus.WrongPasswordOrUserName;
                    break;
                case ResponseStatus.InsideLockTime:
                    result = eResponseStatus.InsideLockTime;
                    break;
                case ResponseStatus.UserNotActivated:
                    result = eResponseStatus.UserNotActivated;
                    break;
                case ResponseStatus.UserAllreadyLoggedIn:
                    result = eResponseStatus.UserAllreadyLoggedIn;
                    break;
                case ResponseStatus.UserDoubleLogIn:
                    result = eResponseStatus.UserDoubleLogIn;
                    break;
                case ResponseStatus.DeviceNotRegistered:
                    result = eResponseStatus.DeviceNotRegistered;
                    break;
                case ResponseStatus.UserNotMasterApproved:
                    result = eResponseStatus.UserNotMasterApproved;
                    break;
                case ResponseStatus.ErrorOnInitUser:
                    result = eResponseStatus.ErrorOnInitUser;
                    break;
                case ResponseStatus.UserNotIndDomain:
                    result = eResponseStatus.UserNotInDomain;
                    break;
                case ResponseStatus.UserWithNoDomain:
                    result = eResponseStatus.UserWithNoDomain;
                    break;
                case ResponseStatus.UserSuspended:
                    result = eResponseStatus.UserSuspended;
                    break;
                case ResponseStatus.UserTypeNotExist:
                    result = eResponseStatus.UserTypeDoesNotExist;
                    break;
                case ResponseStatus.TokenNotFound:
                    result = eResponseStatus.ActivationTokenNotFound;
                    break;
                case ResponseStatus.UserAlreadyMasterApproved:
                    result = eResponseStatus.UserAlreadyMasterApproved;
                    break;
                case ResponseStatus.LoginServerDown:
                    result = eResponseStatus.LoginServerDown;
                    break;
                default:
                    result = eResponseStatus.Error;
                    break;
            }

            return result;
        }


        public static ApiObjects.Response.Status ConvertResponseStatusToResponseObject(ResponseStatus status)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status();

            switch (status)
            {
                case ResponseStatus.OK:
                case ResponseStatus.UserCreatedWithNoRole:
                    result.Code = (int)eResponseStatus.OK;
                    result.Message = "OK";
                    break;
                case ResponseStatus.UserExists:
                    result.Code = (int)eResponseStatus.UserExists;
                    result.Message = "User exists";
                    break;
                case ResponseStatus.UserDoesNotExist:
                    result.Code = (int)eResponseStatus.UserDoesNotExist;
                    result.Message = "User does not exist";
                    break;
                case ResponseStatus.WrongPasswordOrUserName:
                    result.Code = (int)eResponseStatus.WrongPasswordOrUserName;
                    result.Message = "Wrong username or password";
                    break;
                case ResponseStatus.InsideLockTime:
                    result.Code = (int)eResponseStatus.InsideLockTime;
                    result.Message = "Inside lock time";
                    break;
                case ResponseStatus.UserNotActivated:
                    result.Code = (int)eResponseStatus.UserNotActivated;
                    result.Message = "User not activated";
                    break;
                case ResponseStatus.UserAllreadyLoggedIn:
                    result.Code = (int)eResponseStatus.UserAllreadyLoggedIn;
                    result.Message = "User already logged in";
                    break;
                case ResponseStatus.UserDoubleLogIn:
                    result.Code = (int)eResponseStatus.UserDoubleLogIn;
                    result.Message = "User double login";
                    break;
                case ResponseStatus.DeviceNotRegistered:
                    result.Code = (int)eResponseStatus.DeviceNotRegistered;
                    result.Message = "Device not registered";
                    break;
                case ResponseStatus.UserNotMasterApproved:
                    result.Code = (int)eResponseStatus.UserNotMasterApproved;
                    result.Message = "User not master approved";
                    break;
                case ResponseStatus.ErrorOnInitUser:
                    result.Code = (int)eResponseStatus.ErrorOnInitUser;
                    result.Message = "Error on init user";
                    break;
                case ResponseStatus.UserNotIndDomain:
                    result.Code = (int)eResponseStatus.UserNotInDomain;
                    result.Message = "User not in household";
                    break;
                case ResponseStatus.UserWithNoDomain:
                    result.Code = (int)eResponseStatus.UserWithNoDomain;
                    result.Message = "User with no household";
                    break;
                case ResponseStatus.UserSuspended:
                    result.Code = (int)eResponseStatus.UserSuspended;
                    result.Message = "User suspended";
                    break;
                case ResponseStatus.UserTypeNotExist:
                    result.Code = (int)eResponseStatus.UserTypeDoesNotExist;
                    result.Message = "User type does not exist";
                    break;
                case ResponseStatus.TokenNotFound:
                    result.Code = (int)eResponseStatus.ActivationTokenNotFound;
                    result.Message = "Activation token not found";
                    break;
                case ResponseStatus.UserAlreadyMasterApproved:
                    result.Code = (int)eResponseStatus.UserAlreadyMasterApproved;
                    result.Message = "User already master approved";
                    break;
                case ResponseStatus.LoginServerDown:
                    result.Message = "Login server down";
                    result.Code = (int)eResponseStatus.LoginServerDown;
                    break;
                default:
                    result.Code = (int)eResponseStatus.Error;
                    result.Message = "Error";
                    break;
            }

            return result;
        }


        public static ApiObjects.Response.Status ConvertDomainResponseStatusToResponseObject(DomainResponseStatus status)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status();

            switch (status)
            {
                case DomainResponseStatus.LimitationPeriod:
                    result.Code = (int)eResponseStatus.LimitationPeriod;
                    result.Message = "Limitation period";
                    break;
                case DomainResponseStatus.DomainAlreadyExists:
                    result.Code = (int)eResponseStatus.DomainAlreadyExists;
                    result.Message = "Household already exists";
                    break;
                case DomainResponseStatus.ExceededLimit:
                    result.Code = (int)eResponseStatus.ExceededLimit;
                    result.Message = "Exceeded limit";
                    break;
                case DomainResponseStatus.DeviceTypeNotAllowed:
                    result.Code = (int)eResponseStatus.DeviceTypeNotAllowed;
                    result.Message = "Device type not allowed";
                    break;
                case DomainResponseStatus.DeviceNotInDomain:
                    result.Code = (int)eResponseStatus.DeviceNotInDomain;
                    result.Message = "Device not in household";
                    break;
                case DomainResponseStatus.DeviceNotExists:
                    result.Code = (int)eResponseStatus.DeviceNotExists;
                    result.Message = "Device does not exist";
                    break;
                case DomainResponseStatus.DeviceAlreadyExists:
                    result.Code = (int)eResponseStatus.DeviceAlreadyExists;
                    result.Message = "Device already exists";
                    break;
                case DomainResponseStatus.UserNotExistsInDomain:
                    result.Code = (int)eResponseStatus.UserNotExistsInDomain;
                    result.Message = "User not in household";
                    break;
                case DomainResponseStatus.RequestSent:
                case DomainResponseStatus.OK:
                    result.Code = (int)eResponseStatus.OK;
                    result.Message = "OK";
                    break;
                case DomainResponseStatus.ActionUserNotMaster:
                    result.Code = (int)eResponseStatus.ActionUserNotMaster;
                    result.Message = "Action user not master";
                    break;
                case DomainResponseStatus.UserNotAllowed:
                    result.Code = (int)eResponseStatus.UserNotAllowed;
                    result.Message = "User not allowed";
                    break;
                case DomainResponseStatus.ExceededUserLimit:
                    result.Code = (int)eResponseStatus.ExceededUserLimit;
                    result.Message = "Exceeded user limit";
                    break;
                case DomainResponseStatus.NoUsersInDomain:
                    result.Code = (int)eResponseStatus.NoUsersInDomain;
                    result.Message = "No users in household";
                    break;
                case DomainResponseStatus.UserExistsInOtherDomains:
                    result.Code = (int)eResponseStatus.UserExistsInOtherDomains;
                    result.Message = "User exists in other households";
                    break;
                case DomainResponseStatus.DomainNotExists:
                    result.Code = (int)eResponseStatus.DomainNotExists;
                    result.Message = "Household does not exists";
                    break;
                case DomainResponseStatus.HouseholdUserFailed:
                    result.Code = (int)eResponseStatus.HouseholdUserFailed;
                    result.Message = "Household user failed";
                    break;
                case DomainResponseStatus.DeviceExistsInOtherDomains:
                    result.Code = (int)eResponseStatus.DeviceExistsInOtherDomains;
                    result.Message = "Device exists in other households";
                    break;
                case DomainResponseStatus.DomainNotInitialized:
                    result.Code = (int)eResponseStatus.DomainNotInitialized;
                    result.Message = "Household not initialized";
                    break;
                case DomainResponseStatus.DeviceNotConfirmed:
                    result.Code = (int)eResponseStatus.DeviceNotConfirmed;
                    result.Message = "Device not confirmed";
                    break;
                case DomainResponseStatus.RequestFailed:
                    result.Code = (int)eResponseStatus.RequestFailed;
                    result.Message = "Request failed ";
                    break;
                case DomainResponseStatus.InvalidUser:
                    result.Code = (int)eResponseStatus.InvalidUser;
                    result.Message = "Invalid user";
                    break;
                case DomainResponseStatus.ConcurrencyLimitation:
                    result.Code = (int)eResponseStatus.ConcurrencyLimitation;
                    result.Message = "Concurrency limitation";
                    break;
                case DomainResponseStatus.MediaConcurrencyLimitation:
                    result.Code = (int)eResponseStatus.MediaConcurrencyLimitation;
                    result.Message = "Media concurrency limitation";
                    break;
                case DomainResponseStatus.DomainSuspended:
                    result.Code = (int)eResponseStatus.DomainSuspended;
                    result.Message = "Household suspended";
                    break;
                case DomainResponseStatus.UserAlreadyInDomain:
                    result.Code = (int)eResponseStatus.UserAlreadyInDomain;
                    result.Message = "User already in Household";
                    break;
                default:
                    result.Code = (int)eResponseStatus.Error;
                    result.Message = "Error";
                    break;
            }

            return result;
        }

        public static ApiObjects.Response.Status ConvertDomainStatusToResponseObject(DomainStatus status)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status();

            switch (status)
            {
                case DomainStatus.NoUsersInDomain:
                case DomainStatus.DomainSuspended:
                case DomainStatus.DomainCreatedWithoutNPVRAccount:
                case DomainStatus.OK:
                    result.Code = (int)eResponseStatus.OK;
                    result.Message = "OK";
                    break;
                case DomainStatus.DomainAlreadyExists:
                    result.Code = (int)eResponseStatus.DomainAlreadyExists;
                    result.Message = "Household already exists";
                    break;
                case DomainStatus.ExceededLimit:
                    result.Code = (int)eResponseStatus.ExceededLimit;
                    result.Message = "Exceeded limit";
                    break;
                case DomainStatus.DeviceTypeNotAllowed:
                    result.Code = (int)eResponseStatus.DeviceTypeNotAllowed;
                    result.Message = "Device type not allowed";
                    break;
                case DomainStatus.DeviceNotInDomin:
                    result.Code = (int)eResponseStatus.DeviceNotInDomain;
                    result.Message = "Device not in household";
                    break;
                case DomainStatus.MasterEmailAlreadyExists:
                    result.Code = (int)eResponseStatus.MasterEmailAlreadyExists;
                    result.Message = "Master email already exists";
                    break;
                case DomainStatus.UserNotInDomain:
                    result.Code = (int)eResponseStatus.UserNotInDomain;
                    result.Message = "User not in household";
                    break;
                case DomainStatus.DomainNotExists:
                    result.Code = (int)eResponseStatus.DomainNotExists;
                    result.Message = "Household does not exist";
                    break;
                case DomainStatus.HouseholdUserFailed:
                    result.Code = (int)eResponseStatus.HouseholdUserFailed;
                    result.Message = "Household user failed";
                    break;
                case DomainStatus.UserExistsInOtherDomains:
                    result.Code = (int)eResponseStatus.UserExistsInOtherDomains;
                    result.Message = "User exists in other households";
                    break;
                default:
                    result.Code = (int)eResponseStatus.Error;
                    result.Message = "Error";
                    break;
            }

            return result;
        }

        public static ApiObjects.Response.Status ConvertDeviceResponseStatusToResponseObject(DeviceResponseStatus status)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status();

            switch (status)
            {
                case DeviceResponseStatus.DuplicatePin:
                    result.Code = (int)eResponseStatus.DuplicatePin;
                    result.Message = "Duplicate pin";
                    break;
                case DeviceResponseStatus.DeviceNotExists:
                    result.Code = (int)eResponseStatus.DeviceNotExists;
                    result.Message = "Device does not exists";
                    break;
                case DeviceResponseStatus.OK:
                    result.Code = (int)eResponseStatus.OK;
                    result.Message = "OK";
                    break;
                case DeviceResponseStatus.ExceededLimit:
                    result.Code = (int)eResponseStatus.ExceededLimit;
                    result.Message = "Exceeded limit";
                    break;
                default:
                    result.Code = (int)eResponseStatus.Error;
                    result.Message = "Error";
                    break;
            }

            return result;
        }

        internal static ApiObjects.Response.Status ConvertDeviceStateToResponseObject(DeviceState deviceState)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status();

            switch (deviceState)
            {
                case DeviceState.NotExists:
                    result.Code = (int)eResponseStatus.DeviceNotExists;
                    result.Message = "Device does not exists";
                    break;
                //case DeviceState.Pending:
                //    result.Code = (int)eResponseStatus.dev;
                //    result.Message = "Device des not exists";
                //    break;
                //case DeviceState.Activated:
                //     result.Code = (int)eResponseStatus.DeviceNotExists;
                //    result.Message = "Device des not exists";
                //    break;
                //case DeviceState.UnActivated:
                //     result.Code = (int)eResponseStatus.DeviceNotExists;
                //    result.Message = "Device des not exists";
                //    break;
                case DeviceState.UnKnown:
                case DeviceState.Error:
                default:
                    result.Code = (int)eResponseStatus.Error;
                    result.Message = "Error";
                    break;
            }

            return result;
        }

        public static bool IsDeleteUserAllowedForGroup(int groupId)
        {
            bool isAllowed = false;
            DataTable dt = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
            selectQuery += "select allow_delete_user from groups_parameters where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupId);
            dt = selectQuery.Execute("query", true);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                int isGroupAllowed = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "allow_delete_user");
                if (isGroupAllowed == 1)
                {
                    isAllowed = true;
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return isAllowed;
        }

        //public static bool AddInitiateNotificationActionToQueue(int groupId, eUserMessageAction userAction, int userId, string udid, string pushToken = "")
        //{
        //    InitiateNotificationActionQueue que = new InitiateNotificationActionQueue();
        //    ApiObjects.QueueObjects.UserNotificationData messageAnnouncementData = new ApiObjects.QueueObjects.UserNotificationData(groupId, (int)userAction, userId, udid, pushToken);

        //    bool res = que.Enqueue(messageAnnouncementData, ROUTING_KEY_INITIATE_NOTIFICATION_ACTION);

        //    if (res)
        //        log.DebugFormat("Successfully inserted a message to notification action queue. user id: {0}, device id: {1}, push token: {2}, group id: {3}, user action: {4}", userId, udid, pushToken, groupId, userAction);
        //    else
        //        log.ErrorFormat("Error while inserting to notification action queue.  user id: {0}, device id: {1}, push token: {2}, group id: {3}, user action: {4}", userId, udid, pushToken, groupId, userAction);

        //    return res;
        //}

        public static void AddInitiateNotificationAction(int groupId, eUserMessageAction userAction, int userId, string udid, string pushToken = "")
        {
            string response = string.Empty;
            string wsUsername = string.Empty;
            string wsPassword = string.Empty;
            GetWSCredentials(groupId, eWSModules.NOTIFICATIONS, ref wsUsername, ref wsPassword);
            string Address = Utils.GetTcmConfigValue("NotificationService");
            string SoapAction = "http://tempuri.org/INotificationService/InitiateNotificationAction";
            if (string.IsNullOrEmpty(Address) || string.IsNullOrEmpty(wsUsername) || string.IsNullOrEmpty(wsPassword))
            {
                log.DebugFormat("address or wsUsername or wsPassword is empty. Address: {0} , wsUsername: {1}, wsPassword: {2}", Address, wsUsername, wsPassword);
                return;
            }
            try
            {
                string RequestData = CreateInitiazeNotificationACtionSoapRequest(wsUsername, wsPassword, userAction, userId, udid, pushToken);
                string ErrorMsg = string.Empty;
                Task.Factory.StartNew(() => TVinciShared.WS_Utils.SendXMLHttpReqWithHeaders(Address, RequestData, new Dictionary<string, string>() { { "SOAPAction", SoapAction } }));
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Error while inserting to notification. groupId: {0}, userAction: {1}, userId: {2}, udid: {3}, pushToken: {4}, Exception: {5}",
                                    groupId, userAction, userId, udid, pushToken, ex.Message);
            }
        }

        private static string CreateInitiazeNotificationACtionSoapRequest(string wsUserName, string wsPassword, eUserMessageAction action, int userId, string udid, string pushToken)
        {
            string request = string.Empty;
            //  validate request
            //  if()
            request = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
                                        <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
                                          <soap:Body>
                                            <InitiateNotificationAction xmlns=""http://tempuri.org/"">
                                                <sWSUserName>{0}</sWSUserName>
                                                <sWSPassword>{1}</sWSPassword>
                                                <userAction>{2}</userAction>
                                                <userId>{3}</userId>
                                                <udid>{4}</udid>
                                                <pushToken>{5}</pushToken>
                                                </InitiateNotificationAction>
                                            </soap:Body>
                                        </soap:Envelope>", wsUserName, wsPassword, action, userId, udid, pushToken);

            return request;
        }

        public static void GetWSCredentials(int nGroupID, eWSModules eWSModule, ref string sUN, ref string sPass)
        {
            Credentials uc = TvinciCache.WSCredentials.GetWSCredentials(eWSModules.USERS, nGroupID, eWSModule);
            sUN = uc.m_sUsername;
            sPass = uc.m_sPassword;
        }


        public static Tuple<List<long>, bool> Get_UserRoleIds(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<long> roleIds = null;

            try
            {
                if (funcParams != null && funcParams.Count == 2)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("userId"))
                    {
                        int? groupId;
                        string userId;
                        groupId = funcParams["groupId"] as int?;
                        userId = funcParams["userId"] as string;
                        if (groupId.HasValue && !string.IsNullOrEmpty(userId))
                        {
                            roleIds = UsersDal.Get_UserRoleIds(groupId.Value, userId);
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetUserRoleIds failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<List<long>, bool>(roleIds, res);
        }
    }

}
