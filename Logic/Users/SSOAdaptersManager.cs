using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.Billing;
using ApiObjects.Response;
using ApiObjects.SSOAdapter;
using KLogMonitor;
using System.Reflection;

namespace APILogic.Users
{
    public static class SSOAdaptersManager
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string SSO_ADAPTER_NOT_EXIST = "SSO Adapter does not exist.";
        private const string SSO_ADAPTER_ID_REQUIRED = "SSO Adapater Id is required.";
        private const string NO_SSO_ADAPATER_TO_INSERT = "No SSO Adapater provided to insert.";
        private const string NAME_REQUIRED = "SSO Adapater Name is required";
        private const string SHARED_SECRET_REQUIRED = "SSO Adapater Shared Secret is required";
        private const string EXTERNAL_IDENTIFIER_REQUIRED = "SSO Adapater External Identifier is required";
        private const string EXTERNAL_IDENTIFIER_MUST_BE_UNIQUE = "SSO Adapater External Identifier already exist.";

        public static SSOAdaptersResponse GetSSOAdapters(int groupId)
        {
            var response = new SSOAdaptersResponse();
            try
            {
                response.SSOAdapters = DAL.UsersDal.GetSSOAdapters(groupId);
                if (response.SSOAdapters == null || !response.SSOAdapters.Any())
                {
                    response.RespStatus = new Status((int)eResponseStatus.OK, "no sso adapters related to group");
                }
                else
                {
                    response.RespStatus = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                _Logger.ErrorFormat("Failed groupID={0}, ex:{1}", groupId, ex);
            }

            return response;
        }

        public static SSOAdapterResponse InsertSSOAdapter(SSOAdapter adapterDetails, int updaterId)
        {
            var response = new SSOAdapterResponse { RespStatus = new Status((int)eResponseStatus.OK) };
            try
            {
                response.RespStatus = ValidateSSOAdapterModel(adapterDetails);
                _Logger.DebugFormat("Validation Response is code:[{0}] msg:[{1}]", response.RespStatus.Code, response.RespStatus.Message);
                if (response.RespStatus.Code != (int)eResponseStatus.OK) { return response; }

                response.SSOAdapter = DAL.UsersDal.AddSSOAdapters(adapterDetails, updaterId);

            }
            catch (Exception ex)
            {
                _Logger.ErrorFormat("Failed InsertSSOAdapter groupID={0}, ex:{1}", adapterDetails.GroupId, ex);
            }
            return response;


        }

        public static SSOAdapterResponse UpdateSSOAdapter(SSOAdapter adapterDetails, int updaterId)
        {
            var response = new SSOAdapterResponse();
            try
            {
                response.RespStatus = ValidateSSOAdapterModel(adapterDetails);
                _Logger.DebugFormat("Validation Response is code:[{0}] msg:[{1}]", response.RespStatus.Code, response.RespStatus.Message);

                // Only for update we need to check an id is provided
                if (adapterDetails.Id == 0) { response.RespStatus = new Status((int)eResponseStatus.SSOAdapterIdRequired, SSO_ADAPTER_ID_REQUIRED); }
                if (response.RespStatus.Code != (int)eResponseStatus.OK) { return response; }
                _Logger.DebugFormat("Validation Response is code:[{0}] msg:[{1}]", response.RespStatus.Code, response.RespStatus.Message);



                response.SSOAdapter = DAL.UsersDal.UpdateSSOAdapter(adapterDetails, updaterId);
                if (response.SSOAdapter == null) { response.RespStatus = new Status((int)eResponseStatus.SSOAdapterNotExist, SSO_ADAPTER_NOT_EXIST); }
            }
            catch (Exception ex)
            {
                _Logger.ErrorFormat("Failed UpdateSSOAdapter groupID={0}, ex:{1}", response.RespStatus.Message, ex);
            }
            return response;

        }

        public static Status DeleteSSOAdapter(int groupId, int ssoAdapterId, int updaterId)
        {
            var response = new Status((int)eResponseStatus.OK, "Could not delete SSO adapter profile.");
            try
            {
                if (ssoAdapterId == 0)
                {
                    response = new Status((int)eResponseStatus.SSOAdapterIdRequired, SSO_ADAPTER_ID_REQUIRED);
                    return response;
                }

                var isDeleted = DAL.UsersDal.DeleteSSOAdapter(ssoAdapterId, updaterId);
                response = isDeleted ? new Status((int)eResponseStatus.OK) : new Status((int)eResponseStatus.SSOAdapterNotExist, SSO_ADAPTER_NOT_EXIST);
            }
            catch (Exception ex)
            {
                _Logger.ErrorFormat("Failed DeleteSSOAdapter groupID={0}, adapterId={1}, ex:{2}", groupId, ssoAdapterId, ex);
            }
            return response;

        }

        public static SSOAdapterResponse SetSSOAdapterSharedSecret(int ssoAdapterId, string sharedSecret, int updaterId)
        {
            var response = new SSOAdapterResponse { RespStatus = new Status((int)eResponseStatus.Error, "Could not generate shared secret.") };
            try
            {
                if (ssoAdapterId == 0)
                {
                    response.RespStatus = new Status((int)eResponseStatus.SSOAdapterIdRequired, SSO_ADAPTER_ID_REQUIRED);
                    return response;
                }

                response.SSOAdapter = DAL.UsersDal.SetSharedSecret(ssoAdapterId, sharedSecret, updaterId);
                response.RespStatus = response.SSOAdapter != null ? new Status((int)eResponseStatus.OK) : new Status((int)eResponseStatus.SSOAdapterNotExist, SSO_ADAPTER_NOT_EXIST);
            }
            catch (Exception ex)
            {
                _Logger.ErrorFormat("Failed SetSSOAdapterSharedSecret adapterId={0} ex:{1}", ssoAdapterId, ex);
            }
            return response;

        }

        private static Status ValidateSSOAdapterModel(SSOAdapter adapterDetails)
        {

            if (adapterDetails == null) { return new Status((int)eResponseStatus.NoSSOAdapaterToInsert, NO_SSO_ADAPATER_TO_INSERT); }
            if (string.IsNullOrEmpty(adapterDetails.Name)) { return new Status((int)eResponseStatus.NameRequired, NAME_REQUIRED); }
            if (string.IsNullOrEmpty(adapterDetails.SharedSecret)) { return new Status((int)eResponseStatus.SharedSecretRequired, SHARED_SECRET_REQUIRED); }
            if (string.IsNullOrEmpty(adapterDetails.ExternalIdentifier)) { return new Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED); }


            // Check ExternalIdentifierMustBeUnique
            var ssoAdapaterByExternalId = DAL.UsersDal.GetSSOAdapterByExternalId(adapterDetails.ExternalIdentifier);
            _Logger.DebugFormat("Checking if sso adapter external id already exist externalId:[{0}], found sso adapater id:[{1}], externalId:[{2}]", adapterDetails.ExternalIdentifier, ssoAdapaterByExternalId != null ? ssoAdapaterByExternalId.Id : null, ssoAdapaterByExternalId != null ? ssoAdapaterByExternalId.ExternalIdentifier : null);

            if (ssoAdapaterByExternalId != null && ssoAdapaterByExternalId.Id != adapterDetails.Id) { return new Status((int)eResponseStatus.ExternalIdentifierMustBeUnique, EXTERNAL_IDENTIFIER_MUST_BE_UNIQUE); }

            return new Status((int)eResponseStatus.OK);
        }
    }
}
