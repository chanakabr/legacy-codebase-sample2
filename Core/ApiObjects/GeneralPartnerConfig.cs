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

        public List<int> SecondaryCurrencies { get; set; }

        public DowngradePolicy? DowngradePolicy { get; set; }

        public string MailSettings { get; set; }

        public string DateFormat { get; set; }

        public int? HouseholdLimitationModule { get; set; }

        public int? DefaultRegion { get; set; }

        public bool? EnableRegionFiltering { get; set; }

        public bool SetUnchangedProperties(GeneralPartnerConfig oldConfig)
        {
            var needToUpdate = false;
            if ( this.SecondaryCurrencies != null)
            {
                needToUpdate = true;
            }
            else
            {
                this.SecondaryCurrencies = oldConfig.SecondaryCurrencies;
            }

            if (this.SecondaryLanguages != null)
            {
                needToUpdate = true;
            }
            else
            {
                this.SecondaryLanguages = oldConfig.SecondaryLanguages;
            }


            return needToUpdate;
        }

        public RollingDeviceRemovalData RollingDeviceRemovalData { get; set; }
    }

    public class RollingDeviceRemovalData
    {
        public RollingDevicePolicy? RollingDeviceRemovalPolicy { get; set; }

        public List<int> RollingDeviceRemovalFamilyIds { get; set; }
    }
}