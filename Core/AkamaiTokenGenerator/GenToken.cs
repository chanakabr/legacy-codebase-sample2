/**
 * Package:     Secure Streaming Token Generator for C#
 * Title:       GenToken
 * Description: A class providing a command-line interface for
 *              generating Secure Streaming Tokens.
 * Copyright:   Copyright (c) Akamai Technologies, Inc. 2005
 * Company:     Akamai Technologies, Inc.
 */

using System;
using System.Collections;
using System.IO;

namespace Akamai.Authentication.SecureStreaming {
    
    class GenToken {
        private static long   inDuration, inTime, inWindow;
        private static string inIP, inKey, inPasswd, inPath, inPayload,
                              inProfile;
        private static char   inType;

        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.Write("Not enough command line arguments supplied\n\n");
                DisplayHelp();
                Environment.Exit(2);
            }

            try {
                for (int i = 0; i < args.Length; i++) {
                    switch(args[i]) {
                        case "-h":
                            DisplayHelp();
                            Environment.Exit(0);
                            break;
                        case "-v":
                            DisplayVersion();
                            Environment.Exit(0);
                            break;
                        case "-d":
                            inDuration = Convert.ToInt64(args[++i]);
                            break;
                        case "-f":
                            inPath = args[++i];
                            break;
                        case "-i":
                            inIP = args[++i];
                            break;
                        case "-k":
                            string fileName = args[++i];
                            string fileContents = "";

                            try {
                                StreamReader myFile = new StreamReader(fileName);
                                fileContents = myFile.ReadToEnd();
                                myFile.Close();
                            } catch (Exception) {
                                Console.WriteLine("Error reading file: " + fileName);
                                Environment.Exit(2);
                            }

                            if (fileContents.Length < 64) {
                                Console.WriteLine("Error: file should be at least 64 bytes long");
                                Environment.Exit(2);
                            }

                            inKey = fileContents.Substring(0, 64);
                                
                            break;
                        case "-p":
                            inPasswd = args[++i];
                            break;
                        case "-r":
                            inProfile = args[++i];
                            break;
                        case "-t":
                            inTime = Convert.ToInt64(args[++i]);
                            break;
                        case "-w":
                            inWindow = Convert.ToInt64(args[++i]);
                            break;
                        case "-x":
                            inPayload = args[++i];
                            break;
                        case "-y":
                            inType = Convert.ToChar(args[++i]);
                            break;
                        default:
                            Console.Write("Unrecognized command-line argument: " + args[i] + "\n\n");
                            DisplayHelp();
                            Environment.Exit(2);
                            break;
                    }
                }
            } catch (Exception) {
                // This occurs if either the user did not provide data for an
                // argument that requires it or the data provided could not be
                // parsed correctly.
                Console.Write("Invalid command line data\n\n");
                DisplayHelp();
                Environment.Exit(2);
            }

            PrintToken();
        }

        private static void PrintToken() {
            StreamToken token = null;
            
            try {
                switch (inType) {
                    case 'c':
                        token = new TypeCToken(inPath, inIP, inProfile, inPasswd,
                                               inTime, inWindow, inDuration,
                                               inPayload);
                        break;
                    case 'd':
                        token = new TypeDToken(inPath, inIP, inProfile, inPasswd,
                                               inTime, inWindow, inDuration,
                                               inPayload);
                        break;
                    case 'e':
                        token = new TypeEToken(inPath, inIP, inProfile, inPasswd,
                                               inTime, inWindow, inDuration,
                                               inPayload, inKey);
                        break;
                    default:
                        Console.Write("Invalid or missing token type\n\n");
                        DisplayHelp();
                        Environment.Exit(2);
                        break;
                }

                Console.WriteLine("Token: " + token.String);
                
            } catch (Exception e) {
                Console.Write("Unexpected error generating token:\n\n" + e.ToString() + "\n");
                Environment.Exit(2);
            }
            
        }

        private static void DisplayVersion() {
            Console.WriteLine("Akamai Secure Streaming, C# token generator version " + StreamToken.Version);
        }

        private static void DisplayHelp() {
            SortedList options = new SortedList();
            options.Add("-h", "display this help message");
            options.Add("-v", "display program version");
            options.Add("-d duration", "rendering duration (valid for C-, D-, and E-type tokens only)");
            options.Add("-f path", "path being requested");
            options.Add("-i ip", "IP address of requester");
            options.Add("-k key", "key filename and path (for E-type tokens; a 32-byte binary file)");
            options.Add("-p passwd", "password");
            options.Add("-r profile", "authentication profile");
            options.Add("-t time", "time of token creation");
            options.Add("-w window", "time window in seconds");
            options.Add("-x payload", "extra payload");
            options.Add("-y type", "token type: 'c', 'd', or 'e'");
            
            DisplayVersion();

            Console.WriteLine("Usage: GenToken [options]");
            for (int i = 0; i < options.Count; i++) {
                Console.WriteLine(String.Format("\t{0,-12}\t{1}", options.GetKey(i), options.GetByIndex(i)));
            }
        }

    }

}
