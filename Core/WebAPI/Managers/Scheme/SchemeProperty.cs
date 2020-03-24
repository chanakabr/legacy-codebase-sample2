using System;
using System.Web;
using WebAPI.Exceptions;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SchemePropertyAttribute : SchemeInputAttribute
    {
        public bool ReadOnly { get; set; }
        public bool InsertOnly { get; set; }
        public bool WriteOnly { get; set; }
        public int RequiresPermission { get; set; }
        public bool IsNullable { get; set; }

        public SchemePropertyAttribute() : base()
        {
            ReadOnly = false;
            InsertOnly = false;
            WriteOnly = false;
            IsNullable = false;
            RequiresPermission = 0;
        }
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class RuntimeSchemePropertyAttribute : SchemePropertyAttribute
    {
        private string TypeName;

        public RuntimeSchemePropertyAttribute(string typeName) : base()
        {
            TypeName = typeName;
        }

        internal void Validate(string parameterName, object value)
        {
            string name = string.Format("{0}.{1}", TypeName, parameterName);

            base.Validate(name, value);

            RequestType requiresPermission = RequestType.READ;
            if (HttpContext.Current.Items[RequestContext.REQUEST_TYPE] != null)
                requiresPermission = (RequestType)HttpContext.Current.Items[RequestContext.REQUEST_TYPE];

            if (!OldStandardAttribute.isCurrentRequestOldVersion())
            {
                if (isA(requiresPermission, RequestType.WRITE) && ReadOnly)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_IS_READONLY, name);
                }

                if (isA(requiresPermission, RequestType.UPDATE) && InsertOnly)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_IS_INSERTONLY, name);
                }
            }

            if (RequiresPermission > 0)
            {
                RequestType? requestType = (RequestType)HttpContext.Current.Items[RequestContext.REQUEST_TYPE];
                if (requestType.HasValue && isA(requestType.Value, RequiresPermission))
                {
                    RolesManager.ValidatePropertyPermitted(TypeName, parameterName, requestType.Value);
                }
            }
        }
    }
}