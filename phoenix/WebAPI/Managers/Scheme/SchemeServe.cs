using System;

namespace WebAPI.Managers.Scheme
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SchemeServeAttribute : Attribute
    {
        public string ContentType { get; set; }

        public SchemeServeAttribute()
            : base()
        {
            ContentType = "application/json";
        }
    }
}