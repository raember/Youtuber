using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace youtuber.net
{
    public class Video : InternetSite
    {
        private Video(Uri uri) : base(uri){ }

        public string Title
        {
            get
            {
                var match = Regex.Match(content,
                    @"\<span id\=""eow-title"" class=""watch-title"" dir=""ltr"" title=""(?<title>.+?)""\>");
                return WebUtility.HtmlDecode(match.Groups["title"].Value);
            }
        }

        public string VideoID {get; private set;}

        public DateTime UploadedDateTime
        {
            get
            {
                var match = Regex.Match(content,
                    @"\<strong class\=""watch-time-text""\>(\w|\s)+?(?<date>\d+?\.\d+?.\d{4})\</strong\>");
                return DateTime.Parse(match.Groups["date"].Value);
            }
        }

        public string Description
        {
            get
            {
                var match = Regex.Match(content, @"\<p id\=""eow-description"" class="""" \>(?<description>.+?)</p>",
                    RegexOptions.Singleline);
                return WebUtility.HtmlDecode(match.Groups["description"].Value);
            }
        }

        public string Username
        {
            get
            {
                var match = Regex.Match(content,
                    @"\<div class\=""yt-user-info""\>.+?data-sessionlink\="".+?"" \>(?<name>.+?)\</a\>",
                    RegexOptions.Singleline);
                return WebUtility.HtmlDecode(match.Groups["name"].Value);
            }
        }

        public long Views
        {
            get
            {
                var match = Regex.Match(content, @"\<div class\=""watch-view-count""\>(?<views>[\d\.]+?)\s.+?\</div\>");
                var str = match.Groups["views"].Value.Replace(".", "");
                return long.Parse(str);
            }
        }

        public long Likes
        {
            get
            {
                var match = Regex.Match(content,
                    @"\<button class="".+?-like-button.+?\<span class=""yt-uix-button-content""\>(?<likes>[\d\.]+?)\</span\>");
                var str = match.Groups["likes"].Value.Replace(".", "");
                return long.Parse(str);
            }
        }

        public long Dislikes
        {
            get
            {
                var match = Regex.Match(content,
                    @"\<button class="".+?-dislike-button.+?\<span class=""yt-uix-button-content""\>(?<dislikes>[\d\.]+?)\</span\>");
                var str = match.Groups["dislikes"].Value.Replace(".", "");
                return long.Parse(str);
            }
        }

        public long Subscribers
        {
            get
            {
                var match = Regex.Match(content,
                    @"\<span class=""yt-subscription-button-subscriber-count-branded-horizontal yt-subscriber-count"" title="".+?"" aria-label="".+?"" tabindex=""0""\>(?<subs>[\d\.]+?)\</span\>");
                var str = match.Groups["subs"].Value.Replace(".", "");
                return long.Parse(str);
            }
        }

        public List<Recommendation> RelatedVideos
        {
            get
            {
                var list = new List<Recommendation>();
                foreach (Match match in Regex.Matches(content,
                    @"\<li class\=""video-list-item related-list-item.+?(\</div\>|\</a\>)\s*?</li>",
                    RegexOptions.Singleline)) list.Add(Recommendation.FromLiElement(match.Value));
                return list;
            }
        }

        public static async Task<Video> fromID(string videoID){
            var uri = new Uri("https://www.youtube.com/watch?v=" + videoID);
            var youtubeVideo = await new Video(uri).LoadSite();
            youtubeVideo.VideoID = videoID;
            return youtubeVideo;
        }

        private async Task<Video> LoadSite(){
            var headers = request.Headers;
            headers.Add("Host", "www.youtube.com");
            headers.Add("User-Agent", UserAgent);
            headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.5");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add("DNT", "1");
            headers.Add("Upgrade-Insecure-Requests", "1");
            headers.Add("Connection", "keep-alive");
            headers.Add("Pragma", "no-cache");
            headers.Add("Cache-Control", "no-cache");
            await Load();
            return this;
        }
    }
}