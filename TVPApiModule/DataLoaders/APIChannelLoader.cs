using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.ChannelsMedia;

namespace TVPApi
{
    class APIChannelLoader : TVPPro.SiteManager.DataLoaders.TVMChannelLoader
    {

        private string m_tvmUser = string.Empty;
        private string m_tvmPass = string.Empty;

        public PlatformType Platform
        {
            get
            {
                return Parameters.GetParameter<PlatformType>(eParameterType.Retrieve, "Platform", PlatformType.Unknown);
            }
            set
            {
                Parameters.SetParameter<PlatformType>(eParameterType.Retrieve, "Platform", value);
            }

        }

        public override bool ShouldExtractItemsCountInSource
        {
            get
            {
                return true;
            }
        }

        protected override bool TryGetItemsCountInSource(object retrievedData, out long count)
        {
            count = 0;

            if (retrievedData == null)
                return false;

            ChannelsMedia result = retrievedData as ChannelsMedia;

            if (result.response.channelCollection.Count == 0)
            {
                count = 0;
                return true;
            }
            count = long.Parse(result.response.channelCollection[0].media_count);

            return true;
        }

        public APIChannelLoader(long channelID, string picSize) 
			: base(string.Empty, string.Empty, channelID, picSize)
		{
			// Do nothing.
		}

       

        public APIChannelLoader(string TVMUser, string TVMPass, long channelID, string picSize):base(TVMUser,TVMPass,channelID, picSize)
        {
			m_tvmUser = TVMUser;
			m_tvmPass = TVMPass;

            if (string.IsNullOrEmpty(picSize))
            {
                throw new Exception("Picture size is null or empty");
            }

            PicSize = picSize;
            ChannelID = channelID;
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            ChannelsMedia result = new ChannelsMedia();

            channel newChannel = new channel();
            newChannel.id = int.Parse(ChannelID.ToString());
            newChannel.number_of_items = PageSize;
            newChannel.start_index = PageIndex;
            result.root.request.channelCollection.Add(newChannel);

            result.root.flashvars.player_un = m_tvmUser;
            result.root.flashvars.player_pass = m_tvmPass;

            result.root.flashvars.pic_size1 = PicSize;

            if (IsPosterPic)
            {
                result.root.flashvars.pic_size1_format = "POSTER";
                result.root.flashvars.pic_size1_quality = "HIGH";
            }

            result.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform.ToString()).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.file_quality = file_quality.high;
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();
            result.root.request.@params.info_struct.description.MakeSchemaCompliant();

            if (WithInfo)
            {
                string[] arrMetas = ConfigManager.GetInstance().GetConfig(GroupID, Platform.ToString()).MediaConfiguration.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                foreach (string metaName in arrMetas)
                {
                    result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = metaName });
                }

                string[] arrTags = ConfigManager.GetInstance().GetConfig(GroupID, Platform.ToString()).MediaConfiguration.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
                foreach (string tagName in arrTags)
                {
                    result.root.request.@params.info_struct.tags.tag_typeCollection.Add(new tag_type() { name = tagName });
                }
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{698C7873-35F1-4137-86E9-1C13C9CCD744}"); }
        }
    }
}
