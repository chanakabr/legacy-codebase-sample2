using System.IO;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Base class
    /// </summary>
    public class KalturaOTTFile
    {
        public KalturaOTTFile(string filePath, string fileName)
        {
            path = filePath;
            name = fileName;
        }

        public KalturaOTTFile(Stream fileStream, string fileName)
        {
            File = fileStream;
            name = fileName;
            path = string.Empty;
        }

        public Stream File { get; set; }
        public string path { get; set; }
        public string name { get; set; }
    }
}