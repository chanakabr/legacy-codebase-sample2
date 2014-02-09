using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader.Protocols;

namespace Tvinci.Data.TVMDataLoader.Protocols.MediaInfoStruct
{
    public partial class MediaInfoStruct :Protocol
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
