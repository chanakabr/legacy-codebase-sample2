using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.AssetLifeCycleRules
{
    public class LifeCycleFileTypesAndPpvsTransitions
    {
        public HashSet<int> FileTypeIds { get; set; }
        public HashSet<int> PpvIds { get; set; }

        public LifeCycleFileTypesAndPpvsTransitions()
        {
            this.FileTypeIds = new HashSet<int>();
            this.PpvIds = new HashSet<int>();
        }

        public LifeCycleFileTypesAndPpvsTransitions(HashSet<int> fileTypesIds, HashSet<int> ppvIds)
        {
            this.FileTypeIds = fileTypesIds;
            this.PpvIds = ppvIds;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("FileTypeIds: {0}, ", FileTypeIds != null ? string.Join(",", FileTypeIds) : string.Empty));
            sb.Append(string.Format("PpvIds: {0}, ", PpvIds != null ? string.Join(",", PpvIds) : string.Empty));

            return sb.ToString();
        }

    }
}
