using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Youtuber.Net;
using Youtuber.Net.Youtube;
using Youtuber.Net.Youtube.Official;

namespace youtubertest
{
    [TestClass]
    public class OfficialApiTest
    {
        private string apiKey = string.Empty; // Insert API key here.
        private FileStream fileStream;
        private readonly Mock<HttpWebRequest> httpWebRequestMock = new Mock<HttpWebRequest>();
        private readonly Mock<HttpWebResponse> httpWebResponseMock = new Mock<HttpWebResponse>();

        public async Task MakeSureApiKeyIsAvailable() {
            if (string.IsNullOrEmpty(apiKey)) {
                Video video = await Video.fromID("TWcyIpul8OE");
                string playerVersion = video.PlayerVersion;
                BaseDotJs baseDotJs = await BaseDotJs.GetBaseDotJs(playerVersion, video.Cookies);
                apiKey = baseDotJs.ExtractApiKey();
            }
        }

        private void SetMock(string filePathForStream){
            httpWebRequestMock.Setup(request => request.GetResponseAsync()).ReturnsAsync(httpWebResponseMock.Object);
            InternetSite.DefaultHttpWebRequest = httpWebRequestMock.Object;
            fileStream = new FileInfo(filePathForStream).OpenRead();
            httpWebResponseMock.Setup(response => response.GetResponseStream()).Returns(fileStream);
        }

        [DeploymentItem("../../TestData")]
        [TestMethod]
        public async Task SearchFunctionality(){
            await MakeSureApiKeyIsAvailable();
            SetMock("./SearchJson0.json");
            Search search = new Search(apiKey);
            const int RESULTS = 50;
            List<Search.Result> results = await search.Execute("Bon Iver", new[]{Search.Params.MaxResults(RESULTS)});
            Assert.IsTrue(search.Success);
            Assert.IsTrue(search.HasNextPage);
            Assert.IsNotNull(search.ETag);
            Assert.IsNotNull(search.RegionCode);
            Assert.IsTrue(search.TotalResults > 0);
            Assert.AreEqual(RESULTS, search.ResultsPerPage);
            Search.Result result = results.First();
            Assert.IsFalse(string.IsNullOrEmpty(result.ChannelID));
            Assert.IsFalse(string.IsNullOrEmpty(result.ChannelTitle));
            Assert.IsFalse(string.IsNullOrEmpty(result.Description));
            Assert.IsFalse(string.IsNullOrEmpty(result.ETag));
            Assert.IsFalse(string.IsNullOrEmpty(result.LiveBroadcastContent));
            Assert.IsFalse(string.IsNullOrEmpty(result.Title));
            Assert.AreNotEqual(DateTime.MinValue, result.PublishedAt);
            Assert.IsNotNull(result.Thumbnails);
            Assert.IsNotNull(result.Thumbnails.First().Url);
            SetMock("./SearchJson1.json");
            List<Search.Result> result2 = await search.GetNextPage();
        }
    }
}