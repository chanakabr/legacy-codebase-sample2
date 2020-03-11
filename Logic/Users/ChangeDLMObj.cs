using ApiObjects;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class ChangeDLMObj
    {
        public ApiObjects.Response.Status resp { get; set; }

        public List<string> users { get; set; }

        public List<string> devices { get; set; }

        public ChangeDLMObj()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            devices = new List<string>();
            users = new List<string>();
        }
        public ChangeDLMObj(ApiObjects.Response.Status eResp, List<string> lUsers, List<string> lDevices)
        {
            this.resp = eResp;
            this.users = lUsers;
            this.devices = lDevices;
        }
    }
}
