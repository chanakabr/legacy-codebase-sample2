using Core.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaltura
{    
    public class User : KalturaUsers
    {
        public User(int groupId)
            : base(groupId)
        {
            // activate/deactivate user features
            this.ShouldSubscribeNewsLetter = false;
            this.ShouldCreateDefaultRules = false;
            this.ShouldSendWelcomeMail = true;
        }

    }
}
