using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using ImageResizer;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;

namespace ImageResizeHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private string generateSlug(string phrase)
        {
            string str = removeAccent(phrase).ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 255 ? str.Length : 255).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        private string removeAccent(string txt)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public string HandleTask(string data)
        {
            string res = null;

            ResizeImageRequest request = JsonConvert.DeserializeObject<ResizeImageRequest>(data);

            Uri uri = new Uri(request.url);

            byte[] source = null;

            try
            {
                bool _useFileSystem = false;

                string _sUseFileSystem = TCMClient.Settings.Instance.GetValue<string>("TASK_HANDLERS.IMAGE_RESIZER.USE_FILE_SYSTEM");

                if (!string.IsNullOrEmpty(_sUseFileSystem))
                {
                    bool.TryParse(_sUseFileSystem, out _useFileSystem);
                }

                if (_useFileSystem)
                {
                    bool isMutexAlreadyReleased = false;

                    string fileName = generateSlug(System.IO.Path.GetFullPath(uri.LocalPath));

                    Mutex mutex = new Mutex(false, "IRHM_" + fileName);

                    try
                    {
                        mutex.WaitOne();

                        string imagesBasePath = TCMClient.Settings.Instance.GetValue<string>("TASK_HANDLERS.IMAGE_RESIZER.IMAGES_BASE_PATH");

                        if (string.IsNullOrEmpty(imagesBasePath))
                        {
                            imagesBasePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
                        }
                        else if (!Directory.Exists(imagesBasePath))
                        {
                            Directory.CreateDirectory(imagesBasePath);
                        }

                        FileInfo fileInf = new FileInfo(imagesBasePath + fileName);

                        if (fileInf.Exists)
                        {
                            isMutexAlreadyReleased = true;

                            mutex.ReleaseMutex();

                            using (FileStream fs = fileInf.OpenRead())
                            {
                                source = fs.ToByteArray();
                            }
                        }
                        else
                        {
                            source = uri.ToByteArray();

                            using (FileStream fs = fileInf.OpenWrite())
                            {
                                fs.Write(source, 0, source.Length);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        source = uri.ToByteArray();
                    }
                    finally
                    {
                        if (mutex != null && !isMutexAlreadyReleased)
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
                else
                {
                    source = uri.ToByteArray();
                }

                using (MemoryStream dest = new MemoryStream())
                {
                    ResizeSettings resizeSettings = new ResizeSettings()
                    {
                        Mode = FitMode.Crop,
                        Format = null,
                        Quality = 80
                    };

                    string size = request.size.ToLower();

                    switch (size)
                    {
                        case "tn":

                            resizeSettings.Width = 90;
                            resizeSettings.Height = 65;

                            break;

                        case "full":

                            resizeSettings.Quality = 100;

                            break;

                        default:

                            string[] dimensions = size.ToLower().Split('x');

                            resizeSettings.Width = int.Parse(dimensions[0]);
                            resizeSettings.Height = int.Parse(dimensions[1]);

                            break;
                    }

                    ImageResizer.ImageBuilder.Current.Build(source, dest, resizeSettings, false);

                    Byte[] bytes = dest.ToArray();

                    if (bytes != null && bytes.Length > 0)
                        log.DebugFormat("successfully resized image. source: {0}, dest: {1}, resizeSettings: {2}", source, dest, JsonConvert.SerializeObject(resizeSettings));

                    res = Convert.ToBase64String(bytes);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while resizing image", ex);
                throw ex;
            }

            return res;
        }
    }
}

