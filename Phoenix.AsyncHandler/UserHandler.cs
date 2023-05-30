using ApiObjects;
using ApiObjects.Segmentation;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Crud.OttUser;

namespace Phoenix.AsyncHandler
{
    public class UserHandler : CrudHandler<OttUser>
    {
        protected override long GetOperation(OttUser value) => value.Operation;

        protected override HandleResult Create(ConsumeResult<string, OttUser> consumeResult)
        {
            var user = consumeResult.GetValue();
            if (user.Source == Source.Phoenix) return Result.Ok;
            
            Core.Users.Utils.AddInitiateNotificationActionToQueue((int)user.PartnerId, eUserMessageAction.Signup, (int)user.Id.Value, string.Empty);
            return Result.Ok;
        }

        protected override HandleResult Update(ConsumeResult<string, OttUser> consumeResult) => Result.Ok;

        protected override HandleResult Delete(ConsumeResult<string, OttUser> consumeResult)
        {
            var user = consumeResult.GetValue();
            if (user.Source == Source.Phoenix) return Result.Ok;
            
            // GDPR TTV
            UserSegment.Remove(user.Id.ToString());
            
            Core.Users.Utils.AddInitiateNotificationActionToQueue((int)user.PartnerId, eUserMessageAction.DeleteUser, (int)user.Id.Value, string.Empty);
            return Result.Ok;
        }
    }
}