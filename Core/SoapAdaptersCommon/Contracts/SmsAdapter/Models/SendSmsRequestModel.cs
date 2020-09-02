using AdapaterCommon.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SoapAdaptersCommon.Contracts.SmsAdapter.Models
{
    public class SendSmsRequestModel
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public List<KeyValue> AdapterData { get; set; }
    }
}
