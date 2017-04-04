using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Social
{
    public class SocialActionPrivacy
    {  
        public eUserAction Action { get; set; }
        public eSocialActionPrivacy Privacy { get; set; }

        public SocialActionPrivacy()
        {
        }

        public override string ToString()
        {
            return string.Format("Privacy={0}, Action={1}", Privacy.ToString(), Action.ToString());
        }
    }
}
