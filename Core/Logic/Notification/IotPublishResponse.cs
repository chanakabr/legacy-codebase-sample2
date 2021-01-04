namespace ApiLogic.Notification
{
    public class IotPublishResponse
    {
        public int AdapterStatusCode { get; set; }
        public string AdapterStatusValue { get; set; }
        public ResponseObject ResponseObject { get; set; }
    }
    public class ResponseObject
    {
        public bool IsSuccess { get; set; }
        public object InternalMessageId { get; set; }
        public string Error { get; set; }
    }
}
