using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EngagementAdapter
{
    public enum eAdapterStatusCode
    {
        OK = 0,
        Error = 1,
        SignatureMismatch = 2,
        NoConfigurationFound = 3
    }

    public enum eFailReason
    {
    }

    public enum eDocumentType
    {
        Configuration
    }

    public enum eHappynessLevel
    {
        Happy,
        Smile,
        Sad
    }


    // possible http codes:
    // 200 OK
    // 206 PARTIAL_CONTENT
    // 302 REDIRECTION
    // 400 BAD_REQUEST
    // 401 NOT_AUTHORIZED
    // 404 NOT_FOUND
    // 500 SERVER_ERROR
    // 502 SERVER_TIMEOUT

    // happiness values:
    // Happy (8.5-10)
    // Smile (7-8.5)
    // Sad (3-5)
}