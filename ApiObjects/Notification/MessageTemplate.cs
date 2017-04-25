
namespace ApiObjects.Notification
{
    public class MessageTemplate
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string DateFormat { get; set; }
        public MessageTemplateType TemplateType { get; set; }
        public string Sound { get; set; }
        public string Action { get; set; }
        public string URL { get; set; }
    }
}
