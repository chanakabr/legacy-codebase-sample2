/**
 * Package:     Secure Streaming Token Generator for C#
 * Title:       TypeCToken
 * Description: Implements the specific functionality required to
 *              generate C-type Secure Streaming tokens.
 * Copyright:   Copyright (c) Akamai Technologies, Inc. 2005
 * Company:     Akamai Technologies, Inc.
 */

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Akamai.Authentication.SecureStreaming {

    class TypeCToken : StreamToken {

        public TypeCToken(string inPath, string inIP, string inProfile,
                          string inPasswd, long inTime, long inWindow,
                          long inDuration, string inPayload) {
            
            myType = 'c';

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

            byte[] digest = md5.ComputeHash(encoding.GetBytes(sb.ToString()));

            string tokenCore = AkamaiBase64.BytesToAB64(digest);

            myToken = Encapsulate(tokenCore);
        }

    }

}
