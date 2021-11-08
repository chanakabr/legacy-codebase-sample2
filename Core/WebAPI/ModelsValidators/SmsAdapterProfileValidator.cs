using ApiObjects.Response;
using WebAPI.Exceptions;
using WebAPI.Models.Notification;

namespace WebAPI.ModelsValidators
{
    public static class SmsAdapterProfileValidator
    {
        public static void ValidateForAdd(this KalturaSmsAdapterProfile model)
        {
            if (model == null) { throw new ClientException((int)eResponseStatus.NoAdapterToInsert, "No sms adapter to add"); }
            if (string.IsNullOrEmpty(model.AdapterUrl)) { throw new ClientException((int)eResponseStatus.AdapterUrlRequired, "Adapter Url Required"); }
            if (string.IsNullOrEmpty(model.SharedSecret)) { throw new ClientException((int)eResponseStatus.SharedSecretRequired, "Shared Secret Required"); }
        }

        public static void ValidateForUpdate(this KalturaSmsAdapterProfile model)
        {
            if (model.Id == 0)
            {
                throw new ClientException((int)eResponseStatus.IdentifierRequired, "Id is missing");
            }
        }
    }
}