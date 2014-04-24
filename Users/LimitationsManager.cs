using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Users
{
    /*
     * Device limitation manager for Domain obj and DeviceContainer obj.
     * 
     * 
     */ 
    public class LimitationsManager
    {

        private int concurrency;
        private int quantity;
        /*
         * 1. Unlike quantity and concurrency, frequency is defined only at domain level (actually at group level in db.
         *    Have a look at TVinci.dbo.groups_device_limitation_modules)
         * 2. Quantity and concurrency are defined both at domain level and in device family level.
         * 3. This comment is correct to 22.04.14
         */ 
        private int frequency;
        private DateTime nextActionFreqDate;
                            

        public int Concurrency
        {
            get
            {
                return concurrency;
            }
            set
            {
                concurrency = value;
            }
        }

        public int Quantity
        {
            get
            {
                return quantity;
            }
            set
            {
                quantity = value;
            }
        }

        public int Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
            }
        }

        public DateTime NextActionFreqDate
        {
            get
            {
                return nextActionFreqDate;
            }
            set
            {
                nextActionFreqDate = value;
            }
        }

        public void SetConcurrency(int nConcurrencyDomainLevel, int nConcurrencyGroupLevel)
        {
            this.concurrency = nConcurrencyDomainLevel > 0 ? nConcurrencyDomainLevel : nConcurrencyGroupLevel;
        }


        private int GetActualConcurrency(int concurrencyGroupLevel, int concurrencyDomainLevel)
        {
            return concurrencyDomainLevel > 0 ? concurrencyDomainLevel : concurrencyGroupLevel;
        }

        public LimitationsManager(int concurrencyGroupLevel, int concurrencyDomainLevel, int quantity, int frequency, DateTime nextActionFreqDate)
        {
            this.concurrency = GetActualConcurrency(concurrencyGroupLevel, concurrencyDomainLevel);
            this.quantity = quantity;
            this.frequency = frequency;
            this.nextActionFreqDate = nextActionFreqDate;
        }

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
        }

        public LimitationsManager(int concurrencyGroupLevel, int concurrencyDomainLevel, int quantity, int frequency)
        {
            this.concurrency = GetActualConcurrency(concurrencyGroupLevel, concurrencyDomainLevel);
            this.quantity = quantity;
            this.frequency = frequency;
            this.nextActionFreqDate = DateTime.MaxValue;
        }

        public LimitationsManager()
        {
            this.concurrency = 0;
            this.quantity = 0;
            this.frequency = 0;
            this.nextActionFreqDate = DateTime.MinValue;
        }

    }
}
