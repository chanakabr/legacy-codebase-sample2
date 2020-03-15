using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logger
{
    public enum eLogType
    {
        SoapLog,
        SoapRequest,
        SoapResponse,
        CodeLog,
        SqlLog,
        WcfRequest,
        WcfResponse
    }

    //public enum eImplType
    //{
    //    FileLogger,
    //    RabbitLogger,
    //    Log4NetLogger
    //}
}
