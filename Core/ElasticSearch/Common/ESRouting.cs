using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common
{
    public class ESRouting
    {
        public bool required { get; set; }
        public string path { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"_routing\": {");
            string sRequired = required ? "true" : "false";
            sb.AppendFormat("\"required\": {0}", sRequired);

            if (!string.IsNullOrEmpty(path))
            {
                sb.AppendFormat(", \"path\": \"{0}\"", path);
            }

            sb.Append("}");
            
            return sb.ToString();
        }
    }
}
