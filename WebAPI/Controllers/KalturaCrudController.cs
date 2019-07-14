using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Routing;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Reflection;
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
    public abstract class KalturaCrudController<KalturaT, CoreT, IdentifierT> : IKalturaController
        where KalturaT : KalturaCrudObject<CoreT, IdentifierT>
        where CoreT : class, ICrudHandeledObject
        where IdentifierT : IConvertible
    {
        protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static KalturaT Add(int groupId, KalturaT kalturaObjectToAdd, Dictionary<string, object> extraParams = null)
        {
            KalturaT response = null;
            
            try
            {
                kalturaObjectToAdd.ValidateForAdd();
                Func<CoreT, GenericResponse<CoreT>> addFunc = (CoreT objectToAdd) => kalturaObjectToAdd.Handler.Add(groupId, objectToAdd, extraParams);
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
                Func<CoreT, GenericResponse<CoreT>> updateFunc = (CoreT objectToUpdate) => kalturaObjectToUpdate.Handler.Update(groupId, objectToUpdate, extraParams);
                response = GetResponseFromCore(kalturaObjectToUpdate, updateFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
           
            return response;
        }

        internal static void Delete(int groupId, IdentifierT id, ICrudHandler<CoreT, IdentifierT> handler, Dictionary<string, object> extraParams = null)// BaseCrudHandler<CoreT> handler)
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
        
        internal static KalturaT Get(int groupId, IdentifierT id, ICrudHandler<CoreT, IdentifierT> handler, Dictionary<string, object> extraParams = null)
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

        private static KalturaT GetResponseFromCore(KalturaT kalturaObjectfromRequest, Func<CoreT, GenericResponse<CoreT>> funcInCore)
        {
            GenericResponse<CoreT> response = null;

            try
            {
                var mappedCoreObject = AutoMapper.Mapper.Map<CoreT>(kalturaObjectfromRequest);
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

        private static KalturaT GetResponseFromCore(Func<GenericResponse<CoreT>> funcInCore)
        {
            GenericResponse<CoreT> response = null;

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

        internal static KalturaGenericListResponse<U> GetResponseListFromWS<U, T>(Func<GenericListResponse<T>> funcInWS)
            where U : KalturaOTTObject where T : class
        {
            KalturaGenericListResponse<U> result = new KalturaGenericListResponse<U>();
            GenericListResponse<T> response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = funcInWS();
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

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status.Code, response.Status.Message, response.Status.Args);
            }

            if (response.Objects != null)
            {
                result.Objects = AutoMapper.Mapper.Map<List<U>>(response.Objects);
                result.TotalCount = response.TotalItems != 0 ? response.TotalItems : response.Objects.Count;
                // TODO SHIR - order BY GetResponseListFromWS
            }
            else
            {
                result.Objects = new List<U>();
            }

            return result;
        }
    }
}
