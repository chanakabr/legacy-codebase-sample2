using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebAPI.Utils;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Clients
{
    public static class ClientUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static U GetResponseFromWS<U, T>(U requestObject, Func<T, GenericResponse<T>> funcInWS)
            where U : KalturaOTTObject where T : class
        {
            U result = null;
            GenericResponse<T> response = null;

            try
            {
                T dataToCatalogManager = AutoMapper.Mapper.Map<T>(requestObject);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = funcInWS(dataToCatalogManager);
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

            if (response.Object != null)
            {
                result = AutoMapper.Mapper.Map<U>(response.Object);
            }

            return result;
        }

        internal static U GetResponseFromWS<U, T>(Func<GenericResponse<T>> funcInWS)
            where U : KalturaOTTObject where T : class
        {
            U result = null;
            GenericResponse<T> response = null;

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

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Object != null)
            {
                result = AutoMapper.Mapper.Map<U>(response.Object);
            }

            return result;
        }

        /// <summary>
        /// Get result list from CatalogManager (without ordering the list)
        /// </summary>
        /// <typeparam name="U">The Kaltura object from\for the client</typeparam>
        /// <typeparam name="T">The internal representation of the Kaltura object</typeparam>
        /// <param name="funcInWS"></param>
        /// <returns></returns>
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

        internal static bool GetResponseStatusFromWS(Func<Status> funcInWS)
        {
            Status status = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    status = funcInWS();
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

            return true;
        }
        
        internal static void GetResponseStatusFromWS<U, T>(Func<T, Status> funcInWS, U kalturaOTTObject)
            where U : KalturaOTTObject where T : class
        {
            Status status = null;

            try
            {
                T dataToFunc = AutoMapper.Mapper.Map<T>(kalturaOTTObject);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    status = funcInWS(dataToFunc);
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
    }
}