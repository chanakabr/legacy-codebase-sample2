using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.MediaHit;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.Services;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class MediaHitLoader : TVMAdapter<string>
    {        
        private FlashLoadersParams m_FlashLoadersParams;

        #region properties

        public long MediaID
        {
            get
            {
                return Parameters.GetParameter<long>(eParameterType.Retrieve, "MediaID", 0);
            }
            set
            {
                Parameters.SetParameter<long>(eParameterType.Retrieve, "MediaID", value);
            }
        }

        public long FileID
        {
            get
            {
                return Parameters.GetParameter<long>(eParameterType.Retrieve, "FileID", 0);
            }
            set
            {
                Parameters.SetParameter<long>(eParameterType.Retrieve, "FileID", value);
            }
        }

        public String Duration
        {
            get
            {
                return Parameters.GetParameter<String>(eParameterType.Retrieve, "Duration", "0");
            }
            set
            {
                Parameters.SetParameter<String>(eParameterType.Retrieve, "Duration", value);
            }
        }

        public String Billing
        {
            get
            {
                return Parameters.GetParameter<String>(eParameterType.Retrieve, "Billing", "");
            }
            set
            {
                Parameters.SetParameter<String>(eParameterType.Retrieve, "Billing", value);
            }
        }

        public String CDNID
        {
            get
            {
                return Parameters.GetParameter<String>(eParameterType.Retrieve, "CDNID", "");
            }
            set
            {
                Parameters.SetParameter<String>(eParameterType.Retrieve, "CDNID", value);
            }
        }

        public String OrgFileFormat
        {
            get
            {
                return Parameters.GetParameter<String>(eParameterType.Retrieve, "OrgFileFormat", "");
            }
            set
            {
                Parameters.SetParameter<String>(eParameterType.Retrieve, "OrgFileFormat", value);
            }
        }

        public int Location
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "Location", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "Location", value);
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

        public string DeviceUDID
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "DeviceUDID", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "DeviceUDID", value);
            }
        }

        public string SiteGUID
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "SiteGUID", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "SiteGUID", value);
            }
        }

        public string Action
        {
            get
            {
                return Parameters.GetParameter<String>(eParameterType.Retrieve, "Action", string.Empty);
            }
            set
            {
                Parameters.SetParameter<String>(eParameterType.Retrieve, "Action", value);
            }
        }

        public long ProgramId
        {
            get
            {
                return Parameters.GetParameter<long>(eParameterType.Retrieve, "ProgramId", 0);
            }
            set
            {
                Parameters.SetParameter<long>(eParameterType.Retrieve, "ProgramId", value);
            }
        }

        #endregion

        protected override bool ShouldStoreInCache(LoaderAdapterItem result)
        {
            return false;
        }

        protected override bool ShouldExtractFromCache(string cacheKey)
        {
            return false;
        }

        public MediaHitLoader()
            : this(string.Empty, string.Empty, default(FlashLoadersParams))
        {
        }

        public MediaHitLoader(string tvmUn, string tvmPass,FlashLoadersParams flashVars)
        {
            this.TvmPass = tvmPass;
            this.TvmUser = tvmUn;
            m_FlashLoadersParams = flashVars;
        }

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

        protected string NPVRID
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "NPVRID", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "NPVRID", value);
            }
        }

        public override eCacheMode GetCacheMode()
        {
            return eCacheMode.Never;
        }
        
        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override string Execute()
        {
            bool shouldUseNewCache;
            if (bool.TryParse(ConfigurationManager.AppSettings["ShouldUseNewCache"], out shouldUseNewCache) && shouldUseNewCache)
            {
                CatalogLoaders.MediaHitLoader mediaMarkLoader =
                    new CatalogLoaders.MediaHitLoader(PageData.Instance.GetTVMAccountByUserName(TvmUser).BaseGroupID, SiteHelper.GetClientIP(), SiteGUID, DeviceUDID, 
                                                      (int)MediaID, (int)FileID, NPVRID, AvgBitRate, CurrentBitRate, Location, TotalBitRateNum, Action, Duration, ProgramId);

                return mediaMarkLoader.Execute() as string;
            }
            else
            {
                return base.Execute();
            }
        }
        
        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            MediaHit result = new MediaHit();

            result.root.request.type = "hit";
            result.root.request.action = Action;
            result.root.request.watching.location_sec = Location.ToString();

            result.root.request.watching.media.id = MediaID.ToString();
            result.root.request.watching.media.duration = Duration;
            result.root.request.watching.media.file_id = FileID.ToString();
            result.root.request.watching.media.billing = Billing;
            result.root.request.watching.device_udid = DeviceUDID;
            result.root.request.watching.media.cdn_id = CDNID;
            result.root.request.watching.media.file_quality = file_quality.high;
            result.root.request.watching.media.orig_file_format = OrgFileFormat;
            //result.root.request.watching.media.avg_bit_rate_num = AvgBitRate.ToString();
            //result.root.request.watching.media.current_bit_rate_num = CurrentBitRate.ToString();
            //result.root.request.watching.media.total_bit_rate_num = TotalBitRateNum.ToString();

            result.root.flashvars.user_ip = this.m_FlashLoadersParams.UserIP;
            result.root.flashvars.player_un = this.TvmUser;
            result.root.flashvars.player_pass = this.TvmPass;
            result.root.flashvars.pic_size1 = this.m_FlashLoadersParams.Pic1Size;
            result.root.flashvars.pic_size2 = this.m_FlashLoadersParams.Pic2Size;
            result.root.flashvars.pic_size3 = this.m_FlashLoadersParams.Pic3Size;
            result.root.flashvars.lang = this.m_FlashLoadersParams.Language;
            result.root.flashvars.no_cache = "1";
            result.root.flashvars.zip = "1";
            result.root.flashvars.site_guid = UsersService.Instance.GetUserID();

            result.root.request.MakeSchemaCompliant();   

            return result;
        }

        protected override string PreCacheHandling(object retrievedData)
        {
            return (retrievedData as MediaHit).response.type.ToString();
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{E31449B5-8503-4195-8B14-FCB12D01E6C8}"); }
        }
    }
}

