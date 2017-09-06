using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.API
{
    public enum KalturaMetaType
    {
        STRING,
        NUMBER,
        BOOLEAN,
        //STRING_ARRAY // tag  this was modified on Yoda (4.6) version, instaed of STRING_ARRAY we have parameter MultipleValue on KalturaMeta.
        DATE
    }
}