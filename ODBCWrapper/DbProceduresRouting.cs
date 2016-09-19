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
                    if (!this.VersionsToExclude.Contains(version.ToUpper()))
                    {
                        this.VersionsToExclude.Add(version..ToUpper());
                    }
                }
            }
        }
    }

}