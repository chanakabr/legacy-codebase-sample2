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

        public List<SocialActionPrivacy> InternalPrivacy { get; set; }

        public SocialPrivacySettings()
        {
            InternalPrivacy = new List<SocialActionPrivacy>();
        }

        public static SocialPrivacySettings SetDefultPrivacySettings()
        {
            SocialPrivacySettings settings = new SocialPrivacySettings();

            settings.InternalPrivacy = SetDefaultInternalPrivacy();

            settings.SocialNetworks = new List<SocialNetwork>();
            foreach (SocialPlatform platform in Enum.GetValues(typeof(SocialPlatform)))
            {
                settings.SocialNetworks.Add(
                     new SocialNetwork
                     {
                         Network = platform,
                         SocialAction = SetDefaultNetworkPlatformPrivacy(),
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
                         SocialAction  = SetDefaultNetworkPlatformPrivacy(),
                         SocialPrivacy = eSocialPrivacy.SELF
                     }
                     );
            }
        }
        
        private static List<SocialActionPrivacy> SetDefaultInternalPrivacy()
        {
            List<SocialActionPrivacy> internalPrivacy = new List<SocialActionPrivacy>();
            foreach (eUserAction action in Enum.GetValues(typeof(eUserAction)))
            {
                SocialActionPrivacy socialActionPrivacy = new SocialActionPrivacy()
                {
                    Action = action,
                    Privacy = action == eUserAction.WATCHES ? eSocialActionPrivacy.DONT_ALLOW : eSocialActionPrivacy.ALLOW
                };
                internalPrivacy.Add(socialActionPrivacy);
            }            
            return internalPrivacy;
        }

        private static List<SocialActionPrivacy> SetDefaultNetworkPlatformPrivacy()
        {
            List<SocialActionPrivacy> socialNetwork = new List<SocialActionPrivacy>();
            foreach (eUserAction action in Enum.GetValues(typeof(eUserAction)))
            {
                SocialActionPrivacy socialActionPrivacy = new SocialActionPrivacy()
                {
                    Action = action,
                    Privacy = eSocialActionPrivacy.DONT_ALLOW
                };
                socialNetwork.Add(socialActionPrivacy);
            }
            return socialNetwork;
        }

        public override string ToString()
        {
            string res = string.Empty;

            res = string.Format("InternalPrivacy : {0}", string.Join(",",InternalPrivacy.Select(x=>x.ToString()).ToList<string>()));
            res = string.Format("{0}, SocialNetworks: {1}, ", res, SocialNetworks!= null? 
                string.Join(",", SocialNetworks.SelectMany(o => o.Network + " - " + o.SocialAction.ToString()+" -" +o.SocialPrivacy.ToString()).ToList()) : string.Empty);

            return res;
        }

       
    }
}
