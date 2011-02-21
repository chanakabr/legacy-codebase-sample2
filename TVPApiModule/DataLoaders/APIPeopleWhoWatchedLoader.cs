using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.TVMDataLoader.Protocols.PeopleWhoWatched;
using Tvinci.Data.DataLoader;

namespace TVPApi
{
    public class APIPeopleWhoWatchedLoader : PeopleWhoWatchedLoader
    {

        private string m_tvmUser;
        private string m_tvmPass;

        public APIPeopleWhoWatchedLoader(string tvmUser, string tvmPass, long mediaID, string picSize)
            : base(mediaID, picSize)
        {
            m_tvmUser = tvmUser;
            m_tvmPass = tvmPass;
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

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            PeopleWhoWatched result = new PeopleWhoWatched();

            result.root.request.media.id = MediaID;

            result.root.flashvars.pic_size1 = PictureSize;

            result.root.request.@params.with_info = "true";


            // views / rating
            result.root.request.@params.info_struct.statistics = true;
            // Type
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();

            if (IsPosterPic)
            {
                result.root.flashvars.pic_size1_format = "POSTER";
                result.root.flashvars.pic_size1_quality = "HIGH";
            }

            result.root.flashvars.player_un = m_tvmUser;
            result.root.flashvars.player_pass = m_tvmPass;

            result.root.flashvars.file_format = ConfigManager.GetInstance(GroupID, Platform.ToString()).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.file_quality = file_quality.high;
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();

            if (WithInfo)
            {
                string[] arrMetas = ConfigManager.GetInstance(GroupID, Platform.ToString()).MediaConfiguration.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                foreach (string metaName in arrMetas)
                {
                    result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = metaName });
                }

                string[] arrTags = ConfigManager.GetInstance(GroupID, Platform.ToString()).MediaConfiguration.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
                foreach (string tagName in arrTags)
                {
                    result.root.request.@params.info_struct.tags.tag_typeCollection.Add(new tag_type() { name = tagName });
                }
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{0530CA89-3289-4b54-B470-4AA2816CA147}"); }
        }

    }
}
