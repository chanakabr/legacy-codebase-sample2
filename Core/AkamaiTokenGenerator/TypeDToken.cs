/**
 * Package:     Secure Streaming Token Generator for C#
 * Title:       TypeDToken
 * Description: Implements the specific functionality required to
 *              generate D-type Secure Streaming tokens.
 * Copyright:   Copyright (c) Akamai Technologies, Inc. 2005
 * Company:     Akamai Technologies, Inc.
 */

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Akamai.Authentication.SecureStreaming {

    public class TypeDToken : StreamToken {

        public TypeDToken(string inPath, string inIP, string inProfile,
                          string inPasswd, long inTime, long inWindow,
                          long inDuration, string inPayload) {
            
            myType = 'd';

            myPath     = inPath;
            myIP       = inIP;
            myProfile  = inProfile;
            myPasswd   = inPasswd;
            myTime     = inTime;
            myWindow   = inWindow;
            myDuration = inDuration;
            myPayload  = inPayload;

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

            string tokenCore = AkamaiBase64.BytesToAB64(digest2);

            myToken = Encapsulate(tokenCore);
        }

    }

}
