using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Youtuber.Net;
using Youtuber.Net.Youtube;

namespace youtubertest {
    [TestClass]
    public class IntegrationTests {
        public static async Task<bool> RemoteFilePresent(VideoFile videoFile, CookieCollection cookies){
            Uri link = await videoFile.GetDownloadUri();
            HttpWebRequest request = WebRequest.CreateHttp(link);
            request.Method = "HEAD";
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(cookies);
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                return HttpStatusCode.OK.Equals(response.StatusCode);
            }
        }

        [DeploymentItem("../../TestData")]
        [TestMethod]
        public async Task WikiExample(){
            const string VIDEOID = "TWcyIpul8OE";
            const string PLAYLISTID = "RDd2Y4dFVgS8g";
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Uri link = new Uri($"https://www.youtube.com/watch?v={VIDEOID}");
            //Uri link = new Uri($"https://www.youtube.com/watch?v={VIDEOID}&list={PLAYLISTID}");
            //Uri link = new Uri($"https://img.youtube.com/vi/{VIDEOID}/0.jpg");

            // Classify link
            URLResult result = URLUtility.AnalyzeURI(link);

            if (!result.HasFlag(URLResult.IsValid)) return;
            if (result.HasFlag(URLResult.IsPlaylist)) { // You're a playlist, Harry!
                // Not yet implemented
            } else if (result.HasFlag(URLResult.IsVideo)) { // You're a video, Harry!
                string videoId = URLUtility.ExtractVideoID(link);
                Video video = await Video.fromID(videoId);
                if (!video.Success) return;

                // Get video data
                string title = video.Title;
                string Description = video.Description;
                string username = video.User;
                string userId = video.UserID;
                DateTime uploaded = video.UploadedDateTime;
                DateTime requested = video.RequestTime;
                TimeSpan duration = video.Duration;
                List<string> keywords = video.Keywords;
                long views = video.Views;
                long likes = video.Likes;
                long dislikes = video.Dislikes;
                long subscribers = video.Subscribers;

                // Get data on recommendations
                List<Recommendation> recommendations = video.RelatedVideos;
                Recommendation.Video firstRecommendedVideo =
                    recommendations.OfType<Recommendation.Video>().ToList().First();
                string recommendationTitle = firstRecommendedVideo.Title;
                TimeSpan recommendationDuration = firstRecommendedVideo.Duration;
                long recommendationViews = firstRecommendedVideo.Views;
                string recommendationID = firstRecommendedVideo.VideoID;
                Video recommendatedVideo = await Video.fromID(recommendationID);

                // Select your desired download
                List<VideoFile> downloadables = video.ExtractFiles();
                List<VideoFile.DashAudio> audioDownloads = downloadables.OfType<VideoFile.DashAudio>().ToList();
                int highestBitrate = audioDownloads.Max(audio => audio.Bitrate);
                VideoFile.DashAudio bestAudio = audioDownloads.First(audio => audio.Bitrate == highestBitrate);

                // Get your download link
                Uri downloadUri = await bestAudio.GetDownloadUri();
                string extension = bestAudio.Extension;

                // Download
                string path = Path.Combine(basePath, title + extension);
                await new WebClient().DownloadFileTaskAsync(downloadUri, path);
            } else if (result.HasFlag(URLResult.IsImage)) { // You're an image, Harry!
                string videoId = URLUtility.ExtractVideoID(link);

                // Get your download link
                Uri downloadUri = Image.FromID(videoId, ImageType.MaximumResolutionDefault);

                // Download
                string path = Path.Combine(basePath, "image.jpg");
                await new WebClient().DownloadFileTaskAsync(downloadUri, path);
            }
        }

        [TestMethod]
        public async Task GatherData(){
            Video video = await Video.fromID("OpIQNxiKJoE");
            HashSet<string> usedVideoIDs = new HashSet<string>();
            usedVideoIDs.Add(video.VideoID);
            List<string> availableVideoIdBuffer = new List<string>{"b-v-lTtS_os", "FSGfN9rr78Q"};

            List<string> urlKeys = new List<string>{
                "ei",
                "expire",
                "gir",
                "id",
                "initcwndbps",
                "ip",
                "ipbits",
                "itag",
                "keepalive",
                "key",
                "mime",
                "mm",
                "mn",
                "ms",
                "mt",
                "mv",
                "pl",
                "ratebypass",
                "requiressl",
                "signature",
                "source",
                "sparams",
                "beids",
                "clen",
                "noclen",
                "cmbypass",
                "compress",
                "hang",
                "live",
                "lmt",
                "dur",
                "gcr",
                "hightc",
                "pcm2",
                "pcm2cms"
            };
            string csvHeaderSuffix =
                $";VideoID;VideoTitle;Uploader;DownloadURL;{string.Join(";", urlKeys)};AdditionalUrlParams\n";
            File.Delete("./NormalVideo.csv");
            StreamWriter normalVideoStrWriter = new StreamWriter(File.OpenWrite("./NormalVideo.csv"));
            File.Delete("./DashVideo.csv");
            StreamWriter dashVideoStrWriter = new StreamWriter(File.OpenWrite("./DashVideo.csv"));
            File.Delete("./DashVideoLive.csv");
            StreamWriter dashVideoLiveStrWriter = new StreamWriter(File.OpenWrite("./DashVideoLive.csv"));
            File.Delete("./DashVideo3D.csv");
            StreamWriter dashVideo3DStrWriter = new StreamWriter(File.OpenWrite("./DashVideo3D.csv"));
            File.Delete("./DashAudio.csv");
            StreamWriter dashAudioStrWriter = new StreamWriter(File.OpenWrite("./DashAudio.csv"));
            File.Delete("./DashAudioLive.csv");
            StreamWriter dashAudioLiveStrWriter = new StreamWriter(File.OpenWrite("./DashAudioLive.csv"));
            File.Delete("./URL.csv");
            StreamWriter urlStrWriter = new StreamWriter(File.OpenWrite("./URL.csv"));
            normalVideoStrWriter.Write(VideoFile.Normal.GetCsvHeaders() + csvHeaderSuffix);
            dashVideoStrWriter.Write(VideoFile.DashVideo.GetCsvHeaders() + csvHeaderSuffix);
            dashVideoLiveStrWriter.Write(VideoFile.DashVideoLive.GetCsvHeaders() + csvHeaderSuffix);
            dashVideo3DStrWriter.Write(VideoFile.DashVideo3D.GetCsvHeaders() + csvHeaderSuffix);
            dashAudioStrWriter.Write(VideoFile.DashAudio.GetCsvHeaders() + csvHeaderSuffix);
            dashAudioLiveStrWriter.Write(VideoFile.DashAudioLive.GetCsvHeaders() + csvHeaderSuffix);
            urlStrWriter.Write("VideoFileType;VideoID;Url;" + string.Join(";", urlKeys) + ";AdditionalUrlParams\n");

            int count = 1;
            while (count < 500) { // 100 is probably more than enough
                Debug.Print($"Run {count}: {video.VideoID} - \"{video.Title}\"");
                List<VideoFile> videoFiles = video.ExtractFiles();
                List<VideoFile.Normal> nonDash = videoFiles.OfType<VideoFile.Normal>().ToList();
                List<VideoFile.DashVideo> dashVideo = videoFiles.OfType<VideoFile.DashVideo>().ToList();
                List<VideoFile.DashVideoLive> dashVideoLive = videoFiles.OfType<VideoFile.DashVideoLive>().ToList();
                List<VideoFile.DashVideo3D> dashVideo3D = videoFiles.OfType<VideoFile.DashVideo3D>().ToList();
                dashVideo.RemoveAll(dv => dashVideoLive.Contains(dv) || dashVideo3D.Contains(dv));
                List<VideoFile.DashAudio> dashAudio = videoFiles.OfType<VideoFile.DashAudio>().ToList();
                List<VideoFile.DashAudioLive> dashAudioLive = videoFiles.OfType<VideoFile.DashAudioLive>().ToList();
                dashAudio.RemoveAll(dv => dashAudioLive.Contains(dv));
                foreach (VideoFile.Normal vf in nonDash) {
                    string url = (await vf.GetDownloadUri()).AbsoluteUri;
                    Dictionary<string, string> dictionary =
                        URLUtility.ExtractParameters(Regex.Match(url, @"(?<=\?).+?$").Value);
                    string urlParams = DictToCsv(dictionary, urlKeys);
                    string urlLeftovers = DictToString(dictionary);
                    normalVideoStrWriter.Write(vf.ToCsvRow() +
                                               $";{video.VideoID};{video.Title.Replace(';', ':')};{video.User};{url}{urlParams};{urlLeftovers}\n");
                    normalVideoStrWriter.Flush();
                    urlStrWriter.Write($"normal;{video.VideoID};{url}{urlParams};{urlLeftovers}\n");
                    urlStrWriter.Flush();
                }
                foreach (VideoFile.DashVideo vf in dashVideo) {
                    string url = (await vf.GetDownloadUri()).AbsoluteUri;
                    Dictionary<string, string> dictionary =
                        URLUtility.ExtractParameters(Regex.Match(url, @"(?<=\?).+?$").Value);
                    string urlParams = DictToCsv(dictionary, urlKeys);
                    string urlLeftovers = DictToString(dictionary);
                    dashVideoStrWriter.Write(vf.ToCsvRow() +
                                             $";{video.VideoID};{video.Title.Replace(';', ':')};{video.User};{url}{urlParams};{urlLeftovers}\n");
                    dashVideoStrWriter.Flush();
                    urlStrWriter.Write($"dash video;{video.VideoID};{url}{urlParams};{urlLeftovers}\n");
                    urlStrWriter.Flush();
                }
                foreach (VideoFile.DashVideoLive vf in dashVideoLive) {
                    string url = (await vf.GetDownloadUri()).AbsoluteUri;
                    Dictionary<string, string> dictionary =
                        URLUtility.ExtractParameters(Regex.Match(url, @"(?<=\?).+?$").Value);
                    string urlParams = DictToCsv(dictionary, urlKeys);
                    string urlLeftovers = DictToString(dictionary);
                    dashVideoLiveStrWriter.Write(vf.ToCsvRow() +
                                                 $";{video.VideoID};{video.Title.Replace(';', ':')};{video.User};{url}{urlParams};{urlLeftovers}\n");
                    dashVideoLiveStrWriter.Flush();
                    urlStrWriter.Write($"dash video live;{video.VideoID};{url}{urlParams};{urlLeftovers}\n");
                    urlStrWriter.Flush();
                }
                foreach (VideoFile.DashVideo3D vf in dashVideo3D) {
                    string url = (await vf.GetDownloadUri()).AbsoluteUri;
                    Dictionary<string, string> dictionary =
                        URLUtility.ExtractParameters(Regex.Match(url, @"(?<=\?).+?$").Value);
                    string urlParams = DictToCsv(dictionary, urlKeys);
                    string urlLeftovers = DictToString(dictionary);
                    dashVideo3DStrWriter.Write(vf.ToCsvRow() +
                                               $";{video.VideoID};{video.Title.Replace(';', ':')};{video.User};{url}{urlParams};{urlLeftovers}\n");
                    dashVideo3DStrWriter.Flush();
                    urlStrWriter.Write($"dash video 3d;{video.VideoID};{url}{urlParams};{urlLeftovers}\n");
                    urlStrWriter.Flush();
                }
                foreach (VideoFile.DashAudio vf in dashAudio) {
                    string url = (await vf.GetDownloadUri()).AbsoluteUri;
                    Dictionary<string, string> dictionary =
                        URLUtility.ExtractParameters(Regex.Match(url, @"(?<=\?).+?$").Value);
                    string urlParams = DictToCsv(dictionary, urlKeys);
                    string urlLeftovers = DictToString(dictionary);
                    dashAudioStrWriter.Write(vf.ToCsvRow() +
                                             $";{video.VideoID};{video.Title.Replace(';', ':')};{video.User};{url}{urlParams};{urlLeftovers}\n");
                    dashAudioStrWriter.Flush();
                    urlStrWriter.Write($"dash audio;{video.VideoID};{url}{urlParams};{urlLeftovers}\n");
                    urlStrWriter.Flush();
                }
                foreach (VideoFile.DashAudioLive vf in dashAudioLive) {
                    string url = (await vf.GetDownloadUri()).AbsoluteUri;
                    Dictionary<string, string> dictionary =
                        URLUtility.ExtractParameters(Regex.Match(url, @"(?<=\?).+?$").Value);
                    string urlParams = DictToCsv(dictionary, urlKeys);
                    string urlLeftovers = DictToString(dictionary);
                    dashAudioLiveStrWriter.Write(vf.ToCsvRow() +
                                                 $";{video.VideoID};{video.Title.Replace(';', ':')};{video.User};{url}{urlParams};{urlLeftovers}\n");
                    dashAudioLiveStrWriter.Flush();
                    urlStrWriter.Write($"dash audio live;{video.VideoID};{url}{urlParams};{urlLeftovers}\n");
                    urlStrWriter.Flush();
                }

                string newVideoId = string.Empty;
                try {
                    List<Recommendation.Video> recommendations = video
                        .RelatedVideos.Where(rec => !usedVideoIDs.Contains(rec.VideoID)).OfType<Recommendation.Video>()
                        .ToList();
                    recommendations.RemoveAll(v => v.Username.Equals(video.User));
                    List<string> newIds = recommendations.ConvertAll(rec => rec.VideoID);
                    availableVideoIdBuffer.AddRange(newIds);
                    newVideoId = availableVideoIdBuffer.First(id => !usedVideoIDs.Contains(id));
                } catch (Exception) { break; }
                video = await Video.fromID(newVideoId);
                usedVideoIDs.Add(video.VideoID);
                count++;
            }
            Debug.Print($"Ran through {count} videos");
            normalVideoStrWriter.Dispose();
            dashVideoStrWriter.Dispose();
            dashAudioStrWriter.Dispose();
        }

        private string DictToCsv(Dictionary<string, string> dictionary, List<string> keys){
            string result = string.Empty;
            foreach (string key in keys) {
                result += ";";
                if (dictionary.ContainsKey(key)) {
                    result += dictionary[key];
                    dictionary.Remove(key);
                }
            }
            return result;
        }

        private string DictToString(Dictionary<string, string> dictionary){
            string result = string.Empty;
            foreach (KeyValuePair<string, string> keyValuePair in dictionary)
                result += $"&{keyValuePair.Key}={keyValuePair.Value}";
            return result;
        }

        [TestMethod]
        public async Task DownloadAvailable(){
            foreach (string id in new[]{"FffTJk-gFKc", "OEVzPCY2T-g", "tlfAQ33YE1A"}) {
                Video video = await Video.fromID(id);
                foreach (VideoFile downloadable in video.ExtractFiles())
                    Assert.IsTrue(await RemoteFilePresent(downloadable, video.Cookies));
            }
        }
    }
}