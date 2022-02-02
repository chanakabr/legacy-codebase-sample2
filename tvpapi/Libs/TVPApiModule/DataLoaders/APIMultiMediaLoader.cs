using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.TVMDataLoader.Protocols.SingleMedia;
using TVPApi;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.DataEntities;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Manager;
using Phx.Lib.Appconfig;

namespace TVPApiModule.DataLoaders
{
    public class APIMultiMediaLoader : TVMMultiMediaLoader
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

        #region Constractor
        public APIMultiMediaLoader(string[] MediaList, string PictureSize, int MediaTypeId) : base (MediaList, PictureSize, MediaTypeId)
        {
        }

        public APIMultiMediaLoader(string tvmUn, string tvmPass, string[] MediaList, string PictureSize, int MediaTypeId) : base(tvmUn, tvmPass, MediaList, PictureSize, MediaTypeId)
        {
        }

        
        #endregion Constractor

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                List<int> mediaIDs = new List<int>();
                foreach (var id in MediaArrayID)
                {
                    mediaIDs.Add(int.Parse(id));
                }

                return new TVPApiModule.CatalogLoaders.APIMediaLoader(mediaIDs, SiteMapManager.GetInstance.GetPageData(GroupID, Platform).GetTVMAccountByUser(TvmUser).BaseGroupID, GroupID, Platform.ToString(), SiteHelper.GetClientIP(), PicSize)
                {
                    OnlyActiveMedia = true,
                    Platform = Platform.ToString(),
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
            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL))
                (base.GetProvider() as Tvinci.Data.TVMDataLoader.TVMProvider).TVMAltURL = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL;

            base.PreExecute();

            this.FlashVarsFileFormat = GroupsManager.GetGroup(GroupID).GetFlashVars(Platform).FileFormat;
            this.FlashVarsSubFileFormat = GroupsManager.GetGroup(GroupID).GetFlashVars(Platform).SubFileFormat;
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            SingleMedia result = new SingleMedia();
            if (MediaArrayID != null)
            {
                foreach (string media in MediaArrayID)
                {
                    result.root.request.mediaCollection.Add(new Tvinci.Data.TVMDataLoader.Protocols.SingleMedia.media() { id = media });
                }

                result.root.request.@params.with_info = "true";

                result.root.flashvars.player_un = TvmUser;
                result.root.flashvars.player_pass = TvmPass;

                // views / rating
                result.root.request.@params.info_struct.statistics = true;
                // Type
                result.root.request.@params.info_struct.type.MakeSchemaCompliant();
                result.root.flashvars.pic_size1 = PicSize;

                //Set the response info_struct
                result.root.request.@params.info_struct.statistics = true;
                result.root.request.@params.info_struct.name.MakeSchemaCompliant();
                result.root.request.@params.info_struct.description.MakeSchemaCompliant();
                result.root.request.@params.info_struct.type.MakeSchemaCompliant();

                //result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = "Season" });
                //result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = "Episode Number" });
                ////result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = "Episode Name" });
                //// META1_STR_NAME
                //result.root.request.@params.info_struct.tags.Add(new tag_type { name = "Series Name" });

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

                if (IsPosterPic)
                {
                    result.root.flashvars.pic_size1_format = "POSTER";
                    result.root.flashvars.pic_size1_quality = "HIGH";
                }

                result.root.flashvars.file_quality = file_quality.high;
                result.root.flashvars.file_format = GroupsManager.GetGroup(GroupID).GetFlashVars(Platform).FileFormat;

                if (!string.IsNullOrEmpty(GroupsManager.GetGroup(GroupID).GetFlashVars(Platform).SubFileFormat))
                {
                    result.root.flashvars.sub_file_format = GroupsManager.GetGroup(GroupID).GetFlashVars(Platform).SubFileFormat;
                }

            }
            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{ADC197AB-C9C8-4cf9-B132-6BE64AEBBE0A}"); }
        }
    }
}
