using System;
using System.Reflection;
using ApiObjects;
using DAL;
using Microsoft.Extensions.Logging;
using phoenix;
using Phx.Lib.Log;

namespace GrpcAPI.Services
{
    public interface IGroupAndConfigurationService
    {
        GetGroupSecretAndCountryCodeResponse GetGroupSecretAndCountryCode(GetGroupSecretAndCountryCodeRequest request);
        GetCDVRAdapterResponse GetCDVRAdapter(GetCDVRAdapterRequest request);
    }

    public class GroupAndConfigurationService : IGroupAndConfigurationService
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());

        public GetGroupSecretAndCountryCodeResponse GetGroupSecretAndCountryCode(
            GetGroupSecretAndCountryCodeRequest request)
        {
            string groupSecretCode = String.Empty;
            string groupCountryCode = String.Empty;
            bool isSuccess =
                DAL.ConditionalAccessDAL.Get_GroupSecretAndCountryCode(request.GroupId, ref groupSecretCode,
                    ref groupCountryCode);
            return new GetGroupSecretAndCountryCodeResponse
            {
                IsSuccess = isSuccess,
                CountryCode = groupCountryCode,
                SecretCode = groupSecretCode
            };
        }

        public GetCDVRAdapterResponse GetCDVRAdapter(GetCDVRAdapterRequest request)
        {
            try
            {
                int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(request.GroupId);
                CDVRAdapter cdvrAdapter = ConditionalAccessDAL.GetCDVRAdapter(request.GroupId, adapterId);
                return new GetCDVRAdapterResponse
                {
                    Id = cdvrAdapter.ID,
                    AdapterUrl = cdvrAdapter.AdapterUrl,
                    DynamicLinksSupport = cdvrAdapter.DynamicLinksSupport,
                    SharedSecret = cdvrAdapter.SharedSecret,
                    ExternalIdentifier = cdvrAdapter.ExternalIdentifier
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while calling GetCDVRAdapter GRPC service {e.Message}");
            }

            return null;
        }
    }
}