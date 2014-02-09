using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.TVMDataLoader.Protocols.MediaStructure
{
    public partial class MediaStructure : Protocol
    {
        protected override void PreSerialize()
        {
            this.MakeSchemaCompliant();

            if (string.IsNullOrEmpty(this.root.flashvars.player_un))
                this.root.flashvars.player_un = base.getTVMUserValue();

            if (string.IsNullOrEmpty(this.root.flashvars.player_pass))
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
