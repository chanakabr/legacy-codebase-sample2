using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class ResponseUtils
    {
        public static ClientResponseStatus ReturnBadCredentialsClientResponse()
        {
            ClientResponseStatus response = new ClientResponseStatus();
            response.Status.Code = (int)eStatus.BadCredentials;
            response.Status.Message = "Unknown Group";
            return response;
        }

        public static Status ReturnBadCredentialsStatus()
        {
            Status status = new Status();
            status.Code = (int)eStatus.BadCredentials;
            status.Message = "Unknown Group";
            return status;
        }

        public static ClientResponseStatus ReturnGeneralErrorClientResponse()
        {
            return ReturnGeneralErrorClientResponse(null);
        }

        public static ClientResponseStatus ReturnGeneralErrorClientResponse(string message)
        {
            ClientResponseStatus response = new ClientResponseStatus();
            response.Status.Code = (int)eStatus.Error;
            if (!string.IsNullOrEmpty(message))
                response.Status.Message = message;
            return response;
        }

        public static Status ReturnGeneralErrorStatus()
        {
            return ReturnGeneralErrorStatus(string.Empty);
        }

        public static Status ReturnGeneralErrorStatus(string message)
        {
            Status status = new Status();
            status.Code = (int)eStatus.Error;
            if (!string.IsNullOrEmpty(message))
                status.Message = message;
            return status;
        }

        public static Status ReturnBadRequestStatus()
        {
            return ReturnBadRequestStatus(string.Empty);
        }

        public static Status ReturnBadRequestStatus(string message)
        {
            Status status = new Status();
            status.Code = (int)eStatus.BadRequest;
            if (!string.IsNullOrEmpty(message))
                status.Message = message;
            return status;
        }
    }
}
