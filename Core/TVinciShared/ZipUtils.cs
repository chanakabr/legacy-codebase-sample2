using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace TVinciShared
{
    public class ZipUtils
    {
        public static byte[] Compress(string text)
        {
            MemoryStream msCompressed = new MemoryStream();
            ZipOutputStream zosCompressed = new ZipOutputStream(msCompressed);
            zosCompressed.SetLevel(9);
            byte[] bytesBuffer = Encoding.UTF8.GetBytes(text);
            ZipEntry entry = new ZipEntry("api.xml");
            entry.DateTime = DateTime.UtcNow;
            entry.Size = bytesBuffer.Length; 

            zosCompressed.PutNextEntry(entry);
            zosCompressed.Write(bytesBuffer, 0, bytesBuffer.Length);
            zosCompressed.Finish();
            zosCompressed.Close();
            return msCompressed.ToArray();
        }

        public static byte[] Compress(Dictionary<string, byte[]> files)
        {
            MemoryStream msCompressed = new MemoryStream();
            ZipOutputStream zosCompressed = new ZipOutputStream(msCompressed);
            zosCompressed.SetLevel(9);
            foreach (KeyValuePair<string, byte[]> file in files)
            {
                ZipEntry entry = new ZipEntry(file.Key);
                entry.DateTime = DateTime.UtcNow;
                entry.Size = file.Value.Length;
                zosCompressed.PutNextEntry(entry);
                zosCompressed.Write(file.Value, 0, file.Value.Length);
            }

            zosCompressed.Finish();
            zosCompressed.Close();
            return msCompressed.ToArray();
        }
    }
}
