using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Youtuber.Net;

namespace Youtuber.Net.Youtube.Official
{
    public abstract class ApiBase : InternetSite
    {
        public string ApiKey {get;}

        protected ApiBase(string apiKey) : base(new Uri("http://dummy.com/")){
            ApiKey = apiKey;
        }

        protected ApiBase(string apiKey, CookieCollection cookies) : base(new Uri("http://dummy.com/"), cookies){
            ApiKey = apiKey;
        }

        protected async Task<dynamic> GetResponse(Uri uri){
            Uri = uri;
            await Load();
            return JsonConvert.DeserializeObject(content);
        }

        protected override void SetCookies() {
            request.Accept = "*/*";
        }
    }
}