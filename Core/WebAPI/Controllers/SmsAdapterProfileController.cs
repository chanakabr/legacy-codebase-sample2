using ApiObjects;
using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Notification;
using WebAPI.Utils;
namespace WebAPI.Controllers
{
    [AddAction(ClientThrows = new [] { eResponseStatus.RequestFailed })]
    [UpdateAction(ClientThrows = new [] { eResponseStatus.AdapterNotExists })]
    [GetAction(ClientThrows = new eResponseStatus[] { })]
    [ListAction(ClientThrows = new eResponseStatus[] { })]
    [DeleteAction(ClientThrows = new [] { eResponseStatus.AdapterIdentifierRequired, eResponseStatus.AdapterNotExists })]
    [Service("smsAdapterProfile")]
    public class SmsAdapterProfileController : KalturaCrudController<KalturaSmsAdapterProfile, KalturaSmsAdapterProfileListResponse, SmsAdapterProfile, long, KalturaSmsAdapterProfileFilter>
    {
        /// <summary>
        /// Generate Sms Adapter shared secret
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// Sms Adapter id required = 2058, sms adapater not exist = 2056
        /// </remarks>
        /// <param name="smsAdapterId">Sms Adapter identifier</param>
        [Action("generateSharedSecret")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.IdentifierRequired)]
        [Throws(eResponseStatus.AdapterNotExists)]
        [Throws(eResponseStatus.NotExist)]
        static public KalturaSmsAdapterProfile GenerateSharedSecret(int smsAdapterId)
        {
            KalturaSmsAdapterProfile response = null;
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId;

            try
            {
                response = ClientsManager.NotificationClient().GenerateSmsAdapaterSharedSecret(groupId, smsAdapterId, int.Parse(userId));
                if (response == null) { throw new ClientException((int)eResponseStatus.AdapterNotExists); }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}
