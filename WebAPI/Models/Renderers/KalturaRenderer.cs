using System.IO;

namespace WebAPI.Models.Renderers
{
    public abstract class KalturaRenderer
    {
        abstract public void Output(Stream writeStream);
    }
}