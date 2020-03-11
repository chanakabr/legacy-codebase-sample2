using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Core.Catalog.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LogTopicAttribute : Attribute
    {
        public string TopicName { get; set; }

        public LogTopicAttribute(string topicName)
        {
            TopicName = topicName;
        }
    }
}
