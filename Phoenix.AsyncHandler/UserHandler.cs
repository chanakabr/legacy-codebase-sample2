using ApiObjects;
using ApiObjects.Segmentation;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Crud.OttUser;
using SchemaRegistryEvents;

namespace Phoenix.AsyncHandler
{
    public class UserHandler : CrudHandler<OttUser>
    {
        protected override long GetOperation(OttUser value) => value.Operation;

        protected override HandleResult Create(ConsumeResult<string, OttUser> consumeResult)
        {
            if (consumeResult.GetSourceService() == SourceService.Phoenix) return Result.Ok;
            var user = consumeResult.GetValue();

            Core.Users.Utils.AddInitiateNotificationActionToQueue((int)user.PartnerId, eUserMessageAction.Signup, (int)user.Id.Value, string.Empty);
            return Result.Ok;
        }

        protected override HandleResult Update(ConsumeResult<string, OttUser> consumeResult) => Result.Ok;

        protected override HandleResult Delete(ConsumeResult<string, OttUser> consumeResult)
        {
            if (consumeResult.GetSourceService() == SourceService.Phoenix) return Result.Ok;
            var user = consumeResult.GetValue();

            // GDPR TTV
            UserSegment.Remove(user.Id.ToString());
            
            Core.Users.Utils.AddInitiateNotificationActionToQueue((int)user.PartnerId, eUserMessageAction.DeleteUser, (int)user.Id.Value, string.Empty);
            return Result.Ok;
        }
    }
}