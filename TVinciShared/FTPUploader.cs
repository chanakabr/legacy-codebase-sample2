using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace TVinciShared
{
    /// <summary>
    /// Summary description for FTPUploader
    /// </summary>
    public class FTPUploader
    {
        protected string m_sFileToUpload;
        protected bool m_isDelete;
        protected string m_directoryString;
        protected string m_sFTPServerIP;
        protected Int32 m_nFTPPort;
        protected string m_sFTPUserName;
        protected string m_sFTPPass;
        static public Int32 m_nNumberOfRuningUploads = 0;
        static public string m_currentGroupDirUpload = string.Empty;
        static public List<string> m_currentlyUploadedGroups = new List<string>();
        protected int m_nGroupID = 0;
        private static ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();
        
        public FTPUploader(string sFile, string sFTPServerIP, string sFTPUN, string sFTPPass)
        {
            m_sFileToUpload = sFile;
            m_sFTPServerIP = sFTPServerIP;
            if (m_sFTPServerIP.StartsWith("ftp://") == true)
                m_sFTPServerIP = m_sFTPServerIP.Substring(6);
            if (m_sFTPServerIP.StartsWith("sftp://") == true)
                m_sFTPServerIP = m_sFTPServerIP.Substring(7);
            m_sFTPUserName = sFTPUN;
            m_sFTPPass = sFTPPass;
            m_nFTPPort = 21;
            m_isDelete = false;
        }

        public FTPUploader(string sFile, string sFTPServerIP, string sFTPUN, string sFTPPass, bool isDelete)
        {
            m_sFileToUpload = sFile;
            m_sFTPServerIP = sFTPServerIP;
            if (m_sFTPServerIP.StartsWith("ftp://") == true)
                m_sFTPServerIP = m_sFTPServerIP.Substring(6);
            if (m_sFTPServerIP.StartsWith("sftp://") == true)
                m_sFTPServerIP = m_sFTPServerIP.Substring(7);
            m_sFTPUserName = sFTPUN;
            m_sFTPPass = sFTPPass;
            m_nFTPPort = 21;
            m_isDelete = isDelete;
        }

        public FTPUploader(string sFile, string sFTPServerIP, string sFTPUN, string sFTPPass, string directoryStr)
        {
            m_sFileToUpload = sFile;
            m_directoryString = directoryStr;
            m_sFTPServerIP = sFTPServerIP;
            if (m_sFTPServerIP.StartsWith("ftp://") == true)
                m_sFTPServerIP = m_sFTPServerIP.Substring(6);
            if (m_sFTPServerIP.StartsWith("sftp://") == true)
                m_sFTPServerIP = m_sFTPServerIP.Substring(7);
            m_sFTPUserName = sFTPUN;
            m_sFTPPass = sFTPPass;
            m_nFTPPort = 21;
        }

        public FTPUploader(int nGroupID, string sFile, string sFTPServerIP, string sFTPUN, string sFTPPass, string directoryStr)
        {
            m_sFileToUpload = sFile;
            m_directoryString = directoryStr;
            m_sFTPServerIP = sFTPServerIP;
            if (m_sFTPServerIP.StartsWith("ftp://") == true)
                m_sFTPServerIP = m_sFTPServerIP.Substring(6);
            if (m_sFTPServerIP.StartsWith("sftp://") == true)
                m_sFTPServerIP = m_sFTPServerIP.Substring(7);
            m_sFTPUserName = sFTPUN;
            m_sFTPPass = sFTPPass;
            m_nFTPPort = 21;
            m_nGroupID = nGroupID;
        }

        static public void SetRunningProcesses(Int32 n)
        {
            m_nNumberOfRuningUploads = n;
        }

        public void Upload()
        {
            if (m_sFTPServerIP.Trim() == "")
                return;

            while (m_nNumberOfRuningUploads > 10)
            {
                System.Threading.Thread.Sleep(500);
                Logger.Logger.Log("upload pic to: " + m_sFTPServerIP + " with un: " + m_sFTPUserName + ", pass: " + m_sFTPPass, "waiting - more then 10 uploads parallel", "FTPUpload");
            }
            m_nNumberOfRuningUploads++;
            Logger.Logger.Log("upload pic to: " + m_sFTPServerIP + " with un: " + m_sFTPUserName + ", pass: " + m_sFTPPass, "start", "FTPUpload");
            FileInfo fileInf = new FileInfo(m_sFileToUpload);
            string uri = "ftp://" + m_sFTPServerIP + "/" + fileInf.Name;
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
           
            //reqFTP.EnableSsl = true;
            //reqFTP.RequestUri.Port
            reqFTP.Credentials = new NetworkCredential(m_sFTPUserName, m_sFTPPass);
            //reqFTP.UsePassive = true;
            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.UseBinary = true;
            reqFTP.ContentLength = fileInf.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = fileInf.OpenRead();
            try
            {
                Stream strm = reqFTP.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
                Logger.Logger.Log("upload pic to: " + m_sFTPServerIP + " with un: " + m_sFTPUserName + ", pass: " + m_sFTPPass, "finished", "FTPUpload");
                //System.IO.File.Delete(m_sFileToUpload);
                if (m_isDelete)
                {
                    fileInf.Delete();
                    Logger.Logger.Log("DeleteFile from UploadFile: ", m_sFTPServerIP + m_sFileToUpload, "UploadedPics");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Upload pic: " + m_sFileToUpload, "exception" + ex.Message + " || " + ex.StackTrace, "FTPUpload");
            }
            m_nNumberOfRuningUploads--;
        }

        public void UploadDirectory()
        {
            if (m_sFTPServerIP.Trim() == "")
                return;

            
           // m_nNumberOfRuningUploads++;
            Logger.Logger.Log("upload directory to: " + m_sFTPServerIP + " with un: " + m_sFTPUserName + ", pass: " + m_sFTPPass, "start", "FTPUpload");
            if (!Directory.Exists(m_directoryString))
            {
                Logger.Logger.Log("Directory does not exist: " + m_directoryString, "Directory does not exist: " + m_directoryString, "FTPUpload");
                return;
            }

            int failCount = 0;
            string[] files = Directory.GetFiles(m_directoryString);
            if (files != null && files.Length > 0)
            {
                //while (IsGroupUploading())
                //{
                //    System.Threading.Thread.Sleep(500);
                //    Logger.Logger.Log("upload pic to: " + m_sFTPServerIP + " with un: " + m_sFTPUserName + ", pass: " + m_sFTPPass, "Group ALready Uploading", "UploadedPics");
                //}
                files = Directory.GetFiles(m_directoryString);
                AddUploadGroup();
                Logger.Logger.Log("Start Uploading Files " + files.Length + " to: " + m_sFTPServerIP + " with un: " + m_sFTPUserName + ", pass: " + m_sFTPPass, "start", "DirectoryUpload");
                foreach (string file in files)
                {
                    Stream strm = null;
                    FileStream fs = null;
                    try
                    {
                        if (failCount > 3)
                        {
                            Logger.Logger.Log("Fail Count: ", "Fail Count from directory " + m_directoryString, "FTPUpload");
                            RemoveUploadGroup();

                            break;
                        }
                        Logger.Logger.Log("Start upload file: ", m_sFTPServerIP + file, "FTPUpload");
                        FileInfo fileInf = new FileInfo(file);
                        string uri = "ftp://" + m_sFTPServerIP + "/" + fileInf.Name;
                        FtpWebRequest reqFTP = null;
                        reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                        //reqFTP.EnableSsl = true;
                        //reqFTP.RequestUri.Port
                        reqFTP.Credentials = new NetworkCredential(m_sFTPUserName, m_sFTPPass);
                        reqFTP.UsePassive = true;
                        reqFTP.KeepAlive = false;
                        reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                        reqFTP.UseBinary = true;

                        reqFTP.ConnectionGroupName = "Ftp_" + m_nGroupID.ToString();
                        reqFTP.Timeout = 240000;


                        strm = reqFTP.GetRequestStream();

                        reqFTP.ContentLength = fileInf.Length;
                        int buffLength = 2048;
                        byte[] buff = new byte[buffLength];
                        int contentLen;
                        fs = fileInf.OpenRead();
                        try
                        {

                            contentLen = fs.Read(buff, 0, buffLength);
                            while (contentLen != 0)
                            {
                                strm.Write(buff, 0, contentLen);
                                contentLen = fs.Read(buff, 0, buffLength);
                            }
                            //strm.Close();
                            //fs.Close();
                            Logger.Logger.Log("upload file " + file + " to: " + m_sFTPServerIP + " with un: " + m_sFTPUserName + ", pass: " + m_sFTPPass, "finished", "FTPUpload");
                            //fileInf.Delete();


                            //System.Threading.Thread.Sleep(5000);
                            //System.IO.File.Delete(m_sFileToUpload);
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            Logger.Logger.Log("Upload file: " + m_sFileToUpload, "exception" + ex.Message + " || " + ex.StackTrace, "FTPUpload", "FTP Upload exception on file " + file + " server " + m_sFTPServerIP + " " + ex.Message);
                            if (failCount > 3)
                            {

                            }
                        }
                        finally
                        {
                            if (strm != null)
                            {
                                strm.Close();
                                Logger.Logger.Log("Stream Closed: ", file, "UploadedPics");
                            }
                            if (fs != null)
                            {
                                fs.Close();
                                Logger.Logger.Log("File Stream Closed: ", file, "UploadedPics");
                            }
                            if (fileInf != null)
                            {
                                fileInf.Delete();
                                Logger.Logger.Log("DeleteFile: ", m_sFTPServerIP + file, "UploadedPics");
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        Logger.Logger.Log("Exception: ", "Timeout On file " + file + " on ftp " + m_sFTPServerIP + ex.Message, "UploadedPics", "FTP Connection timeout on server " + m_sFTPServerIP + " " + ex.Message);
                    }
                    finally
                    {
                        if (strm != null)
                        {
                            strm.Close();
                            Logger.Logger.Log("Stream Closed: ", file, "UploadedPics");
                        }
                        if (fs != null)
                        {
                            fs.Close();
                            Logger.Logger.Log("File Stream Closed: ", file, "UploadedPics");
                        }

                    }




                    //m_nNumberOfRuningUploads--;
                }

                RemoveUploadGroup();
                

            }
           // RemoveUploadGroup();
            
        }

        private bool IsGroupUploading()
        {
            bool retVal = false;
            try
            {
                if (m_locker.TryEnterReadLock(200))
                {
                    if (m_currentlyUploadedGroups.Contains(m_sFTPServerIP))
                    {
                        retVal = true;
                    }
                }
            }
            finally
            {
                m_locker.ExitReadLock();
            }
            return retVal;
        }

        private void AddUploadGroup()
        {
            try
            {
                if (m_locker.TryEnterWriteLock(200))
                {
                    if (m_currentlyUploadedGroups == null)
                    {
                        m_currentlyUploadedGroups = new List<string>();
                    }
                    if (!m_currentlyUploadedGroups.Contains(m_sFTPServerIP))
                    {
                        m_currentlyUploadedGroups.Add(m_sFTPServerIP);
                    }
                }
            }
            finally
            {
                m_locker.ExitWriteLock();
            }
           
        }

        private void RemoveUploadGroup()
        {
            try
            {
                if (m_locker.TryEnterWriteLock(200))
                {
                    if (m_currentlyUploadedGroups != null && m_currentlyUploadedGroups.Contains(m_sFTPServerIP))
                    {
                        m_currentlyUploadedGroups.Remove(m_sFTPServerIP);
                    }
                }
            }
            finally
            {
                m_locker.ExitWriteLock();
            }
        }

        public string Download()
        {
            if (m_sFTPServerIP.Trim() == "")
                return string.Empty;

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(m_sFileToUpload);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(m_sFTPUserName, m_sFTPPass);

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);

            return reader.ReadToEnd();
        }       
    }
}