using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonWithSL
{    
    public class MediaFile
    {
        public string FileID { get; set; }
        public string URL { get; set; }
        public string FileType { get; set; }
        public string Lang { get; set; }
        public bool CanBeAccessed { get; set; }
        public bool IsSelected { get; set; }

        public MediaFile()
        {
            CanBeAccessed = true;
        }
    }
}
