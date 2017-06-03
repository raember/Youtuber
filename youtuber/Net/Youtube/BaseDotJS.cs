using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using youtuber.net;

namespace youtuber.Net.Youtube
{
    public class BaseDotJs : InternetSite
    {
        private static readonly Hashtable Files = new Hashtable(new Dictionary<string, BaseDotJs>());

        public string PlayerVersion {get;}

        private BaseDotJs(string playerVersion) : base(
            new Uri($"http://s.ytimg.com{playerVersion}")){
            PlayerVersion = playerVersion;
        }

        private BaseDotJs(string playerVersion, CookieCollection cookies) : base(
            new Uri($"http://s.ytimg.com{playerVersion}"), cookies) {
            PlayerVersion = playerVersion;
        }

        private BaseDotJs(string playerVersion, string fileContent) : base(new Uri("http://dummyaddress.com/")){
            content = fileContent;
            PlayerVersion = playerVersion;
        }

        public static async Task<BaseDotJs> GetBaseDotJs(string playerVersion, CookieCollection cookies){
            if (Files.ContainsKey(playerVersion)) return Files[playerVersion] as BaseDotJs;
            BaseDotJs file = new BaseDotJs(playerVersion, cookies);
            Files[playerVersion] = file;
            await file.Load();
            return file;
        }

        public static async Task<BaseDotJs> GetBaseDotJs(string playerVersion) {
            if (Files.ContainsKey(playerVersion))
                return Files[playerVersion] as BaseDotJs;
            BaseDotJs file = new BaseDotJs(playerVersion);
            Files[playerVersion] = file;
            await file.Load();
            return file;
        }

        internal static async Task<BaseDotJs> GetBaseDotJs(string playerVersion, string fileContent){
            return new BaseDotJs(playerVersion, fileContent);
        }

        public string ExtractApiKey(){
            return Regex.Match(content, @"(?<=\(b,""key"","")[^""]+?(?=""\);)").Value;
        }
    }
}