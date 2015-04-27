using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebAPI.Models;

namespace WebAPI.Filters.Exceptions
{
    public class ApiException : HttpResponseException
    {
        public StatusCode Code { get; set; }

        protected ApiException(HttpStatusCode httpCode, int code, string msg)
            : base(new HttpResponseMessage() { ReasonPhrase = msg, StatusCode = httpCode, Content = new StringContent(code.ToString()) })
        {

        }
    }
}