using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Xml.Serialization;
using WebAPI.App_Start;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;

namespace WebAPI.Exceptions
{
    public partial class ApiException : HttpResponseException
    {
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public int Code { get; set; }

        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        new public string Message { get; set; }

        [DataMember(Name = "args")]
        [JsonProperty("args")]
        [XmlArray(ElementName = "args")]
        [XmlArrayItem("item")]
        new public KalturaApiExceptionArg[] Args { get; set; }

        public HttpStatusCode FailureHttpCode { get; }

        #region Classes

        public class ExceptionType
        {
        }

        public class ClientExceptionType : ExceptionType
        {
            public eResponseStatus statusCode;
            public string message;
            public string description;

            public ClientExceptionType(eResponseStatus statusCode, string message)
            {
                this.statusCode = statusCode;
                this.message = message;
                this.description = string.Empty;
            }

            public ClientExceptionType(eResponseStatus statusCode, string message, string description)
            {
                this.statusCode = statusCode;
                this.message = message;
                this.description = description;
            }
        }

        public class ApiExceptionType : ExceptionType
        {
            public int? obsoleteStatusCode = null;
            public int statusCode;
            public string name;
            public string message;
            public string[] parameters;

            public ApiExceptionType(StatusCode statusCode, StatusCode obsoleteStatusCode, string message, params string[] parameters)
                : this(statusCode, message, parameters)
            {
                this.obsoleteStatusCode = (int)obsoleteStatusCode;
            }

            public ApiExceptionType(StatusCode statusCode, string message, params string[] parameters)
                : this((int)statusCode, statusCode.ToString(), message, parameters)
            {
            }

            public ApiExceptionType(int statusCode, string name, string message, params string[] parameters)
                : this(statusCode, message, parameters)
            {
                this.name = name;
            }

            public ApiExceptionType(int statusCode, string message, params string[] parameters)
            {
                this.statusCode = statusCode;
                this.message = message;
                this.parameters = parameters;
            }

            public string Format(params object[] values)
            {
                if (parameters == null || parameters.Length == 0)
                    return message;

                string ret = message;
                string token;
                string value;
                for (int i = 0; i < parameters.Length; i++)
                {
                    token = string.Format("@{0}@", parameters[i]);
                    value = values[i] != null ? values[i].ToString() : string.Empty;
                    ret = ret.Replace(token, value);
                }

                return ret;
            }

            public KalturaApiExceptionArg[] CreateArgs(params object[] values)
            {
                if (parameters == null || parameters.Length == 0)
                    return null;

                KalturaApiExceptionArg[] ret = new KalturaApiExceptionArg[parameters.Length];
                string token;
                string value;
                for (int i = 0; i < parameters.Length; i++)
                {
                    KalturaApiExceptionArg args = new KalturaApiExceptionArg() { name = parameters[i], value = values[i] != null ? values[i].ToString() : string.Empty };
                    ret[i] = args;
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
            public HttpStatusCode failureHttpCode { get; set; }
            public KalturaApiExceptionArg[] arguments { get; set; }
        }

        #endregion

        #region Ctors

        public ApiException()
            : base(HttpStatusCode.OK)
        {
        }

        public ApiException(ClientException ex)
            : this(new ApiExceptionType(ex.Code, ex.ExceptionMessage, ex.Args != null ? ex.Args.Select(a => a.key).ToArray() : null), ex.Args != null ? ex.Args.Select(a => a.value).ToArray() : null)
        {
        }

        public ApiException(ClientExternalException ex)
            : this(ex.ApiExceptionType, ex.ExternalCode, ex.ExternalMessage)
        {
        }

        public ApiException(ClientException ex, HttpStatusCode httpStatusCode)
             : this(ex.Code, ex.ExceptionMessage, null, httpStatusCode)
        {
            FailureHttpCode = httpStatusCode;
        }

        public ApiException(ApiException ex, HttpStatusCode httpStatusCode)
            : this(ex.Code, ex.Message, null, httpStatusCode)
        {
            FailureHttpCode = httpStatusCode;
        }

        public ApiException(Exception ex, HttpStatusCode httpStatusCode)
            : this((int)eResponseStatus.Error, eResponseStatus.Error.ToString(), null, httpStatusCode)
        {
            FailureHttpCode = httpStatusCode;
        }

        protected ApiException(ApiException ex)
            : this(ex.Code, ex.Message)
        {
        }

        protected ApiException(ApiExceptionType type, params object[] parameters)
            : this((int)(OldStandardAttribute.isCurrentRequestOldVersion() && type.obsoleteStatusCode.HasValue ? type.obsoleteStatusCode.Value : type.statusCode), type.Format(parameters), type.CreateArgs(parameters))
        {
        }

        private ApiException(int code, string message, KalturaApiExceptionArg[] args = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
            : base(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ObjectContent(typeof(ExceptionPayload), new ExceptionPayload()
                {
                    error = new HttpError(new Exception(message), true),
                    code = code,
                    failureHttpCode = httpStatusCode,
                    arguments = args
                },
                new JsonMediaTypeFormatter())
            })
        {
            Code = code;
            Message = message;
            Args = args;
            FailureHttpCode = httpStatusCode;
        }

        #endregion
    }
}