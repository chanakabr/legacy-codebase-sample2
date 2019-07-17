using System.Collections.Generic;

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
        
        public KalturaSkipCondition SkipCondition { get; set; }
       
        public Dictionary<string, object> Parameters { get; set; }

        public KalturaMultiRequestAction()
        {
            AbortAllOnError = false;
        }
    }
}