using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models
{    
    public enum Order
    {
        relevancy,

        a_to_z,

        z_to_a,

        views,

        ratings,

        votes,

        newest        
    }
}