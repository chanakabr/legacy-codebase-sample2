using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;


namespace com.llnw.mediavault
{
    public class MediaVault
    {
        #region Variables
        private string _videoURL = String.Empty;
        public string VideoURL{
            get{ return _videoURL; }
            set { _videoURL = System.Web.HttpUtility.UrlDecode(value); }
        }

        private MediaVaultOptions _options = new MediaVaultOptions();
        public MediaVaultOptions Options
        {
            get { return _options; }
            set { _options = value; }
        }

        private string _secret;
        public string Secret
        {
            get { return _secret; }
            set { _secret = value; }
        }

        MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
        #endregion

        #region Constructors
        public MediaVault(){}

        public MediaVault(string _videoURL)
        {
            this._videoURL = _videoURL;
        }

        public MediaVault(string _videoURL, string _secret) : this(_videoURL)
        {
            this._secret = _secret;
        }

        public MediaVault(string _videoURL, string _secret, MediaVaultOptions _options) : this(_videoURL, _secret){
            this._options = _options;
        }
        #endregion

        #region "API"
        public string compute()
        {
            if (_videoURL.Equals(String.Empty) || _secret.Equals(String.Empty))
            {
                throw new Exception("VideoURL and Secret are required.");
            }

            string result = _videoURL;
            string urlParams = String.Empty;
            string hash = String.Empty;

            if (!_options.Referrer.Equals(String.Empty))
            {
                Uri u = new Uri(_options.Referrer);
                urlParams += "&ru=" + (u.Scheme + "://" + u.Host).Length.ToString();
                hash = _options.Referrer;
            }
            if (!_options.PageURL.Equals(String.Empty))
            {
                urlParams += "&pu=" + _options.PageURL.Length.ToString();
                hash += _options.PageURL;
            }
            
            if (_options.StartTime != null) urlParams += String.Format("&s={0}", _options.StartTime);
            if (_options.EndTime != null) urlParams += String.Format("&e={0}", _options.EndTime);
            if (!_options.IPAddress.Equals(String.Empty)) urlParams += String.Format("&ip={0}", _options.IPAddress);
            if(!urlParams.Equals(String.Empty)){
                urlParams = urlParams.Remove(0, 1);
                if (result.Contains("?"))
                {
                    result += "&" + urlParams;
                }
                else
                {
                    result += "?" + urlParams;
                }
            }

            hash = GetMD5Hash(_secret + hash + result);

            result += (result.Contains("?") ? "&h=" + hash : "?h=" + hash);

            return result;
        }

        private string GetMD5Hash(string hash)
        {
            md5Provider = new MD5CryptoServiceProvider();
            byte[] originalBytes = ASCIIEncoding.Default.GetBytes(hash);
            byte[] encodedBytes = md5Provider.ComputeHash(originalBytes);
            return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
        }
        #endregion

        static public string GetHashedURL(string sSecretCode , string sURL , string sIP , string sRefferer)
        {
            Uri u = new Uri(sURL);
            sURL = u.PathAndQuery;
            int unixNow = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds - 3600;

            MediaVault mv = new MediaVault();
            mv.VideoURL = sURL;
            mv.Secret = sSecretCode;

            MediaVaultOptions options = new MediaVaultOptions();
            options.StartTime = unixNow;
            options.EndTime = unixNow + 7200; //5 minutes from now
            options.IPAddress = sIP;
            //options.Referrer = sRefferer;
            mv.Options = options;
            return u.Scheme + "://" + u.Host + mv.compute();
        }
    }
}