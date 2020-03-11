/**
 * Package:     Secure Streaming Token Generator for C#
 * Title:       AkamaiBase64
 * Description: Implements the Akamai variant of the Base64 encoding,
 *              as well as a command line tool to generate AkamaiBase64
 *              strings.
 * Copyright:   Copyright (c) Akamai Technologies, Inc. 2005
 * Company:     Akamai Technologies, Inc.
 */

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Akamai.Authentication.SecureStreaming {

    class KeyEncoder {
        static void Main(string[] args) {
            if (args.Length < 1) {
                Console.Write("Wrong number of command line arguments supplied\n\n");
                DisplayHelp();
                Environment.Exit(2);
            }
            
            if (Array.IndexOf(args, "-h") >= 0) {
                DisplayHelp();
                Environment.Exit(0);
            }
            
            if (args.Length != 3) {
                Console.Write("Wrong number of command line arguments supplied\n\n");
                DisplayHelp();
                Environment.Exit(2);
            }

            bool isEncode = false;

            if (Array.IndexOf(args, "-d") >= 0) {
                if (Array.IndexOf(args, "-e") >= 0) {
                    Console.Write("'-d' and '-e' cannot be used simultaneously\n\n");
                    DisplayHelp();
                    Environment.Exit(2);
                }
                isEncode = false;
            } else if (Array.IndexOf(args, "-e") >= 0) {
                isEncode = true;
            } else {
                Console.Write("Either '-d' or '-e' must be used\n\n");
                DisplayHelp();
                Environment.Exit(2);
            }

            try {
                if (isEncode) {
                    BinaryReader inFile = new BinaryReader(File.Open(args[1], FileMode.Open));
                    byte[] input = inFile.ReadBytes(33);
                    inFile.Close();
                    
                    if (input.Length != 32) {
                        throw new Exception("Binary key not 32 bytes");
                    }
                    
                    string output = AkamaiBase64.BytesToAB64(input);
                    
                    StreamWriter outFile = new StreamWriter(File.Open(args[2], FileMode.Create));
                    outFile.Write(output);
                    outFile.Close();
                } else {
                    StreamReader inFile = new StreamReader(File.Open(args[1], FileMode.Open));
                    String input = inFile.ReadToEnd();
                    inFile.Close();
                    
                    if (input.Length != 64) {
                        throw new Exception("Encoded key not 64 bytes");
                    }
                    
                    byte[] output = AkamaiBase64.AB64ToBytes(input);
                    
                    BinaryWriter outFile = new BinaryWriter(File.Open(args[2], FileMode.Create));
                    outFile.Write(output);
                    outFile.Close();
                }

            } catch (Exception e) {
                Console.Write("Unexpected error encoding/decoding\n\n" + e.ToString() + "\n");
                DisplayHelp();
                Environment.Exit(2);
            }
                
                
        }

        private static void DisplayHelp() {
            SortedList options = new SortedList();
            options.Add("-h", "display this help message");
            options.Add("-d", "decode the provided encoded key");
            options.Add("-e", "encode the provided binary key");

            Console.WriteLine("Usage: KeyEncoder [options] <inputFile> <outputFile>");
            for (int i = 0; i < options.Count; i++) {
                Console.WriteLine(String.Format("\t{0,-4}\t{1}", options.GetKey(i), options.GetByIndex(i)));
            }
        }
    }
}
