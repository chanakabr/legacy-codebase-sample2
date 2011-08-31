using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using TVPApi;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.UserSocialMedias;

namespace TVPApiModule.DataLoaders
{
    public class APIUserSocialMediasLoader : UserSocialMediasLoader
    {
        private string m_tvmUser = string.Empty;
        private string m_tvmPass = string.Empty;

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

        public override bool ShouldExtractItemsCountInSource
        {
            get
            {
                return true;
            }
        }

        public APIUserSocialMediasLoader(string sPicSize) : base(sPicSize)
        {

        }

        public APIUserSocialMediasLoader(string wsUser, string wsPass, string sPicSize)
            : base(wsUser, wsPass, sPicSize)
        {
            m_tvmUser = wsUser;
            m_tvmPass = wsPass;
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            UserSocialMedias result = new UserSocialMedias();

            result.root.request.@params.site_guid = SiteGuid;
            result.root.request.@params.with_file_types = WithFileTypes.ToString();
            result.root.request.@params.social_action = (SocialAction).ToString();
            result.root.request.@params.social_platform = SocialPlatform.ToString();
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();
            result.root.request.@params.info_struct.description.MakeSchemaCompliant();
            result.root.flashvars.no_cache = "1";

            result.root.flashvars.player_un = m_tvmUser;
            result.root.flashvars.player_pass = m_tvmPass;

            result.root.request.channel.number_of_items = PageSize;
            result.root.request.channel.start_index = PageIndex;

            // Type
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();
            result.root.flashvars.pic_size1 = PicSize;

            if (IsPosterPic)
            {
                result.root.flashvars.pic_size1_format = "POSTER";
                result.root.flashvars.pic_size1_quality = "HIGH";
            }

            result.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.file_quality = file_quality.high;

            if (WithInfo)
            {
                string[] arrMetas = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                foreach (string metaName in arrMetas)
                {
                    result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = metaName });
                }

                string[] arrTags = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
                foreach (string tagName in arrTags)
                {
                    result.root.request.@params.info_struct.tags.tag_typeCollection.Add(new tag_type() { name = tagName });
                }
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{5D474FCB-9AF7-467A-97A1-51AB78C9399E}"); }
        }
    }
}
