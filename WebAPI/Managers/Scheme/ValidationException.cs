using System;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class ValidationException : Attribute
    {
        public ValidationException(SchemeValidationType type)
        {
            ValidationType = type;
        }

        public SchemeValidationType ValidationType { get; set; }
    }
}