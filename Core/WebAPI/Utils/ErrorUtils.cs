using System;
using System.Net;
using System.ServiceModel;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;

namespace WebAPI.Utils
{
    public class ErrorUtils
    {
        public static void HandleWSException(Exception ex)
        {
            if (ex.InnerException is BadRequestException)
            {
                throw ex.InnerException;
            }

            throw GetClientException(ex);
        }

        public static void HandleClientException(ClientException ex)
        {            
            throw new ApiException(ex);
        }

        public static void HandleClientExternalException(ClientExternalException ex)
        {
            throw new ApiException(ex);
        }

        public static ClientException GetClientException(Exception ex)
        {
            var matchEx = GetMatchClientException(ex);
            if (matchEx != null)
                return matchEx;

            Exception last = ex.InnerException;
            while (last != null)
            {
                matchEx = GetMatchClientException(last);
                if (matchEx != null)
                    return matchEx;

                last = last.InnerException;
            }

            return new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
        }

        private static ClientException GetMatchClientException(Exception ex)
        {
            switch (ex)
            {
                case BadRequestException c: throw ex;
                case ClientException c: return ex as ClientException;
                case CommunicationException c1:
                case WebException c2: return new ClientException((int)StatusCode.InternalConnectionIssue, StatusCode.InternalConnectionIssue.ToString());
                case TimeoutException c: return new ClientException((int)StatusCode.Timeout, StatusCode.Timeout.ToString());
                case NotImplementedException c: throw new ClientException((int)StatusCode.NotImplemented, "Not implemented");
                default: return null;
            }
        }
    }
}