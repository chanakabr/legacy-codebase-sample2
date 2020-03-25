using AdapterClients.IngestTransformation;
using ApiObjects;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AdapterClients.IngestTransformation;

namespace Core.Profiles
{
    public class IngestProfileManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string ERROR_EXT_ID_ALREADY_IN_USE = "External identifier must be unique";
        private const string NAME_REQUIRED = "Name must have a value";
        private const string NO_PROFILE_TO_INSERT = "No profile to insert";
        private const string PROFILE_NOT_EXIST = "profile doesn't exist";

        public static GenericResponse<IngestProfile> AddIngestProfile(int groupId, int userId, IngestProfile profileToAdd)
        {
            var response = new GenericResponse<IngestProfile>();
            try
            {
                if (profileToAdd == null)
                {
                    response.SetStatus((int)eResponseStatus.IngestProfileNotExists, NO_PROFILE_TO_INSERT);
                    return response;
                }

                if (string.IsNullOrEmpty(profileToAdd.Name))
                {
                    response.SetStatus((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (IsIngestProfileExternalIdExists(profileToAdd.ExternalId))
                {
                    response.SetStatus((int)eResponseStatus.ExternalIdentifierMustBeUnique, ERROR_EXT_ID_ALREADY_IN_USE);
                    return response;
                }

                // Create Shared secret 
                profileToAdd.TransformationAdapterSharedSecret = profileToAdd.TransformationAdapterSharedSecret ?? Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                int id = ApiDAL.AddIngestProfile(groupId, userId, profileToAdd);
                if (id > 0)
                {
                    profileToAdd.Id = id;

                    response.Object = profileToAdd;
                    response.SetStatus(eResponseStatus.OK, "New ingest profile was successfully created");

                    if (!string.IsNullOrEmpty(profileToAdd.TransformationAdapterUrl))
                    {
                        var transformationAdptr = new IngestTransformationAdapterClient(profileToAdd);
                        var status = transformationAdptr.SetConfiguration();
                        if (status != RestAdaptersCommon.eAdapterStatus.OK)
                        {
                            response.SetStatus((int)eResponseStatus.Error, "failed to call transformation adapter client");
                        }
                    }

                }
                else
                {
                    response.SetStatus((int)eResponseStatus.Error, "failed to insert new ingest profile");
                }
            }
            catch (Exception ex)
            {
                response.SetStatus((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed to insert ingest profile. Group Id: {0}. ex: {1}", groupId, ex);
            }

            return response;
        }

        public static GenericListResponse<IngestProfile> GetIngestProfiles(int groupId)
        {
            var response = new GenericListResponse<IngestProfile>();
            try
            {
                var profiles = ApiDAL.GetIngestProfilesByGroupId(groupId);
                response.SetStatus(eResponseStatus.OK);
                response.Objects = profiles;
            }
            catch (Exception ex)
            {
                response.SetStatus((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed to get ingest profiles. Group Id: {0}. ex: {1}", groupId, ex);
            }

            return response;
        }

        public static GenericResponse<IngestProfile> GetIngestProfileById(int groupId, int? profileId)
        {
            // If profileId is not provided we will use a default backward compatiable ingest profile
            if (!profileId.HasValue)
            {
                var defaultResponse = new GenericResponse<IngestProfile>();
                defaultResponse.Object = GetDefaultIngestProgile(groupId);
                defaultResponse.SetStatus(eResponseStatus.OK);
                return defaultResponse;
            }

            var response = new GenericResponse<IngestProfile>();
            try
            {
                var profile = ApiDAL.GetIngestProfilesByProfileId(profileId.Value).FirstOrDefault(profileItem => profileItem.GroupId.Equals(groupId));
                if (profile == null)
                {
                    response.SetStatus(eResponseStatus.IngestProfileNotExists);
                    return response;
                }

                response.SetStatus(eResponseStatus.OK);
                response.Object = profile;
            }
            catch (Exception ex)
            {
                response.SetStatus((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed to get ingest profiles. Group Id: {0}. ex: {1}", profileId, ex);
            }

            return response;
        }

        private static IngestProfile GetDefaultIngestProgile(int groupId)
        {
            return new IngestProfile
            {
                AssetTypeId = 1,
                DefaultAutoFillPolicy = eIngestProfileAutofillPolicy.KeepHoles,
                DefaultOverlapPolicy = eIngestProfileOverlapPolicy.CutTarget,
                ExternalId = "",
                GroupId = groupId,
                Id = 0,
                Name = "Default_Ingest_Profile",
                Settings = null,
                TransformationAdapterSharedSecret = null,
                TransformationAdapterUrl = null,
            };
        }

        public static Status DeleteIngestProfile(int groupId, int userId, int ingestProfileId)
        {
            var response = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (ingestProfileId <= 0)
                {
                    response.Set((int)eResponseStatus.IngestProfileNotExists, PROFILE_NOT_EXIST);
                    return response;
                }

                var profile = GetIngestProfileById(ingestProfileId);

                if (profile == null)
                {
                    response.Set((int)eResponseStatus.IngestProfileNotExists, PROFILE_NOT_EXIST);
                    return response;
                }

                var isDeleted = ApiDAL.DeleteIngestProfile(ingestProfileId, groupId, userId);
                if (!isDeleted)
                {
                    response.Set((int)eResponseStatus.Error, "Ingest profile failed to delete");
                    return response;
                }
            }
            catch (Exception ex)
            {
                response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to delete playback adapter. Group ID: {0}, adapterId: {1}", groupId, ingestProfileId), ex);
            }
            return response;
        }

        public static GenericResponse<IngestProfile> UpdateIngestProfile(int groupId, int userId, int ingestProfileId, IngestProfile profileToUpdate)
        {
            var response = new GenericResponse<IngestProfile>();
            try
            {
                if (ingestProfileId <= 0 || profileToUpdate == null || GetIngestProfileById(ingestProfileId) == null)
                {
                    response.SetStatus((int)eResponseStatus.IngestProfileNotExists, PROFILE_NOT_EXIST);
                    return response;
                }

                if (string.IsNullOrEmpty(profileToUpdate.Name))
                {
                    response.SetStatus((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (IsIngestProfileExternalIdExists(groupId, profileToUpdate.ExternalId, ingestProfileId))
                {
                    response.SetStatus((int)eResponseStatus.ExternalIdentifierMustBeUnique, ERROR_EXT_ID_ALREADY_IN_USE);
                    return response;
                }

                profileToUpdate.TransformationAdapterSharedSecret = profileToUpdate.TransformationAdapterSharedSecret ?? Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                var isProfileUpdateSuccess = ApiDAL.UpdateIngestProfile(ingestProfileId, groupId, userId, profileToUpdate);

                if (isProfileUpdateSuccess)
                {
                    profileToUpdate.Id = ingestProfileId;
                    response.Object = profileToUpdate;
                    response.SetStatus(eResponseStatus.OK, " ingest profile was successfully updated");

                    if (!string.IsNullOrEmpty(profileToUpdate.TransformationAdapterUrl))
                    {
                        var transformationAdptr = new IngestTransformationAdapterClient(profileToUpdate);
                        var status = transformationAdptr.SetConfiguration();
                        if (status != RestAdaptersCommon.eAdapterStatus.OK)
                        {
                            response.SetStatus((int)eResponseStatus.Error, "failed to call transformation adapter client");
                        }
                    }
                }
                else
                {
                    response.SetStatus((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    log.ErrorFormat("Failed to insert ingest profile. Group Id: {0} result from DB update was {1}", groupId, isProfileUpdateSuccess);
                }


            }
            catch (Exception ex)
            {
                response.SetStatus((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed to insert ingest profile. Group Id: {0}. ex: {1}", groupId, ex);
            }

            return response;
        }

        private static bool IsIngestProfileExternalIdExists(string externalId)
        {
            var profile = ApiDAL.Get_IngestProfileByExternalProfileId(externalId).FirstOrDefault();
            return profile != null;
        }

        private static bool IsIngestProfileExternalIdExists(int groupId, string externalId, int profileId)
        {
            var profileByExternalId = ApiDAL.Get_IngestProfileByExternalProfileId(externalId).FirstOrDefault();
            if (profileByExternalId == null)
                return false;
            
            var profileById = ApiDAL.GetIngestProfilesByProfileId(profileId).FirstOrDefault();
            return profileById?.Id != profileByExternalId.Id;
        }

        private static IngestProfile GetIngestProfileById(int id)
        {
            return ApiDAL.GetIngestProfilesByProfileId(id).FirstOrDefault();
        }
    }
}