using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class ApiException : HttpResponseException
    {
        // Household Section 1000 - 1999
        public static ClientExceptionType DOMAIN_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.HouseholdAlreadyExists, "Household Already Exists");
        public static ClientExceptionType EXCEEDED_LIMIT = new ClientExceptionType(eResponseStatus.ExceededLimit, "Exceeded Limit");
        public static ClientExceptionType DEVICE_TYPE_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.DeviceTypeNotAllowed, "Device Type Not Allowed");
        public static ClientExceptionType DEVICE_NOT_IN_DOMAIN = new ClientExceptionType(eResponseStatus.DeviceNotInHousehold, "Device Not In Household");
        public static ClientExceptionType MASTER_EMAIL_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.MasterEmailAlreadyExists, "Master Email Already Exists");
        public static ClientExceptionType USER_NOT_IN_DOMAIN = new ClientExceptionType(eResponseStatus.UserNotInHousehold, "User Not In Household");
        public static ClientExceptionType DOMAIN_NOT_EXISTS = new ClientExceptionType(eResponseStatus.HouseholdNotExists, "Household Not Exists");
        public static ClientExceptionType HOUSEHOLD_USER_FAILED = new ClientExceptionType(eResponseStatus.HouseholdUserFailed, "Household User Failed");
        public static ClientExceptionType DOMAIN_CREATED_WITHOUT_NPVRACCOUNT = new ClientExceptionType(eResponseStatus.HouseholdCreatedWithoutNPVRAccount, "Household Created Without NPVRAccount");
        public static ClientExceptionType DOMAIN_SUSPENDED = new ClientExceptionType(eResponseStatus.HouseholdSuspended, "Household Suspended");
        public static ClientExceptionType DLM_NOT_EXIST = new ClientExceptionType(eResponseStatus.DlmNotExist, "Dlm Not Exist");
        public static ClientExceptionType WRONG_PASSWORD_OR_USER_NAME = new ClientExceptionType(eResponseStatus.WrongPasswordOrUserName, "Wrong Password Or User Name");
        public static ClientExceptionType DOMAIN_ALREADY_SUSPENDED = new ClientExceptionType(eResponseStatus.HouseholdAlreadySuspended, "Household Already Suspended");
        public static ClientExceptionType DOMAIN_ALREADY_ACTIVE = new ClientExceptionType(eResponseStatus.HouseholdAlreadyActive, "Household Already Active");
        public static ClientExceptionType LIMITATION_PERIOD = new ClientExceptionType(eResponseStatus.LimitationPeriod, "Limitation Period");
        public static ClientExceptionType DEVICE_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.DeviceAlreadyExists, "Device Already Exists");
        public static ClientExceptionType DEVICE_EXISTS_IN_OTHER_DOMAINS = new ClientExceptionType(eResponseStatus.DeviceExistsInOtherHouseholds, "Device Exists In Other Households");
        public static ClientExceptionType NO_USERS_IN_DOMAIN = new ClientExceptionType(eResponseStatus.NoUsersInHousehold, "No Users In Household");
        public static ClientExceptionType USER_EXISTS_IN_OTHER_DOMAINS = new ClientExceptionType(eResponseStatus.UserExistsInOtherHouseholds, "User Exists In Other Households");
        public static ClientExceptionType DEVICE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.DeviceNotExists, "Device Not Exists");
        public static ClientExceptionType USER_NOT_EXISTS_IN_DOMAIN = new ClientExceptionType(eResponseStatus.UserNotExistsInHousehold, "User Not Exists In Household");
        public static ClientExceptionType ACTION_USER_NOT_MASTER = new ClientExceptionType(eResponseStatus.ActionUserNotMaster, "Action User Not Master");
        public static ClientExceptionType EXCEEDED_USER_LIMIT = new ClientExceptionType(eResponseStatus.ExceededUserLimit, "Exceeded User Limit");
        public static ClientExceptionType DOMAIN_NOT_INITIALIZED = new ClientExceptionType(eResponseStatus.HouseholdNotInitialized, "Household Not Initialized");
        public static ClientExceptionType DEVICE_NOT_CONFIRMED = new ClientExceptionType(eResponseStatus.DeviceNotConfirmed, "Device Not Confirmed");
        public static ClientExceptionType REQUEST_FAILED = new ClientExceptionType(eResponseStatus.RequestFailed, "Request Failed");
        public static ClientExceptionType INVALID_USER = new ClientExceptionType(eResponseStatus.InvalidUser, "Invalid User");
        public static ClientExceptionType USER_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.UserNotAllowed, "User Not Allowed");
        public static ClientExceptionType DUPLICATE_PIN = new ClientExceptionType(eResponseStatus.DuplicatePin, "Duplicate Pin");
        public static ClientExceptionType USER_ALREADY_IN_DOMAIN = new ClientExceptionType(eResponseStatus.UserAlreadyInHousehold, "User Already In Household");
        public static ClientExceptionType NOT_ALLOWED_TO_DELETE = new ClientExceptionType(eResponseStatus.NotAllowedToDelete, "Not Allowed To Delete");
        public static ClientExceptionType HOME_NETWORK_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.HomeNetworkAlreadyExists, "Home Network Already Exists");
        public static ClientExceptionType HOME_NETWORK_LIMITATION = new ClientExceptionType(eResponseStatus.HomeNetworkLimitation, "Home Network Limitation");
        public static ClientExceptionType HOME_NETWORK_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.HomeNetworkDoesNotExist, "Home Network Does Not Exist");
        public static ClientExceptionType HOME_NETWORK_FREQUENCY = new ClientExceptionType(eResponseStatus.HomeNetworkFrequency, "Home Network Frequency");

        // User Section 2000 - 2999
        public static ClientExceptionType USER_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.UserDoesNotExist, "User Does Not Exist");
        public static ClientExceptionType USER_SUSPENDED = new ClientExceptionType(eResponseStatus.UserSuspended, "User Suspended");
        public static ClientExceptionType GENERATE_NEW_LOGIN_PIN = new ClientExceptionType(eResponseStatus.GenerateNewLoginPIN, "Generate New Login PIN");
        public static ClientExceptionType PIN_NOT_EXISTS = new ClientExceptionType(eResponseStatus.PinNotExists, "Pin Not Exists");
        public static ClientExceptionType PIN_EXPIRED = new ClientExceptionType(eResponseStatus.PinExpired, "Pin Expired");
        public static ClientExceptionType NO_VALID_PIN = new ClientExceptionType(eResponseStatus.NoValidPin, "No Valid Pin");
        public static ClientExceptionType MISSING_SECURITY_PARAMETER = new ClientExceptionType(eResponseStatus.MissingSecurityParameter, "Missing Security Parameter");
        public static ClientExceptionType SECRET_IS_WRONG = new ClientExceptionType(eResponseStatus.SecretIsWrong, "Secret Is Wrong");
        public static ClientExceptionType LOGIN_VIA_PIN_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.LoginViaPinNotAllowed, "Login Via Pin Not Allowed");
        public static ClientExceptionType PIN_NOT_IN_THE_RIGHT_LENGTH = new ClientExceptionType(eResponseStatus.PinNotInTheRightLength, "Pin Not In The Right Length");
        public static ClientExceptionType PIN_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.PinAlreadyExists, "Pin Already Exists");
        public static ClientExceptionType USER_EXISTS = new ClientExceptionType(eResponseStatus.UserExists, "User Exists");
        public static ClientExceptionType INSIDE_LOCK_TIME = new ClientExceptionType(eResponseStatus.InsideLockTime, "Inside Lock Time");
        public static ClientExceptionType USER_NOT_ACTIVATED = new ClientExceptionType(eResponseStatus.UserNotActivated, "User Not Activated");
        public static ClientExceptionType USER_ALLREADY_LOGGED_IN = new ClientExceptionType(eResponseStatus.UserAllreadyLoggedIn, "User Allready Logged In");
        public static ClientExceptionType USER_DOUBLE_LOG_IN = new ClientExceptionType(eResponseStatus.UserDoubleLogIn, "User Double Log In");
        public static ClientExceptionType DEVICE_NOT_REGISTERED = new ClientExceptionType(eResponseStatus.DeviceNotRegistered, "Device Not Registered");
        public static ClientExceptionType NOT_ACTIVATED = new ClientExceptionType(eResponseStatus.NotActivated, "Not Activated");
        public static ClientExceptionType ERROR_ON_INIT_USER = new ClientExceptionType(eResponseStatus.ErrorOnInitUser, "Error On Init User");
        public static ClientExceptionType USER_NOT_MASTER_APPROVED = new ClientExceptionType(eResponseStatus.UserNotMasterApproved, "User Not Master Approved");
        public static ClientExceptionType USER_WITH_NO_DOMAIN = new ClientExceptionType(eResponseStatus.UserWithNoHousehold, "User With No Household");
        public static ClientExceptionType USER_TYPE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.UserTypeDoesNotExist, "User Type Does Not Exist");
        public static ClientExceptionType ACTIVATION_TOKEN_NOT_FOUND = new ClientExceptionType(eResponseStatus.ActivationTokenNotFound, "Activation Token Not Found");
        public static ClientExceptionType USER_ALREADY_MASTER_APPROVED = new ClientExceptionType(eResponseStatus.UserAlreadyMasterApproved, "User Already Master Approved");
        public static ClientExceptionType LOGIN_SERVER_DOWN = new ClientExceptionType(eResponseStatus.LoginServerDown, "Login Server Down");
        public static ClientExceptionType ROLE_ALREADY_ASSIGNED_TO_USER = new ClientExceptionType(eResponseStatus.RoleAlreadyAssignedToUser, "Role Already Assigned To User");
        public static ClientExceptionType DEFAULT_USER_CANNOT_BE_DELETED = new ClientExceptionType(eResponseStatus.DefaultUserCannotBeDeleted, "Default User Cannot Be Deleted");
        public static ClientExceptionType EXCLUSIVE_MASTER_USER_CANNOT_BE_DELETED = new ClientExceptionType(eResponseStatus.ExclusiveMasterUserCannotBeDeleted, "Exclusive Master User Cannot Be Deleted");
        public static ClientExceptionType ITEM_NOT_FOUND = new ClientExceptionType(eResponseStatus.ItemNotFound, "Item Not Found");

        // CAS Section 3000 - 3999
        public static ClientExceptionType INVALID_PURCHASE = new ClientExceptionType(eResponseStatus.InvalidPurchase, "Invalid Purchase");
        public static ClientExceptionType CANCELATION_WINDOW_PERIOD_EXPIRED = new ClientExceptionType(eResponseStatus.CancelationWindowPeriodExpired, "Cancelation Window Period Expired");
        public static ClientExceptionType SUBSCRIPTION_NOT_RENEWABLE = new ClientExceptionType(eResponseStatus.SubscriptionNotRenewable, "Subscription Not Renewable");
        public static ClientExceptionType SERVICE_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.ServiceNotAllowed, "Service Not Allowed");
        public static ClientExceptionType INVALID_BASE_LINK = new ClientExceptionType(eResponseStatus.InvalidBaseLink, "Invalid Base Link");
        public static ClientExceptionType CONTENT_ALREADY_CONSUMED = new ClientExceptionType(eResponseStatus.ContentAlreadyConsumed, "Content Already Consumed");
        public static ClientExceptionType REASON_UNKNOWN = new ClientExceptionType(eResponseStatus.ReasonUnknown, "Reason Unknown");
        public static ClientExceptionType CHARGE_STATUS_UNKNOWN = new ClientExceptionType(eResponseStatus.ChargeStatusUnknown, "Charge Status Unknown");
        public static ClientExceptionType CONTENT_IDMISSING = new ClientExceptionType(eResponseStatus.ContentIDMissing, "Content IDMissing");   ///
        public static ClientExceptionType NO_MEDIA_RELATED_TO_FILE = new ClientExceptionType(eResponseStatus.NoMediaRelatedToFile, "No Media Related To File");
        public static ClientExceptionType NO_CONTENT_ID = new ClientExceptionType(eResponseStatus.NoContentID, "No Content ID"); /// 
        public static ClientExceptionType NO_PRODUCT_ID = new ClientExceptionType(eResponseStatus.NoProductID, "No Product ID");
        public static ClientExceptionType COUPON_NOT_VALID = new ClientExceptionType(eResponseStatus.CouponNotValid, "Coupon Not Valid");
        public static ClientExceptionType UNABLE_TO_PURCHASE_PPVPURCHASED = new ClientExceptionType(eResponseStatus.UnableToPurchasePPVPurchased, "Unable To Purchase PPVPurchased");
        public static ClientExceptionType UNABLE_TO_PURCHASE_FREE = new ClientExceptionType(eResponseStatus.UnableToPurchaseFree, "Unable To Purchase Free");
        public static ClientExceptionType UNABLE_TO_PURCHASE_FOR_PURCHASE_SUBSCRIPTION_ONLY = new ClientExceptionType(eResponseStatus.UnableToPurchaseForPurchaseSubscriptionOnly, "Unable To Purchase For Purchase Subscription Only");
        public static ClientExceptionType UNABLE_TO_PURCHASE_SUBSCRIPTION_PURCHASED = new ClientExceptionType(eResponseStatus.UnableToPurchaseSubscriptionPurchased, "Unable To Purchase Subscription Purchased");
        public static ClientExceptionType NOT_FOR_PURCHASE = new ClientExceptionType(eResponseStatus.NotForPurchase, "Not For Purchase");
        public static ClientExceptionType FAIL = new ClientExceptionType(eResponseStatus.Fail, "Fail");
        public static ClientExceptionType UNABLE_TO_PURCHASE_COLLECTION_PURCHASED = new ClientExceptionType(eResponseStatus.UnableToPurchaseCollectionPurchased, "Unable To Purchase Collection Purchased");
        public static ClientExceptionType FILE_TO_MEDIA_MISMATCH = new ClientExceptionType(eResponseStatus.FileToMediaMismatch, "File To Media Mismatch");
        public static ClientExceptionType RECONCILIATION_FREQUENCY_LIMITATION = new ClientExceptionType(eResponseStatus.ReconciliationFrequencyLimitation, "Reconciliation Frequency Limitation");
        public static ClientExceptionType INVALID_CUSTOM_DATA_IDENTIFIER = new ClientExceptionType(eResponseStatus.InvalidCustomDataIdentifier, "Invalid Custom Data Identifier");
        public static ClientExceptionType INVALID_FILE_TYPE = new ClientExceptionType(eResponseStatus.InvalidFileType, "Invalid File Type");
        public static ClientExceptionType NOT_ENTITLED = new ClientExceptionType(eResponseStatus.NotEntitled, "Not Entitled");
        public static ClientExceptionType ACCOUNT_CDVR_NOT_ENABLED = new ClientExceptionType(eResponseStatus.AccountCdvrNotEnabled, "Account Cdvr Not Enabled");
        public static ClientExceptionType ACCOUNT_CATCH_UP_NOT_ENABLED = new ClientExceptionType(eResponseStatus.AccountCatchUpNotEnabled, "Account Catch Up Not Enabled");
        public static ClientExceptionType PROGRAM_CDVR_NOT_ENABLED = new ClientExceptionType(eResponseStatus.ProgramCdvrNotEnabled, "Program Cdvr Not Enabled");
        public static ClientExceptionType PROGRAM_CATCH_UP_NOT_ENABLED = new ClientExceptionType(eResponseStatus.ProgramCatchUpNotEnabled, "Program Catch Up Not Enabled");
        public static ClientExceptionType CATCH_UP_BUFFER_LIMITATION = new ClientExceptionType(eResponseStatus.CatchUpBufferLimitation, "Catch Up Buffer Limitation");
        public static ClientExceptionType PROGRAM_NOT_IN_RECORDING_SCHEDULE_WINDOW = new ClientExceptionType(eResponseStatus.ProgramNotInRecordingScheduleWindow, "Program Not In Recording Schedule Window");
        public static ClientExceptionType RECORDING_NOT_FOUND = new ClientExceptionType(eResponseStatus.RecordingNotFound, "Recording Not Found");
        public static ClientExceptionType RECORDING_FAILED = new ClientExceptionType(eResponseStatus.RecordingFailed, "Recording Failed");
        public static ClientExceptionType PAYMENT_METHOD_IS_USED_BY_HOUSEHOLD = new ClientExceptionType(eResponseStatus.PaymentMethodIsUsedByHousehold, "Payment Method Is Used By Household");
        public static ClientExceptionType EXCEEDED_QUOTA = new ClientExceptionType(eResponseStatus.ExceededQuota, "Exceeded Quota");
        public static ClientExceptionType RECORDING_STATUS_NOT_VALID = new ClientExceptionType(eResponseStatus.RecordingStatusNotValid, "Recording Status Not Valid");
        public static ClientExceptionType EXCEEDED_PROTECTION_QUOTA = new ClientExceptionType(eResponseStatus.ExceededProtectionQuota, "Exceeded Protection Quota");
        public static ClientExceptionType ACCOUNT_PROTECT_RECORD_NOT_ENABLED = new ClientExceptionType(eResponseStatus.AccountProtectRecordNotEnabled, "Account Protect Record Not Enabled");
        public static ClientExceptionType ACCOUNT_SERIES_RECORDING_NOT_ENABLED = new ClientExceptionType(eResponseStatus.AccountSeriesRecordingNotEnabled, "Account Series Recording Not Enabled");
        public static ClientExceptionType ALREADY_RECORDED_AS_SERIES_OR_SEASON = new ClientExceptionType(eResponseStatus.AlreadyRecordedAsSeriesOrSeason, "Already Recorded As Series Or Season");
        public static ClientExceptionType SERIES_RECORDING_NOT_FOUND = new ClientExceptionType(eResponseStatus.SeriesRecordingNotFound, "Series Recording Not Found");
        public static ClientExceptionType EPG_ID_NOT_PART_OF_SERIES = new ClientExceptionType(eResponseStatus.EpgIdNotPartOfSeries, "Epg Id Not Part Of Series");
        public static ClientExceptionType RECORDING_PLAYBACK_NOT_ALLOWED_FOR_NON_EXISTING_EPG_CHANNEL = new ClientExceptionType(eResponseStatus.RecordingPlaybackNotAllowedForNonExistingEpgChannel, "Recording Playback Not Allowed For Non Existing Epg Channel");
        public static ClientExceptionType RECORDING_PLAYBACK_NOT_ALLOWED_FOR_NOT_ENTITLED_EPG_CHANNEL = new ClientExceptionType(eResponseStatus.RecordingPlaybackNotAllowedForNotEntitledEpgChannel, "Recording Playback Not Allowed For Not Entitled Epg Channel");
        public static ClientExceptionType SEASON_NUMBER_NOT_MATCH = new ClientExceptionType(eResponseStatus.SeasonNumberNotMatch, "Season Number Not Match");


        //Catalog 4000 - 4999
        public static ClientExceptionType MEDIA_CONCURRENCY_LIMITATION = new ClientExceptionType(eResponseStatus.MediaConcurrencyLimitation, "Media Concurrency Limitation");
        public static ClientExceptionType CONCURRENCY_LIMITATION = new ClientExceptionType(eResponseStatus.ConcurrencyLimitation, "Concurrency Limitation");
        public static ClientExceptionType BAD_SEARCH_REQUEST = new ClientExceptionType(eResponseStatus.BadSearchRequest, "Bad Search Request");
        public static ClientExceptionType INDEX_MISSING = new ClientExceptionType(eResponseStatus.IndexMissing, "Index Missing");
        public static ClientExceptionType SYNTAX_ERROR = new ClientExceptionType(eResponseStatus.SyntaxError, "Syntax Error");
        public static ClientExceptionType INVALID_SEARCH_FIELD = new ClientExceptionType(eResponseStatus.InvalidSearchField, "Invalid Search Field");
        public static ClientExceptionType NO_RECOMMENDATION_ENGINE_TO_INSERT = new ClientExceptionType(eResponseStatus.NoRecommendationEngineToInsert, "No Recommendation Engine To Insert");
        public static ClientExceptionType RECOMMENDATION_ENGINE_NOT_EXIST = new ClientExceptionType(eResponseStatus.RecommendationEngineNotExist, "Recommendation Engine Not Exist");
        public static ClientExceptionType RECOMMENDATION_ENGINE_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.RecommendationEngineIdentifierRequired, "Recommendation Engine Identifier Required");
        public static ClientExceptionType RECOMMENDATION_ENGINE_PARAMS_REQUIRED = new ClientExceptionType(eResponseStatus.RecommendationEngineParamsRequired, "Recommendation Engine Params Required");
        public static ClientExceptionType NO_EXTERNAL_CHANNEL_TO_INSERT = new ClientExceptionType(eResponseStatus.NoExternalChannelToInsert, "No External Channel To Insert");
        public static ClientExceptionType EXTERNAL_CHANNEL_NOT_EXIST = new ClientExceptionType(eResponseStatus.ExternalChannelNotExist, "External Channel Not Exist");
        public static ClientExceptionType NO_EXTERNAL_CHANNEL_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoExternalChannelToUpdate, "No External Channel To Update");
        public static ClientExceptionType EXTERNAL_CHANNEL_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.ExternalChannelIdentifierRequired, "External Channel Identifier Required");
        public static ClientExceptionType EXTERNAL_CHANNEL_HAS_NO_RECOMMENDATION_ENGINE = new ClientExceptionType(eResponseStatus.ExternalChannelHasNoRecommendationEngine, "External Channel Has No Recommendation Engine");
        public static ClientExceptionType NO_RECOMMENDATION_ENGINE_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoRecommendationEngineToUpdate, "No Recommendation Engine To Update");
        public static ClientExceptionType INACTIVE_EXTERNAL_CHANNEL_ENRICHMENT = new ClientExceptionType(eResponseStatus.InactiveExternalChannelEnrichment, "Inactive External Channel Enrichment");
        public static ClientExceptionType IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.IdentifierRequired, "Identifier Required");
        public static ClientExceptionType OBJECT_NOT_EXIST = new ClientExceptionType(eResponseStatus.ObjectNotExist, "Object Not Exist");
        public static ClientExceptionType NO_OBJECT_TO_INSERT = new ClientExceptionType(eResponseStatus.NoObjectToInsert, "No Object To Insert");
        public static ClientExceptionType INVALID_MEDIA_TYPE = new ClientExceptionType(eResponseStatus.InvalidMediaType, "Invalid Media Type");
        public static ClientExceptionType INVALID_ASSET_TYPE = new ClientExceptionType(eResponseStatus.InvalidAssetType, "Invalid Asset Type");
        public static ClientExceptionType PROGRAM_DOESNT_EXIST = new ClientExceptionType(eResponseStatus.ProgramDoesntExist, "Program Doesnt Exist");
        public static ClientExceptionType ACTION_NOT_RECOGNIZED = new ClientExceptionType(eResponseStatus.ActionNotRecognized, "Action Not Recognized");
        public static ClientExceptionType INVALID_ASSET_ID = new ClientExceptionType(eResponseStatus.InvalidAssetId, "Invalid Asset Id");
        public static ClientExceptionType COUNTRY_NOT_FOUND = new ClientExceptionType(eResponseStatus.CountryNotFound, "Country Not Found");

        // Api 5000 - 5999
        public static ClientExceptionType NO_PIN_DEFINED = new ClientExceptionType(eResponseStatus.NoPinDefined, "No Pin Defined");
        public static ClientExceptionType PIN_MISMATCH = new ClientExceptionType(eResponseStatus.PinMismatch, "Pin Mismatch");
        public static ClientExceptionType RULE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.RuleNotExists, "Rule Not Exists");
        public static ClientExceptionType NO_OSSADAPTER_TO_INSERT = new ClientExceptionType(eResponseStatus.NoOSSAdapterToInsert, "No OSSAdapter To Insert");
        public static ClientExceptionType NAME_REQUIRED = new ClientExceptionType(eResponseStatus.NameRequired, "Name Required");
        public static ClientExceptionType SHARED_SECRET_REQUIRED = new ClientExceptionType(eResponseStatus.SharedSecretRequired, "Shared Secret Required");
        public static ClientExceptionType OSSADAPTER_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.OSSAdapterIdentifierRequired, "OSSAdapter Identifier Required");
        public static ClientExceptionType OSSADAPTER_NOT_EXIST = new ClientExceptionType(eResponseStatus.OSSAdapterNotExist, "OSSAdapter Not Exist");
        public static ClientExceptionType OSSADAPTER_PARAMS_REQUIRED = new ClientExceptionType(eResponseStatus.OSSAdapterParamsRequired, "OSSAdapter Params Required");
        public static ClientExceptionType UNKNOWN_OSSADAPTER_STATE = new ClientExceptionType(eResponseStatus.UnknownOSSAdapterState, "Unknown OSSAdapter State");
        public static ClientExceptionType ACTION_IS_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.ActionIsNotAllowed, "Action Is Not Allowed");
        public static ClientExceptionType NO_OSSADAPTER_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoOSSAdapterToUpdate, "No OSSAdapter To Update");
        public static ClientExceptionType ADAPTER_URL_REQUIRED = new ClientExceptionType(eResponseStatus.AdapterUrlRequired, "Adapter Url Required");
        public static ClientExceptionType CONFLICTED_PARAMS = new ClientExceptionType(eResponseStatus.ConflictedParams, "Conflicted Params");
        public static ClientExceptionType PURCHASE_SETTINGS_TYPE_INVALID = new ClientExceptionType(eResponseStatus.PurchaseSettingsTypeInvalid, "Purchase Settings Type Invalid");
        public static ClientExceptionType EXPORT_TASK_NOT_FOUND = new ClientExceptionType(eResponseStatus.ExportTaskNotFound, "Export Task Not Found");
        public static ClientExceptionType EXPORT_NOTIFICATION_URL_REQUIRED = new ClientExceptionType(eResponseStatus.ExportNotificationUrlRequired, "Export Notification Url Required");
        public static ClientExceptionType EXPORT_FREQUENCY_MIN_VALUE = new ClientExceptionType(eResponseStatus.ExportFrequencyMinValue, "Export Frequency Min Value");
        public static ClientExceptionType ALIAS_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.AliasMustBeUnique, "Alias Must Be Unique");
        public static ClientExceptionType ALIAS_REQUIRED = new ClientExceptionType(eResponseStatus.AliasRequired, "Alias Required");
        public static ClientExceptionType USER_PARENTAL_RULE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.UserParentalRuleNotExists, "User Parental Rule Not Exists");
        public static ClientExceptionType TIME_SHIFTED_TV_PARTNER_SETTINGS_NOT_FOUND = new ClientExceptionType(eResponseStatus.TimeShiftedTvPartnerSettingsNotFound, "Time Shifted Tv Partner Settings Not Found");
        public static ClientExceptionType TIME_SHIFTED_TV_PARTNER_SETTINGS_NOT_SENT = new ClientExceptionType(eResponseStatus.TimeShiftedTvPartnerSettingsNotSent, "Time Shifted Tv Partner Settings Not Sent");
        public static ClientExceptionType TIME_SHIFTED_TV_PARTNER_SETTINGS_NEGATIVE_BUFFER_SENT = new ClientExceptionType(eResponseStatus.TimeShiftedTvPartnerSettingsNegativeBufferSent, "Time Shifted Tv Partner Settings Negative Buffer Sent");
        public static ClientExceptionType CDNPARTNER_SETTINGS_NOT_FOUND = new ClientExceptionType(eResponseStatus.CDNPartnerSettingsNotFound, "CDNPartner Settings Not Found");

        // Billing 6000 - 6999
        public static ClientExceptionType INCORRECT_PRICE = new ClientExceptionType(eResponseStatus.IncorrectPrice, "Incorrect Price");
        public static ClientExceptionType UN_KNOWN_PPVMODULE = new ClientExceptionType(eResponseStatus.UnKnownPPVModule, "Un Known PPVModule");
        public static ClientExceptionType EXPIRED_CARD = new ClientExceptionType(eResponseStatus.ExpiredCard, "Expired Card");
        public static ClientExceptionType CELLULAR_PERMISSIONS_ERROR = new ClientExceptionType(eResponseStatus.CellularPermissionsError, "Cellular Permissions Error");
        public static ClientExceptionType UN_KNOWN_BILLING_PROVIDER = new ClientExceptionType(eResponseStatus.UnKnownBillingProvider, "Un Known Billing Provider");
        public static ClientExceptionType PAYMENT_GATEWAY_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayIdRequired, "Payment Gateway Id Required");
        public static ClientExceptionType PAYMENT_GATEWAY_PARAMS_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayParamsRequired, "Payment Gateway Params Required");
        public static ClientExceptionType PAYMENT_GATEWAY_NOT_SET_FOR_HOUSEHOLD = new ClientExceptionType(eResponseStatus.PaymentGatewayNotSetForHousehold, "Payment Gateway Not Set For Household");
        public static ClientExceptionType PAYMENT_GATEWAY_NOT_EXIST = new ClientExceptionType(eResponseStatus.PaymentGatewayNotExist, "Payment Gateway Not Exist");
        public static ClientExceptionType PAYMENT_GATEWAY_CHARGE_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayChargeIdRequired, "Payment Gateway Charge Id Required");
        public static ClientExceptionType NO_CONFIGURATION_FOUND = new ClientExceptionType(eResponseStatus.NoConfigurationFound, "No Configuration Found");
        public static ClientExceptionType ADAPTER_APP_FAILURE = new ClientExceptionType(eResponseStatus.AdapterAppFailure, "Adapter App Failure");
        public static ClientExceptionType SIGNATURE_MISMATCH = new ClientExceptionType(eResponseStatus.SignatureMismatch, "Signature Mismatch");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_TRANSACTION = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayTransaction, "Error Saving Payment Gateway Transaction");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_PENDING = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayPending, "Error Saving Payment Gateway Pending");
        public static ClientExceptionType EXTERNAL_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.ExternalIdentifierRequired, "External Identifier Required");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_HOUSEHOLD = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayHousehold, "Error Saving Payment Gateway Household");
        public static ClientExceptionType NO_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.NoPaymentGateway, "No Payment Gateway");
        public static ClientExceptionType PAYMENT_GATEWAY_NAME_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayNameRequired, "Payment Gateway Name Required");
        public static ClientExceptionType PAYMENT_GATEWAY_SHARED_SECRET_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewaySharedSecretRequired, "Payment Gateway Shared Secret Required");
        public static ClientExceptionType HOUSEHOLD_ALREADY_SET_TO_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.HouseholdAlreadySetToPaymentGateway, "Household Already Set To Payment Gateway");
        public static ClientExceptionType CHARGE_ID_ALREADY_SET_TO_HOUSEHOLD_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.ChargeIdAlreadySetToHouseholdPaymentGateway, "Charge Id Already Set To Household Payment Gateway");
        public static ClientExceptionType CHARGE_ID_NOT_SET_TO_HOUSEHOLD = new ClientExceptionType(eResponseStatus.ChargeIdNotSetToHousehold, "Charge Id Not Set To Household");
        public static ClientExceptionType HOUSEHOLD_NOT_SET_TO_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.HouseholdNotSetToPaymentGateway, "Household Not Set To Payment Gateway");
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
        public static ClientExceptionType HOUSEHOLD_REQUIRED = new ClientExceptionType(eResponseStatus.HouseholdRequired, "Household Required");
        public static ClientExceptionType PAYMENT_GATEWAY_ADAPTER_FAIL_REASON_UNKNOWN = new ClientExceptionType(eResponseStatus.PaymentGatewayAdapterFailReasonUnknown, "Payment Gateway Adapter Fail Reason Unknown");
        public static ClientExceptionType NO_PARTNER_CONFIGURATION_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoPartnerConfigurationToUpdate, "No Partner Configuration To Update");
        public static ClientExceptionType NO_CONFIGURATION_VALUE_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoConfigurationValueToUpdate, "No Configuration Value To Update");
        public static ClientExceptionType PAYMENT_METHOD_NOT_SET_FOR_HOUSEHOLD = new ClientExceptionType(eResponseStatus.PaymentMethodNotSetForHousehold, "Payment Method Not Set For Household");
        public static ClientExceptionType PAYMENT_METHOD_NOT_EXIST = new ClientExceptionType(eResponseStatus.PaymentMethodNotExist, "Payment Method Not Exist");
        public static ClientExceptionType PAYMENT_METHOD_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentMethodIdRequired, "Payment Method Id Required");
        public static ClientExceptionType PAYMENT_METHOD_EXTERNAL_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentMethodExternalIdRequired, "Payment Method External Id Required");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_HOUSEHOLD_PAYMENT_METHOD = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayHouseholdPaymentMethod, "Error Saving Payment Gateway Household Payment Method");
        public static ClientExceptionType PAYMENT_METHOD_ALREADY_SET_TO_HOUSEHOLD_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.PaymentMethodAlreadySetToHouseholdPaymentGateway, "Payment Method Already Set To Household Payment Gateway");
        public static ClientExceptionType PAYMENT_METHOD_NAME_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentMethodNameRequired, "Payment Method Name Required");
        public static ClientExceptionType PAYMENT_GATEWAY_NOT_SUPPORT_PAYMENT_METHOD = new ClientExceptionType(eResponseStatus.PaymentGatewayNotSupportPaymentMethod, "Payment Gateway Not Support Payment Method");

        // social 7000 - 7999
        public static ClientExceptionType CONFLICT = new ClientExceptionType(eResponseStatus.Conflict, "Conflict");
        public static ClientExceptionType MIN_FRIENDS_LIMITATION = new ClientExceptionType(eResponseStatus.MinFriendsLimitation, "Min Friends Limitation");

        // notification 8000-8999
        public static ClientExceptionType NO_NOTIFICATION_SETTINGS_SENT = new ClientExceptionType(eResponseStatus.NoNotificationSettingsSent, "No Notification Settings Sent");
        public static ClientExceptionType PUSH_NOTIFICATION_FALSE = new ClientExceptionType(eResponseStatus.PushNotificationFalse, "Push notification false can't combine with push system announcements true");
        public static ClientExceptionType NO_NOTIFICATION_PARTNER_SETTINGS = new ClientExceptionType(eResponseStatus.NoNotificationPartnerSettings, "No Notification Partner Settings");
        public static ClientExceptionType NO_NOTIFICATION_SETTINGS = new ClientExceptionType(eResponseStatus.NoNotificationSettings, "No Notification Settings");
        public static ClientExceptionType ANNOUNCEMENT_MESSAGE_IS_EMPTY = new ClientExceptionType(eResponseStatus.AnnouncementMessageIsEmpty, "Announcement Message Is Empty");
        public static ClientExceptionType ANNOUNCEMENT_INVALID_START_TIME = new ClientExceptionType(eResponseStatus.AnnouncementInvalidStartTime, "Announcement Invalid Start Time");
        public static ClientExceptionType ANNOUNCEMENT_NOT_FOUND = new ClientExceptionType(eResponseStatus.AnnouncementNotFound, "Announcement Not Found");
        public static ClientExceptionType ANNOUNCEMENT_UPDATE_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.AnnouncementUpdateNotAllowed, "Announcement Update Not Allowed");
        public static ClientExceptionType ANNOUNCEMENT_INVALID_TIMEZONE = new ClientExceptionType(eResponseStatus.AnnouncementInvalidTimezone, "Announcement Invalid Timezone");
        public static ClientExceptionType FEATURE_DISABLED = new ClientExceptionType(eResponseStatus.FeatureDisabled, "Feature Disabled");
        public static ClientExceptionType ANNOUNCEMENT_MESSAGE_TOO_LONG = new ClientExceptionType(eResponseStatus.AnnouncementMessageTooLong, "Announcement Message Too Long");
        public static ClientExceptionType FAIL_CREATE_ANNOUNCEMENT = new ClientExceptionType(eResponseStatus.FailCreateAnnouncement, "Fail Create Announcement");
        public static ClientExceptionType USER_NOT_FOLLOWING = new ClientExceptionType(eResponseStatus.UserNotFollowing, "User Not Following");
        public static ClientExceptionType USER_ALREADY_FOLLOWING = new ClientExceptionType(eResponseStatus.UserAlreadyFollowing, "User Already Following");
        public static ClientExceptionType MESSAGE_PLACEHOLDERS_INVALID = new ClientExceptionType(eResponseStatus.MessagePlaceholdersInvalid, "Message Placeholders Invalid");
        public static ClientExceptionType DATETIME_FORMAT_IS_INVALID = new ClientExceptionType(eResponseStatus.DatetimeFormatIsInvalid, "Datetime Format Is Invalid");
        public static ClientExceptionType MESSAGE_TEMPLATE_NOT_FOUND = new ClientExceptionType(eResponseStatus.MessageTemplateNotFound, "Message Template Not Found");
        public static ClientExceptionType URLPLACEHOLDERS_INVALID = new ClientExceptionType(eResponseStatus.URLPlaceholdersInvalid, "URLPlaceholders Invalid");
        public static ClientExceptionType INVALID_MESSAGE_TTL = new ClientExceptionType(eResponseStatus.InvalidMessageTTL, "Invalid Message TTL");
        public static ClientExceptionType MESSAGE_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.MessageIdentifierRequired, "Message Identifier Required");
        public static ClientExceptionType USER_INBOX_MESSAGES_NOT_EXIST = new ClientExceptionType(eResponseStatus.UserInboxMessagesNotExist, "User Inbox Messages Not Exist");

        //Pricing 9000-9999
        public static ClientExceptionType INVALID_PRICE_CODE = new ClientExceptionType(eResponseStatus.InvalidPriceCode, "Invalid Price Code");
        public static ClientExceptionType INVALID_VALUE = new ClientExceptionType(eResponseStatus.InvalidValue, "Invalid Value");
        public static ClientExceptionType INVALID_DISCOUNT_CODE = new ClientExceptionType(eResponseStatus.InvalidDiscountCode, "Invalid Discount Code");
        public static ClientExceptionType INVALID_PRICE_PLAN = new ClientExceptionType(eResponseStatus.InvalidPricePlan, "Invalid Price Plan");
        public static ClientExceptionType CODE_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.CodeMustBeUnique, "Code Must Be Unique");
        public static ClientExceptionType CODE_NOT_EXIST = new ClientExceptionType(eResponseStatus.CodeNotExist, "Code Not Exist");
        public static ClientExceptionType INVALID_CODE_NOT_EXIST = new ClientExceptionType(eResponseStatus.InvalidCodeNotExist, "Invalid Code Not Exist");
        public static ClientExceptionType INVALID_CHANNELS = new ClientExceptionType(eResponseStatus.InvalidChannels, "Invalid Channels");
        public static ClientExceptionType INVALID_FILE_TYPES = new ClientExceptionType(eResponseStatus.InvalidFileTypes, "Invalid File Types");
        public static ClientExceptionType INVALID_PREVIEW_MODULE = new ClientExceptionType(eResponseStatus.InvalidPreviewModule, "Invalid Preview Module");
        public static ClientExceptionType MANDATORY_FIELD = new ClientExceptionType(eResponseStatus.MandatoryField, "Mandatory Field");
        public static ClientExceptionType UNIQUE_FILED = new ClientExceptionType(eResponseStatus.UniqueFiled, "Unique Filed");
        public static ClientExceptionType INVALID_USAGE_MODULE = new ClientExceptionType(eResponseStatus.InvalidUsageModule, "Invalid Usage Module");
        public static ClientExceptionType INVALID_COUPON_GROUP = new ClientExceptionType(eResponseStatus.InvalidCouponGroup, "Invalid Coupon Group");
        public static ClientExceptionType INVALID_CURRENCY = new ClientExceptionType(eResponseStatus.InvalidCurrency, "Invalid Currency");
        public static ClientExceptionType MODULE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.ModuleNotExists, "Module Not Exists");

        // Adapters 10000-10999
        public static ClientExceptionType ADAPTER_NOT_EXISTS = new ClientExceptionType(eResponseStatus.AdapterNotExists, "Adapter Not Exists");
        public static ClientExceptionType ADAPTER_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.AdapterIdentifierRequired, "Adapter Identifier Required");
        public static ClientExceptionType ADAPTER_IS_REQUIRED = new ClientExceptionType(eResponseStatus.AdapterIsRequired, "Adapter Is Required");
        public static ClientExceptionType NO_ADAPTER_TO_INSERT = new ClientExceptionType(eResponseStatus.NoAdapterToInsert, "No Adapter To Insert");

        // Ingest 11000-11999
        public static ClientExceptionType ILLEGAL_XML = new ClientExceptionType(eResponseStatus.IllegalXml, "Illegal XML");
        public static ClientExceptionType MISSING_EXTERNAL_IDENTIFIER = new ClientExceptionType(eResponseStatus.MissingExternalIdentifier, "Missing External Identifier");
        public static ClientExceptionType UNKNOWN_INGEST_TYPE = new ClientExceptionType(eResponseStatus.UnknownIngestType, "Unknown Ingest Type");
        public static ClientExceptionType EPG_PROGRAM_DATES_ERROR = new ClientExceptionType(eResponseStatus.EPGSProgramDatesError, "EPG Program Dates Error");
        
        public int Code { get; set; }
        new public string Message { get; set; }

        public class ExceptionType
        {
        }

        public class ClientExceptionType : ExceptionType
        {
            public eResponseStatus statusCode;

            public ClientExceptionType(eResponseStatus statusCode, string message)
            {
                this.statusCode = statusCode;
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
                    value = values[i].ToString();
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
        }

        public ApiException(ClientException ex)
            : this(ex.Code, ex.ExceptionMessage)
        {
        }

        protected ApiException(ApiException ex)
            : this(ex.Code, ex.Message)
        {
        }

        protected ApiException(ApiExceptionType type, params object[] parameters)
            : this((int)(OldStandardAttribute.isCurrentRequestOldVersion() && type.obsoleteStatusCode.HasValue ? type.obsoleteStatusCode.Value : type.statusCode), type.Format(parameters))
        {
        }

        private ApiException(int code, string message)
            : base(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ObjectContent(typeof(ExceptionPayload), new ExceptionPayload()
                {
                    error = new HttpError(new Exception(message), true),
                    code = code
                },
                new JsonMediaTypeFormatter())
            })
        {
            Code = code;
            Message = message;
        }
    }
}