using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("cDVRAdapterProfile")]
    public class CDVRAdapterProfileController : IKalturaController
    {
        /// <summary>
        /// Returns all C-DVR adapters for partner
        /// </summary>
        /// <remarks> 
        /// 
        /// </remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaCDVRAdapterProfileListResponse List()
        {
            List<KalturaCDVRAdapterProfile> list = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                list = ClientsManager.ConditionalAccessClient().GetCDVRAdapters(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaCDVRAdapterProfileListResponse() { Objects = list, TotalCount = list.Count };
        }

        /// <summary>
        /// Returns all C-DVR adapters for partner
        /// </summary>
        /// <remarks> 
        /// 
        /// </remarks>
        [Action("listOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("list")]
        [Obsolete]
        static public List<KalturaCDVRAdapterProfile> ListOldStandard()
        {
            List<KalturaCDVRAdapterProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GetCDVRAdapters(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete C-DVR adapter by C-DVR adapter id
        /// </summary>
        /// <remarks>
        /// Possible status codes: Adapter does not exist = 10000
        /// </remarks>
        /// <param name="adapterId">C-DVR adapter identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [OldStandardArgument("adapterId", "adapter_id")]
        [Throws(eResponseStatus.AdapterNotExists)]
        static public bool Delete(int adapterId)
        {
            bool response = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().DeleteCDVRAdapter(groupId, adapterId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new C-DVR adapter for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// Adapter Name is required = 5005, Adapter URL is required = 5013, External identifier is required = 6016, External identifier must be unique = 6040
        /// </remarks>
        /// <param name="adapter">C-DVR adapter object</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.ExternalIdentifierMustBeUnique)]
        static public KalturaCDVRAdapterProfile Add(KalturaCDVRAdapterProfile adapter)
        {
            KalturaCDVRAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().InsertCDVRAdapter(groupId, adapter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update C-DVR adapter details
        /// </summary>
        /// <remarks>
        /// Possible status codes:   
        /// Adapter name is required = 5005, Adapter identifier is required = 10001, Adapter URL is required = 5013, External identifier required = 6016, Adapter does not exist = 10000
        /// </remarks>
        /// <param name="adapterId">C-DVR adapter identifier</param>       
        /// <param name="adapter">C-DVR adapter Object</param>       
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.AdapterIdentifierRequired)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.AdapterNotExists)]
        static public KalturaCDVRAdapterProfile Update(int adapterId, KalturaCDVRAdapterProfile adapter)
        {
            KalturaCDVRAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;
            adapter.Id = adapterId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().SetCDVRAdapter(groupId, adapter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update C-DVR adapter details
        /// </summary>
        /// <remarks>
        /// Possible status codes:   
        /// Adapter name is required = 5005, Adapter identifier is required = 10001, Adapter URL is required = 5013, External identifier required = 6016, Adapter does not exist = 10000
        /// </remarks>
        /// <param name="adapter">C-DVR adapter Object</param>       
        [Action("updateOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("update")]
        [Obsolete]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.AdapterIdentifierRequired)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.ExternalIdentifierRequired)]
        [Throws(eResponseStatus.AdapterNotExists)]
        static public KalturaCDVRAdapterProfile UpdateOldStandard(KalturaCDVRAdapterProfile adapter)
        {
            KalturaCDVRAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().SetCDVRAdapter(groupId, adapter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Generate C-DVR adapter shared secret
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// Adapter identifier is required = 10001, AdapterNotExists = 10000
        /// </remarks>
        /// <param name="adapterId">C-DVR adapter identifier</param>
        [Action("generateSharedSecret")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [OldStandardArgument("adapterId", "adapter_id")]
        [Throws(eResponseStatus.AdapterIdentifierRequired)]
        [Throws(eResponseStatus.AdapterNotExists)]
        static public KalturaCDVRAdapterProfile GenerateSharedSecret(int adapterId)
        {
            KalturaCDVRAdapterProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GenerateCDVRSharedSecret(groupId, adapterId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}