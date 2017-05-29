﻿using System;
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
    public class IntegrationTests {
        [DeploymentItem("../../TestData")]
        [TestMethod]
        public async Task IntegrationTest(){
            const string VIDEOID = "TWcyIpul8OE";
            const string PLAYLISTID = "RDd2Y4dFVgS8g";
            Uri link = new Uri($"https://www.youtube.com/watch?v={VIDEOID}");
            //Uri link = new Uri($"https://www.youtube.com/watch?v={VIDEOID}&list={PLAYLISTID}");
            //Uri link = new Uri($"https://img.youtube.com/vi/{VIDEOID}/0.jpg");

            // Classify link
            URLResult result = URLUtility.AnalyzeURI(link);

            if (!result.HasFlag(URLResult.IsValid)) return;
            if (result.HasFlag(URLResult.IsPlaylist)) { // You're a playlist, Harry!
                // Not yet implemented
            }else if(result.HasFlag(URLResult.IsVideo)) { // You're a video, Harry!
                string videoId = URLUtility.ExtractVideoID(link);
                Video video = await Video.fromID(videoId);
                if (!video.Success) return;

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

                // Download (Don't do it like this. Do it properly, please)
                byte[] data = await new WebClient().DownloadDataTaskAsync(downloadUri);
                File.WriteAllBytes($"./{title}{extension}", data);
            } else if (result.HasFlag(URLResult.IsImage)) { // You're an image, Harry!
                string videoId = URLUtility.ExtractVideoID(link);

                // Get your download link
                Uri downloadUri = Image.FromID(videoId, ImageType.MaximumResolutionDefault);

                // Download (Don't do it like this. Do it properly, please)
                byte[] data = await new WebClient().DownloadDataTaskAsync(downloadUri);
                File.WriteAllBytes("./image.jpg", data);
            }
        }

        [TestMethod]
        public async Task URLIntegrationTest(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=BaW_jenozKc&t=1s&end=9");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual("youtube-dl test video \"'/\\ä↭𝕐", video.Title);
            Assert.AreEqual(new DateTime(2012, 10, 2), video.UploadedDateTime);
            //Works so far
            //TODO: Complete test transfer from youtube-dl
        }
    }
}