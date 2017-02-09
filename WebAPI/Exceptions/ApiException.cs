using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Http;
using System.Xml.Serialization;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class ApiException : HttpResponseException
    {
        // Domain Section 1000 - 1999
        public static ClientExceptionType Domain_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.DomainAlreadyExists, "Domain Already Exists", "The household you entered already exists in the system");
        public static ClientExceptionType EXCEEDED_LIMIT = new ClientExceptionType(eResponseStatus.ExceededLimit, "Exceeded Limit", "The number of devices or users has exceeded the household limit");
        public static ClientExceptionType DEVICE_TYPE_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.DeviceTypeNotAllowed, "Device Type Not Allowed", "The device type you selected is not supported by the system");
        public static ClientExceptionType DEVICE_NOT_IN_Domain = new ClientExceptionType(eResponseStatus.DeviceNotInDomain, "Device Not In Domain", "The device you selected is not part of the current household");
        public static ClientExceptionType MASTER_EMAIL_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.MasterEmailAlreadyExists, "Master Email Already Exists", "The master email address you entered already exists in the system");
        public static ClientExceptionType USER_NOT_IN_Domain = new ClientExceptionType(eResponseStatus.UserNotInDomain, "User Not In Domain", "The user you selected is not part of the current household");
        public static ClientExceptionType Domain_NOT_EXISTS = new ClientExceptionType(eResponseStatus.DomainNotExists, "Domain Not Exists", "The household you selected does not exist");
        public static ClientExceptionType Domain_USER_FAILED = new ClientExceptionType(eResponseStatus.HouseholdUserFailed, "Domain User Failed", "The system was unable to register this household user");
        public static ClientExceptionType Domain_CREATED_WITHOUT_NPVRACCOUNT = new ClientExceptionType(eResponseStatus.DomainCreatedWithoutNPVRAccount, "Domain Created Without NPVRAccount", "The household was created without an external NPVR account");
        public static ClientExceptionType Domain_SUSPENDED = new ClientExceptionType(eResponseStatus.DomainSuspended, "Domain Suspended", "Unable to perform the action requested because the household has been suspended");
        public static ClientExceptionType DLM_NOT_EXIST = new ClientExceptionType(eResponseStatus.DlmNotExist, "Dlm Not Exist", "The device limitation module (DLM) you entered does not exist in the system");
        public static ClientExceptionType WRONG_PASSWORD_OR_USER_NAME = new ClientExceptionType(eResponseStatus.WrongPasswordOrUserName, "Wrong Password Or User Name",
            "Unable to authenticate this user - the user name or password are incorrect. Please re-enter this information");
        public static ClientExceptionType Domain_ALREADY_SUSPENDED = new ClientExceptionType(eResponseStatus.DomainAlreadySuspended, "Domain Already Suspended", "This household has already been suspended");
        public static ClientExceptionType Domain_ALREADY_ACTIVE = new ClientExceptionType(eResponseStatus.DomainAlreadyActive, "Domain Already Active", "This household has already been activated");
        public static ClientExceptionType LIMITATION_PERIOD = new ClientExceptionType(eResponseStatus.LimitationPeriod, "Limitation Period", "Unable to remove the device or user from the household because of the limitation period");
        public static ClientExceptionType DEVICE_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.DeviceAlreadyExists, "Device Already Exists", "The device you are trying to add already exists");
        public static ClientExceptionType DEVICE_EXISTS_IN_OTHER_DomainS = new ClientExceptionType(eResponseStatus.DeviceExistsInOtherDomains, "Device Exists In Other Domains",
            "Unable to add this device to this household because the device is already associated with another household");
        public static ClientExceptionType NO_USERS_IN_Domain = new ClientExceptionType(eResponseStatus.NoUsersInDomain, "No Users In Domain", "There are no users associated with this household");
        public static ClientExceptionType USER_EXISTS_IN_OTHER_DomainS = new ClientExceptionType(eResponseStatus.UserExistsInOtherDomains, "User Exists In Other Domains", "Unable to add this user to this household because the user is already associated with another household");
        public static ClientExceptionType DEVICE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.DeviceNotExists, "Device Not Exists", "The device you selected for this action does not exist in the household");
        public static ClientExceptionType USER_NOT_EXISTS_IN_Domain = new ClientExceptionType(eResponseStatus.UserNotExistsInDomain, "User Not Exists In Domain", "The user you selected for this action does not exist in the household");
        public static ClientExceptionType ACTION_USER_NOT_MASTER = new ClientExceptionType(eResponseStatus.ActionUserNotMaster, "Action User Not Master", "Unable to perform this action: the user is not the household master");
        public static ClientExceptionType EXCEEDED_USER_LIMIT = new ClientExceptionType(eResponseStatus.ExceededUserLimit, "Exceeded User Limit", "Unable to perform this action: you have exceeded the number of users for this household");
        public static ClientExceptionType Domain_NOT_INITIALIZED = new ClientExceptionType(eResponseStatus.DomainNotInitialized, "Domain Not Initialized", "This household has not been initialized");
        public static ClientExceptionType DEVICE_NOT_CONFIRMED = new ClientExceptionType(eResponseStatus.DeviceNotConfirmed, "Device Not Confirmed", "Unable to confirm this device");
        public static ClientExceptionType REQUEST_FAILED = new ClientExceptionType(eResponseStatus.RequestFailed, "Request Failed", "");
        public static ClientExceptionType INVALID_USER = new ClientExceptionType(eResponseStatus.InvalidUser, "Invalid User", "The user you selected for this action is not a valid user");
        public static ClientExceptionType USER_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.UserNotAllowed, "User Not Allowed", "The user you selected for this action is not allowed for it");
        public static ClientExceptionType DUPLICATE_PIN = new ClientExceptionType(eResponseStatus.DuplicatePin, "Duplicate Pin", "The PIN number you entered is already being used in this household");
        public static ClientExceptionType USER_ALREADY_IN_Domain = new ClientExceptionType(eResponseStatus.UserAlreadyInDomain, "User Already In Domain", "Unable to add a user to the same household twice");
        public static ClientExceptionType NOT_ALLOWED_TO_DELETE = new ClientExceptionType(eResponseStatus.NotAllowedToDelete, "Not Allowed To Delete", "Account not permitted to delete user");
        public static ClientExceptionType HOME_NETWORK_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.HomeNetworkAlreadyExists, "Home Network Already Exists", "Unable to add an home network to the same household twice");
        public static ClientExceptionType HOME_NETWORK_LIMITATION = new ClientExceptionType(eResponseStatus.HomeNetworkLimitation, "Home Network Limitation", "The home network amount in the household has been exceeded");
        public static ClientExceptionType HOME_NETWORK_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.HomeNetworkDoesNotExist, "Home Network Does Not Exist", "The home network you specified does not exis");
        public static ClientExceptionType HOME_NETWORK_FREQUENCY = new ClientExceptionType(eResponseStatus.HomeNetworkFrequency, "Home Network Frequency", "Uable to remove the home network from the household because of the frequency limitation");

        // User Section 2000 - 2999
        public static ClientExceptionType USER_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.UserDoesNotExist, "User Does Not Exist", "This user does not exist");
        public static ClientExceptionType USER_SUSPENDED = new ClientExceptionType(eResponseStatus.UserSuspended, "User Suspended", "Unable to perform this action due to a household suspension");
        public static ClientExceptionType PIN_NOT_EXISTS = new ClientExceptionType(eResponseStatus.PinNotExists, "Pin Not Exists", "The PIN provided does not exist in the system");
        public static ClientExceptionType PIN_EXPIRED = new ClientExceptionType(eResponseStatus.PinExpired, "Pin Expired", "The PIN provided has expired");
        public static ClientExceptionType NO_VALID_PIN = new ClientExceptionType(eResponseStatus.NoValidPin, "No Valid Pin", "The PIN provided is not valid");
        public static ClientExceptionType MISSING_SECURITY_PARAMETER = new ClientExceptionType(eResponseStatus.MissingSecurityParameter, "Missing Security Parameter");//??????
        public static ClientExceptionType SECRET_IS_WRONG = new ClientExceptionType(eResponseStatus.SecretIsWrong, "Secret Is Wrong", "The adapter application secret provided is incorrect. Please re-enter");
        public static ClientExceptionType LOGIN_VIA_PIN_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.LoginViaPinNotAllowed, "Login Via Pin Not Allowed", "Log in using a PIN is not enabled for accoun");
        public static ClientExceptionType PIN_NOT_IN_THE_RIGHT_LENGTH = new ClientExceptionType(eResponseStatus.PinNotInTheRightLength, "Pin Not In The Right Length", "The PIN provided is not valid.(does not match the required number of digits).");
        public static ClientExceptionType PIN_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.PinAlreadyExists, "Pin Already Exists", "The PIN that you entered already exists in the system");
        public static ClientExceptionType USER_EXISTS = new ClientExceptionType(eResponseStatus.UserExists, "User Exists", "The user you are trying to add already exists");
        public static ClientExceptionType INSIDE_LOCK_TIME = new ClientExceptionType(eResponseStatus.InsideLockTime, "Inside Lock Time"); //????
        public static ClientExceptionType USER_NOT_ACTIVATED = new ClientExceptionType(eResponseStatus.UserNotActivated, "User Not Activated", "The user must be activated to log in");
        public static ClientExceptionType USER_ALLREADY_LOGGED_IN = new ClientExceptionType(eResponseStatus.UserAllreadyLoggedIn, "User Allready Logged In", "This user is already logged in");
        public static ClientExceptionType USER_DOUBLE_LOG_IN = new ClientExceptionType(eResponseStatus.UserDoubleLogIn, "User Double Log In"); // ???
        public static ClientExceptionType DEVICE_NOT_REGISTERED = new ClientExceptionType(eResponseStatus.DeviceNotRegistered, "Device Not Registered", "The device you are trying to connect is not registered");
        //public static ClientExceptionType NOT_ACTIVATED = new ClientExceptionType(eResponseStatus.NotActivated, "Not Activated");
        public static ClientExceptionType ERROR_ON_INIT_USER = new ClientExceptionType(eResponseStatus.ErrorOnInitUser, "Error On Init User");// ???
        public static ClientExceptionType USER_NOT_MASTER_APPROVED = new ClientExceptionType(eResponseStatus.UserNotMasterApproved, "User Not Master Approved", "The user must be approved by the household master");
        public static ClientExceptionType USER_WITH_NO_Domain = new ClientExceptionType(eResponseStatus.UserWithNoDomain, "User With No Domain", "This user is not associated with any household.");
        public static ClientExceptionType USER_TYPE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.UserTypeDoesNotExist, "User Type Does Not Exist", "The specified user type does not exist");
        public static ClientExceptionType ACTIVATION_TOKEN_NOT_FOUND = new ClientExceptionType(eResponseStatus.ActivationTokenNotFound, "Activation Token Not Found", "The activation token is incorrec");
        public static ClientExceptionType USER_ALREADY_MASTER_APPROVED = new ClientExceptionType(eResponseStatus.UserAlreadyMasterApproved, "User Already Master Approved", "This user has already been approved by the household master");
        public static ClientExceptionType LOGIN_SERVER_DOWN = new ClientExceptionType(eResponseStatus.LoginServerDown, "Login Server Down", "The login server is not available. Please try again");
        public static ClientExceptionType ROLE_ALREADY_ASSIGNED_TO_USER = new ClientExceptionType(eResponseStatus.RoleAlreadyAssignedToUser, "Role Already Assigned To User", "This role has already been associated with this user");
        public static ClientExceptionType DEFAULT_USER_CANNOT_BE_DELETED = new ClientExceptionType(eResponseStatus.DefaultUserCannotBeDeleted, "Default User Cannot Be Deleted", "Unable to delete the default household user");
        public static ClientExceptionType EXCLUSIVE_MASTER_USER_CANNOT_BE_DELETED = new ClientExceptionType(eResponseStatus.ExclusiveMasterUserCannotBeDeleted, "Exclusive Master User Cannot Be Deleted");//??
        public static ClientExceptionType ITEM_NOT_FOUND = new ClientExceptionType(eResponseStatus.ItemNotFound, "Item Not Found");//??

        // CAS Section 3000 - 3999
        public static ClientExceptionType INVALID_PURCHASE = new ClientExceptionType(eResponseStatus.InvalidPurchase, "Invalid Purchase"); //??
        public static ClientExceptionType CANCELATION_WINDOW_PERIOD_EXPIRED = new ClientExceptionType(eResponseStatus.CancelationWindowPeriodExpired, "Cancelation Window Period Expired",
            "Unable to cancel the product request because the cancellation window has expired.");
        public static ClientExceptionType SUBSCRIPTION_NOT_RENEWABLE = new ClientExceptionType(eResponseStatus.SubscriptionNotRenewable, "Subscription Not Renewable", "Unable to perform this action on subscription which is not renewable");
        public static ClientExceptionType SERVICE_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.ServiceNotAllowed, "Service Not Allowed", "The user is not entitled to the premium service that he or she is trying to access");
        public static ClientExceptionType INVALID_BASE_LINK = new ClientExceptionType(eResponseStatus.InvalidBaseLink, "Invalid Base Link", "The CDN code that was provided is incorrect");
        public static ClientExceptionType CONTENT_ALREADY_CONSUMED = new ClientExceptionType(eResponseStatus.ContentAlreadyConsumed, "Content Already Consumed", "Unable to complete this request - content cannot be cancelled after being viewed.");
        public static ClientExceptionType REASON_UNKNOWN = new ClientExceptionType(eResponseStatus.ReasonUnknown, "Reason Unknown", "The request failed for an unknown reason");
        public static ClientExceptionType CHARGE_STATUS_UNKNOWN = new ClientExceptionType(eResponseStatus.ChargeStatusUnknown, "Charge Status Unknown"); //??
        public static ClientExceptionType CONTENT_IDMISSING = new ClientExceptionType(eResponseStatus.ContentIDMissing, "Content IDMissing");   //??
        public static ClientExceptionType NO_MEDIA_RELATED_TO_FILE = new ClientExceptionType(eResponseStatus.NoMediaRelatedToFile, "No Media Related To File", "There is no media for the file you requested.");
        public static ClientExceptionType NO_CONTENT_ID = new ClientExceptionType(eResponseStatus.NoContentID, "No Content ID", "Please enter the content ID and try again");
        public static ClientExceptionType NO_PRODUCT_ID = new ClientExceptionType(eResponseStatus.NoProductID, "No Product ID", "Please enter the product ID and try again");
        public static ClientExceptionType COUPON_NOT_VALID = new ClientExceptionType(eResponseStatus.CouponNotValid, "Coupon Not Valid", "The coupon you entered is not valid");
        public static ClientExceptionType UNABLE_TO_PURCHASE_PPVPURCHASED = new ClientExceptionType(eResponseStatus.UnableToPurchasePPVPurchased, "Unable To Purchase PPVPurchased", "Pay-Per-View was already purchased by this household");
        public static ClientExceptionType UNABLE_TO_PURCHASE_FREE = new ClientExceptionType(eResponseStatus.UnableToPurchaseFree, "Unable To Purchase Free", "The product you are trying to purchase is free");
        public static ClientExceptionType UNABLE_TO_PURCHASE_FOR_PURCHASE_SUBSCRIPTION_ONLY = new ClientExceptionType(eResponseStatus.UnableToPurchaseForPurchaseSubscriptionOnly, "Unable To Purchase For Purchase Subscription Only",
            "The product you are trying to purchase is restricted to subscription purchases only");
        public static ClientExceptionType UNABLE_TO_PURCHASE_SUBSCRIPTION_PURCHASED = new ClientExceptionType(eResponseStatus.UnableToPurchaseSubscriptionPurchased, "Unable To Purchase Subscription Purchased",
            "This subscription was already purchased by this household.");
        public static ClientExceptionType NOT_FOR_PURCHASE = new ClientExceptionType(eResponseStatus.NotForPurchase, "Not For Purchase", "This file is not available for purchase");
        public static ClientExceptionType FAIL = new ClientExceptionType(eResponseStatus.Fail, "Fail", ""); //??
        public static ClientExceptionType UNABLE_TO_PURCHASE_COLLECTION_PURCHASED = new ClientExceptionType(eResponseStatus.UnableToPurchaseCollectionPurchased, "Unable To Purchase Collection Purchased", "This collection has already been purchased by this household");
        public static ClientExceptionType FILE_TO_MEDIA_MISMATCH = new ClientExceptionType(eResponseStatus.FileToMediaMismatch, "File To Media Mismatch", ""); //??
        public static ClientExceptionType RECONCILIATION_FREQUENCY_LIMITATION = new ClientExceptionType(eResponseStatus.ReconciliationFrequencyLimitation, "Reconciliation Frequency Limitation", "");
        public static ClientExceptionType INVALID_CUSTOM_DATA_IDENTIFIER = new ClientExceptionType(eResponseStatus.InvalidCustomDataIdentifier, "Invalid Custom Data Identifier", "The custom data identifier you entered is invalid");
        public static ClientExceptionType INVALID_FILE_TYPE = new ClientExceptionType(eResponseStatus.InvalidFileType, "Invalid File Type", "The file type provided is invalid");
        public static ClientExceptionType NOT_ENTITLED = new ClientExceptionType(eResponseStatus.NotEntitled, "Not Entitled", "The user does not have permission to access this content");
        public static ClientExceptionType ACCOUNT_CDVR_NOT_ENABLED = new ClientExceptionType(eResponseStatus.AccountCdvrNotEnabled, "Account Cdvr Not Enabled", "Your account is not enabled for the C-DVR (recording) feature");
        public static ClientExceptionType ACCOUNT_CATCH_UP_NOT_ENABLED = new ClientExceptionType(eResponseStatus.AccountCatchUpNotEnabled, "Account Catch Up Not Enabled", "Your account is not enabled for the catch-up feature");
        public static ClientExceptionType PROGRAM_CDVR_NOT_ENABLED = new ClientExceptionType(eResponseStatus.ProgramCdvrNotEnabled, "Program Cdvr Not Enabled", "This program is not recordable");
        public static ClientExceptionType PROGRAM_CATCH_UP_NOT_ENABLED = new ClientExceptionType(eResponseStatus.ProgramCatchUpNotEnabled, "Program Catch Up Not Enabled", "This program does not support catch-up");
        public static ClientExceptionType CATCH_UP_BUFFER_LIMITATION = new ClientExceptionType(eResponseStatus.CatchUpBufferLimitation, "Catch Up Buffer Limitation", "You've reach the maximum limit for the catch up buffer");
        public static ClientExceptionType PROGRAM_NOT_IN_RECORDING_SCHEDULE_WINDOW = new ClientExceptionType(eResponseStatus.ProgramNotInRecordingScheduleWindow, "Program Not In Recording Schedule Window",
            "This program cannot be recorded because it is not in the recording schedule window");
        public static ClientExceptionType RECORDING_NOT_FOUND = new ClientExceptionType(eResponseStatus.RecordingNotFound, "Recording Not Found", "The program ID provided is invalid");
        public static ClientExceptionType RECORDING_FAILED = new ClientExceptionType(eResponseStatus.RecordingFailed, "Recording Failed", "The program recording failed");
        public static ClientExceptionType PAYMENT_METHOD_IS_USED_BY_Domain = new ClientExceptionType(eResponseStatus.PaymentMethodIsUsedByHousehold, "Payment Method Is Used By Domain", "The payment method entered is already being used by the household");
        public static ClientExceptionType EXCEEDED_QUOTA = new ClientExceptionType(eResponseStatus.ExceededQuota, "Exceeded Quota", "You've reached the maximum quote buffer for your household");
        public static ClientExceptionType RECORDING_STATUS_NOT_VALID = new ClientExceptionType(eResponseStatus.RecordingStatusNotValid, "Recording Status Not Valid", "Recording status is not valid (the only permitted status are: Recorded, Recording, Scheduled).");
        public static ClientExceptionType EXCEEDED_PROTECTION_QUOTA = new ClientExceptionType(eResponseStatus.ExceededProtectionQuota, "Exceeded Protection Quota", "You've reached the maximum quota on protected programs, and can't protect any additional programs");
        public static ClientExceptionType ACCOUNT_PROTECT_RECORD_NOT_ENABLED = new ClientExceptionType(eResponseStatus.AccountProtectRecordNotEnabled, "Account Protect Record Not Enabled", "The account recording protection feature is disabled");
        public static ClientExceptionType ACCOUNT_SERIES_RECORDING_NOT_ENABLED = new ClientExceptionType(eResponseStatus.AccountSeriesRecordingNotEnabled, "Account Series Recording Not Enabled", "The account series recording feature is disabled");
        public static ClientExceptionType ALREADY_RECORDED_AS_SERIES_OR_SEASON = new ClientExceptionType(eResponseStatus.AlreadyRecordedAsSeriesOrSeason, "Already Recorded As Series Or Season", "This program has already been recorded as part of a series/season recording");
        public static ClientExceptionType SERIES_RECORDING_NOT_FOUND = new ClientExceptionType(eResponseStatus.SeriesRecordingNotFound, "Series Recording Not Found", "Unable to find the requested series recording");
        public static ClientExceptionType EPG_ID_NOT_PART_OF_SERIES = new ClientExceptionType(eResponseStatus.EpgIdNotPartOfSeries, "Epg Id Not Part Of Series", "The EPG program is not part of the series");
        public static ClientExceptionType RECORDING_PLAYBACK_NOT_ALLOWED_FOR_NON_EXISTING_EPG_CHANNEL = new ClientExceptionType(eResponseStatus.RecordingPlaybackNotAllowedForNonExistingEpgChannel, "Recording Playback Not Allowed For Non Existing Epg Channel", "");//??
        public static ClientExceptionType RECORDING_PLAYBACK_NOT_ALLOWED_FOR_NOT_ENTITLED_EPG_CHANNEL = new ClientExceptionType(eResponseStatus.RecordingPlaybackNotAllowedForNotEntitledEpgChannel, "Recording Playback Not Allowed For Not Entitled Epg Channel", "");//??
        public static ClientExceptionType SEASON_NUMBER_NOT_MATCH = new ClientExceptionType(eResponseStatus.SeasonNumberNotMatch, "Season Number Not Match", "The season number you entered doesn't match the season number was record");


        //Catalog 4000 - 4999
        public static ClientExceptionType MEDIA_CONCURRENCY_LIMITATION = new ClientExceptionType(eResponseStatus.MediaConcurrencyLimitation, "Media Concurrency Limitation", "Media concurrency limitation (according to DLM configuration)");
        public static ClientExceptionType CONCURRENCY_LIMITATION = new ClientExceptionType(eResponseStatus.ConcurrencyLimitation, "Concurrency Limitation", "Concurrency limitation (according to DLM configuration)");
        public static ClientExceptionType BAD_SEARCH_REQUEST = new ClientExceptionType(eResponseStatus.BadSearchRequest, "Bad Search Request", ""); //??
        public static ClientExceptionType INDEX_MISSING = new ClientExceptionType(eResponseStatus.IndexMissing, "Index Missing", "");//??
        public static ClientExceptionType SYNTAX_ERROR = new ClientExceptionType(eResponseStatus.SyntaxError, "Syntax Error", "Syntax error"); //?? mabye add some details 
        public static ClientExceptionType INVALID_SEARCH_FIELD = new ClientExceptionType(eResponseStatus.InvalidSearchField, "Invalid Search Field", "");//??
        public static ClientExceptionType NO_RECOMMENDATION_ENGINE_TO_INSERT = new ClientExceptionType(eResponseStatus.NoRecommendationEngineToInsert, "No Recommendation Engine To Insert", "There's no available recommendation engine to connect");
        public static ClientExceptionType RECOMMENDATION_ENGINE_NOT_EXIST = new ClientExceptionType(eResponseStatus.RecommendationEngineNotExist, "Recommendation Engine Not Exist", "The recommendation engine specified doesn't exist");
        public static ClientExceptionType RECOMMENDATION_ENGINE_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.RecommendationEngineIdentifierRequired, "Recommendation Engine Identifier Required",
            "The mandatory recommendation engine identifier field is missing from the request");
        public static ClientExceptionType RECOMMENDATION_ENGINE_PARAMS_REQUIRED = new ClientExceptionType(eResponseStatus.RecommendationEngineParamsRequired, "Recommendation Engine Params Required", "The mandatory recommendation engine parameter fields are missing from the request");
        public static ClientExceptionType NO_EXTERNAL_CHANNEL_TO_INSERT = new ClientExceptionType(eResponseStatus.NoExternalChannelToInsert, "No External Channel To Insert", "There's no external channel to connect");
        public static ClientExceptionType EXTERNAL_CHANNEL_NOT_EXIST = new ClientExceptionType(eResponseStatus.ExternalChannelNotExist, "External Channel Not Exist", "The external channel specified doesn't exist");
        public static ClientExceptionType NO_EXTERNAL_CHANNEL_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoExternalChannelToUpdate, "No External Channel To Update", "There's no external channel to update");
        public static ClientExceptionType EXTERNAL_CHANNEL_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.ExternalChannelIdentifierRequired, "External Channel Identifier Required", "The mandatory external channel identifier field is missing from the request");
        public static ClientExceptionType EXTERNAL_CHANNEL_HAS_NO_RECOMMENDATION_ENGINE = new ClientExceptionType(eResponseStatus.ExternalChannelHasNoRecommendationEngine, "External Channel Has No Recommendation Engine", "The external channel isn't connected to a recommendation engine");
        public static ClientExceptionType NO_RECOMMENDATION_ENGINE_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoRecommendationEngineToUpdate, "No Recommendation Engine To Update", "There's no recommendation engine to update");
        public static ClientExceptionType INACTIVE_EXTERNAL_CHANNEL_ENRICHMENT = new ClientExceptionType(eResponseStatus.InactiveExternalChannelEnrichment, "Inactive External Channel Enrichment", "Inactive external channel enrichment");
        public static ClientExceptionType IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.IdentifierRequired, "Identifier Required", "Identifier is required");
        public static ClientExceptionType OBJECT_NOT_EXIST = new ClientExceptionType(eResponseStatus.ObjectNotExist, "Object Not Exist", "The object requested doesn't exist");
        public static ClientExceptionType NO_OBJECT_TO_INSERT = new ClientExceptionType(eResponseStatus.NoObjectToInsert, "No Object To Insert", "No object to insert");
        public static ClientExceptionType INVALID_MEDIA_TYPE = new ClientExceptionType(eResponseStatus.InvalidMediaType, "Invalid Media Type", "The asset type does not match one of the group asset types");
        public static ClientExceptionType INVALID_ASSET_TYPE = new ClientExceptionType(eResponseStatus.InvalidAssetType, "Invalid Asset Type", "The asset requested is not a valid asset type");
        public static ClientExceptionType PROGRAM_DOESNT_EXIST = new ClientExceptionType(eResponseStatus.ProgramDoesntExist, "Program Doesnt Exist", "The EPG program requested doesn't exist");
        public static ClientExceptionType ACTION_NOT_RECOGNIZED = new ClientExceptionType(eResponseStatus.ActionNotRecognized, "Action Not Recognized", "Action is not recognized");
        public static ClientExceptionType INVALID_ASSET_ID = new ClientExceptionType(eResponseStatus.InvalidAssetId, "Invalid Asset Id", "The specified asset ID is invalid");
        public static ClientExceptionType COUNTRY_NOT_FOUND = new ClientExceptionType(eResponseStatus.CountryNotFound, "Country Not Found", "Unable to find the country code specified");

        // Api 5000 - 5999
        public static ClientExceptionType NO_PIN_DEFINED = new ClientExceptionType(eResponseStatus.NoPinDefined, "No Pin Defined", "No parental PIN was defined for this user/household");
        public static ClientExceptionType PIN_MISMATCH = new ClientExceptionType(eResponseStatus.PinMismatch, "Pin Mismatch", "The parental PIN provided doesn't match the user/household PIN");
        public static ClientExceptionType RULE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.RuleNotExists, "Rule Not Exists", "Rule doesn't exists"); // ?? add details 
        public static ClientExceptionType NO_OSSADAPTER_TO_INSERT = new ClientExceptionType(eResponseStatus.NoOSSAdapterToInsert, "No OSSAdapter To Insert", "There's no OSS adapter to insert");
        public static ClientExceptionType NAME_REQUIRED = new ClientExceptionType(eResponseStatus.NameRequired, "Name Required", "The mandatory name field is missing from the request");
        public static ClientExceptionType SHARED_SECRET_REQUIRED = new ClientExceptionType(eResponseStatus.SharedSecretRequired, "Shared Secret Required", "The mandatory shared secret field is missing from the request");
        public static ClientExceptionType OSSADAPTER_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.OSSAdapterIdentifierRequired, "OSSAdapter Identifier Required", "The mandatory OSS adapter identifier field is missing from the request");
        public static ClientExceptionType OSSADAPTER_NOT_EXIST = new ClientExceptionType(eResponseStatus.OSSAdapterNotExist, "OSSAdapter Not Exist", "The requested OSS adapter doesn't exist");
        public static ClientExceptionType OSSADAPTER_PARAMS_REQUIRED = new ClientExceptionType(eResponseStatus.OSSAdapterParamsRequired, "OSSAdapter Params Required", "The mandatory OSS adapter parameter fields are missing from the request");
        public static ClientExceptionType UNKNOWN_OSSADAPTER_STATE = new ClientExceptionType(eResponseStatus.UnknownOSSAdapterState, "Unknown OSSAdapter State", "The status of the OSS adapter is unknown");
        public static ClientExceptionType ACTION_IS_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.ActionIsNotAllowed, "Action Is Not Allowed", "The action requested is not allowed");
        public static ClientExceptionType NO_OSSADAPTER_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoOSSAdapterToUpdate, "No OSSAdapter To Update", "There's no OSS adapter to update");
        public static ClientExceptionType ADAPTER_URL_REQUIRED = new ClientExceptionType(eResponseStatus.AdapterUrlRequired, "Adapter Url Required", "The mandatory adapter URL field is missing from the request");
        public static ClientExceptionType CONFLICTED_PARAMS = new ClientExceptionType(eResponseStatus.ConflictedParams, "Conflicted Params", "The system has detected conflicts between parameters; please check the parameters and try again");
        public static ClientExceptionType PURCHASE_SETTINGS_TYPE_INVALID = new ClientExceptionType(eResponseStatus.PurchaseSettingsTypeInvalid, "Purchase Settings Type Invalid", "The specified purchase settings type is Invalid");
        public static ClientExceptionType EXPORT_TASK_NOT_FOUND = new ClientExceptionType(eResponseStatus.ExportTaskNotFound, "Export Task Not Found", "The requested export task wasn't found");
        public static ClientExceptionType EXPORT_NOTIFICATION_URL_REQUIRED = new ClientExceptionType(eResponseStatus.ExportNotificationUrlRequired, "Export Notification Url Required", "The mandatory export notification URL field is missing from the request");
        public static ClientExceptionType EXPORT_FREQUENCY_MIN_VALUE = new ClientExceptionType(eResponseStatus.ExportFrequencyMinValue, "Export Frequency Min Value", "Export frequency Minimum value");
        public static ClientExceptionType ALIAS_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.AliasMustBeUnique, "Alias Must Be Unique", "Invalid entry: the alias value must be unique");
        public static ClientExceptionType ALIAS_REQUIRED = new ClientExceptionType(eResponseStatus.AliasRequired, "Alias Required", "The mandatory alias value field is missing from the request");
        public static ClientExceptionType USER_PARENTAL_RULE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.UserParentalRuleNotExists, "User Parental Rule Not Exists", "There is no parental rule associated with this user");
        public static ClientExceptionType TIME_SHIFTED_TV_PARTNER_SETTINGS_NOT_FOUND = new ClientExceptionType(eResponseStatus.TimeShiftedTvPartnerSettingsNotFound, "Time Shifted Tv Partner Settings Not Found", "The system did not find TimeShiftedTvPartner settings");
        public static ClientExceptionType TIME_SHIFTED_TV_PARTNER_SETTINGS_NOT_SENT = new ClientExceptionType(eResponseStatus.TimeShiftedTvPartnerSettingsNotSent, "Time Shifted Tv Partner Settings Not Sent", "The TimeShiftedTvPartner Settings were not sent");
        public static ClientExceptionType TIME_SHIFTED_TV_PARTNER_SETTINGS_NEGATIVE_BUFFER_SENT = new ClientExceptionType(eResponseStatus.TimeShiftedTvPartnerSettingsNegativeBufferSent, "Time Shifted Tv Partner Settings Negative Buffer Sent",
            "You've configured a negative buffer value in the TimeShiftedTvPartnerr settings");
        public static ClientExceptionType CDNPARTNER_SETTINGS_NOT_FOUND = new ClientExceptionType(eResponseStatus.CDNPartnerSettingsNotFound, "CDNPartner Settings Not Found", "The system didn't find CDN partner settings");

        // Billing 6000 - 6999
        public static ClientExceptionType INCORRECT_PRICE = new ClientExceptionType(eResponseStatus.IncorrectPrice, "Incorrect Price");
        public static ClientExceptionType UN_KNOWN_PPVMODULE = new ClientExceptionType(eResponseStatus.UnKnownPPVModule, "Un Known PPVModule");
        public static ClientExceptionType EXPIRED_CARD = new ClientExceptionType(eResponseStatus.ExpiredCard, "Expired Card");
        public static ClientExceptionType CELLULAR_PERMISSIONS_ERROR = new ClientExceptionType(eResponseStatus.CellularPermissionsError, "Cellular Permissions Error");
        public static ClientExceptionType UN_KNOWN_BILLING_PROVIDER = new ClientExceptionType(eResponseStatus.UnKnownBillingProvider, "Un Known Billing Provider");
        public static ClientExceptionType PAYMENT_GATEWAY_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayIdRequired, "Payment Gateway Id Required");
        public static ClientExceptionType PAYMENT_GATEWAY_PARAMS_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayParamsRequired, "Payment Gateway Params Required");
        public static ClientExceptionType PAYMENT_GATEWAY_NOT_SET_FOR_Domain = new ClientExceptionType(eResponseStatus.PaymentGatewayNotSetForHousehold, "Payment Gateway Not Set For Domain");
        public static ClientExceptionType PAYMENT_GATEWAY_NOT_EXIST = new ClientExceptionType(eResponseStatus.PaymentGatewayNotExist, "Payment Gateway Not Exist");
        public static ClientExceptionType PAYMENT_GATEWAY_CHARGE_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayChargeIdRequired, "Payment Gateway Charge Id Required");
        public static ClientExceptionType NO_CONFIGURATION_FOUND = new ClientExceptionType(eResponseStatus.NoConfigurationFound, "No Configuration Found");
        public static ClientExceptionType ADAPTER_APP_FAILURE = new ClientExceptionType(eResponseStatus.AdapterAppFailure, "Adapter App Failure");
        public static ClientExceptionType SIGNATURE_MISMATCH = new ClientExceptionType(eResponseStatus.SignatureMismatch, "Signature Mismatch");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_TRANSACTION = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayTransaction, "Error Saving Payment Gateway Transaction");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_PENDING = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayPending, "Error Saving Payment Gateway Pending");
        public static ClientExceptionType EXTERNAL_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.ExternalIdentifierRequired, "External Identifier Required");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_Domain = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayHousehold, "Error Saving Payment Gateway Domain");
        public static ClientExceptionType NO_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.NoPaymentGateway, "No Payment Gateway");
        public static ClientExceptionType PAYMENT_GATEWAY_NAME_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayNameRequired, "Payment Gateway Name Required");
        public static ClientExceptionType PAYMENT_GATEWAY_SHARED_SECRET_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewaySharedSecretRequired, "Payment Gateway Shared Secret Required");
        public static ClientExceptionType Domain_ALREADY_SET_TO_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.HouseholdAlreadySetToPaymentGateway, "Domain Already Set To Payment Gateway");
        public static ClientExceptionType CHARGE_ID_ALREADY_SET_TO_Domain_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.ChargeIdAlreadySetToHouseholdPaymentGateway, "Charge Id Already Set To Domain Payment Gateway");
        public static ClientExceptionType CHARGE_ID_NOT_SET_TO_Domain = new ClientExceptionType(eResponseStatus.ChargeIdNotSetToHousehold, "Charge Id Not Set To Domain");
        public static ClientExceptionType Domain_NOT_SET_TO_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.HouseholdNotSetToPaymentGateway, "Domain Not Set To Payment Gateway");
        public static ClientExceptionType PAYMENT_GATEWAY_SELECTION_IS_DISABLED = new ClientExceptionType(eResponseStatus.PaymentGatewaySelectionIsDisabled, "Payment Gateway Selection Is Disabled");
        public static ClientExceptionType NO_RESPONSE_FROM_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.NoResponseFromPaymentGateway, "No Response From Payment Gateway");
        public static ClientExceptionType INVALID_ACCOUNT = new ClientExceptionType(eResponseStatus.InvalidAccount, "Invalid Account");
        public static ClientExceptionType INSUFFICIENT_FUNDS = new ClientExceptionType(eResponseStatus.InsufficientFunds, "Insufficient Funds");
        public static ClientExceptionType UNKNOWN_PAYMENT_GATEWAY_RESPONSE = new ClientExceptionType(eResponseStatus.UnknownPaymentGatewayResponse, "Unknown Payment Gateway Response");
        public static ClientExceptionType PAYMENT_GATEWAY_ADAPTER_USER_KNOWN = new ClientExceptionType(eResponseStatus.PaymentGatewayAdapterUserKnown, "Payment Gateway Adapter User Known");
        public static ClientExceptionType PAYMENT_GATEWAY_ADAPTER_REASON_UNKNOWN = new ClientExceptionType(eResponseStatus.PaymentGatewayAdapterReasonUnknown, "Payment Gateway Adapter Reason Unknown");
        public static ClientExceptionType SIGNATURE_DOES_NOT_MATCH = new ClientExceptionType(eResponseStatus.SignatureDoesNotMatch, "Signature Does Not Match");
        public static ClientExceptionType ERROR_UPDATING_PENDING_TRANSACTION = new ClientExceptionType(eResponseStatus.ErrorUpdatingPendingTransaction, "Error Updating Pending Transaction");
        public static ClientExceptionType PAYMENT_GATEWAY_TRANSACTION_NOT_FOUND = new ClientExceptionType(eResponseStatus.PaymentGatewayTransactionNotFound, "Payment Gateway Transaction Not Found");
        public static ClientExceptionType PAYMENT_GATEWAY_TRANSACTION_IS_NOT_PENDING = new ClientExceptionType(eResponseStatus.PaymentGatewayTransactionIsNotPending, "Payment Gateway Transaction Is Not Pending");
        public static ClientExceptionType EXTERNAL_IDENTIFIER_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.ExternalIdentifierMustBeUnique, "External Identifier Must Be Unique");
        public static ClientExceptionType NO_PAYMENT_GATEWAY_TO_INSERT = new ClientExceptionType(eResponseStatus.NoPaymentGatewayToInsert, "No Payment Gateway To Insert");
        public static ClientExceptionType UNKNOWN_TRANSACTION_STATE = new ClientExceptionType(eResponseStatus.UnknownTransactionState, "Unknown Transaction State");
        public static ClientExceptionType PAYMENT_GATEWAY_NOT_VALID = new ClientExceptionType(eResponseStatus.PaymentGatewayNotValid, "Payment Gateway Not Valid");
        public static ClientExceptionType Domain_REQUIRED = new ClientExceptionType(eResponseStatus.HouseholdRequired, "Domain Required");
        public static ClientExceptionType PAYMENT_GATEWAY_ADAPTER_FAIL_REASON_UNKNOWN = new ClientExceptionType(eResponseStatus.PaymentGatewayAdapterFailReasonUnknown, "Payment Gateway Adapter Fail Reason Unknown");
        public static ClientExceptionType NO_PARTNER_CONFIGURATION_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoPartnerConfigurationToUpdate, "No Partner Configuration To Update");
        public static ClientExceptionType NO_CONFIGURATION_VALUE_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoConfigurationValueToUpdate, "No Configuration Value To Update");
        public static ClientExceptionType PAYMENT_METHOD_NOT_SET_FOR_Domain = new ClientExceptionType(eResponseStatus.PaymentMethodNotSetForHousehold, "Payment Method Not Set For Domain");
        public static ClientExceptionType PAYMENT_METHOD_NOT_EXIST = new ClientExceptionType(eResponseStatus.PaymentMethodNotExist, "Payment Method Not Exist");
        public static ClientExceptionType PAYMENT_METHOD_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentMethodIdRequired, "Payment Method Id Required");
        public static ClientExceptionType PAYMENT_METHOD_EXTERNAL_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentMethodExternalIdRequired, "Payment Method External Id Required");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_Domain_PAYMENT_METHOD = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayHouseholdPaymentMethod, "Error Saving Payment Gateway Domain Payment Method");
        public static ClientExceptionType PAYMENT_METHOD_ALREADY_SET_TO_Domain_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.PaymentMethodAlreadySetToHouseholdPaymentGateway, "Payment Method Already Set To Domain Payment Gateway");
        public static ClientExceptionType PAYMENT_METHOD_NAME_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentMethodNameRequired, "Payment Method Name Required");
        public static ClientExceptionType PAYMENT_GATEWAY_NOT_SUPPORT_PAYMENT_METHOD = new ClientExceptionType(eResponseStatus.PaymentGatewayNotSupportPaymentMethod, "Payment Gateway Not Support Payment Method");

        // social 7000 - 7999
        public static ClientExceptionType CONFLICT = new ClientExceptionType(eResponseStatus.Conflict, "Conflict");
        public static ClientExceptionType MIN_FRIENDS_LIMITATION = new ClientExceptionType(eResponseStatus.MinFriendsLimitation, "Min Friends Limitation");

        // notification 8000-8999
        public static ClientExceptionType NO_NOTIFICATION_SETTINGS_SENT = new ClientExceptionType(eResponseStatus.NoNotificationSettingsSent, "Internal error occurred", "Internal error occurred.");
        public static ClientExceptionType PUSH_NOTIFICATION_FALSE = new ClientExceptionType(eResponseStatus.PushNotificationFalse, "Push notification false can't combine with push system announcements true", "Push notifications are disabled.");
        public static ClientExceptionType NO_NOTIFICATION_PARTNER_SETTINGS = new ClientExceptionType(eResponseStatus.NoNotificationPartnerSettings, "No Notification Partner Settings", "Internal error occurred.");
        public static ClientExceptionType NO_NOTIFICATION_SETTINGS = new ClientExceptionType(eResponseStatus.NoNotificationSettings, "No Notification Settings", "Internal error occurred.﻿");
        public static ClientExceptionType ANNOUNCEMENT_MESSAGE_IS_EMPTY = new ClientExceptionType(eResponseStatus.AnnouncementMessageIsEmpty, "Announcement Message Is Empty", "The mandatory message field in the announcement message Is empty.");
        public static ClientExceptionType ANNOUNCEMENT_INVALID_START_TIME = new ClientExceptionType(eResponseStatus.AnnouncementInvalidStartTime, "Announcement Invalid Start Time", "The announcement start time is invalid. Please check and try again.");
        public static ClientExceptionType ANNOUNCEMENT_NOT_FOUND = new ClientExceptionType(eResponseStatus.AnnouncementNotFound, "Announcement Not Found", "The announcement requested couldn't be found.");
        public static ClientExceptionType ANNOUNCEMENT_UPDATE_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.AnnouncementUpdateNotAllowed, "Announcement Update Not Allowed", "Unable to update the announcement; the announcement was already sent.");
        public static ClientExceptionType ANNOUNCEMENT_INVALID_TIMEZONE = new ClientExceptionType(eResponseStatus.AnnouncementInvalidTimezone, "Announcement Invalid Timezone", "The announcement time zone is invalid (for example \"UTC\" or \"Pacific Standard Time\").");
        public static ClientExceptionType FEATURE_DISABLED = new ClientExceptionType(eResponseStatus.FeatureDisabled, "Feature Disabled", "Relevant feature is disabled.");
        public static ClientExceptionType ANNOUNCEMENT_MESSAGE_TOO_LONG = new ClientExceptionType(eResponseStatus.AnnouncementMessageTooLong, "Announcement Message Too Long", "The announcement message exceeds the permitted message length.");
        public static ClientExceptionType FAIL_CREATE_ANNOUNCEMENT = new ClientExceptionType(eResponseStatus.FailCreateAnnouncement, "Fail Create Announcement", "An error occurred while creating the announcement.");
        public static ClientExceptionType USER_NOT_FOLLOWING = new ClientExceptionType(eResponseStatus.UserNotFollowing, "User Not Following", "The user is not following this series.");
        public static ClientExceptionType USER_ALREADY_FOLLOWING = new ClientExceptionType(eResponseStatus.UserAlreadyFollowing, "User Already Following", "The user is already following the requested series.");
        public static ClientExceptionType MESSAGE_PLACEHOLDERS_INVALID = new ClientExceptionType(eResponseStatus.MessagePlaceholdersInvalid, "Message Placeholders Invalid", "The message placeholder is invalid.");
        public static ClientExceptionType DATETIME_FORMAT_IS_INVALID = new ClientExceptionType(eResponseStatus.DatetimeFormatIsInvalid, "Datetime Format Is Invalid", "The message date-time format Is Invalid.");
        public static ClientExceptionType MESSAGE_TEMPLATE_NOT_FOUND = new ClientExceptionType(eResponseStatus.MessageTemplateNotFound, "Message Template Not Found", "Unable to find the message template.");
        public static ClientExceptionType URLPLACEHOLDERS_INVALID = new ClientExceptionType(eResponseStatus.URLPlaceholdersInvalid, "URLPlaceholders Invalid", "The URL placeholder specified is invalid.");
        public static ClientExceptionType INVALID_MESSAGE_TTL = new ClientExceptionType(eResponseStatus.InvalidMessageTTL, "Invalid Message TTL", "Invalid message TTL");
        public static ClientExceptionType MESSAGE_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.MessageIdentifierRequired, "Message Identifier Required","The mandatory message ID field is missing in the request.");
        public static ClientExceptionType USER_INBOX_MESSAGES_NOT_EXIST = new ClientExceptionType(eResponseStatus.UserInboxMessagesNotExist, "User Inbox Messages Not Exist","Requested inbox message was not found.");

        //Pricing 9000-9999
        public static ClientExceptionType INVALID_PRICE_CODE = new ClientExceptionType(eResponseStatus.InvalidPriceCode, "Invalid Price Code", "Invalid price code: The price code entered doesn't exist for this account");
        public static ClientExceptionType INVALID_VALUE = new ClientExceptionType(eResponseStatus.InvalidValue, "Invalid Value", "Invalid value");
        public static ClientExceptionType INVALID_DISCOUNT_CODE = new ClientExceptionType(eResponseStatus.InvalidDiscountCode, "Invalid Discount Code", "Invalid discount code: The discount code entered doesn't exist for this account");
        public static ClientExceptionType INVALID_PRICE_PLAN = new ClientExceptionType(eResponseStatus.InvalidPricePlan, "Invalid Price Plan", "Invalid price plan: The price plan entered isn't in use with this account");
        public static ClientExceptionType CODE_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.CodeMustBeUnique, "Code Must Be Unique", "The code entered code must be unique");
        public static ClientExceptionType CODE_NOT_EXIST = new ClientExceptionType(eResponseStatus.CodeNotExist, "Code Not Exist", "The code entered doesn't exist");
        public static ClientExceptionType INVALID_CODE_NOT_EXIST = new ClientExceptionType(eResponseStatus.InvalidCodeNotExist, "Invalid Code Not Exist", "The code entered is invalid");
        public static ClientExceptionType INVALID_CHANNELS = new ClientExceptionType(eResponseStatus.InvalidChannels, "Invalid Channels", "Invalid channel: This channel doesn't exist in this account");
        public static ClientExceptionType INVALID_FILE_TYPES = new ClientExceptionType(eResponseStatus.InvalidFileTypes, "Invalid File Types", "Invalid file type: This file type doesn't exist in this account.");
        public static ClientExceptionType INVALID_PREVIEW_MODULE = new ClientExceptionType(eResponseStatus.InvalidPreviewModule, "Invalid Preview Module", "Invalid preview module: The preview module doesn't exist in this account");
        public static ClientExceptionType MANDATORY_FIELD = new ClientExceptionType(eResponseStatus.MandatoryField, "Mandatory Field", "Mandatory fields in a request must be completed");
        public static ClientExceptionType UNIQUE_FILED = new ClientExceptionType(eResponseStatus.UniqueFiled, "Unique Filed");
        public static ClientExceptionType INVALID_USAGE_MODULE = new ClientExceptionType(eResponseStatus.InvalidUsageModule, "Invalid Usage Module", "Invalid usage module: The usage module specified doesn't exist in this account");
        public static ClientExceptionType INVALID_COUPON_GROUP = new ClientExceptionType(eResponseStatus.InvalidCouponGroup, "Invalid Coupon Group", "Invalid coupon group: The coupon group specified doesn't exist in this account");
        public static ClientExceptionType INVALID_CURRENCY = new ClientExceptionType(eResponseStatus.InvalidCurrency, "Invalid Currency", "Invalid currency: The currency specified is not configured for this account");
        public static ClientExceptionType MODULE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.ModuleNotExists, "Module Not Exists", "PPV Module doesn't exists");

        // Adapters 10000-10999
        public static ClientExceptionType ADAPTER_NOT_EXISTS = new ClientExceptionType(eResponseStatus.AdapterNotExists, "Adapter Not Exists", "The adapter you're trying to connect doesn't exist");
        public static ClientExceptionType ADAPTER_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.AdapterIdentifierRequired, "Adapter Identifier Required", "The mandatory adapter ID field is missing from the request");
        public static ClientExceptionType ADAPTER_IS_REQUIRED = new ClientExceptionType(eResponseStatus.AdapterIsRequired, "Adapter Is Required");
        public static ClientExceptionType NO_ADAPTER_TO_INSERT = new ClientExceptionType(eResponseStatus.NoAdapterToInsert, "No Adapter To Insert");

        // Ingest 11000-11999
        public static ClientExceptionType ILLEGAL_XML = new ClientExceptionType(eResponseStatus.IllegalXml, "Illegal XML", "The XML is formatted incorrectly. Please check the file for format errors");
        public static ClientExceptionType MISSING_EXTERNAL_IDENTIFIER = new ClientExceptionType(eResponseStatus.MissingExternalIdentifier, "Missing External Identifier", "The external ID is missing");
        public static ClientExceptionType UNKNOWN_INGEST_TYPE = new ClientExceptionType(eResponseStatus.UnknownIngestType, "Unknown Ingest Type", "The Ingest type is not known");
        public static ClientExceptionType EPG_PROGRAM_DATES_ERROR = new ClientExceptionType(eResponseStatus.EPGSProgramDatesError, "EPG Program Dates Error", "An error has occurred with the EPG program dates");

        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public int Code { get; set; }

        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        new public string Message { get; set; }

        private HttpStatusCode FailureHttpCode;

        public class ExceptionType
        {
        }

        public class ClientExceptionType : ExceptionType
        {
            public eResponseStatus statusCode;

            public string description;

            public ClientExceptionType(eResponseStatus statusCode, string message)
            {
                this.statusCode = statusCode;
                this.description = string.Empty;
            }

            public ClientExceptionType(eResponseStatus statusCode, string message, string description)
            {
                this.statusCode = statusCode;
                this.description = description;
            }
        }

        public class ApiExceptionType : ExceptionType
        {
            public StatusCode? obsoleteStatusCode = null;
            public StatusCode statusCode;
            public string message;
            public string[] parameters;

            public ApiExceptionType(StatusCode statusCode, StatusCode obsoleteStatusCode, string message, params string[] parameters)
                : this(statusCode, message, parameters)
            {
                this.obsoleteStatusCode = obsoleteStatusCode;
            }

            public ApiExceptionType(StatusCode statusCode, string message, params string[] parameters)
            {
                this.statusCode = statusCode;
                this.message = message;
                this.parameters = parameters;
            }

            public string Format(params object[] values)
            {
                if (parameters.Length == 0)
                    return message;

                string ret = message;
                string token;
                string value;
                for (int i = 0; i < parameters.Length; i++)
                {
                    token = string.Format("@{0}@", parameters[i]);
                    value = values[i] != null ? values[i].ToString() : string.Empty;
                    ret = ret.Replace(token, value);
                }

                return ret;
            }
        }

        public class ExceptionPayload
        {
            public ExceptionPayload()
            {

            }
            public int code { get; set; }
            public HttpError error { get; set; }
            public HttpStatusCode failureHttpCode { get; set; }
        }

        public ApiException()
            : base(HttpStatusCode.OK)
        {
        }

        public ApiException(ClientException ex)
            : this(ex.Code, ex.ExceptionMessage)
        {
        }

        public ApiException(ClientException ex, HttpStatusCode httpStatusCode)
            : this(ex.Code, ex.ExceptionMessage, httpStatusCode)
        {
            FailureHttpCode = httpStatusCode;
        }

        public ApiException(ApiException ex, HttpStatusCode httpStatusCode)
            : this(ex.Code, ex.Message, httpStatusCode)
        {
            FailureHttpCode = httpStatusCode;
        }

        public ApiException(Exception ex, HttpStatusCode httpStatusCode)
            : this((int)eResponseStatus.Error, eResponseStatus.Error.ToString(), httpStatusCode)
        {
            FailureHttpCode = httpStatusCode;
        }

        protected ApiException(ApiException ex)
            : this(ex.Code, ex.Message)
        {
        }

        protected ApiException(ApiExceptionType type, params object[] parameters)
            : this((int)(OldStandardAttribute.isCurrentRequestOldVersion() && type.obsoleteStatusCode.HasValue ? type.obsoleteStatusCode.Value : type.statusCode), type.Format(parameters))
        {
        }

        private ApiException(int code, string message, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
            : base(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ObjectContent(typeof(ExceptionPayload), new ExceptionPayload()
                {
                    error = new HttpError(new Exception(message), true),
                    code = code,
                    failureHttpCode = httpStatusCode
                },
                new JsonMediaTypeFormatter())
            })
        {
            Code = code;
            Message = message;
        }
    }
}