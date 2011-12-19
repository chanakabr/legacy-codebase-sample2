using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using TVPApi;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.SubscriptionMedia;

namespace TVPApiModule.DataLoaders
{
    public class APITVMSubscriptionMediaLoader : TVMSubscriptionMediaLoader
    {
        private string m_tvmUser = string.Empty;
        private string m_tvmPass = string.Empty;
        private long m_BaseID;

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

        public APITVMSubscriptionMediaLoader(long BaseID)
            : base(BaseID)
        {
            m_BaseID = BaseID;
        }

        public APITVMSubscriptionMediaLoader(string TVMUser, string TVMPass, long BaseID) : base(TVMUser, TVMPass, BaseID)
        {
            m_tvmUser = TVMUser;
            m_tvmPass = TVMPass;
            m_BaseID = BaseID;
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            SubscriptionMedia result = new SubscriptionMedia();

            subscription sub = new subscription();
            sub.id = this.m_BaseID;
            sub.number_of_items = PageSize.ToString();
            sub.start_index = (PageIndex * PageSize).ToString();
            result.root.request.subscription = sub;

            result.root.flashvars.player_un = m_tvmUser;
            result.root.flashvars.player_pass = m_tvmPass;

            result.root.flashvars.pic_size1 = PicSize;
            result.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.file_quality = file_quality.high;
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();
            result.root.request.@params.info_struct.description.MakeSchemaCompliant();

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
            get { return new Guid("{A0E21BD5-5922-4B40-BE18-42580A70589E}"); }
        }
    }
}
