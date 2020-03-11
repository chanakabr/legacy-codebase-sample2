namespace RestAdaptersCommon
{
    public class AdapterStatus
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return $"code:[{Code}], message:[{Message}]";
        }

        public static AdapterStatus SignatureMismatch => new AdapterStatus { Code = (int)eAdapterStatus.SignatureMismatch, Message = "Signature Mismatch" };
        public static AdapterStatus Ok => new AdapterStatus { Code = (int)eAdapterStatus.OK, Message = "" };
        public static AdapterStatus NoConfiguration => new AdapterStatus { Code = (int)eAdapterStatus.NoConfigurationFound, Message = "Configuration not found" };
        public static AdapterStatus GetErrorStatus(string msg) => new AdapterStatus { Code = (int)eAdapterStatus.Error, Message = msg };
    }
}
