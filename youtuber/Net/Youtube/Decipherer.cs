using System;
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
                                                                    var sb = new StringBuilder(cipher);
                                                                    var c = cipher[0];
                                                                    sb[0] = cipher[index % cipher.Length];
                                                                    sb[index % cipher.Length] = c;
                                                                    return sb.ToString();
                                                                };

        public static readonly Func<string, int, string> Reverse =
            (cipher, index) => { return new string(cipher.ToCharArray().Reverse().ToArray()); };

        public static readonly Func<string, int, string> Splice =
            (cipher, index) => { return cipher.Substring(Math.Min(index, cipher.Length)); };

        private static readonly Hashtable Decipherers = new Hashtable(new Dictionary<string, Decipherer>());

        private readonly List<Tuple<Func<string, int, string>, int>> operations =
            new List<Tuple<Func<string, int, string>, int>>();

        private readonly string fileContent;
        private readonly Hashtable functionMapping = new Hashtable(new Dictionary<string, Func<string, int, string>>());

        private Decipherer(string playerVersion, string fileContent){
            this.fileContent = fileContent;
            Uri = new Uri($"http://s.ytimg.com/yts/jsbin/player-{playerVersion}/en_US/base.js");
        }

        public Uri Uri {get;}

        private async Task Setup(){
            var funcName = Regex.Match(fileContent, @"(?<=""signature"",)[\w\d\$]+?(?=\()").Value;
            funcName = Regex.Escape(funcName);
            var funcBody = Regex.Match(fileContent,
                @"(?<=;[\s\r\n]*?" + funcName + @"\=function\(\w+\)\{).*?(?=\};)").Value;
            var family = string.Empty;
            var functionPool = string.Empty;
            var funcPoolParsed = false;
            foreach (var line in funcBody.Split(';')) {
                var match = Regex.Match(line, @"(?<family>\w+?)\.(?<name>\w+?)\(\w+?,(?<index>\d+?)\)");
                if (!match.Success) continue;
                var parsedFamily = match.Groups["family"].Value;
                if (string.IsNullOrEmpty(family)) family = parsedFamily;
                if (!family.Equals(parsedFamily)) throw new Exception("The function changed unexpectedly");
                var function = match.Groups["name"].Value;
                var index = int.Parse(match.Groups["index"].Value);
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
                var operation = (Func<string, int, string>) functionMapping[function];
                operations.Add(new Tuple<Func<string, int, string>, int>(operation, index));
            }
        }

        public string Decipher(string signature){
            var deciphered = new string(signature.ToCharArray());
            foreach (var functionTuple in operations) {
                var function = functionTuple.Item1;
                var index = functionTuple.Item2;
                deciphered = function.Invoke(deciphered, index);
            }
            return deciphered;
        }

        public static async Task<Decipherer> GetDecipherer(string playerVersion, string fileContent){
            if (Decipherers.ContainsKey(playerVersion)) return (Decipherer) Decipherers[playerVersion];
            var decipherer = new Decipherer(playerVersion, fileContent);
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