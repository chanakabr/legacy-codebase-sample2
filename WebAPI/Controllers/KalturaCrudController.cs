using ApiLogic.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    // TODO SHIR - CRUD changes
    /// <summary>
    /// abstract class which represents a controller with CRUD actions
    /// </summary>
    /// <typeparam name="KalturaT">kaltura object</typeparam>
    /// <typeparam name="CoreT">core object</typeparam>
    /// <typeparam name="IdentifierT">Identifier type</typeparam>
    public abstract class KalturaCrudController<KalturaT, ICrudHandeledObject, IdentifierT, ICrudFilter> : IKalturaController
        where KalturaT : KalturaCrudObject<ICrudHandeledObject, IdentifierT, ICrudFilter>
        where IdentifierT : IConvertible
    {
        protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static KalturaT Add(int groupId, KalturaT kalturaObjectToAdd, Dictionary<string, object> extraParams = null)
        {
            KalturaT response = null;
            
            try
            {
                kalturaObjectToAdd.ValidateForAdd();
                Func<ICrudHandeledObject, GenericResponse<ICrudHandeledObject>> addFunc = (ICrudHandeledObject objectToAdd) => kalturaObjectToAdd.Handler.Add(groupId, objectToAdd, extraParams);
                response = GetResponseFromCore(kalturaObjectToAdd, addFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        internal static KalturaT Update(int groupId, KalturaT kalturaObjectToUpdate, Dictionary<string, object> extraParams = null)
        {
            KalturaT response = null;

            try
            {
                kalturaObjectToUpdate.ValidateForUpdate();
                Func<ICrudHandeledObject, GenericResponse<ICrudHandeledObject>> updateFunc = (ICrudHandeledObject objectToUpdate) => kalturaObjectToUpdate.Handler.Update(groupId, objectToUpdate, extraParams);
                response = GetResponseFromCore(kalturaObjectToUpdate, updateFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
           
            return response;
        }

        internal static void Delete(int groupId, IdentifierT id, ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter> handler, Dictionary<string, object> extraParams = null)// BaseCrudHandler<CoreT> handler)
        {
            try
            {
                GetResponseStatusFromCore(() => handler.Delete(groupId, id, extraParams));
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
        
        internal static KalturaT Get(int groupId, IdentifierT id, ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter> handler, Dictionary<string, object> extraParams = null)
        {
            KalturaT response = null;

            try
            {
                response = GetResponseFromCore(() => handler.Get(groupId, id, extraParams));
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        // TODO SHIR - TALK WITH TANTAN ABOUT THIS
        //internal static KalturaGenericListResponse<KalturaT> List<FilterT, OrderByT>(FilterT filter) 
        //    where FilterT : KalturaFilter<OrderByT>
        //    where OrderByT : struct, IComparable, IFormattable, IConvertible
        //{
        //    int groupId = KS.GetFromRequest().GroupId;

        //    if (filter == null)
        //    {
        //        filter = new FilterT();
        //    }

        //    KalturaBusinessModuleRuleListResponse response = null;

        //    try
        //    {
        //        response = ClientsManager.ApiClient().GetBusinessModuleRules(groupId, filter);
        //    }
        //    catch (ClientException ex)
        //    {
        //        ErrorUtils.HandleClientException(ex);
        //    }

        //    return response;
        //}

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
                throw new ClientException(response.Status.Code, response.Status.Message);
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
                throw new ClientException(status.Code, status.Message);
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
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            KalturaT result = null;
            if (response.Object != null)
            {
                result = AutoMapper.Mapper.Map<KalturaT>(response.Object);
            }

            return result;
        }
    }
}
