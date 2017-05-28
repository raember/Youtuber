using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using youtuber.net;
using youtuber.Net.Youtube;

namespace youtubertest
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public async Task IntegrationTest(){
            Uri link = new Uri("https://www.youtube.com/watch?v=TWcyIpul8OE");

            // Classify link
            URLResult result = URLUtility.AnalyzeURI(link);

            if (!result.HasFlag(URLResult.isValid)) return;
            if (result.HasFlag(URLResult.isVideo)) { // You're a video, Harry!
                string VideoID = URLUtility.ExtractVideoID(link);
                Video video = await Video.fromID(VideoID);

                // Get video data
                string title = video.Title;
                string htmlDescription = video.Description;
                string username = video.Username;
                DateTime uploaded = video.UploadedDateTime;
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
                int highestBitrate = audioDownloads.Max(audio => audio.AudioBitrate);
                VideoFile.DashAudio bestAudio = audioDownloads.First(audio => audio.AudioBitrate == highestBitrate);

                // Get your download link
                Uri downloadUri = await bestAudio.GetDownloadUri();
                string extension = bestAudio.Extension;
                string basePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

                // Download (Don't do it like this. Do it properly, please)
                byte[] data = await new WebClient().DownloadDataTaskAsync(downloadUri);
                File.WriteAllBytes(Path.Combine(basePath, $"{title}{extension}"), data);
            } else if (result.HasFlag(URLResult.isPlaylist)) { // You're a playlist, Harry!
                // Not yet implemented
            } else if (result.HasFlag(URLResult.isImage)) { // You're an image, Harry!
                // Not yet implemented
            }
        }
    }
}