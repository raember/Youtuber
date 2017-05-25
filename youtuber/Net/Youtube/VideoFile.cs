using System;
using System.Net;

namespace youtuber.Net.Youtube
{
    public class VideoFile
    {
        internal VideoFile(string rawFormat){
            foreach (var keyvaluepair in rawFormat.Split('&')) {
                var key = WebUtility.UrlDecode(keyvaluepair.Split('=')[0]);
                var value = WebUtility.UrlDecode(WebUtility.UrlDecode(keyvaluepair.Split('=')[1]));
                switch (key) {
                    case "s":
                        S = value;
                        break;
                    case "init":
                        Init = value;
                        break;
                    case "type":
                        Type = value;
                        break;
                    case "index":
                        Index = value;
                        break;
                    case "fps":
                        Fps = int.Parse(value);
                        break;
                    case "bitrate":
                        Bitrate = int.Parse(value);
                        break;
                    case "size":
                        Size = value;
                        break;
                    case "clen":
                        Clen = value;
                        break;
                    case "xtags":
                        XTags = value;
                        break;
                    case "url":
                        Url = new Uri(value);
                        break;
                    case "quality_label":
                        XTags = value;
                        break;
                    case "itag":
                        ITag = int.Parse(value);
                        break;
                    case "lmt":
                        Lmt = long.Parse(value);
                        break;
                    case "projection_type":
                        ProjectionType = int.Parse(value);
                        break;
                    default:
                        throw new Exception(string.Format("No mapping for {0} implemented", key));
                }
            }
        }

        public string S {get;}
        public string Init {get;}
        public string Type {get;}
        public string Index {get;}
        public int Fps {get;}
        public int Bitrate {get;}
        public string Size {get;}
        public string Clen {get;}
        public string XTags {get;}
        public Uri Url {get;}
        public string QualityLabel {get;}
        public int ITag {get;}
        public long Lmt {get;}
        public int ProjectionType {get;}

        public override string ToString(){
            return ITag + ": " + QualityLabel + ", " + Type + ", " + Size;
        }
    }
}