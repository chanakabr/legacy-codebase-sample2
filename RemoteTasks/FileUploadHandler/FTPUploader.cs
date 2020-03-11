using System;
using System.Net;
using System.IO;

namespace FileUploadHandler
{
    public class FTPUploader : BaseUploader
    {
        protected Int32 m_nFTPPort;

        public FTPUploader(string sAddress, string sUN, string sPass, string sPrefix)
        {
            m_sAddress = sAddress;

            if (m_sAddress.StartsWith("ftp://") == true)
                m_sAddress = m_sAddress.Substring(6);
            if (m_sAddress.StartsWith("sftp://") == true)
                m_sAddress = m_sAddress.Substring(7);

            m_sUserName = sUN;
            m_sPass = sPass;
            m_sPrefix = sPrefix;

            m_nFTPPort = 21;
        }

        public override void Upload(string file, string fileName)
        {
            string url = string.Empty;

            if (string.IsNullOrEmpty(m_sPrefix))
                url = "ftp://" + m_sAddress + "/" + fileName;
            else
                url = "ftp://" + m_sAddress + "/" + m_sPrefix + "/" + fileName;

            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri(url));

            ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
            ftpWebRequest.Credentials = new NetworkCredential(m_sUserName, m_sPass);
            ftpWebRequest.UsePassive = true;
            ftpWebRequest.KeepAlive = false;
            ftpWebRequest.UseBinary = true;
            ftpWebRequest.Timeout = 240000;

            byte[] fileContents = Convert.FromBase64String(file);

            ftpWebRequest.ContentLength = fileContents.Length;

            using (Stream requestStream = ftpWebRequest.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }
        }
    }
}
