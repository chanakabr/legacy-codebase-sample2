using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mailer
{
    public interface IMailer
    {

        bool SendMailTemplate(ApiObjects.MailRequestObj request); 
    }
}
