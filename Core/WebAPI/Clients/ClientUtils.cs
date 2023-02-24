using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Utils;

[assembly: InternalsVisibleTo("WebAPI.Tests")]

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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            if (response.Objects != null)
            {
                result.Objects = AutoMapper.Mapper.Map<List<U>>(response.Objects);
                result.TotalCount = response.TotalItems != 0 ? response.TotalItems : response.Objects.Count;
            }
            else
            {
                result.Objects = new List<U>();
            }

            return result;
        }

        internal static KalturaGenericListResponse<U> ListFromLogic<U, T>(Func<IEnumerable<T>> funcInWS)
            where U : KalturaOTTObject where T : class
        {
            KalturaGenericListResponse<U> result = new KalturaGenericListResponse<U>();
            IEnumerable<T> response = null;

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
                throw new ClientException(StatusCode.Error);
            }

            result.Objects = AutoMapper.Mapper.Map<List<U>>(response);
            result.TotalCount = result.Objects.Count;

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
                throw new ClientException(StatusCode.Error);
            }

            if (status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(status);
            }
        }

        internal static T GetResponseFromWs<T>(Func<T> funcInWs) where T : class
        {
            T response = null;
            try
            {
                using (new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = funcInWs();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception received while calling client service.", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            return response;
        }

        internal static bool GetBoolResponseFromWS(Func<bool> funcInWS)
        {
            bool result = false;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    result = funcInWS();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception received while calling service.", ex);
                ErrorUtils.HandleWSException(ex);
            }

            return result;
        }

        internal static string GetStringResponseFromWS(Func<string> funcInWS)
        {
            string result = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    result = funcInWS();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception received while calling service.", ex);
                ErrorUtils.HandleWSException(ex);
            }

            return result;
        }

        internal static long GetLongResponseFromWS(Func<long> funcInWS)
        {
            long result = 0;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    result = funcInWS();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception received while calling service.", ex);
                ErrorUtils.HandleWSException(ex);
            }

            return result;
        }

        internal static List<int> GetListIntResponseFromWS(Func<List<int>> funcInWS)
        {
            List<int> result = new List<int>();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    result = funcInWS();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception received while calling service.", ex);
                ErrorUtils.HandleWSException(ex);
            }

            return result;
        }

        internal static U GetResponseFromWS<U, T>(Func<T> funcInWS)
            where U : KalturaOTTObject where T : class
        {
            U result = null;
            T response = null;

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
                throw new ClientException(StatusCode.Error);
            }

            result = AutoMapper.Mapper.Map<U>(response);

            return result;
        }

        internal static GenericListResponse<T> GetGenericListResponseFromWS<T>(Func<GenericListResponse<T>> funcInWS) where T : class
        {
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
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            return response;
        }

        internal static T Get<T>(this GenericResponse<T> response)
        {
            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            return response.Object;
        }

        internal static (List<T> items, int totalItems) GetList<T>(this GenericListResponse<T> response)
        {
            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            return (response.Objects, response.TotalItems);
        }
    }
}