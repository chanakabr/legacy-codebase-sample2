using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class SendVia
    {
        public int is_email { get; set; }
        public int is_sms { get; set; }
        public int is_device { get; set; }

        public SendVia()
        {
        }
        public SendVia(int isEmail, int isSms, int isDevice)
        {
            this.is_email = isEmail;
            this.is_sms = isSms;
            this.is_device = isDevice;
        }
    }
}
