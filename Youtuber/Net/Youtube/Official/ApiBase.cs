using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Youtuber.Net.Youtube.Official {
    public abstract class ApiBase : InternetSite {
        protected ApiBase(string apiKey) : base(new Uri("http://dummy.com/")){
            ApiKey = apiKey;
        }

        protected ApiBase(string apiKey, CookieCollection cookies) : base(new Uri("http://dummy.com/"), cookies){
            ApiKey = apiKey;
        }

        public string ApiKey {get;}

        protected async Task<dynamic> GetResponse(Uri uri){
            Uri = uri;
            await Load();
            return JsonConvert.DeserializeObject(content);
        }

        protected override void SetCookies(){
            request.Accept = "*/*";
        }
    }
}