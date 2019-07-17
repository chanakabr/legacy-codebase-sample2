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
            if (ex is CommunicationException || ex is WebException)
            {
                return new ClientException((int)StatusCode.InternalConnectionIssue, StatusCode.InternalConnectionIssue.ToString());
            }

            if (ex is TimeoutException)
            {
                return new ClientException((int)StatusCode.Timeout, StatusCode.Timeout.ToString());
            }

            if (ex is ClientException)
            {
                return ex as ClientException;
            }

            Exception last = ex.InnerException;
            while (last != null)
            {
                if (last is ClientException)
                    return last as ClientException;
                 
                last = last.InnerException;
            }

            return new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
        }
    }
}