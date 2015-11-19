using System;

namespace ApiObjects
{
    public class MessageQueue
    {
        public int Id { get; set; }
        public string MessageData { get; set; }
        public string RoutingKey { get; set; }
        public DateTime ExecutionDate { get; set; }
        public string Type{ get; set; }


        public MessageQueue()
        {

        }
    }

}
