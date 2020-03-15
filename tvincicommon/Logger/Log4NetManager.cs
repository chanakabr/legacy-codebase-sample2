using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Logger
{
    //get new Logger4Net 
    public class Log4NetManager
    {
        public static ILogger4Net GetLogger(Type type)
        {
            return new Logger4Net(type);
        }
    }
}
