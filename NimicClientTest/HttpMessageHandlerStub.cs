using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Nimiq;

namespace NimiqClientTest
{
    public class HttpMessageHandlerStub : HttpMessageHandler
    {
        // test data
        public static string testData;
        public static Dictionary<string, object> latestRequest;
        public static string latestRequestMethod;
        public static object[] latestRequestParams;

        // send back the resonse
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            latestRequest = null;
            latestRequestMethod = null;
            latestRequestParams = null;

            var content = request.Content;
            var json = content.ReadAsStringAsync().Result;

            latestRequest = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            latestRequestMethod = (string)((JsonElement)latestRequest["method"]).TryGetObject();
            latestRequestParams = (object[])((JsonElement)latestRequest["params"]).TryGetObject();

            // load test data
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(testData)
            };

            return await Task.FromResult(responseMessage);
        }
    }
}
