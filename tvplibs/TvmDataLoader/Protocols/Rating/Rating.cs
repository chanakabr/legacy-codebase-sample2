using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.TVMDataLoader.Protocols.Rating
{
    public partial class Rating : Protocol
    {
        //public Rating(string mediaID, int userRate)
        //{
        //    this.root.request.media.id = mediaID;
        //    this.root.request.rating.value = userRate.ToString();
        //}

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
