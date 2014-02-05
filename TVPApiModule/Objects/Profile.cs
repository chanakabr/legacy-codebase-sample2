using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Objects.Responses;

/// <summary>
/// Summary description for Profile
/// </summary>
/// 

namespace TVPApiModule.Objects
{
    public class Profile
    {
        public long ProfileID { get; set; }

        public List<PageGallery> Galleries { get; set; }

        public Profile()
        {
            
        }
    }
}
