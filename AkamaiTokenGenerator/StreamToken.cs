/**
 * Package:     Secure Streaming Token Generator for C#
 * Title:       StreamToken
 * Description: Implements token generation functionality common to
 *              the different token types.
 * Copyright:   Copyright (c) Akamai Technologies, Inc. 2005
 * Company:     Akamai Technologies, Inc.
 */

using System;
using System.Collections;
using System.Text;

namespace Akamai.Authentication.SecureStreaming {

    public abstract class StreamToken {

        protected enum Flags {
            IP,
            Path,
            Profile,
            Passwd,
            Window,
            Payload,
            Duration };

        private long myCurTime = 
            (long) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

        private static string myVersion = "3.0.1";

        private static int rotBase = 9;
        
        protected string   myToken;
        protected char     myType;
        protected BitArray myFlags = new BitArray(7, false);

        protected string myPath;
        protected string myIP;
        protected string myProfile;
        protected string myPasswd;
        protected long   myTime;
        protected long   myWindow;
        protected long   myDuration;
        protected string myPayload;

        public string String {
            get {
                return myToken;
            }
        }

        public static string Version {
            get {
                return myVersion;
            }
        }

        protected bool IsSet(Flags flag) {
            return myFlags[(int) flag];
        }

        protected void SetFlags() {
            if (myIP != "" && myIP != null) {
                myFlags[(int) Flags.IP] = true;
            }
            if (myPath != "" && myPath != null) {
                myFlags[(int) Flags.Path] = true;
            }
            if (myProfile != "" && myProfile != null) {
                myFlags[(int) Flags.Profile] = true;
            }
            if (myPasswd != "" && myPasswd != null) {
                myFlags[(int) Flags.Passwd] = true;
            }
            if (myWindow > 0) {
                myFlags[(int) Flags.Window] = true;
            }
            if (myPayload != "" && myPayload != null) {
                myFlags[(int) Flags.Payload] = true;
            }
            if (myDuration > 0) {
                myFlags[(int) Flags.Duration] = true;
            }
        }

        protected void CreateExpiryData(StringBuilder refSB) {
            if (myTime <= 0) {
                refSB.Append(AkamaiBase64.LongToAB64(myCurTime));
            } else {
                refSB.Append(AkamaiBase64.LongToAB64(myTime));
            }
            refSB.Append("-");
            refSB.Append(AkamaiBase64.LongToAB64(myWindow));
        }

        protected void MakeTrailer(StringBuilder refToken) {
            StringBuilder trailer = new StringBuilder();
            
            if (IsSet(Flags.Profile)) {
                trailer.Append("-");
                trailer.Append(myProfile);
            }

            if (IsSet(Flags.Payload)) {
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] buffer = encoding.GetBytes(myPayload);
                
                trailer.Append("-");
                trailer.Append(AkamaiBase64.BytesToAB64(buffer));
            }

            Obfuscate(refToken, trailer, 3, 32);

            refToken.Append(trailer.ToString());
        }

        protected void Obfuscate(StringBuilder refToken, StringBuilder refTrailer, int idxStart, int lenDigest) {
            int[] digits = new int[lenDigest];
            int rotValue;
            int cur;
            
            char c;
            for (int i = 0; i < lenDigest; i++) {
                c = refToken[idxStart + i];
                digits[i] = (int) (AkamaiBase64.AB64ToLong(c.ToString()) % 10);
            }

            for (int i = 0; i < refTrailer.Length; i++) {
                rotValue = rotBase + digits[i % lenDigest];
                cur = Convert.ToInt32(refTrailer[i]);
                
                if (cur >= 97 && cur <= 122) {
                    // 'a' to 'z'
                    cur += rotValue;
                    if (cur > 122) {
                        // Overflowed 'z', adjust into 'A' to 'Z'
                        cur -= 58;
                    }
                } else if (cur >= 65 && cur <= 90) {
                    // 'A' to 'Z'
                    cur += rotValue;
                    if (cur > 90) {
                        // Overflowed 'Z', adjust into '0' to '9'
                        cur -= 43;
                        if (cur > 57) {
                            // Overflowed '9', adjust into 'a' to 'z'
                            cur += 39;
                        }
                    }
                } else if (cur >= 48 && cur <= 57) {
                    // '0' to '9'
                    cur += rotValue;
                    if (cur > 57) {
                        // Overflowed '9', adjust into 'a' to 'z'
                        cur += 39;
                    }
                }
                refTrailer[i] = Convert.ToChar(cur);
            }
        }

        protected string Encapsulate(string inValue) {
            StringBuilder sb = new StringBuilder();

            sb.Append(myType);
            sb.Append(AkamaiBase64.BitArrayToAB64(myFlags));
            sb.Append(inValue);

            if (IsSet(Flags.Window)) {
                sb.Append("-");
                CreateExpiryData(sb);
            }

            if (IsSet(Flags.Duration)) {
                sb.Append("-");
                sb.Append(AkamaiBase64.LongToAB64(myDuration));
            }

            MakeTrailer(sb);

            return sb.ToString();
        }

    }

}
