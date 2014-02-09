using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader.Protocols;

namespace Tvinci.Data.TVMDataUpdater.Protocols.MediaCUDItem
{
    public partial class MediaCUDItem : Protocol
    {
        protected override void PreSerialize()
        {

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
