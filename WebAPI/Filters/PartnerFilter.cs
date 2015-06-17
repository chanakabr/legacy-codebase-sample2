using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebAPI.Exceptions;

namespace WebAPI.Filters
{
    public class PartnerFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            string pID = (string) actionContext.ActionArguments["partner_id"];
            
            int groupId;
            if (!int.TryParse(pID, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be int");
            }

            base.OnActionExecuting(actionContext);
        }
    }
}
