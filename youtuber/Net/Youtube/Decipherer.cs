﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace youtuber.Net.Youtube
{
    public class Decipherer
    {
        public static WebClient webClient = new WebClient();

        public static readonly Func<string, int, string> Swap = (cipher, index) => {
                                                                    StringBuilder sb = new StringBuilder(cipher);
                                                                    char c = cipher[0];
                                                                    sb[0] = cipher[index % cipher.Length];
                                                                    sb[index % cipher.Length] = c;
                                                                    return sb.ToString();
                                                                };

        public static readonly Func<string, int, string> Reverse =
            (cipher, index) => { return new string(cipher.ToCharArray().Reverse().ToArray()); };

        public static readonly Func<string, int, string> Splice =
            (cipher, index) => { return cipher.Substring(Math.Min(index, cipher.Length)); };

        private static readonly Hashtable Decipherers = new Hashtable(new Dictionary<string, Decipherer>());

        private readonly string fileContent;
        private readonly Hashtable functionMapping = new Hashtable(new Dictionary<string, Func<string, int, string>>());

        private readonly List<Tuple<Func<string, int, string>, int>> operations =
            new List<Tuple<Func<string, int, string>, int>>();

        private Decipherer(string playerVersion, string fileContent){
            this.fileContent = fileContent;
            Uri = new Uri($"http://s.ytimg.com/yts/jsbin/player-{playerVersion}/en_US/base.js");
        }

        public Uri Uri {get;}

        private async Task Setup(){
            string funcName = Regex.Match(fileContent, @"(?<=""signature"",)[\w\d\$]+?(?=\()").Value;
            funcName = Regex.Escape(funcName);
            string funcBody = Regex.Match(fileContent,
                @"(?<=;[\s\r\n]*?" + funcName + @"\=function\(\w+\)\{).*?(?=\};)").Value;
            string family = string.Empty;
            string functionPool = string.Empty;
            bool funcPoolParsed = false;
            foreach (string line in funcBody.Split(';')) {
                Match match = Regex.Match(line, @"(?<family>\w+?)\.(?<name>\w+?)\(\w+?,(?<index>\d+?)\)");
                if (!match.Success) continue;
                string parsedFamily = match.Groups["family"].Value;
                if (string.IsNullOrEmpty(family)) family = parsedFamily;
                if (!family.Equals(parsedFamily)) throw new Exception("The function changed unexpectedly");
                string function = match.Groups["name"].Value;
                int index = int.Parse(match.Groups["index"].Value);
                if (!functionMapping.ContainsKey(function)) {
                    if (!funcPoolParsed) {
                        functionPool = Regex.Match(fileContent, @"(?<=\W" + family + @"\=\{).*?(?=\};)",
                            RegexOptions.Singleline).Value;
                        funcPoolParsed = true;
                    }
                    if (functionPool.Contains(function + ":function(a,b){var c=a[0]")
                    ) functionMapping.Add(function, Swap);
                    else if (
                        functionPool.Contains(function + ":function(a,b){a.splice(0,b)"))
                        functionMapping.Add(function, Splice);
                    else if (functionPool.Contains(function + ":function(a){a.reverse()"))
                        functionMapping.Add(function, Reverse);
                    else throw new Exception($"No function found for {function}");
                }
                Func<string, int, string> operation = (Func<string, int, string>) functionMapping[function];
                operations.Add(new Tuple<Func<string, int, string>, int>(operation, index));
            }
        }

        public string Decipher(string signature){
            string deciphered = new string(signature.ToCharArray());
            foreach (Tuple<Func<string, int, string>, int> functionTuple in operations) {
                Func<string, int, string> function = functionTuple.Item1;
                int index = functionTuple.Item2;
                deciphered = function.Invoke(deciphered, index);
            }
            return deciphered;
        }

        public static async Task<Decipherer> GetDecipherer(string playerVersion, string fileContent){
            if (Decipherers.ContainsKey(playerVersion)) return (Decipherer) Decipherers[playerVersion];
            Decipherer decipherer = new Decipherer(playerVersion, fileContent);
            await decipherer.Setup();
            Decipherers.Add(playerVersion, decipherer);
            return decipherer;
        }

        public static async Task<Decipherer> GetDecipherer(string playerVersion){
            return await GetDecipherer(playerVersion,
                await webClient.DownloadStringTaskAsync(
                    $"http://s.ytimg.com/yts/jsbin/player-{playerVersion}/en_US/base.js"));
        }
    }
}