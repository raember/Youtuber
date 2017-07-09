using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace Youtuber.Net {
    /// <summary>
    ///     Utility class revolving Youtube uris.
    /// </summary>
    public static class URLUtility {
        private const string VIDEOIDPATTERN = @"(?<id>[\w\d_-]{11})";
        private const string PLAYLISTPATTERNSHORT = @"(?<pl>[\w\d_-]{13})";
        private const string PLAYLISTPATTERNLONG = @"(?<pl>[\w\d_-]{34})";

        private const string VIDEO1PATHQUERYVALIDATIONPATTERN =
            @"^/watch\?v\=" + VIDEOIDPATTERN + @"(&list\=" + PLAYLISTPATTERNSHORT + "|)";

        private const string VIDEO2PATHQUERYVALIDATIONPATTERN = @"^/" + VIDEOIDPATTERN + "$";

        private const string PLAYLISTPATHQUERYVALIDATIONPATTERN = @"^/playlist\?list=" + PLAYLISTPATTERNLONG + "$";

        private const string VIDEO3PATHQUERYVALIDATIONPATTERN = @"^/embed/" + VIDEOIDPATTERN + "$";

        private const string IMAGEPATHQUERYVALIDATIONPATTERN =
            @"^/vi/" + VIDEOIDPATTERN + @"/(0|1|2|3|default|hqdefault|mqdefault|sddefault|maxresdefault).jpg";

        /// <summary>
        ///     Analyzes a given uri. Succeeds only if the uri is a valid youtube link to either a video(can be in a playlist) or
        ///     an image of a video.
        /// </summary>
        /// <param name="uri">the uri in question</param>
        /// <returns>an Enum representing the essential properties of the uri</returns>
        public static URLResult AnalyzeURI(Uri uri){
            URLResult result = 0;
            bool isValid = false;
            Match match;
            switch (uri.Host) {
                case "www.youtube.com":
                case "youtube.com":
                case "www.m.youtube.com":
                case "m.youtube.com":
                    match = Regex.Match(uri.PathAndQuery, VIDEO1PATHQUERYVALIDATIONPATTERN);
                    if (match.Success) {
                        result |= URLResult.IsVideo;
                        result |= URLResult.HasVideoID;
                        if (match.Groups["pl"].Success) result |= URLResult.IsPlaylist;
                        isValid = true;
                    } else {
                        if (Regex.IsMatch(uri.PathAndQuery, PLAYLISTPATHQUERYVALIDATIONPATTERN)) {
                            result |= URLResult.IsPlaylist;
                            isValid = true;
                        }
                    }
                    break;
                case "www.youtu.be":
                case "youtu.be":
                    result |= URLResult.IsVideo;
                    match = Regex.Match(uri.PathAndQuery, VIDEO2PATHQUERYVALIDATIONPATTERN);
                    if (match.Success) {
                        result |= URLResult.HasVideoID;
                        if (match.Groups["pl"].Success) result |= URLResult.IsPlaylist;
                        isValid = true;
                    }
                    break;
                case "www.youtube-nocookie.com":
                case "youtube-nocookie.com":
                    result |= URLResult.IsVideo;
                    match = Regex.Match(uri.PathAndQuery, VIDEO3PATHQUERYVALIDATIONPATTERN);
                    if (match.Success) {
                        result |= URLResult.HasVideoID;
                        if (match.Groups["pl"].Success) result |= URLResult.IsPlaylist;
                        isValid = true;
                    }
                    break;
                case "img.youtube.com":
                case "i.ytimg.com":
                case "i1.ytimg.com":
                case "i2.ytimg.com":
                case "i3.ytimg.com":
                case "i4.ytimg.com":
                    result |= URLResult.IsImage;
                    match = Regex.Match(uri.PathAndQuery, IMAGEPATHQUERYVALIDATIONPATTERN);
                    if (match.Success) {
                        result |= URLResult.HasVideoID;
                        isValid = true;
                    }
                    break;
            }
            isValid &= ("file".Equals(uri.Scheme) ||
                        "https".Equals(uri.Scheme)) &&
                       string.IsNullOrWhiteSpace(uri.UserInfo) &&
                       (uri.Port == -1 ||
                        uri.Port == 443);
            if (isValid) result |= URLResult.IsValid;
            return result;
        }

        /// <summary>
        ///     Extracts a video ID of a given uri.
        /// </summary>
        /// <param name="uri">the uri to be parsed</param>
        /// <returns>the video ID within the uri</returns>
        public static string ExtractVideoID(Uri uri){
            Match match = Regex.Match(uri.PathAndQuery, @"(?<=/watch\?v\=|/embed/|/vi/|/)" + VIDEOIDPATTERN);
            return match.Groups["id"].Value;
        }

        /// <summary>
        ///     Extracts a playlist ID of a given uri.
        /// </summary>
        /// <param name="uri">the uri to be parsed</param>
        /// <returns>the playlist ID within the uri</returns>
        public static string ExtractPlaylistID(Uri uri){
            Match match = Regex.Match(uri.Query, @"(?<=&list\=)" + PLAYLISTPATTERNSHORT);
            if (match.Success) return match.Groups["pl"].Value;
            match = Regex.Match(uri.Query, @"(?<=\?playlist\=)" + PLAYLISTPATTERNLONG);
            return match.Groups["pl"].Value;
        }

        public static Dictionary<string, string> ExtractParameters(string urlQuery){
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string line in urlQuery.Split('&')) {
                string[] splitted = line.Split('=');
                string key = WebUtility.UrlDecode(splitted[0]);
                string value = WebUtility.UrlDecode(WebUtility.UrlDecode(splitted[1]));
                result.Add(key, value);
            }
            return result;
        }
    }

    [Flags]
    public enum URLResult {
        IsValid = 1 << 0,
        HasVideoID = 1 << 1,
        IsVideo = 1 << 2,
        IsPlaylist = 1 << 3,
        IsImage = 1 << 4
    }
}