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

        //public SkipOptions SkipOption { get; set; }
        // TODO SHIR - SKIP ON ERROR
        // SKIP - PUT EXCEPTION THAT SKIP
        
        public Dictionary<string, object> Parameters { get; set; }

        public KalturaMultiRequestAction()
        {
            AbortAllOnError = false;
            //SkipOption = SkipOptions.No;
        }
    }

    public enum SkipOptions
    {
        No, 
        Previous,
        Any
    }
}