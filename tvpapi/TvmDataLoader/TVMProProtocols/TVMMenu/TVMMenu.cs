using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.TVMDataLoader.Protocols.TVMMenu
{
    
    public partial class TVMMenu : Protocols.Protocol
    {
        protected override void PreSerialize()
        {
            this.MakeSchemaCompliant();
            base.PreSerialize();
        }

        protected override eProtocolType GetProtocolType()
        {
            return eProtocolType.Read;
        }

        public override bool IsTVMProProtocol()
        {
            return true;
        }
    }
}
