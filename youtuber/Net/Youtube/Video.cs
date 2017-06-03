using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using youtuber.Net.Youtube;

namespace youtuber.net
{
    public class Video : InternetSite
    {
        private Video(Uri uri) : base(uri){ }
        private Video(Uri uri, CookieCollection cookies) : base(uri, cookies){ }

        private dynamic json;

        public string Title
        {
            get
            {
                try { return WebUtility.HtmlDecode(json.args.title.ToString()); } catch (Exception) {
                    return string.Empty;
                }
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
                if (!match.Success) return DateTime.MinValue;
                return DateTime.ParseExact(match.Value, "yyyy-MM-dd", new CultureInfo("en-US"));
            }
        }

        public DateTime RequestTime
        {
            get
            {
                try {
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddSeconds(int.Parse(json.args.timestamp.ToString())).ToLocalTime();
                    return dtDateTime;
                } catch (Exception) { return DateTime.MinValue; }
            }
        }

        public string Description
        {
            get
            {
                Match match = Regex.Match(content, @"(?<=id\=""eow-description""[^<>]*?\>).*?(?=\</p\>)",
                    RegexOptions.Singleline);
                if (!match.Success) return string.Empty;
                string description = match.Value;
                description = Regex.Replace(description, @"\<a[^<>]*?data-url\=""([^""]+?)""[^<>]*?\>[^<>]*?\</a\>",
                    @"$1");
                description = Regex.Replace(description, @"\<br\s*?/\>", "\n");
                return WebUtility.HtmlDecode(description);
            }
        }

        public string User
        {
            get
            {
                try { return WebUtility.HtmlDecode(json.args.author.ToString()); } catch (Exception) {
                    return string.Empty;
                }
            }
        }

        public string UserID
        {
            get
            {
                Match match = Regex.Match(content, @"(?<=www\.youtube\.com/user/)[^/""]+?(?="")");
                if (!match.Success) return string.Empty;
                return Regex.Unescape(match.Value);
            }
        }

        public List<string> Keywords
        {
            get
            {
                try {
                    List<string> keywords = new List<string>(json.args.keywords.ToString().Split(','));
                    return keywords.ConvertAll(Regex.Unescape);
                } catch (Exception) { return new List<string>(); }
            }
        }

        public long Views
        {
            get
            {
                try { return long.Parse(json.args.view_count.ToString()); } catch (Exception) { return -1; }
            }
        }

        public TimeSpan Duration
        {
            get
            {
                try { return TimeSpan.FromSeconds(int.Parse(json.args.length_seconds.ToString())); } catch (Exception) {
                    return TimeSpan.Zero;
                }
            }
        }

        public long Likes
        {
            get
            {
                Match match = Regex.Match(content,
                    @"(?<=like-button-renderer-like-button-unclicked[^<>]+?\>\<span class\=""yt-uix-button-content""\>)[0-9,]*?(?=\</span\>)");
                if (!match.Success) return 0;
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
                if (!match.Success) return 0;
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
                if (!match.Success) return 0;
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

        public static async Task<Video> fromID(string videoID, CookieCollection cookies){
            Uri uri = new Uri("https://www.youtube.com/watch?v=" + videoID);
            Video youtubeVideo = await new Video(uri, cookies).LoadSite();
            youtubeVideo.VideoID = videoID;
            return youtubeVideo;
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
            Success &= !Regex.Match(content, @"id\=""player""[^""]*?class=""[^""]*?off-screen-trigger[^""]*?""")
                             .Success;
            string jsonString = Regex.Match(content, @"(?<=ytplayer\.config\s\=\s){.*?}(?=;)").Value;
            json = JsonConvert.DeserializeObject(jsonString);
            return this;
        }

        public List<VideoFile> ExtractFiles(){
            try {
                dynamic args = this.json.args;

                string adaptiveFormatStr = args == null ? String.Empty : args.adaptive_fmts;
                adaptiveFormatStr = string.IsNullOrEmpty(adaptiveFormatStr) ? String.Empty : adaptiveFormatStr;
                adaptiveFormatStr = Regex.Unescape(adaptiveFormatStr);
                string urlencstreammapStr = args.url_encoded_fmt_stream_map;
                urlencstreammapStr = string.IsNullOrEmpty(urlencstreammapStr) ? String.Empty : urlencstreammapStr;
                urlencstreammapStr = Regex.Unescape(urlencstreammapStr);
                dynamic assets = json.assets;
                string playerVersion = assets.js;
                string nonDashFormatDetails = args.fmt_list;
                List<string> formatDetailsStr = Regex.Unescape(nonDashFormatDetails).Split(',').ToList();
                List<Match> formatDetails =
                    formatDetailsStr.ConvertAll(s => Regex.Match(s,
                                                    @"^(?<itag>\d+?)/(?<width>\d+?)x(?<height>\d+?)/(?<arg1>\d+?)/(?<arg2>\d+?)/(?<arg3>\d+?)$"));
                List<VideoFile> formats = new List<VideoFile>();
                if (!string.IsNullOrEmpty(adaptiveFormatStr)) {
                    foreach (string formatStr in adaptiveFormatStr.Split(',')) {
                        VideoFile vf = VideoFile.FromFormatString(formatStr, playerVersion);
                        formats.Add(vf);
                    }
                }
                if (!string.IsNullOrEmpty(urlencstreammapStr)) {
                    foreach (string formatStr in urlencstreammapStr.Split(',')) {
                        VideoFile vf = VideoFile.FromFormatString(formatStr, playerVersion);
                        formats.Add(vf);
                        Match match =
                            formatDetails.FirstOrDefault(fd => int.Parse(fd.Groups["itag"].Value).Equals(vf.ITag));
                        if (match != null) {
                            var vfn = vf as VideoFile.Normal;
                            vfn.Width = int.Parse(match.Groups["width"].Value);
                            vfn.Height = int.Parse(match.Groups["height"].Value);
                            vfn.Arg1 = int.Parse(match.Groups["arg1"].Value);
                            vfn.Arg2 = int.Parse(match.Groups["arg2"].Value);
                            vfn.Arg3 = int.Parse(match.Groups["arg3"].Value);
                        }
                    }
                }
                formats.Sort();
                return formats;
            } catch (Exception e) {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}