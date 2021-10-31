namespace ApiObjects
{
    public class DefaultParentalSettingsPartnerConfig
    {
        public long? DefaultMoviesParentalRuleId { get; set; }

        public long? DefaultTvSeriesParentalRuleId { get; set; }

        public string DefaultParentalPin { get; set; }

        public string DefaultPurchasePin { get; set; }

        public long? DefaultPurchaseSettings { get; set; }

        public bool SetUnchangedProperties(DefaultParentalSettingsPartnerConfig oldConfig)
        {
            var needToUpdate = false;
            if (this.DefaultMoviesParentalRuleId.HasValue && this.DefaultMoviesParentalRuleId != oldConfig.DefaultMoviesParentalRuleId)
            {
                needToUpdate = true;
            }
            else
            {
                this.DefaultMoviesParentalRuleId = oldConfig.DefaultMoviesParentalRuleId;
            }

            if (this.DefaultTvSeriesParentalRuleId.HasValue && DefaultTvSeriesParentalRuleId != oldConfig.DefaultTvSeriesParentalRuleId)
            {
                needToUpdate = true;
            }
            else
            {
                this.DefaultTvSeriesParentalRuleId = oldConfig.DefaultTvSeriesParentalRuleId;
            }

            if (!string.IsNullOrEmpty(DefaultParentalPin) && !string.Equals(DefaultParentalPin, oldConfig.DefaultParentalPin))
            {
                needToUpdate = true;
            }
            else
            {
                DefaultParentalPin = oldConfig.DefaultParentalPin;
            }

            if (!string.IsNullOrEmpty(DefaultPurchasePin) && !string.Equals(DefaultPurchasePin, oldConfig.DefaultPurchasePin))
            {
                needToUpdate = true;
            }
            else
            {
                DefaultPurchasePin = oldConfig.DefaultPurchasePin;
            }

            if (this.DefaultPurchaseSettings.HasValue && DefaultPurchaseSettings != oldConfig.DefaultPurchaseSettings)
            {
                needToUpdate = true;
            }
            else
            {
                this.DefaultPurchaseSettings = oldConfig.DefaultPurchaseSettings;
            }

            return needToUpdate;
        }
    }
}
