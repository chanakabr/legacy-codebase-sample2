using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public class NPVRProviderImp
    {
        public NPVRProvider npvrProvider { get; set; }
        public bool synchronizeNpvrWithDomain { get; set; }

        public NPVRProviderImp(NPVRProvider provider, bool synchronizeNpvrWithDomain)
        {
            this.npvrProvider = provider;
            this.synchronizeNpvrWithDomain = synchronizeNpvrWithDomain;
        }
    }
}
