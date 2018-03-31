using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class UserPINDigitsConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue NumberOfDigits;
        public NumericConfigurationValue MinNumberOfDigits;
        public NumericConfigurationValue MaxNumberOfDigits;

        public UserPINDigitsConfiguration(string key) : base(key)
        {
            NumberOfDigits = new NumericConfigurationValue("number_of_digits", this)
            {
                DefaultValue = 10,
                OriginalKey = "PIN_NUMBER_OF_DIGITS"
            };
            MinNumberOfDigits = new NumericConfigurationValue("min_number_of_digits", this)
            {
                DefaultValue = 8,
                OriginalKey = "PIN_MIN_NUMBER_OF_DIGITS"
            };
            MaxNumberOfDigits = new NumericConfigurationValue("max_number_of_digits", this)
            {
                DefaultValue = 10,
                OriginalKey = "PIN_MAX_NUMBER_OF_DIGITS"
            };
        }
    }
}
