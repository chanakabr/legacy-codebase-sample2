using System;
using phoenix;

namespace GrpcAPI.Services
{
    public interface IGroupAndConfigurationService
    {
        GetGroupSecretAndCountryCodeResponse GetGroupSecretAndCountryCode(GetGroupSecretAndCountryCodeRequest request);
    }

    public class GroupAndConfigurationService : IGroupAndConfigurationService
    {
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
        
    }
}