
namespace M1BL
{
    public enum M1ItemType
    {
        PPV = 1,
        Subscription = 2     
    }

    public enum M1TransactionStatus
    {
        Pending = 0,
        Success = 1,
        Fail = 2
    }

    public enum M1_API_ResponseReason
    {
        OK = 0,
        CAN_ACCESS_PRIMARY_REJECTION,
        CAN_ACCESS_EXECUTE_SERVICE_REJECTION,
        WRAPPER_NOT_INITIALIZED,
        USER_TYPE_UNKNOWN,
        CAN_ACCESS_VAS_SERVICE_FAILURE,
        USER_BLACKLISTED,
        SESSION_TOKEN_INVALID,
        SINGLESIGNON_SERVICE_TICKET_ERROR,
        CHECK_BLACK_LIST_ERROR,
        CREATE_DUMMY_VAS_ERROR,
        REMOVE_DUMMY_VAS_ERROR,
        GENERAL_ERROR
    }
}
