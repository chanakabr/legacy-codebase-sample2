using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAL.DTO
{
    public class LimitationsManagerDTO
    {
        internal static readonly DateTime FICTIVE_DATE = new DateTime(2000, 1, 1); // fictive date. exactly the same as LimitationsManager

        public const int NO_LIMITATION_VALUE = -1;
        public int Frequency { get; set; }
        public int Quantity { get; set; }
        private DateTime NextActionFreqDate { get; set; }
        public int Concurrency { get; set; }
        public List<DeviceFamilyLimitationsDTO> lDeviceFamilyLimitations { get; set; }
        public int domianLimitID { get; set; }
        public string DomainLimitName { get; set; }
        public int npvrQuotaInSecs { get; set; }
        public int nUserLimit { get; set; }
        public int UserFrequency { get; set; }
        public string Description { get; set; }

        public void SetConcurrency(int nConcurrencyDomainLevel, int nConcurrencyGroupLevel)
        {
            this.Concurrency = nConcurrencyDomainLevel > 0 ? nConcurrencyDomainLevel : nConcurrencyGroupLevel;
        }

        private int GetActualConcurrency(int concurrencyGroupLevel, int concurrencyDomainLevel)
        {
            return concurrencyDomainLevel > 0 ? concurrencyDomainLevel : concurrencyGroupLevel;
        }

        public LimitationsManagerDTO(int concurrencyGroupLevel, int concurrencyDomainLevel, int quantity, int frequency, DateTime nextActionFreqDate)
        {
            Concurrency = GetActualConcurrency(concurrencyGroupLevel, concurrencyDomainLevel);
            Quantity = quantity;
            Frequency = frequency;
            NextActionFreqDate = nextActionFreqDate;
        }

        public LimitationsManagerDTO(int concurrency, int quantity, int frequency, DateTime nextActionFreqDate)
        {
            Concurrency = concurrency;
            Quantity = quantity;
            Frequency = frequency;
            NextActionFreqDate = nextActionFreqDate;
        }

        public LimitationsManagerDTO(int concurrency, int quantity, int frequency)
        {
            Concurrency = concurrency;
            Quantity = quantity;
            Frequency = frequency;
            NextActionFreqDate = FICTIVE_DATE;
        }

        public LimitationsManagerDTO(int concurrencyGroupLevel, int concurrencyDomainLevel, int quantity, int frequency)
        {
            Concurrency = GetActualConcurrency(concurrencyGroupLevel, concurrencyDomainLevel);
            Quantity = quantity;
            Frequency = frequency;
            NextActionFreqDate = FICTIVE_DATE;
        }

        public LimitationsManagerDTO()
        {
            Concurrency = 0;
            Quantity = 0;
            Frequency = 0;
            NextActionFreqDate = FICTIVE_DATE;
        }

        public IEnumerable<KeyValuePair<int, int>> CreateConcurrencyLimitationsList() => lDeviceFamilyLimitations.Select(x => new KeyValuePair<int, int>(x.deviceFamily, x.concurrency));
        public IEnumerable<KeyValuePair<int, int>> CreateDeviceLimitationsList() => lDeviceFamilyLimitations.Select(x => new KeyValuePair<int, int>(x.deviceFamily, x.quantity));
        public IEnumerable<KeyValuePair<int, int>> CreateFrequencyLimitationsList() => lDeviceFamilyLimitations.Select(x => new KeyValuePair<int, int>(x.deviceFamily, x.Frequency));
    }
}
