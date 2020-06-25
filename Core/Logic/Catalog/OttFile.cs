using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ApiLogic.Catalog
{
    public abstract class OTTBasicFile
    {
        public abstract GenericResponse<string> SaveFile(string id, string objectTypeName);
        public abstract GenericResponse<string> SaveFile(long id, string objectTypeName);
        public string Name { get; set; }
    }

    public class OTTStreamFile : OTTBasicFile
    {
       
        public Stream File { get; set; }

        public Stream GetFileStream()
        {
            return File;
        }

        public override GenericResponse<string> SaveFile(string id, string objectTypeName)
        {
           return FileHandler.Instance.SaveFile(id, this, objectTypeName);
        }

        public override GenericResponse<string> SaveFile(long id, string objectTypeName)
        {
            return FileHandler.Instance.SaveFile(id, this, objectTypeName);
        }

        public OTTStreamFile(Stream formFile, string fileName)
        {
            File = formFile;            
            Name = fileName;
        }
    }

    public class OTTFile: OTTBasicFile
    {
        public OTTFile(string filePath, string fileName)
        {
            Path = filePath;
            Name = fileName;
        }        
        public string Path { get; set; }

        public override GenericResponse<string> SaveFile(string id, string objectTypeName)
        {
            return FileHandler.Instance.SaveFile(id, this, objectTypeName);
        }

        public override GenericResponse<string> SaveFile(long id, string objectTypeName)
        {
            return FileHandler.Instance.SaveFile(id, this, objectTypeName);
        }
    }
}
