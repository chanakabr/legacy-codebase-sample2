using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.TVMDataLoader.Protocols.NowPlaying
{
    public partial class NowPlaying : Protocol
    {
        //public NowPlaying(string ChannelID, int PageSize)
        //{
        //    this.root.request.channel.id = ChannelID;
        //    this.root.request.channel.number_of_items = PageSize.ToString();
        //    this.root.request.@params.with_info = "true";
        //}

        protected override void PreSerialize()
        {
			this.MakeSchemaCompliant();
            this.root.flashvars.no_cache = base.getCacheValue(this.root.flashvars.no_cache);
			this.root.flashvars.player_un = base.getTVMUserValue();
			this.root.flashvars.player_pass = base.getTVMPasswordValue();
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
