using System.IO;

namespace WebAPI.Models.Renderers
{
    public abstract class KalturaRenderer
    {
        public abstract void Output(Stream writeStream);
        public abstract string GetOutput();
    }
}