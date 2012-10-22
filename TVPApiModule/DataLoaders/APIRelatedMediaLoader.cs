using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.SearchRelated;
using Tvinci.Data.TVMDataLoader.Protocols;

namespace TVPApi 
{
    public class APIRelatedMediaLoader : TVPPro.SiteManager.DataLoaders.RelatedMoviesLoader
    {

        public APIRelatedMediaLoader(long mediaID) : this(mediaID, string.Empty, string.Empty)
        {
        }

        public APIRelatedMediaLoader(long mediaID, string userName, string pass) : base(mediaID, userName, pass)
        {
            
        }

        public override bool ShouldExtractItemsCountInSource
        {
            get
            {
                return true;
            }
        }

        public int GroupID
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "GroupID", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "GroupID", value);
            }
        }

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
        public string DeviceUDID
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Filter, "DeviceUDID", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Filter, "DeviceUDID", value);
            }
        }

        protected override bool TryGetItemsCountInSource(object retrievedData, out long count)
        {
            count = 0;

            if (retrievedData == null)
                return false;

            SearchRelated result = retrievedData as SearchRelated;

            if (result.response.channel.media_count == null)
                return false;

            count = long.Parse(result.response.channel.media_count);
            if (count != 0)
            {
                count++;
            }
            return true;
        }

        protected override IProtocol CreateProtocol()
        {
            SearchRelated protocol = new SearchRelated();
            protocol.root.request.media.id = MediaID.ToString();

            protocol.root.request.channel.start_index = PageIndex.ToString();
            protocol.root.request.channel.number_of_items = PageSize.ToString();
            protocol.root.flashvars.pic_size1 = PicSize;
            protocol.root.request.@params.with_info = "true";
            protocol.root.flashvars.player_un = TvmUser;
            protocol.root.flashvars.player_pass = TvmPass;
            protocol.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            protocol.root.flashvars.file_quality = file_quality.high;
            protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.description.MakeSchemaCompliant();
            protocol.root.flashvars.device_udid = DeviceUDID;

            if (IsPosterPic)
            {
                protocol.root.flashvars.pic_size1_format = "POSTER";
                protocol.root.flashvars.pic_size1_quality = "HIGH";
            }

            if (WithInfo)
            {
                string[] arrMetas = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                foreach (string metaName in arrMetas)
                {
                    protocol.root.request.@params.info_struct.metaCollection.Add(new meta() { name = metaName });
                }

                string[] arrTags = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
                foreach (string tagName in arrTags)
                {
                    protocol.root.request.@params.info_struct.tags.tag_typeCollection.Add(new tag_type() { name = tagName });
                }
            }
            //if (WithInfo)
            //{
            //    protocol.root.request.@params.info_struct.metaCollection.Add(new meta() { name = "Description (Short)" });
            //}

            return protocol;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{F84E2DE7-317B-4022-95B3-A5C4AB6F699E}"); }
        }
    }
}
