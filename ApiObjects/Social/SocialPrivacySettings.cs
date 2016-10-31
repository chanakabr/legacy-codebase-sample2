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

        public override string ToString()
        {
            string res = string.Empty;

            res = string.Format("InternalPrivacy : {0}", InternalPrivacy.ToString());
            res = string.Format("{0}, SocialNetworks: {1}, ", res, string.Join(",", SocialNetworks.SelectMany(o => o.Network + " - " + o.Privacy.ToString()+" -" +o.SocialPrivacy.ToString()).ToList()));

            return res;
        }
    }
}
