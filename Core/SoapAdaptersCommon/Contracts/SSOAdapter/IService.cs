using System.Collections.Generic;
using System.ServiceModel;
using AdapaterCommon.Models;
using SSOAdapter.Models;

namespace SSOAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatusCode SetConfiguration(int adapterId, int partnerId, IDictionary<string, string> configuration, string signature);

        [OperationContract]
        SSOImplementationsResponse GetConfiguration(int adapterId);

        [OperationContract]
        PreSignInResponse PreSignIn(int adapterId, PreSignInModel preSignInData, string signature);

        [OperationContract]
        UserResponse PostSignIn(int adapterId, PostSignInModel postSignInData, string signature);

        [OperationContract]
        UserResponse PreGetUserData(int adapterId, int userId, string ipAddress, IDictionary<string, string> customParams, string signature);

        [OperationContract]
        UserResponse PostGetUserData(int adapterId, User userData, IDictionary<string, string> customParams, string signature);

        [OperationContract]
        UserResponse PreSignOut(int adapterId, PreSignOutModel preSignOutData, string signature);

        [OperationContract]
        UserResponse PostSignOut(int adapterId, PostSignOutModel postSignOutData, string signature);

        [OperationContract]
        SSOAdapterProfileInvokeResponse Invoke(int adapterId, SSOAdapterProfileInvokeModel ssoAdapterProfileInvokeModel);

        [OperationContract]
        AdjustRegionIdResponse AdjustRegionId(int adapterId, int dafaultRegionId, User ottUser, List<long> userSegments, IDictionary<string, string> adapterData);
    }
}
