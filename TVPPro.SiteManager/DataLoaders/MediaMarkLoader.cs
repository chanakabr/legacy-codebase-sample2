using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.Services;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class MediaMarkLoader : TVMAdapter<string>
    {        
        protected FlashLoadersParams m_FlashLoadersParams;

        #region properties
        public action Action
        {
            get
            {
                return Parameters.GetParameter<action>(eParameterType.Retrieve, "MarkType", action.none);
            }
            set
            {
                Parameters.SetParameter<action>(eParameterType.Retrieve, "MarkType", value);
            }
        }

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

        public String MediaDuration
        {
            get
            {
                return Parameters.GetParameter<String>(eParameterType.Retrieve, "MediaDuration", String.Empty);
            }
            set
            {
                Parameters.SetParameter<String>(eParameterType.Retrieve, "MediaDuration", value);
            }
        }

        public String Billing
        {
            get
            {
                return Parameters.GetParameter<String>(eParameterType.Retrieve, "Billing", String.Empty);
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
                return Parameters.GetParameter<String>(eParameterType.Retrieve, "CDNID", String.Empty);
            }
            set
            {
                Parameters.SetParameter<String>(eParameterType.Retrieve, "CDNID", value);
            }
        }

        public String FileFormat
        {
            get
            {
                return Parameters.GetParameter<String>(eParameterType.Retrieve, "FileFormat", String.Empty);
            }
            set
            {
                Parameters.SetParameter<String>(eParameterType.Retrieve, "FileFormat", value);
            }
        }

        public String OrgFileFormat
        {
            get
            {
                return Parameters.GetParameter<String>(eParameterType.Retrieve, "FileQuality", String.Empty);
            }
            set
            {
                Parameters.SetParameter<String>(eParameterType.Retrieve, "FileQuality", value);
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

        public string ErrorCode
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "ErrorCode", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "ErrorCode", value);
            }
        }

        public string ErrorMessage
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "ErrorMessage", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "ErrorMessage", value);
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

        public string UserIP
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "UserIP", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "UserIP", value);
            }
        }

        public string AdminToken
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "AdminToken", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "AdminToken", value);
            }
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
        #endregion

        public override eCacheMode GetCacheMode()
        {
            return eCacheMode.Never;
        }

        protected override bool ShouldStoreInCache(LoaderAdapterItem result)
        {
            return false;
        }

        protected override bool ShouldExtractFromCache(string cacheKey)
        {
            return false;
        }

        public MediaMarkLoader(string tvmUn, string tvmPass):this(tvmUn,tvmPass,default(FlashLoadersParams))
        {            
        }

        public MediaMarkLoader(string tvmUn, string tvmPass,FlashLoadersParams flashVars)
        {
            this.TvmPass = tvmPass;
            this.TvmUser = tvmUn;
            this.m_FlashLoadersParams = flashVars;
        }

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override String Execute()
        {
            bool shouldUseNewCache;
            if (bool.TryParse(ConfigurationManager.AppSettings["ShouldUseNewCache"], out shouldUseNewCache) && shouldUseNewCache)
            {
                // TODO SHIR - ASK IRA ABOUT THE PROGRAM_ID HERE
                long programId = 0;
                CatalogLoaders.MediaMarkLoader mediaMarkLoader = 
                    new CatalogLoaders.MediaMarkLoader(PageData.Instance.GetTVMAccountByUserName(TvmUser).BaseGroupID, SiteHelper.GetClientIP(), UsersService.Instance.GetUserID(),
                                                       DeviceUDID, (int)MediaID, (int)FileID, NPVRID, AvgBitRate, CurrentBitRate, Location, TotalBitRateNum, Action.ToString(),
                                                       MediaDuration, ErrorCode, ErrorMessage, CDNID, programId);
                return mediaMarkLoader.Execute() as string;
            }
            else
            {
                return base.Execute();
            }
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            MediaMark result = new MediaMark();
            result.root.request.type = "media_mark";
            result.root.request.mark.action = Action;
            result.root.request.mark.location_sec = Location.ToString();
            //XXX: result.root.request.mark.error_code = ErrorCode;
            //XXX: result.root.request.mark.error_message = ErrorMessage;
            result.root.request.mark.device_udid = DeviceUDID;
            result.root.request.mark.media.id = MediaID.ToString();
            result.root.request.mark.media.duration = MediaDuration;
            result.root.request.mark.media.file_id = FileID.ToString();
            result.root.request.mark.media.billing = Billing;
            result.root.request.mark.media.cdn_id = CDNID;
            result.root.request.mark.media.file_quality = file_quality.high;
            result.root.request.mark.media.orig_file_format = OrgFileFormat;
            result.root.request.mark.media.avg_bit_rate_num = AvgBitRate.ToString();
            result.root.request.mark.media.current_bit_rate_num = CurrentBitRate.ToString();
            result.root.request.mark.media.total_bit_rate_num = TotalBitRateNum.ToString();

            result.root.flashvars.user_ip = (string.IsNullOrEmpty(UserIP)) ? SiteManager.Helper.SiteHelper.GetClientIP() : UserIP;
            result.root.flashvars.admin_token = AdminToken;
            result.root.flashvars.player_un = this.TvmUser;
            result.root.flashvars.player_pass = this.TvmPass;
            result.root.flashvars.pic_size1 = this.m_FlashLoadersParams.Pic1Size;
            result.root.flashvars.pic_size2 = this.m_FlashLoadersParams.Pic2Size;
            result.root.flashvars.pic_size3 = this.m_FlashLoadersParams.Pic3Size;
            result.root.flashvars.lang = this.m_FlashLoadersParams.Language;
            result.root.flashvars.no_cache = "0";
            result.root.flashvars.file_format = FileFormat;
            result.root.flashvars.file_quality = file_quality2.high;
            result.root.flashvars.zip = "1";
            result.root.flashvars.site_guid = UsersService.Instance.GetUserID();

            result.root.request.MakeSchemaCompliant();            

            return result;
        }

        protected override string PreCacheHandling(object retrievedData)
        {
            return (retrievedData as MediaMark).response.type.ToString();
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{E1FFF415-5976-4096-B333-616282E7B550}"); }
        }
    }
}
