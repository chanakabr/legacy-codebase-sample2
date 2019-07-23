using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Utils;
using WebAPI.Managers.Models;
using System.Reflection;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Base Crud filter
    /// </summary>
    [Serializable]
    public abstract partial class KalturaCrudFilter<KalturaOrderByT, ICrudHandeledObject, IdentifierT, ICrudFilter> : KalturaFilter<KalturaOrderByT> 
        where KalturaOrderByT : struct, IComparable, IFormattable, IConvertible
        where IdentifierT : IConvertible
    {
        protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal abstract ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter> Handler { get; }
        internal abstract void Validate();
        
        internal KalturaListResponseT Execute<KalturaListResponseT, KalturaT>()
            where KalturaListResponseT : KalturaListResponse<KalturaT>, new()
            where KalturaT : KalturaCrudObject<ICrudHandeledObject, IdentifierT, ICrudFilter>
        {
            KalturaListResponseT response = new KalturaListResponseT();
            
            try
            {
                // Validate(); TODO Shir: i want to remove this
                var contextData = KS.GetContextData();
                // TODO SHIR - TALK WITH TANTAN about all list objects so id FINISH GENERIC LIST METHOD in ICrudHandler - put in controller
                var coreResponse = GetResponseListFromCore<KalturaT>(contextData);
                response.SetData(coreResponse);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        internal KalturaGenericListResponse<KalturaT> GetResponseListFromCore<KalturaT>(ContextData contextData)
            where KalturaT : KalturaCrudObject<ICrudHandeledObject, IdentifierT, ICrudFilter>
        {
            GenericListResponse<ICrudHandeledObject> response = null;

            try
            {
                var coreFilter = AutoMapper.Mapper.Map<ICrudFilter>(this);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = this.Handler.List(contextData, coreFilter);
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception received while calling CRUD handler.list.", ex);
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

            var result = new KalturaGenericListResponse<KalturaT>();
            if (response.Objects != null)
            {
                result.Objects = AutoMapper.Mapper.Map<List<KalturaT>>(response.Objects);
                result.TotalCount = response.TotalItems != 0 ? response.TotalItems : response.Objects.Count;
                // TODO SHIR - order BY GetResponseListFromWS
            }
            else
            {
                result.Objects = new List<KalturaT>();
            }

            return result;
        }
    }
}