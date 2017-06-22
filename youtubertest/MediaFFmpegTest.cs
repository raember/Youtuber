using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using youtuber.Media.FFmpeg;

namespace youtubertest
{
    [TestClass]
    public class MediaFFmpegTest
    {
        [DeploymentItem("../../TestData")]
        [TestMethod]
        public void ConvertMp4ToMp3()
        {
            ProcessExe clsProcExe = new ProcessExe();

            clsProcExe.StartProcess("JustinSeven-MusicTheBest3.MP4", "test2.mp3");

            Assert.IsTrue(File.Exists("test2.mp3")); 
        }
    }
}
