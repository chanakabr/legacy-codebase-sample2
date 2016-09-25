using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODBCWrapper
{
    public class DbProceduresRouting
    {

        public Dictionary<string, ProcedureRoutingInfo> ProceduresMapping { get; set; }

        public DbProceduresRouting()
        {
            this.ProceduresMapping = new Dictionary<string, ProcedureRoutingInfo>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{\n");
            foreach (KeyValuePair<string, ProcedureRoutingInfo> spRoute in ProceduresMapping)
            {
                sb.Append(string.Format("{{Name: {0}", spRoute.Key));
                sb.Append(string.Format("Info: {0}", spRoute.Value.ToString()));
            }

            sb.Append("\n}");
            return sb.ToString();
        }
    }

    public class ProcedureRoutingInfo
    {

        public bool IsWritable { get; set; }

        public HashSet<string> VersionsToExclude { get; set; }

        public ProcedureRoutingInfo()
        {
            this.IsWritable = false;
            this.VersionsToExclude = new HashSet<string>();
        }

        public ProcedureRoutingInfo(bool isWritable, string versionsToExclude)
        {
            this.IsWritable = isWritable;            
            this.VersionsToExclude = new HashSet<string>();
            if (!string.IsNullOrEmpty(versionsToExclude))
            {
                string[] versions = versionsToExclude.Split(';');
                foreach (string version in versions)
                {
                    if (!this.VersionsToExclude.Contains(version.ToLower()))
                    {
                        this.VersionsToExclude.Add(version.ToLower());
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("IsWriteable - {0}, ", IsWritable));
            sb.Append("VersionsToExclude - ");
            foreach (string version in VersionsToExclude)
            {
                sb.Append(string.Format("{0}, ", version));
            }
            sb.Append("}");

            return sb.ToString();
        }
    }

}