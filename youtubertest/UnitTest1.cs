using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using youtuber.net;

namespace youtubertest
{
    [TestClass]
    public class URLTest
    {
        const string VIDEOID = "RCQV7FYlwlE";
        const string HOST01 = "www.youtube.com";
        const string HOST02 = "youtube.com";
        const string HOST03 = "www.m.youtube.com";
        const string HOST04 = "m.youtube.com";
        const string HOST05 = "www.youtu.be";
        const string HOST06 = "youtu.be";
        const string HOST09 = "www.youtube-nocookie.com";
        const string HOST10 = "youtube-nocookie.com";

        const string HOST20 = "img.youtube.com";
        const string HOST21 = "i.ytimg.com";
        const string HOST22 = "i1.ytimg.com";
        const string HOST23 = "i2.ytimg.com";
        const string HOST24 = "i3.ytimg.com";
        const string HOST25 = "i4.ytimg.com";

        static Uri normalUrl1 = new Uri("https://" + HOST01 + "/watch?v=" + VIDEOID);
        static Uri normalUrl2 = new Uri("https://" + HOST02 + "/watch?v=" + VIDEOID);
        static Uri mobileUrl1 = new Uri("https://" + HOST03 + "/watch?v=" + VIDEOID);
        static Uri mobileUrl2 = new Uri("https://" + HOST04 + "/watch?v=" + VIDEOID);
        static Uri shortUrl1 = new Uri("https://" + HOST05 + "/" + VIDEOID);
        static Uri shortUrl2 = new Uri("https://" + HOST06 + "/" + VIDEOID);
        static Uri nocookieUrl1 = new Uri("https://" + HOST09 + "/embed/" + VIDEOID);
        static Uri nocookieUrl2 = new Uri("https://" + HOST10 + "/embed/" + VIDEOID);

        static Uri imageUrl0 = new Uri("https://" + HOST20 + "/vi/" + VIDEOID + "/0.jpg");
        static Uri imageUrl1 = new Uri("https://" + HOST21 + "/vi/" + VIDEOID + "/1.jpg");
        static Uri imageUrl2 = new Uri("https://" + HOST22 + "/vi/" + VIDEOID + "/2.jpg");
        static Uri imageUrl3 = new Uri("https://" + HOST23 + "/vi/" + VIDEOID + "/3.jpg");
        static Uri imageUrldef = new Uri("https://" + HOST24 + "/vi/" + VIDEOID + "/atic default.jpg");
        static Uri imageUrlhq = new Uri("https://" + HOST25 + "/vi/" + VIDEOID + "/atic thqdefault.jpg");
        static Uri imageUrlmq = new Uri("https://" + HOST20 + "/vi/" + VIDEOID + "/atic tmqdefault.jpg");
        static Uri imageUrlsd = new Uri("https://" + HOST21 + "/vi/" + VIDEOID + "/atic tsddefault.jpg");
        static Uri imageUrlmaxres = new Uri("https://" + HOST22 + "/vi/" + VIDEOID + "/maxresdefault.jpg");

        List<Uri> validVideoUris = new List<Uri>(){
            normalUrl1,
            normalUrl2,
            mobileUrl1,
            mobileUrl2,
            mobileUrl2,
            shortUrl1,
            shortUrl2,
            nocookieUrl1,
            nocookieUrl2
        };

        List<Uri> validImageUris = new List<Uri>(){
            imageUrl0,
            imageUrl1,
            imageUrl2,
            imageUrl3,
            imageUrldef,
            imageUrlhq,
            imageUrlmq,
            imageUrlsd,
            imageUrlmaxres
        };

        [TestMethod]
        public void AcceptValidURLs(){
            URLResult validVideo = URLResult.isValid | URLResult.hasVideoID | URLResult.isVideo;
            URLResult validImage = URLResult.isValid | URLResult.hasVideoID | URLResult.isImage;
            foreach (Uri validVideoUri in validVideoUris) {
                Assert.AreEqual(validVideo, YoutubeUtility.analyseURL(validVideoUri));
            }
            foreach (Uri validImageUri in validImageUris) {
                Assert.AreEqual(validImage, YoutubeUtility.analyseURL(validImageUri));
            }
        }
    }
}