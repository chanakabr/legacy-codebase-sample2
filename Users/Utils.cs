using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using DAL;

namespace Users
{
    public class Utils
    {
        public const int USER_COGUID_LENGTH = 15;
        internal static readonly DateTime FICTIVE_DATE = new DateTime(2000, 1, 1); // fictive date. must match with the
        // default result of GetDateSafeVal in ODBCWrapper.Utils
        internal static readonly int CONCURRENCY_MILLISEC_THRESHOLD = 65000;

        static public Int32 GetGroupID(string sWSUserName, string sPass, string sFunctionName, ref BaseUsers t)
        {
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            Int32 nGroupID = TVinciShared.WS_Utils.GetGroupID("users", sFunctionName, sWSUserName, sPass, sIP);
            if (nGroupID != 0)
                Utils.GetBaseUsersImpl(ref t, nGroupID);
            else
                Logger.Logger.Log("WS ignored", "IP: " + sIP + ",Function: " + sFunctionName + " UN: " + sWSUserName + " Pass: " + sPass, "users");
            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sPass, string sFunctionName, ref BaseDomain t)
        {
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            Int32 nGroupID = TVinciShared.WS_Utils.GetGroupID("domains", sFunctionName, sWSUserName, sPass, sIP);
            if (nGroupID != 0)
                Utils.GetBaseDomainsImpl(ref t, nGroupID);
            else                Logger.Logger.Log("WS ignored", "IP: " + sIP + ",Function: " + sFunctionName + " UN: " + sWSUserName + " Pass: " + sPass, "domains");
            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sPass, string sFunctionName, ref BaseDevice t)
        {
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            Int32 nGroupID = TVinciShared.WS_Utils.GetGroupID("domains", sFunctionName, sWSUserName, sPass, sIP);
            if (nGroupID != 0)
                Utils.GetBaseDeviceImpl(ref t, nGroupID);
            else Logger.Logger.Log("WS ignored", "IP: " + sIP + ",Function: " + sFunctionName + " UN: " + sWSUserName + " Pass: " + sPass, "domains");
            return nGroupID;
        }

        static public void GetBaseUsersImpl(ref Users.BaseUsers t, Int32 nGroupID)
        {
            DataRow dr  = DAL.UtilsDal.GetModuleImpementationID(nGroupID, (int)ImplementationsModules.Users);
            int nImplID = ODBCWrapper.Utils.GetIntSafeVal(dr["IMPLEMENTATION_ID"]);

            switch (nImplID)
            {
                case 1:
                    t = new Users.TvinciUsers(nGroupID);
                    break;
                case 3:
                    t = new Users.SSOUsers(nGroupID, 0);
                    break;
                case 4:
                    t = new Users.MediaCorpUsers(nGroupID, -1);
                    break;
                case 5:
                    t = new Users.YesUsers(nGroupID);
                    break;
                case 6:
                    t = new Users.EutelsatUsers(nGroupID);
                    break;
                default:
                    break;
            }
                
        }

        static public void GetBaseEncrypterImpl(ref Users.BaseEncrypter t, Int32 nGroupID)
        {
            int nImplID = 0;
            DataRow dr  = DAL.UtilsDal.GetEncrypterData(nGroupID);

            if (dr != null)
            {
                nImplID = ODBCWrapper.Utils.GetIntSafeVal(dr["ENCRYPTER_IMPLEMENTATION"]);
            }

            switch (nImplID)
            {
                case 1:
                    t = new Users.MD5Encrypter(nGroupID);
                    break;
                case 2:
                    t = new Users.SHA1Encrypter(nGroupID);
                    break;
                case 3:
                    t = new Users.SHA256Encrypter(nGroupID);
                    break;
                case 4:
                    t = new Users.SHA384Encrypter(nGroupID);
                    break;
                default:
                    break;
            }

        }


        static public string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }


        static public void GetBaseDomainsImpl(ref Users.BaseDomain t, Int32 nGroupID)
        {
            DataRow dr  = DAL.UtilsDal.GetModuleImpementationID(nGroupID, (int)ImplementationsModules.Domains);
            int nImplID = ODBCWrapper.Utils.GetIntSafeVal(dr["IMPLEMENTATION_ID"]);

            switch (nImplID)
            {
                case 1:
                    t = new Users.TvinciDomain(nGroupID);
                    break;
                case 2:
                    t = new Users.EutelsatDomain(nGroupID);
                    break;
                default:
                    break;
            }
        }

        static public void GetBaseDeviceImpl(ref Users.BaseDevice t, Int32 nGroupID)
        {
            DataRow dr  = DAL.UtilsDal.GetModuleImpementationID(nGroupID, (int)ImplementationsModules.Domains);
            int nImplID = ODBCWrapper.Utils.GetIntSafeVal(dr["IMPLEMENTATION_ID"]);

            switch (nImplID)
            {
                case 1:
                case 2:
                    t = new Users.TvinciDevice(nGroupID);
                    break;
                default:
                    break;
            }
        }

        static public BaseNewsLetterImpl GetBaseNewsLetterImpl(string apiKey, string listID, int implID)
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

            List<int> lCountryIDs = DAL.UtilsDal.GetAllCountries();

            if (lCountryIDs != null && lCountryIDs.Count > 0)
            {
                ret = new Country[lCountryIDs.Count];

                for (int i = 0; i < lCountryIDs.Count; i++)
                {
                    ret[i] = new Country();
                    ret[i].Initialize(lCountryIDs[i]);
                }
            }

            return ret;
        }

        static public State[] GetStateList(Int32 nCountryID)
        {
            State[] ret = null;

            List<int> lStateIDs = DAL.UtilsDal.GetStatesByCountry(nCountryID);

            if (lStateIDs != null && lStateIDs.Count > 0)
            {
                ret = new State[lStateIDs.Count];

                for (int i = 0; i < lStateIDs.Count; i++)
                {
                    ret[i] = new State();
                    ret[i].Initialize(lStateIDs[i]);
                }
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

        static public BaseMailImpl GetBaseMailImpl(int nGroupID, int nRuleID, int nImpID)
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

        static public bool SendMail(int nGroupID, TvinciAPI.MailRequestObj request)
        {
            using (TvinciAPI.API client = new TvinciAPI.API())
            {

                string sWSURL = Utils.GetWSURL("api_ws");
                if (sWSURL.Length > 0)
                    client.Url = sWSURL;

                string sIP = "1.1.1.1";
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "Mailer", "API", sIP, ref sWSUserName, ref sWSPass);
                bool result = client.SendMailTemplate(sWSUserName, sWSPass, request);
                return result;
            }
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

                Logger.Logger.Log("GetUserOperatorAndHouseholdIDs", sb.ToString(), "Users.Utils");

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

                Utils.GetBaseEncrypterImpl(ref encrypter, nGroupID);
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

        internal static List<HomeNetwork> GetHomeNetworksOfDomain(long lDomainID, int nGroupID)
        {
            List<HomeNetwork> res = null;

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
            }
            else
            {
                res = new List<HomeNetwork>(0);
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
    }
}
