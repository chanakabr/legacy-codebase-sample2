using ApiObjects.Response;
using System;
using WebAPI.Managers.Models;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class CrudActionAttribute : Attribute
    {
        public string Name { get; protected set; }
        public string Summary { get; set; }
        public eResponseStatus[] ClientThrows { get; set; }
        public StatusCode[] ApiThrows { get; set; }

        public abstract string GetDescription(string paramName);
    }

    public class AddActionAttribute : CrudActionAttribute
    {
        public const string Add = "add";
        public string ObjectToAddDescription { get; set; }
        
        public AddActionAttribute()
        {
            this.Name = Add;
            this.Summary = "Add an object";
            this.ObjectToAddDescription = "Object to add";
        }

        public override string GetDescription(string paramName)
        {
            return this.ObjectToAddDescription;
        }
    }

    public class UpdateActionAttribute : CrudActionAttribute
    {
        public const string Update = "update";
        public string IdDescription { get; set; }
        public string ObjectToUpdateDescription { get; set; }
        
        public UpdateActionAttribute()
        {
            this.Name = Update;
            this.Summary = "Update an object";
            this.ObjectToUpdateDescription = "Object to update";
            this.IdDescription = "Object ID to update";
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
        public const string Delete = "delete";
        public string IdDescription { get; set; }
        
        public DeleteActionAttribute()
        {
            this.Name = Delete;
            this.Summary = "Delete an object";
            this.IdDescription = "Object ID to delete";
        }

        public override string GetDescription(string paramName)
        {
            return this.IdDescription;
        }
    }

    public class GetActionAttribute : CrudActionAttribute
    {
        public const string Get = "get";
        public string IdDescription { get; set; }
        
        public GetActionAttribute()
        {
            this.Name = Get;
            this.Summary = "Get an object";
            this.IdDescription = "Object ID to get";
        }

        public override string GetDescription(string paramName)
        {
            return this.IdDescription;
        }
    }

    // TODO SHIR - FINISH ALL PROPERTIES OF ListActionAttribute
    public class ListActionAttribute : CrudActionAttribute
    {
        public const string List = "list";

        public ListActionAttribute()
        {
            this.Name = List;
        }

        public override string GetDescription(string paramName)
        {
            throw new NotImplementedException();
        }
    }
}