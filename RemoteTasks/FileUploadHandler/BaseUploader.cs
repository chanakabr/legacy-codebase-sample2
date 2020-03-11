
namespace FileUploadHandler
{
    public abstract class BaseUploader
    {
        protected string m_sBasePath;
        protected string m_sAddress;
        protected string m_sUserName;
        protected string m_sPass;
        protected string m_sPrefix;

        public abstract void Upload(string file, string fileName);
    }
}
