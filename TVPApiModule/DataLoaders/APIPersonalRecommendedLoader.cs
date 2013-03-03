using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.TVMDataLoader.Protocols.PersonalRecommended;
using TVPApi;
using Tvinci.Data.DataLoader;

namespace TVPApiModule.DataLoaders
{
    class APIPersonalRecommendedLoader : PersonalRecommendedLoader
    {
        #region Parameters
        protected string TvmUser
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmUser", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmUser", value);
            }

        }
        protected string TvmPass
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmPass", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmPass", value);
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

        public int[] MediaTypes
        {
            get
            {
                return Parameters.GetParameter<int[]>(eParameterType.Retrieve, "MediaTypes", null);
            }
            set
            {
                Parameters.SetParameter<int[]>(eParameterType.Retrieve, "MediaTypes", value);
            }
        }
        #endregion

        public APIPersonalRecommendedLoader(string tvmUser, string tvmPass)
            : base(tvmUser, tvmPass)
        {
            TvmUser = tvmUser;
            TvmPass = tvmPass;
        }

        protected override void PreExecute()
        {
            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL))
                (base.GetProvider() as Tvinci.Data.TVMDataLoader.TVMProvider).TVMAltURL = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL;

            base.PreExecute();
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            PersonalRecommended protocol = new PersonalRecommended();

            protocol.root.request.channel.start_index = PageIndex.ToString();
            protocol.root.request.channel.number_of_items = PageSize.ToString();
            protocol.root.request.channel.id = "";
            protocol.root.flashvars.no_cache = "0";
            protocol.root.flashvars.pic_size1 = PicSize;

            protocol.root.request.@params.with_info = WithInfo.ToString();

            protocol.root.request.@params.info_struct.statistics = false;
            protocol.root.request.@params.info_struct.personal = true;
            protocol.root.request.@params.info_struct.name.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.description.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
            protocol.root.request.@params.site_guid = SiteGuid;

            if (MediaTypes != null)
                protocol.root.request.@params.media_types = string.Join(";", MediaTypes.Select(x => x.ToString()).ToArray());

            protocol.root.flashvars.player_un = TvmUser;
            protocol.root.flashvars.player_pass = TvmPass;

            protocol.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            protocol.root.flashvars.file_quality = file_quality.high;



            string[] MetaNames = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
            string[] TagNames = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });


            if (WithInfo)
            {
                foreach (string meta in MetaNames)
                {
                    protocol.root.request.@params.info_struct.metaCollection.Add(new meta { name = meta });
                }

                foreach (string tagName in TagNames)
                {
                    protocol.root.request.@params.info_struct.tags.Add(new tag_type { name = tagName });
                }
            }

            return protocol;
        }
    }
}
