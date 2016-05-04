using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Schema
{
    public class ValidationException : Attribute
    {
        public ValidationException(SchemaValidationType type)
        {
            ValidationType = type;
        }

        public SchemaValidationType ValidationType { get; set; }
    }
}