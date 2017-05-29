using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace youtuber.Net.Youtube
{
    public abstract class VideoFile
    {
        internal VideoFile(int iTag, Container defaultContainer){
            ITag = iTag;
            DefaultContainer = defaultContainer;
            Arguments = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Arguments {get;}
        public int ITag {get;}
        public Container DefaultContainer {get;}

        public string Extension
        {
            get
            {
                switch (DefaultContainer) {
                    case Container.FLV:
                        return ".flv";
                    case Container.M4A:
                        return ".m4a";
                    case Container.MP4:
                        return ".mp4";
                    case Container.ThreeGP:
                        return ".3gp";
                    case Container.WebM:
                        return ".webm";
                    default:
                        return string.Empty;
                }
            }
        }

        protected string PlayerVersion {get; private set;}

        internal VideoFile ConsumeFormatString(string rawFormat){
            foreach (string keyvaluepair in rawFormat.Split('&')) {
                string key = WebUtility.UrlDecode(keyvaluepair.Split('=')[0]);
                string value = WebUtility.UrlDecode(WebUtility.UrlDecode(keyvaluepair.Split('=')[1]));
                Arguments.Add(key, value);
            }
            return this;
        }

        public static VideoFile FromFormatString(string format, string playerVersion){
            int itag = int.Parse(Regex.Match(format, @"(?<=itag\=)\d+?(?=&|$)").Value);
            VideoFile videoFile = null;
            if (NonDash.Available.Contains(itag)) videoFile = NonDash.FromITag(itag);
            else if (
                DashVideo.Available.Contains(itag)) videoFile = DashVideo.FromITag(itag);
            else if (
                DashAudio.Available.Contains(itag)) videoFile = DashAudio.FromITag(itag);
            if (videoFile == null) throw new Exception("I don't know the format corresponding to ITag " + itag);
            videoFile.PlayerVersion = playerVersion;
            return videoFile.ConsumeFormatString(format);
        }

        public abstract Task<Uri> GetDownloadUri();

        public override string ToString(){
            return $"{ITag} : {DefaultContainer}";
        }

        public class NonDash : VideoFile
        {
            internal static int[] Available = {
                5, 6, 13, 17, 18, 22, 34, 35, 36, 37, 38, 43, 44, 45, 46, 59, 78, 82, 83, 84, 85, 91, 92, 93, 94, 95,
                96, 100, 101, 102, 132, 151
            };

            private NonDash(int iTag, Container defaultContainer, int videoResolution, int fps,
                            VideoEncoding videoEncoding,
                            VideoProfile videoProfile, double videoBitrateMinimum, double videoBitrateMaximum,
                            AudioEncoding audioEncoding, int audioBitrate) : base(iTag, defaultContainer){
                VideoResolution = videoResolution;
                FramesPerSecond = fps;
                VideoEncoding = videoEncoding;
                VideoProfile = videoProfile;
                VideoBitrateMinimum = videoBitrateMinimum;
                VideoBitrateMaximum = videoBitrateMaximum;
                AudioEncoding = audioEncoding;
                AudioBitrate = audioBitrate;
            }

            public int VideoResolution {get;}
            public int FramesPerSecond {get;}
            public VideoEncoding VideoEncoding {get;}
            public VideoProfile VideoProfile {get;}
            public double VideoBitrateMinimum {get;}
            public double VideoBitrateMaximum {get;}
            public AudioEncoding AudioEncoding {get;}
            public int AudioBitrate {get;}

            internal static VideoFile FromITag(int iTag){
                // * = Data incomplete(mainly bitrate and video profile)
                switch (iTag) { //               itag,    format        ,resol,fps,      encoding(vid)       ,     profile(vid)    ,bitrMin,bitrMax, encoding(audio),  bitr(audio)
                    case 5:   return new NonDash(iTag, Container.FLV,     240, 24, VideoEncoding.H263,        VideoProfile.Simple,   0.175, 0.175, AudioEncoding.MP3,     64); //*
                    case 6:   return new NonDash(iTag, Container.FLV,     270, 24, VideoEncoding.H263,        VideoProfile.Simple,   0.175, 0.175, AudioEncoding.MP3,     64); //*
                    case 13:  return new NonDash(iTag, Container.ThreeGP, 144, 24, VideoEncoding.MPEG4Visual, VideoProfile.Simple,   0.05,  0.05,  AudioEncoding.AAC,     24); //*
                    case 17:  return new NonDash(iTag, Container.ThreeGP, 144, 24, VideoEncoding.MPEG4Visual, VideoProfile.Simple,   0.05,  0.05,  AudioEncoding.AAC,     24);
                    case 18:  return new NonDash(iTag, Container.MP4,     360, 24, VideoEncoding.H264,        VideoProfile.Baseline, 5,     5,     AudioEncoding.AAC,     96);
                    case 22:  return new NonDash(iTag, Container.MP4,     720, 24, VideoEncoding.H264,        VideoProfile.High,     2,     3,     AudioEncoding.AAC,    192);
                    case 34:  return new NonDash(iTag, Container.FLV,     360, 24, VideoEncoding.H264,        VideoProfile.Simple,   0.5,   0.75,  AudioEncoding.AAC,    128); //*
                    case 35:  return new NonDash(iTag, Container.FLV,     480, 24, VideoEncoding.H264,        VideoProfile.Simple,   0.8,   1.25,  AudioEncoding.AAC,    128); //*
                    case 36:  return new NonDash(iTag, Container.ThreeGP, 240, 24, VideoEncoding.MPEG4Visual, VideoProfile.Simple,   0.175, 0.175, AudioEncoding.AAC,     32);
                    case 37:  return new NonDash(iTag, Container.MP4,    1080, 24, VideoEncoding.H264,        VideoProfile.Simple,   0.8,   1.25,  AudioEncoding.AAC,    192); //*
                    case 38:  return new NonDash(iTag, Container.MP4,    3072, 24, VideoEncoding.H264,        VideoProfile.Simple,   2.5,   6,     AudioEncoding.AAC,    192); //*
                    case 43:  return new NonDash(iTag, Container.WebM,    360, 24, VideoEncoding.VP8,         VideoProfile.Unknown,  2.5,   6,     AudioEncoding.Vorbis, 128);
                    case 44:  return new NonDash(iTag, Container.WebM,    480, 24, VideoEncoding.VP8,         VideoProfile.Unknown,  2.5,   6,     AudioEncoding.Vorbis, 128); //*
                    case 45:  return new NonDash(iTag, Container.WebM,    720, 24, VideoEncoding.VP8,         VideoProfile.Unknown,  2.5,   6,     AudioEncoding.Vorbis, 192); //*
                    case 46:  return new NonDash(iTag, Container.WebM,   1080, 24, VideoEncoding.VP8,         VideoProfile.Unknown,  2.5,   6,     AudioEncoding.Vorbis, 192); //*
                    case 59:  return new NonDash(iTag, Container.MP4,     480, 24, VideoEncoding.VP8,         VideoProfile.Unknown,  0.8,   1.25,  AudioEncoding.Vorbis, 128); //*
                    case 78:  return new NonDash(iTag, Container.MP4,     480, 24, VideoEncoding.VP8,         VideoProfile.Unknown,  0.8,   1.25,  AudioEncoding.Vorbis, 128); //*
					// 3D
                    case 82:  return new NonDash(iTag, Container.MP4,     360, 24, VideoEncoding.H264,        VideoProfile.Simple,   0.5,   0.75,  AudioEncoding.AAC,    128); //*
                    case 83:  return new NonDash(iTag, Container.MP4,     480, 24, VideoEncoding.H264,        VideoProfile.Simple,   0.5,   0.75,  AudioEncoding.AAC,    128); //*
                    case 84:  return new NonDash(iTag, Container.MP4,     720, 24, VideoEncoding.H264,        VideoProfile.Simple,   0.5,   0.75,  AudioEncoding.AAC,    192); //*
                    case 85:  return new NonDash(iTag, Container.MP4,    1080, 24, VideoEncoding.H264,        VideoProfile.Simple,   0.5,   0.75,  AudioEncoding.AAC,    192); //*
					// live streams
                    case 91:  return new NonDash(iTag, Container.MP4,     144, 24, VideoEncoding.H264,        VideoProfile.Main,     0.1,   0.1,   AudioEncoding.AAC,     48);
                    case 92:  return new NonDash(iTag, Container.MP4,     240, 24, VideoEncoding.H264,        VideoProfile.Main,     0.1,   0.3,   AudioEncoding.AAC,     48);
                    case 93:  return new NonDash(iTag, Container.MP4,     360, 24, VideoEncoding.H264,        VideoProfile.Main,     0.5,   1,     AudioEncoding.AAC,    128);
                    case 94:  return new NonDash(iTag, Container.MP4,     480, 24, VideoEncoding.H264,        VideoProfile.Main,     0.8,   1.25,  AudioEncoding.AAC,    128);
                    case 95:  return new NonDash(iTag, Container.MP4,     720, 24, VideoEncoding.H264,        VideoProfile.Main,     1.5,   3,     AudioEncoding.AAC,    256);
                    case 96:  return new NonDash(iTag, Container.MP4,    1080, 24, VideoEncoding.H264,        VideoProfile.High,     2.5,   6,     AudioEncoding.AAC,    256);
					// 3D
                    case 100: return new NonDash(iTag, Container.WebM,    360, 24, VideoEncoding.VP8,         VideoProfile.Simple,   2.5,   6,     AudioEncoding.AAC,    128); //*
                    case 101: return new NonDash(iTag, Container.WebM,    480, 24, VideoEncoding.VP8,         VideoProfile.Simple,   2.5,   6,     AudioEncoding.AAC,    192); //*
                    case 102: return new NonDash(iTag, Container.WebM,    720, 24, VideoEncoding.VP8,         VideoProfile.Simple,   2.5,   6,     AudioEncoding.AAC,    192); //*
					// live streams
                    case 132: return new NonDash(iTag, Container.MP4,     240, 24, VideoEncoding.H264,        VideoProfile.Main,     0.1,   0.3,   AudioEncoding.AAC,     48); //*
                    case 151: return new NonDash(iTag, Container.MP4,      72, 24, VideoEncoding.H264,        VideoProfile.Main,     0.1,   0.3,   AudioEncoding.AAC,     24); //*
                    default: throw new ArgumentException($"No mapping for itag {iTag} implemented", nameof(iTag));
                }
            }

            public override async Task<Uri> GetDownloadUri(){
                string url = Arguments["url"];
                if (Arguments.ContainsKey("s")) {
                    string signature = WebUtility.UrlEncode(Arguments["s"]);
                    url += $"&signature={signature}";
                }
                return new Uri(url);
            }

            public override string ToString(){
                return $"{ITag} (normal video): {DefaultContainer}, {VideoResolution}p";
            }
        }

        public class DashVideo : VideoFile
        {
            internal static int[] Available = {
                160, 133, 134, 135, 136, 298, 137, 299, 264, 266, 138, 278, 242, 243, 244, 247, 248, 271, 313, 272, 302,
                303, 308, 315, 330, 331, 332, 333, 334, 335, 336, 337
            };

            private DashVideo(int iTag, Container defaultContainer, int videoResolution, int fps,
                              VideoEncoding videoEncoding,
                              VideoProfile videoProfile, double videoBitrateMinimum, double videoBitrateMaximum) : base(
                iTag, defaultContainer){
                VideoResolution = videoResolution;
                FramesPerSecond = fps;
                VideoEncoding = videoEncoding;
                VideoProfile = videoProfile;
                VideoBitrateMinimum = videoBitrateMinimum;
                VideoBitrateMaximum = videoBitrateMaximum;
            }

            public int VideoResolution {get;}
            public int FramesPerSecond {get;}
            public VideoEncoding VideoEncoding {get;}
            public VideoProfile VideoProfile {get;}
            public double VideoBitrateMinimum {get;}
            public double VideoBitrateMaximum {get;}

            internal static VideoFile FromITag(int iTag){
                // * = Data incomplete(mainly bitrate and video profile)
                switch (iTag) { //                 itag,    format     ,resol,fps,   encoding(vid)   ,     profile(vid)       ,bitrMin,bitrMax
                    case 133: return new DashVideo(iTag, Container.MP4,   240, 24, VideoEncoding.H264, VideoProfile.Main,       0.2,   0.3 );
                    case 134: return new DashVideo(iTag, Container.MP4,   360, 24, VideoEncoding.H264, VideoProfile.Main,       0.3,   0.4 );
                    case 135: return new DashVideo(iTag, Container.MP4,   480, 24, VideoEncoding.H264, VideoProfile.Main,       0.5,   1   );
                    case 136: return new DashVideo(iTag, Container.MP4,   720, 24, VideoEncoding.H264, VideoProfile.Main,       1,     1.5 );
                    case 137: return new DashVideo(iTag, Container.MP4,  1080, 24, VideoEncoding.H264, VideoProfile.High,       2.5,   3   );
                    case 138: return new DashVideo(iTag, Container.MP4,  4320, 24, VideoEncoding.H264, VideoProfile.High,      13.5,  25   );
                    case 160: return new DashVideo(iTag, Container.MP4,   144, 15, VideoEncoding.H264, VideoProfile.Main,       0.1,   0.1 );
                    case 167: return new DashVideo(iTag, Container.WebM,  360, 24, VideoEncoding.VP8,  VideoProfile.Profile_0,  0.25,  0.25); //*
                    case 168: return new DashVideo(iTag, Container.WebM,  480, 24, VideoEncoding.VP8,  VideoProfile.Profile_0,  0.5,   0.5 ); //*
                    case 169: return new DashVideo(iTag, Container.WebM,  720, 24, VideoEncoding.VP8,  VideoProfile.Profile_0,  0.7,   0.8 ); //*
                    case 170: return new DashVideo(iTag, Container.WebM, 1080, 24, VideoEncoding.VP8,  VideoProfile.Profile_0,  1.5,   1.5 ); //*
                    case 212: return new DashVideo(iTag, Container.MP4,   480, 24, VideoEncoding.H264, VideoProfile.Main,       0.5,   1   ); //*
                    case 218: return new DashVideo(iTag, Container.WebM,  480, 24, VideoEncoding.VP8,  VideoProfile.Profile_0,  0.5,   0.5 ); //*
                    case 219: return new DashVideo(iTag, Container.WebM,  480, 24, VideoEncoding.VP8,  VideoProfile.Profile_0,  0.5,   0.5 ); //*
                    case 242: return new DashVideo(iTag, Container.WebM,  240, 24, VideoEncoding.VP9,  VideoProfile.Profile_0,  0.1,   0.2 );
                    case 243: return new DashVideo(iTag, Container.WebM,  360, 24, VideoEncoding.VP9,  VideoProfile.Profile_0,  0.25,  0.25);
                    case 244: return new DashVideo(iTag, Container.WebM,  480, 24, VideoEncoding.VP9,  VideoProfile.Profile_0,  0.5,   0.5 );
                    case 245: return new DashVideo(iTag, Container.WebM,  480, 24, VideoEncoding.VP9,  VideoProfile.Profile_0,  0.5,   0.5 ); //*
                    case 246: return new DashVideo(iTag, Container.WebM,  480, 24, VideoEncoding.VP9,  VideoProfile.Profile_0,  0.5,   0.5 ); //*
                    case 247: return new DashVideo(iTag, Container.WebM,  720, 24, VideoEncoding.VP9,  VideoProfile.Profile_0,  0.7,   0.8 );
                    case 248: return new DashVideo(iTag, Container.WebM, 1080, 24, VideoEncoding.VP9,  VideoProfile.Profile_0,  1.5,   1.5 );
                    case 264: return new DashVideo(iTag, Container.MP4,  1440, 24, VideoEncoding.H264, VideoProfile.High,       4,     4.5 );
                    case 266: return new DashVideo(iTag, Container.MP4,  2160, 24, VideoEncoding.H264, VideoProfile.High,      12.5,  16   );
                    case 271: return new DashVideo(iTag, Container.WebM, 1440, 24, VideoEncoding.VP9,  VideoProfile.Profile_0,  9,     9   );
                    case 272: return new DashVideo(iTag, Container.WebM, 4320, 24, VideoEncoding.VP9,  VideoProfile.Profile_0, 20,    25   );
                    case 278: return new DashVideo(iTag, Container.WebM,  144, 15, VideoEncoding.VP9,  VideoProfile.Profile_0,  0.08,  0.08);
                    case 298: return new DashVideo(iTag, Container.MP4,   720, 60, VideoEncoding.H264, VideoProfile.Main,       3,     3.5 );
                    case 299: return new DashVideo(iTag, Container.MP4,  1080, 60, VideoEncoding.H264, VideoProfile.High,       5.5,   5.5 );
                    case 302: return new DashVideo(iTag, Container.WebM,  720, 60, VideoEncoding.VP9,  VideoProfile.Profile_0,  2.5,   2.5 );
                    case 303: return new DashVideo(iTag, Container.WebM, 1080, 60, VideoEncoding.VP9,  VideoProfile.Profile_0,  5,     5   );
                    case 308: return new DashVideo(iTag, Container.WebM, 1440, 60, VideoEncoding.VP9,  VideoProfile.Profile_0, 10,    10   );
                    case 313: return new DashVideo(iTag, Container.WebM, 2160, 24, VideoEncoding.VP9,  VideoProfile.Profile_0, 13,    15   );
                    case 315: return new DashVideo(iTag, Container.WebM, 2160, 60, VideoEncoding.VP9,  VideoProfile.Profile_0, 20,    25   );
                    case 330: return new DashVideo(iTag, Container.WebM,  144, 60, VideoEncoding.VP9,  VideoProfile.Profile_1,  0.08,  0.08);
                    case 331: return new DashVideo(iTag, Container.WebM,  260, 60, VideoEncoding.VP9,  VideoProfile.Profile_1,  0.1,   0.15);
                    case 332: return new DashVideo(iTag, Container.WebM,  360, 60, VideoEncoding.VP9,  VideoProfile.Profile_1,  0.25,  0.25);
                    case 333: return new DashVideo(iTag, Container.WebM,  480, 60, VideoEncoding.VP9,  VideoProfile.Profile_1,  0.5,   0.5 );
                    case 334: return new DashVideo(iTag, Container.WebM,  720, 60, VideoEncoding.VP9,  VideoProfile.Profile_1,  1,     1   );
                    case 335: return new DashVideo(iTag, Container.WebM, 1080, 60, VideoEncoding.VP9,  VideoProfile.Profile_1,  1.5,   2   );
                    case 336: return new DashVideo(iTag, Container.WebM, 1440, 60, VideoEncoding.VP9,  VideoProfile.Profile_1,  5,     7   );
                    case 337: return new DashVideo(iTag, Container.WebM, 2160, 60, VideoEncoding.VP9,  VideoProfile.Profile_1, 12,    14   );
                    default: throw new ArgumentException($"No mapping for itag {iTag} implemented", nameof(iTag));
                }
            }

            public override async Task<Uri> GetDownloadUri(){
                string url = Arguments["url"];
                if (Arguments.ContainsKey("s")) {
                    string signature = WebUtility.UrlEncode(Arguments["s"]);
                    signature = (await Decipherer.GetDecipherer(PlayerVersion)).Decipher(signature);
                    url += $"&signature={signature}";
                }
                if (!Regex.IsMatch(url, @"&ratebypass")) url += "&ratebypass=yes";
                return new Uri(url);
            }

            public override string ToString(){
                return $"{ITag} (dash video): {DefaultContainer}, {VideoResolution}p";
            }
        }

        public class DashAudio : VideoFile
        {
            internal static int[] Available = {139, 140, 141, 171, 249, 250, 251, 256, 258, 325, 328};

            private DashAudio(int iTag, Container defaultContainer, AudioEncoding audioEncoding,
                              int audioBitrate) : base(iTag, defaultContainer){
                AudioEncoding = audioEncoding;
                AudioBitrate = audioBitrate;
            }

            public AudioEncoding AudioEncoding {get;}
            public int AudioBitrate {get;}

            internal static VideoFile FromITag(int iTag){
                // * = Data incomplete(mainly bitrate and video profile)
                switch (iTag) { //                 itag,    format      ,   encoding(audio)   ,bitr
                    case 139: return new DashAudio(iTag, Container.M4A,  AudioEncoding.AAC,    48); //*
                    case 140: return new DashAudio(iTag, Container.M4A,  AudioEncoding.AAC,    128);
                    case 141: return new DashAudio(iTag, Container.M4A,  AudioEncoding.AAC,    256); //*
                    case 171: return new DashAudio(iTag, Container.WebM, AudioEncoding.Vorbis, 128);
                    case 249: return new DashAudio(iTag, Container.WebM, AudioEncoding.Orpus,   48);
                    case 250: return new DashAudio(iTag, Container.WebM, AudioEncoding.Orpus,   48);
                    case 251: return new DashAudio(iTag, Container.WebM, AudioEncoding.Orpus,  160);
                    case 256: return new DashAudio(iTag, Container.M4A,  AudioEncoding.AAC,    128); //*
                    case 258: return new DashAudio(iTag, Container.M4A,  AudioEncoding.AAC,    128); //*
                    case 325: return new DashAudio(iTag, Container.M4A,  AudioEncoding.DTSE,   128); //*
                    case 328: return new DashAudio(iTag, Container.M4A,  AudioEncoding.EC_3,   128); //*
                    default: throw new ArgumentException($"No mapping for itag {iTag} implemented", nameof(iTag));
                }
            }

            public override async Task<Uri> GetDownloadUri(){
                string url = Arguments["url"];
                if (Arguments.ContainsKey("s")) {
                    string signature = WebUtility.UrlEncode(Arguments["s"]);
                    signature = (await Decipherer.GetDecipherer(PlayerVersion)).Decipher(signature);
                    url += $"&signature={signature}";
                }
                if (!Regex.IsMatch(url, @"&ratebypass")) url += "&ratebypass=yes";
                return new Uri(url);
            }

            public override string ToString(){
                return $"{ITag} (dash audio): {DefaultContainer}, {AudioBitrate}kbit/s";
            }
        }
    }

    public enum Container
    {
        ThreeGP,
        FLV,
        MP4,
        M4A,
        WebM,
        Unknown
    }

    public enum VideoEncoding
    {
        AVC,
        H263,
        H264,
        MPEG4Visual,
        SorensonH263,
        VP8,
        VP9,
        Unknown
    }

    public enum VideoProfile
    {
        Baseline,
        High,
        Main,
        Simple,
        Profile_0,
        Profile_1,
        Unknown
    }

    public enum AudioEncoding
    {
        MP3,
        AAC,
        Vorbis,
        Orpus,
        DTSE,
        EC_3,
        Unknown
    }
}