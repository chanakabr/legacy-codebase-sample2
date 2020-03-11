using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tvinci.Data.TVMDataLoader.Protocols.Shared;
using System.Configuration;

namespace Tvinci.Data.TVMDataLoader.Protocols.MediaInfo
{
	public partial class MediaInfo : Protocol
	{
	        
        //public static MediaInfo CreateProtocol(string[] mediaIDList, string[] imageSize)
        //{
        //    MediaInfo item = new MediaInfo();            

        //    if (imageSize != null)
        //    {
        //        if (imageSize.Length == 1)
        //        {
        //            item.root.flashvars.pic_size1 = imageSize[0];
        //        }
        //        else if (imageSize.Length == 2)
        //        {
        //            item.root.flashvars.pic_size1 = imageSize[0];
        //            item.root.flashvars.pic_size2 = imageSize[1];
        //        }
        //    }
            
        //    foreach (string mediaID in mediaIDList)
        //    {
        //        item.root.request.mediaCollection.Add(new media() { id = mediaID });
        //    }

        //    return item;
        //}

        //public static MediaInfo CreateProtocol(string[] mediaIDList)
        //{
        //    return CreateProtocol(mediaIDList, null);
        //}

        //public static  MediaInfo CreateProtocol(string  mediaID)
        //{
        //    MediaInfo item = new MediaInfo();            
        //    item.root.request.mediaCollection.Add(new media() { id = mediaID });

        //    return item;
        //}
        //public override string PreResponseProcess(string originalResponse)
        //{
        //    string result = Regex.Replace(originalResponse, "[<]media_info ", "<media ");
        //    return Regex.Replace(result, "[<]/media_info", "</media");
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
