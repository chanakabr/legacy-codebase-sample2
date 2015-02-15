using ApiObjects;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class ChangeDLMObj
    {
        public StatusObject resp { get; set; }

        public List<string> users { get; set; }

        public List<string> devices { get; set; }

        public ChangeDLMObj()
        {
            resp = new StatusObject((int)eResponseStatus.InternalError, string.Empty);
            devices = new List<string>();
            users = new List<string>();
        }
        public ChangeDLMObj(StatusObject eResp, List<string> lUsers, List<string> lDevices)
        {
            this.resp = eResp;
            this.users = lUsers;
            this.devices = lDevices;
        }
    }
}
