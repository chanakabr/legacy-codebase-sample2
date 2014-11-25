using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    /*
     * Don't change the visibility of this class to public. Any communication with a third party NPVR Provider should be done via
     * the interface INPVRProvider.
     */ 
    internal class AlcatelLucentNPVR : INPVRProvider
    {
        private static readonly string ALU_LOG_FILE = "AlcatelLucent";
        private static readonly string LOG_HEADER_EXCEPTION = "Exception";
        private static readonly string LOG_HEADER_ERROR = "Error";

        private static readonly string DATE_TIME_FORMAT = "yyyyMMddHHmmss";
        private static readonly DateTime UNIX_ZERO_TIME = new DateTime(1970, 1, 1);

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

        private static readonly string ALU_FORM_URL_PARAM = "form";
        private static readonly string ALU_QUOTA_URL_PARAM = "quota";
        private static readonly string ALU_SCHEMA_URL_PARAM = "schema";
        private static readonly string ALU_USER_ID_URL_PARAM = "userId";
        private static readonly string ALU_PROGRAM_ID_URL_PARAM = "programId";
        private static readonly string ALU_CHANNEL_ID_URL_PARAM = "channelId";
        private static readonly string ALU_START_TIME_URL_PARAM = "startTime";
        private static readonly string ALU_ASSET_ID_URL_PARAM = "assetId";
        private static readonly string ALU_NAME_URL_PARAM = "name";
        private static readonly string ALU_VALUE_URL_PARAM = "value";

        private int groupID;

        public AlcatelLucentNPVR(int groupID)
        {
            this.groupID = groupID;
        }

        private bool IsCreateInputValid(NPVRParamsObj args)
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

        public NPVRUserActionResponse CreateAccount(NPVRParamsObj args)
        {
            NPVRUserActionResponse res = new NPVRUserActionResponse();
            try
            {
                if (IsCreateInputValid(args))
                {
                    Logger.Logger.Log("CreateAccount", string.Format("CreateAccount request has been issued. G ID: {0} , Params Obj: {1}", groupID, args.ToString()), GetLogFilename());
                    List<KeyValuePair<string, string>> urlParams = new List<KeyValuePair<string, string>>(3);
                    urlParams.Add(new KeyValuePair<string, string>(ALU_QUOTA_URL_PARAM, args.Quota.ToString()));
                    urlParams.Add(new KeyValuePair<string,string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string,string>(ALU_USER_ID_URL_PARAM, args.EntityID));

                    string url = BuildRestCommand(ALU_CREATE_ACCOUNT_COMMAND, ALU_ENDPOINT_USER, urlParams);
                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;
                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        // parse here the json, understand the response and fill the response object

                    }
                    else
                    {
                        // log here the error. 
                        Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("CreateAccount. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg), GetLogFilename());
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
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetLogMsg("Exception at CreateAccount.", args, ex), GetLogFilename());
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
                    Logger.Logger.Log("DeleteAccount", string.Format("DeleteAccount request has been issued. G ID: {0} , Params Obj: {1}", groupID, args.ToString()), GetLogFilename());
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
                        // parse here the json, understand the response and fill the result obj.
                    }
                    else
                    {
                        // log here the error
                        Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("DeleteAccount. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg), GetLogFilename());
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
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetLogMsg("Exception at DeleteAccount.", args, ex), GetLogFilename());
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
                        // parse here the json.
                    }
                    else
                    {
                        // log here the error
                        Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("GetQuotaData. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg), GetLogFilename());
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
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetLogMsg("Exception at GetQuotaData.", args, ex), GetLogFilename());
                throw;
            }
            return res;
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
                    urlParams.Add(new KeyValuePair<string, string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_PROGRAM_ID_URL_PARAM, args.AssetID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_CHANNEL_ID_URL_PARAM, args.EpgChannelID));
                    urlParams.Add(new KeyValuePair<string,string>(ALU_START_TIME_URL_PARAM, TVinciShared.DateUtils.DateTimeToUnixTimestamp(args.StartDate).ToString()));

                    string url = BuildRestCommand(ALU_ADD_BY_PROGRAM_COMMAND, ALU_ENDPOINT_RECORD, urlParams);
                    
                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if(TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg)) 
                    {
                        // parse here json
                    }
                    else 
                    {
                        Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("RecordAsset. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg), GetLogFilename());
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
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetLogMsg("Exception at RecordAsset", args, ex), GetLogFilename());
                throw;
            }

            return res;
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
                    urlParams.Add(new KeyValuePair<string,string>(ALU_SCHEMA_URL_PARAM, "1.0"));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_ASSET_ID_URL_PARAM, args.AssetID));
                    urlParams.Add(new KeyValuePair<string, string>(ALU_USER_ID_URL_PARAM, args.EntityID));

                    string url = BuildRestCommand(ALU_CANCEL_COMMAND, ALU_ENDPOINT_RECORD, urlParams);

                    int httpStatusCode = 0;
                    string responseJson = string.Empty;
                    string errorMsg = string.Empty;

                    if (TVinciShared.WS_Utils.TrySendHttpGetRequest(url, Encoding.UTF8, ref httpStatusCode, ref responseJson, ref errorMsg))
                    {
                        // parse here json
                    }
                    else
                    {
                        Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("CancelAsset. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg), GetLogFilename());
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
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetLogMsg("Exception at CancelAsset.", args, ex), GetLogFilename());
                throw;
            }

            return res;
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
                        // parse here json
                    }
                    else
                    {
                        Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("DeleteAsset. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg), GetLogFilename());
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
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetLogMsg("Exception at DeleteAsset.", args, ex), GetLogFilename());
                throw;
            }

            return res;
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
                        // parse here json
                    }
                    else
                    {
                        Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("SetAssetProtectionStatus. An error occurred while trying to contact ALU REST interface. G ID: {0} , Params Obj: {1} , HTTP Status Code: {2} , Info: {3}", groupID, args.ToString(), httpStatusCode, errorMsg), GetLogFilename());
                        res.recordingID = string.Empty;
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
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetLogMsg("Exception at SetAssetProtectionStatus.", args, ex), GetLogFilename());
                throw;
            }

            return res;
        }

        private bool IsRetrieveAssetsInputValid(NPVRRetrieveParamsObj args, ref ulong uniqueSearchBy)
        {
            if (args != null && args.SearchBy.Count > 0 && !string.IsNullOrEmpty(args.EntityID))
            {
                bool seenUnique = false;
                IEnumerable<SearchByField> distinct = args.SearchBy.Distinct();
                foreach (SearchByField sbf in distinct)
                {
                    switch (sbf)
                    {
                        case SearchByField.byAssetId:
                            if ((string.IsNullOrEmpty(args.AssetID) && args.AssetIDs.Count == 0) || seenUnique)
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
                            if (string.IsNullOrEmpty(args.EpgProgramID))
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

                }
                else
                {
                    throw new ArgumentException("Input is invalid.");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetLogMsg("Exception at RetrieveAssets.", args, ex), GetLogFilename());
                throw;
            }

            return res;
        }
    }
}
