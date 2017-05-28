using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using youtuber.Net.Youtube;

namespace youtubertest
{
    [TestClass]
    public class DeciphererTest
    {
        private static readonly string basePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        private static readonly string fileContent = File.ReadAllText(Path.Combine(basePath, "baseTest.js"));

        [TestMethod]
        public void CorrectFunctions(){
            Assert.AreEqual("ABCDE", Decipherer.Swap.Invoke("ABCDE", 0));
            Assert.AreEqual("BACDE", Decipherer.Swap.Invoke("ABCDE", 1));
            Assert.AreEqual("CBADE", Decipherer.Swap.Invoke("ABCDE", 2));
            Assert.AreEqual("DBCAE", Decipherer.Swap.Invoke("ABCDE", 3));
            Assert.AreEqual("EBCDA", Decipherer.Swap.Invoke("ABCDE", 4));
            Assert.AreEqual("ABCDE", Decipherer.Swap.Invoke("ABCDE", 5));
            Assert.AreEqual("BACDE", Decipherer.Swap.Invoke("ABCDE", 6));
            Assert.AreEqual("CBADE", Decipherer.Swap.Invoke("ABCDE", 7));
            Assert.AreEqual("DBCAE", Decipherer.Swap.Invoke("ABCDE", 8));
            Assert.AreEqual("EBCDA", Decipherer.Swap.Invoke("ABCDE", 9));

            Assert.AreEqual("EDCBA", Decipherer.Reverse.Invoke("ABCDE", 0));
            Assert.AreEqual("EDCBA", Decipherer.Reverse.Invoke("ABCDE", 3));

            Assert.AreEqual("ABCDE", Decipherer.Splice.Invoke("ABCDE", 0));
            Assert.AreEqual("BCDE", Decipherer.Splice.Invoke("ABCDE", 1));
            Assert.AreEqual("", Decipherer.Splice.Invoke("ABCDE", 5));
            Assert.AreEqual("", Decipherer.Splice.Invoke("ABCDE", 7));
        }

        [TestMethod]
        public void DecipherSignature(){
            string signature =
                "2D9D9371C436678DDD6C2878F0788A7DB08232F09DB.5010C1AC6B935240BA998B59F97872F231C1AE1617617";
            Decipherer decipherer = Decipherer.GetDecipherer("playerversion", fileContent).Result;
            string deciphered = decipherer.Decipher(signature);
            Assert.AreEqual("D9371C436678D2D6C2878F0788A7DB08232F09DB.5010C1AC7B935240BA998B59F97872F231C1AE16",
                deciphered);
        }
    }
}