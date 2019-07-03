using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.App_Start;

namespace WebAPI
{
    public class KalturaApiExceptionHelpers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static KalturaAPIExceptionWrapper prepareExceptionResponse(int statusCode, string msg, KalturaApiExceptionArg[] arguments = null)
        {
            return new KalturaAPIExceptionWrapper() { error = new KalturaAPIException() { message = msg, code = statusCode.ToString(), args = arguments == null ? null : arguments.ToList() } };
        }

        public static string HandleError(string errorMsg, string stack)
        {
            string message = errorMsg;
            
            #if DEBUG
            
            message = string.Concat(message, stack);
            log.ErrorFormat("{0}", message);
            
            #else
            
            log.ErrorFormat("{0} {1}", message, stack);
            
            #endif


            return message;
        }
    }
}