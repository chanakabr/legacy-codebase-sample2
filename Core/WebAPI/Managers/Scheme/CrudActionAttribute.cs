using ApiObjects.Response;
using System;
using System.Collections.Generic;
using WebAPI.Managers.Models;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class CrudActionAttribute : Attribute
    {
        public string Summary { get; set; }
        public eResponseStatus[] ClientThrows { get; set; }
        public StatusCode[] ApiThrows { get; set; }
        /// <summary>
        /// Prevents from this service to be written in client xml
        /// </summary>
        public bool IsInternal { get; set; }
        public abstract string GetName();
        public abstract string GetDescription(string paramName);
        public virtual Dictionary<string, bool> GetOptionalParameters() { return null; }
    }

    public class AddActionAttribute : CrudActionAttribute
    {
        public const string Name = "add";
        public string ObjectToAddDescription { get; set; }
        
        public AddActionAttribute()
        {
            this.Summary = "Add an object";
            this.ObjectToAddDescription = "Object to add";
        }

        public override string GetName()
        {
            return Name;
        }

        public override string GetDescription(string paramName)
        {
            return this.ObjectToAddDescription;
        }
    }

    public class UpdateActionAttribute : CrudActionAttribute
    {
        public const string Name = "update";
        public string IdDescription { get; set; }
        public string ObjectToUpdateDescription { get; set; }
        
        public UpdateActionAttribute()
        {
            this.Summary = "Update an object";
            this.ObjectToUpdateDescription = "Object to update";
            this.IdDescription = "Object ID to update";
        }

        public override string GetName()
        {
            return Name;
        }

        public override string GetDescription(string paramName)
        {
            if (paramName == "id")
            {
                return IdDescription;
            }

            return ObjectToUpdateDescription;
        }
    }

    public class DeleteActionAttribute : CrudActionAttribute
    {
        public const string Name = "delete";
        public string IdDescription { get; set; }
        
        public DeleteActionAttribute()
        {
            this.Summary = "Delete an object";
            this.IdDescription = "Object ID to delete";
        }

        public override string GetName()
        {
            return Name;
        }

        public override string GetDescription(string paramName)
        {
            return this.IdDescription;
        }
    }

    public class GetActionAttribute : CrudActionAttribute
    {
        public const string Name = "get";
        public string IdDescription { get; set; }
        
        public GetActionAttribute()
        {
            this.Summary = "Get an object";
            this.IdDescription = "Object ID to get";
        }

        public override string GetName()
        {
            return Name;
        }

        public override string GetDescription(string paramName)
        {
            return this.IdDescription;
        }
    }
    
    public class ListActionAttribute : CrudActionAttribute
    {
        public const string Name = "list";
        
        private bool isPagerOptional;
        private bool hasPager;

        public string FilterDescription { get; set; }
        public string PagerDescription { get; set; }
        public bool IsFilterOptional { get; set; }
        
        public bool IsPagerOptional
        {
            get
            {
                return isPagerOptional;
            }
            set
            {
                isPagerOptional = value;
                hasPager = true;
            }
        }

        public ListActionAttribute()
        {
            this.FilterDescription = "Request filter";
            this.PagerDescription = "Request pager";
            this.IsFilterOptional = false;
        }

        public override string GetName()
        {
            return Name;
        }

        public override string GetDescription(string paramName)
        {
            if (paramName == "filter")
            {
                return FilterDescription;
            }

            if (paramName == "pager")
            {
                return PagerDescription;
            }
           
            return string.Empty;
        }

        public override Dictionary<string, bool> GetOptionalParameters()
        {
            var optionalParameters = new Dictionary<string, bool>()
            {
                { "filter", IsFilterOptional }
            };

            if (hasPager)
            {
                optionalParameters.Add("pager", IsPagerOptional);
            }

            return optionalParameters;
        }
    }
}