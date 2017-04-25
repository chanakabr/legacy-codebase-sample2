using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using Tvinci.Core.DAL;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Web;
using System.ServiceModel;
using Core.Users;
using ApiObjects.SearchObjects;
using ApiObjects.AssetLifeCycleRules;
using Core.Api.Managers;
using System.Security.Cryptography;
using CachingProvider.LayeredCache;

namespace APILogic
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static int GetIntSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    return int.Parse(dr[sField].ToString());
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static string GetSafeStr(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    return dr[sField].ToString();
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static int GetIntSafeVal(string val)
        {
            try
            {
                if (!string.IsNullOrEmpty(val))
                    return int.Parse(val);
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        public static double GetDoubleSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    return double.Parse(dr[sField].ToString());
                return -1.0;
            }
            catch
            {
                return -1.0;
            }
        }

        public static double GetDoubleSafeVal(string val)
        {
            try
            {
                if (!string.IsNullOrEmpty(val))
                    return double.Parse(val);
                return -1.0;
            }
            catch
            {
                return -1.0;
            }
        }

        //public static string GetSafeStr(object o)
        //{
        //    if (o == DBNull.Value)
        //        return string.Empty;
        //    else if (o == null)
        //        return string.Empty;
        //    else
        //        return o.ToString();
        //}

        internal static List<int> ConvertMediaResultObjectIDsToIntArray(SearchResult[] medias)
        {
            List<int> res = new List<int>();

            if (medias != null && medias.Length > 0)
            {
                IEnumerable<int> ids = from media in medias
                                       select media.assetID;

                res = ids.ToList<int>();
            }

            return res;
        }

        public static string GetCatalogUrl()
        {
            string sCatalogURL = string.Empty;

            try
            {
                sCatalogURL = GetWSUrl("WS_Catalog");
            }
            catch (Exception ex)
            {
                log.Error("Catalog URL - Cannot read catalog URL", ex);
            }

            return sCatalogURL;
        }

        public static string GetWSUrl(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        /// <summary>
        /// Validates that a user exists and belongs to a given domain
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public static ResponseStatus ValidateUser(int groupId, string siteGuid, int domainId)
        {
            ResponseStatus status = ResponseStatus.InternalError;

            try
            {
                UserResponseObject response = Core.Users.Module.GetUserData(groupId, siteGuid, string.Empty);

                // Make sure response is OK
                if (response != null)
                {
                    if (response.m_RespStatus == ResponseStatus.OK)
                    {
                        // If the user belongs to the domain or no domain was sent
                        if (response.m_user != null &&
                            (domainId == 0 || response.m_user.m_domianID == domainId))
                        {
                            status = ResponseStatus.OK;
                        }
                        else
                        {
                            status = ResponseStatus.UserNotIndDomain;
                        }
                    }
                    else
                    {
                        status = response.m_RespStatus;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("ValidateUser - " +
                    string.Format("Error when validating user {0} in group {1}. ex = {2}, ST = {3}", siteGuid, groupId, ex.Message, ex.StackTrace),
                    ex);
            }

            return status;
        }

        /// <summary>
        /// Validates that a domain exists
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public static eResponseStatus ValidateDomain(int groupId, int domainId, out Domain domain)
        {
            domain = null;
            eResponseStatus responseStatus = eResponseStatus.Error;

            try
            {
                // get domain info
                DomainResponse response = Core.Domains.Module.GetDomainInfo(groupId, domainId);

                // validate response
                if (!Enum.TryParse(response.Status.Code.ToString(), out responseStatus))
                    responseStatus = eResponseStatus.Error;

                domain = response.Domain;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("ValidateDomain - Error when validating domain {0} in group {1}. ex = {2}", domainId, groupId, ex);
            }

            return responseStatus;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword)
        {
            Credentials oCredentials = new Credentials(sWSUserName, sWSPassword);
            Int32 nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.API, oCredentials);

            if (nGroupID == 0)
                log.Debug("WS ignored -  eWSModules: eWSModules.API " + " UN: " + sWSUserName + " Pass: " + sWSPassword);

            return nGroupID;
        }

        public static string GetGroupDefaultLanguageCode(int groupId)
        {
            var language = Core.Api.api.GetGroupLanguages(groupId).Where(l => l.IsDefault).FirstOrDefault();
            return language != null ? language.Code : string.Empty;
        }

        public static int[] GetGroupMediaTypesIds(int groupId)
        {
            int[] ids = null;

            try
            {
                Dictionary<int, string> idToName;
                Dictionary<string, int> nameToId;
                Dictionary<int, int> parentMediaTypes;
                List<int> linearMediaTypes;

                CatalogDAL.GetMediaTypes(groupId, out idToName, out nameToId, out parentMediaTypes, out linearMediaTypes);
                if (idToName != null)
                {
                    ids = idToName.Keys.ToArray();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed to get media types ids for group {0} with error", groupId), ex);
            }
            return ids;
        }

        /// <summary>
        /// Compress a given file
        /// </summary>
        /// <param name="filePathToCompress">file path + file name. For example: c:\example\test.xml</param>
        /// <param name="compressedFileLocation">directory location without file name. For example: c:\output\</param>
        /// <returns></returns>
        public static bool CompressFile(string filePathToCompress, string compressedFileLocation)
        {
            try
            {
                FileInfo fi = new FileInfo(filePathToCompress);

                // Get the stream of the source file.
                using (FileStream inFile = fi.OpenRead())
                {
                    // Prevent compressing hidden and 
                    // already compressed files.
                    if ((File.GetAttributes(fi.FullName)
                        & FileAttributes.Hidden)
                        != FileAttributes.Hidden & fi.Extension != ".gz")
                    {
                        // Create the compressed file.
                        using (FileStream outFile =
                                    File.Create((compressedFileLocation.EndsWith("\\") ? compressedFileLocation : compressedFileLocation + "\\") + fi.Name + ".gz"))
                        {
                            using (GZipStream Compress =
                                new GZipStream(outFile,
                                CompressionMode.Compress))
                            {
                                // Copy the source file into 
                                // the compression stream.
                                inFile.CopyTo(Compress);

                                Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                                    fi.Name, fi.Length.ToString(), outFile.Length.ToString());

                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("error in compress file", ex);
                return false;
            }

            return false;
        }

        static public bool SendGetHttpRequest(string url, ref string response, ref int statusCode)
        {
            bool result = false;
            statusCode = -1;

            HttpWebResponse webResponse = null;
            Stream receiveStream = null;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

                webResponse = (HttpWebResponse)webRequest.GetResponse();

                response = string.Empty;

                if (webResponse.ContentLength < 10000000 && webResponse.ContentLength > 0)
                {
                    receiveStream = webResponse.GetResponseStream();
                    StreamReader sr = new StreamReader(receiveStream);
                    response = sr.ReadToEnd();
                    webResponse.Close();
                    sr.Close();
                }
                else
                {
                    webResponse.Close();
                }

                statusCode = (int)webResponse.StatusCode;
                result = true;

                webRequest = null;
                webResponse = null;

            }
            catch (Exception ex)
            {
                log.Error(string.Format("SendGetHttpRequest failed, url = {0}", url), ex);
                if (webResponse != null)
                    webResponse.Close();
                if (receiveStream != null)
                    receiveStream.Close();
                statusCode = 500;
                response = null;
            }

            return result;
        }

        public static ApiObjects.MetaType GetMetaTypeByDbName(string metaDbName)
        {
            if (metaDbName.EndsWith("DOUBLE"))
            {
                return ApiObjects.MetaType.Number;
            }

            if (metaDbName.EndsWith("BOOL"))
            {
                return ApiObjects.MetaType.Bool;
            }

            if (metaDbName.EndsWith("STR"))
            {
                return ApiObjects.MetaType.String;
            }

            if (metaDbName.StartsWith("date"))
            {
                return ApiObjects.MetaType.DateTime;
            }

            return ApiObjects.MetaType.All;
        }

        internal static Tuple<DataTable, bool> Get_GeoBlockPerMedia(Dictionary<string, object> funcParams)
        {
            bool res = false;
            DataTable dt = null;
            try
            {
                if (funcParams != null && funcParams.Count == 2 && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("mediaId"))
                {
                    int? groupId, mediaId;
                    groupId = funcParams["groupId"] as int?;
                    mediaId = funcParams["mediaId"] as int?;
                    if (groupId.HasValue && mediaId.HasValue)
                    {
                        dt = DAL.ApiDAL.Get_GeoBlockRuleForMediaAndCountries(groupId.Value, mediaId.Value);
                        res = dt != null;
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Get_GeoBlockPerMedia failed, parameters : {0}", string.Join(";",funcParams.Keys)), ex);
            }
            return new Tuple<DataTable,bool>(dt, res);
        }

        internal static Tuple<bool?, bool> GetIsMediaExistsToUserType(Dictionary<string, object> funcParams)
        {
            bool res = false;
            bool? isMediaExistsToUserType = null;
            if (funcParams != null && funcParams.Count == 2)
            {
                if (funcParams.ContainsKey("mediaId") && funcParams.ContainsKey("userTypeId"))
                {
                    int? mediaId, userTypeId;                    
                    mediaId = funcParams["mediaId"] as int?;
                    userTypeId = funcParams["userTypeId"] as int?;
                    if (mediaId.HasValue && userTypeId.HasValue)
                    {
                        isMediaExistsToUserType = DAL.ApiDAL.Is_MediaExistsToUserType(mediaId.Value, userTypeId.Value);
                        res = isMediaExistsToUserType.HasValue;
                    }
                }
            }

            return new Tuple<bool?, bool>(isMediaExistsToUserType, res);
        }

        internal static Tuple<List<MediaConcurrencyRule>, bool> Get_MCRulesByGroup(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<MediaConcurrencyRule> result = new List<MediaConcurrencyRule>();
            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("groupId"))
                    {
                        int? groupId;
                        groupId = funcParams["groupId"] as int?;
                        if (groupId.HasValue)
                        {
                            DataSet ds = DAL.ApiDAL.Get_MCRulesByGroup(groupId.Value);
                            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                            {
                                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                                {
                                    MediaConcurrencyRule rule = new MediaConcurrencyRule();
                                    foreach (DataRow dr in ds.Tables[0].Rows)
                                    {
                                        int ruleID = APILogic.Utils.GetIntSafeVal(dr, "rule_id");
                                        string name = APILogic.Utils.GetSafeStr(dr, "name");
                                        int tagTypeID = APILogic.Utils.GetIntSafeVal(dr, "tag_type_id");
                                        string tagType = APILogic.Utils.GetSafeStr(dr, "tag_type_name");
                                        int MCLimitation = APILogic.Utils.GetIntSafeVal(dr, "media_concurrency_limit");
                                        int bmId = ODBCWrapper.Utils.GetIntSafeVal(dr, "BM_ID");
                                        int bmType = ODBCWrapper.Utils.GetIntSafeVal(dr, "type");
                                        rule = new MediaConcurrencyRule(ruleID, tagTypeID, tagType, name, 1, bmId, (eBusinessModule)bmType);
                                        //get all tagValues
                                        if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                                        {
                                            DataRow[] drTags = ds.Tables[1].Select("rule_id=" + ruleID);

                                            rule.TagValues.AddRange(drTags.Select(x => (int)x.Field<Int64>("TAG_ID")).ToList());
                                            rule.AllTagValues.AddRange(drTags.Select(x => x.Field<string>("VALUE")).ToList());
                                        }
                                        result.Add(rule);
                                    }
                                    res = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Get_MCRulesByGroup faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<List<MediaConcurrencyRule>, bool>(result, res);
        }

        internal static Tuple<ApiObjects.Country, bool> GetCountryByIpFromES(Dictionary<string, object> funcParams)
        {
            bool res = false;
            ApiObjects.Country country = null;
            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("ip"))
                {
                    string ip = funcParams["ip"].ToString();
                    if (!string.IsNullOrEmpty(ip))
                    {
                        country = ElasticSearch.Utilities.IpToCountry.GetCountryByIp(ip);
       
                        res = country != null;
                    }                    
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetCountryByIpFromES failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<ApiObjects.Country, bool>(country, res);
        }

        internal static Tuple<ApiObjects.Country, bool> GetCountryByCountryNameFromES(Dictionary<string, object> funcParams)
        {
            bool res = false;
            ApiObjects.Country country = null;
            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("countryName"))
                {
                    string countryName = funcParams["countryName"].ToString();
                    if (!string.IsNullOrEmpty(countryName))
                    {
                        country = ElasticSearch.Utilities.IpToCountry.GetCountryByCountryName(countryName);

                        res = country != null;
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetCountryByCountryNameFromES failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<ApiObjects.Country, bool>(country, res);
        }

        internal static Tuple<List<ParentalRule>, bool> GetGroupParentalRules(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<ParentalRule> result = null;
            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        result = DAL.ApiDAL.Get_Group_ParentalRules(groupId.Value);

                        res = result != null ? true : false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupParentalRules failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<ParentalRule>, bool>(result, res);
        }

        internal static Tuple<Dictionary<long, eRuleLevel>, bool> GetUserParentalRules(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<long, eRuleLevel> result = null;
            try
            {
                if (funcParams != null && funcParams.Count == 2)
                {
                    int? groupId;
                    string userId = string.Empty;

                    if (funcParams.ContainsKey("groupId"))
                    {
                        groupId = funcParams["groupId"] as int?;

                        if (funcParams.ContainsKey("userId"))
                        {
                            userId = funcParams["userId"].ToString();
                        }
                        if (groupId.HasValue && !string.IsNullOrEmpty(userId))
                        {
                            result = DAL.ApiDAL.Get_UserParentalRules(groupId.Value, userId);

                            res = result != null ? true : false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupParentalRules failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<long, eRuleLevel>, bool>(result, res);
        }

        internal static FriendlyAssetLifeCycleRuleResponse InsertOrUpdateFriendlyAssetLifeCycleRule(int groupId, FriendlyAssetLifeCycleRule rule)
        {
            FriendlyAssetLifeCycleRuleResponse result = new FriendlyAssetLifeCycleRuleResponse();

            try
            {
                List<string> tagNamesToAdd = new List<string>(rule.TagNamesToAdd);
                List<string> tagNamesToRemove = new List<string>(rule.TagNamesToRemove);
                List<int> tagIdsToAdd = new List<int>();
                List<int> tagIdsToRemove = new List<int>();
                int filterTagTypeId = 0;
                if (!int.TryParse(rule.FilterTagType.key, out filterTagTypeId) || filterTagTypeId == 0)
                {
                    log.ErrorFormat("failed parsing filterTagTypeId, groupId: {0}, id: {1}, name: {2}", groupId, rule.Id, rule.Name);
                    return result;
                }

                KeyValuePair filterTagType = AssetLifeCycleRuleManager.GetFilterTagTypeById(groupId, filterTagTypeId);
                if (filterTagType == null)
                {
                    log.ErrorFormat("GetFilterTagTypeById returned null result, groupId: {0}, id: {1}, name: {2}, filterTagTypeId: {3}", groupId, rule.Id, rule.Name, filterTagTypeId);
                    return result;
                }

                rule.FilterTagType = new KeyValuePair(filterTagType.key, filterTagType.value);
                if (rule.TagNamesToAdd != null && rule.TagNamesToAdd.Count > 0)
                {
                    rule.TagNamesToAdd = rule.TagNamesToAdd.Distinct().ToList();
                    tagIdsToAdd = AssetLifeCycleRuleManager.GetTagIdsByTagNames(groupId, rule.FilterTagType.value, rule.TagNamesToAdd);
                    if (tagIdsToAdd.Count != rule.TagNamesToAdd.Count)
                    {
                        log.ErrorFormat("GetTagIdsByTagNames returned incorrect number of results, groupId: {0}, id: {1}, name: {2}, tagNamesToAdd: {3}", groupId, rule.Id, rule.Name, string.Join(",", rule.TagNamesToAdd));
                        return result;
                    }
                }

                if (rule.TagNamesToRemove != null && rule.TagNamesToRemove.Count > 0)
                {
                    rule.TagNamesToRemove = rule.TagNamesToRemove.Distinct().ToList();
                    tagIdsToRemove = AssetLifeCycleRuleManager.GetTagIdsByTagNames(groupId, rule.FilterTagType.value, rule.TagNamesToRemove);
                    if (tagIdsToRemove.Count != rule.TagNamesToRemove.Count)
                    {
                        log.ErrorFormat("GetTagIdsByTagNames returned incorrect number of results, groupId: {0}, id: {1}, name: {2}, tagNamesToRemove: {3}", groupId, rule.Id, rule.Name, string.Join(",", rule.TagNamesToRemove));
                        return result;
                    }
                }

                rule = new FriendlyAssetLifeCycleRule(rule.Id, groupId, rule.Name, rule.Description, rule.TransitionIntervalUnits, rule.FilterTagType, rule.FilterTagValues, rule.FilterTagOperand,
                                                                                 rule.MetaDateName, rule.MetaDateValue, tagIdsToAdd, tagIdsToRemove);
                if (!AssetLifeCycleRuleManager.BuildActionRuleKsqlFromData(rule))
                {
                    log.ErrorFormat("failed BuildActionRuleKsqlFromData, groupId: {0}, id: {1}, name: {2}", groupId, rule.Id, rule.Name);
                    return result;
                }

                long id = DAL.ApiDAL.InsertOrUpdateAssetLifeCycleRule(rule);
                if (id > 0)
                {
                    // in case of insert
                    if (rule.Id == 0)
                    {
                        rule.Id = id;
                    }

                    rule.TagNamesToAdd = new List<string>(tagNamesToAdd);
                    rule.TagNamesToRemove = new List<string>(tagNamesToRemove);
                    result = new FriendlyAssetLifeCycleRuleResponse(rule);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in InsertOrUpdateFriendlyAssetLifeCycleRule, groupId: {0}, id: {1}, name: {2}", groupId, rule.Id, rule.Name), ex);
            }

            return result;
        }

        internal static bool InsertOrUpdateAssetLifeCycleRulePpvsAndFileTypes(int groupId, FriendlyAssetLifeCycleRule rule)
        {
            bool result = false;

            try
            {
                result = DAL.ApiDAL.InsertOrUpdateAssetLifeCycleRulePpvsAndFileTypes(rule);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in InsertOrUpdateAssetLifeCycleRulePpvsAndFileTypes, groupId: {0}, id: {1}", groupId, rule.Id), ex);
            }

            return result;
        }

        internal static Dictionary<int, CountryLocale> GetCountriesLocaleMap(int groupId, List<int> countryIds)
        {
            Dictionary<int, CountryLocale> result = null;            
            try
            {
                if (countryIds != null && countryIds.Count > 0)
                {
                    Dictionary<long, CountryLocale> countryLocaleMapping = new Dictionary<long,CountryLocale>();
                    DataSet ds = DAL.ApiDAL.GetCountriesLocale(groupId, countryIds);
                    if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                    {
                        result = new Dictionary<int, CountryLocale>();                        
                        if (ds.Tables[0] != null && ds.Tables[0].Rows != null)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                                int countryId = ODBCWrapper.Utils.GetIntSafeVal(dr, "COUNTRY_ID");
                                if (id > 0 && countryId > 0)
                                {
                                    string currencyCode = ODBCWrapper.Utils.GetSafeStr(dr, "CURRENCY_CODE");
                                    string currencySign = ODBCWrapper.Utils.GetSafeStr(dr, "CURRENCY_SIGN");
                                    string mainLanguageCode = ODBCWrapper.Utils.GetSafeStr(dr, "LANGUAGE_CODE");
                                    double vatPercent = ODBCWrapper.Utils.GetDoubleSafeVal(dr, "vat_percent");
                                    countryLocaleMapping.Add(id, new CountryLocale(countryId, currencyCode, currencySign, mainLanguageCode, vatPercent));
                                }
                            }
                        }

                        if (ds.Tables.Count == 2 && ds.Tables[1] != null && ds.Tables[1].Rows != null)
                        {
                            foreach (DataRow dr in ds.Tables[1].Rows)
                            {
                                long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                                string languageCode = ODBCWrapper.Utils.GetSafeStr(dr, "LANGUAGE_CODE");
                                if (id > 0 && countryLocaleMapping.ContainsKey(id) && !countryLocaleMapping[id].LanguageCodes.Contains(languageCode))
                                {
                                    countryLocaleMapping[id].LanguageCodes.Add(languageCode);
                                }
                            }
                        }

                        result = countryLocaleMapping.ToDictionary(x => x.Value.Id, x => x.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetCountriesLocaleMap, groupId: {0}, countryIds: {1}", groupId, countryIds != null ? string.Join(",", countryIds) : string.Empty), ex);
            }

            return result;
        }

        internal static Tuple<List<int>, bool> GetMediaFilesByMediaId(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<int> result = null;
            try
            {
                if (funcParams != null && funcParams.Count == 2 && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("mediaId"))
                {
                    int? groupId, mediaId;
                    groupId = funcParams["groupId"] as int?;
                    mediaId = funcParams["mediaId"] as int?;
                    if (groupId.HasValue && mediaId.HasValue)
                    {
                        DataTable dt = DAL.ApiDAL.GetMediaFilesByMediaId(groupId.Value, mediaId.Value);
                        if (dt != null && dt.Rows != null)
                        {
                            result = new List<int>();
                            HashSet<int> ids = new HashSet<int>();
                            foreach (DataRow dr in dt.Rows)
                            {
                                int mediaFileId = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID", 0);
                                if (mediaFileId > 0 && !ids.Contains(mediaFileId))
                                {
                                    ids.Add(mediaFileId);
                                }
                            }

                            result = ids.ToList();
                        }

                        res = result != null;
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaFilesByMediaId failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<int>, bool>(result, res);
        }

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {

                sb.Append(hash[i].ToString("X2"));

            }

            return sb.ToString();
        }

        public static bool IsProxyBlocked(int groupId, string ip)
        {
            bool isProxyBlocked = false;

            try
            {
                string key = LayeredCacheKeys.GetProxyIpKey(ip);
                if (!LayeredCache.Instance.Get<bool>(key, ref isProxyBlocked, IsProxyBlockedForIp, new Dictionary<string, object>() { { "ip", ip } }, groupId,
                                                    LayeredCacheConfigNames.IS_PROXY_BLOCKED_FOR_IP_LAYERED_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheConfigNames.GET_PROXY_IP_INVALIDATION_KEY }))
                {
                    log.ErrorFormat("Failed checking IsProxyBlocked from LayeredCache, ip: {0}, key: {1}", ip, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed IsProxyBlocked for ip: {0}", ip), ex);
            }

            return isProxyBlocked;
        }

        internal static Tuple<bool, bool> IsProxyBlockedForIp(Dictionary<string, object> funcParams)
        {
            bool res = false, isProxyBlocked = false;
            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("ip"))
                {
                    string ip = funcParams["ip"].ToString();
                    if (!string.IsNullOrEmpty(ip))
                    {
                        long convertedIp = 0;
                        if (ConvertIpToInt(ip, ref convertedIp) && convertedIp > 0)
                        {
                            isProxyBlocked = DAL.ApiDAL.IsProxyBlockedForIp(convertedIp);
                            res = true;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("IsProxyBlockedForIp failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<bool, bool>(isProxyBlocked, res);
        }

        public static bool ConvertIpToInt(string ip, ref long convertedIp)
        {
            if (string.IsNullOrEmpty(ip))
            {
                return false;
            }            
            try
            {
                string[] splitted = ip.Split('.');
                convertedIp = Int64.Parse(splitted[3]) + Int64.Parse(splitted[2]) * 256 + Int64.Parse(splitted[1]) * 256 * 256 + Int64.Parse(splitted[0]) * 256 * 256 * 256;
                return true;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed ConvertIpToInt for ip: {0}", ip), ex);
                return false;
            }            
        }

    }
}
