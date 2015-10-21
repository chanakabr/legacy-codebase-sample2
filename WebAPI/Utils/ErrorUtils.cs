using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Web;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Utils
{
    public class ErrorUtils
    {
        private static Dictionary<int, HttpStatusCode> statuses = new Dictionary<int, HttpStatusCode>() { 
            { 0, HttpStatusCode.OK },
            { 1006, HttpStatusCode.NotFound },   
            { 1010, HttpStatusCode.NotFound },            
            { 1019, HttpStatusCode.NotFound },
            { 1020, HttpStatusCode.NotFound },
            { 2000, HttpStatusCode.NotFound },
            { 2003, HttpStatusCode.NotFound },
            { 2010, HttpStatusCode.BadRequest },
            { 2012, HttpStatusCode.BadRequest },
            { 2013, HttpStatusCode.BadRequest },
            { 4002, HttpStatusCode.BadRequest },
            { 4003, HttpStatusCode.BadRequest },
            { 4004, HttpStatusCode.BadRequest },
            { 4005, HttpStatusCode.BadRequest },
            { 5001, HttpStatusCode.NotFound },
            { 6001, HttpStatusCode.BadRequest },
            { (int)StatusCode.InternalConnectionIssue, HttpStatusCode.InternalServerError },
            { (int)StatusCode.Timeout, HttpStatusCode.GatewayTimeout },
            { (int)StatusCode.Error, HttpStatusCode.InternalServerError },
            // ExternalChannelNotExist
            { 4011, HttpStatusCode.NotFound}
        };

        public static void HandleWSException(Exception ex)
        {
            if (ex is CommunicationException || ex is WebException)
            {
                throw new ClientException((int)StatusCode.InternalConnectionIssue, StatusCode.InternalConnectionIssue.ToString());
            }

            if (ex is TimeoutException)
            {
                throw new ClientException((int)StatusCode.Timeout, StatusCode.Timeout.ToString());
            }

            if (ex is Exception)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
        }

        public static void HandleClientException(ClientException ex)
        {
            if (!statuses.ContainsKey(ex.Code))
                throw new PartialSuccessException(ex.Code, ex.ExceptionMessage);

            switch (statuses[ex.Code])
            {
                case HttpStatusCode.OK:
                    break;
                case HttpStatusCode.InternalServerError:
                    throw new InternalServerErrorException(ex.Code, ex.ExceptionMessage);
                case HttpStatusCode.BadRequest:
                    throw new BadRequestException(ex.Code, ex.ExceptionMessage);
                case HttpStatusCode.NotFound:
                    throw new NotFoundException(ex.Code, ex.ExceptionMessage);
                case HttpStatusCode.GatewayTimeout:
                    throw new GatewayTimeoutException(ex.Code, ex.ExceptionMessage);
                default:
                    throw new PartialSuccessException(ex.Code, ex.ExceptionMessage);
            }
        }
    }
}