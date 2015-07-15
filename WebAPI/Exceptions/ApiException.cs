using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class ApiException : HttpResponseException
    {
        public StatusCode Code { get; set; }
        public class ExceptionPayload
        {
            public int code { get; set; }
            public HttpError error { get; set; }
        }

        protected ApiException(HttpStatusCode httpCode, int code, string msg)
            : base(new HttpResponseMessage()
            {                
                StatusCode = httpCode,
                Content = new ObjectContent(typeof(ExceptionPayload), new ExceptionPayload()
                {
                    error = new HttpError(new Exception(msg), true),
                    code = code                    
                },
            new JsonMediaTypeFormatter())
            })
        {

        }
    }
}