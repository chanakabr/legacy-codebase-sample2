using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace CouchbaseManager.Compression
{
    public static class GzipExtensions
    {
        public static byte[] Compress(this string obj)
        {
            byte[] compressed;
            using (var resultStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(resultStream, CompressionMode.Compress))
                {
                    using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(obj)))
                    {
                        memoryStream.CopyTo(gzipStream);
                    }   
                }

                compressed = resultStream.ToArray();
            }

            return compressed;
        }
        
        public static byte[] Compress<T>(this T obj, JsonSerializerSettings settings = null)
        {
            var serializedObject = JsonConvert.SerializeObject(obj, settings ?? new JsonSerializerSettings {Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.Auto});
            return serializedObject.Compress();
        }

        public static string Decompress(this byte[] compressed)
        {
            using (var compressedMemoryStream = new MemoryStream(compressed))
            {
                using (var decompressStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress))
                {
                    using (var outputStream = new MemoryStream())
                    {
                        decompressStream.CopyTo(outputStream);
                        return Encoding.UTF8.GetString(outputStream.ToArray());
                    }
                }
            }
        }

        public static T Decompress<T>(this byte[] compressed, JsonSerializerSettings settings = null)
        {
            var jsonSerializerSettings = settings ?? new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            return JsonConvert.DeserializeObject<T>(compressed.Decompress(), jsonSerializerSettings);

        }
    }
}