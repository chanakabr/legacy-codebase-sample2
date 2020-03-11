using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Notification;

namespace Core.Notification
{
    public class UserSettings
    {
        public string siteGuid { get; set; }
        public SendVia sendVia { get; set; }
        public int status { get; set; }
        public int is_active { get; set; }

        public UserSettings() 
        {
        }
    }
}
