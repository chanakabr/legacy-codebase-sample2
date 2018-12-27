using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KLogMonitor
{
    public class Events
    {
        public enum eEvent
        {
            EVENT_API_START,
            EVENT_API_END,
            EVENT_CLIENT_API_START,
            EVENT_CLIENT_API_END,
            EVENT_DATABASE,
            EVENT_COUCHBASE,
            EVENT_ELASTIC,
            EVENT_RABBITMQ,
            EVENT_SPHINX,
            EVENT_CONNTOOK,
            EVENT_DUMPFILE,
            EVENT_WS
        }

        internal static string GetEventString(eEvent eventMonitor)
        {
            switch (eventMonitor)
            {
                case eEvent.EVENT_API_START:
                    return Constants.EVENT_API_START;
                case eEvent.EVENT_API_END:
                    return Constants.EVENT_API_END;
                case eEvent.EVENT_CLIENT_API_START:
                    return Constants.EVENT_CLIENT_API_START;
                case eEvent.EVENT_CLIENT_API_END:
                    return Constants.EVENT_CLIENT_API_END;
                case eEvent.EVENT_DATABASE:
                    return Constants.EVENT_DATABASE;
                case eEvent.EVENT_COUCHBASE:
                    return Constants.EVENT_COUCHBASE;
                case eEvent.EVENT_ELASTIC:
                    return Constants.EVENT_ELASTIC;
                case eEvent.EVENT_RABBITMQ:
                    return Constants.EVENT_RABBITMQ;
                case eEvent.EVENT_SPHINX:
                    return Constants.EVENT_SPHINX;
                case eEvent.EVENT_CONNTOOK:
                    return Constants.EVENT_CONNTOOK;
                case eEvent.EVENT_DUMPFILE:
                    return Constants.EVENT_DUMPFILE;
                case eEvent.EVENT_WS:
                    return Constants.EVENT_WS;
                default:
                    break;
            }
            return null;
        }
    }
}
