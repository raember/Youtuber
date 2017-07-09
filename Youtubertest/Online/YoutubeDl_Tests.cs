using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Youtuber.Net;
using Youtuber.Net.Youtube;

namespace youtubertest {
    [TestClass]
    public class YoutubeDl_Tests {
        [TestMethod]
        public async Task BaW_jenozKc(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=BaW_jenozKc");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2012, 10, 2), video.UploadedDateTime);
            Assert.AreEqual("youtube-dl test video \"'/\\ä↭𝕐", video.Title);
            Assert.AreEqual("Philipp Hagemeister", video.User);
            Assert.AreEqual("phihag", video.UserID);
            Assert.AreEqual("test chars:  \"'/\\ä↭𝕐\n" +
                            "test URL: https://github.com/rg3/youtube-dl/issues/1892\n" +
                            "\n" +
                            "This is a test video for youtube-dl.\n" +
                            "\n" +
                            "For more information, contact phihag@phihag.de .",
                video.Description);
            Assert.AreEqual(1, video.Keywords.Count);
            Assert.AreEqual(TimeSpan.FromSeconds(10), video.Duration);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();
            Assert.AreEqual(22, downloadables.Count);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task UxxajLWwzqY_use_cipher_signature_video_bug897(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=UxxajLWwzqY");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2012, 5, 6), video.UploadedDateTime);
            Assert.AreEqual("Icona Pop - I Love It (feat. Charli XCX) [OFFICIAL VIDEO]", video.Title);
            Assert.AreEqual(18, video.Keywords.Count);
            Assert.AreEqual(new TimeSpan(0, 3, 0), video.Duration);
            Assert.AreEqual("Icona Pop", video.User);
            Assert.AreEqual("IconaPop", video.UserID);
            Assert.AreEqual("I Love It (feat. Charli XCX) \n" +
                            "\n" +
                            "STREAM 'This Is... Icona Pop': http://smarturl.it/ThisIs_Streaming\n" +
                            "\n" +
                            "DOWNLOAD 'This Is... Icona Pop'\n" +
                            " http://smarturl.it/ThisIs\n" +
                            "\n" +
                            "FOLLOW:\n" +
                            "http://iconapop.com\n" +
                            "http://facebook.com/iconapop \n" +
                            "https://instagram.com/iconapop\n" +
                            "http://twitter.com/iconapop\n" +
                            "http://soundcloud.com/iconapop",
                video.Description);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.AreEqual(20, downloadables.Count);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task _07FYdnEawAQ_VEVO_video_with_age_protection_bug956(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=07FYdnEawAQ");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2013, 7, 3), video.UploadedDateTime);
            Assert.AreEqual("Justin Timberlake - Tunnel Vision (Explicit)", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(419), video.Duration);
            Assert.AreEqual("justintimberlakeVEVO", video.User);
            Assert.AreEqual("justintimberlakeVEVO", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }


        [TestMethod]
        public async Task yZIXLfi8CZQ_Embed_only_bug1746(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=yZIXLfi8CZQ");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2012, 6, 8), video.UploadedDateTime);
            Assert.AreEqual("Principal Sexually Assaults A Teacher - Episode 117 - 8th June 2012", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(419), video.Duration);
            Assert.AreEqual("SET India", video.User);
            Assert.AreEqual("setindia", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task a9LDPn_MO4I_256k_DASH_audio_format_141_via_DASH_manifest(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=a9LDPn-MO4I");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2012, 10, 2), video.UploadedDateTime);
            Assert.AreEqual("UHDTV TEST 8K VIDEO.mp4", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(60), video.Duration);
            Assert.AreEqual("8KVIDEO", video.User);
            Assert.AreEqual("8KVIDEO", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            Assert.IsTrue(downloadables.Exists(d => d.ITag == 141));
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task IB3lcPjvWLA_DASH_manifest_with_encrypted_signature(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=IB3lcPjvWLA");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2013, 10, 11), video.UploadedDateTime);
            Assert.AreEqual("Afrojack, Spree Wilson - The Spark ft. Spree Wilson", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(244), video.Duration);
            Assert.AreEqual("AfrojackVEVO", video.User);
            Assert.AreEqual("AfrojackVEVO", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task nfWlot6h_JM_BaseJS_function_containing_dollar_sign(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=nfWlot6h_JM");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2014, 8, 18), video.UploadedDateTime);
            Assert.AreEqual("Taylor Swift - Shake It Off", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(242), video.Duration);
            Assert.AreEqual("TaylorSwiftVEVO", video.User);
            Assert.AreEqual("TaylorSwiftVEVO", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task T4XJQO3qol8_Controversy_video(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=T4XJQO3qol8");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2010, 9, 9), video.UploadedDateTime);
            Assert.AreEqual("Burning Everyone\'s Koran", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(219), video.Duration);
            Assert.AreEqual("The Amazing Atheist", video.User);
            Assert.AreEqual("TheAmazingAtheist", video.UserID);
            Assert.AreEqual("SUBSCRIBE: http://www.youtube.com/saturninefilms" +
                            "\n" +
                            "\nEven Obama has taken a stand against freedom on this issue: http://www.huffingtonpost.com/2010/09/09/obama-gma-interview-quran_n_710282.html",
                video.Description);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task HtVdAasjOgU_Normal_age_gate_video_No_vevo_embed_allowed(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=HtVdAasjOgU");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2014, 6, 5), video.UploadedDateTime);
            Assert.AreEqual("The Witcher 3: Wild Hunt - The Sword Of Destiny Trailer", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(142), video.Duration);
            Assert.AreEqual("The Witcher", video.User);
            Assert.AreEqual("WitcherGame", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task _6kLq3WMV1nU_Age_gate_video_with_encrypted_signature(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=6kLq3WMV1nU");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2011, 6, 29), video.UploadedDateTime);
            Assert.AreEqual("Dedication To My Ex (Miss That) (Lyric Video)", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(247), video.Duration);
            Assert.AreEqual("LloydVEVO", video.User);
            Assert.AreEqual("LloydVEVO", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task __2ABJjxzNo_video_info_is_none_bug4421(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=__2ABJjxzNo");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2010, 4, 30), video.UploadedDateTime);
            Assert.AreEqual("Deadmau5 - Some Chords (HD)", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(266), video.Duration);
            Assert.AreEqual("deadmau5", video.User);
            Assert.AreEqual("deadmau5", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task lqQg6PlCWgI_Olympics_bug4431(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=lqQg6PlCWgI");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2015, 8, 27), video.UploadedDateTime);
            Assert.AreEqual("Hockey - Women -  GER-AUS - London 2012 Olympic Games", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(6085), video.Duration);
            Assert.AreEqual("Olympic", video.User);
            Assert.AreEqual("olympic", video.UserID);
            Assert.AreEqual("HO09  - Women -  GER-AUS - Hockey - 31 July 2012 - London 2012 Olympic Games",
                video.Description);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task _b_2C3KPAM0_non_square_pixels(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=_b-2C3KPAM0");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2011, 3, 10), video.UploadedDateTime);
            Assert.AreEqual("[A-made] 變態妍字幕版 太妍 我就是這樣的人", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(85), video.Duration);
            Assert.AreEqual("孫艾倫", video.User);
            Assert.AreEqual("AllenMeow", video.UserID);
            Assert.AreEqual("made by Wacom from Korea | 字幕&加油添醋 by TY\'s Allen | 感謝heylisa00cavey1001同學熱情提供梗及翻譯",
                video.Description);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task qEJwOuvDf7I_url_encoded_fmt_stream_map_is_empty_string(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=qEJwOuvDf7I");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2015, 4, 4), video.UploadedDateTime);
            Assert.AreEqual("Обсуждение судебной практики по выборам 14 сентября 2014 года в Санкт-Петербурге",
                video.Title);
            Assert.AreEqual("Наблюдатели Петербурга", video.User);
            Assert.AreEqual("spbelect", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task FIl7x6_3R5Y_Extraction_from_multiple_DASH_manifests_bug6097(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=FIl7x6_3R5Y");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2015, 6, 25), video.UploadedDateTime);
            Assert.AreEqual("[60fps] 150614  마마무 솔라 'Mr. 애매모호' 라이브 직캠 @대학로 게릴라 콘서트", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(220), video.Duration);
            Assert.AreEqual("dorappi2000", video.User);
            Assert.AreEqual("dorappi2000", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 31);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task CsmdDsKjzN8_DASH_manifest_with_segment_list(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=CsmdDsKjzN8");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2015, 5, 1), video.UploadedDateTime);
            Assert.AreEqual("Retransmisión XVIII Media maratón Zaragoza 2015", video.Title);
            Assert.AreEqual("Airtek", video.User);
            Assert.AreEqual("Airtek", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task lsguqyKfVQg_Title_with_JS_like_syntax_bug7468(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=lsguqyKfVQg");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2015, 11, 19), video.UploadedDateTime);
            Assert.AreEqual("{dark walk}; Loki/AC/Dishonored; collab w/Elflover21", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(133), video.Duration);
            Assert.AreEqual("IronSoulElf", video.User);
            Assert.AreEqual("IronSoulElf", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task Ms7iBXnlUO8_Tags_with_JS_like_syntax_bug7468(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=Ms7iBXnlUO8");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual("info: {}});},n);}};(function(w,startTick", string.Join(",", video.Keywords));
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task M4gD1WSo5mA_Video_licensed_under_Creative_Commons(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=M4gD1WSo5mA");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2015, 1, 27), video.UploadedDateTime);
            Assert.AreEqual(
                "William Fisher, CopyrightX: Lecture 3.2, The Subject Matter of Copyright: Drama and choreography",
                video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(721), video.Duration);
            Assert.AreEqual("The Berkman Klein Center for Internet & Society", video.User);
            Assert.AreEqual("BerkmanCenter", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task eQcmzGIKrzg_Channel_like_uploader_url(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=eQcmzGIKrzg");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2015, 11, 19), video.UploadedDateTime);
            Assert.AreEqual("Democratic Socialism and Foreign Policy | Bernie Sanders", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(4060), video.Duration);
            Assert.AreEqual("Bernie 2016", video.User);
            Assert.AreEqual("UCH1dpzjCEiGAt8CXkryhkZg", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task V36LpHqtcDY(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=V36LpHqtcDY");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2012, 7, 19), video.UploadedDateTime);
            Assert.AreEqual("Software as a Service Intro Video - Fall 2012", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(147), video.Duration);
            Assert.AreEqual("edXgreg", video.User);
            Assert.AreEqual("edXgreg", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task i1Ko8UG_Tdo_YouTube_Red_paid_video_bug10059(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=i1Ko8UG-Tdo");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2016, 7, 6), video.UploadedDateTime);
            Assert.AreEqual("OSTRICH JOCKEYS?! (Final Fantasy XV) - Game Lab", video.Title);
            Assert.AreEqual(new TimeSpan(0, 19, 59), video.Duration);
            Assert.AreEqual("The Game Theorists", video.User);
            Assert.AreEqual("MatthewPatrick13", video.UserID);
            Assert.AreEqual(-1, video.Likes);
            Assert.AreEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.AreEqual(0, downloadables.Count);
        }

        [TestMethod]
        public async Task yYr8q0y5Jfg_Rental_video_preview(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=yYr8q0y5Jfg");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2015, 8, 11), video.UploadedDateTime);
            Assert.AreEqual("Piku - Trailer", video.Title);
            Assert.AreEqual(new TimeSpan(2, 2, 22), video.Duration);
            Assert.AreEqual("FlixMatrix", video.User);
            Assert.AreEqual("FlixMatrixKaravan", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task iqKdEhx_dD4_YouTube_Red_video_with_episode_data(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=iqKdEhx-dD4");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2017, 1, 18), video.UploadedDateTime);
            Assert.AreEqual("Isolation - Mind Field (Ep 1)", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(2085), video.Duration);
            Assert.AreEqual("Vsauce", video.User);
            Assert.AreEqual("Vsauce", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task _1t24XAntNCY_Itag_212(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=1t24XAntNCY");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2012, 3, 6), video.UploadedDateTime);
            Assert.AreEqual("Key & Peele - Flicker", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(343), video.Duration);
            Assert.AreEqual("Comedy Central", video.User);
            Assert.AreEqual("comedycentral", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            Assert.IsTrue(downloadables.Exists(d => d.ITag == 212));
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task sJL6WA_aGkQ_Geo_restricted_to_JP(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=sJL6WA-aGkQ");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2016, 10, 4), video.UploadedDateTime);
            Assert.AreEqual("西野カナ 『Dear Bride』MV(Short Ver.)", video.Title);
            Assert.AreEqual("西野カナ Official YouTube Channel", video.User);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }

        [TestMethod]
        public async Task MuAGGZNfUkU(){
            Uri uri = new Uri("https://www.youtube.com/watch?v=MuAGGZNfUkU");
            string videoId = URLUtility.ExtractVideoID(uri);
            Video video = await Video.fromID(videoId);
            Assert.IsTrue(video.Success);
            Assert.AreEqual(new DateTime(2010, 7, 19), video.UploadedDateTime);
            Assert.AreEqual("Hans Zimmer - Time", video.Title);
            Assert.AreEqual(TimeSpan.FromSeconds(285), video.Duration);
            Assert.AreEqual("nopleaseyes", video.User);
            Assert.AreEqual("nopleaseyes", video.UserID);
            Assert.AreNotEqual(-1, video.Likes);
            Assert.AreNotEqual(-1, video.Dislikes);
            List<VideoFile> downloadables = video.ExtractFiles();

            Assert.IsTrue(downloadables.Count > 0);
            foreach (VideoFile downloadable in downloadables)
                Assert.IsTrue(await IntegrationTests.RemoteFilePresent(downloadable, video.Cookies));
        }
    }
}