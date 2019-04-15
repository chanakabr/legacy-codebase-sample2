namespace RestAdaptersCommon
{
    public class BaseAdapterResponse
    {
        public AdapterStatus ResponseStatus { get; set; }

        public static BaseAdapterResponse SignatureMismatch => new BaseAdapterResponse { ResponseStatus=AdapterStatus.SignatureMismatch };
        public static BaseAdapterResponse Ok => new BaseAdapterResponse { ResponseStatus = AdapterStatus.Ok };
        public static BaseAdapterResponse NoConfiguration => new BaseAdapterResponse { ResponseStatus = AdapterStatus.NoConfiguration };
        public static BaseAdapterResponse GetErrorStatus(string msg) => new BaseAdapterResponse { ResponseStatus = AdapterStatus.GetErrorStatus(msg) };


    }
}
