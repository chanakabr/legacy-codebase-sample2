using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mailer
{
    public class MailFactory
    {

        public static IMailer GetMailer(MailImplementors implID)
        {
            IMailer mailer = null;

            switch (implID)
            {
                case MailImplementors.MCMailer:
                    {
                        mailer = new MCMailer();
                        break;
                    }
                
                default:
                    {
                        break;
                    }
            }
            return mailer;
        }

        
    }
}
