using System;
using System.Net;
using System.Text.RegularExpressions;

namespace youtuber.net
{
    public class Recommendation
    {
        protected Recommendation(string title, string videoId){
            Title = title;
            VideoID = videoId;
        }

        public string Title {get;}

        public string VideoID {get;}

        public static Recommendation FromLiElement(string liElement){
            string title;
            var pathandquery = WebUtility.HtmlDecode(Regex.Match(liElement, @"(?<=href\="").+?(?="")").Value);
            var tempUri = new Uri("https://www.youtube.com" + pathandquery);
            var videoID = URLUtility.ExtractVideoID(tempUri);
            var videoContent = Regex.Match(liElement,
                @"\<div class\=""content-wrapper""\>\s*?\<a\s.*?\</a\>\s*?\</div\>", RegexOptions.Singleline);
            if (videoContent.Success) {
                var content = videoContent.Value;
                title = WebUtility.HtmlDecode(Regex.Match(content,
                                                       @"(?<=\<span[^<>]+?class\=""title""[^<>]*?\>)[^<>]+?(?=\</span\>)")
                                                   .Value.Trim('\n', '\r', ' ', '\t'));
                string username = WebUtility.HtmlDecode(Regex
                    .Match(content, @"(?<=\<span class\=""g-hovercard""[^<>]*?\>)[^<>]*?(?=\</span\>)").Value);
                string durationStr = Regex.Match(content,
                                              @"(?<=\<span class\=""accessible-description""[^<>]*?\>.*?:\s)[\d:]+?(?=\.\s*?\</span\>)",
                                              RegexOptions.Singleline)
                                          .Value;
                TimeSpan duration = TimeSpan.Zero;
                if (!string.IsNullOrEmpty(durationStr)) {
                    switch (durationStr.Split(':').Length) {
                        case 1:
                            durationStr = "0:0:" + durationStr;
                            break;
                        case 2:
                            durationStr = "0:" + durationStr;
                            break;
                    }
                    duration = TimeSpan.Parse(durationStr);
                }
                string viewsStr = Regex.Match(content, @"(?<=\<span class\=""stat view-count""\>)[^<>]*?(?=\sviews\</span\>)").Value;
                long views = string.IsNullOrEmpty(viewsStr) ? -1 : long.Parse(viewsStr.Replace(",", ""));
                return new Video(title, videoID, username, duration, views);
            }
            title = WebUtility.HtmlDecode(Regex.Match(liElement,
                                                   @"(?<=\<span[^<>]+?class\=""title""[^<>]*?\>)[^<>]+?(?=\</span\>)")
                                               .Value);
            var from = WebUtility.HtmlDecode(Regex
                .Match(liElement, @"(?<=\<span class\=""stat attribution""\>)[^<>]+?(?=\</span\>)").Value);
            var videos = Regex.Match(liElement, @"(?<=\<b\>)[^<>]+?(?=\</b\>)").Value.Trim();
            int videoCount;
            if (videos.EndsWith("+")) { videoCount = int.MaxValue; } else {
                videoCount = int.Parse(videos.Substring(0, videos.Length - 1));
            }
            string playlistID = URLUtility.ExtractPlaylistID(tempUri);
            return new Playlist(title, videoID, playlistID, from, videoCount);
        }

        public override string ToString(){
            return Title;
        }

        public class Video : Recommendation
        {
            internal Video(string title, string videoId, string username, TimeSpan duration, long views) : base(title,
                videoId){
                Username = username;
                Duration = duration;
                Views = views;
            }

            public string Username {get;}
            public TimeSpan Duration {get;}
            public long Views {get;}
        }

        public class Playlist : Recommendation
        {
            internal Playlist(string title, string videoId, string playlistId, string from,
                              int videosCount) : base(title, videoId){
                PlaylistID = playlistId;
                From = from;
                VideosCount = videosCount;
            }

            public string PlaylistID {get;}
            public string From {get;}
            public int VideosCount {get;}
        }
    }
}