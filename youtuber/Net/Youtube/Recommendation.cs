using System;
using System.Net;
using System.Text.RegularExpressions;

namespace youtuber.Net {
    public class Recommendation
    {
        protected Recommendation(string title, string videoId){
            Title = title;
            VideoID = videoId;
        }

        public string Title {get;}

        public string VideoID {get;}

        public static Recommendation FromLiElement(string liElement){
            string pathandquery = WebUtility.HtmlDecode(Regex.Match(liElement, @"(?<=href\="")[^""]+?(?="")").Value);
            Uri tempUri = new Uri("https://www.youtube.com" + pathandquery);
            string videoID = URLUtility.ExtractVideoID(tempUri);
            if (liElement.Contains("yt-badge-live")) return LiveStream.FromLiElement(liElement, videoID);
            Match videoContent = Regex.Match(liElement,
                @"\<div class\=""content-wrapper""\>\s*?\<a\s.*?\</a\>\s*?\</div\>", RegexOptions.Singleline);
            if (videoContent.Success) return Video.FromLiElement(videoContent.Value, videoID);
            string playlistId = URLUtility.ExtractPlaylistID(tempUri);
            return Playlist.FromLiElement(liElement, videoID, playlistId);
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

            internal static Video FromLiElement(string liElement, string videoId){
                string title = WebUtility.HtmlDecode(Regex.Match(liElement,
                                                              @"(?<=\<span[^<>]+?class\=""title""[^<>]*?\>)[^<>]+?(?=\</span\>)")
                                                          .Value.Trim('\n', '\r', ' ', '\t'));
                string username = WebUtility.HtmlDecode(Regex
                    .Match(liElement, @"(?<=\<span class\=""g-hovercard""[^<>]*?\>)[^<>]*?(?=\</span\>)").Value);
                string durationStr = Regex.Match(liElement,
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
                string viewsStr = Regex.Match(liElement,
                                           @"(?<=\<span class\=""stat view-count""\>)[^<>]*?(?=\sviews\</span\>)")
                                       .Value;
                long views = string.IsNullOrEmpty(viewsStr) ? -1 : long.Parse(viewsStr.Replace(",", ""));
                return new Video(title, videoId, username, duration, views);
            }
        }

        public class Playlist : Recommendation
        {
            internal Playlist(string title, string videoId, string playlistId, string from,
                              int videosCount, bool videoCountUnfinished) : base(title, videoId){
                PlaylistID = playlistId;
                From = from;
                VideosCount = videosCount;
                VideoCountUnfinished = videoCountUnfinished;
            }

            public string PlaylistID {get;}
            public string From {get;}
            public int VideosCount {get;}
            public bool VideoCountUnfinished {get;}

            internal static Playlist FromLiElement(string liElement, string videoId, string playlistId){
                string title = WebUtility.HtmlDecode(Regex.Match(liElement,
                                                              @"(?<=\<span[^<>]+?class\=""title""[^<>]*?\>)[^<>]+?(?=\</span\>)")
                                                          .Value);
                string from = WebUtility.HtmlDecode(Regex
                    .Match(liElement, @"(?<=\<span class\=""stat attribution""\>)[^<>]+?(?=\</span\>)").Value);
                string videos = Regex.Match(liElement, @"(?<=\<b\>)[^<>]+?(?=\</b\>)").Value.Trim();
                int videoCount;
                bool videoCountUnfinished = videos.EndsWith("+");
                videoCount = int.Parse(Regex.Match(videos, @"^\d+?(?=\D|$)").Value);
                return new Playlist(title, videoId, playlistId, from, videoCount, videoCountUnfinished);
            }
        }

        public class LiveStream : Recommendation
        {
            //yt-badge-live
            internal LiveStream(string title, string videoId, string username, long views) : base(title,
                videoId){
                Username = username;
                Views = views;
            }

            public string Username {get;}
            public long Views {get;}

            internal static LiveStream FromLiElement(string liElement, string videoId){
                string title = WebUtility.HtmlDecode(Regex.Match(liElement,
                                                              @"(?<=\<span[^<>]+?class\=""title""[^<>]*?\>)[^<>]+?(?=\</span\>)")
                                                          .Value.Trim('\n', '\r', ' ', '\t'));
                string username = WebUtility.HtmlDecode(Regex
                    .Match(liElement, @"(?<=\<span class\=""g-hovercard""[^<>]*?\>)[^<>]*?(?=\</span\>)").Value);
                string viewsStr = Regex.Match(liElement,
                                           @"(?<=\<span class\=""stat view-count""\>)[^<>]*?(?=\sviews\</span\>)")
                                       .Value;
                long views = string.IsNullOrEmpty(viewsStr) ? -1 : long.Parse(viewsStr.Replace(",", ""));
                return new LiveStream(title, videoId, username, views);
            }
        }
    }
}