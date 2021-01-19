using System.Text;

namespace Core.Users
{
    public class MD5Encrypter : BaseEncrypter
    {
        public override string Encrypt(string sPass, string key/*not used*/)
        {
            // step 1, calculate MD5 hash from input
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(sPass);
                byte[] hash = md5.ComputeHash(inputBytes);

                // step 2, convert byte array to hex string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}
