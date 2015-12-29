using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Application token hash type
    /// </summary>
    public enum KalturaAppTokenHashType
    {
        SHA1,		
        SHA256,			
        SHA512,
        MD5,
    }
}