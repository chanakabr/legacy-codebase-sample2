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

        public int AvgBitRate
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "AvgBitRate", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "AvgBitRate", value);
            }
        }

        public int CurrentBitRate
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "CurrentBitRate", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "CurrentBitRate", value);
            }
        }

        public int TotalBitRateNum
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "TotalBitRateNum", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "TotalBitRateNum", value);
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

        protected override void PreExecute()
        {
            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL))
                (base.GetProvider() as Tvinci.Data.TVMDataLoader.TVMProvider).TVMAltURL = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL;

            base.PreExecute();
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            //MediaMark result = new MediaMark();
            //result.root.request.mark.action = Action;
            //result.root.request.mark.location_sec = Location.ToString();
            //result.root.request.mark.media.id = MediaID.ToString();
            //result.root.request.mark.media.orig_file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            //result.root.request.mark.media.file_quality = file_quality.high;
            //result.root.request.mark.device_udid = DeviceUDID;

            //result.root.flashvars.lang = Language;

            //result.root.flashvars.player_un = TvmUser;
            //result.root.flashvars.player_pass = TvmPass;
            //result.root.flashvars.file_quality = file_quality2.high;
            //result.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            //result.root.flashvars.site_guid = SiteGUID;

            MediaMark result = new MediaMark();
            result.root.request.type = "media_mark";
            result.root.request.mark.action = Action;
            result.root.request.mark.location_sec = Location.ToString();
            result.root.request.mark.device_udid = DeviceUDID;
            //XXX: result.root.request.mark.error_code = ErrorCode;
            //XXX: result.root.request.mark.error_message = ErrorMessage;
            result.root.request.mark.media.id = MediaID.ToString();
            result.root.request.mark.media.duration = MediaDuration;
            result.root.request.mark.media.file_id = FileID.ToString();
            result.root.request.mark.media.billing = Billing;
            result.root.request.mark.media.cdn_id = CDNID;
            result.root.request.mark.media.file_quality = file_quality.high;
            result.root.request.mark.media.orig_file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.request.mark.media.avg_bit_rate_num = AvgBitRate.ToString();
            result.root.request.mark.media.current_bit_rate_num = CurrentBitRate.ToString();
            result.root.request.mark.media.total_bit_rate_num = TotalBitRateNum.ToString();

            result.root.flashvars.user_ip = (string.IsNullOrEmpty(UserIP)) ? TVPPro.SiteManager.Helper.SiteHelper.GetClientIP() : UserIP;
            result.root.flashvars.admin_token = AdminToken;
            result.root.flashvars.player_un = this.TvmUser;
            result.root.flashvars.player_pass = this.TvmPass;
            result.root.flashvars.pic_size1 = this.m_FlashLoadersParams.Pic1Size;
            result.root.flashvars.pic_size2 = this.m_FlashLoadersParams.Pic2Size;
            result.root.flashvars.pic_size3 = this.m_FlashLoadersParams.Pic3Size;
            result.root.flashvars.lang = this.m_FlashLoadersParams.Language;
            result.root.flashvars.no_cache = "1";
            result.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.file_quality = file_quality2.high;
            result.root.flashvars.zip = "1";
            result.root.flashvars.site_guid = SiteGUID;

            result.root.request.MakeSchemaCompliant();            

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{2E71D27C-37B7-4D5E-ACDA-0149AF694BFB}"); }
        }
    }
}
