using System;
using System.IO;

namespace FileUploadHandler
{
    public class LocalUploader : BaseUploader
    {
        public LocalUploader()
        {
        }

        public override void Upload(string file, string fileName)
        {
            byte[] bytes = Convert.FromBase64String(file);

            File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + fileName, bytes);
        }
    }
}
