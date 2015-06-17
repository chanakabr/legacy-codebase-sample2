using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebAPI.Exceptions;

namespace WebAPI.Filters
{
    public class UserFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (string.IsNullOrEmpty((string)actionContext.ActionArguments["user_id"]))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.UserIDInvalid, "no user_id");
            }

            base.OnActionExecuting(actionContext);
        }
    }
}
