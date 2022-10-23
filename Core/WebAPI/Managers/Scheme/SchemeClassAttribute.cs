using System;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SchemeClassAttribute : Attribute
    {
        public string[] Required { get; set; }
        public string[] OneOf { get; set; }
        public string[] AnyOf { get; set; }

        /// <summary>
        /// minimum properties allowed in class
        /// </summary>
        public int MinProperties { get; set; }

        /// <summary>
        /// maximum properties allowed in class
        /// </summary>
        public int MaxProperties { get; set; }

        public SchemeClassAttribute()
        {
            Required = null;
            OneOf = null;
            AnyOf = null;
            MinProperties = -1;
            MaxProperties = -1;
        }
    }
}
