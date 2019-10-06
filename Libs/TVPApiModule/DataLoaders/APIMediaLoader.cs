using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.SingleMedia;
using TVPPro.SiteManager.DataEntities;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Manager;
using ConfigurationManager;

namespace TVPApi
{
    [Serializable]
    public class APIMediaLoader : TVPPro.SiteManager.DataLoaders.TVMMediaLoader
    {
        public string SiteGuid
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "SiteGuid", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "SiteGuid", value);
            }
        }
        public string Language
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "Language", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "Language", value);
            }
        }

        public int GroupID
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "GroupID", -1);
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

        public APIMediaLoader(string mediaID)
            : this(string.Empty, string.Empty, mediaID)
        {
        }

        public APIMediaLoader(string tvmUn, string tvmPass, string mediaID)
            : base(tvmUn, tvmPass, mediaID)
        {

        }

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            
            if (ApplicationConfiguration.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return new TVPApiModule.CatalogLoaders.APIMediaLoader(int.Parse(MediaID), SiteMapManager.GetInstance.GetPageData(GroupID, Platform).GetTVMAccountByUser(TvmUser).BaseGroupID, GroupID, Platform.ToString(), SiteHelper.GetClientIP(), PicSize)
                {
                    DeviceId = DeviceUDID,
                    OnlyActiveMedia = true,
                    Platform = Platform.ToString(),
                    UseFinalDate = bool.Parse(UseFinalEndDate),
                    UseStartDate = bool.Parse(GetFutureStartDate),
                    Culture = Language,
                    SiteGuid = SiteGuid
                }.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
            }
        }

        protected override void PreExecute()
        {
            if(!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL))
                (base.GetProvider() as Tvinci.Data.TVMDataLoader.TVMProvider).TVMAltURL = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL;

            base.PreExecute();
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {

            SingleMedia result = new SingleMedia();
            result.root.request.mediaCollection.Add(new media() { id = MediaID });
            if (string.IsNullOrEmpty(BrandingFileFormat))
            {
                result.root.flashvars.file_quality = file_quality.high;
                result.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            }
            else // for brandig media
            {
                result.root.flashvars.pic_size3_quality = "HIGH";
                result.root.flashvars.pic_size3_format = BrandingFileFormat;
                result.root.flashvars.pic_size3 = BrandingPicSize;
                result.root.flashvars.pic_size2 = "full";
                result.root.flashvars.pic_size2_quality = "HIGH";
                result.root.flashvars.pic_size2_format = RepeatBrandingPicFormat;
            }

            result.root.flashvars.lang = Language;

            result.root.flashvars.player_un = TvmUser;
            result.root.flashvars.player_pass = TvmPass;
            result.root.flashvars.use_final_end_date = UseFinalEndDate;

            result.root.request.@params.with_info = "true";
            result.root.flashvars.file_quality = file_quality.high;
            result.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;

            result.root.flashvars.no_file_url = ConfigManager.GetInstance().GetConfig(GroupID, Platform).SiteConfiguration.Data.Features.EncryptMediaFileURL;

            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.SubFileFormat))
            {
                result.root.flashvars.sub_file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.SubFileFormat;
            }
            
            //result.root.flashvars.pic_size1_format = TechnicalConfiguration.Instance.Data.TVM.FlashVars.FileFormat;
            //result.root.flashvars.pic_size1_quality = "HIGH";
            result.root.flashvars.pic_size1 = PicSize;
            result.root.flashvars.device_udid = DeviceUDID;
            //Set the response info_struct
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.name.MakeSchemaCompliant();
            result.root.request.@params.info_struct.description.MakeSchemaCompliant();
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();

            string[] MetaNames = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
            string[] TagNames = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });

            foreach (string meta in MetaNames)
            {
                result.root.request.@params.info_struct.metaCollection.Add(new meta { name = meta });
            }

            foreach (string tagName in TagNames)
            {
                result.root.request.@params.info_struct.tags.Add(new tag_type { name = tagName });
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{112EA03B-E369-4088-9998-662BD9C2F91E}"); }
        }
    }
}
