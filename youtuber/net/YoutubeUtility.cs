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
        public static URLResult analyseURL(Uri uri){
            URLResult result = 0;
            if (string.IsNullOrEmpty(uri.Scheme) ||
                "https".Equals(uri.Scheme) &&
                string.IsNullOrWhiteSpace(uri.UserInfo) &&
                uri.Port == 0 ||
                uri.Port == 443) { result |= URLResult.isValid; }
            switch (uri.Host) {
                case "www.youtube.com":
                case "youtube.com":
                case "www.m.youtube.com":
                case "m.youtube.com":
                    result |= URLResult.isVideo;
                    break;
                case "www.youtu.be":
                case "youtu.be":
                case "www.youtube-nocookie.com":
                case "youtube-nocookie.com":
                    result |= URLResult.isVideo;
                    break;
                case "img.youtube.com":
                case "i.ytimg.com":
                case "i1.ytimg.com":
                case "i2.ytimg.com":
                case "i3.ytimg.com":
                case "i4.ytimg.com":
                    result |= URLResult.isImage;
                    if (!uri.PathAndQuery.StartsWith("/vi/")) {
                        if (result.HasFlag(URLResult.isValid)) result -= URLResult.isValid;
                    }
                    break;
            }
            if (Regex.IsMatch(uri.PathAndQuery, @"^/(watch\?v\=|embed/|vi/|)[a-zA-Z0-9_-]{11}")) {
                result |= URLResult.hasVideoID;
            }
            if (Regex.IsMatch(uri.PathAndQuery, @"&list=[a-zA-Z0-9_-]{13}")) { result |= URLResult.isPlaylist; }
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