using System;
using System.Net;
using System.Text.RegularExpressions;

namespace youtuber.net
{
    public class YoutubeRecommendation
    {
        private YoutubeRecommendation(string title, string videoId, string playlistId){
            Title = title;
            VideoId = videoId;
            PlaylistId = playlistId;
        }

        public string Title {get;}

        public string VideoId {get;}

        public string PlaylistId {get;}

        public bool IsPlaylist => !string.IsNullOrEmpty(PlaylistId);

        public static YoutubeRecommendation FromLiElement(string liElement){
            var title = WebUtility.HtmlDecode(Regex
                .Match(liElement, @"(?<=\<(a|span)[^<>]+?title\="").+?(?="")").Value);
            var pathandquery =
                WebUtility.HtmlDecode(Regex.Match(liElement, @"(?<=\<a href\="").+?(?="")").Value);
            var tempUri = new Uri("https://www.youtube.com" + pathandquery);
            var result = URLUtility.AnalyzeURI(tempUri);
            var videoID = URLUtility.ExtractVideoID(tempUri);
            string playlistID = null;
            if (result.HasFlag(URLResult.isPlaylist)) playlistID = URLUtility.ExtractPlaylistID(tempUri);
            return new YoutubeRecommendation(title, videoID, playlistID);
        }

        public override string ToString(){
            return Title;
        }
    }
}