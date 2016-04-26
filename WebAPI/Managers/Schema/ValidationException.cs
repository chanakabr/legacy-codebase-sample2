using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Schema
{
    public class ValidationException : Attribute
    {
        public ValidationException(ValidationType type)
        {
            ValidationType = type;
        }

        public ValidationType ValidationType { get; set; }
    }
}