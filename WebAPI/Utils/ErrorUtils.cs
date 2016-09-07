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
            throw new ApiException(ex);
        }
    }
}