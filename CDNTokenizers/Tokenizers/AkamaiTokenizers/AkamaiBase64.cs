using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDNTokenizers.Tokenizers.AkamaiTokenizers
{
    class AkamaiBase64
    {
        private static char[] map = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h',
                                     'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
                                     'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
                                     'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F',
                                     'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N',
                                     'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V',
                                     'W', 'X', 'Y', 'Z', '0', '1', '2', '3',
                                     '4', '5', '6', '7', '8', '9', '.', '_'};

        public static string LongToAB64(long inValue)
        {
            StringBuilder outValue = new StringBuilder();
            long remainder;

            do
            {
                inValue = Math.DivRem(inValue, 64, out remainder);
                outValue.Insert(0, map[remainder]);
            } while (inValue >= 1);

            return outValue.ToString();
        }

        public static long AB64ToLong(string inValue)
        {
            long outValue = 0;

            foreach (char c in inValue)
            {
                outValue = (outValue * 64) + Array.IndexOf(map, c);
            }

            return outValue;
        }

        public static string BytesToAB64(byte[] inValue)
        {
            StringBuilder outValue = new StringBuilder();
            StringBuilder temp;
            int remainder;

            for (int i = 0; i < inValue.Length; i++)
            {
                temp = new StringBuilder();
                do
                {
                    inValue[i] = (byte)Math.DivRem(inValue[i], 64, out remainder);
                    temp.Insert(0, map[remainder]);
                } while (inValue[i] >= 1);

                while (temp.Length < 2)
                {
                    temp.Insert(0, map[0]);
                }

                outValue.Append(temp.ToString());
            }

            return outValue.ToString();
        }

        public static byte[] AB64ToBytes(string inValue)
        {
            System.Collections.ArrayList al = new System.Collections.ArrayList();

            for (int i = 0; i < inValue.Length; i += 2)
            {
                al.Add((byte)AB64ToLong(inValue.Substring(i, 2)));
            }

            return (byte[])al.ToArray(typeof(Byte));
        }

        public static string BitArrayToAB64(System.Collections.BitArray inBits)
        {
            StringBuilder outValue = new StringBuilder();
            int total = 0;
            int remainder;

            for (int i = 0; i < inBits.Length; i++)
            {
                if (inBits[i])
                {
                    total += (int)Math.Pow(2, i);
                }
            }

            do
            {
                total = Math.DivRem(total, 64, out remainder);
                outValue.Insert(0, map[remainder]);
            } while (total >= 1);

            while (outValue.Length < 2)
            {
                outValue.Insert(0, map[0]);
            }

            return outValue.ToString();
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Write("Wrong number of command line arguments supplied\n\n");
                DisplayHelp();
                Environment.Exit(2);
            }

            if (Array.IndexOf(args, "-h") >= 0)
            {
                DisplayHelp();
                Environment.Exit(0);
            }

            bool isString = false;

            if (Array.IndexOf(args, "-l") >= 0)
            {
                if (Array.IndexOf(args, "-s") >= 0)
                {
                    Console.Write("'-l' and '-s' cannot be used simultaneously\n\n");
                    DisplayHelp();
                    Environment.Exit(2);
                }
                isString = false;
            }
            else if (Array.IndexOf(args, "-s") >= 0)
            {
                isString = true;
            }
            else
            {
                Console.Write("Either '-l' or '-s' must be used\n\n");
                DisplayHelp();
                Environment.Exit(2);
            }

            try
            {
                Console.Write("Input: ");
                string input = Console.ReadLine();

                if (isString)
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] temp = encoding.GetBytes(input);
                    string output = AkamaiBase64.BytesToAB64(temp);

                    Console.WriteLine("Output: " + output);
                }
                else
                {
                    long temp = Convert.ToInt64(input);
                    string output = AkamaiBase64.LongToAB64(temp);

                    Console.WriteLine("Output: " + output);
                }

            }
            catch (FormatException)
            {
                Console.Write("Input cannot be converted to a 64-bit integer\n\n");
                DisplayHelp();
                Environment.Exit(2);
            }
            catch (Exception e)
            {
                Console.Write("Unexpected error creating Akamai Base64 string\n\n" + e.ToString() + "\n");
                DisplayHelp();
                Environment.Exit(2);
            }


        }

        private static void DisplayHelp()
        {
            System.Collections.SortedList options = new System.Collections.SortedList();
            options.Add("-h", "display this help message");
            options.Add("-l", "treat input as a 64-bit integer");
            options.Add("-s", "treat input as a string");

            Console.WriteLine("Usage: AkamaiBase64 [options]");
            for (int i = 0; i < options.Count; i++)
            {
                Console.WriteLine(String.Format("\t{0,-4}\t{1}", options.GetKey(i), options.GetByIndex(i)));
            }
        }
    }
}
