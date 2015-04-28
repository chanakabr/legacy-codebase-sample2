using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class Language
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Direction { get; set; }

        public bool IsDefault { get; set; }
    }
}