using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Users
{
    [DataContract]
    public class LimitationsManager
    {
        [DataMember]
        public int concurrency;
        [DataMember]
        public int quantity;
        [DataMember]
        public int frequency;
        [DataMember]
        private bool isHomeDevice;

        public LimitationsManager(int concurrency, int quantity, int frequency, bool isHomeDevice)
        {
            this.concurrency = concurrency;
            this.quantity = quantity;
            this.frequency = frequency;
            this.isHomeDevice = isHomeDevice;
        }

        public LimitationsManager(int concurrency, int quantity, int frequency)
        {
            this.concurrency = concurrency;
            this.quantity = quantity;
            this.frequency = frequency;
            this.isHomeDevice = false;
        }

        public LimitationsManager()
        {
            this.concurrency = 0;
            this.quantity = 0;
            this.frequency = 0;
            this.isHomeDevice = false;
        }

    }
}
