using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebAPI.Exceptions;

namespace WebAPI.App_Start
{
    public class ValidateRequest : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var qs = HttpUtility.ParseQueryString(actionContext.Request.RequestUri.Query);
            if (qs["user_id"] != null)
            {
                if (string.IsNullOrEmpty((string)qs["user_id"]))
                {
                    throw new BadRequestException((int)WebAPI.Models.General.StatusCode.UserIDInvalid, "no user_id");
                }
            }

            if (qs["partner_id"] != null)
            {
                int groupId;
                if (!int.TryParse((string)qs["partner_id"], out groupId))
                {
                    throw new BadRequestException((int)WebAPI.Models.General.StatusCode.PartnerInvalid, "partner_id must be int");
                }
            }

            if (qs["household_id"] != null)
            {
                int did = 0;
                if (!int.TryParse(qs["household_id"], out did) || did <= 0)
                {
                    throw new BadRequestException((int)WebAPI.Models.General.StatusCode.HouseholdInvalid, "household_id is invalid");
                }
            }

            base.OnActionExecuting(actionContext);
        }
    }
}
