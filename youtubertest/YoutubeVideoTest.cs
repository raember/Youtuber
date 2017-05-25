using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using youtuber.net;

namespace youtubertest
{
    [TestClass]
    public class YoutubeVideoTest
    {
        private const string VIDEOID = "TWcyIpul8OE";
        private Mock<HttpWebRequest> httpWebRequestMock = new Mock<HttpWebRequest>();
        private Mock<HttpWebResponse> httpWebResponseMock = new Mock<HttpWebResponse>();
        private string basePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        private FileStream fileStream;

        [TestInitialize]
        public void Initialize(){
            httpWebRequestMock.Setup(request => request.GetResponseAsync()).ReturnsAsync(httpWebResponseMock.Object);
            httpWebRequestMock.SetupAllProperties();
            httpWebRequestMock.SetupGet(request => request.Headers).Returns(new WebHeaderCollection());
            InternetSite.DefaultHttpWebRequest = httpWebRequestMock.Object;
        }

        private void SetMock(string filePathForStream){
            fileStream = new FileInfo(filePathForStream).OpenRead();
            httpWebResponseMock.Setup(response => response.GetResponseStream()).Returns(fileStream);
        }

        [TestCleanup]
        public void Cleanup(){
            fileStream.Dispose();
            InternetSite.DefaultHttpWebRequest = null;
        }

        [TestMethod]
        public void GetVideoData(){
            SetMock(Path.Combine(basePath, "HoloceneVideo.html"));

            Task<Video> task = Video.fromID(VIDEOID);
            Video video = task.Result;
            Assert.IsTrue(video.Success);
            Assert.AreEqual(VIDEOID, video.VideoID);
            Assert.AreEqual("Bon Iver - Holocene (Official Music Video)", video.Title);
            Assert.AreEqual(DateTime.Parse("17.08.2011"), video.UploadedDateTime);
            Assert.AreEqual(
                "Director: NABIL (NABIL.com)<br />" +
                "Producer: Jill Hammer<br />" +
                "Production Company: NE Direction<br />" +
                "Editor: Isaac Hagy<br />" +
                "DOP: Larkin Sieple<br />" +
                "<br />" +
                "boniver.org<br />" +
                "jagjaguwar.com",
                video.Description);
            Assert.AreEqual("boniver", video.Username);
            Assert.AreEqual(31278815, video.Views);
            Assert.AreEqual(171340, video.Likes);
            Assert.AreEqual(3198, video.Dislikes);
            Assert.AreEqual(235000, video.Subscribers);
            List<Recommendation> relatedVideos = video.RelatedVideos;
            Assert.IsTrue(relatedVideos.Count == 20);
            Recommendation recomm = relatedVideos.First();
            Assert.IsTrue(recomm.GetType() == typeof(Recommendation.Playlist));
            Recommendation.Playlist playlist = (Recommendation.Playlist)recomm;
            Assert.IsTrue(playlist.Title.EndsWith("Bon Iver - Holocene (Official Music Video)"));
            Assert.AreEqual("TWcyIpul8OE", playlist.VideoID);
            Assert.AreEqual("RDTWcyIpul8OE", playlist.PlaylistID);
        }
    }
}