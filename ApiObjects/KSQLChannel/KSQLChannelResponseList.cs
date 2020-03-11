using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class KSQLChannelResponseList
    {
        public Response.Status Status
        {
            get;
            set;
        }

        public List<KSQLChannel> Channels
        {
            get;
            set;
        }
        
        public KSQLChannelResponseList()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            Channels = new List<KSQLChannel>();
        }

        public KSQLChannelResponseList(ApiObjects.Response.Status status, List<KSQLChannel> channels)
        {
            this.Status = status;
            this.Channels = channels;
        }

    }
}