using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPI.Models.General
{
    [Serializable]
    public enum KalturaEventAction
    {
        None,
        Added,
        Changed,
        Copied,
        Created,
        Deleted,
        Erased,
        Saved,
        Updated,
        Replaced
    }
}
