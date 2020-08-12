using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.EventBus;
using ApiObjects.Response;
using EventBus.RabbitMQ;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using ApiObjects.Rules;
using domain = Core.Domains.Module;
using notification = Core.Notification.Module;
using System.Threading.Tasks;
using System.Linq;

namespace ApiLogic.Users.Managers
{
    public class CampaignManager : ICrudHandler<Campaign, long>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<CampaignManager> lazy = new Lazy<CampaignManager>(() => new CampaignManager());
        public static CampaignManager Instance { get { return lazy.Value; } }

        private CampaignManager() { }

        public Status Delete(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<Campaign> Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        GenericResponse<Campaign> ICrudHandler<Campaign, long>.Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<Campaign> List(ContextData contextData, object filter)
        {
            return null;
        }

        private static GenericResponse<Campaign> UpdateCampaignStatusWithVersionCheck(Campaign objectToUpdate, CampaignEventStatus newStatus)
        {
            var response = new GenericResponse<Campaign>();
            try
            {
                var originalStatus = objectToUpdate.Status;
                response.Object = objectToUpdate;

                CampaignEventStatus updatedStatus;

                //if (!CatalogDAL.SaveBulkUploadStatusAndErrorsCB(response.Object, BULK_UPLOAD_CB_TTL, out updatedStatus))
                //{
                //    log.ErrorFormat("UpdateBulkUploadStatusWithVersionCheck > Error while saving BulkUpload to CB. bulkUploadId:{0}, status:{1}.", response.Object.Id, newStatus);
                //}
                //log.Debug($"UpdateBulkUploadStatusWithVersionCheck > status by results is:[{updatedStatus}], status to set:[{newStatus}]");
                response.Object.Status = newStatus;

                //UpdateBulkUploadInSqlAndInvalidateKeys(response.Object, originalStatus);


                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in UpdateCampaignStatusWithVersionCheck. Id:{objectToUpdate.Id}, status:{objectToUpdate.Status}, ex: {ex}", ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        // TODO MATAN
        /// <summary>
        /// Validate if user matches to CampaignConditions
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="triggerCampaign"></param>
        /// <param name="coreObject"></param>
        /// <returns></returns>
        public bool ValidateCampaignConditionsToUser(int userId, TriggerCampaign triggerCampaign, CoreObject coreObject)
        {
            return false;
        }

        // TODO MATAN
        /// <summary>
        /// Validate coreobject matches to TriggerConditions
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="triggerCampaign"></param>
        /// <param name="coreObject"></param>
        /// <returns></returns>
        public bool ValidateTriggerCampaign(TriggerCampaign triggerCampaign, CoreObject coreObject)
        {
            return false;
        }
    }

    public abstract class Campaign : ICrudHandeledObject
    {
        public long Id { get; set; }
        public CampaignEventStatus Status { get; set; }

        public Campaign()
        {
        }

        [JsonProperty(PropertyName = "Conditions",
                      TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<RuleCondition> CampaignConditions { get; set; }
    }

    public class TriggerCampaign : Campaign
    {
        [JsonProperty(PropertyName = "Conditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<RuleCondition> TriggerConditions { get; set; }
    }

    public enum CampaignEventStatus
    {
        Queued, Failed, InProgress
    }
}
