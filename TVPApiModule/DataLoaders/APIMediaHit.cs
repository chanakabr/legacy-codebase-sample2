using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.MediaHit;

namespace TVPApiModule.DataLoaders
{
    [Serializable]
    public class APIMediaHit : TVPPro.SiteManager.DataLoaders.MediaHitLoader
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

        public APIMediaHit()
            : this(string.Empty, string.Empty)
        {
        }

        public APIMediaHit(string tvmUn, string tvmPass)
            : base(tvmUn, tvmPass)
        {
            this.m_tvmPass = tvmPass;
            this.m_tvmUser = tvmUn;
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            MediaHit result = new MediaHit();
            result.root.request.watching.location_sec = Location.ToString();
            result.root.request.watching.media.id = MediaID.ToString();
            result.root.request.watching.media.orig_file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.request.watching.media.file_quality = file_quality.high;
            result.root.request.watching.device_udid = DeviceUDID;

            result.root.flashvars.lang = Language;

            result.root.flashvars.player_un = m_tvmUser;
            result.root.flashvars.player_pass = m_tvmPass;
            result.root.flashvars.file_quality = file_quality2.high;
            result.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.site_guid = SiteGUID;

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{FA7556D9-3A7C-42C6-B296-6531E29A61C1}"); }
        }
    }
}
