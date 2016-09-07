using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class ApiException : HttpResponseException
    {
        public int Code { get; set; }
        new public string Message { get; set; }

        public class ApiExceptionType
        {
            public StatusCode? obsoleteStatusCode = null;
            public StatusCode statusCode;
            public string message;
            public string[] parameters;

            public ApiExceptionType(StatusCode obsoleteStatusCode, StatusCode statusCode, string message, params string[] parameters) : this(statusCode, message, parameters)
            {
                this.obsoleteStatusCode = obsoleteStatusCode;
            }

            public ApiExceptionType(StatusCode statusCode, string message, params string[] parameters)
            {
                this.statusCode = statusCode;
                this.message = message;
                this.parameters = parameters;
            }

            public string Format(params object[] values)
            {
                if (parameters.Length == 0)
                    return message;

                string ret = message;
                string token;
                string value;
                for (int i = 0; i < parameters.Length; i++)
                {
                    token = string.Format("@{0}@", parameters[i]);
                    value = values[i].ToString();
                    ret = ret.Replace(token, value);
                }

                return ret;
            }
        }

        public class ExceptionPayload
        {
            public ExceptionPayload() 
            { 

            }
            public int code { get; set; }
            public HttpError error { get; set; }
        }

        public ApiException(ClientException ex)
            : this(ex.Code, ex.Message)
        {
        }

        protected ApiException(ApiException ex)
            : this(ex.Code, ex.Message)
        {
        }

        protected ApiException(ApiExceptionType type, params object[] parameters)
            : this((int)(OldStandardAttribute.isCurrentRequestOldVersion() ? type.obsoleteStatusCode : type.statusCode), type.Format(parameters))
        {
        }

        private ApiException(int code, string message)
            : base(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ObjectContent(typeof(ExceptionPayload), new ExceptionPayload()
                {
                    error = new HttpError(new Exception(message), true),
                    code = code
                },
                new JsonMediaTypeFormatter())
            })
        {
            Code = code;
            Message = message;
        }
    }
}