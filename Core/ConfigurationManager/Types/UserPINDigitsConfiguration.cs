using System;
using System.Collections.Generic;
using System.Text;
using ConfigurationManager.Types;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class UserPINDigitsConfiguration : BaseConfig<UserPINDigitsConfiguration>
    {
        public BaseValue<int> NumberOfDigits = new BaseValue<int>("number_of_digits", 10);
        public BaseValue<int> MinNumberOfDigits = new BaseValue<int>("min_number_of_digits",8);
        public BaseValue<int> MaxNumberOfDigits = new BaseValue<int>("max_number_of_digits", 10);

        public override string TcmKey => TcmObjectKeys.UserPINDigitsConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}
