using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader.Protocols;

namespace Tvinci.Data.TVMDataLoader.Protocols.GroupChannels
{
    public partial class GroupChannels : Protocol
    {
        protected override void PreSerialize()
        {
            this.MakeSchemaCompliant();
            base.PreSerialize();
        }

        protected override Protocol.eProtocolType GetProtocolType()
        {
            return Protocol.eProtocolType.Read;
        }

        public override bool IsTVMProProtocol()
        {
            return true;
        }
    }
}
