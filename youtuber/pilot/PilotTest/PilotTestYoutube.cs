using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PilotApp;

namespace PilotTest
{
    [TestClass]
    public class PilotTestYoutube
    {
        [TestMethod]
        public void EchoURL()
        {
            string url = @"https://www.youtube.com/watch?v=bM7SZ5SBzyY";  // just a random link from youtube
            Assert.AreEqual<string>(url, PilotYoutube.VerifyURL(url));
        }

        [TestMethod]
        public void ReturnNullWhenURLDoesNotContainYoutube()
        {
            string url = @"https://www.foobar.com/watch?v=bM7SZ5SBzyY";  // just a random link no youtube url
            Assert.AreEqual<string>("", PilotYoutube.VerifyURL(url));
        }
    }
}
