using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.TVMDataLoader.Protocols.SingleMedia
{
    public partial class SingleMedia : Protocol
    {            
        protected override void PreSerialize()
        {
            this.MakeSchemaCompliant();

            if (string.IsNullOrEmpty(this.root.flashvars.no_cache))
                this.root.flashvars.no_cache = base.getCacheValue(this.root.flashvars.no_cache);

            if (string.IsNullOrEmpty(this.root.flashvars.player_un))
                this.root.flashvars.player_un = base.getTVMUserValue();

            if (string.IsNullOrEmpty(this.root.flashvars.player_pass))
                this.root.flashvars.player_pass = base.getTVMPasswordValue();

            if (string.IsNullOrEmpty(this.root.flashvars.lang))
                this.root.flashvars.lang = base.getLanguageValue();

            this.root.flashvars.zip = base.getUseZipValue();

            base.PreSerialize();
        }

		protected override eProtocolType GetProtocolType()
		{
			return eProtocolType.Read;
		}
	}
}
