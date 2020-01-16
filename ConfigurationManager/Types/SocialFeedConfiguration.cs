using System;
using System.Collections.Generic;
using System.Text;
using ConfigurationManager.Types;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class SocialFeedConfiguration : BaseConfig<SocialFeedConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.SocialFeedConfiguration;
        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<int> FacebookItemCount = new BaseValue<int>("facebook_item_count", 100);
        public BaseValue<int> InAppItemCount = new BaseValue<int>("in_app_item_count", 100);
        public BaseValue<int> TwitterItemCount = new BaseValue<int>("twitter_item_count", 100);
        public BaseValue<int> FacebookTTL = new BaseValue<int>("facebook_ttl", 10);
        public BaseValue<int> InAppTTL = new BaseValue<int>("in_app_ttl", 10);
        public BaseValue<int> TwitterTTL = new BaseValue<int>("twitter_ttl", 10);
        public BaseValue<int> TagsTTL = new BaseValue<int>("tags_ttl", 30);

        public BaseValue<int> GetTTLByPlatform(string platform)
        {
            BaseValue<int> ttl = new BaseValue<int>("unknown_ttl", 10);
            switch (platform.ToLower())
            {
                case "inapp":
                    ttl.ActualValue = InAppTTL.Value;
                    break;
                case "facebook":
                    ttl.ActualValue = FacebookTTL.Value;
                    break;
                case "twitter":
                    ttl.ActualValue = TwitterTTL.Value;
                    break;
                case "unknown":
                default:

                    break;
            }

            return ttl;
        }

    }
}
    
/*    }
            
        }*/
    