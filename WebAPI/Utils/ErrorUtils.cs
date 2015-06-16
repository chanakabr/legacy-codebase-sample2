using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Web;
using WebAPI.Exceptions;
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

        // additionalBadRequestStatusCodes - might be WS statuses pointing on bad request 
        // additionalNotFoundStatusCodes - might be WS statuses pointing on not found 
        public static void HandleClientException(ClientException ex, List<int> additionalBadRequestStatusCodes = null, List<int> additionalNotFoundStatusCodes = null)
        {
            if (ex.Code == (int)WebAPI.Models.General.StatusCode.BadRequest || (additionalBadRequestStatusCodes != null && additionalBadRequestStatusCodes.Contains(ex.Code)))
            {
                throw new BadRequestException(ex.Code, ex.ExceptionMessage);
            }

            if (ex.Code == (int)WebAPI.Models.General.StatusCode.NotFound || (additionalNotFoundStatusCodes != null && additionalNotFoundStatusCodes.Contains(ex.Code)))
            {
                throw new NotFoundException(ex.Code, ex.ExceptionMessage);
            }

            throw new InternalServerErrorException(ex.Code, ex.ExceptionMessage);
        }
    }
}