using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Social
{
    public class SocialPrivacySettings
    {
        public List<SocialNetwork> SocialNetworks { get; set; }

        public eSocialActionPrivacy InternalPrivacy { get; set; }

        public SocialPrivacySettings()
        {
            InternalPrivacy = eSocialActionPrivacy.ALLOW; //Default value : Enabled
        }

        public static SocialPrivacySettings SetDefultPrivacySettings()
        {
            SocialPrivacySettings settings = new SocialPrivacySettings();

            settings.SocialNetworks = new List<SocialNetwork>();
            foreach (SocialPlatform platform in Enum.GetValues(typeof(SocialPlatform)))
            {
                settings.SocialNetworks.Add(
                     new SocialNetwork
                     {
                         Network = platform,
                         Privacy = eSocialActionPrivacy.DONT_ALLOW,
                         SocialPrivacy = eSocialPrivacy.SELF
                     }
                     );
            }
            return settings;
        }

        public static void SetDefultPrivacySettings(ref SocialPrivacySettings settings)
        {
            settings.SocialNetworks = new List<SocialNetwork>();
            foreach (SocialPlatform platform in Enum.GetValues(typeof(SocialPlatform)))
            {
                settings.SocialNetworks.Add(
                     new SocialNetwork
                     {
                         Network = platform,
                         Privacy = eSocialActionPrivacy.DONT_ALLOW,
                         SocialPrivacy = eSocialPrivacy.SELF
                     }
                     );
            }
        }

        public override string ToString()
        {
            string res = string.Empty;

            res = string.Format("InternalPrivacy : {0}", InternalPrivacy.ToString());
            res = string.Format("{0}, SocialNetworks: {1}, ", res, SocialNetworks!= null? 
                string.Join(",", SocialNetworks.SelectMany(o => o.Network + " - " + o.Privacy.ToString()+" -" +o.SocialPrivacy.ToString()).ToList()) : string.Empty);

            return res;
        }

       
    }
}
