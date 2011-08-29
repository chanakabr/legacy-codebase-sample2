using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using TVPApi;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.PersonalLastWatched;

namespace TVPApiModule.DataLoaders
{
    [Serializable]
    class APILastWatchedLoader : LastWatchedLoader
    {
        private string m_tvmUser;
        private string m_tvmPass;

        public APILastWatchedLoader() : base()
        {
        }

        public APILastWatchedLoader(string tvmUser, string tvmPass)
        {
            m_tvmUser = tvmUser;
            m_tvmPass = tvmPass;
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

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            PersonalLastWatched protocol = new PersonalLastWatched();
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


            string[] MetaNames = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.GalleryMediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
            string[] TagNames = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.GalleryMediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });


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
