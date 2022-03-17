using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Social;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class SocialFriendActivityFilterMapper
    {
        public static List<KalturaSocialActionType> GetActionTypeIn(this KalturaSocialFriendActivityFilter model)
        {
            List<KalturaSocialActionType> actions = new List<KalturaSocialActionType>();
            if (!string.IsNullOrEmpty(model.ActionTypeIn))
            {
                string[] splitActions = model.ActionTypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string action in splitActions)
                {
                    KalturaSocialActionType parsedAction;
                    if (Enum.TryParse(action, true, out parsedAction) && 
                        (parsedAction == KalturaSocialActionType.LIKE || parsedAction == KalturaSocialActionType.RATE || parsedAction == KalturaSocialActionType.WATCH))
                    {
                        actions.Add(parsedAction);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "KalturaSocialFriendActivityFilter.actionTypeIn", action);
                    }
                }
            }

            return actions;

        }
    }
}