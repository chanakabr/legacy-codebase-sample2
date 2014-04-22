using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Users
{
    public class LimitationsManager
    {
       
        public int concurrency;
        public int quantity;
        /*
         * 1. Unlike quantity and concurrency, frequency is defined only at domain level (actually at group level in db.
         *    Have a look at TVinci.dbo.groups_device_limitation_modules)
         * 2. Quantity and concurrency are defined both at domain level and in device family level.
         * 3. This comment is correct to 22.04.14
         */ 
        public int frequency;
        public DateTime nextActionFreqDate;
                            
        //[DataMember]
        //public bool isHomeDevice;

        //public LimitationsManager(int concurrency, int quantity, int frequency/*, bool isHomeDevice*/)
        //{
        //    this.concurrency = concurrency;
        //    this.quantity = quantity;
        //    this.frequency = frequency;
        //    //this.isHomeDevice = isHomeDevice;
        //}

        public LimitationsManager(int concurrency, int quantity, int frequency, DateTime nextActionFreqDate)
        {
            this.concurrency = concurrency;
            this.quantity = quantity;
            this.frequency = frequency;
            this.nextActionFreqDate = nextActionFreqDate;
        }

        public LimitationsManager(int concurrency, int quantity, int frequency)
        {
            this.concurrency = concurrency;
            this.quantity = quantity;
            this.frequency = frequency;
            this.nextActionFreqDate = DateTime.MaxValue;
            //this.isHomeDevice = false;
        }

        public LimitationsManager()
        {
            this.concurrency = 0;
            this.quantity = 0;
            this.frequency = 0;
            this.nextActionFreqDate = DateTime.MinValue;
            //this.isHomeDevice = false;
        }

    }
}
