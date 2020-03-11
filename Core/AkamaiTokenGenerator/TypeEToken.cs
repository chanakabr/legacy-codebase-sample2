/**
 * Package:     Secure Streaming Token Generator for C#
 * Title:       TypeEToken
 * Description: Implements the specific functionality required to
 *              generate E-type Secure Streaming tokens.
 * Copyright:   Copyright (c) Akamai Technologies, Inc. 2005
 * Company:     Akamai Technologies, Inc.
 */

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Akamai.Authentication.SecureStreaming {

    class TypeEToken : StreamToken {

        protected string myKey;

        public TypeEToken(string inPath, string inIP, string inProfile,
                          string inPasswd, long inTime, long inWindow,
                          long inDuration, string inPayload,
                          string inKey) {
            
            myType = 'e';

            myPath     = inPath;
            myIP       = inIP;
            myProfile  = inProfile;
            myPasswd   = inPasswd;
            myTime     = inTime;
            myWindow   = inWindow;
            myDuration = inDuration;
            myPayload  = inPayload;
            myKey      = inKey;

            SetFlags();

            StringBuilder sb = new StringBuilder();

            sb.Append(inPath);
            sb.Append(inIP);

            if (IsSet(Flags.Window)) {
                CreateExpiryData(sb);
            }
            
            sb.Append(myProfile);
            sb.Append(myPasswd);
            sb.Append(myPayload);

            if (IsSet(Flags.Duration)) {
                sb.Append(AkamaiBase64.LongToAB64(myDuration));
            }

            MD5 md5 = new MD5CryptoServiceProvider();

            ASCIIEncoding encoding = new ASCIIEncoding();

            byte[] digest1 = md5.ComputeHash(encoding.GetBytes(sb.ToString()));

            byte[] passBytes = {};
            if (IsSet(Flags.Passwd)) {
                passBytes = encoding.GetBytes(inPasswd);
            }
            
            byte[] buffer = new byte[digest1.Length + passBytes.Length];
            Array.Copy(digest1, 0, buffer, 0, digest1.Length);
            Array.Copy(passBytes, 0, buffer, digest1.Length, passBytes.Length);
            byte[] digest2 = md5.ComputeHash(buffer);

            string tokenCore;
            if (myKey != "" && myKey != null) {
                byte[] key = AkamaiBase64.AB64ToBytes(myKey);

                byte[] rdlBytes = new byte[32];
                
                Rijndael rdl = new RijndaelManaged();
                rdl.Mode = CipherMode.CBC;
                rdl.Padding = PaddingMode.None;
                rdl.Key = key;
                rdl.GenerateIV();

                Array.Copy(rdl.IV, 0, rdlBytes, 0, rdl.IV.Length);

                ICryptoTransform xfrm = rdl.CreateEncryptor();

                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, xfrm, CryptoStreamMode.Write);

                cs.Write(digest2, 0, digest2.Length);
                cs.FlushFinalBlock();
                
                byte[] encBytes = ms.ToArray();

                ms.Close();
                cs.Close();

                Array.Copy(encBytes, 0, rdlBytes, rdl.IV.Length, encBytes.Length);
                tokenCore = AkamaiBase64.BytesToAB64(rdlBytes);
            } else {
                tokenCore = AkamaiBase64.BytesToAB64(digest2);
            }

            myToken = Encapsulate(tokenCore);
        }

    }

}
