using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.TVMDataLoader.Protocols.TVC
{
    public partial class TVCProtocol : Protocol
    {
        
        protected override void PreSerialize()
        {
            this.MakeSchemaCompliant();
            this.root.flashvars.no_cache = base.getCacheValue(this.root.flashvars.no_cache);
            this.root.flashvars.player_un = base.getTVMUserValue();
            this.root.flashvars.player_pass = base.getTVMPasswordValue();
            this.root.flashvars.zip = base.getUseZipValue();

            base.PreSerialize();
        }

		protected override eProtocolType GetProtocolType()
		{
			return eProtocolType.Read;
		}
	}
}
