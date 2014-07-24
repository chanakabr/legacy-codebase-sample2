using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDNTokenizers.Tokenizers.AkamaiTokenizers
{
    public abstract class AbstractAkamaiTokenizer : BaseCDNTokenizer
    {
        public AbstractAkamaiTokenizer(int nGroupID, int nStreamingCompanyID)
            :base(nGroupID, nStreamingCompanyID)
        {
        }


        protected enum Flags
        {
            IP,
            Path,
            Profile,
            Passwd,
            Window,
            Payload,
            Duration
        };

        private long myCurTime = Utils.GetEpochUTCTimeNow();

        private static string myVersion = "3.0.1";

        private static int rotBase = 9;

        protected string myToken;
        protected char m_sType;
        protected System.Collections.BitArray m_lFlags = new System.Collections.BitArray(7, false);

        protected string m_sToRemoveFromUrl;
        protected string m_sAifp;

        protected string m_sPath;
        protected string m_sProfile;
        protected string m_sIP;
        protected string m_sSecretCode;
        protected long m_nTime;
        protected long m_nWindow;
        protected long m_nDuration;
        protected string m_sPayload;



        public string String
        {
            get
            {
                return myToken;
            }
        }

        public static string Version
        {
            get
            {
                return myVersion;
            }
        }

        protected bool IsSet(Flags flag)
        {
            return m_lFlags[(int)flag];
        }

        protected void SetFlags()
        {
            if (m_sIP != "" && m_sIP != null)
            {
                m_lFlags[(int)Flags.IP] = true;
            }
            if (m_sPath != "" && m_sPath != null)
            {
                m_lFlags[(int)Flags.Path] = true;
            }
            if (m_sProfile != "" && m_sProfile != null)
            {
                m_lFlags[(int)Flags.Profile] = true;
            }
            if (m_sSecretCode != "" && m_sSecretCode != null)
            {
                m_lFlags[(int)Flags.Passwd] = true;
            }
            if (m_nWindow > 0)
            {
                m_lFlags[(int)Flags.Window] = true;
            }
            if (m_sPayload != "" && m_sPayload != null)
            {
                m_lFlags[(int)Flags.Payload] = true;
            }
            if (m_nDuration > 0)
            {
                m_lFlags[(int)Flags.Duration] = true;
            }
        }

        protected void CreateExpiryData(StringBuilder refSB)
        {
            if (m_nTime <= 0)
            {
                refSB.Append(AkamaiBase64.LongToAB64(myCurTime));
            }
            else
            {
                refSB.Append(AkamaiBase64.LongToAB64(m_nTime));
            }
            refSB.Append("-");
            refSB.Append(AkamaiBase64.LongToAB64(m_nWindow));
        }

        protected void MakeTrailer(StringBuilder refToken)
        {
            StringBuilder trailer = new StringBuilder();

            if (IsSet(Flags.Profile))
            {
                trailer.Append("-");
                trailer.Append(m_sProfile);
            }

            if (IsSet(Flags.Payload))
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] buffer = encoding.GetBytes(m_sPayload);

                trailer.Append("-");
                trailer.Append(AkamaiBase64.BytesToAB64(buffer));
            }

            Obfuscate(refToken, trailer, 3, 32);

            refToken.Append(trailer.ToString());
        }

        protected void Obfuscate(StringBuilder refToken, StringBuilder refTrailer, int idxStart, int lenDigest)
        {
            int[] digits = new int[lenDigest];
            int rotValue;
            int cur;

            char c;
            for (int i = 0; i < lenDigest; i++)
            {
                c = refToken[idxStart + i];
                digits[i] = (int)(AkamaiBase64.AB64ToLong(c.ToString()) % 10);
            }

            for (int i = 0; i < refTrailer.Length; i++)
            {
                rotValue = rotBase + digits[i % lenDigest];
                cur = Convert.ToInt32(refTrailer[i]);

                if (cur >= 97 && cur <= 122)
                {
                    // 'a' to 'z'
                    cur += rotValue;
                    if (cur > 122)
                    {
                        // Overflowed 'z', adjust into 'A' to 'Z'
                        cur -= 58;
                    }
                }
                else if (cur >= 65 && cur <= 90)
                {
                    // 'A' to 'Z'
                    cur += rotValue;
                    if (cur > 90)
                    {
                        // Overflowed 'Z', adjust into '0' to '9'
                        cur -= 43;
                        if (cur > 57)
                        {
                            // Overflowed '9', adjust into 'a' to 'z'
                            cur += 39;
                        }
                    }
                }
                else if (cur >= 48 && cur <= 57)
                {
                    // '0' to '9'
                    cur += rotValue;
                    if (cur > 57)
                    {
                        // Overflowed '9', adjust into 'a' to 'z'
                        cur += 39;
                    }
                }
                refTrailer[i] = Convert.ToChar(cur);
            }
        }

        protected string Encapsulate(string inValue)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(m_sType);
            sb.Append(AkamaiBase64.BitArrayToAB64(m_lFlags));
            sb.Append(inValue);

            if (IsSet(Flags.Window))
            {
                sb.Append("-");
                CreateExpiryData(sb);
            }

            if (IsSet(Flags.Duration))
            {
                sb.Append("-");
                sb.Append(AkamaiBase64.LongToAB64(m_nDuration));
            }

            MakeTrailer(sb);

            return sb.ToString();
        }

    }
}
