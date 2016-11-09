using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Social
{
    public class SocialNetwork
    {
        public SocialPlatform Network { get; set; }
        public eSocialActionPrivacy Privacy { get; set; }
        public eSocialPrivacy SocialPrivacy { get; set; }

        public override string ToString()
        {
            return string.Format("Network:{0}, Privacy:{1}, SocialPrivacy:{2}", Network.ToString(), Privacy.ToString(), SocialPrivacy.ToString());
        }
    }
}
