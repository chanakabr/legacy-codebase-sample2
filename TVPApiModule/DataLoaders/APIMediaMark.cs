using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;

namespace TVPApiModule.DataLoaders
{
    [Serializable]
    public class APIMediaMark : TVPPro.SiteManager.DataLoaders.MediaMarkLoader
    {
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

        public APIMediaMark()
            : this(string.Empty, string.Empty)
        {
        }

        public APIMediaMark(string tvmUn, string tvmPass)
            : base(tvmUn, tvmPass)
        {
            this.TvmPass = tvmPass;
            this.TvmUser = tvmUn;
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            MediaMark result = new MediaMark();
            result.root.request.mark.action = Action;
            result.root.request.mark.location_sec = Location.ToString();
            result.root.request.mark.media.id = MediaID.ToString();
            result.root.request.mark.media.orig_file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.request.mark.media.file_quality = file_quality.high;
            result.root.request.mark.device_udid = DeviceUDID;

            result.root.flashvars.lang = Language;

            result.root.flashvars.player_un = TvmUser;
            result.root.flashvars.player_pass = TvmPass;
            result.root.flashvars.file_quality = file_quality2.high;
            result.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.site_guid = SiteGUID;

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{2E71D27C-37B7-4D5E-ACDA-0149AF694BFB}"); }
        }
    }
}
