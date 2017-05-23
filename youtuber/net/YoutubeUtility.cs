using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace youtuber.net
{
    public static class YoutubeUtility
    {
        private const String VIDEOIDPATTERN = @"(?<id>[a-zA-Z0-9_-]{11})";
        private const String PLAYLISTPATTERN = @"&list\=(?<pl>[a-zA-Z0-9_-]{13})";

        private const String VIDEO1PATHQUERYVALIDATIONPATTERN =
            @"^/watch\?v\=" + VIDEOIDPATTERN + "(" + PLAYLISTPATTERN + "|)";

        private const String VIDEO2PATHQUERYVALIDATIONPATTERN = @"^/" + VIDEOIDPATTERN + "(" + PLAYLISTPATTERN + "|)";

        private const String VIDEO3PATHQUERYVALIDATIONPATTERN =
            @"^/embed/" + VIDEOIDPATTERN + "(" + PLAYLISTPATTERN + "|)";

        private const String IMAGEPATHQUERYVALIDATIONPATTERN = @"^/vi/" + VIDEOIDPATTERN +
                                                               @"/(0|1|2|3|default|hqdefault|mqdefault|sddefault|maxresdefault).jpg$"
            ;

        public static URLResult analyzeURL(Uri uri){
            URLResult result = 0;
            bool isValid = false;
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