
using ApiObjects.Response;
using System.Web.Http;
using WebAPI.Managers.Models;

namespace WebAPI.Exceptions
{
    public partial class ApiException : HttpResponseException
    {
        #region Domain Section 1000 - 1999

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
        public static ClientExceptionType LIMITATION_PERIOD = new ClientExceptionType(eResponseStatus.LimitationPeriod, "Limitation Period", "Unable to remove the device or user from the household at this time because of the limitation period");
        public static ClientExceptionType DEVICE_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.DeviceAlreadyExists, "Device Already Exists", "The device you are trying to add already exists in the system");
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
        public static ClientExceptionType REQUEST_FAILED = new ClientExceptionType(eResponseStatus.RequestFailed, "Request Failed", "The request you entered could not be completed at this time");
        public static ClientExceptionType INVALID_USER = new ClientExceptionType(eResponseStatus.InvalidUser, "Invalid User", "The user you selected for this action is not a valid user");
        public static ClientExceptionType USER_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.UserNotAllowed, "User Not Allowed", "The user you selected for this action doesn't have the necessary permissions");
        public static ClientExceptionType DUPLICATE_PIN = new ClientExceptionType(eResponseStatus.DuplicatePin, "Duplicate Pin", "The PIN number you entered is already being used in this household");
        public static ClientExceptionType USER_ALREADY_IN_Domain = new ClientExceptionType(eResponseStatus.UserAlreadyInDomain, "User Already In Domain", "Unable to add a user to the same household twice");
        public static ClientExceptionType NOT_ALLOWED_TO_DELETE = new ClientExceptionType(eResponseStatus.NotAllowedToDelete, "Not Allowed To Delete", "Unable to delete this user from the account due to permission limitations");
        public static ClientExceptionType HOME_NETWORK_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.HomeNetworkAlreadyExists, "Home Network Already Exists", "Unable to add a home network to the same household twice");
        public static ClientExceptionType HOME_NETWORK_LIMITATION = new ClientExceptionType(eResponseStatus.HomeNetworkLimitation, "Home Network Limitation", "The number of home networks in the household has been exceeded");
        public static ClientExceptionType HOME_NETWORK_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.HomeNetworkDoesNotExist, "Home Network Does Not Exist", "The home network you specified does not exis");
        public static ClientExceptionType HOME_NETWORK_FREQUENCY = new ClientExceptionType(eResponseStatus.HomeNetworkFrequency, "Home Network Frequency", "Unable to remove the home network from the household because of the frequency limitation");
        public static ClientExceptionType REGION_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.RegionDoesNotExist, "Region does not exist", "Region does not exist");

        #endregion

        #region User Section 2000 - 2999

        public static ClientExceptionType USER_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.UserDoesNotExist, "User Does Not Exist", "This user doesn't exist");
        public static ClientExceptionType USER_SUSPENDED = new ClientExceptionType(eResponseStatus.UserSuspended, "User Suspended", "Unable to perform this action due to a household suspension");
        public static ClientExceptionType PIN_NOT_EXISTS = new ClientExceptionType(eResponseStatus.PinNotExists, "Pin Not Exists", "The PIN provided does not exist in the system");
        public static ClientExceptionType PIN_EXPIRED = new ClientExceptionType(eResponseStatus.PinExpired, "Pin Expired", "The PIN provided has expired");
        public static ClientExceptionType NO_VALID_PIN = new ClientExceptionType(eResponseStatus.NoValidPin, "No Valid Pin", "The PIN provided is not valid");
        public static ClientExceptionType MISSING_SECURITY_PARAMETER = new ClientExceptionType(eResponseStatus.MissingSecurityParameter, "Missing Security Parameter", "The security answer for the PIN code is missing");
        public static ClientExceptionType SECRET_IS_WRONG = new ClientExceptionType(eResponseStatus.SecretIsWrong, "Secret Is Wrong", "The secret provided is incorrect");
        public static ClientExceptionType LOGIN_VIA_PIN_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.LoginViaPinNotAllowed, "Login Via Pin Not Allowed", "Log in using a PIN is not enabled for this account");
        public static ClientExceptionType PIN_NOT_IN_THE_RIGHT_LENGTH = new ClientExceptionType(eResponseStatus.PinNotInTheRightLength, "Pin Not In The Right Length", "The PIN provided is not valid.(does not match the required number of digits).");
        public static ClientExceptionType PIN_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.PinAlreadyExists, "Pin Already Exists", "The PIN that you entered already exists in the system");
        public static ClientExceptionType USER_EXISTS = new ClientExceptionType(eResponseStatus.UserExists, "User Exists", "The user you are trying to add already exists");
        public static ClientExceptionType INSIDE_LOCK_TIME = new ClientExceptionType(eResponseStatus.InsideLockTime, "Inside Lock Time", "The account has been locked");
        public static ClientExceptionType USER_NOT_ACTIVATED = new ClientExceptionType(eResponseStatus.UserNotActivated, "User Not Activated", "The user must be activated to log in");
        public static ClientExceptionType USER_ALLREADY_LOGGED_IN = new ClientExceptionType(eResponseStatus.UserAllreadyLoggedIn, "User Allready Logged In", "This user is already logged in");
        public static ClientExceptionType USER_DOUBLE_LOG_IN = new ClientExceptionType(eResponseStatus.UserDoubleLogIn, "User Double Log In", "The user can't be logged in on more than one device");
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
        public static ClientExceptionType EXCLUSIVE_MASTER_USER_CANNOT_BE_DELETED = new ClientExceptionType(eResponseStatus.ExclusiveMasterUserCannotBeDeleted, "Exclusive Master User Cannot Be Deleted", "The exclusive household master user can't be deleted");
        public static ClientExceptionType ITEM_NOT_FOUND = new ClientExceptionType(eResponseStatus.ItemNotFound, "Item Not Found", "Unable to find the item you requested");
        public static ClientExceptionType EXTERNAL_ID_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.ExternalIdAlreadyExists, "External ID already exists", "The external ID you are trying to add / update already exists");
        public static ClientExceptionType PARENTID_SHOULD_NOT_POINT_TO_ITSELF = new ClientExceptionType(eResponseStatus.ParentIdShouldNotPointToItself, "ParentId Should Not Point To Itself", "ParentId should not point to itself");
        public static ClientExceptionType PARENTID_NOT_EXIST = new ClientExceptionType(eResponseStatus.ParentIdNotExist, "ParentId Not Exist");
        public static ClientExceptionType USER_FAVORITE_NOT_DELETED = new ClientExceptionType(eResponseStatus.UserFavoriteNotDeleted, "User Favorite Not Deleted");
        public static ClientExceptionType USER_SELF_DELETE_NOT_PERMITTED = new ClientExceptionType(eResponseStatus.UserSelfDeleteNotPermitted, "User Self Delete Not Permitted", "Self deletion is not permitted.");
        public static ClientExceptionType USER_EXTERNAL_ERROR = new ClientExceptionType(eResponseStatus.UserExternalError, "User External Error", "User External Error.");
        public static ClientExceptionType ActionBlocked = new ClientExceptionType(eResponseStatus.ActionBlocked, "Blocked by segment", "Blocked by segment.");

        #endregion

        #region CAS Section 3000 - 3999

        public static ClientExceptionType INVALID_PURCHASE = new ClientExceptionType(eResponseStatus.InvalidPurchase, "Invalid Purchase", "Unable to complete the purchase of the item requested");
        public static ClientExceptionType CANCELATION_WINDOW_PERIOD_EXPIRED = new ClientExceptionType(eResponseStatus.CancelationWindowPeriodExpired, "Cancelation Window Period Expired",
            "Unable to cancel the product request because the cancellation window has expired.");
        public static ClientExceptionType SUBSCRIPTION_NOT_RENEWABLE = new ClientExceptionType(eResponseStatus.SubscriptionNotRenewable, "Subscription Not Renewable", "Unable to perform this action on a subscription that is not renewable");
        public static ClientExceptionType SERVICE_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.ServiceNotAllowed, "Service Not Allowed", "The user is not entitled to the premium service that he or she is trying to access");
        public static ClientExceptionType INVALID_BASE_LINK = new ClientExceptionType(eResponseStatus.InvalidBaseLink, "Invalid Base Link", "The CDN code that was provided is incorrect");
        public static ClientExceptionType CONTENT_ALREADY_CONSUMED = new ClientExceptionType(eResponseStatus.ContentAlreadyConsumed, "Content Already Consumed", "Unable to complete this request - content cannot be cancelled after being viewed.");
        public static ClientExceptionType REASON_UNKNOWN = new ClientExceptionType(eResponseStatus.ReasonUnknown, "Reason Unknown", "The request failed for an unknown reason");
        public static ClientExceptionType CHARGE_STATUS_UNKNOWN = new ClientExceptionType(eResponseStatus.ChargeStatusUnknown, "Charge Status Unknown", "The charge status for this purchase is unknown");
        public static ClientExceptionType CONTENT_IDMISSING = new ClientExceptionType(eResponseStatus.ContentIDMissing, "Content IDMissing", "The Content ID is missing");
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
        public static ClientExceptionType NOT_FOR_PURCHASE = new ClientExceptionType(eResponseStatus.NotForPurchase, "Not For Purchase", "The Content ID entered is not available for purchase.");
        public static ClientExceptionType FAIL = new ClientExceptionType(eResponseStatus.Fail, "Fail"); //??
        public static ClientExceptionType UNABLE_TO_PURCHASE_COLLECTION_PURCHASED = new ClientExceptionType(eResponseStatus.UnableToPurchaseCollectionPurchased, "Unable To Purchase Collection Purchased", "This collection has already been purchased by this household");
        public static ClientExceptionType FILE_TO_MEDIA_MISMATCH = new ClientExceptionType(eResponseStatus.FileToMediaMismatch, "File To Media Mismatch", "The file and media don't match");
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
        public static ClientExceptionType PAYMENT_METHOD_IS_USED_BY_Domain = new ClientExceptionType(eResponseStatus.PaymentMethodIsUsedByHousehold, "The payment method you're trying to remove is already being used by the household");
        public static ClientExceptionType EXCEEDED_QUOTA = new ClientExceptionType(eResponseStatus.ExceededQuota, "Exceeded Quota", "You've reached the maximum quote buffer for your household");
        public static ClientExceptionType RECORDING_STATUS_NOT_VALID = new ClientExceptionType(eResponseStatus.RecordingStatusNotValid, "Recording Status Not Valid", "Unable to perform the action requested because of the current recording status. Actions are only allowed for these statuses:Recorded, Recording, Scheduled");
        public static ClientExceptionType EXCEEDED_PROTECTION_QUOTA = new ClientExceptionType(eResponseStatus.ExceededProtectionQuota, "Exceeded Protection Quota", "You've reached the maximum quota on protected programs, and can't protect any additional programs");
        public static ClientExceptionType ACCOUNT_PROTECT_RECORD_NOT_ENABLED = new ClientExceptionType(eResponseStatus.AccountProtectRecordNotEnabled, "Account Protect Record Not Enabled", "The account recording protection feature is disabled");
        public static ClientExceptionType ACCOUNT_SERIES_RECORDING_NOT_ENABLED = new ClientExceptionType(eResponseStatus.AccountSeriesRecordingNotEnabled, "Account Series Recording Not Enabled", "The account series recording feature is disabled");
        public static ClientExceptionType ALREADY_RECORDED_AS_SERIES_OR_SEASON = new ClientExceptionType(eResponseStatus.AlreadyRecordedAsSeriesOrSeason, "Already Recorded As Series Or Season", "This program has already been recorded as part of a series/season recording");
        public static ClientExceptionType SERIES_RECORDING_NOT_FOUND = new ClientExceptionType(eResponseStatus.SeriesRecordingNotFound, "Series Recording Not Found", "Unable to find the requested series recording");
        public static ClientExceptionType EPG_ID_NOT_PART_OF_SERIES = new ClientExceptionType(eResponseStatus.EpgIdNotPartOfSeries, "Epg Id Not Part Of Series", "Unable to cancel or delete an EPG program that is not part of the series");
        public static ClientExceptionType RECORDING_PLAYBACK_NOT_ALLOWED_FOR_NON_EXISTING_EPG_CHANNEL = new ClientExceptionType(eResponseStatus.RecordingPlaybackNotAllowedForNonExistingEpgChannel, "Recording Playback Not Allowed For Non Existing Epg Channel", "Recording playback is not allowed for a non-existing linear channel");
        public static ClientExceptionType RECORDING_PLAYBACK_NOT_ALLOWED_FOR_NOT_ENTITLED_EPG_CHANNEL = new ClientExceptionType(eResponseStatus.RecordingPlaybackNotAllowedForNotEntitledEpgChannel, "Recording Playback Not Allowed For Not Entitled Epg Channel", "Recording playback is not allowed for non-entitled linear channels");
        public static ClientExceptionType SEASON_NUMBER_NOT_MATCH = new ClientExceptionType(eResponseStatus.SeasonNumberNotMatch, "Season Number Not Match", "The season number you entered doesn't match the season number that was recorded");
        public static ClientExceptionType SUBSCRIPTION_CANCELLATION_IS_BLOCKED = new ClientExceptionType(eResponseStatus.SubscriptionCancellationIsBlocked, "The cancellation for the specified subscription is blocked");
        public static ClientExceptionType INVALID_OFFER = new ClientExceptionType(eResponseStatus.InvalidOffer, "This offer is invalid", "This offer is invalid");
        public static ClientExceptionType SUBSCRIPTION_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.SubscriptionDoesNotExist, "The subscription does not exist", "The subscription does not exist");
        public static ClientExceptionType OTHER_COUPON_ALREADY_APPLIED = new ClientExceptionType(eResponseStatus.OtherCouponIsAlreadyAppliedForSubscription, "Other coupon is already applied for subscription", "Other coupon is already applied for subscription");
        public static ClientExceptionType CAMPAIGN_ALREADY_APPLIED = new ClientExceptionType(eResponseStatus.CampaignIsAlreadyAppliedForSubscription, "Campaign is already applied for subscription", "Campaign is already applied for subscription");
        public static ClientExceptionType PURCHASE_PENDING_FAILED = new ClientExceptionType(eResponseStatus.PurchasePendingFailed, "");
        public static ClientExceptionType PENDING_ENTITELMENT = new ClientExceptionType(eResponseStatus.PendingEntitlement, "", "Entitlement is pending");
        public static ClientExceptionType INVALID_CONTENT_ID = new ClientExceptionType(eResponseStatus.InvalidContentId, "Illegal content ID", "Illegal content ID");
        public static ClientExceptionType CAN_ONLY_BE_ENTITLED_TO_ONE_SUBSCRIPTION_PER_SUBSCRIPTIONSET = new ClientExceptionType(eResponseStatus.CanOnlyBeEntitledToOneSubscriptionPerSubscriptionSet, "Can only be entitled to one subscription per subscriptionSet, please use Upgrade or Downgrade", "Can only be entitled to one subscription per subscriptionSet, please use Upgrade or Downgrade");
        public static ClientExceptionType SUBSCRIPTION_NOT_ALLOWED_FOR_USER_TYPE = new ClientExceptionType(eResponseStatus.SubscriptionNotAllowedForUserType, "Subscription is not allowed for user type", "Subscription is not allowed for user type");
        public static ClientExceptionType MISSING_BASE_PACKAGE = new ClientExceptionType(eResponseStatus.MissingBasePackage, "Missing base package", "Missing base package");
        public static ClientExceptionType SUBSCRIPTION_SET_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.SubscriptionSetDoesNotExist, "The subscriptionSet does not exist", "The subscriptionSet does not exist");
        public static ClientExceptionType PURCHASE_PASSED_ENTITLEMENT_FAILED = new ClientExceptionType(eResponseStatus.PurchasePassedEntitlementFailed, "purchase passed but entitlement failed", "purchase passed but entitlement failed");
        public static ClientExceptionType PURCHASE_FAILED = new ClientExceptionType(eResponseStatus.PurchaseFailed, "purchase failed", "purchase failed");
        public static ClientExceptionType PROGRAM_START_OVER_NOT_ENABLED = new ClientExceptionType(eResponseStatus.ProgramStartOverNotEnabled, "Program Start Over Not Enabled", "This program does not support start-over");

        public static ClientExceptionType PAGO_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.ProgramAssetGroupOfferDoesNotExist, "The programAssetGroupOffer does not exist", "The programAssetGroupOffer does not exist");
        public static ClientExceptionType EXTERNAL_OFFER_ID_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.ExternalOfferIdAlreadyExists, "External offer id already exists", "The external offer Id you are trying to add / update already exists");
        public static ClientExceptionType UNABLE_TO_PURCHASE_PAGO_PURCHASED = new ClientExceptionType(eResponseStatus.UnableToPurchaseProgramAssetGroupOfferPurchased, "Unable To Purchase PAGO Purchased", "This ProgramAssetGroupOffer has already been purchased by this household");

        public static ClientExceptionType CAMPAIGN_UPDATE_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.CampaignUpdateNotAllowed, "Campaign Update Not Allowed", "Only campaign in state INACTIVE can be updated");
        public static ClientExceptionType INVALID_CAMPAIGN_STATE = new ClientExceptionType(eResponseStatus.InvalidCampaignState, "Invalid Campaign State", "Campaign is already in given state");
        public static ClientExceptionType CAMPAIGN_STATE_UPDATE_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.CampaignStateUpdateNotAllowed, "Campaign State Update Not Allowed", "Can update campaign state only from INACTIVE to ACTIVE or from ACTIVE to ARCHIVE");
        public static ClientExceptionType INVALID_CAMPAIGN_ENDDATE = new ClientExceptionType(eResponseStatus.InvalidCampaignEndDate, "Invalid Campaign EndDate", "Can update campaign state only if its EndDate is in the future");
        public static ClientExceptionType CAN_ONLY_UPDATE_PADDING_AFTER_RECORDING_BEFORE_RECORDING_END = new ClientExceptionType(eResponseStatus.CanOnlyUpdatePaddingAfterRecordingBeforeRecordingEnd, "Can Only Update Padding After Recording Before Recording End", "Can Only Update Padding After Recording Before Recording End");
        public static ClientExceptionType CAN_ONLY_UPDATE_PADDING_BEFORE_RECORDING_BEFORE_RECORDING_START = new ClientExceptionType(eResponseStatus.CanOnlyUpdatePaddingBeforeRecordingBeforeRecordingStart, "Can Only Update Padding Before Recording Before Recording Start", "Can Only Update Padding Before Recording Before Recording Start");
        public static ClientExceptionType CAN_ONLY_ADD_RECORDING_BEFORE_RECORDING_START = new ClientExceptionType(eResponseStatus.CanOnlyAddRecordingBeforeRecordingStart, "Can Only Add Recording Before Recording Start", "Can Only Add Recording Before Recording Start");
        public static ClientExceptionType CAN_ONLY_CANCEL_RECORDING_BEFORE_RECORDING_START = new ClientExceptionType(eResponseStatus.CanOnlyCancelRecordingBeforeRecordingEnd, "Can Only Cancel Recording Before Recording End", "Can Only Cancel Recording Before Recording End");
        public static ClientExceptionType CAN_ONLY_DELETE_RECORDING_AFTER_RECORDING_END = new ClientExceptionType(eResponseStatus.CanOnlyDeleteRecordingAfterRecordingEnd, "Can Only Delete Recording After Recording end", "Can Only Delete Recording After Recording end");
        public static ClientExceptionType RECORDING_EXCEEDED_CONCURRENCY = new ClientExceptionType(eResponseStatus.RecordingExceededConcurrency, "Recording Exceeded Concurrency", "Recording Exceeded Concurrency");

        #endregion

        #region Catalog 4000 - 4999

        public static ClientExceptionType MEDIA_CONCURRENCY_LIMITATION = new ClientExceptionType(eResponseStatus.MediaConcurrencyLimitation, "Media Concurrency Limitation", "Media concurrency limitation (according to DLM configuration)");
        public static ClientExceptionType CONCURRENCY_LIMITATION = new ClientExceptionType(eResponseStatus.ConcurrencyLimitation, "Concurrency Limitation", "Concurrency limitation (according to DLM configuration)");
        public static ClientExceptionType BAD_SEARCH_REQUEST = new ClientExceptionType(eResponseStatus.BadSearchRequest, "Unified Search request has something invalid, either one of the data parameters or one of the field values in KSQL query.", "");
        public static ClientExceptionType INDEX_MISSING = new ClientExceptionType(eResponseStatus.IndexMissing, "Index Missing", "Relevant ElasticSearch index doesn't exist, either because it wasn't built or some other error.");
        public static ClientExceptionType SYNTAX_ERROR = new ClientExceptionType(eResponseStatus.SyntaxError, "Syntax Error", "KSQL query string contains a syntax error. It is not in the correct and expected format.");
        public static ClientExceptionType INVALID_SEARCH_FIELD = new ClientExceptionType(eResponseStatus.InvalidSearchField, "KSQL - at least one field name doesn't exist as a meta, tag or reserved keyword", "");
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
        public static ClientExceptionType INACTIVE_EXTERNAL_CHANNEL_ENRICHMENT = new ClientExceptionType(eResponseStatus.InactiveExternalChannelEnrichment, "Inactive External Channel Enrichment", "The specified external channel enrichment is not available");
        public static ClientExceptionType IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.IdentifierRequired, "Identifier Required", "Please specify the channel ID identifier");
        public static ClientExceptionType OBJECT_NOT_EXIST = new ClientExceptionType(eResponseStatus.ObjectNotExist, "Object Not Exist", "The object requested doesn't exist");
        public static ClientExceptionType NO_OBJECT_TO_INSERT = new ClientExceptionType(eResponseStatus.NoObjectToInsert, "No Object To Insert", "There's no channel to add");
        public static ClientExceptionType INVALID_MEDIA_TYPE = new ClientExceptionType(eResponseStatus.InvalidMediaType, "Invalid Media Type", "The asset type does not match one of the group asset types");
        public static ClientExceptionType INVALID_ASSET_TYPE = new ClientExceptionType(eResponseStatus.InvalidAssetType, "Invalid Asset Type", "The asset requested is not a valid asset type");
        public static ClientExceptionType PROGRAM_DOESNT_EXIST = new ClientExceptionType(eResponseStatus.ProgramDoesntExist, "Program Doesnt Exist", "The EPG program requested doesn't exist");
        public static ClientExceptionType ACTION_NOT_RECOGNIZED = new ClientExceptionType(eResponseStatus.ActionNotRecognized, "Action Not Recognized", "Unable to recognize the action you specified");
        public static ClientExceptionType INVALID_ASSET_ID = new ClientExceptionType(eResponseStatus.InvalidAssetId, "Invalid Asset Id", "The specified asset ID is invalid");
        public static ClientExceptionType COUNTRY_NOT_FOUND = new ClientExceptionType(eResponseStatus.CountryNotFound, "Country Not Found", "Unable to find the country code specified");

        public static ClientExceptionType ASSET_STRUCT_NAME_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.AssetStructNameAlreadyInUse, "Name Already Used", "The asset struct name is already in use");
        public static ClientExceptionType ASSET_STRUCT_SYSTEM_NAME_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.AssetStructSystemNameAlreadyInUse, "System Name Already Used", "The asset struct system name is already in use");
        public static ClientExceptionType META_IDS_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.MetaIdsDoesNotExist, "Invalid Meta Id", "One or more of the specified meta ids does not exist");
        public static ClientExceptionType ASSET_STRUCT_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.AssetStructDoesNotExist, "Invalid Asset Struct Id", "The specified asset strcut id does not exist");
        public static ClientExceptionType ASSET_STRUCT_METAS_CONTAIN_SYSTEM_NAME_DUPLICATION = new ClientExceptionType(eResponseStatus.AssetStructMetasConatinSystemNameDuplication, "Metas with the same system name were send as part of the Asset Struct");
        public static ClientExceptionType CAN_NOT_CHANGE_PREDEFINED_ASSET_STRUCT_SYSTEM_NAME = new ClientExceptionType(eResponseStatus.CanNotChangePredefinedAssetStructSystemName, "System Name Can Not Be Changed",
                                                                                                                        "can not change predefined asset struct name");
        public static ClientExceptionType CAN_NOT_DELETE_PREDEFINED_ASSET_STRUCT = new ClientExceptionType(eResponseStatus.CanNotDeletePredefinedAssetStruct, "Predefined Asset Struct Can not be deleted",
                                                                                                            "can not delete predefined asset struct");
        public static ClientExceptionType META_SYSTEM_NAME_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.MetaSystemNameAlreadyInUse, "System Name Already Used", "The meta system name is already in use");
        public static ClientExceptionType INVALID_MULTIPLE_VALUE_FOR_META_DATA_TYPE = new ClientExceptionType(eResponseStatus.InvalidMutlipleValueForMetaType, "MultipleValue can only be set to true for KalturaMetaType - STRING");
        public static ClientExceptionType META_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.MetaDoesNotExist, "Invalid Meta Id", "The specified meta id does not exist");
        public static ClientExceptionType CAN_NOT_CHANGE_PREDEFINED_META_SYSTEM_NAME = new ClientExceptionType(eResponseStatus.CanNotChangePredefinedMetaSystemName, "System Name Can Not Be Changed",
                                                                                                                        "can not change predefined meta name");
        public static ClientExceptionType CAN_NOT_DELETE_PREDEFINED_META = new ClientExceptionType(eResponseStatus.CanNotDeletePredefinedMeta, "Predefined meta Can not be deleted",
                                                                                                            "can not delete predefined meta");
        public static ClientExceptionType ASSET_STRUCT_MISSING_BASIC_META_IDS = new ClientExceptionType(eResponseStatus.AssetStructMissingBasicMetaIds, "Missing Basic Meta Ids", "One or more of the basic meta ids was not sent");
        public static ClientExceptionType ASSET_EXTERNAL_ID_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.AssetExternalIdMustBeUnique, "The Specified External Id Must Be Unique");
        public static ClientExceptionType INVALID_META_TYPE = new ClientExceptionType(eResponseStatus.InvalidMetaType, "Invalid Meta Type Sent");
        public static ClientExceptionType INVALID_VALUE_SENT_FOR_META = new ClientExceptionType(eResponseStatus.InvalidValueSentForMeta, "Invalid Value Sent For Meta");
        public static ClientExceptionType INVALID_DEVICE_RULE = new ClientExceptionType(eResponseStatus.DeviceRuleDoesNotExistForGroup, "Invalid Device Rules Sent");
        public static ClientExceptionType INVALID_GEO_BLOCK_RULE = new ClientExceptionType(eResponseStatus.GeoBlockRuleDoesNotExistForGroup, "Invalid Geo Block Rules Sent");
        public static ClientExceptionType ASSET_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.AssetDoesNotExist, "The Specified Asset ID Does Not Exist");
        public static ClientExceptionType METAS_DOES_NOT_EXIST_ON_ASSET = new ClientExceptionType(eResponseStatus.MetaIdsDoesNotExistOnAsset, "Invalid Asset Struct Id", "One or more of the specified meta ids does not exist");
        public static ClientExceptionType ASSET_FILE_TYPE_NAME_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.MediaFileTypeNameAlreadyInUse, "The Media File Type Name Is Already In Use");
        public static ClientExceptionType Media_FILE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.MediaFileTypeDoesNotExist, "The Asset File Type Does Not Exist");
        public static ClientExceptionType CAN_NOT_REMOVE_BASIC_META_IDS = new ClientExceptionType(eResponseStatus.CanNotRemoveBasicMetaIds, "Can Not Remove Basic Meta Ids from Asset");
        public static ClientExceptionType RATIO_ALREADY_EXIST = new ClientExceptionType(eResponseStatus.RatioAlreadyExist, "Ratio Already Exist");
        public static ClientExceptionType RATIO_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.RatioDoesNotExist, "Ratio Does Not Exist");
        public static ClientExceptionType IVALID_URL_FOR_IMAGE = new ClientExceptionType(eResponseStatus.InvalidUrlForImage, "Invalid Url For Image");
        public static ClientExceptionType MEDIA_FILE_WITH_THIS_TYPE_ALREADY_EXISTS_FOR_ASSET = new ClientExceptionType(eResponseStatus.MediaFileWithThisTypeAlreadyExistForAsset, "Media File With This Type Already Exist For Asset");
        public static ClientExceptionType DEFUALT_CDN_ADAPTER_PROFILE_NOT_CONFIGURED = new ClientExceptionType(eResponseStatus.DefaultCdnAdapterProfileNotConfigurd, "Default Cdn Adapter Profile Not Configured");
        public static ClientExceptionType CDN_ADAPTER_PROFILE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.CdnAdapterProfileDoesNotExist, "Cdn Adapter Profile Does Not Exist");
        public static ClientExceptionType IVALID_RATIO_FOR_IMAGE = new ClientExceptionType(eResponseStatus.InvalidRatioForImage, "Invalid Ratio For Image");
        public static ClientExceptionType EXTERNAL_AND_ALT_EXTERNAL_ID_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.ExternaldAndAltExternalIdMustBeUnique, "External Id And Alt External Id Must Be Unique");
        public static ClientExceptionType MEDIA_FILE_ALT_EXTERNAL_ID_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.MediaFileAltExternalIdMustBeUnique, "Media File Alt External Id Must Be Unique");
        public static ClientExceptionType MEDIA_FILE_EXTERNAL_ID_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.MediaFileExternalIdMustBeUnique, "Media File External Id Must Be Unique");
        public static ClientExceptionType MEDIA_FILE_NOT_BELONG_TO_ASSET = new ClientExceptionType(eResponseStatus.MediaFileNotBelongToAsset, "Media File Not Belong To Asset");
        public static ClientExceptionType MEDIA_FILE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.MediaFileDoesNotExist, "Media File Does Not Exist");
        public static ClientExceptionType IMAGE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.ImageDoesNotExist, "Image Does Not Exist");
        public static ClientExceptionType DEFAULT_IMAGE_INVALID_IMAGE_TYPE = new ClientExceptionType(eResponseStatus.DefaultImageInvalidImageType, "Default Image Invalid Image Type");
        public static ClientExceptionType IMAGE_TYPE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.ImageTypeDoesNotExist, "Image Type Does Not Exist");
        public static ClientExceptionType IMAGE_TYPE_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.ImageTypeAlreadyInUse, "Image Type Already In Use");
        public static ClientExceptionType TAG_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.TagDoesNotExist, "Tag Does Not Exist");
        public static ClientExceptionType TAG_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.TagAlreadyInUse, "Tag Already In Use");
        public static ClientExceptionType CHANNEL_SYSTEM_NAME_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.ChannelSystemNameAlreadyInUse, "System Name Already Used", "The channel system name is already in use");
        public static ClientExceptionType CHANNEL_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.ChannelDoesNotExist, "Channel Does Not Exist");
        public static ClientExceptionType CHANNEL_META_ORDER_BY_IS_INVALID = new ClientExceptionType(eResponseStatus.ChannelMetaOrderByIsInvalid, "Channel Meta Order By Is Invalid");
        public static ClientExceptionType ACCOUNT_IS_NOT_OPC_SUPPORTED = new ClientExceptionType(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
        public static ClientExceptionType CANNOT_DELETE_PARENT_ASSET_STRUCT = new ClientExceptionType(eResponseStatus.CanNotDeleteParentAssetStruct, "Can Not Delete Parent Asset Struct");
        public static ClientExceptionType INVALID_BULK_UPLOAD_STRUCTURE = new ClientExceptionType(eResponseStatus.InvalidBulkUploadStructure, "", "Invalid BulkUpload Structure");
        public static ClientExceptionType BULK_UPLOAD_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.BulkUploadDoesNotExist, "", "BulkUpload Does Not Exist");
        public static ClientExceptionType BULK_UPLOAD_RESULT_IS_MISSING = new ClientExceptionType(eResponseStatus.BulkUploadResultIsMissing, "", "BulkUploadResult Is Missing");
        public static ClientExceptionType RELATED_ENTITIES_EXCEED_LIMITATION = new ClientExceptionType(eResponseStatus.RelatedEntitiesExceedLimitation, "", "Related entities exceed limitation");
        public static ClientExceptionType ACCOUNT_EPG_INGEST_VERSION_NOT_SUPPORTED = new ClientExceptionType(eResponseStatus.AccountEpgIngestVersionDoesNotSupportBulk, "", "Account Epg Ingest Version does not support ingest using bulk upload");
        public static ClientExceptionType CAN_NOT_DELETE_OBJECT_VIRTUAL_ASSET_META = new ClientExceptionType(eResponseStatus.CanNotDeleteObjectVirtualAssetMeta, "", "can not delete object virtual asset meta");
        public static ClientExceptionType CATEGORY_NOT_EXIST = new ClientExceptionType(eResponseStatus.CategoryNotExist, "", "The category you selected does not exist");
        public static ClientExceptionType CHILD_CATEGORY_NOT_EXIST = new ClientExceptionType(eResponseStatus.ChildCategoryNotExist, "", "Child category does not exist");
        public static ClientExceptionType CHILD_CATEGORY_ALREADY_BELONGS_TO_ANOTHER = new ClientExceptionType(eResponseStatus.ChildCategoryAlreadyBelongsToAnotherCategory, "", "Child Category already belongs to another category");
        public static ClientExceptionType CHILD_CATEGORY_CANNOT_BE_THE_CATEGORY_ITSELF = new ClientExceptionType(eResponseStatus.ChildCategoryCannotBeTheCategoryItself, "", "A child category cannot be the category itself");
        public static ClientExceptionType INVALID_ASSET_STRUCT = new ClientExceptionType(eResponseStatus.InvalidAssetStruct, "", "Invalid asset struct");
        public static ClientExceptionType NO_NEXT_EPISODE = new ClientExceptionType(eResponseStatus.NoNextEpisode, "", "User have not started watching this TV series");
        public static ClientExceptionType CANNOT_DELETE_ASSET_STRUCT = new ClientExceptionType(eResponseStatus.CannotDeleteAssetStruct, "", "Cannot delete asset struct");
        public static ClientExceptionType CATEGORY_TYPE_NOT_EXIST = new ClientExceptionType(eResponseStatus.CategoryTypeNotExist, "", "Category type does not exist");
        public static ClientExceptionType EXTENDED_TYPE_VALUE_CANNOT_BE_CHANGED = new ClientExceptionType(eResponseStatus.ExtendedTypeValueCannotBeChanged, "", "ExtendedType value cannot be changed");
        public static ClientExceptionType INPUT_FORMAT_IS_INVALID = new ClientExceptionType(eResponseStatus.InputFormatIsInvalid, "", "The input format is invalid");
        public static ClientExceptionType DUPLICATE_REGION_CHANNEL = new ClientExceptionType(eResponseStatus.DuplicateRegionChannel, "", "The channel already in region channels list");
        public static ClientExceptionType PARENT_ALREADY_CONTAINS_CHANNEL = new ClientExceptionType(eResponseStatus.ParentAlreadyContainsChannel, "", "Parent region already contains channel");
        public static ClientExceptionType START_DATE_SHOULD_BE_LESS_THAN_END_DATE = new ClientExceptionType(eResponseStatus.StartDateShouldBeLessThanEndDate, "StartDate should be less than EndDate");
        public static ClientExceptionType LABEL_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.LabelAlreadyInUse, "Label Already In Use");
        public static ClientExceptionType LABEL_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.LabelDoesNotExist, "Label Does Not Exist");
        public static ClientExceptionType PREMIUM_SERVICE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.PremiumServiceDoesNotExist, "Premium Service Does Not Exist");
        public static ClientExceptionType VIDEO_CODECS_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.VideoCodecsDoesNotExist, "Video Codecs Does Not Exist");
        public static ClientExceptionType AUDIO_CODECS_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.AudioCodecsDoesNotExist, "Audio Codecs Does Not Exist");
        public static ClientExceptionType SEARCH_PRIORITY_GROUP_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.SearchPriorityGroupDoesNotExist, "Search Priority Group Does Not Exist");
        public static ClientExceptionType DYNAMIC_DATA_KEY_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.DynamicDataKeyDoesNotExist, "Dynamic Data Keys Do Not Exist.");
        public static ClientExceptionType DYNAMIC_DATA_KEY_VALUE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.DynamicDataKeyValueDoesNotExist, "Dynamic Data Key Value Does Not Exist.");
        #endregion

        #region Api 5000 - 5999

        public static ClientExceptionType NO_PIN_DEFINED = new ClientExceptionType(eResponseStatus.NoPinDefined, "No Pin Defined", "No parental PIN was defined for this user/household");
        public static ClientExceptionType PIN_MISMATCH = new ClientExceptionType(eResponseStatus.PinMismatch, "Pin Mismatch", "The parental PIN provided doesn't match the user/household PIN");
        public static ClientExceptionType RULE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.RuleNotExists, "Rule Not Exists", "This rule doesn't exist in the system");
        public static ClientExceptionType NO_OSSADAPTER_TO_INSERT = new ClientExceptionType(eResponseStatus.NoOSSAdapterToInsert, "No OSSAdapter To Insert", "There's no OSS Adapater Profile to add to the system");
        public static ClientExceptionType NAME_REQUIRED = new ClientExceptionType(eResponseStatus.NameRequired, "Name Required", "The mandatory name field is missing from the request");
        public static ClientExceptionType SHARED_SECRET_REQUIRED = new ClientExceptionType(eResponseStatus.SharedSecretRequired, "Shared Secret Required", "The mandatory shared secret field is missing from the request");
        public static ClientExceptionType OSSADAPTER_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.OSSAdapterIdentifierRequired, "OSSAdapter Identifier Required", "The mandatory OSS adapter identifier field is missing from the request");
        public static ClientExceptionType OSSADAPTER_NOT_EXIST = new ClientExceptionType(eResponseStatus.OSSAdapterNotExist, "OSSAdapter Not Exist", "The requested OSS adapter doesn't exist");
        public static ClientExceptionType OSSADAPTER_PARAMS_REQUIRED = new ClientExceptionType(eResponseStatus.OSSAdapterParamsRequired, "OSSAdapter Params Required", "The mandatory OSS adapter parameter fields are missing from the request");
        public static ClientExceptionType UNKNOWN_OSSADAPTER_STATE = new ClientExceptionType(eResponseStatus.UnknownOSSAdapterState, "Unknown OSSAdapter State", "The status of the OSS adapter is unknown");
        public static ClientExceptionType ACTION_IS_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.ActionIsNotAllowed, "Action Is Not Allowed", "The action requested is not allowed");
        public static ClientExceptionType NO_OSSADAPTER_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoOSSAdapterToUpdate, "No OSSAdapter To Update", "There's no OSS adapter to update");
        public static ClientExceptionType ADAPTER_URL_REQUIRED = new ClientExceptionType(eResponseStatus.AdapterUrlRequired, "Adapter Url Required", "The mandatory adapter URL field is missing from the request");
        public static ClientExceptionType CONFLICTED_PARAMS = new ClientExceptionType(eResponseStatus.ConflictedParams, "Conflicted Params", "The system has detected conflicts between parameters");
        public static ClientExceptionType PURCHASE_SETTINGS_TYPE_INVALID = new ClientExceptionType(eResponseStatus.PurchaseSettingsTypeInvalid, "Purchase Settings Type Invalid", "The specified purchase settings type is Invalid");
        public static ClientExceptionType EXPORT_TASK_NOT_FOUND = new ClientExceptionType(eResponseStatus.ExportTaskNotFound, "Export Task Not Found", "The requested export task wasn't found");
        public static ClientExceptionType EXPORT_NOTIFICATION_URL_REQUIRED = new ClientExceptionType(eResponseStatus.ExportNotificationUrlRequired, "Export Notification Url Required", "The mandatory export notification URL field is missing from the request");
        public static ClientExceptionType EXPORT_FREQUENCY_MIN_VALUE = new ClientExceptionType(eResponseStatus.ExportFrequencyMinValue, "Export Frequency Min Value", "The export frequency set is below the minimum allowed");
        public static ClientExceptionType ALIAS_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.AliasMustBeUnique, "Alias Must Be Unique", "Invalid entry: the alias value must be unique");
        public static ClientExceptionType ALIAS_REQUIRED = new ClientExceptionType(eResponseStatus.AliasRequired, "Alias Required", "The mandatory alias value field is missing from the request");
        public static ClientExceptionType USER_PARENTAL_RULE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.UserParentalRuleNotExists, "User Parental Rule Not Exists", "There is no parental rule associated with this user");
        public static ClientExceptionType TIME_SHIFTED_TV_PARTNER_SETTINGS_NOT_FOUND = new ClientExceptionType(eResponseStatus.TimeShiftedTvPartnerSettingsNotFound, "Time Shifted Tv Partner Settings Not Found", "The system did not find any TimeShiftedTvPartner-related settings");
        public static ClientExceptionType TIME_SHIFTED_TV_PARTNER_SETTINGS_NOT_SENT = new ClientExceptionType(eResponseStatus.TimeShiftedTvPartnerSettingsNotSent, "Time Shifted Tv Partner Settings Not Sent", "The TimeShiftedTvPartner settings specified are null");
        public static ClientExceptionType TIME_SHIFTED_TV_PARTNER_SETTINGS_NEGATIVE_BUFFER_SENT = new ClientExceptionType(eResponseStatus.TimeShiftedTvPartnerSettingsNegativeBufferSent, "Time Shifted Tv Partner Settings Negative Buffer Sent",
            "You've configured a negative buffer value in the TimeShiftedTvPartnerr settings");
        public static ClientExceptionType CDNPARTNER_SETTINGS_NOT_FOUND = new ClientExceptionType(eResponseStatus.CDNPartnerSettingsNotFound, "CDNPartner Settings Not Found", "The system didn't find any CDN partner -related settings for the group account");
        public static ClientExceptionType PERMISSION_NAME_NOT_EXISTS = new ClientExceptionType(eResponseStatus.PermissionNameNotExists, "", "Permission Name Not Exists");
        public static ClientExceptionType ASSET_RULE_NOT_EXIST = new ClientExceptionType(eResponseStatus.AssetRuleNotExists, "", "Asset rule doesn't exist");
        public static ClientExceptionType ASSET_USER_RULE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.AssetUserRuleDoesNotExists, "", "Asset user rule does not exist");
        public static ClientExceptionType USER_ALREADY_ATTACHED_TO_ASSET_USER_RULE = new ClientExceptionType(eResponseStatus.UserAlreadyAttachedToAssetUserRule, "", "User already attached to this AssetUserRule");
        public static ClientExceptionType ASSET_USER_RULES_OPERATIONS_DISABLE = new ClientExceptionType(eResponseStatus.AssetUserRulesOperationsDisable, "", "AssetUserRule operations are disabled for this partner");
        public static ClientExceptionType ROLE_DOES_NOT_EXISTS = new ClientExceptionType(eResponseStatus.RoleDoesNotExists, "", "Role Does Not Exists");
        public static ClientExceptionType FILE_DOES_NOT_EXISTS = new ClientExceptionType(eResponseStatus.FileDoesNotExists, "File Does Not Exists", "The file does not exist in a given path");
        public static ClientExceptionType FILE_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.FileAlreadyExists, "File Already Exists", "The file already exists in a given path");
        public static ClientExceptionType ERROR_SAVING_FILE = new ClientExceptionType(eResponseStatus.ErrorSavingFile, "Error While Saving File", "Error occurred while saving file to File Server");
        public static ClientExceptionType FILE_ID_NOT_IN_CORRECT_LENGTH = new ClientExceptionType(eResponseStatus.FileIdNotInCorrectLength, "", "The file ID is not the correct length");
        public static ClientExceptionType ILLEGAL_EXCEL_FILE = new ClientExceptionType(eResponseStatus.IllegalExcelFile, "Illegal Excel File", "The Excel is formatted incorrectly. Please check the file for format errors");
        public static ClientExceptionType ENQUEUE_FAILED = new ClientExceptionType(eResponseStatus.EnqueueFailed, "Enqueue Failed", "Enqueue object to Celery queue failed");
        public static ClientExceptionType EXCEL_MANDTORY_VALUE_IS_MISSING = new ClientExceptionType(eResponseStatus.ExcelMandatoryValueIsMissing, "Mandatory Value In Excel File Is Missing", "One of the mandatory values in the excel is missing");
        public static ClientExceptionType ASSET_RULE_STATUS_NOT_WRITABLE = new ClientExceptionType(eResponseStatus.AssetRuleStatusNotWritable, "Asset Rule Status Not Writable", "Cannot update or delete asset rule when in progress");
        public static ClientExceptionType Permission_Not_Found = new ClientExceptionType(eResponseStatus.PermissionNotFound, "Permission Not Found", "Permission not found");
        public static ClientExceptionType PERMISSION_NAME_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.PermissionNameAlreadyInUse, "Permission Name Already In Use", "Permission name already in use");
        public static ClientExceptionType EVENT_NOTIFICATION_ID_IS_MISSING = new ClientExceptionType(eResponseStatus.EventNotificationIdIsMissing, "event notification id is missing", "event notification id is missing");
        public static ClientExceptionType EVENT_NOTIFICATION_ID_NOT_FOUND = new ClientExceptionType(eResponseStatus.EventNotificationIdNotFound, "Event notification id not found", "Event notification id not found");
        public static ClientExceptionType REGION_NOT_FOUND = new ClientExceptionType(eResponseStatus.RegionNotFound, "Region was not found", "Region was not found");
        public static ClientExceptionType REGION_CANNOT_BE_PARENT = new ClientExceptionType(eResponseStatus.RegionCannotBeParent, "Region cannot be parent", "Region cannot be parent");
        public static ClientExceptionType DEFAULT_REGION_CANNOT_BE_DELETED = new ClientExceptionType(eResponseStatus.DefaultRegionCannotBeDeleted, "Default region cannot be deleted", "Default region cannot be deleted");
        public static ClientExceptionType CANNOT_DELETE_REGION_IN_USE = new ClientExceptionType(eResponseStatus.CannotDeleteRegionInUse, "Region in use by household and cannot be deleted", "Region in use by household and cannot be deleted");
        public static ClientExceptionType FILE_EXCEEDED_MAX_SIZE = new ClientExceptionType(eResponseStatus.FileExceededMaxSize, "File Exceeded Max Size", "File Exceeded Max Size");
        public static ClientExceptionType FILE_EXTENSION_NOT_SUPPORTED = new ClientExceptionType(eResponseStatus.FileExtensionNotSupported, "File Extension Not Supported", "File Extension Not Supported");
        public static ClientExceptionType FILE_MIME_DIFFERENT_THAN_EXPECTED = new ClientExceptionType(eResponseStatus.FileMimeDifferentThanExpected, "File Mime Different Than Expected", "File Mime Different Than Expected");
        public static ClientExceptionType PERMISSION_ITEM_NOT_FOUND = new ClientExceptionType(eResponseStatus.PermissionItemNotFound, "Permission item cannot be found", "Permission item cannot be found");
        public static ClientExceptionType PERMISSION_READ_ONLY = new ClientExceptionType(eResponseStatus.PermissionReadOnly, "Permission is readonly and cannot be updated", "Permission is readonly and cannot be updated");
        public static ClientExceptionType PERMISSION_PERMISSION_ITEM_NOT_FOUND = new ClientExceptionType(eResponseStatus.PermissionPermissionItemNotFound, "Permission item is not associated with permission", "Permission item is not associated with permission");
        public static ClientExceptionType PERMISSION_PERMISSION_ITEM_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.PermissionPermissionItemAlreadyExists, "Permission item is already associated with permission", "Permission item is already associated with permission");
        public static ClientExceptionType ROLE_READ_ONLY = new ClientExceptionType(eResponseStatus.RoleReadOnly, "Role is readonly and cannot be updated", "Role is readonly and cannot be updated");
        public static ClientExceptionType CAN_MODIFY_ONLY_NORMAL_PERMISSION = new ClientExceptionType(eResponseStatus.CanModifyOnlyNormalPermission, "Only permission type NORMAL can be modified", "Only permission type NORMAL can be modified");
        public static ClientExceptionType CANNOT_ADD_PERMISSION_TYPE_GROUP = new ClientExceptionType(eResponseStatus.CannotAddPermissionTypeGroup, "Permission type GROUP cannot be added", "Permission type GROUP cannot be added");
        public static ClientExceptionType DEVICE_BRAND_IDS_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.DeviceBrandIdsDoesNotExist, "Device Brand Ids Does Not Exist");
        public static ClientExceptionType SEGMENTS_IDS_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.SegmentsIdsDoesNotExist, "Segments Ids Does Not Exist");
        public static ClientExceptionType DEVICEMANUFACTURER_IDS_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.DeviceManufacturerIdsDoesNotExist, "DeviceManufacturer Ids Does Not Exist");
        public static ClientExceptionType USERSESSIONPROFILE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.UserSessionProfileDoesNotExist, "UserSessionProfile Does Not Exist");
        public static ClientExceptionType CANNOT_DELETE_USERSESSIONPROFILE = new ClientExceptionType(eResponseStatus.CannotDeleteUserSessionProfile, "Cannot Delete UserSessionProfile");
        public static ClientExceptionType DEVICE_FAMILY_ID_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.DeviceFamilyIdAlreadyInUse, "Device family id already in use");
        public static ClientExceptionType DEVICE_FAMILY_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.DeviceFamilyDoesNotExist, "Device family does not exist");
        public static ClientExceptionType DEVICE_BRAND_ID_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.DeviceBrandIdAlreadyInUse, "Device brand id already in use");
        public static ClientExceptionType DEVICE_BRAND_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.DeviceBrandDoesNotExist, "Device brand does not exist");
        public static ClientExceptionType CANNOT_DELETE_ATTACHED_SEGMENT = new ClientExceptionType(eResponseStatus.CannotDeleteAttachedSegment, "Cannot delete attached segment");
        public static ClientExceptionType DYNAMIC_SEGMENTS_EXCEEDED = new ClientExceptionType(eResponseStatus.DynamicSegmentsExceeded, "Dynamic segments exceeded ");
        public static ClientExceptionType DYNAMIC_SEGMENT_PERIOD_EXCEEDED = new ClientExceptionType(eResponseStatus.DynamicSegmentPeriodExceeded, "Dynamic segment period exceeded ");
        public static ClientExceptionType DYNAMIC_SEGMENT_CONDITIONS_EXCEEDED = new ClientExceptionType(eResponseStatus.DynamicSegmentConditionsExceeded, "Dynamic segment conditions exceeded");
        public static ClientExceptionType NAME_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.NameMustBeUnique, "Name must be unique");
        public static ClientExceptionType ENTITY_IS_NOT_ASSOCIATED_WITH_SHOP = new ClientExceptionType(eResponseStatus.EntityIsNotAssociatedWithShop, "Entity is not associated with Shop");

        #endregion

        #region Billing 6000 - 6999

        public static ClientExceptionType INCORRECT_PRICE = new ClientExceptionType(eResponseStatus.IncorrectPrice, "Incorrect Price", "The price shown for the item in the request is not the actual price.");
        public static ClientExceptionType UN_KNOWN_PPVMODULE = new ClientExceptionType(eResponseStatus.UnKnownPPVModule, "Un Known PPVModule", "This PPVModule does not belong to the item.");
        public static ClientExceptionType EXPIRED_CARD = new ClientExceptionType(eResponseStatus.ExpiredCard, "Expired Card", "The specified credit card has expired.");
        public static ClientExceptionType CELLULAR_PERMISSIONS_ERROR = new ClientExceptionType(eResponseStatus.CellularPermissionsError, "Cellular Permissions Error", "Cellular Permissions Error");
        public static ClientExceptionType UN_KNOWN_BILLING_PROVIDER = new ClientExceptionType(eResponseStatus.UnKnownBillingProvider, "Un Known Billing Provider", "The billing provider specified is not a recognized provider.");
        public static ClientExceptionType PAYMENT_GATEWAY_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayIdRequired, "Payment Gateway Id Required", "The mandatory Payment Gateway ID field is missing from the request.");
        public static ClientExceptionType PAYMENT_GATEWAY_PARAMS_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayParamsRequired, "Payment Gateway Params Required", "The mandatory Payment Gateway settings field is missing from the request.");
        public static ClientExceptionType PAYMENT_GATEWAY_NOT_SET_FOR_Domain = new ClientExceptionType(eResponseStatus.PaymentGatewayNotSetForHousehold, "Payment Gateway Not Set For Domain", "There's no Payment Gateway set up for this household.");
        public static ClientExceptionType PAYMENT_GATEWAY_NOT_EXIST = new ClientExceptionType(eResponseStatus.PaymentGatewayNotExist, "Payment Gateway Not Exist", "The requested Payment Gateway doesn't exist.");
        public static ClientExceptionType PAYMENT_GATEWAY_CHARGE_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayChargeIdRequired, "Payment Gateway Charge Id Required", "The mandatory Payment Gateway charge ID field is missing from the request.");
        public static ClientExceptionType NO_CONFIGURATION_FOUND = new ClientExceptionType(eResponseStatus.NoConfigurationFound, "No Configuration Found", "The configuration for the credit card clearing has not been set.");
        public static ClientExceptionType ADAPTER_APP_FAILURE = new ClientExceptionType(eResponseStatus.AdapterAppFailure, "Adapter App Failure", "The adapter failed to complete the request.");
        public static ClientExceptionType SIGNATURE_MISMATCH = new ClientExceptionType(eResponseStatus.SignatureMismatch, "Signature Mismatch", "The signature provide doesn't match the signature on record.");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_TRANSACTION = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayTransaction, "Error Saving Payment Gateway Transaction", "An error occurred while trying to save the Payment Gateway transaction.");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_PENDING = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayPending, "Error Saving Payment Gateway Pending", "An error occurred while trying to save the pending Payment Gateway");
        public static ClientExceptionType EXTERNAL_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.ExternalIdentifierRequired, "External Identifier Required", "The mandatory external identifier field is missing from the request.");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_Domain = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayHousehold, "Error Saving Payment Gateway Domain", "An error occurred while trying to set a Payment Gateway for this household.");
        public static ClientExceptionType NO_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.NoPaymentGateway, "No Payment Gateway", "No Payment Gateway specified in the request.");
        public static ClientExceptionType PAYMENT_GATEWAY_NAME_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewayNameRequired, "Payment Gateway Name Required", "The mandatory Payment Gateway name field is missing from the request.");
        public static ClientExceptionType PAYMENT_GATEWAY_SHARED_SECRET_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentGatewaySharedSecretRequired, "Payment Gateway Shared Secret Required", "The mandatory Payment Gateway shared secret field is missing from the request.");
        public static ClientExceptionType Domain_ALREADY_SET_TO_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.HouseholdAlreadySetToPaymentGateway, "Domain Already Set To Payment Gateway", "The household is already set to a Payment Gateway.");
        public static ClientExceptionType CHARGE_ID_ALREADY_SET_TO_Domain_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.ChargeIdAlreadySetToHouseholdPaymentGateway, "Charge Id Already Set To Domain Payment Gateway", "The charge ID was already set for the household Payment Gateway.");
        public static ClientExceptionType CHARGE_ID_NOT_SET_TO_Domain = new ClientExceptionType(eResponseStatus.ChargeIdNotSetToHousehold, "Charge Id Not Set To Domain", "There's no charge ID set for this household.");
        public static ClientExceptionType Domain_NOT_SET_TO_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.HouseholdNotSetToPaymentGateway, "Domain Not Set To Payment Gateway", "There's no Payment Gateway set for this household.");
        public static ClientExceptionType PAYMENT_GATEWAY_SELECTION_IS_DISABLED = new ClientExceptionType(eResponseStatus.PaymentGatewaySelectionIsDisabled, "Payment Gateway Selection Is Disabled", "The multiple Payment Gateway selection feature is disabled.");
        public static ClientExceptionType NO_RESPONSE_FROM_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.NoResponseFromPaymentGateway, "No Response From Payment Gateway", "The Payment Gateway failed to respond to the request because of a problem with the Payment Gateway adapter.");
        public static ClientExceptionType INVALID_ACCOUNT = new ClientExceptionType(eResponseStatus.InvalidAccount, "Invalid Account", "The account specified is invalid: there is a problem with the Payment Gateway adapter.");
        public static ClientExceptionType INSUFFICIENT_FUNDS = new ClientExceptionType(eResponseStatus.InsufficientFunds, "Insufficient Funds", "The payment method selected doesn't have sufficient funds for the transaction requested.");
        public static ClientExceptionType UNKNOWN_PAYMENT_GATEWAY_RESPONSE = new ClientExceptionType(eResponseStatus.UnknownPaymentGatewayResponse, "Unknown Payment Gateway Response", "An unknown error occurred with the Payment Gateway adapter.");
        public static ClientExceptionType PAYMENT_GATEWAY_ADAPTER_USER_KNOWN = new ClientExceptionType(eResponseStatus.PaymentGatewayAdapterUserKnown, "Payment Gateway Adapter User Known", "");
        public static ClientExceptionType PAYMENT_GATEWAY_ADAPTER_REASON_UNKNOWN = new ClientExceptionType(eResponseStatus.PaymentGatewayAdapterReasonUnknown, "Payment Gateway Adapter Reason Unknown", "");
        public static ClientExceptionType SIGNATURE_DOES_NOT_MATCH = new ClientExceptionType(eResponseStatus.SignatureDoesNotMatch, "Signature Does Not Match", "The payment method signatures don't match.");
        public static ClientExceptionType ERROR_UPDATING_PENDING_TRANSACTION = new ClientExceptionType(eResponseStatus.ErrorUpdatingPendingTransaction, "Error Updating Pending Transaction", "An error occurred when updating the pending transaction.");
        public static ClientExceptionType PAYMENT_GATEWAY_TRANSACTION_NOT_FOUND = new ClientExceptionType(eResponseStatus.PaymentGatewayTransactionNotFound, "Payment Gateway Transaction Not Found", "The requested Payment Gateway transaction was not found.");
        public static ClientExceptionType PAYMENT_GATEWAY_TRANSACTION_IS_NOT_PENDING = new ClientExceptionType(eResponseStatus.PaymentGatewayTransactionIsNotPending, "Payment Gateway Transaction Is Not Pending", "This transaction isn't in a Pending state.");
        public static ClientExceptionType EXTERNAL_IDENTIFIER_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.ExternalIdentifierMustBeUnique, "External Identifier Must Be Unique", "External identifier must be unique.");
        public static ClientExceptionType NO_PAYMENT_GATEWAY_TO_INSERT = new ClientExceptionType(eResponseStatus.NoPaymentGatewayToInsert, "No Payment Gateway To Insert", "Unable to complete the request: there's no new Payment Gateway to insert.");
        public static ClientExceptionType UNKNOWN_TRANSACTION_STATE = new ClientExceptionType(eResponseStatus.UnknownTransactionState, "Unknown Transaction State", "The transaction's state is currently unknown.");
        public static ClientExceptionType PAYMENT_GATEWAY_NOT_VALID = new ClientExceptionType(eResponseStatus.PaymentGatewayNotValid, "Payment Gateway Not Valid", "The specified Payment Gateway is not valid.");
        public static ClientExceptionType Domain_REQUIRED = new ClientExceptionType(eResponseStatus.HouseholdRequired, "Domain Required", "The mandatory household field is missing from the request.");
        public static ClientExceptionType PAYMENT_GATEWAY_ADAPTER_FAIL_REASON_UNKNOWN = new ClientExceptionType(eResponseStatus.PaymentGatewayAdapterFailReasonUnknown, "Payment Gateway Adapter Fail Reason Unknown", "The Payment Gateway adapter failed for an unknown reason.");
        public static ClientExceptionType NO_PARTNER_CONFIGURATION_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoPartnerConfigurationToUpdate, "No Partner Configuration To Update", "The partner configuration pair (type and configuration value) you've asked to update is an empty pair.");
        public static ClientExceptionType NO_CONFIGURATION_VALUE_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoConfigurationValueToUpdate, "No Configuration Value To Update", "The configuration value being updated is empty.");
        public static ClientExceptionType PAYMENT_METHOD_NOT_SET_FOR_Domain = new ClientExceptionType(eResponseStatus.PaymentMethodNotSetForHousehold, "Payment Method Not Set For Domain", "No payment method was set for this household.");
        public static ClientExceptionType PAYMENT_METHOD_NOT_EXIST = new ClientExceptionType(eResponseStatus.PaymentMethodNotExist, "Payment Method Not Exist", "The selected payment method doesn't exist.");
        public static ClientExceptionType PAYMENT_METHOD_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentMethodIdRequired, "Payment Method Id Required", "The mandatory payment method ID field is missing from the request.");
        public static ClientExceptionType PAYMENT_METHOD_EXTERNAL_ID_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentMethodExternalIdRequired, "Payment Method External Id Required", "The mandatory payment method external ID field is missing from the request.");
        public static ClientExceptionType ERROR_SAVING_PAYMENT_GATEWAY_Domain_PAYMENT_METHOD = new ClientExceptionType(eResponseStatus.ErrorSavingPaymentGatewayHouseholdPaymentMethod, "Error Saving Payment Gateway Domain Payment Method", "An error occurred while trying to save the payment method of the household  Payment Gateway. Please try again.");
        public static ClientExceptionType PAYMENT_METHOD_ALREADY_SET_TO_Domain_PAYMENT_GATEWAY = new ClientExceptionType(eResponseStatus.PaymentMethodAlreadySetToHouseholdPaymentGateway, "Payment Method Already Set To Domain Payment Gateway", "A payment method was already set for the household Payment Gateway.");
        public static ClientExceptionType PAYMENT_METHOD_NAME_REQUIRED = new ClientExceptionType(eResponseStatus.PaymentMethodNameRequired, "Payment Method Name Required", "The mandatory payment method name field is missing in the request.");
        public static ClientExceptionType PAYMENT_GATEWAY_NOT_SUPPORT_PAYMENT_METHOD = new ClientExceptionType(eResponseStatus.PaymentGatewayNotSupportPaymentMethod, "Payment Gateway Not Support Payment Method", "The Payment Gateway doesn't support this payment method.");
        public static ClientExceptionType PAYMENT_GATEWAY_SUSPENDED = new ClientExceptionType(eResponseStatus.PaymentGatewaySuspended, "Payment gateway suspended to this householdId", "Payment gateway suspended to this householdId.");

        #endregion

        #region social 7000 - 7999

        public static ClientExceptionType CONFLICT = new ClientExceptionType(eResponseStatus.Conflict, "Conflict", "A conflict has occurred.");
        public static ClientExceptionType MIN_FRIENDS_LIMITATION = new ClientExceptionType(eResponseStatus.MinFriendsLimitation, "Min Friends Limitation", "Minimum friends limitation");
        public static ClientExceptionType INVALID_PARAMETERS = new ClientExceptionType(eResponseStatus.InvalidParameters, "", "Invalid Parameters");

        #endregion

        #region notification 8000-8999

        public static ClientExceptionType NO_NOTIFICATION_SETTINGS_SENT = new ClientExceptionType(eResponseStatus.NoNotificationSettingsSent, "Internal error occurred", "The updated or new notification settings weren't received by the Web service because of an internal error.");
        public static ClientExceptionType PUSH_NOTIFICATION_FALSE = new ClientExceptionType(eResponseStatus.PushNotificationFalse, "Push notification false can't combine with push system announcements true", "Push notifications are disabled.");
        public static ClientExceptionType NO_NOTIFICATION_PARTNER_SETTINGS = new ClientExceptionType(eResponseStatus.NoNotificationPartnerSettings, "No Notification Partner Settings", "The updated or new partner notification settings weren't received by the Web service because of an internal error.");
        public static ClientExceptionType NO_NOTIFICATION_SETTINGS = new ClientExceptionType(eResponseStatus.NoNotificationSettings, "No Notification Settings", "The updated or new notification settings weren't received by the Web service because of an internal error.");
        public static ClientExceptionType ANNOUNCEMENT_MESSAGE_IS_EMPTY = new ClientExceptionType(eResponseStatus.AnnouncementMessageIsEmpty, "Announcement Message Is Empty", "The mandatory message field in the announcement message Is empty.");
        public static ClientExceptionType ANNOUNCEMENT_INVALID_START_TIME = new ClientExceptionType(eResponseStatus.AnnouncementInvalidStartTime, "Announcement Invalid Start Time", "The announcement start time is invalid. Please check and try again.");
        public static ClientExceptionType ANNOUNCEMENT_NOT_FOUND = new ClientExceptionType(eResponseStatus.AnnouncementNotFound, "Announcement Not Found", "The announcement requested couldn't be found.");
        public static ClientExceptionType ANNOUNCEMENT_UPDATE_NOT_ALLOWED = new ClientExceptionType(eResponseStatus.AnnouncementUpdateNotAllowed, "Announcement Update Not Allowed", "Unable to update the announcement; the announcement was already sent.");
        public static ClientExceptionType ANNOUNCEMENT_INVALID_TIMEZONE = new ClientExceptionType(eResponseStatus.AnnouncementInvalidTimezone, "Announcement Invalid Timezone", "The announcement time zone is invalid (for example 'UTC' or 'Pacific Standard Time').");
        public static ClientExceptionType FEATURE_DISABLED = new ClientExceptionType(eResponseStatus.FeatureDisabled, "Feature Disabled", "This feature is disabled.");
        public static ClientExceptionType ANNOUNCEMENT_MESSAGE_TOO_LONG = new ClientExceptionType(eResponseStatus.AnnouncementMessageTooLong, "Announcement Message Too Long", "The announcement message exceeds the permitted message length.");
        public static ClientExceptionType FAIL_CREATE_ANNOUNCEMENT = new ClientExceptionType(eResponseStatus.FailCreateAnnouncement, "Fail Create Announcement", "An error occurred while creating the announcement.");
        public static ClientExceptionType USER_NOT_FOLLOWING = new ClientExceptionType(eResponseStatus.UserNotFollowing, "User Not Following", "The user is not following this series.");
        public static ClientExceptionType USER_ALREADY_FOLLOWING = new ClientExceptionType(eResponseStatus.UserAlreadyFollowing, "User Already Following", "The user is already following the requested series.");
        public static ClientExceptionType MESSAGE_PLACEHOLDERS_INVALID = new ClientExceptionType(eResponseStatus.MessagePlaceholdersInvalid, "Message Placeholders Invalid", "The message placeholder is invalid.");
        public static ClientExceptionType DATETIME_FORMAT_IS_INVALID = new ClientExceptionType(eResponseStatus.DatetimeFormatIsInvalid, "Datetime Format Is Invalid", "The message date-time format Is Invalid.");
        public static ClientExceptionType MESSAGE_TEMPLATE_NOT_FOUND = new ClientExceptionType(eResponseStatus.MessageTemplateNotFound, "Message Template Not Found", "Unable to find the message template.");
        public static ClientExceptionType URLPLACEHOLDERS_INVALID = new ClientExceptionType(eResponseStatus.URLPlaceholdersInvalid, "URLPlaceholders Invalid", "The URL placeholder specified is invalid.");
        public static ClientExceptionType INVALID_MESSAGE_TTL = new ClientExceptionType(eResponseStatus.InvalidMessageTTL, "Invalid Message TTL", "Invalid message TTL");
        public static ClientExceptionType MESSAGE_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.MessageIdentifierRequired, "Message Identifier Required", "The mandatory message ID field is missing in the request.");
        public static ClientExceptionType USER_INBOX_MESSAGES_NOT_EXIST = new ClientExceptionType(eResponseStatus.UserInboxMessagesNotExist, "User Inbox Messages Not Exist", "Requested inbox message was not found.");
        public static ClientExceptionType FAIL_CREATE_TOPIC_NOTIFICATION = new ClientExceptionType(eResponseStatus.FailCreateTopicNotification, "Fail to create topic notification", "Fail to create topic notification");
        public static ClientExceptionType TOPIC_NOTIFICATION_NOT_FOUND = new ClientExceptionType(eResponseStatus.TopicNotificationNotFound, "Topic Notification Not Found", "The topic notification requested couldn't be found.");
        public static ClientExceptionType TOPIC_NOTIFICATION_MESSAGE_NOT_FOUND = new ClientExceptionType(eResponseStatus.TopicNotificationMessageNotFound, "Topic Notification Message Not Found", "The topic notification requested couldn't be found.");
        public static ClientExceptionType WRONG_TOPIC_NOTIFICATION = new ClientExceptionType(eResponseStatus.WrongTopicNotification, "Wrong Topic Notification Identifier", "Wrong Topic Notification Identifier");
        public static ClientExceptionType WRONG_TOPIC_NOTIFICATION_TRIGGER = new ClientExceptionType(eResponseStatus.WrongTopicNotificationTrigger, "Wrong Topic Notification trigger", "Wrong Topic Notification trigger");

        #endregion

        #region Pricing 9000-9999

        public static ClientExceptionType INVALID_PRICE_CODE = new ClientExceptionType(eResponseStatus.InvalidPriceCode, "Invalid Price Code", "Invalid price code: The price code entered doesn't exist for this account");
        public static ClientExceptionType INVALID_VALUE = new ClientExceptionType(eResponseStatus.InvalidValue, "Invalid Value", "The value specified, such as FullLifeCycle/ ViewLifeCycle, is invalid");
        public static ClientExceptionType INVALID_DISCOUNT_CODE = new ClientExceptionType(eResponseStatus.InvalidDiscountCode, "Invalid Discount Code", "Invalid discount code: The discount code entered doesn't exist for this account");
        public static ClientExceptionType INVALID_PRICE_PLAN = new ClientExceptionType(eResponseStatus.InvalidPricePlan, "Invalid Price Plan", "Invalid price plan: The price plan entered isn't in use with this account");
        public static ClientExceptionType CODE_MUST_BE_UNIQUE = new ClientExceptionType(eResponseStatus.CodeMustBeUnique, "Code Must Be Unique", "The billing code entered must be unique");
        public static ClientExceptionType CODE_NOT_EXIST = new ClientExceptionType(eResponseStatus.CodeNotExist, "Code Not Exist", "The billing code entered doesn't exist");
        public static ClientExceptionType INVALID_CODE_NOT_EXIST = new ClientExceptionType(eResponseStatus.InvalidCodeNotExist, "Invalid Code Not Exist", "The code entered is invalid");
        public static ClientExceptionType INVALID_CHANNELS = new ClientExceptionType(eResponseStatus.InvalidChannels, "Invalid Channels", "Invalid channel: This channel doesn't exist in this account");
        public static ClientExceptionType INVALID_FILE_TYPES = new ClientExceptionType(eResponseStatus.InvalidFileTypes, "Invalid File Types", "Invalid file type: This file type doesn't exist for this account");
        public static ClientExceptionType INVALID_PREVIEW_MODULE = new ClientExceptionType(eResponseStatus.InvalidPreviewModule, "Invalid Preview Module", "Invalid preview module: The preview module doesn't exist in this account");
        public static ClientExceptionType MANDATORY_FIELD = new ClientExceptionType(eResponseStatus.MandatoryField, "Mandatory Field", "Mandatory fields in a request must be completed");
        public static ClientExceptionType UNIQUE_FILED = new ClientExceptionType(eResponseStatus.UniqueFiled, "Unique Filed");
        public static ClientExceptionType INVALID_USAGE_MODULE = new ClientExceptionType(eResponseStatus.InvalidUsageModule, "Invalid Usage Module", "The usage module specified related to PPV doesn't exist in this account");
        public static ClientExceptionType INVALID_COUPON_GROUP = new ClientExceptionType(eResponseStatus.InvalidCouponGroup, "Invalid Coupon Group", "Invalid coupon group: The coupon group specified doesn't exist in this account");
        public static ClientExceptionType INVALID_CURRENCY = new ClientExceptionType(eResponseStatus.InvalidCurrency, "Invalid Currency", "Invalid currency: The currency specified is not configured for this account");
        public static ClientExceptionType MODULE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.ModuleNotExists, "Module Not Exists", "The PPV module doesn't exist in the database");
        public static ClientExceptionType PRICE_PLAN_NOT_EXISTS = new ClientExceptionType(eResponseStatus.PricePlanDoesNotExist, "Price plan does not exist", "The price plan doesn't exist in the database");
        public static ClientExceptionType PRICE_DETAILS_NOT_EXISTS = new ClientExceptionType(eResponseStatus.PriceDetailsDoesNotExist, "Price details does not exist", "The price details doesn't exist in the database");
        public static ClientExceptionType COUPON_CODE_IS_MISSING = new ClientExceptionType(eResponseStatus.CouponCodeIsMissing, "Coupon code is missing", "Coupon code is missing");
        public static ClientExceptionType COUPON_CODE_ALREADY_LOADED = new ClientExceptionType(eResponseStatus.CouponCodeAlreadyLoaded, "Coupon code already loaded", "Coupon code already loaded");
        public static ClientExceptionType COUPON_CODE_NOT_IN_HOUSEHOLD = new ClientExceptionType(eResponseStatus.CouponCodeNotInHousehold, "The coupon code is not in household", "The coupon code is not in household");
        public static ClientExceptionType EXCEEDED_HOUSEHOLD_COUPON_LIMIT = new ClientExceptionType(eResponseStatus.ExceededHouseholdCouponLimit, "Exceeded household coupon limit", "Exceeded household coupon limit");

        #endregion

        #region Adapters 10000-10999

        public static ClientExceptionType ADAPTER_NOT_EXISTS = new ClientExceptionType(eResponseStatus.AdapterNotExists, "Adapter Not Exists", "The adapter you're trying to connect doesn't exist");
        public static ClientExceptionType ADAPTER_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.AdapterIdentifierRequired, "Adapter Identifier Required", "The mandatory adapter ID field is missing from the request");
        public static ClientExceptionType ADAPTER_IS_REQUIRED = new ClientExceptionType(eResponseStatus.AdapterIsRequired, "Adapter Is Required");
        public static ClientExceptionType NO_ADAPTER_TO_INSERT = new ClientExceptionType(eResponseStatus.NoAdapterToInsert, "No Adapter To Insert");
        public static ClientExceptionType CAN_NOT_DELETE_DEFAULT_ADAPTER = new ClientExceptionType(eResponseStatus.CanNotDeleteDefaultAdapter, "Can not delete default adapter");

        #endregion

        #region  Ingest 11000-11999

        public static ClientExceptionType ILLEGAL_XML = new ClientExceptionType(eResponseStatus.IllegalXml, "Illegal XML", "The XML is formatted incorrectly. Please check the file for format errors");
        public static ClientExceptionType MISSING_EXTERNAL_IDENTIFIER = new ClientExceptionType(eResponseStatus.MissingExternalIdentifier, "Missing External Identifier", "The external ID is missing");
        public static ClientExceptionType UNKNOWN_INGEST_TYPE = new ClientExceptionType(eResponseStatus.UnknownIngestType, "Unknown Ingest Type", "The Ingest type is not known");
        public static ClientExceptionType EPG_PROGRAM_DATES_ERROR = new ClientExceptionType(eResponseStatus.EPGSProgramDatesError, "EPG Program Dates Error", "The EPG program dates specified are incorrectly formatted");
        public static ClientExceptionType INGEST_PROFILE_NOT_EXISTS = new ClientExceptionType(eResponseStatus.IngestProfileNotExists, "Ingest profile does not exist");
        public static ClientExceptionType INGEST_PROFILE_REQUIRED = new ClientExceptionType(eResponseStatus.IngestProfileIdRequired, "Ingest profile id is mandatory");
        public static ClientExceptionType NO_INGEsT_PROFILE_TO_INSERT = new ClientExceptionType(eResponseStatus.NoIngestProfileToInsert, "No Ingest Profile found to insert");
        public static ClientExceptionType EPG_PROGRAM_LANG_NOT_EXISTS = new ClientExceptionType(eResponseStatus.EPGLanguageNotFound, "EPG Program required language not found ", "The EPG program does not have the required language or the required default language as a fallback option");
        public static ClientExceptionType EPG_PROGRAM_OVERLAP_FIXED = new ClientExceptionType(eResponseStatus.EPGProgramOverlapFixed, "EPGProgramOverlapFixed");

        #endregion

        #region else

        //public static ClientExceptionType ERROR = new ClientExceptionType(eResponseStatus.Error, "Error");
        public static ClientExceptionType DLM_EXIST = new ClientExceptionType(eResponseStatus.DlmExist, "DlmExist");
        public static ClientExceptionType MASTER_USER_NOT_FOUND = new ClientExceptionType(eResponseStatus.MasterUserNotFound, "MasterUserNotFound");
        public static ClientExceptionType PASSWORD_POLICY_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.PasswordPolicyDoesNotExist, "PasswordPolicyDoesNotExist");
        public static ClientExceptionType PASSWORD_POLICY_VIOLATION = new ClientExceptionType(eResponseStatus.PasswordPolicyViolation, "PasswordPolicyViolation");
        public static ClientExceptionType PASSWORD_EXPIRED = new ClientExceptionType(eResponseStatus.PasswordExpired, "PasswordExpired");
        public static ClientExceptionType PASSWORD_CANNOT_BE_REUSED = new ClientExceptionType(eResponseStatus.PasswordCannotBeReused, "PasswordCannotBeReused");
        public static ClientExceptionType INVALID_PASSWORD_COMPLEXITY = new ClientExceptionType(eResponseStatus.InvalidPasswordComplexity, "InvalidPasswordComplexity");
        public static ClientExceptionType INTERNAL_CONNECTION_ISSUE = new ClientExceptionType(eResponseStatus.InternalConnectionIssue, "InternalConnectionIssue");

        public static ClientExceptionType GENERATE_NEW_LOGIN_PIN = new ClientExceptionType(eResponseStatus.GenerateNewLoginPIN, "GenerateNewLoginPIN");
        public static ClientExceptionType NOT_ACTIVATED = new ClientExceptionType(eResponseStatus.NotActivated, "NotActivated");
        public static ClientExceptionType USER_INTEREST_NOT_EXIST = new ClientExceptionType(eResponseStatus.UserInterestNotExist, "UserInterestNotExist");
        public static ClientExceptionType USER_INTEREST_ALREADY_EXIST = new ClientExceptionType(eResponseStatus.UserInterestAlreadyExist, "UserInterestAlreadyExist");
        public static ClientExceptionType NO_USER_INTEREST_TO_INSERT = new ClientExceptionType(eResponseStatus.NoUserInterestToInsert, "NoUserInterestToInsert");
        public static ClientExceptionType META_ID_REQUIRED = new ClientExceptionType(eResponseStatus.MetaIdRequired, "MetaIdRequired");
        public static ClientExceptionType META_VALUE_REQUIRED = new ClientExceptionType(eResponseStatus.MetaValueRequired, "MetaValueRequired");
        public static ClientExceptionType TOPIC_NOT_FOUND = new ClientExceptionType(eResponseStatus.TopicNotFound, "TopicNotFound");
        public static ClientExceptionType PARENT_DUPLICATE_ASSOCIATION = new ClientExceptionType(eResponseStatus.ParentDuplicateAssociation, "ParentDuplicateAssociation");
        public static ClientExceptionType META_NOT_A_USER_INTEREST = new ClientExceptionType(eResponseStatus.MetaNotAUserinterest, "MetaNotAUserinterest");
        public static ClientExceptionType PARENT_ID_NOT_A_USER_INTEREST = new ClientExceptionType(eResponseStatus.ParentIdNotAUserInterest, "ParentIdNotAUserInterest");
        public static ClientExceptionType PARENT_ASSET_TYPE_DIFFRENT_FROM_META = new ClientExceptionType(eResponseStatus.ParentAssetTypeDiffrentFromMeta, "ParentAssetTypeDiffrentFromMeta");
        public static ClientExceptionType META_NOT_FOUND = new ClientExceptionType(eResponseStatus.MetaNotFound, "MetaNotFound");
        public static ClientExceptionType META_NOT_BELONG_TO_PARTNER = new ClientExceptionType(eResponseStatus.MetaNotBelongtoPartner, "MetaNotBelongtoPartner");
        public static ClientExceptionType WRONG_META_NAME = new ClientExceptionType(eResponseStatus.WrongMetaName, "WrongMetaName");
        public static ClientExceptionType PARENT_PARNER_DIFFRENT_FROM_META_PARTNER = new ClientExceptionType(eResponseStatus.ParentParnerDiffrentFromMetaPartner, "ParentParnerDiffrentFromMetaPartner");
        public static ClientExceptionType PARTNER_TOPIC_INTEREST_IS_MISSING = new ClientExceptionType(eResponseStatus.PartnerTopicInterestIsMissing, "PartnerTopicInterestIsMissing");
        public static ClientExceptionType PARENT_TOPIC_IS_REQUIRED = new ClientExceptionType(eResponseStatus.ParentTopicIsRequired, "ParentTopicIsRequired");
        public static ClientExceptionType PARENT_TOPIC_SHOULD_NOT_HAVE_VALUE = new ClientExceptionType(eResponseStatus.ParentTopicShouldNotHaveValue, "ParentTopicShouldNotHaveValue");
        public static ClientExceptionType PARENT_TOPIC_META_ID_NOT_EQUAL_TO_META_PARENT_META_ID = new ClientExceptionType(eResponseStatus.ParentTopicMetaIdNotEqualToMetaParentMetaID, "ParentTopicMetaIdNotEqualToMetaParentMetaID");
        public static ClientExceptionType PARENT_TOPIC_VALUE_IS_MISSING = new ClientExceptionType(eResponseStatus.ParentTopicValueIsMissing, "ParentTopicValueIsMissing");
        public static ClientExceptionType SSO_ADAPTER_NOT_EXIST = new ClientExceptionType(eResponseStatus.SSOAdapterNotExist, "SSOAdapterNotExist");
        public static ClientExceptionType NO_SSO_ADAPATER_TO_INSERT = new ClientExceptionType(eResponseStatus.NoSSOAdapaterToInsert, "NoSSOAdapaterToInsert");
        public static ClientExceptionType SSO_ADAPTER_ID_REQUIRED = new ClientExceptionType(eResponseStatus.SSOAdapterIdRequired, "SSOAdapterIdRequired");

        public static ClientExceptionType SERVICE_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.ServiceAlreadyExists, "ServiceAlreadyExists");
        public static ClientExceptionType NO_FILES_FOUND = new ClientExceptionType(eResponseStatus.NoFilesFound, "NoFilesFound");
        public static ClientExceptionType COMPENSATION_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.CompensationAlreadyExists, "CompensationAlreadyExists");
        public static ClientExceptionType COMPENSATION_NOT_FOUND = new ClientExceptionType(eResponseStatus.CompensationNotFound, "CompensationNotFound");
        public static ClientExceptionType COUPON_PROMOTION_DATE_EXPIRED = new ClientExceptionType(eResponseStatus.CouponPromotionDateExpired, "CouponPromotionDateExpired");
        public static ClientExceptionType COUPON_PROMOTION_DATE_NOT_STARTED = new ClientExceptionType(eResponseStatus.CouponPromotionDateNotStarted, "CouponPromotionDateNotStarted");
        public static ClientExceptionType SUBSCRIPTION_ALREADY_BELONGS_TO_ANOTHER_SUBSCRIPTION_SET = new ClientExceptionType(eResponseStatus.SubscriptionAlreadyBelongsToAnotherSubscriptionSet, "SubscriptionAlreadyBelongsToAnotherSubscriptionSet");
        //public static ClientExceptionType CAN_ONLY_BEEN_TITLED_TO_ONE_SUBSCRIPTION_PER_SUBSCRIPTION_SET = new ClientExceptionType(eResponseStatus.CanOnlyBeEntitledToOneSubscriptionPerSubscriptionSet, "CanOnlyBeEntitledToOneSubscriptionPerSubscriptionSet");
        public static ClientExceptionType CAN_ONLY_UPGRADE_OR_DOWNGRADE_RECURRING_SUBSCRIPTION_IN_THE_SAME_SUBSCRIPTION_SET = new ClientExceptionType(eResponseStatus.CanOnlyUpgradeOrDowngradeRecurringSubscriptionInTheSameSubscriptionSet, "CanOnlyUpgradeOrDowngradeRecurringSubscriptionInTheSameSubscriptionSet");
        public static ClientExceptionType CAN_ONLY_UPGRADE_SUBSCRIPTION_WITH_HIGHER_PRIORITY = new ClientExceptionType(eResponseStatus.CanOnlyUpgradeSubscriptionWithHigherPriority, "CanOnlyUpgradeSubscriptionWithHigherPriority");
        public static ClientExceptionType CAN_ONLY_DOWNGRADE_SUBSCRIPTION_WITH_LOWERP_RIORITY = new ClientExceptionType(eResponseStatus.CanOnlyDowngradeSubscriptionWithLowerPriority, "CanOnlyDowngradeSubscriptionWithLowerPriority");
        public static ClientExceptionType CAN_ONLY_UPGRADE_OR_DOWNGRADE_SUBSCRIPTION_ONCE = new ClientExceptionType(eResponseStatus.CanOnlyUpgradeOrDowngradeSubscriptionOnce, "CanOnlyUpgradeOrDowngradeSubscriptionOnce");
        public static ClientExceptionType CAN_ONLY_UPGRADE_SUBSCRIPTION_WITH_THE_SAME_CURRENCY_AS_CURRENT_SUBSCRIPTION = new ClientExceptionType(eResponseStatus.CanOnlyUpgradeSubscriptionWithTheSameCurrencyAsCurrentSubscription, "CanOnlyUpgradeSubscriptionWithTheSameCurrencyAsCurrentSubscription");
        public static ClientExceptionType SCHEDULED_SUBSCRIPTION_NOT_FOUND = new ClientExceptionType(eResponseStatus.ScheduledSubscriptionNotFound, "ScheduledSubscriptionNotFound");
        public static ClientExceptionType CAN_NOT_CANCEL_SUBSCRIPTION_WHILE_DOWNGRADE_IS_PENDING = new ClientExceptionType(eResponseStatus.CanNotCancelSubscriptionWhileDowngradeIsPending, "CanNotCancelSubscriptionWhileDowngradeIsPending");
        public static ClientExceptionType CAN_NOT_CANCEL_SUBSCRIPTION_RENEWAL_WHILE_DOWNGRADE_IS_PENDING = new ClientExceptionType(eResponseStatus.CanNotCancelSubscriptionRenewalWhileDowngradeIsPending, "CanNotCancelSubscriptionRenewalWhileDowngradeIsPending");
        public static ClientExceptionType BASE_SUBSCRIPTION_ALREADY_BELONGS_TO_ANOTHER_SUBSCRIPTION_SET = new ClientExceptionType(eResponseStatus.BaseSubscriptionAlreadyBelongsToAnotherSubscriptionSet, "BaseSubscriptionAlreadyBelongsToAnotherSubscriptionSet");
        public static ClientExceptionType WRONG_SUBSCRIPTION_TYPE = new ClientExceptionType(eResponseStatus.WrongSubscriptionType, "WrongSubscriptionType");
        public static ClientExceptionType INVALID_PRODUCT_TYPE = new ClientExceptionType(eResponseStatus.InvalidProductType, "InvalidProductType");
        public static ClientExceptionType NETWORK_RULE_BLOCK = new ClientExceptionType(eResponseStatus.NetworkRuleBlock, "NetworkRuleBlock");
        public static ClientExceptionType RECORDING_IDS_EXCEEDED_LIMIT = new ClientExceptionType(eResponseStatus.RecordingIdsExceededLimit, "RecordingIdsExceededLimit");

        public static ClientExceptionType ELASTIC_SEARCH_RETURNED_DELETE_ITEM = new ClientExceptionType(eResponseStatus.ElasticSearchReturnedDeleteItem, "ElasticSearchReturnedDeleteItem");
        public static ClientExceptionType ELASTIC_SEARCH_RETURNED_UNUPDATED_ITEM = new ClientExceptionType(eResponseStatus.ElasticSearchReturnedUnupdatedItem, "ElasticSearchReturnedUnupdatedItem");
        public static ClientExceptionType MISSING_BASIC_VALUE_FOR_ASSET = new ClientExceptionType(eResponseStatus.MissingBasicValueForAsset, "MissingBasicValueForAsset");
        public static ClientExceptionType CAN_NOT_DELETE_CONNECTING_ASSET_STRUCT_META = new ClientExceptionType(eResponseStatus.CanNotDeleteConnectingAssetStructMeta, "CanNotDeleteConnectingAssetStructMeta");
        public static ClientExceptionType NO_PARENT_ASSOCIATED_TO_TOPIC = new ClientExceptionType(eResponseStatus.NoParentAssociatedToTopic, "NoParentAssociatedToTopic");
        public static ClientExceptionType WRONG_PARENT_ASSOCIATION = new ClientExceptionType(eResponseStatus.WrongParentAssociation, "WrongParentAssociation");
        public static ClientExceptionType META_DOES_NOT_BELONG_TO_PARENT_ASSET_STRUCT = new ClientExceptionType(eResponseStatus.MetaDoesNotBelongToParentAssetStruct, "MetaDoesNotBelongToParentAssetStruct");
        public static ClientExceptionType META_IDS_DUPLICATION = new ClientExceptionType(eResponseStatus.MetaIdsDuplication, "MetaIdsDuplication");
        public static ClientExceptionType CAN_NOT_REMOVE_META_IDS_FOR_LIVE_TO_VOD = new ClientExceptionType(eResponseStatus.CanNotRemoveMetaIdsForLiveToVod, "CanNotRemoveMetaIdsForLiveToVod");
        public static ClientExceptionType L2V_METADATA_CLASSIFIER_IS_NOT_VALID = new ClientExceptionType(eResponseStatus.L2VMetadataClassifierIsNotValid, "Program asset struct doesn't have boolean meta with provided name.");
        public static ClientExceptionType ASSET_FILE_PPV_NOT_EXIST = new ClientExceptionType(eResponseStatus.AssetFilePPVNotExist, "AssetFilePPVNotExist");
        public static ClientExceptionType GROUP_DOES_NOT_CONTAIN_CURRENCY = new ClientExceptionType(eResponseStatus.GroupDoesNotContainCurrency, "GroupDoesNotContainCurrency");
        public static ClientExceptionType NO_VALUES_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoValuesToUpdate, "NoValuesToUpdate");
        public static ClientExceptionType IMAGE_URL_REQUIRED = new ClientExceptionType(eResponseStatus.ImageUrlRequired, "ImageUrlRequired");
        public static ClientExceptionType CATEGORY_VERSION_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.CategoryVersionDoesNotExist, "CategoryVersionDoesNotExist");
        public static ClientExceptionType CATEGORY_VERSION_IS_NOT_DRAFT = new ClientExceptionType(eResponseStatus.CategoryVersionIsNotDraft, "CategoryVersionIsNotDraft");
        public static ClientExceptionType CATEGORY_IS_ALREADY_ASSOCIATED_TO_VERSION = new ClientExceptionType(eResponseStatus.CategoryIsAlreadyAssociatedToVersion, "CategoryIsAlreadyAssociatedToVersion");
        public static ClientExceptionType CATEGORY_IS_NOT_ROOT = new ClientExceptionType(eResponseStatus.CategoryIsNotRoot, "CategoryIsNotRoot");
        public static ClientExceptionType CATEGORY_TREE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.CategoryTreeDoesNotExist, "CategoryTreeDoesNotExist");
        public static ClientExceptionType CATEGORY_ITEM_IS_ROOT = new ClientExceptionType(eResponseStatus.CategoryItemIsRoot, "CategoryItemIsRoot");
        public static ClientExceptionType CATEGORY_VERSION_IS_OLDER_THAN_DEFAULT = new ClientExceptionType(eResponseStatus.CategoryVersionIsOlderThanDefault, "CategoryVersionIsOlderThanDefault");
        public static ClientExceptionType CATEGORY_IS_ALREADY_ASSOCIATED_TO_VERSION_TREE = new ClientExceptionType(eResponseStatus.CategoryIsAlreadyAssociatedToVersionTree, "CategoryIsAlreadyAssociatedToVersionTree");

        public static ClientExceptionType NO_META_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoMetaToUpdate, "NoMetaToUpdate");
        public static ClientExceptionType NOT_A_TOPIC_INTEREST_META = new ClientExceptionType(eResponseStatus.NotaTopicInterestMeta, "NotaTopicInterestMeta");
        public static ClientExceptionType ROLE_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.RoleAlreadyExists, "RoleAlreadyExists");
        public static ClientExceptionType NON_EXISTING_DEVICE_FAMILY_IDS = new ClientExceptionType(eResponseStatus.NonExistingDeviceFamilyIds, "NonExistingDeviceFamilyIds");
        public static ClientExceptionType PARENTAL_RULE_NAME_ALREADY_IN_USE = new ClientExceptionType(eResponseStatus.ParentalRuleNameAlreadyInUse, "ParentalRuleNameAlreadyInUse");
        public static ClientExceptionType PARENTAL_RULE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.ParentalRuleDoesNotExist, "ParentalRuleDoesNotExist");
        public static ClientExceptionType CAN_NOT_DELETE_DEFAULT_PARENTAL_RULE = new ClientExceptionType(eResponseStatus.CanNotDeleteDefaultParentalRule, "CanNotDeleteDefaultParentalRule");
        public static ClientExceptionType INVALID_LANGUAGE = new ClientExceptionType(eResponseStatus.InvalidLanguage, "InvalidLanguage");
        public static ClientExceptionType PARTNER_CONFIGURATION_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.PartnerConfigurationDoesNotExist, "PartnerConfigurationDoesNotExist");

        public static ClientExceptionType PAYMENT_GATEWAY_EXTERNAL_VERIFICATION = new ClientExceptionType(eResponseStatus.PaymentGatewayExternalVerification, "PaymentGatewayExternalVerification");

        public static ClientExceptionType NO_USER_SOCIAL_SETTINGS_FOUND = new ClientExceptionType(eResponseStatus.NoUserSocialSettingsFound, "NoUserSocialSettingsFound");
        public static ClientExceptionType ASSET_ALREADY_LIKED = new ClientExceptionType(eResponseStatus.AssetAlreadyLiked, "AssetAlreadyLiked");
        public static ClientExceptionType SOCIAL_ACTION_PRIVACY_DONT_ALLOW = new ClientExceptionType(eResponseStatus.SocialActionPrivacyDontAllow, "SocialActionPrivacyDontAllow");
        public static ClientExceptionType EMPTY_FACEBOOK_OBJECT_ID = new ClientExceptionType(eResponseStatus.EmptyFacebookObjectId, "EmptyFacebookObjectId");
        public static ClientExceptionType UNKNOWN_ACTION = new ClientExceptionType(eResponseStatus.UnknownAction, "UnknownAction");
        public static ClientExceptionType INVALID_ACCESS_TOKEN = new ClientExceptionType(eResponseStatus.InvalidAccessToken, "InvalidAccessToken");
        public static ClientExceptionType INVALID_PLATFORM_REQUEST = new ClientExceptionType(eResponseStatus.InvalidPlatformRequest, "InvalidPlatformRequest");
        public static ClientExceptionType ASSET_DOES_NOT_EXISTS = new ClientExceptionType(eResponseStatus.AssetDoseNotExists, "AssetDoseNotExists");
        public static ClientExceptionType USER_DOES_NOT_EXISTS = new ClientExceptionType(eResponseStatus.UserDoseNotExists, "UserDoseNotExists");
        public static ClientExceptionType NO_FACEBOOK_ACTION = new ClientExceptionType(eResponseStatus.NoFacebookAction, "NoFacebookAction");
        public static ClientExceptionType NOT_ALLOWED = new ClientExceptionType(eResponseStatus.NotAllowed, eResponseStatus.NotAllowed.ToString(), "Action not allowed due to roleId [@roleId@]");
        public static ClientExceptionType ASSET_ALREADY_RATED = new ClientExceptionType(eResponseStatus.AssetAlreadyRated, "AssetAlreadyRated");
        public static ClientExceptionType ASSET_NEVER_LIKED = new ClientExceptionType(eResponseStatus.AssetNeverLiked, "AssetNeverLiked");
        public static ClientExceptionType SOCIAL_ACTION_ID_DOES_NOT_EXISTS = new ClientExceptionType(eResponseStatus.SocialActionIdDoseNotExists, "SocialActionIdDoseNotExists");
        public static ClientExceptionType USER_EMAIL_IS_MISSING = new ClientExceptionType(eResponseStatus.UserEmailIsMissing, "UserEmailIsMissing");

        //public static ClientExceptionType URL_PLACEHOLDERS_INVALID = new ClientExceptionType(eResponseStatus.URLPlaceholdersInvalid, "URLPlaceholdersInvalid");
        public static ClientExceptionType INVALID_REMINDER_PRE_PADDING_SEC = new ClientExceptionType(eResponseStatus.InvalidReminderPrePaddingSec, "InvalidReminderPrePaddingSec");
        public static ClientExceptionType REMINDER_NOT_FOUND = new ClientExceptionType(eResponseStatus.ReminderNotFound, "ReminderNotFound");
        public static ClientExceptionType USER_ALREADY_SET_REMINDER = new ClientExceptionType(eResponseStatus.UserAlreadySetReminder, "UserAlreadySetReminder");
        public static ClientExceptionType PASSED_ASSET = new ClientExceptionType(eResponseStatus.PassedAsset, "PassedAsset");
        public static ClientExceptionType ENGAGEMENT_ADAPTER_IDENTIFIER_REQUIRED = new ClientExceptionType(eResponseStatus.EngagementAdapterIdentifierRequired, "EngagementAdapterIdentifierRequired");
        public static ClientExceptionType ENGAGEMENT_ADAPTER_NOT_EXIST = new ClientExceptionType(eResponseStatus.EngagementAdapterNotExist, "EngagementAdapterNotExist");
        public static ClientExceptionType ENGAGEMENT_ADAPTER_PARAMS_REQUIRED = new ClientExceptionType(eResponseStatus.EngagementAdapterParamsRequired, "EngagementAdapterParamsRequired");
        public static ClientExceptionType NO_ENGAGEMENT_ADAPTER_TO_INSERT = new ClientExceptionType(eResponseStatus.NoEngagementAdapterToInsert, "NoEngagementAdapterToInsert");
        public static ClientExceptionType NO_ENGAGEMENT_ADAPTER_TO_UPDATE = new ClientExceptionType(eResponseStatus.NoEngagementAdapterToUpdate, "NoEngagementAdapterToUpdate");
        public static ClientExceptionType NO_ENGAGEMENT_TO_INSERT = new ClientExceptionType(eResponseStatus.NoEngagementToInsert, "NoEngagementToInsert");
        public static ClientExceptionType ENGAGEMENT_REQUIRED = new ClientExceptionType(eResponseStatus.EngagementRequired, "EngagementRequired");
        public static ClientExceptionType ENGAGEMENT_NOT_EXIST = new ClientExceptionType(eResponseStatus.EngagementNotExist, "EngagementNotExist");
        public static ClientExceptionType PROVIDER_URL_REQUIRED = new ClientExceptionType(eResponseStatus.ProviderUrlRequired, "ProviderUrlRequired");
        public static ClientExceptionType ENGAGEMENT_TIME_DIFFERENCE = new ClientExceptionType(eResponseStatus.EngagementTimeDifference, "EngagementTimeDifference");
        public static ClientExceptionType ENGAGEMENT_ILLEGAL_SEND_TIME = new ClientExceptionType(eResponseStatus.EngagementIllegalSendTime, "EngagementIllegalSendTime");
        public static ClientExceptionType FUTURE_SCHEDULED_ENGAGEMENT_DETECTED = new ClientExceptionType(eResponseStatus.FutureScheduledEngagementDetected, "FutureScheduledEngagementDetected");
        public static ClientExceptionType ENGAGEMENT_TEMPLATE_NOT_FOUND = new ClientExceptionType(eResponseStatus.EngagementTemplateNotFound, "EngagementTemplateNotFound");
        public static ClientExceptionType ENGAGEMENT_SCHEDULE_WITHOUT_ADAPTER = new ClientExceptionType(eResponseStatus.EngagementScheduleWithoutAdapter, "EngagementScheduleWithoutAdapter");
        public static ClientExceptionType MAIL_NOTIFICATION_ADAPTER_NOT_EXIST = new ClientExceptionType(eResponseStatus.MailNotificationAdapterNotExist, "MailNotificationAdapterNotExist");
        public static ClientExceptionType INVALID_TOKEN = new ClientExceptionType(eResponseStatus.InvalidToken, "InvalidToken");
        public static ClientExceptionType INVALID_NOTIFICATION_SETTINGS_SETUP = new ClientExceptionType(eResponseStatus.InvalidNotificationSettingsSetup, "InvalidNotificationSettingsSetup");

        public static ClientExceptionType COUPON_CODE_ALREADY_EXISTS = new ClientExceptionType(eResponseStatus.CouponCodeAlreadyExists, "CouponCodeAlreadyExists");
        public static ClientExceptionType COUPON_GROUP_NOT_EXIST = new ClientExceptionType(eResponseStatus.CouponGroupNotExist, "CouponGroupNotExist");
        public static ClientExceptionType COUPON_CODE_NOT_IN_THE_RIGHT_LENGTH = new ClientExceptionType(eResponseStatus.CouponCodeNotInTheRightLength, "CouponCodeNotInTheRightLength");
        public static ClientExceptionType DISCOUNT_CODE_NOT_EXIST = new ClientExceptionType(eResponseStatus.DiscountCodeNotExist, "DiscountCodeNotExist");
        public static ClientExceptionType CAMPAIGN_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.CampaignDoesNotExist, "CampaignDoesNotExist");
        public static ClientExceptionType EXCEEDED_MAX_CAPACITY = new ClientExceptionType(eResponseStatus.ExceededMaxCapacity, "ExceededMaxCapacity");
        public static ClientExceptionType CAN_DELETE_ONLY_INACTIVE_CAMPAIGN = new ClientExceptionType(eResponseStatus.CanDeleteOnlyInactiveCampaign, "CanDeleteOnlyInactiveCampaign");
        public static ClientExceptionType DYNAMIC_LIST_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.DynamicListDoesNotExist, "DynamicListDoesNotExist");
        public static ClientExceptionType EXCEEDED_MAX_LENGTH = new ClientExceptionType(eResponseStatus.ExceededMaxLength, "ExceededMaxLength");
        public static ClientExceptionType PRICE_IS_MISSING = new ClientExceptionType(eResponseStatus.PriceIsMissing, "Price Is Missing");
        public static ClientExceptionType AMOUNT_IS_MISSING = new ClientExceptionType(eResponseStatus.AmountIsMissing, "Amount Is Missing");
        public static ClientExceptionType CURRENCY_IS_MISSING = new ClientExceptionType(eResponseStatus.CurrencyIsMissing, "Currency Is Missing");
        public static ClientExceptionType PRICECODE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.PriceCodeDoesNotExist, "PriceCode Does Not Exist");
        public static ClientExceptionType USAGEMODULE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.UsageModuleDoesNotExist, "UsageModule Does Not Exist");
        public static ClientExceptionType DRMADAPTER_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.DrmAdapterNotExist, "DrmAdapter Does Not Exist");
        public static ClientExceptionType PREVIEWMODULE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.PreviewModuleNotExist, "PreviewModule Does Not Exist");
        public static ClientExceptionType COLLECTION_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.CollectionNotExist, "The collection does not exist", "The collection does not exist");
        public static ClientExceptionType PARTNER_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.PartnerDoesNotExist, "Partner Does Not Exist");
        public static ClientExceptionType PPVMODULE_DOES_NOT_EXIST = new ClientExceptionType(eResponseStatus.PpvModuleNotExist, "PpvModule Does Not Exist");
        public static ClientExceptionType USAGEMODULE_EXIST_IN_PPV = new ClientExceptionType(eResponseStatus.UsageModuleExistInPpv, "UsageModule Exist In Ppv");

        public static ClientExceptionType FORBIDDEN = new ClientExceptionType(eResponseStatus.Forbidden, "Forbidden");
        public static ClientExceptionType ILLEGAL_QUERY_PARAMS = new ClientExceptionType(eResponseStatus.IllegalQueryParams, "IllegalQueryParams");
        public static ClientExceptionType ILLEGAL_POST_DATA = new ClientExceptionType(eResponseStatus.IllegalPostData, "IllegalPostData");
        public static ClientExceptionType NOT_EXIST = new ClientExceptionType(eResponseStatus.NotExist, "NotExist");
        public static ClientExceptionType PARTNER_MISMATCH = new ClientExceptionType(eResponseStatus.PartnerMismatch, "PartnerMismatch");
        public static ClientExceptionType ITEM_ALREADY_EXIST = new ClientExceptionType(eResponseStatus.ItemAlreadyExist, "ItemAlreadyExist");
        public static ClientExceptionType REGISTERED = new ClientExceptionType(eResponseStatus.Registered, "Registered");
        public static ClientExceptionType VERSION_NOT_FOUND = new ClientExceptionType(eResponseStatus.VersionNotFound, "VersionNotFound");
        public static ClientExceptionType ALREADY_EXIST = new ClientExceptionType(eResponseStatus.AlreadyExist, "AlreadyExist");

        public static ClientExceptionType FAILED_TO_DELETE_GROUP_CANARY_DEPLOYMENT_CONFIGURATION = new ClientExceptionType(eResponseStatus.FailedToDeleteGroupCanaryDeploymentConfiguration, "FailedToDeleteGroupCanaryDeploymentConfiguration");
        public static ClientExceptionType FAILED_TO_SET_ALL_GROUP_CANARY_DEPLOYMENT_MIGRATION_EVENTS_STATUS = new ClientExceptionType(eResponseStatus.FailedToSetAllGroupCanaryDeploymentMigrationEventsStatus, "FailedToSetAllGroupCanaryDeploymentMigrationEventsStatus");
        public static ClientExceptionType FAILED_TO_ENABLE_CANARY_DEPLOYMENT_MIGRATION_EVENT = new ClientExceptionType(eResponseStatus.FailedToEnableCanaryDeploymentMigrationEvent, "FailedToEnableCanaryDeploymentMigrationEvent");
        public static ClientExceptionType FAILED_TO_DISABLE_CANARY_DEPLOYMENT_MIGRATION_EVENT = new ClientExceptionType(eResponseStatus.FailedToDisableCanaryDeploymentMigrationEvent, "FailedToDisableCanaryDeploymentMigrationEvent");
        public static ClientExceptionType FAILED_TO_SET_ROUTE_APP_TOKEN_CONTROLLER = new ClientExceptionType(eResponseStatus.FailedToSetRouteAppTokenController, "FailedToSetRouteAppTokenController");
        public static ClientExceptionType FAILED_TO_SET_ROUTE_USER_LOGIN_PIN_CONTROLLER = new ClientExceptionType(eResponseStatus.FailedToSetRouteUserLoginPinController, "FailedToSetRouteUserLoginPinController");
        public static ClientExceptionType GROUP_CANARY_DEPLOYMENT_CONFIGURATION_NOT_SET_YET = new ClientExceptionType(eResponseStatus.GroupCanaryDeploymentConfigurationNotSetYet, "GroupCanaryDeploymentConfigurationNotSetYet");
        public static ClientExceptionType FAILED_TO_SET_ROUTE_SESSION_CONTROLLER = new ClientExceptionType(eResponseStatus.FailedToSetRouteSessionController, "FailedToSetRouteSessionController");
        public static ClientExceptionType FAILED_TO_SET_ROUTE_HOUSEHOLD_DEVICE_PIN_ACTIONS = new ClientExceptionType(eResponseStatus.FailedToSetRouteHouseHoldDevicePinActions, "FailedToSetRouteHouseHoldDevicePinActions");
        public static ClientExceptionType FAILED_TO_SET_ROUTE_REFRESH_TOKEN = new ClientExceptionType(eResponseStatus.FailedToSetRouteRefreshToken, "FailedToSetRouteRefreshToken");
        public static ClientExceptionType FAILED_TO_SET_ALL_ROUTING_ACTIONS = new ClientExceptionType(eResponseStatus.FailedToSetAllRoutingActions, "FailedToSetAllRoutingActions");
        public static ClientExceptionType CANARY_DEPLOYMENT_CONFIGURATION_IS_DISABLED_ON_THE_ENVIRONMENT = new ClientExceptionType(eResponseStatus.CanaryDeploymentConfigurationIsDisabledOnTheEnvironment, "CanaryDeploymentConfigurationIsDisabledOnTheEnvironment");

        #endregion
    }
}