using ApiObjects;
using ApiObjects.Epg;
using Newtonsoft.Json;
using NPVR.AlcatelLucentResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using Tvinci.Core.DAL;
using TVinciShared;
using CachingProvider.LayeredCache;


namespace NPVR
{
    /*
     * Don't change the visibility of this class to public. Any communication with a third party NPVR Provider should be done via
     * the interface INPVRProvider.
     */
    internal class AlcatelLucentNPVR : INPVRProvider
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly string ALU_LOG_FILE = "AlcatelLucent";
        private static readonly string LOG_HEADER_EXCEPTION = "Exception";
        private static readonly string LOG_HEADER_ERROR = "Error";

        private static readonly DateTime UNIX_ZERO_TIME = new DateTime(1970, 1, 1);
        private static readonly Regex JSON_UNBEAUTIFIER = new Regex("[\r\n\t ]");
        private static readonly string EMPTY_JSON = "{}";
        private static readonly int HTTP_STATUS_OK = 200;
        private static readonly string DATE_FORMAT = "yyyyMMddHHmmss";

        private static readonly string ALU_GENERIC_BODY = "scheduler/web/";
        private static readonly string ALU_ENDPOINT_RECORD = "Record/";
        private static readonly string ALU_ENDPOINT_SEASON = "Season/"; // Series
        private static readonly string ALU_ENDPOINT_USER = "User/";

        private static readonly string ALU_CREATE_ACCOUNT_COMMAND = "addById";
        private static readonly string ALU_DELETE_ACCOUNT_COMMAND = "delete";
        private static readonly string ALU_GET_QUOTA_COMMAND = "getProfile";
        private static readonly string ALU_ADD_BY_PROGRAM_COMMAND = "addByProgram";
        private static readonly string ALU_CANCEL_COMMAND = "cancel";
        private static readonly string ALU_DELETE_COMMAND = "delete";
        private static readonly string ALU_UPDATE_FIELD_COMMAND = "updateField";
        private static readonly string ALU_READ_COMMAND = "read";
        private static readonly string ALU_GET_LOCATOR_COMMAND = "getLocator";

        private static readonly string ALU_FORM_URL_PARAM = "form";
        private static readonly string ALU_QUOTA_URL_PARAM = "quota";
        private static readonly string ALU_SCHEMA_URL_PARAM = "schema";
        private static readonly string ALU_USER_ID_URL_PARAM = "userId";
        private static readonly string ALU_ACCOUNT_ID_URL_PARAM = "accountId";
        private static readonly string ALU_PROGRAM_ID_URL_PARAM = "programId";
        private static readonly string ALU_CHANNEL_ID_URL_PARAM = "channelId";
        private static readonly string ALU_START_TIME_URL_PARAM = "startTime";
        private static readonly string ALU_ASSET_ID_URL_PARAM = "assetId";
        private static readonly string ALU_NAME_URL_PARAM = "name";
        private static readonly string ALU_VALUE_URL_PARAM = "value";
        private static readonly string ALU_COUNT_URL_PARAM = "count";
        private static readonly string ALU_ENTRIES_START_INDEX_URL_PARAM = "entriesStartIndex";
        private static readonly string ALU_ENTRIES_PAGE_SIZE_URL_PARAM = "entriesPageSize";
        private static readonly string ALU_ID_URL_PARAM = "id";
        private static readonly string ALU_STREAM_TYPE_URL_PARAM = "streamType";
        private static readonly string ALU_HAS_FORMAT_URL_PARAM = "HASFormat";
        private static readonly string ALU_SORT_FIELD_URL_PARAM = "sortField";
        private static readonly string ALU_SORT_DIRECTION_URL_PARAM = "sortDirection";
        private static readonly string ALU_X_KDATA = "X-KDATA";

        private static readonly string ALU_SEASON_ID = "seasonId";
        private static readonly string ALU_SEASON_NAME = "seasonName";
        private static readonly string ALU_DURATION = "duration";
        private static readonly string ALU_GENRE = "genre";
        private static readonly string ALU_YEAR = "year";
        private static readonly string ALU_EPISODE = "episode";
        private static readonly string ALU_SEASON_NUMBER = "seasonNumber";
        private static readonly string ALU_RATING = "rating";

        private const string USE_OLD_IMAGE_SERVER_KEY = "USE_OLD_IMAGE_SERVER";

        private int groupID;
        public bool SynchronizeNpvrWithDomain { get; set; }

        public AlcatelLucentNPVR(int groupID, bool synchronizeNpvrWithDomain)
        {
            this.groupID = groupID;
            SynchronizeNpvrWithDomain = synchronizeNpvrWithDomain;
        }

        private bool IsCreateOrUpdateInputValid(NPVRParamsObj args)
        {
            return args != null && args.Quota > 0 && !string.IsNullOrEmpty(args.EntityID);
        }

        private string GetLogFilename()
        {
            return String.Concat(ALU_LOG_FILE, "_", groupID);
        }

        private string BuildRestCommand(string method, string endPoint, List<KeyValuePair<string, string>> urlParams)
        {
            string baseUrl = TVinciShared.WS_Utils.GetTcmGenericValue<string>(String.Concat("ALU_BASE_URL_", groupID));
            bool isAddSlash = !baseUrl.EndsWith("/");
            return String.Concat(baseUrl, isAddSlash ? "/" : string.Empty, ALU_GENERIC_BODY, endPoint, method, "?",
                TVinciShared.WS_Utils.BuildDelimiterSeperatedString(urlParams, "&", false, false));
        }

        private void GetCreateAccountResponse(string responseJson, NPVRParamsObj args, NPVRUserActionResponse response)
        {
            try
            {
                // first, try to parse it as a json returned upon success
                CreateAccountResponseJSON aluResp = JsonConvert.DeserializeObject<CreateAccountResponseJSON>(responseJson);
                if (aluResp != null && !string.IsNullOrEmpty(aluResp.UserID))
                {
                    response.isOK = true;
                    response.entityID = aluResp.UserID;
                    response.quota = args.Quota;
                }
                else
                {
                    GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                    response.isOK = false;
                    response.msg = error.Description;
                    log.Error("Error - " + GetLogMsg(string.Format("Failed to create account. {0}", error != null ? error.ToString() : "generic failure response is null"), args, null));
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + GetLogMsg(string.Format("Exception at GetCreateAccountResponse. Inner catch block. Resp JSON: {0}", responseJson), args, ex), ex);
                throw;
            }
        }

        public NPVRUserActionResponse CreateAccount(NPVRParamsObj args)
        {
            NPVRUserActionResponse res = new NPVRUserActionResponse();
            try
            {
                if (IsCreateOrUpdateInputValid(args))
                {
                    log.Debug("CreateAccount - " + string.Format("CreateAccount request has been issued. G ID: {0} , Params Obj: {1}", groupID, args.ToString()));
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(3);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_QUOTA_URL_PARAM, (args.Quota * 60).ToString()));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));
                    string sAccountID = TVinciShared.WS_Utils.GetTcmGenericValue<string>(string.Format("ALU_ACCOUNT_ID_{0}", groupID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_ACCOUNT_ID_URL_PARAM, sAccountID));

                    string url = BuildRestCommand(ALU_CREATE_ACCOUNT_COMMAND, ALU_ENDPOINT_USER, urlParams);
                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;
                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetCreateAccountResponse(responseJson, args, res);
                            log.Debug(string.Format("CreateAccount. Group ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode));
                        }
                        else
                        {
                            throw new Exception(string.Format("CreateAccount. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        // log here the error. 
                        log.Error(LOG_HEADER_ERROR + string.Format("CreateAccount. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.isOK = false;
                        res.msg = "An error occurred. Refer to server log files.";
                        res.quota = 0;
                        res.entityID = string.Empty;
                    }
                }
                else
                {
                    throw new ArgumentException("Either args obj is null or domain id is empty or quota is non-positive.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at CreateAccount.", args, ex), ex);
                throw;
            }

            return res;
        }

        private bool IsDeleteInputValid(NPVRParamsObj args)
        {
            return args != null && !string.IsNullOrEmpty(args.EntityID);
        }

        public NPVRUserActionResponse DeleteAccount(NPVRParamsObj args)
        {
            NPVRUserActionResponse res = new NPVRUserActionResponse();
            try
            {
                if (IsDeleteInputValid(args))
                {
                    log.Debug("DeleteAccount - " + string.Format("DeleteAccount request has been issued. G ID: {0} , Params Obj: {1}", groupID, args.ToString()));
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(3);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_FORM_URL_PARAM, "json"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));

                    string url = BuildRestCommand(ALU_DELETE_ACCOUNT_COMMAND, ALU_ENDPOINT_USER, urlParams);
                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetAccountResponse(responseJson, args, res, "Delete");
                        }
                        else
                        {
                            throw new Exception(string.Format("DeleteAccount. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        // log here the error
                        log.Error(LOG_HEADER_ERROR + string.Format("DeleteAccount. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.isOK = false;
                        res.msg = "An error occurred. Refer to server log files.";
                        res.quota = 0;
                        res.entityID = args.EntityID;
                    }
                }
                else
                {
                    throw new ArgumentException("Either args obj is null or domain id is empty.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at DeleteAccount.", args, ex), ex);
                throw;
            }

            return res;
        }

        private string GetLogMsg(string msg, NPVRParamsObj obj, Exception ex)
        {
            StringBuilder sb = new StringBuilder(String.Concat(msg, "."));
            sb.Append(String.Concat(" Params Obj: ", obj != null ? obj.ToString() : "null"));
            sb.Append(String.Concat(" Group ID: ", groupID));
            if (ex != null)
            {
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
            }

            return sb.ToString();
        }

        private bool IsGetQuotaInputValid(NPVRParamsObj args)
        {
            return args != null && !string.IsNullOrEmpty(args.EntityID);
        }

        public NPVRQuotaResponse GetQuotaData(NPVRParamsObj args)
        {
            NPVRQuotaResponse res = new NPVRQuotaResponse();
            try
            {
                if (IsGetQuotaInputValid(args))
                {
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(2);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));

                    string url = BuildRestCommand(ALU_GET_QUOTA_COMMAND, ALU_ENDPOINT_USER, urlParams);
                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;
                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetGetQuotaDataResponse(responseJson, args, res);
                        }
                        else
                        {
                            throw new Exception(string.Format("GetQuotaData. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        // log here the error
                        log.Error(LOG_HEADER_ERROR + string.Format("GetQuotaData. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.entityID = args.EntityID;
                        res.isOK = false;
                        res.totalQuota = -1;
                        res.usedQuota = -1;
                        res.msg = "An error occurred. Refer to server log files.";
                    }
                }
                else
                {
                    throw new ArgumentException("Either args obj is null or entity id is empty.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at GetQuotaData.", args, ex), ex);
                throw;
            }
            return res;
        }

        private void GetGetQuotaDataResponse(string responseJson, NPVRParamsObj args, NPVRQuotaResponse response)
        {
            try
            {
                // first try to parse it as a json returned upon success
                QuotaResponseJSON success = JsonConvert.DeserializeObject<QuotaResponseJSON>(responseJson);
                if (success != null)
                {
                    response.isOK = true;
                    response.entityID = args.EntityID;
                    response.totalQuota = success.TotalQuota;
                    response.usedQuota = success.OccupiedQuota;
                }
                else
                {
                    GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                    response.isOK = false;
                    response.entityID = args.EntityID;
                    response.totalQuota = 0;
                    response.usedQuota = 0;
                }
            }
            catch (Exception ex)
            {
                GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                response.isOK = false;
                response.entityID = args.EntityID;
                response.totalQuota = 0;
                response.usedQuota = 0;
                log.Error("Error - " + GetLogMsg(string.Format("Exception in GetGetQuotaDataResponse. Json is not in a correct form. Inner catch block. JSON: {0}", responseJson), args, ex), ex);
                throw;
            }
        }

        private bool IsRecordAssetInputValid(NPVRParamsObj args)
        {
            return args != null && !string.IsNullOrEmpty(args.AssetID) && !string.IsNullOrEmpty(args.EntityID) && !string.IsNullOrEmpty(args.EpgChannelID);
        }


        public NPVRRecordResponse RecordAsset(NPVRParamsObj args)
        {
            NPVRRecordResponse res = new NPVRRecordResponse();
            try
            {
                if (IsRecordAssetInputValid(args))
                {
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(5);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "2.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_PROGRAM_ID_URL_PARAM, args.AssetID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_CHANNEL_ID_URL_PARAM, ConvertEpgChannelIdToExternalID(args.EpgChannelID)));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_START_TIME_URL_PARAM, TVinciShared.DateUtils.DateTimeToUnixTimestampMilliseconds(args.StartDate).ToString()));

                    string url = BuildRestCommand(ALU_ADD_BY_PROGRAM_COMMAND, ALU_ENDPOINT_RECORD, urlParams);

                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetRecordAssetResponse(responseJson, args, res);
                        }
                        else
                        {
                            throw new Exception(string.Format("RecordAsset. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }

                    }
                    else
                    {
                        log.Error(LOG_HEADER_ERROR + string.Format("RecordAsset. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.entityID = args.EntityID;
                        res.recordingID = string.Empty;
                        res.status = RecordStatus.Error;
                        res.msg = "An error occurred. Refer to server log files.";
                    }

                }
                else
                {
                    throw new ArgumentException("Either args obj is null or entity id is empty or asset id is empty or epg channel id is empty.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at RecordAsset", args, ex), ex);
                throw;
            }

            return res;
        }

        private void GetRecordAssetResponse(string responseJson, NPVRParamsObj args, NPVRRecordResponse response)
        {
            RecordAssetResponseJSON success = JsonConvert.DeserializeObject<RecordAssetResponseJSON>(responseJson);
            if (success != null && !string.IsNullOrEmpty(success.RecordingID))
            {
                response.entityID = args.EntityID;
                response.status = RecordStatus.OK;
                response.recordingID = success.RecordingID;
                response.msg = string.Empty;
            }
            else
            {
                GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                if (error != null)
                {
                    response.entityID = args.EntityID;
                    response.recordingID = string.Empty;
                    switch (error.ResultCode)
                    {
                        case 210:
                            response.status = RecordStatus.ResourceAlreadyExists;
                            response.msg = "Trying to create a resource that does already exist.";
                            break;
                        case 420:
                            response.status = RecordStatus.QuotaExceeded;
                            response.msg = "Recording can not be done because the user has exceeded the assigned quota.";
                            break;
                        default:
                            GetGenericFailureResponse(response, error);
                            break;
                    }
                }
            }
        }

        private bool IsCancelDeleteAssetInputValid(NPVRParamsObj args)
        {
            return args != null && !string.IsNullOrEmpty(args.EntityID) && !string.IsNullOrEmpty(args.AssetID);
        }


        public NPVRCancelDeleteResponse CancelAsset(NPVRParamsObj args)
        {
            NPVRCancelDeleteResponse res = new NPVRCancelDeleteResponse();
            try
            {
                if (IsCancelDeleteAssetInputValid(args))
                {
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(3);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_ASSET_ID_URL_PARAM, args.AssetID));

                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));

                    string url = BuildRestCommand(ALU_CANCEL_COMMAND, ALU_ENDPOINT_RECORD, urlParams);

                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetCancelAssetResponse(responseJson, args, res);
                        }
                        else
                        {
                            throw new Exception(string.Format("CancelAsset. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        log.Error(LOG_HEADER_ERROR + string.Format("CancelAsset. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.entityID = args.EntityID;
                        res.recordingID = string.Empty;
                        res.status = CancelDeleteStatus.Error;
                        res.msg = "An error occurred. Refer to server log files.";
                    }

                }
                else
                {
                    throw new ArgumentException("Either args obj is null or entity id is empty or asset id is empty.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at CancelAsset.", args, ex), ex);
                throw;
            }

            return res;
        }

        private void GetCancelAssetResponse(string responseJson, NPVRParamsObj args, NPVRCancelDeleteResponse initializedResp)
        {

            string unbeautified = JSON_UNBEAUTIFIER.Replace(responseJson, string.Empty);
            if (unbeautified.Equals(EMPTY_JSON))
            {
                initializedResp.entityID = args.EntityID;
                initializedResp.msg = string.Empty;
                initializedResp.recordingID = args.AssetID;
                initializedResp.status = CancelDeleteStatus.OK;
            }
            else
            {
                try
                {
                    GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                    initializedResp.recordingID = args.AssetID;
                    initializedResp.entityID = args.EntityID;
                    switch (error.ResultCode)
                    {
                        case 404:
                            initializedResp.status = CancelDeleteStatus.AssetDoesNotExist;
                            initializedResp.msg = "Asset does not exist.";
                            break;
                        case 409:
                            initializedResp.status = CancelDeleteStatus.AssetAlreadyRecorded;
                            initializedResp.msg = "Asset already recorded.";
                            break;
                        case 401:
                            initializedResp.status = CancelDeleteStatus.UnauthorizedOperation;
                            initializedResp.msg = "This operation is forbidden due to lack of privileges.";
                            break;
                        case 408:
                            initializedResp.status = CancelDeleteStatus.CommunicationsError;
                            initializedResp.msg = "communication problems.";
                            break;
                        case 500:
                            initializedResp.status = CancelDeleteStatus.InternalServerError;
                            initializedResp.msg = "Internal server error.";
                            break;
                        case 501:
                            initializedResp.status = CancelDeleteStatus.NotImplemented;
                            initializedResp.msg = "Parameter value not supported by the method.";
                            break;
                        default:
                            initializedResp.status = CancelDeleteStatus.Error;
                            initializedResp.msg = error.Description;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + GetLogMsg(String.Concat("Failed to deserialize JSON at GetCancelAssetResponse. Response JSON: ", responseJson), null, ex), ex);
                    throw;
                }
            }
        }

        public NPVRCancelDeleteResponse DeleteAsset(NPVRParamsObj args)
        {
            NPVRCancelDeleteResponse res = new NPVRCancelDeleteResponse();
            try
            {
                if (IsCancelDeleteAssetInputValid(args))
                {
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(3);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_ASSET_ID_URL_PARAM, args.AssetID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));

                    string url = BuildRestCommand(ALU_DELETE_COMMAND, ALU_ENDPOINT_RECORD, urlParams);

                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetDeleteAssetResponse(responseJson, args, res);
                        }
                        else
                        {
                            throw new Exception(string.Format("DeleteAsset. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        log.Error(LOG_HEADER_ERROR + string.Format("DeleteAsset. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.entityID = args.EntityID;
                        res.recordingID = args.AssetID;
                        res.status = CancelDeleteStatus.Error;
                        res.msg = "An error occurred. Refer to server log files.";
                    }
                }
                else
                {
                    throw new ArgumentException("Either args obj is null or entity id is empty or asset id is empty.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at DeleteAsset.", args, ex), ex);
                throw;
            }

            return res;
        }

        private void GetDeleteAssetResponse(string responseJson, NPVRParamsObj args, NPVRCancelDeleteResponse initializedResp)
        {
            string unbeautified = JSON_UNBEAUTIFIER.Replace(responseJson, string.Empty);
            if (unbeautified.Equals(EMPTY_JSON))
            {
                initializedResp.entityID = args.EntityID;
                initializedResp.msg = string.Empty;
                initializedResp.recordingID = args.AssetID;
                initializedResp.status = CancelDeleteStatus.OK;
            }
            else
            {
                try
                {
                    GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                    initializedResp.entityID = args.EntityID;
                    initializedResp.recordingID = args.AssetID;
                    switch (error.ResultCode)
                    {
                        case 404:
                            initializedResp.msg = "Asset does not exist.";
                            initializedResp.status = CancelDeleteStatus.AssetDoesNotExist;
                            break;
                        default:
                            initializedResp.msg = error.Description;
                            initializedResp.status = CancelDeleteStatus.Error;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + GetLogMsg(String.Concat("Failed to deserialize JSON at GetDeleteAssetResponse. Response JSON: ", responseJson), null, ex), ex);
                    throw;
                }
            }
        }

        private bool IsSetAssetProtectionStatusInputValid(NPVRParamsObj args)
        {
            return args != null && !string.IsNullOrEmpty(args.EntityID) && !string.IsNullOrEmpty(args.AssetID);
        }

        public NPVRProtectResponse SetAssetProtectionStatus(NPVRParamsObj args)
        {
            NPVRProtectResponse res = new NPVRProtectResponse();
            try
            {
                if (IsSetAssetProtectionStatusInputValid(args))
                {
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(5);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_NAME_URL_PARAM, "protected"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_VALUE_URL_PARAM, args.IsProtect.ToString().ToLower()));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_ASSET_ID_URL_PARAM, args.AssetID));

                    string url = BuildRestCommand(ALU_UPDATE_FIELD_COMMAND, ALU_ENDPOINT_RECORD, urlParams);

                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetSetAssetProtectionStatusResponse(responseJson, args, res);
                        }
                        else
                        {
                            throw new Exception(string.Format("SetAssetProtectionStatus. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        log.Error(LOG_HEADER_ERROR + string.Format("SetAssetProtectionStatus. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.recordingID = args.AssetID;
                        res.status = ProtectStatus.Error;
                        res.entityID = args.EntityID;
                        res.msg = "An error occurred. Refer to server log files.";
                    }


                }
                else
                {
                    throw new ArgumentException("Either args obj is null or entity id is empty or asset id is empty.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at SetAssetProtectionStatus.", args, ex), ex);
                throw;
            }

            return res;
        }

        private void GetSetAssetProtectionStatusResponse(string responseJson, NPVRParamsObj args, NPVRProtectResponse response)
        {
            string unbeautified = JSON_UNBEAUTIFIER.Replace(responseJson, string.Empty);
            if (unbeautified.Equals(EMPTY_JSON))
            {
                response.entityID = args.EntityID;
                response.msg = string.Empty;
                response.recordingID = args.AssetID;
                response.status = args.IsProtect ? ProtectStatus.Protected : ProtectStatus.NotProtected;
            }
            else
            {
                try
                {
                    GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                    response.entityID = args.EntityID;
                    response.msg = error.Description;
                    response.recordingID = args.AssetID;
                    switch (error.ResultCode)
                    {
                        case 404:
                            response.status = ProtectStatus.RecordingDoesNotExist;
                            break;
                        default:
                            response.status = ProtectStatus.Error;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + GetLogMsg(String.Concat("Failed to deserialize JSON at GetSetAssetProtectionStatusResponse. Response JSON: ", responseJson), null, ex), ex);
                    throw;
                }
            }
        }

        private bool IsRetrieveAssetsInputValid(NPVRRetrieveParamsObj args, ref ulong uniqueSearchBy)
        {
            if (args != null && !string.IsNullOrEmpty(args.EntityID) && (args.PageSize > 0 || args.PageIndex == 0))
            {
                bool seenUnique = false;
                IEnumerable<SearchByField> distinct = args.GetUniqueSearchBy();
                foreach (SearchByField sbf in distinct)
                {
                    switch (sbf)
                    {
                        case SearchByField.byAssetId:
                            if (seenUnique || (args.AssetIDs.Count == 0 && string.IsNullOrEmpty(args.AssetID)))
                                return false;
                            seenUnique = true;
                            uniqueSearchBy = (ulong)SearchByField.byAssetId;
                            break;
                        case SearchByField.byStartTime:
                            if ((args.StartDate.CompareTo(UNIX_ZERO_TIME) < 1) || seenUnique)
                                return false;
                            seenUnique = true;
                            uniqueSearchBy = (ulong)SearchByField.byStartTime;
                            break;
                        case SearchByField.byProgramId:
                            if (args.EpgProgramIDs.Count == 0)
                                return false;
                            break;
                        case SearchByField.byChannelId:
                            if (string.IsNullOrEmpty(args.EpgChannelID))
                                return false;
                            break;
                        case SearchByField.byStatus:
                            seenUnique = true;
                            uniqueSearchBy = (ulong)SearchByField.byStatus;
                            break;
                        default:
                            break;

                    }
                } //foreach

                return true;

            }

            return false;
        }

        public NPVRRetrieveAssetsResponse RetrieveAssets(NPVRRetrieveParamsObj args)
        {
            NPVRRetrieveAssetsResponse res = new NPVRRetrieveAssetsResponse();
            try
            {
                ulong uniqueSearchBy = 0;
                if (IsRetrieveAssetsInputValid(args, ref uniqueSearchBy))
                {

                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>();
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_COUNT_URL_PARAM, "true"));
                    if (args.PageSize > 0)
                    {
                        // Because ALU handles "page index" as "first entry", we will not send the regular page index we are used to,
                        // we will send index * size to get ALU bring us the correct assets
                        urlParams.Add(new KeyValuePair<string, string>(ALU_ENTRIES_START_INDEX_URL_PARAM,
                                                                       (args.PageIndex * args.PageSize).ToString()));
                        urlParams.Add(new KeyValuePair<string, string>(ALU_ENTRIES_PAGE_SIZE_URL_PARAM, args.PageSize.ToString()));
                    }
                    else
                    {
                        // bring all. just for readability. no code is supposed to be here.
                    }

                    IEnumerable<SearchByField> searchByFields = args.GetUniqueSearchBy();
                    foreach (SearchByField sbf in searchByFields)
                    {
                        switch (sbf)
                        {
                            case SearchByField.byAssetId:
                                if (args.AssetIDs.Count > 0)
                                {
                                    urlParams.Add(new KeyValuePair<string, string>(sbf.ToString(), ConvertToMultipleURLParams(args.AssetIDs, false)));
                                }
                                else
                                {
                                    urlParams.Add(new KeyValuePair<string, string>(sbf.ToString(), args.AssetID));
                                }
                                break;
                            case SearchByField.byChannelId:
                                urlParams.Add(new KeyValuePair<string, string>(sbf.ToString(), ConvertEpgChannelIdToExternalID(args.EpgChannelID)));
                                break;
                            case SearchByField.byProgramId:
                                urlParams.Add(new KeyValuePair<string, string>(sbf.ToString(), ConvertToMultipleURLParams(args.EpgProgramIDs, false)));
                                break;
                            case SearchByField.byStartTime:
                                urlParams.Add(new KeyValuePair<string, string>(sbf.ToString(), TVinciShared.DateUtils.DateTimeToUnixTimestampMilliseconds(args.StartDate).ToString()));
                                break;
                            case SearchByField.byStatus:
                                urlParams.Add(new KeyValuePair<string, string>(sbf.ToString(), ConvertToMultipleURLParams(args.RecordingStatus, true)));
                                break;
                            case SearchByField.bySeasonId:
                                urlParams.Add(new KeyValuePair<string, string>(sbf.ToString(), ConvertToMultipleURLParams(args.SeriesIDs, false)));
                                break;
                            default:
                                break;
                        }
                    } //foreach

                    urlParams.Add(new KeyValuePair<string, string>(ALU_SORT_FIELD_URL_PARAM, args.OrderBy.ToString()));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SORT_DIRECTION_URL_PARAM, args.Direction.ToString().ToLower()));

                    string url = BuildRestCommand(ALU_READ_COMMAND, ALU_ENDPOINT_RECORD, urlParams);

                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetRetrieveAssetsResponse(responseJson, args, res);
                        }
                        else
                        {
                            throw new Exception(string.Format("RetrieveAssets. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        log.Error(LOG_HEADER_ERROR + string.Format("RetrieveAssets. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        // add status fail to response obj.
                        res.isOK = false;
                        res.msg = "Failed to establise connection to ALU. Refer to Server logs.";
                    }
                }
                else
                {
                    throw new ArgumentException("RetrieveAssets. Input is invalid.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at RetrieveAssets.", args, ex), ex);
                throw;
            }

            return res;
        }

        private List<RecordedEPGChannelProgrammeObject> ParseALUReadResponse(ReadResponseJSON aluResponse)
        {
            List<RecordedEPGChannelProgrammeObject> res = new List<RecordedEPGChannelProgrammeObject>(aluResponse.EntriesLength);
            if (aluResponse != null && aluResponse.entries != null && aluResponse.entries.Count > 0)
            {
                List<Ratio> epgRatios = new List<Ratio>();
                Dictionary<int, List<EpgPicture>> picGroupTree = CatalogDAL.GetGroupTreeMultiPicEpgUrl(groupID, ref epgRatios);

                // choose the higher domain number  ( not the Parent domain) 
                int epgGroupId = groupID;

                if (picGroupTree != null & picGroupTree.Keys.Count > 0)
                {
                    epgGroupId = picGroupTree.Keys.Max();
                    log.Debug("NPVRPics " + string.Format("picGroupTree[{0}] count {1}", epgGroupId, picGroupTree[epgGroupId].Count));

                }


                foreach (EntryJSON entry in aluResponse.entries)
                {
                    RecordedEPGChannelProgrammeObject obj = new RecordedEPGChannelProgrammeObject();
                    obj.RecordingID = entry.AssetID;
                    obj.IsAssetProtected = entry.Protected;
                    obj.ChannelName = entry.ChannelName;
                    obj.DESCRIPTION = entry.Description;
                    obj.START_DATE = GetStartTime(entry);
                    obj.END_DATE = GetEndTime(entry);
                    obj.EPG_CHANNEL_ID = ConvertExternalIDToEpgChannelId(entry.ChannelID);
                    obj.EPG_ID = 0;
                    obj.EPG_IDENTIFIER = entry.ProgramID;
                    obj.EPG_Meta = new List<EPGDictionary>();
                    obj.EPG_TAGS = new List<EPGDictionary>();
                    obj.EPG_PICTURES = new List<EpgPicture>();
                    // return seasonId + seasonName
                    if (!string.IsNullOrEmpty(entry.SeasonID))
                    {
                        EPGDictionary oEPGDictionary = new EPGDictionary();
                        oEPGDictionary.Key = ALU_SEASON_ID;
                        oEPGDictionary.Value = entry.SeasonID;
                        obj.EPG_TAGS.Add(oEPGDictionary);
                    }
                    if (!string.IsNullOrEmpty(entry.SeasonName))
                    {
                        EPGDictionary oEPGDictionary = new EPGDictionary();
                        oEPGDictionary.Key = ALU_SEASON_NAME;
                        oEPGDictionary.Value = entry.SeasonName;
                        obj.EPG_TAGS.Add(oEPGDictionary);
                    }

                    if (entry.Duration != 0)
                    {
                        obj.EPG_TAGS.Add(new EPGDictionary()
                            {
                                Key = ALU_DURATION,
                                Value = entry.Duration.ToString()
                            });
                    }

                    if (!string.IsNullOrEmpty(entry.Genre))
                    {
                        obj.EPG_TAGS.Add(new EPGDictionary()
                        {
                            Key = ALU_GENRE,
                            Value = entry.Genre
                        });
                    }

                    if (!string.IsNullOrEmpty(entry.Episode))
                    {
                        obj.EPG_TAGS.Add(new EPGDictionary()
                        {
                            Key = ALU_EPISODE,
                            Value = entry.Episode
                        });
                    }

                    if (!string.IsNullOrEmpty(entry.Year))
                    {
                        obj.EPG_TAGS.Add(new EPGDictionary()
                        {
                            Key = ALU_YEAR,
                            Value = entry.Year
                        });
                    }

                    if (!string.IsNullOrEmpty(entry.Rating))
                    {
                        obj.EPG_TAGS.Add(new EPGDictionary()
                        {
                            Key = ALU_RATING,
                            Value = entry.Rating
                        });
                    }

                    if (!string.IsNullOrEmpty(entry.SeasonNumber))
                    {
                        obj.EPG_TAGS.Add(new EPGDictionary()
                        {
                            Key = ALU_SEASON_NUMBER,
                            Value = entry.SeasonNumber
                        });
                    }

                    obj.PIC_URL = entry.Thumbnail;

                    if (!string.IsNullOrEmpty(entry.Thumbnail))
                    {

                        if (!entry.Thumbnail.ToLower().StartsWith("http://"))
                        {

                            Dictionary<int, KeyValuePair<string, string>> ratioDictionary = SetRatioList(entry.Thumbnail);

                            if (picGroupTree != null && picGroupTree.Count > 0 && picGroupTree.ContainsKey(epgGroupId))
                            {
                                SetEpgPictures(ratioDictionary, obj, picGroupTree[epgGroupId]);
                                if (obj.EPG_PICTURES.Count > 0)
                                {
                                    obj.PIC_URL = obj.EPG_PICTURES[0].Url;
                                }
                            }
                            else
                            {
                                // no sizes defined
                                if (!WS_Utils.IsGroupIDContainedInConfig(groupID, USE_OLD_IMAGE_SERVER_KEY, ';') &&
                                    epgRatios != null &&
                                    epgRatios.Count > 0)
                                {
                                    // use new image server flow
                                    foreach (var item in ratioDictionary)
                                    {
                                        obj.EPG_PICTURES.Add(new EpgPicture()
                                        {
                                            PicHeight = 0,
                                            PicID = 0,
                                            PicWidth = 0,
                                            Ratio = epgRatios.Where(x => x.Id == item.Key).First().Name,
                                            RatioId = item.Key,
                                            Url = ImageUtils.BuildImageUrl(groupID, item.Value.Key)
                                        });
                                    }
                                }
                            }
                        }
                    }


                    obj.GROUP_ID = groupID.ToString();
                    obj.IS_ACTIVE = "true";
                    obj.LIKE_COUNTER = 0;
                    obj.media_id = string.Empty;
                    obj.NAME = entry.Name;
                    obj.PUBLISH_DATE = string.Empty;
                    obj.STATUS = entry.Status;
                    obj.RecordSource = entry.Source;
                    res.Add(obj);
                }

            }
            return res;
        }

        private void SetEpgPictures(Dictionary<int, KeyValuePair<string, string>> ratioDic, RecordedEPGChannelProgrammeObject obj, List<EpgPicture> pictures)
        {
            if (pictures == null || pictures.Count == 0)
            {
                log.Debug("SetEpgPictures - " + string.Format("pictures is null or empty"));
                return;
            }

            StringBuilder urlStr;

            foreach (EpgPicture pic in pictures)
            {
                if (ratioDic.ContainsKey(pic.RatioId))
                {
                    urlStr = new StringBuilder();
                    urlStr.Append(pic.Url);
                    urlStr.Append(ratioDic[pic.RatioId].Key);
                    urlStr.Append(string.Format("_{0}X{1}.", pic.PicWidth, pic.PicHeight));
                    urlStr.Append(ratioDic[pic.RatioId].Value);

                    log.Debug("SetEpgPictures " + string.Format("RatioId= {0} Name= {1}", pic.RatioId, ratioDic[pic.RatioId].Key));


                    string url = string.Empty;
                    if (WS_Utils.IsGroupIDContainedInConfig(groupID, USE_OLD_IMAGE_SERVER_KEY, ';'))
                    {
                        // use old image server flow
                        url = urlStr.ToString();
                    }
                    else
                    {
                        // use new image server flow
                        url = ImageUtils.BuildImageUrl(groupID, ratioDic[pic.RatioId].Key, 0, pic.PicWidth, pic.PicHeight, 100, false);
                    }

                    obj.EPG_PICTURES.Add(new EpgPicture()
                    {
                        PicHeight = pic.PicHeight,
                        PicID = pic.PicID,
                        PicWidth = pic.PicWidth,
                        Ratio = pic.Ratio,
                        RatioId = pic.RatioId,
                        Url = url
                    });
                }
            }
        }

        private Dictionary<int, KeyValuePair<string, string>> SetRatioList(string thumbnail)
        {
            log.Debug("SetRatioList " + string.Format("thumbnail={0}", thumbnail));

            string sep = ";";
            var pics = thumbnail.Split(sep.ToCharArray());   //sample of thumbnail-->  [rationid]=[basepic].[suffix];;
            var list = new Dictionary<int, KeyValuePair<string, string>>();

            foreach (string pic in pics)
            {
                log.Debug("SetRatioList " + string.Format("pic={0}", pic));
                if (!string.IsNullOrEmpty(pic))
                {
                    var internalStr = pic.Split((new char[] { '=', '.' }));
                    if (internalStr.Length == 3)
                    {
                        int ratioId = 0;
                        if (int.TryParse(internalStr[0], out ratioId))
                        {
                            log.Debug("SetRatioList " + string.Format("pic={0} ratioId={1}", pic, ratioId));
                            list.Add(ratioId, new KeyValuePair<string, string>(internalStr[1], internalStr[2]));
                        }
                    }
                }
            }

            return list;
        }

        private string GetEndTime(EntryJSON entry)
        {
            long unixTime = 0;
            if (!string.IsNullOrEmpty(entry.EndTime) && Int64.TryParse(entry.EndTime, out unixTime))
                return TVinciShared.DateUtils.UnixTimeStampMillisecondsToDateTime(unixTime).ToString(DATE_FORMAT);
            if (entry.Duration > 0 && !string.IsNullOrEmpty(entry.StartTime) && Int64.TryParse(entry.StartTime, out unixTime))
                return TVinciShared.DateUtils.UnixTimeStampMillisecondsToDateTime(unixTime).AddSeconds(entry.Duration).ToString(DATE_FORMAT);
            return UNIX_ZERO_TIME.ToString(DATE_FORMAT);
        }

        private string GetStartTime(EntryJSON entry)
        {
            long unixTime = 0;
            if (!string.IsNullOrEmpty(entry.StartTime) && Int64.TryParse(entry.StartTime, out unixTime))
                return TVinciShared.DateUtils.UnixTimeStampMillisecondsToDateTime(unixTime).ToString(DATE_FORMAT);
            return UNIX_ZERO_TIME.ToString(DATE_FORMAT);
        }

        private void GetRetrieveAssetsResponse(string responseJson, NPVRRetrieveParamsObj args, NPVRRetrieveAssetsResponse response)
        {
            try
            {
                ReadResponseJSON success = JsonConvert.DeserializeObject<ReadResponseJSON>(responseJson);
                if (success != null)
                {
                    response.entityID = args.EntityID;
                    response.isOK = true;
                    response.msg = string.Empty;
                    response.totalItems = success.EntriesLength;
                    response.results = ParseALUReadResponse(success);
                }
                else
                {
                    try
                    {
                        GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                        response.isOK = false;
                        response.entityID = args.EntityID;
                        response.msg = error.Description;
                        response.totalItems = 0;
                        response.results = new List<RecordedEPGChannelProgrammeObject>(0);

                        log.Error("Error - " + GetLogMsg(string.Format("An error occurred while trying to retrieve assets from ALU. Resp JSON: {0}", responseJson), args, null));

                    }
                    catch (Exception ex)
                    {
                        GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                        response.isOK = false;
                        response.entityID = args.EntityID;
                        response.msg = error.Description;
                        response.totalItems = 0;
                        response.results = new List<RecordedEPGChannelProgrammeObject>(0);
                        log.Error("Exception - " + GetLogMsg(String.Concat("Failed to deserialize JSON at GetRetrieveAssetsResponse.Inner catch block. Response JSON: ", responseJson), args, ex), ex);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + GetLogMsg(String.Concat("Failed to deserialize JSON at GetRetrieveAssetsResponse.Inner catch block. Response JSON: ", responseJson), args, ex), ex);
                throw;
            }
        }

        private string ConvertToMultipleURLParams<T>(List<T> args, bool isLowerElementsInList)
        {
            StringBuilder sb = new StringBuilder();
            if (args != null && args.Count > 0)
            {
                for (int i = 0; i < args.Count; i++)
                    sb.Append(String.Concat(i == 0 ? string.Empty : ",", isLowerElementsInList ? args[i].ToString().ToLower() : args[i].ToString()));
            }

            return sb.ToString();
        }

        public NPVRRecordResponse RecordSeries(NPVRParamsObj args)
        {
            NPVRRecordResponse res = new NPVRRecordResponse();
            try
            {
                if (IsRecordAssetInputValid(args))
                {
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(5);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "2.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_PROGRAM_ID_URL_PARAM, args.AssetID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_CHANNEL_ID_URL_PARAM, ConvertEpgChannelIdToExternalID(args.EpgChannelID)));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_START_TIME_URL_PARAM, TVinciShared.DateUtils.DateTimeToUnixTimestampMilliseconds(args.StartDate).ToString()));

                    string url = BuildRestCommand(ALU_ADD_BY_PROGRAM_COMMAND, ALU_ENDPOINT_SEASON, urlParams);

                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetRecordSeriesResponse(responseJson, args, res);
                        }
                        else
                        {
                            throw new Exception(string.Format("RecordSeries. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        log.Error(LOG_HEADER_ERROR + string.Format("RecordSeries. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.entityID = args.EntityID;
                        res.recordingID = string.Empty;
                        res.status = RecordStatus.Error;
                        res.msg = "An error occurred. Refer to server log files.";
                    }
                }
                else
                {
                    // input is not valid
                    throw new ArgumentException("Either args obj is null or entity id is empty or asset id is empty or epg channel id is empty.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at RecordSeries.", args, ex), ex);
                throw;
            }

            return res;
        }

        private void GetRecordSeriesResponse(string responseJson, NPVRParamsObj args, NPVRRecordResponse response)
        {
            try
            {
                RecordSeriesResponseJSON success = JsonConvert.DeserializeObject<RecordSeriesResponseJSON>(responseJson);
                if (success != null && !string.IsNullOrEmpty(success.RecordingID))
                {
                    response.entityID = args.EntityID;
                    response.msg = string.Empty;
                    response.status = RecordStatus.OK;
                    response.recordingID = success.RecordingID;
                }
                else
                {
                    GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                    response.recordingID = string.Empty;
                    response.msg = error.Description;
                    switch (error.ResultCode)
                    {
                        case 210:
                            response.status = RecordStatus.ResourceAlreadyExists;
                            response.msg = "Trying to create a resource that does already exist.";
                            break;
                        case 420:
                            response.status = RecordStatus.QuotaExceeded;
                            response.msg = "Recording can not be done because the user has exceeded the assigned quota.";
                            break;
                        default:
                            GetGenericFailureResponse(response, error);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + GetLogMsg(String.Concat("Failed to deserialize JSON at GetRecordSeriesResponse.Inner catch block. Response JSON: ", responseJson), args, ex), ex);
                throw;
            }
        }

        private void GetGenericFailureResponse(NPVRRecordResponse response, GenericFailureResponseJSON error)
        {
            switch (error.ResultCode)
            {
                case 404:
                    response.status = RecordStatus.AssetDoesNotExist;
                    response.msg = "Asset does not exist.";
                    break;
                case 409:
                    response.status = RecordStatus.InvalidStatus;
                    response.msg = "The status of a resource does not allow to perform the operation.";
                    break;
                case 400:
                    response.status = RecordStatus.BadRequest;
                    response.msg = "Generic problem with arguments syntax.";
                    break;
                case 401:
                    response.status = RecordStatus.UnauthorizedOperation;
                    response.msg = "Operation is forbidden due to lack of privileges.";
                    break;
                case 408:
                    response.status = RecordStatus.CommunicationsError;
                    response.msg = "Request has not been completed in time due to communication problems.";
                    break;
                case 500:
                    response.status = RecordStatus.InternalServerError;
                    response.msg = "Request has not been completed in time due to communication problems.";
                    break;
                case 501:
                    response.status = RecordStatus.NotImplemented;
                    response.msg = "Parameter value not supported by the method.";
                    break;
                default:
                    response.status = RecordStatus.Error;
                    response.msg = "Unknown error";
                    break;
            }
        }

        public NPVRCancelDeleteResponse CancelSeries(NPVRParamsObj args)
        {
            NPVRCancelDeleteResponse res = new NPVRCancelDeleteResponse();
            try
            {
                if (IsCancelDeleteAssetInputValid(args))
                {
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(3);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_ID_URL_PARAM, args.AssetID));

                    string url = BuildRestCommand(ALU_CANCEL_COMMAND, ALU_ENDPOINT_SEASON, urlParams);

                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        // parse here json
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetCancelAssetResponse(responseJson, args, res);
                        }
                        else
                        {
                            throw new Exception(string.Format("CancelSeries. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        log.Error(LOG_HEADER_ERROR + string.Format("CancelSeries. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.entityID = args.EntityID;
                        res.recordingID = string.Empty;
                        res.status = CancelDeleteStatus.Error;
                        res.msg = "An error occurred. Refer to server log files.";
                    }
                }
                else
                {
                    throw new ArgumentException("Either args obj is null or entity id is empty or asset id is empty.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at CancelSeries.", args, ex), ex);
                throw;
            }

            return res;
        }

        public NPVRCancelDeleteResponse DeleteSeries(NPVRParamsObj args)
        {
            NPVRCancelDeleteResponse res = new NPVRCancelDeleteResponse();
            try
            {
                if (IsCancelDeleteAssetInputValid(args))
                {
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(3);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_ID_URL_PARAM, args.AssetID));

                    string url = BuildRestCommand(ALU_DELETE_COMMAND, ALU_ENDPOINT_SEASON, urlParams);

                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetDeleteAssetResponse(responseJson, args, res);
                        }
                        else
                        {
                            throw new Exception(string.Format("DeleteSeries. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        log.Error(LOG_HEADER_ERROR + string.Format("DeleteSeries. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.entityID = args.EntityID;
                        res.recordingID = string.Empty;
                        res.status = CancelDeleteStatus.Error;
                        res.msg = "An error occurred. Refer to server log files.";
                    }
                }
                else
                {
                    throw new ArgumentException("Either args obj is null or entity id is empty or asset id is empty.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at DeleteSeries.", args, ex), ex);
                throw;
            }

            return res;
        }

        private bool IsLicensedLinkInputValid(NPVRParamsObj args)
        {
            if (!string.IsNullOrEmpty(args.AssetID))
                return !(string.IsNullOrEmpty(args.HASFormat) ^ string.IsNullOrEmpty(args.StreamType));
            return false;
        }

        public NPVRLicensedLinkResponse GetNPVRLicensedLink(NPVRParamsObj args)
        {
            NPVRLicensedLinkResponse res = new NPVRLicensedLinkResponse();
            try
            {
                if (IsLicensedLinkInputValid(args))
                {
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(5);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_FORM_URL_PARAM, "json"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_ASSET_ID_URL_PARAM, args.AssetID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));
                    if (!string.IsNullOrEmpty(args.StreamType) && !string.IsNullOrEmpty(args.HASFormat))
                    {
                        urlParams.Add(new KeyValuePair<string, string>(ALU_STREAM_TYPE_URL_PARAM, args.StreamType));
                        urlParams.Add(new KeyValuePair<string, string>(ALU_HAS_FORMAT_URL_PARAM, args.HASFormat));
                    }

                    string url = BuildRestCommand(ALU_GET_LOCATOR_COMMAND, ALU_ENDPOINT_RECORD, urlParams);

                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;
                    Dictionary<string, string> headersToAdd = null;
                    if (!string.IsNullOrEmpty(args.XkData))
                    {
                        headersToAdd = new Dictionary<string, string>() { { ALU_X_KDATA, args.XkData } };
                    }

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg, headersToAdd))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetGetNPVRLicensedLinkResponse(responseJson, args, res);
                        }
                        else
                        {
                            throw new Exception(string.Format("GetNPVRLicensedLink. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        log.Error(LOG_HEADER_ERROR + string.Format("GetNPVRLicensedLink. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.isOK = false;
                        res.licensedLink = string.Empty;
                        res.msg = "An error occurred. Refer to server log files.";
                    }

                }
                else
                {
                    throw new ArgumentException("GetNPVRLicensedLink input is invalid.");
                }

            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at GetNPVRLicensedLink.", args, ex), ex);
                throw;
            }

            return res;
        }

        private void GetGetNPVRLicensedLinkResponse(string responseJson, NPVRParamsObj args, NPVRLicensedLinkResponse response)
        {
            try
            {
                GetLocatorResponseJSON success = JsonConvert.DeserializeObject<GetLocatorResponseJSON>(responseJson);
                if (success != null && !string.IsNullOrEmpty(success.Locator))
                {
                    response.isOK = true;
                    response.licensedLink = success.Locator;
                }
                else
                {
                    GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                    response.isOK = false;
                    response.licensedLink = string.Empty;
                    switch (error.ResultCode)
                    {
                        case 404:
                            response.msg = "Asset does not exist.";
                            break;
                        default:
                            response.msg = error.Description;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + GetLogMsg(string.Format("Exception at GetGetNPVRLicensedLinkResponse. Inner catch block. Resp JSON: {0}", responseJson), args, ex), ex);
                throw;
            }
        }

        private bool IsRetrieveSeriesInputValid(NPVRRetrieveParamsObj args)
        {
            return args != null && !string.IsNullOrEmpty(args.EntityID) && (args.PageSize > 0 || args.PageIndex == 0);
        }

        private List<RecordedSeriesObject> ExtractRecordedSeries(ReadSeriesResponseJSON responseJson)
        {
            List<RecordedSeriesObject> res = new List<RecordedSeriesObject>(responseJson.Entries.Count);
            foreach (SeriesEntryJSON entry in responseJson.Entries)
            {
                RecordedSeriesObject obj = new RecordedSeriesObject();
                obj.epgChannelID = ConvertExternalIDToEpgChannelId(entry.ChannelID);
                obj.recordingID = entry.RecordingID;
                obj.seriesID = entry.SeriesID;
                obj.seriesName = entry.SeriesName;
                res.Add(obj);
            }

            return res;
        }

        private void GetRetrieveSeriesResponse(string responseJson, NPVRRetrieveParamsObj args, NPVRRetrieveSeriesResponse response)
        {
            try
            {
                ReadSeriesResponseJSON success = JsonConvert.DeserializeObject<ReadSeriesResponseJSON>(responseJson);
                response.isOK = true;
                response.msg = string.Empty;
                response.results = ExtractRecordedSeries(success);
                response.totalItems = response.results.Count;
            }
            catch (Exception jsonEx)
            {
                try
                {
                    log.Error("", jsonEx);
                    GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                    response.isOK = false;
                    response.results = new List<RecordedSeriesObject>(0);
                    response.totalItems = 0;
                    switch (error.ResultCode)
                    {
                        case 404:
                            response.msg = "Entity ID probably does not exist";
                            break;
                        default:
                            response.msg = error.Description;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Exception - " + GetLogMsg(string.Format("Exception at GetRetrieveSeriesResponse. Inner catch block. Resp JSON: {0}", responseJson), args, ex), ex);
                    throw;
                }
            }
        }

        public NPVRRetrieveSeriesResponse RetrieveSeries(NPVRRetrieveParamsObj args)
        {
            NPVRRetrieveSeriesResponse res = new NPVRRetrieveSeriesResponse();
            try
            {
                if (IsRetrieveSeriesInputValid(args))
                {
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>();
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_COUNT_URL_PARAM, "true"));
                    if (args.PageSize > 0)
                    {
                        urlParams.Add(new KeyValuePair<string, string>(ALU_ENTRIES_PAGE_SIZE_URL_PARAM, args.PageSize.ToString()));
                        urlParams.Add(new KeyValuePair<string, string>(ALU_ENTRIES_START_INDEX_URL_PARAM, args.PageIndex.ToString()));
                    }

                    string url = BuildRestCommand(ALU_READ_COMMAND, ALU_ENDPOINT_SEASON, urlParams);

                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetRetrieveSeriesResponse(responseJson, args, res);
                        }
                        else
                        {
                            throw new Exception(string.Format("RetrieveAssets. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        log.Error(LOG_HEADER_ERROR + string.Format("RetrieveSeries. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                    }
                }
                else
                {
                    // input not valid
                    throw new ArgumentException("RetrieveSeries input is invalid.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at RetrieveSeries.", args, ex), ex);
                throw;
            }

            return res;
        }

        private static void SetRecordedSeriesListAccordingToPaging(ref NPVRRetrieveSeriesResponse res, int pageSize, int pageIndex)
        {
            int validNumberOfMediasRange = pageSize;
            if (TVinciShared.WS_Utils.ValidatePageSizeAndPageIndexAgainstToltalList(res.totalItems, pageIndex, ref validNumberOfMediasRange))
            {
                if (validNumberOfMediasRange > 0)
                {
                    res.results = res.results.GetRange(pageSize * pageIndex, validNumberOfMediasRange);
                }
            }
            else
            {
                res.results.Clear();
            }
        }

        public NPVRUserActionResponse UpdateAccount(NPVRParamsObj args)
        {
            NPVRUserActionResponse res = new NPVRUserActionResponse();
            try
            {
                if (IsCreateOrUpdateInputValid(args))
                {
                    log.Debug("UpdateAccount - " + string.Format("UpdateAccount request has been issued. G ID: {0} , Params Obj: {1}", groupID, args.ToString()));
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(3);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_QUOTA_URL_PARAM, (args.Quota * 60).ToString()));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));
                    string sAccountID = TVinciShared.WS_Utils.GetTcmGenericValue<string>(string.Format("ALU_ACCOUNT_ID_{0}", groupID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_ACCOUNT_ID_URL_PARAM, sAccountID));

                    string url = BuildRestCommand(ALU_CREATE_ACCOUNT_COMMAND, ALU_ENDPOINT_USER, urlParams);
                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;
                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        if (httpStatusCode == HTTP_STATUS_OK)
                        {
                            GetAccountResponse(responseJson, args, res, "Update");

                            log.Debug(string.Format("UpdateAccount. Group ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode));
                        }
                        else
                        {
                            throw new Exception(string.Format("UpdateAccount. Connection error to ALU. HTTP Status Code: {0} , Response JSON: {1} , Err Msg: {2}", httpStatusCode, responseJson, errorMsg));
                        }
                    }
                    else
                    {
                        // log here the error. 
                        log.Error(LOG_HEADER_ERROR + string.Format("UpdateAccount. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg));
                        res.isOK = false;
                        res.msg = "An error occurred. Refer to server log files.";
                        res.quota = 0;
                        res.entityID = string.Empty;
                    }
                }
                else
                {
                    throw new ArgumentException("Either args obj is null or domain id is empty or quota is non-positive.");
                }
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception at CreateAccount.", args, ex), ex);
                throw;
            }

            return res;
        }

        private void GetAccountResponse(string responseJson, NPVRParamsObj args, NPVRUserActionResponse response, string action)
        {
            string unbeautified = JSON_UNBEAUTIFIER.Replace(responseJson, string.Empty);
            if (unbeautified.Equals(EMPTY_JSON))
            {
                response.isOK = true;
                response.quota = args.Quota;
                response.entityID = args.EntityID;
            }
            else
            {
                try
                {
                    GenericFailureResponseJSON error = JsonConvert.DeserializeObject<GenericFailureResponseJSON>(responseJson);
                    response.entityID = args.EntityID;
                    response.msg = error.Description;
                    response.quota = 0;
                    response.isOK = false;

                    log.Error("Error - " + GetLogMsg(string.Format("Failed to {0} account. Error resp: {1}", action, error != null ? error.ToString() : "generic failure response is null"), args, null));

                }
                catch (Exception ex)
                {
                    log.Error("Exception - " + GetLogMsg(string.Format("Exception at GetAccountResponse. Action: {0}, Resp JSON: {1}", action, responseJson), args, ex), ex);
                    throw;
                }
            }
        }

        private string ConvertEpgChannelIdToExternalID(string epgChannelId)
        {
            string cdvrId = string.Empty;
            try
            {
                string key = LayeredCacheKeys.GetEpgChannelExternalIdKey(groupID, epgChannelId);
                // try to get from cache            
                bool cacheResult = LayeredCache.Instance.Get<string>(key, ref cdvrId, GetExternalIdByEpgChannelId, new Dictionary<string, object>() { { "groupId", groupID },
                                                                    { "epgChannelId", epgChannelId } }, groupID, LayeredCacheConfigNames.GET_EPG_CHANNEL_CDVR_ID);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail in ConvertEpgChannelIdToExternalID epgChannelId={0}, ex={1}", epgChannelId, ex);
            }
            return string.IsNullOrEmpty(cdvrId) ? epgChannelId : cdvrId;
        }

        private string ConvertExternalIDToEpgChannelId(string cdvrId)
        {
            int epgChannelId = 0;
            try
            {
                string key = LayeredCacheKeys.GetExternalIdEpgChannelKey(groupID, cdvrId);
                // try to get from cache            
                bool cacheResult = LayeredCache.Instance.Get<int>(key, ref epgChannelId, GetEpgChannelIdByExternalId, new Dictionary<string, object>() { { "groupId", groupID },
                                                                    { "cdvrId", cdvrId } }, groupID, LayeredCacheConfigNames.GET_EPG_CHANNEL_CDVR_ID);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail in ConvertEpgChannelIdToExternalID epgChannelId={0}, ex={1}", epgChannelId, ex);
            }
            return epgChannelId > 0 ? epgChannelId.ToString() : cdvrId;
        }


        private Tuple<string, bool> GetExternalIdByEpgChannelId(Dictionary<string, object> funcParams)
        {
            bool res = false;
            string response = string.Empty;

            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("epgChannelId") && !string.IsNullOrEmpty(funcParams.ContainsKey("epgChannelId").ToString()))
                    {
                        int? epgChannelId;
                        epgChannelId = funcParams["epgChannelId"] as int?;

                        if (epgChannelId.HasValue)
                        {
                            response = DAL.ConditionalAccessDAL.GetExternalIdByEpgChannel(epgChannelId.Value);
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetExternalIdByEpgChannelId failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<string, bool>(response, res);
        }

        private Tuple<int, bool> GetEpgChannelIdByExternalId(Dictionary<string, object> funcParams)
        {
            bool res = false;
            int response = 0;

            try
            {
                if (funcParams != null && funcParams.Count == 2)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("cdvrId"))
                    {
                        int? groupId;
                        string cdvrId = string.Empty;
                        groupId = funcParams["groupId"] as int?;
                        cdvrId = funcParams["cdvrId"].ToString();

                        if (groupId.HasValue && !string.IsNullOrEmpty(cdvrId))
                        {
                            response = DAL.ConditionalAccessDAL.GetEpgChannelByExternalId(groupId.Value, cdvrId);
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetEpgChannelIdByExternalId failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<int, bool>(response, res);
        }
    }
}
