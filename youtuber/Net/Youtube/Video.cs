using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using youtuber.Net.Youtube;

namespace youtuber.net
{
    public class Video : InternetSite
    {
        private Video(Uri uri) : base(uri){
        }

        public string Title
        {
            get
            {
                Match match = Regex.Match(content,
                    @"\<span id\=""eow-title"" class=""watch-title"" dir=""ltr"" title=""(?<title>.+?)""\>");
                return WebUtility.HtmlDecode(match.Groups["title"].Value);
            }
        }

        public string VideoID {get; private set;}

        public DateTime UploadedDateTime
        {
            get
            {
                Match match = Regex.Match(content,
                    @"(?<=meta\sitemprop\=""datePublished"" content\="")[\d-]+?(?="")",
                    RegexOptions.Singleline);
                return DateTime.ParseExact(match.Value, "yyyy-MM-dd", new CultureInfo("en-US"));
            }
        }

        public string Description
        {
            get
            {
                Match match = Regex.Match(content, @"\<p id\=""eow-description"" class="""" \>(?<description>.+?)</p>",
                    RegexOptions.Singleline);
                return WebUtility.HtmlDecode(match.Groups["description"].Value);
            }
        }

        public string Username
        {
            get
            {
                Match match = Regex.Match(content,
                    @"\<div class\=""yt-user-info""\>.+?data-sessionlink\="".+?"" \>(?<name>.+?)\</a\>",
                    RegexOptions.Singleline);
                return WebUtility.HtmlDecode(match.Groups["name"].Value);
            }
        }

        public long Views
        {
            get
            {
                Match match = Regex.Match(content, @"(?<=\<div class\=""watch-view-count""\>).+?(?=\sviews\</div\>)");
                string str = match.Value.Replace(",", "");
                return long.Parse(str);
            }
        }

        public long Likes
        {
            get
            {
                Match match = Regex.Match(content,
                    @"(?<=like-button-renderer-like-button-unclicked[^<>]+?\>\<span class\=""yt-uix-button-content""\>)[0-9,]*?(?=\</span\>)");
                string str = match.Value.Replace(",", "");
                return long.Parse(str);
            }
        }

        public long Dislikes
        {
            get
            {
                Match match = Regex.Match(content,
                    @"(?<=like-button-renderer-dislike-button-unclicked[^<>]+?\>\<span class\=""yt-uix-button-content""\>)[0-9,]*?(?=\</span\>)");
                string str = match.Value.Replace(",", "");
                return long.Parse(str);
            }
        }

        public long Subscribers
        {
            get
            {
                Match match = Regex.Match(content,
                    @"(?<=class\=""[^<>]*?yt-subscriber-count[^<>]*?title\="")[^""]*?(?="")");
                string str = match.Value.Replace("K", "000").Replace("M", "000000");
                return long.Parse(str);
            }
        }

        public List<Recommendation> RelatedVideos
        {
            get
            {
                List<Recommendation> list = new List<Recommendation>();
                foreach (Match match in Regex.Matches(content,
                    @"\<li class\=""video-list-item related-list-item.+?(\</div\>|\</a\>)\s*?</li>",
                    RegexOptions.Singleline)) list.Add(Recommendation.FromLiElement(match.Value));
                return list;
            }
        }

        public static async Task<Video> fromID(string videoID){
            Uri uri = new Uri("https://www.youtube.com/watch?v=" + videoID);
            Video youtubeVideo = await new Video(uri).LoadSite();
            youtubeVideo.VideoID = videoID;
            return youtubeVideo;
        }

        private async Task<Video> LoadSite(){
            if (DefaultHttpWebRequest == null) {
                WebHeaderCollection headers = request.Headers;
                //headers.Add("User-Agent", UserAgent);
                request.UserAgent = UserAgent;
                //headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                request.Accept = "text/html";
                //headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.5");
                //headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
                //headers.Add("DNT", "1");
                //headers.Add("Upgrade-Insecure-Requests", "1");
                //headers.Add("Connection", "keep-alive");
                request.KeepAlive = true;
                //headers.Add("Pragma", "no-cache");
                //headers.Add("Cache-Control", "no-cache");
            }
            await Load();
            Success &= !Regex.Match(content, @"id\=""player""[^""]*?class=""[^""]*?off-screen-trigger[^""]*?""").Success;
            return this;
        }

        public List<VideoFile> ExtractFiles(){
            string jsonString = Regex.Match(content, @"(?<=ytplayer\.config\s\=\s){.*?}(?=;)").Value;
            dynamic json = JsonConvert.DeserializeObject(jsonString);
            string adaptiveFormatStr = json.args.adaptive_fmts;
            adaptiveFormatStr = Regex.Unescape(adaptiveFormatStr);
            string urlencstreammapStr = json.args.url_encoded_fmt_stream_map;
            urlencstreammapStr = Regex.Unescape(urlencstreammapStr);
            string jsassets = json.assets.js;
            string playerVersion = Regex.Match(jsassets, @"(?<=player-).+?(?=/)").Value;
            List<VideoFile> formats = new List<VideoFile>();
            foreach (string formatStr in adaptiveFormatStr.Split(','))
                formats.Add(VideoFile.FromFormatString(formatStr, playerVersion));
            foreach (string formatStr in urlencstreammapStr.Split(','))
                formats.Add(VideoFile.FromFormatString(formatStr, playerVersion));
            return formats;
        }
    }
}