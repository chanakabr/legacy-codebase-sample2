using ApiObjects;
using ApiObjects.DRM;
using ApiObjects.Response;
using Phx.Lib.Appconfig;
using Core.Users.Cache;
using DAL;
using Phx.Lib.Log;
using QueueWrapper;
using QueueWrapper.Queues.QueueObjects;
using ScheduledTasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using APILogic.Users;
using TVinciShared;
using ApiObjects.EventBus;

namespace Core.Users
{
    public class Utils
    {
        private static string USERS_CONNECTION = "users_connection_string";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public const int USER_COGUID_LENGTH = 15;
        internal static readonly DateTime FICTIVE_DATE = new DateTime(2000, 1, 1); // fictive date. must match with the       
        protected const string ROUTING_KEY_INITIATE_NOTIFICATION_ACTION = "PROCESS_INITIATE_NOTIFICATION_ACTION";

        private const string PURGE_TASK = "distributed_tasks.process_purge";
        private const double MAX_SERVER_TIME_DIF = 55; //BEO-5280
        private const double HANDLE_PURGE_SCHEDULED_TASKS_INTERVAL_SEC = 21600; // 6 hours
        private const string ROUTING_KEY_PURGE = "PROCESS_PURGE";

        static public Int32 GetGroupID(string sWSUserName, string sPass)
        {
            Credentials oCredentials = new Credentials(sWSUserName, sPass);
            Int32 nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.USERS, oCredentials);
            if (nGroupID == 0)
                log.Debug("WS ignored - eWSModules: eWSModules.USERS " + " UN: " + sWSUserName + " Pass: " + sPass);

            return nGroupID;
        }

        static public Int32 GetDomainGroupID(string sWSUserName, string sPass)
        {
            Credentials oCredentials = new Credentials(sWSUserName, sPass);
            Int32 nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.DOMAINS, oCredentials);
            if (nGroupID == 0)
                log.Debug("WS ignored - eWSModules: eWSModules.DOMAINS " + " UN: " + sWSUserName + " Pass: " + sPass);

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

        static public void GetBaseImpl(ref KalturaBaseUsers user, Int32 nGroupID, int operatorId = -1, string className = "User", bool checkForSsoAdapter = true)
        {
            try
            {
                user = new KalturaUsers(nGroupID);

                // adding for https://kaltura.atlassian.net/browse/GEN-1301, remove sso adapter implementation for GetUserData & GetUsersData
                if (checkForSsoAdapter)
                {
                    var httpSsoAdaptersResponse = SSOAdaptersManager.GetSSOAdapters(nGroupID);
                    if (httpSsoAdaptersResponse != null && httpSsoAdaptersResponse.SSOAdapters != null && httpSsoAdaptersResponse.SSOAdapters.Any())
                    {
                        var httpSSOAdapter = httpSsoAdaptersResponse.SSOAdapters.First();
                        user = new KalturaHttpSSOUser(nGroupID, httpSSOAdapter);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetBaseUsersImplModuleName Error - " + string.Format(" Error while trying to get user implementation for group ID: {0} error: {1}", nGroupID, ex.Message), ex);
            }
        }

        static public BaseEncrypter GetBaseImpl(Int32 nGroupID)
        {
            BaseEncrypter baseEncrypter = null;

            var key = string.Format("users_GetBaseEncrypterImpl_{0}", nGroupID);
            bool bRes = UsersCache.GetItem(key, out int nImplID);

            if (!bRes)
            {
                DataRow dr = DAL.UtilsDal.GetEncrypterData(nGroupID, "USERS_CONNECTION_STRING");

                if (dr != null)
                {
                    nImplID = ODBCWrapper.Utils.GetIntSafeVal(dr["ENCRYPTER_IMPLEMENTATION"]);
                }

                UsersCache.AddItem(key, nImplID);
            }
            
            switch (nImplID)
            {
                case 1:
                    baseEncrypter = new MD5Encrypter();
                    break;
                case 2:
                    baseEncrypter = new SHA1Encrypter();
                    break;
                case 3:
                    baseEncrypter = new SHA256Encrypter();
                    break;
                case 4:
                    baseEncrypter = new SHA384Encrypter();
                    break;
                default:
                    break;
            }

            return baseEncrypter;
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

        static public Country[] GetCountryList()
        {
            Country[] ret = null;

            string key = "users_GetCountryList";
            List<int> lCountryIDs;
            List<Country> lCountry;
            bool bRes = UsersCache.GetItem<List<Country>>(key, out lCountry);
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

        static public Country GetIPCountry2(int groupId, string sIP)
        {
            var id = 0;
            try
            {
                var country = Core.Api.api.GetCountryByIp(groupId, sIP);
                id = country?.Id ?? 0;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetIPCountry2 with groupId: {0}, ip: {1}", groupId, sIP), ex);
            }

            var ret = new Country();
            ret.Initialize(id);
            return ret;
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
                    oDomainCache.RemoveDomain(nGroupID, (int)lDomainID);
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

        static public bool IsGroupIDContainedInConfig(long lGroupID)
        {
            bool res = false;
            string rawStrFromConfig = ApplicationConfiguration.Current.ExcludePsDllImplementation.Value;
            if (rawStrFromConfig.Length > 0)
            {
                string[] strArrOfIDs = rawStrFromConfig.Split(';');
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

        public static ResponseStatus MapToResponseStatus(UserActivationState activationState)
        {
            switch (activationState)
            {
                case UserActivationState.Error: return ResponseStatus.InternalError;
                case UserActivationState.UserDoesNotExist: return ResponseStatus.UserDoesNotExist;
                case UserActivationState.Activated: return ResponseStatus.OK;
                case UserActivationState.NotActivated: return ResponseStatus.UserNotActivated;
                case UserActivationState.NotActivatedByMaster: return ResponseStatus.UserNotMasterApproved;
                case UserActivationState.UserRemovedFromDomain: return ResponseStatus.UserNotIndDomain;
                case UserActivationState.UserWIthNoDomain: return ResponseStatus.UserWithNoDomain;
                case UserActivationState.UserSuspended: return ResponseStatus.UserSuspended;
                default: return ResponseStatus.InternalError;
            };
        }

        public static ApiObjects.Response.Status ConvertResponseStatusToResponseObject(ResponseStatus responseStatus, ApiObjects.Response.Status status = null, bool isLogin = false, int externalCode = 0, string externalMessage = null)
        {
            var result = new ApiObjects.Response.Status();

            if (isLogin && responseStatus == ResponseStatus.UserSuspended)
            {
                result.Code = (int)eResponseStatus.OK;
                result.Message = "OK";
                return result;
            }

            switch (responseStatus)
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
                    result.Code = (int)eResponseStatus.WrongPasswordOrUserName;
                    result.Message = "The username or password is not correct";
                    break;
                case ResponseStatus.WrongPasswordOrUserName:
                    result.Code = (int)eResponseStatus.WrongPasswordOrUserName;
                    result.Message = "The username or password is not correct";
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
                case ResponseStatus.ExternalIdAlreadyExists:
                    result.Message = "External ID already exists";
                    result.Code = (int)eResponseStatus.ExternalIdAlreadyExists;
                    break;
                case ResponseStatus.ExternalError:
                    if (externalCode > 0 && !string.IsNullOrEmpty(externalMessage))
                    {
                        result.Code = (int)eResponseStatus.UserExternalError;
                        result.Message = "User External Error";
                    }
                    else
                    {
                        result.Code = (int)eResponseStatus.Error;
                        result.Message = "Error";
                    }
                    break;
                case ResponseStatus.PasswordPolicyViolation:
                    result.Set(status);
                    break;
                case ResponseStatus.PasswordExpired:
                    result.Set(eResponseStatus.PasswordExpired, "Password Expired, please update password");
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
                case DomainResponseStatus.RegionDoesNotExist:
                    result.Code = (int)eResponseStatus.RegionDoesNotExist;
                    result.Message = "Region does not exist";
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

        public static void AddInitiateNotificationActionToQueue(int groupId, eUserMessageAction userAction, int userId, string udid, string pushToken = "")
        {
            if (Notification.NotificationSettings.IsNotificationSettingsExistsForPartner(groupId))
            {
                InitiateNotificationActionQueue que = new InitiateNotificationActionQueue();
                ApiObjects.QueueObjects.UserNotificationData messageAnnouncementData = new ApiObjects.QueueObjects.UserNotificationData(groupId, (int)userAction, userId, udid, pushToken);

                bool res = que.Enqueue(messageAnnouncementData, ROUTING_KEY_INITIATE_NOTIFICATION_ACTION);

                if (res)
                    log.DebugFormat("Successfully inserted a message to notification action queue. user id: {0}, device id: {1}, push token: {2}, group id: {3}, user action: {4}", userId, udid, pushToken, groupId, userAction);
                else
                    log.ErrorFormat("Error while inserting to notification action queue.  user id: {0}, device id: {1}, push token: {2}, group id: {3}, user action: {4}", userId, udid, pushToken, groupId, userAction);
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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                roleIds = null;
                log.Error(string.Format("GetUserRoleIds failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<List<long>, bool>(roleIds, roleIds != null);
        }

        public static DrmPolicy GetDrmPolicy(int groupId)
        {
            DrmPolicy drmPolicy = new DrmPolicy();
            drmPolicy = DomainDal.GetDrmPolicy(groupId);
            if (drmPolicy == null)
            {
                // set object with default value 
                drmPolicy = new DrmPolicy()
                {
                    Policy = DrmSecurityPolicy.DeviceLevel,
                    FamilyLimitation = new List<int>()
                };
            }
            return drmPolicy;
        }

        public static Dictionary<int, string> GetDomainDrmId(int groupId, int domainId, List<int> deviceIds)
        {
            Dictionary<int, string> domainDrmId = new Dictionary<int, string>();
            try
            {
                domainDrmId = Utils.GetDomainDrmId(groupId, domainId);

                if (domainDrmId == null || domainDrmId.Count == 0 || (deviceIds != null && deviceIds.Count > 0
                    && (deviceIds.Where(d => !domainDrmId.Keys.Contains(d)).Count() > 0)))
                {
                    domainDrmId = BuildDomainDrmId(groupId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetDomainDrmId failed, groupId : {0}, domainId : {1}", groupId, domainId), ex);
                domainDrmId = new Dictionary<int, string>();
            }

            if (deviceIds != null && deviceIds.Count > 0)
            {
                return domainDrmId.Where(x => deviceIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            }

            return domainDrmId;
        }

        public static Dictionary<int, string> GetDomainDrmId(int groupId, int domainId)
        {
            Dictionary<int, string> response = null;
            try
            {
                response = DomainDal.GetDomainDrmId(domainId);
                if (response == null || response.Count == 0)
                {
                    response = BuildDomainDrmId(groupId, domainId);
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get domaindrm id, domainId: {0}, groupId: {1}, ex: {2}", domainId, groupId, ex);
            }

            return response;
        }

        private static Dictionary<int, string> BuildDomainDrmId(int groupId, int domainId)
        {
            Dictionary<int, string> domainDrmId = new Dictionary<int, string>();
            // get data from db and insert it to CB
            // get all devices per domain
            DataTable dt = DomainDal.Get_DomainDevices(groupId, domainId);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    int deviceId = 0;
                    string drm_Id = string.Empty;

                    drm_Id = ODBCWrapper.Utils.GetSafeStr(dr, "drm_id");
                    deviceId = ODBCWrapper.Utils.GetIntSafeVal(dr, "device_id");

                    domainDrmId.Add(deviceId, drm_Id);
                }
                if (domainDrmId != null && domainDrmId.Count > 0)
                {
                    // insert to CB
                    bool result = DomainDal.SetDomainDrmId(domainDrmId, domainId);
                }
            }
            return domainDrmId;
        }

        public static bool IsDrmIdUnique(string drmId, int domainId, string udid, int groupId, ref KeyValuePair<int, string> drmValue)
        {
            KeyValuePair<int, string> response = GetDrmId(drmId, groupId, domainId);
            if (response.Key == domainId)
            {
                drmValue = response;
                return true;
            }
            else if (response.Key != domainId && response.Key > 0)
            {
                return false;  // exsits for another domain
            }
            else
            {
                drmValue = response;
                return true;
            }
        }

        private static KeyValuePair<int, string> GetDrmId(string drmId, int groupId, int domainId)
        {
            KeyValuePair<int, string> response = new KeyValuePair<int, string>(0, string.Empty);

            bool res = false;
            KeyValuePair<string, KeyValuePair<int, string>> drm = DomainDal.GetDrmId(drmId, groupId, ref res);

            if (!res)
            {
                // find device of this drmId
                // check that udid exsits in doimain device list
                DataTable dt = DomainDal.Get_DomainDevices(groupId, domainId);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    string udid = dt.AsEnumerable().Where(x => x.Field<string>("drm_id") == drmId).Select(x => x.Field<string>("UDID")).FirstOrDefault();

                    drm = new KeyValuePair<string, KeyValuePair<int, string>>(drmId, new KeyValuePair<int, string>(domainId, udid));
                    bool result = DomainDal.SetDrmId(drm, drmId, groupId);

                }
            }
            if (drm.Key == drmId)
            {
                response = drm.Value;
            }

            return response;
        }

        internal static bool HandleDeviceDrmIdUpdate(int groupID, string deviceId, string drmId, int domainId, string udid)
        {
            bool result = false;
            try
            {
                result = DomainDal.UpdateDeviceDrmID(groupID, deviceId, drmId, domainId);
                if (result)
                {
                    KeyValuePair<string, KeyValuePair<int, string>> document = new KeyValuePair<string, KeyValuePair<int, string>>(drmId, new KeyValuePair<int, string>(domainId, udid));
                    result = DomainDal.SetDrmId(document, drmId, groupID);
                    if (!result)
                    {
                        log.ErrorFormat("fail SetDrmId document={0}, drmId={1}", document.ToJSON(), drmId);
                    }
                }
                else
                {
                    log.ErrorFormat("fail UpdateDeviceDrmID groupID={0}, deviceId={1}, drmId={2}, domainId={3}", groupID, deviceId, drmId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail HandleDeviceDrmIdUpdate groupID={0}, deviceId={1}, drmId={2}, domainId={3}, udid={4}, ex={5}",
                    groupID, deviceId, drmId, domainId, udid, ex);
                result = false;
            }
            return result;
        }

        internal static bool SetDrmId(string drmId, int domainId, string udid, int groupId)
        {
            KeyValuePair<string, KeyValuePair<int, string>> document = new KeyValuePair<string, KeyValuePair<int, string>>(drmId, new KeyValuePair<int, string>(domainId, udid));
            bool result = DomainDal.SetDrmId(document, drmId, groupId);
            return result;
        }

        internal static bool IsServiceAllowed(int groupId, int domainId, eService service)
        {
            List<int> services;
            string key = string.Format("GroupEnforcedServices_{0}", groupId);
            if (!DomainsCache.GetItem<List<int>>(key, out services))
            {
                log.DebugFormat("Failed getting GroupEnforcedServices from cache, key: {0}", key);
                services = Tvinci.Core.DAL.CatalogDAL.GetGroupServices(groupId);
                if (services == null)
                {
                    log.ErrorFormat("Failed CatalogDAL.GetGroupServices for groupID: {0}", groupId);
                }
                else if (!DomainsCache.AddItem(key, services))
                {
                    log.ErrorFormat("Failed inserting GroupEnforcedServices to cache, key: {0}", key);
                }
            }
            //check if service is part of the group enforced services
            if (services == null || services.Count == 0 || !services.Contains((int)service))
            {
                return true;
            }
            return false;
        }



        internal static bool RemoveDrmId(List<string> drmIds, int groupId)
        {
            bool result = true;
            if (drmIds == null || drmIds.Count == 0)
            {
                return result;
            }

            foreach (string drmId in drmIds)
            {
                result = result & DomainDal.RemoveDrmId(drmId, groupId);
            }
            return result;
        }

        public static bool Purge()
        {
            double purgeScheduledTaskIntervalSec = 0;
            bool shouldEnqueueFollowUp = false;
            try
            {
                // try to get interval for next run take default
                BaseScheduledTaskLastRunDetails purgeScheduledTask = new BaseScheduledTaskLastRunDetails(ScheduledTaskType.purgeScheduledTasks);

                // get run details
                ScheduledTaskLastRunDetails lastRunDetails = purgeScheduledTask.GetLastRunDetails();
                purgeScheduledTask = lastRunDetails != null ? (BaseScheduledTaskLastRunDetails)lastRunDetails : null;

                if (purgeScheduledTask != null &&
                    purgeScheduledTask.Status.Code == (int)eResponseStatus.OK &&
                    purgeScheduledTask.NextRunIntervalInSeconds > 0)
                {
                    purgeScheduledTaskIntervalSec = purgeScheduledTask.NextRunIntervalInSeconds;

                    if (purgeScheduledTask.LastRunDate.AddSeconds(purgeScheduledTaskIntervalSec - MAX_SERVER_TIME_DIF) > DateTime.UtcNow)
                    {
                        return true;
                    }
                    else
                    {
                        shouldEnqueueFollowUp = true;
                    }
                }
                else
                {
                    shouldEnqueueFollowUp = true;
                    purgeScheduledTaskIntervalSec = HANDLE_PURGE_SCHEDULED_TASKS_INTERVAL_SEC;
                }

                int impactedItems = UsersDal.Purge();

                if (impactedItems > 0)
                {
                    log.DebugFormat("Successfully applied purge on: {0} users", impactedItems);
                }
                else
                {
                    log.DebugFormat("No users were modified on purge");
                }

                purgeScheduledTask = new BaseScheduledTaskLastRunDetails(DateTime.UtcNow, impactedItems, purgeScheduledTaskIntervalSec, ScheduledTaskType.purgeScheduledTasks);
                if (!purgeScheduledTask.SetLastRunDetails())
                {
                    log.InfoFormat("Failed to run purge scheduled task last run details, PurgeScheduledTask: {0}", purgeScheduledTask.ToString());
                }
                else
                {
                    log.Debug("Successfully ran purge scheduled task last run date");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in purge process", ex);
                shouldEnqueueFollowUp = true;
            }
            finally
            {
                if (shouldEnqueueFollowUp)
                {
                    if (purgeScheduledTaskIntervalSec == 0)
                    {
                        purgeScheduledTaskIntervalSec = HANDLE_PURGE_SCHEDULED_TASKS_INTERVAL_SEC;
                    }

                    DateTime nextExecutionDate = DateTime.UtcNow.AddSeconds(purgeScheduledTaskIntervalSec);
                    SetupTasksQueue queue = new SetupTasksQueue();
                    CelerySetupTaskData data = new CelerySetupTaskData(0, eSetupTask.PurgeUsers, new Dictionary<string, object>()) { ETA = nextExecutionDate };
                    bool enqueueResult = queue.Enqueue(data, ROUTING_KEY_PURGE);
                }
            }

            return true;
        }
    }
}