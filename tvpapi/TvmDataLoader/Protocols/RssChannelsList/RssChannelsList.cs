using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader.Protocols.Shared;


namespace Tvinci.Data.TVMDataLoader.Protocols.RssChannelsList
{
	public partial class RssChannelsList : Protocol
	{
        public string OverridenLanguage { get; set; }

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
                this.root.flashvars.lang = string.IsNullOrEmpty(OverridenLanguage) ? base.getLanguageValue() : OverridenLanguage;

            this.root.flashvars.zip = base.getUseZipValue();

            base.PreSerialize();
        }

		protected override eProtocolType GetProtocolType()
		{
			return eProtocolType.Read;
		}
	}
}
