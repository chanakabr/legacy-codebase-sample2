using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;

namespace Logger
{
    public class AMQPAppender : log4net.Appender.AppenderSkeleton
    {
        // Note: all members' values are loaded from the log4net.config file. Please be aware that the members name must be compatible to those you defined 
        //       in the config file!
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private string password;
        private string userName;
        private string virtualHost;
        private string hostName;
        private string requestedHeartbeat;
        private string port;
        private string routingKey;
        private string flushInterval;
        private string exchange;
        private string queue;
        private string exchangeType;

        public string Password
        {
            get { return this.password; }
            set { this.password = value; }
        }

        public string Username
        {
            get { return this.userName; }
            set { this.userName = value; }
        }

        public string VirtualHost
        {
            get { return this.virtualHost; }
            set { this.virtualHost = value; }
        }

        public string HostName
        {
            get { return this.hostName; }
            set { this.hostName = value; }
        }

        public string RequestedHeartbeat
        {
            get { return this.requestedHeartbeat; }
            set { this.requestedHeartbeat = value; }
        }

        public string RoutingKey
        {
            get { return this.routingKey; }
            set { this.routingKey = value; }
        }

        public string FlushInterval
        {
            get { return this.flushInterval; }
            set { this.flushInterval = value; }
        }

        public string Port
        {
            get { return this.port; }
            set { this.port = value; }
        }

        public string Exchange
        {
            get { return this.exchange; }
            set { this.exchange = value; }
        }

        public string Queue
        {
            get { return this.queue; }
            set { this.queue = value; }
        }

        public string ExchangeType
        {
            get { return this.exchangeType; }
            set { this.exchangeType = value; }
        }

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            Level logLevel = Level.Error;
            switch (loggingEvent.Level.Name)
            {
                case "DEBUG":
                    logLevel = Level.Debug;
                    break;
                case "WARN":
                case "INFO":
                    logLevel = Level.Info;
                    break;
                case "ERROR":
                    logLevel = Level.Error;
                    break;
                case "FATAL":
                    logLevel = Level.Critical;
                    break;
            }

            string sMessageToWrite = loggingEvent.MessageObject.ToString();
        }
    }
}
