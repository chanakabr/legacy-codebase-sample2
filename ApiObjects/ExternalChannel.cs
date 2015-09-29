using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ExternalChannel
    {
        public string externalId;
        public string name;
        public List<string> enrichments;

        /// <summary>
        /// KSQL expression with personalized filtering
        /// </summary>
        public string filterExpression;
    }
}
