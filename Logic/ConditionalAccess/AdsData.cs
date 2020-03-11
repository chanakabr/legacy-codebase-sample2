using ApiObjects;
using Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.ConditionalAccess
{
    public class AdsControlData
    {

        public AdsControlData()
        {
            this.FileId = 0;
            this.FileType = string.Empty;
            this.AdsParam = string.Empty;
            this.AdsPolicy = null;
        }

        public int FileId { get; set; }

        public string FileType { get; set; }

        public string AdsParam { get; set; }

        public AdsPolicy? AdsPolicy { get; set; }
    }

    public class AdsControlResponse
    {
        public List<AdsControlData> Sources { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
