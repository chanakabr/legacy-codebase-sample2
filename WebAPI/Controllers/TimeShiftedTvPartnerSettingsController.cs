using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/timeShiftedTvPartnerSettings/action")]
    public class TimeShiftedTvPartnerSettingsController : ApiController
    {

        /// <summary>
        /// Retrieve the account’s time-shifted TV settings (catch-up and C-DVR, Trick-play, Start-over)
        /// </summary>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, TimeShiftedTvPartnerSettingsNotFound = 5022</remarks>   
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaTimeShiftedTvPartnerSettings Get()
        {
            KalturaTimeShiftedTvPartnerSettings response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ApiClient().GetTimeShiftedTvPartnerSettings(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Configure the account’s time-shifted TV settings (catch-up and C-DVR, Trick-play, Start-over)
        /// </summary>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, TimeShiftedTvPartnerSettingsNotSent = 5023, TimeShiftedTvPartnerSettingsNegativeBufferSent = 5024</remarks>  
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(KalturaTimeShiftedTvPartnerSettings settings)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // validate paddingBeforeProgramStarts
                if (settings.PaddingBeforeProgramStarts.HasValue && settings.PaddingBeforeProgramStarts.Value < 0)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "paddingBeforeProgramStarts can not be negative");
                }

                // validate paddingAfterProgramEnds
                if (settings.PaddingAfterProgramEnds.HasValue && settings.PaddingAfterProgramEnds.Value < 0)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "paddingAfterProgramEnds can not be negative");
                }

                // validate protectionPeriod
                if (settings.ProtectionPeriod.HasValue && settings.ProtectionPeriod.Value <= 0)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "protectionPeriod must be above 0");
                }

                // validate recordingLifetimePeriod
                if (settings.RecordingLifetimePeriod.HasValue && settings.RecordingLifetimePeriod.Value <= 0)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "recordingLifetimePeriod must be above 0");
                }

                // validate cleanupNoticePeriod
                if (settings.CleanupNoticePeroid.HasValue && settings.CleanupNoticePeroid.Value <= 0)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "cleanupNoticePeroid must be above 0");
                }

                // validate protectionQuotaPercentage
                if (settings.ProtectionQuotaPercentage.HasValue && (settings.ProtectionQuotaPercentage.Value < 10 || settings.ProtectionQuotaPercentage.Value > 100))
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "protectionQuotaPercentage must be between 10 and 100");
                }

                // call client
                response = ClientsManager.ApiClient().UpdateTimeShiftedTvPartnerSettings(groupId, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

    }
}