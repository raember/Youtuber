using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using youtuber.net;
using youtuber.Net.Youtube;

namespace youtubertest
{
    [TestClass]
    public class YoutubeVideoTest
    {
        private const string VIDEOID = "TWcyIpul8OE";
        private readonly string basePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        private FileStream fileStream;
        private readonly Mock<HttpWebRequest> httpWebRequestMock = new Mock<HttpWebRequest>();
        private readonly Mock<HttpWebResponse> httpWebResponseMock = new Mock<HttpWebResponse>();

        [TestInitialize]
        public void Initialize(){
            httpWebRequestMock.Setup(request => request.GetResponseAsync()).ReturnsAsync(httpWebResponseMock.Object);
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

        [DeploymentItem("../../TestData")]
        [TestMethod]
        public async Task GetVideoData(){
            SetMock("./HoloceneVideo.html");

            Video video = await Video.fromID(VIDEOID);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(VIDEOID, video.VideoID);
            Assert.AreEqual("Bon Iver - Holocene (Official Music Video)", video.Title);
            Assert.AreEqual(new DateTime(2011, 8, 17), video.UploadedDateTime);
            Assert.AreEqual("Director: NABIL (NABIL.com)\n" +
                            "Producer: Jill Hammer\n" +
                            "Production Company: NE Direction\n" +
                            "Editor: Isaac Hagy\n" +
                            "DOP: Larkin Sieple\n" +
                            "\n" +
                            "boniver.org\n" +
                            "jagjaguwar.com",
                video.Description);
            Assert.AreEqual("boniver", video.UserID);
            Assert.AreEqual("boniver", video.User);
            Assert.AreEqual(31305565, video.Views);
            Assert.AreEqual(171471, video.Likes);
            Assert.AreEqual(3202, video.Dislikes);
            Assert.AreEqual(235000, video.Subscribers);
            Assert.AreEqual(new TimeSpan(0, 5, 44), video.Duration);
            Assert.AreEqual(3, video.Keywords.Count);
            Assert.AreEqual(new DateTime(2017, 5, 25, 17, 0, 57), video.RequestTime);
            Assert.AreEqual(new DateTime(2011, 8, 17), video.UploadedDateTime);

            List<Recommendation> relatedVideos = video.RelatedVideos;
            Assert.IsTrue(relatedVideos.Count == 20);

            Recommendation recomm = relatedVideos.First();
            Assert.IsTrue(recomm.GetType() == typeof(Recommendation.Video));
            Recommendation.Video videoRecomm = (Recommendation.Video) recomm;
            Assert.AreEqual("Bon Iver - Towers (Official Music Video)", videoRecomm.Title);
            Assert.AreEqual("t60roHM1t7o", videoRecomm.VideoID);
            Assert.AreEqual(new TimeSpan(0, 4, 51), videoRecomm.Duration);
            Assert.AreEqual(7511628, videoRecomm.Views);
            Assert.AreEqual("boniver", videoRecomm.Username);

            recomm = relatedVideos[1];
            Assert.IsTrue(recomm.GetType() == typeof(Recommendation.Playlist));
            Recommendation.Playlist playlistRecomm = (Recommendation.Playlist) recomm;
            Assert.IsTrue(playlistRecomm.Title.EndsWith("Bon Iver - Holocene (Official Music Video)"));
            Assert.AreEqual("TWcyIpul8OE", playlistRecomm.VideoID);
            Assert.AreEqual("RDTWcyIpul8OE", playlistRecomm.PlaylistID);
        }

        [DeploymentItem("../../TestData")]
        [TestMethod]
        public async Task DetectRemovedVideo(){
            SetMock("./removedVideo.html");

            Video video = await Video.fromID("7KTLh716rGY");
            Assert.IsFalse(video.Success);
        }

        [DeploymentItem("../../TestData")]
        [TestMethod]
        public async Task GetDownloadData(){
            SetMock("./HoloceneVideo.html");

            Video video = await Video.fromID(VIDEOID);
            Assert.IsTrue(video.Success);
            List<VideoFile> videoFiles = video.ExtractFiles();
            Assert.IsNotNull(videoFiles);
            VideoFile videoFile = videoFiles.First(vf => vf.ITag == 247);
            Assert.IsNotNull(videoFile);
            Uri link = videoFile.GetDownloadUri().Result;
            Assert.IsNotNull(link);
            Uri referenceUri =
                new Uri(
                    "https://r2---sn-nfpnnjvh-9an6.googlevideo.com/videoplayback?source=youtube&sparams=clen,dur,ei,gir,id,initcwndbps,ip,ipbits,itag,keepalive,lmt,mime,mm,mn,ms,mv,pcm2cms,pl,requiressl,source,expire&clen=27525886&mime=video/webm&requiressl=yes&expire=1495746057&gir=yes&mm=31&ms=au&ei=qfEmWeucIYircI2YsvgH&mv=m&pl=45&mt=1495724371&pcm2cms=yes&ip=2a02:1205:5078:a750:3c6c:1d0e:4be3:c31c&id=o-AGFlAJjaiQ9KzoAQW07hluBljk-WchtQ7l8J8ET4AzFM&keepalive=yes&mn=sn-nfpnnjvh-9an6&itag=247&ipbits=0&initcwndbps=7265000&dur=343.541&key=yt6&lmt=1449576890172300&signature=D9371C436678D2D6C2878F0788A7DB08232F09DB.5010C1AC7B935240BA998B59F97872F231C1AE16&ratebypass=yes");
            Assert.AreEqual(referenceUri, link);
        }
    }
}