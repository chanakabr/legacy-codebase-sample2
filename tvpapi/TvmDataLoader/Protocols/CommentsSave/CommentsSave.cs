using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.TVMDataLoader.Protocols.CommentsSave
{
    public partial class CommentsSave : Protocol
    {
        protected override void PreSerialize()
        {
            this.MakeSchemaCompliant();
            this.root.flashvars.no_cache = base.getCacheValue(this.root.flashvars.no_cache);

			if (string.IsNullOrEmpty(this.root.flashvars.player_un))
				this.root.flashvars.player_un = base.getTVMUserValue();

			if (string.IsNullOrEmpty(this.root.flashvars.player_pass))
				this.root.flashvars.player_pass = base.getTVMPasswordValue();
            
			this.root.flashvars.lang = base.getLanguageValue();
            this.root.flashvars.zip = base.getUseZipValue();

            base.PreSerialize();
        }

		protected override eProtocolType GetProtocolType()
		{
			return eProtocolType.Write;
		}
	}
}
