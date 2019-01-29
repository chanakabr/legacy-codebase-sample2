using System.Collections.Generic;

namespace ApiObjects
{
    public class GeneralPartnerConfig
    {
        public string PartnerName { get; set; }

        public int? MainLanguage { get; set; }

        public List<int> SecondaryLanguages { get; set; }

        public DeleteMediaPolicy? DeleteMediaPolicy { get; set; }

        public int? MainCurrency { get; set; }

        public List<int> SecondaryCurrencys { get; set; }

        public DowngradePolicy? DowngradePolicy { get; set; }

        public string MailSettings { get; set; }

        public string DateFormat { get; set; }

        public int? HouseholdLimitationModule { get; set; }
    }
}