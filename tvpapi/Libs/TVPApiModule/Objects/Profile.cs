using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;

/// <summary>
/// Summary description for Profile
/// </summary>
/// 

namespace TVPApi
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
