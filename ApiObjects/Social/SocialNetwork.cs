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
        public eSocialPrivacy SocialPrivacy { get; set; }

        public List<SocialActionPrivacy> SocialAction { get; set; }

        public override string ToString()
        {
            return string.Format("Network:{0}, Privacy:{1}, SocialAction:{2}", Network.ToString(), SocialPrivacy.ToString(), string.Join(",", SocialAction.Select(x => x.ToString()).ToList<string>()));
        }
    }
}
