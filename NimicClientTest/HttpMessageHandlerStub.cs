using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

namespace NimiqClientTest
{
    public class HttpMessageHandlerStub : HttpMessageHandler
    {
        // test data
        public static string testData;
        public static Dictionary<string, object> latestRequest;
        public static string latestRequestMethod;
        public static Array latestRequestParams;

        // send back the resonse
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            latestRequest = null;
            latestRequestMethod = null;
            latestRequestParams = null;

            var content = request.Content;
            var json = content.ReadAsStringAsync().Result;

            latestRequest = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            latestRequestMethod = (string)TryGetObject((JsonElement)latestRequest["method"]);
            latestRequestParams = (Array)TryGetObject((JsonElement)latestRequest["params"]);

            // load test data
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(testData)
            };

            return await Task.FromResult(responseMessage);
        }

        public static object TryGetObject(JsonElement jsonElement)
        {
            object result = null;

            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Null:
                    result = null;
                    break;
                case JsonValueKind.Number:
                    result = jsonElement.GetDouble();
                    break;
                case JsonValueKind.False:
                    result = false;
                    break;
                case JsonValueKind.True:
                    result = true;
                    break;
                case JsonValueKind.Undefined:
                    result = null;
                    break;
                case JsonValueKind.String:
                    result = jsonElement.GetString();
                    break;
                case JsonValueKind.Object:
                    result = TryGetObject(jsonElement);
                    break;
                case JsonValueKind.Array:
                    result = jsonElement.EnumerateArray()
                        .Select(o => TryGetObject(o))
                        .ToArray();
                    break;
            }

            return result;
        }
    }
}
