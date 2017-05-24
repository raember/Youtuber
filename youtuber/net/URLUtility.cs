using System;
using System.Text.RegularExpressions;

namespace youtuber.net
{
    /// <summary>
    ///     Utility class revolving Youtube uris.
    /// </summary>
    public static class URLUtility
    {
        private const string VIDEOIDPATTERN = @"(?<id>[a-zA-Z0-9_-]{11})";
        private const string PLAYLISTPATTERN = @"(?<pl>[a-zA-Z0-9_-]{13})";

        private const string VIDEO1PATHQUERYVALIDATIONPATTERN =
            @"^/watch\?v\=" + VIDEOIDPATTERN + @"(&list\=" + PLAYLISTPATTERN + "|)";

        private const string VIDEO2PATHQUERYVALIDATIONPATTERN = @"^/" + VIDEOIDPATTERN + "$";

        private const string VIDEO3PATHQUERYVALIDATIONPATTERN = @"^/embed/" + VIDEOIDPATTERN + "$";

        private const string IMAGEPATHQUERYVALIDATIONPATTERN =
            @"^/vi/" + VIDEOIDPATTERN + @"/(0|1|2|3|default|hqdefault|mqdefault|sddefault|maxresdefault).jpg$";

        /// <summary>
        ///     Analyzes a given uri. Succeeds only if the uri is a valid youtube link to either a video(can be in a playlist) or
        ///     an image of a video.
        /// </summary>
        /// <param name="uri">the uri in question</param>
        /// <returns>an Enum representing the essential properties of the uri</returns>
        public static URLResult AnalyzeURI(Uri uri){
            URLResult result = 0;
            var isValid = false;
            Match match;
            switch (uri.Host) {
                case "www.youtube.com":
                case "youtube.com":
                case "www.m.youtube.com":
                case "m.youtube.com":
                    result |= URLResult.isVideo;
                    match = Regex.Match(uri.PathAndQuery, VIDEO1PATHQUERYVALIDATIONPATTERN);
                    if (match.Success) {
                        result |= URLResult.hasVideoID;
                        if (match.Groups["pl"].Success) result |= URLResult.isPlaylist;
                        isValid = true;
                    }
                    break;
                case "www.youtu.be":
                case "youtu.be":
                    result |= URLResult.isVideo;
                    match = Regex.Match(uri.PathAndQuery, VIDEO2PATHQUERYVALIDATIONPATTERN);
                    if (match.Success) {
                        result |= URLResult.hasVideoID;
                        if (match.Groups["pl"].Success) result |= URLResult.isPlaylist;
                        isValid = true;
                    }
                    break;
                case "www.youtube-nocookie.com":
                case "youtube-nocookie.com":
                    result |= URLResult.isVideo;
                    match = Regex.Match(uri.PathAndQuery, VIDEO3PATHQUERYVALIDATIONPATTERN);
                    if (match.Success) {
                        result |= URLResult.hasVideoID;
                        if (match.Groups["pl"].Success) result |= URLResult.isPlaylist;
                        isValid = true;
                    }
                    break;
                case "img.youtube.com":
                case "i.ytimg.com":
                case "i1.ytimg.com":
                case "i2.ytimg.com":
                case "i3.ytimg.com":
                case "i4.ytimg.com":
                    result |= URLResult.isImage;
                    match = Regex.Match(uri.PathAndQuery, IMAGEPATHQUERYVALIDATIONPATTERN);
                    if (match.Success) {
                        result |= URLResult.hasVideoID;
                        isValid = true;
                    }
                    break;
            }
            isValid &= ("file".Equals(uri.Scheme) ||
                        "https".Equals(uri.Scheme)) &&
                       string.IsNullOrWhiteSpace(uri.UserInfo) &&
                       (uri.Port == -1 ||
                        uri.Port == 443);
            if (isValid) result |= URLResult.isValid;
            return result;
        }

        /// <summary>
        ///     Extracts a video ID of a given uri.
        /// </summary>
        /// <param name="uri">the uri to be parsed</param>
        /// <returns>the video ID within the uri</returns>
        public static string ExtractVideoID(Uri uri){
            var match = Regex.Match(uri.PathAndQuery, @"(?<=/watch\?v\=|/embed/|/vi/|/)" + VIDEOIDPATTERN);
            return match.Groups["id"].Value;
        }

        /// <summary>
        ///     Extracts a playlist ID of a given uri.
        /// </summary>
        /// <param name="uri">the uri to be parsed</param>
        /// <returns>the playlist ID within the uri</returns>
        public static string ExtractPlaylistID(Uri uri){
            var match = Regex.Match(uri.Query, @"(?<=&list\=)" + PLAYLISTPATTERN);
            return match.Groups["pl"].Value;
        }
    }

    [Flags]
    public enum URLResult
    {
        isValid = 1 << 0,
        hasVideoID = 1 << 1,
        isVideo = 1 << 2,
        isPlaylist = 1 << 3,
        isImage = 1 << 4
    }
}