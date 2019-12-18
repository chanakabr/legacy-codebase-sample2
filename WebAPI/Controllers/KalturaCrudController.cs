using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    /// <summary>
    /// abstract class which represents a controller with CRUD actions
    /// </summary>
    /// <typeparam name="KalturaT">kaltura object</typeparam>
    /// <typeparam name="ICrudHandeledObject">core object</typeparam>
    /// <typeparam name="IdentifierT">Identifier type</typeparam>
    /// <typeparam name="ICrudFilter">core filter</typeparam>
    public abstract class KalturaCrudController<KalturaT, KalturaListResponseT, ICrudHandeledObject, IdentifierT, KalturaFilterT, ICrudFilter> : IKalturaController
        where KalturaT : KalturaCrudObject<ICrudHandeledObject, IdentifierT, ICrudFilter>, new()
        where KalturaListResponseT : KalturaListResponse<KalturaT>, new()
        where IdentifierT : IConvertible
        where KalturaFilterT : KalturaOTTObject, IKalturaCrudFilter<ICrudHandeledObject, IdentifierT, ICrudFilter>, new()
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected static KalturaT DefaultObject { get; set; }
        
        static KalturaCrudController()
        {
            DefaultObject = new KalturaT();
        }
        
        [Action(AddActionAttribute.Name)]
        [ApiAuthorize]
        public static KalturaT Add(KalturaT objectToAdd)
        {
            KalturaT response = null;

            try
            {
                objectToAdd.ValidateForAdd();
                var contextData = KS.GetContextData();
                GenericResponse<ICrudHandeledObject> addFunc(ICrudHandeledObject coreObjectToAdd) => 
                    objectToAdd.Handler.Add(contextData, coreObjectToAdd);
                response = GetResponseFromCore(objectToAdd, addFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        [Action(UpdateActionAttribute.Name)]
        [ApiAuthorize]
        public static KalturaT Update(IdentifierT id, KalturaT objectToUpdate)
        {
            KalturaT response = null;

            try
            {
                objectToUpdate.SetId(id);
                objectToUpdate.ValidateForUpdate();
                var contextData = KS.GetContextData();
                GenericResponse<ICrudHandeledObject> updateFunc(ICrudHandeledObject coreObjectToUpdate) =>
                     objectToUpdate.Handler.Update(contextData, coreObjectToUpdate);
                response = GetResponseFromCore(objectToUpdate, updateFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
           
            return response;
        }

        [Action(DeleteActionAttribute.Name)]
        [ApiAuthorize]
        public static void Delete(IdentifierT id)
        {
            try
            {
                if (DefaultObject == null)
                {
                    throw new ClientException((int)StatusCode.NotImplemented, string.Format("Default Object was not defined for service:{0}", MethodBase.GetCurrentMethod().DeclaringType));
                }

                if (DefaultObject.Handler == null)
                {
                    throw new ClientException((int)StatusCode.NotImplemented, string.Format("Default Handler was not defined for object:{0}", DefaultObject.GetType().Name));
                }

                var contextData = KS.GetContextData();
                GetResponseStatusFromCore(() => DefaultObject.Handler.Delete(contextData, id));
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        [Action(GetActionAttribute.Name)]
        [ApiAuthorize]
        public static KalturaT Get(IdentifierT id)
        {
            KalturaT response = null;

            try
            {
                if (DefaultObject == null)
                {
                    throw new ClientException((int)StatusCode.NotImplemented, string.Format("Default Object was not defined for service:{0}", MethodBase.GetCurrentMethod().DeclaringType));
                }

                if (DefaultObject.Handler == null)
                {
                    throw new ClientException((int)StatusCode.NotImplemented, string.Format("Default Handler was not defined for object:{0}", DefaultObject.GetType().Name));
                }

                var contextData = KS.GetContextData();
                response = GetResponseFromCore(() => DefaultObject.Handler.Get(contextData, id));
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        
        [Action(ListActionAttribute.Name)]
        [ApiAuthorize]
        public static KalturaListResponseT List(KalturaFilterT filter)
        {
            KalturaListResponseT response = null;
            
            try
            {
                if (filter == null)
                {
                    filter = new KalturaFilterT();
                }
                else
                {
                    filter.Validate();
                }
                
                var contextData = KS.GetContextData();
                response = GetResponseListFromCore(filter, contextData);

                var responseProfile = Utils.Utils.GetResponseProfileFromRequest();
                if (response.Objects.Count > 0 && responseProfile != null && filter.RelatedObjectFilterType != null)
                {
                    KalturaDetachedResponseProfile profile = null;
                   
                    if (responseProfile is KalturaDetachedResponseProfile detachedResponseProfile)
                    {
                        profile = detachedResponseProfile.RelatedProfiles.FirstOrDefault(x => x.Filter.GetType() == filter.RelatedObjectFilterType);
                    }
                    
                    if (profile != null && !string.IsNullOrEmpty(profile.Name))
                    {
                        response.SetRelatedObjects(contextData, profile);
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        private static KalturaT GetResponseFromCore(KalturaT kalturaObjectfromRequest, Func<ICrudHandeledObject, GenericResponse<ICrudHandeledObject>> funcInCore)
        {
            GenericResponse<ICrudHandeledObject> response = null;

            try
            {
                var mappedCoreObject = AutoMapper.Mapper.Map<ICrudHandeledObject>(kalturaObjectfromRequest);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = funcInCore(mappedCoreObject);
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception received while calling client service.", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message, response.Status.Args);
            }

            KalturaT result = null;
            if (response.Object != null)
            {
                result = AutoMapper.Mapper.Map<KalturaT>(response.Object);
            }

            return result;
        }

        private static void GetResponseStatusFromCore(Func<Status> funcInCore)
        {
            Status status = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    status = funcInCore();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception received while calling catalog service.", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(status.Code, status.Message, status.Args);
            }
        }

        private static KalturaT GetResponseFromCore(Func<GenericResponse<ICrudHandeledObject>> funcInCore)
        {
            GenericResponse<ICrudHandeledObject> response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = funcInCore();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception received while calling catalog service.", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message, response.Status.Args);
            }

            KalturaT result = null;
            if (response.Object != null)
            {
                result = AutoMapper.Mapper.Map<KalturaT>(response.Object);
            }

            return result;
        }

        private static KalturaListResponseT GetResponseListFromCore(KalturaFilterT filter, ContextData contextData)
        {
            GenericListResponse<ICrudHandeledObject> response = null;

            try
            {
                var coreFilter = AutoMapper.Mapper.Map<ICrudFilter>(filter);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = filter.Handler.List(contextData, coreFilter);
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception received while calling CRUD handler action list.", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status.Code, response.Status.Message, response.Status.Args);
            }

            var result = new KalturaListResponseT();
            if (response.Objects != null)
            {
                result.Objects = AutoMapper.Mapper.Map<List<KalturaT>>(response.Objects);
                result.TotalCount = response.TotalItems != 0 ? response.TotalItems : response.Objects.Count;
                // TODO - order BY GetResponseListFromWS
            }
            else
            {
                result.Objects = new List<KalturaT>();
            }

            return result;
        }
    }
}