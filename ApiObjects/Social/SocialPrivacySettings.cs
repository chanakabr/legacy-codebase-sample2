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
            SocialNetworks = new List<SocialNetwork>();
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
                         SocialAction = SetDefaultNetworkPlatformPrivacy(),
                         SocialPrivacy = eSocialPrivacy.SELF
                     }
                     );
            }
        }

        private static List<SocialActionPrivacy> SetDefaultInternalPrivacy()
        {
            List<eUserAction> actions = new List<eUserAction>() { eUserAction.LIKE, eUserAction.WATCHES, eUserAction.SHARE, eUserAction.RATES };
            List<SocialActionPrivacy> internalPrivacy = new List<SocialActionPrivacy>();
            foreach (eUserAction action in actions)
            {
                SocialActionPrivacy socialActionPrivacy = new SocialActionPrivacy()
                {
                    Action = action,
                    Privacy = action == eUserAction.WATCHES ? eSocialActionPrivacy.DONT_ALLOW : eSocialActionPrivacy.ALLOW
                };
                internalPrivacy.Add(socialActionPrivacy);
            }
            internalPrivacy = internalPrivacy.OrderByDescending(x => x.Action == eUserAction.WATCHES).ThenBy(x => x.Action != eUserAction.WATCHES).ToList();
            return internalPrivacy;
        }

        private static List<SocialActionPrivacy> SetDefaultNetworkPlatformPrivacy()
        {
            List<eUserAction> actions = new List<eUserAction>() { eUserAction.LIKE, eUserAction.WATCHES, eUserAction.SHARE, eUserAction.RATES };
            List<SocialActionPrivacy> socialNetwork = new List<SocialActionPrivacy>();
            foreach (eUserAction action in actions)
            {
                SocialActionPrivacy socialActionPrivacy = new SocialActionPrivacy()
                {
                    Action = action,
                    Privacy = eSocialActionPrivacy.DONT_ALLOW
                };
                socialNetwork.Add(socialActionPrivacy);
            }

            socialNetwork = socialNetwork.OrderByDescending(x => x.Action == eUserAction.WATCHES).ThenBy(x => x.Action != eUserAction.WATCHES).ToList();
            return socialNetwork;
        }


        public static List<SocialActionPrivacy> SetDefaultInternalPrivacy(List<eUserAction> missingActions)
        {
            List<SocialActionPrivacy> internalPrivacy = new List<SocialActionPrivacy>();
            foreach (eUserAction action in missingActions)
            {
                SocialActionPrivacy socialActionPrivacy = new SocialActionPrivacy()
                {
                    Action = action,
                    Privacy = action == eUserAction.WATCHES ? eSocialActionPrivacy.DONT_ALLOW : eSocialActionPrivacy.ALLOW
                };
                internalPrivacy.Add(socialActionPrivacy);
            }
            internalPrivacy = internalPrivacy.OrderByDescending(x => x.Action == eUserAction.WATCHES).ThenBy(x => x.Action != eUserAction.WATCHES).ToList();
            return internalPrivacy;
        }

        public static List<SocialActionPrivacy> SetDefaultNetworkPlatformPrivacy(List<eUserAction> missingActions)
        {
            List<SocialActionPrivacy> socialNetwork = new List<SocialActionPrivacy>();
            foreach (eUserAction action in missingActions)
            {
                SocialActionPrivacy socialActionPrivacy = new SocialActionPrivacy()
                {
                    Action = action,
                    Privacy = eSocialActionPrivacy.DONT_ALLOW
                };
                socialNetwork.Add(socialActionPrivacy);
            }
            socialNetwork = socialNetwork.OrderByDescending(x => x.Action == eUserAction.WATCHES).ThenBy(x => x.Action != eUserAction.WATCHES).ToList();
            return socialNetwork;
        }

        public static List<SocialNetwork> SetDefultNetworkPrivacySettings()
        {
            List<SocialNetwork> SocialNetworks = new List<SocialNetwork>();
            foreach (SocialPlatform platform in Enum.GetValues(typeof(SocialPlatform)))
            {
                SocialNetworks.Add(
                     new SocialNetwork
                     {
                         Network = platform,
                         SocialAction = SetDefaultNetworkPlatformPrivacy(),
                         SocialPrivacy = eSocialPrivacy.SELF
                     }
                     );
            }

            return SocialNetworks;
        }

        public override string ToString()
        {
            string res = string.Empty;

            res = string.Format("InternalPrivacy : {0}", string.Join(",", InternalPrivacy.Select(x => x.ToString()).ToList<string>()));
            res = string.Format("{0}, SocialNetworks: {1}, ", res, SocialNetworks != null ?
                string.Join(",", SocialNetworks.SelectMany(o => o.Network + " - " + o.SocialAction.ToString() + " -" + o.SocialPrivacy.ToString()).ToList()) : string.Empty);

            return res;
        }

       
    }
}
