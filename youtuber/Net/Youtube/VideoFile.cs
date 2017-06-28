using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using youtuber.net;

namespace youtuber.Net.Youtube
{
    public abstract class VideoFile : IComparable
    {
        internal const string ITAG = "itag";
        internal const string TYPE = "type";
        internal const string QUALITY = "quality";
        internal const string URL = "url";
        internal const string SIGNATURE = "s";
        internal const string BITRATE = "bitrate";
        internal const string PROJECTIONTYPE = "projection_type";
        internal const string INIT = "init";
        internal const string INDEX = "index";
        internal const string CLEN = "clen";
        internal const string LMT = "lmt";
        internal const string XTAGS = "xtags";
        internal const string FPS = "fps";
        internal const string QUALITYLABEL = "quality_label";
        internal const string SIZE = "size";
        internal const string TARGETDURATIONSEC = "target_duration_sec";
        internal const string STEREOLAYOUT = "stereo_layout";
        internal static ContentType THREE_GP = new ContentType("video/3gpp");
        internal static ContentType MP4_VIDEO = new ContentType("video/mp4");
        internal static ContentType MP4_AUDIO = new ContentType("audio/mp4");
        internal static ContentType WEBM_VIDEO = new ContentType("video/webm");
        internal static ContentType WEBM_AUDIO = new ContentType("audio/webm");
        internal static ContentType FLASH = new ContentType("video/flv");

        internal VideoFile(Dictionary<string, string> arguments){
            Arguments = arguments;
            ITag = int.Parse(Arguments[ITAG]);
            Arguments.Remove(ITAG);
            Type = Arguments[TYPE];
            Arguments.Remove(TYPE);
            Url = Arguments[URL];
            Arguments.Remove(URL);
            S = string.Empty;
            if (Arguments.ContainsKey(SIGNATURE)) {
                S = Arguments[SIGNATURE];
                Arguments.Remove(SIGNATURE);
            }
        }

        public Dictionary<string, string> Arguments {get;}
        public int ITag {get;}
        public string Type {get;}
        public string Url {get;}
        public string S {get;}

        public string Extension
        {
            get
            {
                if (MimeType.Equals(THREE_GP)) return ".3gp";
                if (MimeType.Equals(MP4_VIDEO) || MimeType.Equals(MP4_AUDIO)) return ".mp4";
                if (MimeType.Equals(WEBM_VIDEO) || MimeType.Equals(WEBM_AUDIO)) return ".webm";
                if (MimeType.Equals(FLASH)) return ".flv";
                throw new Exception($"Coudln't map {MimeType} to an extension");
            }
        }

        public ContentType MimeType => new ContentType(Regex.Match(Type, @"^[^;]+?(?=;)").Value);

        protected string PlayerVersion {get; private set;}

        public int CompareTo(object obj){
            return ITag.CompareTo(((VideoFile) obj).ITag);
        }

        public abstract string ToCsvRow();

        public static VideoFile FromFormatString(string format, string playerVersion){
            Dictionary<string, string> args = URLUtility.ExtractParameters(format);
            VideoFile videoFile;
            if (args.ContainsKey(QUALITY)) { // Normal video
                videoFile = new Normal(args);
            } else if (args.ContainsKey(FPS)) { // Dash video
                if (args.ContainsKey(TARGETDURATIONSEC)) videoFile = new DashVideoLive(args); // Live stream
                else if (args.ContainsKey(STEREOLAYOUT)) videoFile = new DashVideo3D(args); // 3D
                else videoFile = new DashVideo(args);
            } else { // Dash audio
                if (args.ContainsKey(TARGETDURATIONSEC)) videoFile = new DashAudioLive(args); // Live stream
                else videoFile = new DashAudio(args);
            }
            videoFile.PlayerVersion = playerVersion;
            return videoFile;
        }

        public async Task<Uri> GetDownloadUri(){
            string url = Url;
            if (!string.IsNullOrEmpty(S)) {
                string signature = (await Decipherer.GetDecipherer(PlayerVersion)).Decipher(S);
                url += $"&signature={signature}";
            }
            url += !Regex.IsMatch(url, @"ratebypass\=yes") ? "&ratebypass=yes" : string.Empty;
            return new Uri(url);
        }

        public override string ToString(){
            return $"{ITag} : {Type}";
        }

        public class Normal : VideoFile
        {
            internal Normal(Dictionary<string, string> arguments) : base(arguments){
                switch (arguments[QUALITY]) {
                    case "small":
                        Quality = VideoQuality.Small;
                        break;
                    case "medium":
                        Quality = VideoQuality.Medium;
                        break;
                    case "hd720":
                        Quality = VideoQuality.HD720;
                        break;
                    default:
                        throw new Exception($"I don't know the quality \"{arguments[QUALITY]}\"");
                }
                Arguments.Remove(QUALITY);
                Match codec = Regex.Match(Type, @"codecs\=""(?<video>[^"",]+?),(?<audio>[^"",]+?)""");
                if (!codec.Success) throw new Exception($"I coudn't parse the codec information out of {Type}");
                VideoCodec = codec.Groups["video"].Value;
                AudioCodec = codec.Groups["audio"].Value;
            }

            public VideoQuality Quality {get;}
            public string VideoCodec {get;}
            public string AudioCodec {get;}
            public int Width {get; internal set;}
            public int Height {get; internal set;}
            public int Arg1 {get; internal set;}
            public int Arg2 {get; internal set;}
            public int Arg3 {get; internal set;}

            public static string GetCsvHeaders(){
                return string.Join(";", ITAG, TYPE, URL, SIGNATURE, QUALITY, "Width", "Height", "Arg1", "Arg2", "Arg3",
                    "*Extension", "*MimeType", "*PlayerVersion");
            }

            public override string ToCsvRow(){
                return string.Join(";", ITag, Type.Replace(';', '|'), Url, S, Quality, Width, Height, Arg1, Arg2, Arg3,
                    Extension, MimeType, PlayerVersion);
            }

            public override string ToString(){
                return $"{ITag} (normal video): {Type}, quality={Quality}";
            }
        }

        public abstract class Dash : VideoFile
        {
            public Dash(Dictionary<string, string> arguments) : base(arguments){
                Bitrate = int.Parse(arguments[BITRATE]);
                Arguments.Remove(BITRATE);
                ProjectionType = int.Parse(arguments[PROJECTIONTYPE]);
                Arguments.Remove(PROJECTIONTYPE);
                string[] index = arguments[INDEX].Split('-');
                Arguments.Remove(INDEX);
                IndexFrom = int.Parse(index[0]);
                IndexTo = int.Parse(index[1]);
                string[] init = arguments[INIT].Split('-');
                Arguments.Remove(INIT);
                InitFrom = int.Parse(init[0]);
                InitTo = int.Parse(init[1]);
                ContentLength = long.Parse(arguments[CLEN]);
                Arguments.Remove(CLEN);
                Lmt = double.Parse(arguments[LMT]);
                Arguments.Remove(LMT);
                string xtagStr = arguments[XTAGS];
                Arguments.Remove(XTAGS);
                XTags = !string.IsNullOrEmpty(xtagStr) ? xtagStr.Split(',').ToList() : new List<string>();
            }

            public int Bitrate {get;}
            public int ProjectionType {get;}
            public int IndexFrom {get;}
            public int IndexTo {get;}
            public int InitFrom {get;}
            public int InitTo {get;}
            public long ContentLength {get;}
            public double Lmt {get;}
            public List<string> XTags {get;}
        }

        public class DashVideo : Dash
        {
            public DashVideo(Dictionary<string, string> arguments) : base(arguments){
                Fps = int.Parse(arguments[FPS]);
                Arguments.Remove(FPS);
                if (arguments.ContainsKey(QUALITYLABEL)) {
                    QualityLabel = arguments[QUALITYLABEL];
                    Arguments.Remove(QUALITYLABEL);
                }
                Width = Height = -1;
                if (arguments.ContainsKey(SIZE)) {
                    string[] sizeStr = arguments[SIZE].Split('x');
                    Arguments.Remove(SIZE);
                    Width = int.Parse(sizeStr[0]);
                    Height = int.Parse(sizeStr[1]);
                }
                Match codec = Regex.Match(Type, @"codecs\=""(?<video>[^"",]+?)""");
                if (!codec.Success) throw new Exception($"I coudn't parse the codec information out of {Type}");
                VideoCodec = codec.Groups["video"].Value;
            }

            public int Fps {get;}
            public string QualityLabel {get;}
            public int Width {get;}
            public int Height {get;}
            public string VideoCodec {get;}

            public static string GetCsvHeaders(){
                return string.Join(";",
                    ITAG,
                    TYPE,
                    URL,
                    SIGNATURE,
                    "*Extension",
                    "*MimeType",
                    "*PlayerVersion",
                    BITRATE,
                    PROJECTIONTYPE,
                    INDEX,
                    INIT,
                    CLEN,
                    LMT,
                    XTAGS,
                    FPS,
                    QUALITYLABEL,
                    SIZE,
                    "*VideoCodec",
                    "*ArgumentLeftovers");
            }

            public override string ToCsvRow(){
                return string.Join(";",
                    ITag,
                    Type.Replace(';', '|'),
                    Url,
                    S,
                    Extension,
                    MimeType,
                    PlayerVersion,
                    Bitrate,
                    ProjectionType,
                    IndexFrom + " - " + IndexTo,
                    InitFrom + " - " + InitTo,
                    ContentLength,
                    Lmt,
                    string.Join(", ", XTags),
                    Fps,
                    QualityLabel,
                    Width + " x " + Height,
                    VideoCodec,
                    string.Join(", ", Arguments.ToList().ConvertAll(kvp => $"{kvp.Key} = {kvp.Value}")));
            }

            public override string ToString(){
                return $"{ITag} (dash video): {Type}, {QualityLabel}";
            }
        }

        public class DashVideoLive : DashVideo
        {
            public DashVideoLive(Dictionary<string, string> arguments) : base(arguments){
                TargetDurationSec = double.Parse(arguments[TARGETDURATIONSEC]);
                Arguments.Remove(TARGETDURATIONSEC);
            }

            public double TargetDurationSec {get;}

            public new static string GetCsvHeaders(){
                return string.Join(";",
                    ITAG,
                    TYPE,
                    URL,
                    SIGNATURE,
                    "*Extension",
                    "*MimeType",
                    "*PlayerVersion",
                    BITRATE,
                    PROJECTIONTYPE,
                    INDEX,
                    INIT,
                    CLEN,
                    LMT,
                    XTAGS,
                    FPS,
                    QUALITYLABEL,
                    SIZE,
                    "*VideoCodec",
                    TARGETDURATIONSEC,
                    "*ArgumentLeftovers");
            }

            public override string ToCsvRow(){
                return string.Join(";",
                    ITag,
                    Type.Replace(';', '|'),
                    Url,
                    S,
                    Extension,
                    MimeType,
                    PlayerVersion,
                    Bitrate,
                    ProjectionType,
                    IndexFrom + " - " + IndexTo,
                    InitFrom + " - " + InitTo,
                    ContentLength,
                    Lmt,
                    string.Join(", ", XTags),
                    Fps,
                    QualityLabel,
                    Width + " x " + Height,
                    VideoCodec,
                    TargetDurationSec,
                    string.Join(", ", Arguments.ToList().ConvertAll(kvp => $"{kvp.Key} = {kvp.Value}")));
            }
        }

        public class DashVideo3D : DashVideo
        {
            public DashVideo3D(Dictionary<string, string> arguments) : base(arguments){
                StereoLayout = int.Parse(arguments[STEREOLAYOUT]);
                Arguments.Remove(STEREOLAYOUT);
            }

            public int StereoLayout {get;}

            public new static string GetCsvHeaders(){
                return string.Join(";",
                    ITAG,
                    TYPE,
                    URL,
                    SIGNATURE,
                    "*Extension",
                    "*MimeType",
                    "*PlayerVersion",
                    BITRATE,
                    PROJECTIONTYPE,
                    INDEX,
                    INIT,
                    CLEN,
                    LMT,
                    XTAGS,
                    FPS,
                    QUALITYLABEL,
                    SIZE,
                    "*VideoCodec",
                    STEREOLAYOUT,
                    "*ArgumentLeftovers");
            }

            public override string ToCsvRow(){
                return string.Join(";",
                    ITag,
                    Type.Replace(';', '|'),
                    Url,
                    S,
                    Extension,
                    MimeType,
                    PlayerVersion,
                    Bitrate,
                    ProjectionType,
                    IndexFrom + " - " + IndexTo,
                    InitFrom + " - " + InitTo,
                    ContentLength,
                    Lmt,
                    string.Join(", ", XTags),
                    Fps,
                    QualityLabel,
                    Width + " x " + Height,
                    VideoCodec,
                    StereoLayout,
                    string.Join(", ", Arguments.ToList().ConvertAll(kvp => $"{kvp.Key} = {kvp.Value}")));
            }
        }

        public class DashAudio : Dash
        {
            public DashAudio(Dictionary<string, string> arguments) : base(arguments){
                Match codec = Regex.Match(Type, @"codecs\=""(?<audio>[^"",]+?)""");
                if (!codec.Success) throw new Exception($"I coudn't parse the codec information out of {Type}");
                AudioCodec = codec.Groups["audio"].Value;
            }

            public string AudioCodec {get;}

            public static string GetCsvHeaders(){
                return string.Join(";",
                    ITAG,
                    TYPE,
                    URL,
                    SIGNATURE,
                    "*Extension",
                    "*MimeType",
                    "*PlayerVersion",
                    BITRATE,
                    PROJECTIONTYPE,
                    INDEX,
                    INIT,
                    CLEN,
                    LMT,
                    XTAGS,
                    "*AudioCodec",
                    "*ArgumentLeftovers");
            }

            public override string ToCsvRow(){
                return string.Join(";",
                    ITag,
                    Type.Replace(';', '|'),
                    Url,
                    S,
                    Extension,
                    MimeType,
                    PlayerVersion,
                    Bitrate,
                    ProjectionType,
                    IndexFrom + " - " + IndexTo,
                    InitFrom + " - " + InitTo,
                    ContentLength,
                    Lmt,
                    string.Join(", ", XTags),
                    AudioCodec,
                    string.Join(", ", Arguments.ToList().ConvertAll(kvp => $"{kvp.Key} = {kvp.Value}")));
            }

            public override string ToString(){
                return $"{ITag} (dash audio): {Type}, {Bitrate / 1000}kbit/s";
            }
        }

        public class DashAudioLive : DashAudio
        {
            public DashAudioLive(Dictionary<string, string> arguments) : base(arguments){
                TargetDurationSec = double.Parse(arguments[TARGETDURATIONSEC]);
                Arguments.Remove(TARGETDURATIONSEC);
            }

            public double TargetDurationSec {get;}

            public static string GetCsvHeaders(){
                return string.Join(";",
                    ITAG,
                    TYPE,
                    URL,
                    SIGNATURE,
                    "*Extension",
                    "*MimeType",
                    "*PlayerVersion",
                    BITRATE,
                    PROJECTIONTYPE,
                    INDEX,
                    INIT,
                    CLEN,
                    LMT,
                    XTAGS,
                    "*AudioCodec",
                    TARGETDURATIONSEC,
                    "*ArgumentLeftovers");
            }

            public override string ToCsvRow(){
                return string.Join(";",
                    ITag,
                    Type.Replace(';', '|'),
                    Url,
                    S,
                    Extension,
                    MimeType,
                    PlayerVersion,
                    Bitrate,
                    ProjectionType,
                    IndexFrom + " - " + IndexTo,
                    InitFrom + " - " + InitTo,
                    ContentLength,
                    Lmt,
                    string.Join(", ", XTags),
                    AudioCodec,
                    TargetDurationSec,
                    string.Join(", ", Arguments.ToList().ConvertAll(kvp => $"{kvp.Key} = {kvp.Value}")));
            }
        }
    }

    public enum VideoQuality
    {
        Small,
        Medium,
        HD720
    }
}