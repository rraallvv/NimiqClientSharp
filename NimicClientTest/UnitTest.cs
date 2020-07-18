using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nimiq;

namespace NimiqClientTest
{
    [TestClass]
    public class UnitTest
    {
        static NimiqClient client;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            //set up the stub
            var httpClient = new HttpClient(new HttpMessageHandlerStub());

            // init our JSON RPC client with that
            client = new NimiqClient(
                "http",
                "user",
                "password",
                "127.0.0.1",
                8648,
                httpClient
            );
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestMethod]
        public async Task TestConsensusState()
        {
            HttpMessageHandlerStub.testData = Fixtures.ConsensusSyncing();

            object result = await client.Consensus();

            Assert.AreEqual("consensus", HttpMessageHandlerStub.latestRequestMethod);

            Assert.AreEqual("syncing", result);
        }
    }
}
