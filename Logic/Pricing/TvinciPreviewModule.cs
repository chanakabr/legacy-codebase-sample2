using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class TvinciPreviewModule : BasePreviewModule
    {
        public TvinciPreviewModule() : base() { }
        public TvinciPreviewModule(int nGroupID) : base(nGroupID) { }
    }
}
