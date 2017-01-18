using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Users
{
    [DataContract]
    public class NetworkResponseObject
    {
        [DataMember]
        public bool bSuccess;
        [DataMember]
        public NetworkResponseStatus eReason;

        public NetworkResponseObject() { }

        public NetworkResponseObject(bool bSuccess, NetworkResponseStatus eReason)
        {
            this.bSuccess = bSuccess;
            this.eReason = eReason;
        }
    }
}