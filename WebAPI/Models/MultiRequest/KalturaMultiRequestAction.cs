using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.MultiRequest
{
    public class KalturaMultiRequestAction
    {
        public string Service { get; set; }

        public string Action { get; set; }

        /// <summary>
        /// Abort all following requests if current request has an error
        /// </summary>
        public bool AbortAllOnError { get; set; }

        /// <summary>
        /// skip current request according to skip option
        /// </summary>
        public KalturaSkipOptions SkipOnError { get; set; }
       
        public Dictionary<string, object> Parameters { get; set; }

        public KalturaMultiRequestAction()
        {
            AbortAllOnError = false;
            SkipOnError = KalturaSkipOptions.No;
        }
    }
}