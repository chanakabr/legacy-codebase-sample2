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
    // TODO SHIR - SET crud method in KalturaCrudController as real methods AND DELETE crud METHODS FROM inheritors
    // until then KalturaCrudController crud methods will be DoAdd, DoDelete etc and will be update to Add, Delete 
    // when i will finish it. 
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

        // TODO SHIR - ADD ALL RELEVANT ITEMS TO typesToHandlersMap - by reflection when phoenix service starts
        private static Dictionary<Type, ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter>> typesToHandlersMap = 
            new Dictionary<Type, ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter>>();

        protected static KalturaT DoAdd(KalturaT kalturaObjectToAdd)
        {
            KalturaT response = null;
            typesToHandlersMap.Add(typeof(KalturaT), kalturaObjectToAdd.Handler);

            try
            {
                kalturaObjectToAdd.ValidateForAdd();
                var contextData = KS.GetContextData();
                Func<ICrudHandeledObject, GenericResponse<ICrudHandeledObject>> addFunc = 
                    (ICrudHandeledObject objectToAdd) => kalturaObjectToAdd.Handler.Add(contextData, objectToAdd);
                response = GetResponseFromCore(kalturaObjectToAdd, addFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        protected static KalturaT DoUpdate(IdentifierT id, KalturaT kalturaObjectToUpdate)
        {
            KalturaT response = null;

            try
            {
                kalturaObjectToUpdate.SetId(id);
                kalturaObjectToUpdate.ValidateForUpdate();
                var contextData = KS.GetContextData();
                Func<ICrudHandeledObject, GenericResponse<ICrudHandeledObject>> updateFunc = 
                    (ICrudHandeledObject objectToUpdate) => kalturaObjectToUpdate.Handler.Update(contextData, objectToUpdate);
                response = GetResponseFromCore(kalturaObjectToUpdate, updateFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
           
            return response;
        }

        protected static void DoDelete(IdentifierT id, ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter> handler)
        {
            try
            {
                var contextData = KS.GetContextData();
                GetResponseStatusFromCore(() => typesToHandlersMap[typeof(KalturaT)].Delete(contextData, id));
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        protected static KalturaT DoGet(IdentifierT id, ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter> handler)
        {
            KalturaT response = null;

            try
            {
                var contextData = KS.GetContextData();
                response = GetResponseFromCore(() => handler.Get(contextData, id));
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        //public static KalturaListResponseT DoList<KalturaFilterT, KalturaOrderByT>(KalturaFilterT kalturaFilter)
        //    where KalturaListResponseT :  KalturaListResponse<KalturaT>
        //    where KalturaFilterT: KalturaCrudFilter<KalturaOrderByT, ICrudHandeledObject, IdentifierT, ICrudFilter>
        //    where KalturaOrderByT : struct, IComparable, IFormattable, IConvertible
        //{
        //    var response = new KalturaListResponse<KalturaT>();
        //    var groupId = KS.GetFromRequest().GroupId;
        //    var householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

        //    if (filter == null)
        //        filter = new KalturaHouseholdCouponFilter();

        //    try
        //    {
        //        // TODO SHIR - TALK WITH TANTAN about all list objects so id FINISH GENERIC LIST METHOD in ICrudHandler - put in controller
        //        KalturaGenericListResponse<KalturaHouseholdCoupon> coreResponse = filter.Execute<KalturaHouseholdCoupon>();
        //        response.Objects = coreResponse.Objects;
        //        response.TotalCount = coreResponse.TotalCount;
        //    }
        //    catch (ClientException ex)
        //    {
        //        ErrorUtils.HandleClientException(ex);
        //    }

        //    return response;

        //    //--------------------
        //    GenericListResponse<ICrudHandeledObject> response = null;
            
        //    try
        //    {
        //        var coreFilter = AutoMapper.Mapper.Map<ICrudFilter>(kalturaFilter);
        //        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
        //        {
        //            response = kalturaFilter.Handler.List(coreFilter);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("Exception received while calling CRUD handler.list.", ex);
        //        ErrorUtils.HandleWSException(ex);
        //    }

        //    if (response == null)
        //    {
        //        throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
        //    }

        //    if (!response.IsOkStatusCode())
        //    {
        //        throw new ClientException(response.Status.Code, response.Status.Message, response.Status.Args);
        //    }

        //    var result = new KalturaGenericListResponse<KalturaT>();
        //    if (response.Objects != null)
        //    {
        //        result.Objects = AutoMapper.Mapper.Map<List<KalturaT>>(response.Objects);
        //        result.TotalCount = response.TotalItems != 0 ? response.TotalItems : response.Objects.Count;
        //        // TODO SHIR - order BY GetResponseListFromWS
        //    }
        //    else
        //    {
        //        result.Objects = new List<KalturaT>();
        //    }

        //    return result;
        //}

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

        //private static KalturaGenericListResponse<KalturaT> GetResponseListFromCore<KalturaT>(KalturaFilterT filter)
        //    where KalturaFilterT : KalturaCrudFilter<KalturaOrderByT, ICrudHandeledObject, IdentifierT, ICrudFilter>
        //    where KalturaT : KalturaCrudObject<ICrudHandeledObject, IdentifierT, ICrudFilter>
        //{
        //    GenericListResponse<ICrudHandeledObject> response = null;

        //    try
        //    {
        //        var coreFilter = AutoMapper.Mapper.Map<ICrudFilter>(this);
        //        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
        //        {
        //            response = this.Handler.List(coreFilter);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("Exception received while calling CRUD handler.list.", ex);
        //        ErrorUtils.HandleWSException(ex);
        //    }

        //    if (response == null)
        //    {
        //        throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
        //    }

        //    if (!response.IsOkStatusCode())
        //    {
        //        throw new ClientException(response.Status.Code, response.Status.Message, response.Status.Args);
        //    }

        //    var result = new KalturaGenericListResponse<KalturaT>();
        //    if (response.Objects != null)
        //    {
        //        result.Objects = AutoMapper.Mapper.Map<List<KalturaT>>(response.Objects);
        //        result.TotalCount = response.TotalItems != 0 ? response.TotalItems : response.Objects.Count;
        //        // TODO SHIR - order BY GetResponseListFromWS
        //    }
        //    else
        //    {
        //        result.Objects = new List<KalturaT>();
        //    }

        //    return result;
        //}
    }
}
