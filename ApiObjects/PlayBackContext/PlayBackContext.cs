using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class PlayBackContextResponse
    {
        public List<MediaFile> Files { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }

    public class MediaFile
    {

    }

}
