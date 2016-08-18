using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SchemePropertyAttribute : SchemeInputAttribute
    {
        public bool ReadOnly { get; set; }
        public bool InsertOnly { get; set; }
        public bool WriteOnly { get; set; }
        public int RequiresPermission { get; set; }

        public SchemePropertyAttribute() : base()
        {
            ReadOnly = false;
            InsertOnly = false;
            WriteOnly = false;
            RequiresPermission = 0;
        }

        internal void Validate(string typeName, string parameterName, object value)
        {
            string name = string.Format("{0}.{1}", typeName, parameterName);

            base.Validate(name, value);

            RequestType requiresPermission = RequestType.READ;
            if(HttpContext.Current.Items[RequestParser.REQUEST_TYPE] != null)
                requiresPermission = (RequestType)HttpContext.Current.Items[RequestParser.REQUEST_TYPE];

            if (isA(requiresPermission, RequestType.WRITE) && ReadOnly && !OldStandardAttribute.isCurrentRequestOldVersion())
            {
                throw new BadRequestException((int)StatusCode.InvalidActionParameters, string.Format("Object property {0} is read only.", name));
            }

            if (isA(requiresPermission, RequestType.UPDATE) && InsertOnly)
                throw new BadRequestException((int)StatusCode.InvalidActionParameters, string.Format("Object property {0} is insert only.", name));

            if (RequiresPermission > 0)
            {
                RequestType? requestType = (RequestType)HttpContext.Current.Items[RequestParser.REQUEST_TYPE];
                if (requestType.HasValue && isA(requestType.Value, RequiresPermission))
                {
                    RolesManager.ValidatePropertyPermitted(typeName, parameterName, requestType.Value);
                }
            }
        }
    }
}