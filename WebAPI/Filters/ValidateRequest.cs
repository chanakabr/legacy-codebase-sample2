using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebAPI.Exceptions;

namespace WebAPI.App_Start
{
    public class ValidateRequest : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ActionArguments.ContainsKey("user_id"))
            {
                if (string.IsNullOrEmpty((string)actionContext.ActionArguments["user_id"]))
                {
                    throw new BadRequestException((int)WebAPI.Models.General.StatusCode.UserIDInvalid, "no user_id");
                }
            }

            if (actionContext.ActionArguments.ContainsKey("partner_id"))
            {
                int groupId;
                if (!int.TryParse((string)actionContext.ActionArguments["partner_id"], out groupId))
                {
                    throw new BadRequestException((int)WebAPI.Models.General.StatusCode.PartnerInvalid, "partner_id must be int");
                }
            }

            if (actionContext.ActionArguments.ContainsKey("household_id"))
            {
                int hID;
                if (!int.TryParse((string)actionContext.ActionArguments["household_id"], out hID) || hID <= 0)
                {
                    throw new BadRequestException((int)WebAPI.Models.General.StatusCode.HouseholdInvalid, "household_id is invalid");
                }
            }

            base.OnActionExecuting(actionContext);
        }
    }
}
