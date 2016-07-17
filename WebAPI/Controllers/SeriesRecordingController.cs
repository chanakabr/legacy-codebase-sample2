using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/seriesRecording/action")]    
    public class SeriesRecordingController : ApiController
    {
       
        /// <summary>        
        /// Cancel a previously requested series recording. Cancel series recording can be called for recording in status Scheduled or Recording Only 
        /// </summary>
        /// <param name="id">Series Recording identifier</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003,UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001,
        /// UserWithNoDomain = 2024, RecordingNotFound = 3039,RecordingStatusNotValid = 3043, SeriesRecordingNotFound= 3046 </remarks>
        [Route("cancel"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public KalturaSeriesRecording Cancel(long id)
        {
            KalturaSeriesRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                // call client                
                response = ClientsManager.ConditionalAccessClient().CancelSeriesRecord(groupId, userId, domainId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

           /// <summary>        
           /// Delete series recording(s). Delete series recording can be called recordings in any status
           /// </summary>
           /// <param name="id">Series Recording identifier</param>
           /// <returns></returns>
           /// <remarks>Possible status codes: BadRequest = 500003,UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001,
           /// UserWithNoDomain = 2024, RecordingNotFound = 3039,RecordingStatusNotValid = 3043, SeriesRecordingNotFound= 3046 </remarks>
           [Route("delete"), HttpPost]
           [ApiAuthorize]
           public KalturaSeriesRecording Delete(long id)
           {
               KalturaSeriesRecording response = null;

               try
               {
                   int groupId = KS.GetFromRequest().GroupId;
                   string userId = KS.GetFromRequest().UserId;
                   long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                   // call client                
                   response = ClientsManager.ConditionalAccessClient().DeleteSeriesRecord(groupId, userId, domainId, id);
               }
               catch (ClientException ex)
               {
                   ErrorUtils.HandleClientException(ex);
               }
               return response;
           }
    }
}