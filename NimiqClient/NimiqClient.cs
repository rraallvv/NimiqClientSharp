using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System;

namespace Nimiq
{
    // JSONRPC Models

    /// <summary>Error returned in the response for the JSONRPC the server.</summary>
    [Serializable]
    public class ResponseError
    {
        public int code { get; set; }
        public string message { get; set; }
    }

    /// <summary>Used to decode the JSONRPC response returned by the server.</summary>
    [Serializable]
    public class Root<T>
    {
        public string jsonrpc { get; set; }
        public T result { get; set; }
        public int id { get; set; }
        public ResponseError error { get; set; }
    }

    // JSONRPC Client

    /// <summary>Used in initialization of NimiqClient class.</summary>
    public class Config
    {
        /// <summary>Protocol squeme, <c>"http"</c> or <c>"https"</c>.</summary>
        public string scheme;
        /// <summary>Host IP address.</summary>
        public string host;
        /// <summary>Host port.</summary>
        public int port;
        /// <summary>Authorized user.</summary>
        public string user;
        /// <summary>Password for the authorized user.</summary>
        public string password;
    }

    /// <summary>Thrown when something when wrong with the JSONRPC request.</summary>
    public class NimiqClientException : System.Exception
    {
        public NimiqClientException(string message) : base(message) { }
    }

    /// <summary>Nimiq JSONRPC Client</summary>
    public class NimiqClient
    {
        /// <summary>Number in the sequence for the of the next request.</summary>
        public int id = 0;

        /// <summary>URL of the JSONRPC server.
        /// - Format: <c>scheme://user:password@host:port</c><summary>
        private string url;

        /// <summary>HttpClient used for HTTP requests send to the JSONRPC server.</summary>
        private HttpClient client = null;

        /// <summary>Client initialization from a Config structure.
        /// When no parameter is given, it uses de default configuration in the server (<c>http://:@127.0.0.1:8648</c>).</summary>
        /// <param name="config">Options used for the configuration.</param>
        public NimiqClient(Config config = null)
        {
            if (config != null) {
                Init(config.scheme, config.user, config.password, config.host, config.port);
            }
            else
            {
                Init("http", "", "", "127.0.0.1", 8648);
            }
        }

        /// <summary>Initialization.</summary>
        /// <param name="scheme">Protocol squeme, <c>"http"</c> or <c>"https"</c>.</param>
        /// <param name="user">Authorized user.</param>
        /// <param name="password">Password for the authorized user.</param>
        /// <param name="host">Host IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="client">Used to make all requests. If ommited the an instance of HttpClient is automaticaly create.</param>
        public NimiqClient(string scheme, string user, string password, string host, int port, HttpClient client = null)
        {
            Init(scheme, user, password, host, port, client);
        }

        /// <summary>Designated initializer for the client.</summary>
        /// <param name="scheme">Protocol squeme, <c>"http"</c> or <c>"https"</c>.</param>
        /// <param name="user">Authorized user.</param>
        /// <param name="password">Password for the authorized user.</param>
        /// <param name="host">Host IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="client">Used to make all requests. If ommited the an instance of HttpClient is automaticaly create.</param>
        private void Init(string scheme, string user, string password, string host, int port, HttpClient client = null)
        {
            url = $@"{scheme}://{user}:{password}@{host}:{port}";
            if (client != null)
            {
                this.client = client!;
            }
            else
            {
                this.client = new HttpClient();
            }
        }

        /// <summary>Used in all JSONRPC requests to fetch the data.</summary>
        /// <param name="method">JSONRPC method.</param>
        /// <param name="params">Parameters used by the request.</param>
        /// <returns>If succesfull, returns the model reperestation of the result, <c>nil</c> otherwise.</returns>
        private async Task<T> Fetch<T>(string method, object[] parameters)
        {
            Root<T> responseObject = null;
            Exception clientError = null;
            try
            {
                // prepare the request
                var serializedParams = JsonSerializer.Serialize(parameters);
                var contentData = new StringContent($@"{{""jsonrpc"": ""2.0"", ""method"": ""{method}"", ""params"": {serializedParams}, ""id"": {id}}}", Encoding.UTF8, "application/json");
                // send the request
                var response = await client.PostAsync(url, contentData);
                var content = response.Content;
                var data = await content.ReadAsStringAsync();
                // serialize the data into an object
                responseObject = JsonSerializer.Deserialize<Root<T>>(data);
            }
            catch (Exception error)
            {
                clientError = error;
            }

            // throw if there are any errors
            if (clientError != null)
            {
                throw new NimiqClientException(clientError.Message); ;
            }

            if (responseObject.error != null)
            {
                var responseError = responseObject.error;
                throw new NimiqClientException($"{responseError.message} (Code: {responseError.code})");
            }

            // increase the JSONRPC client request id for the next request
            id = id + 1;

            return responseObject.result;
        }

        /// <summary>Returns information on the current consensus state.</summary>
        /// <returns>Consensus state. <c>"established"</c> is the value for a good state, other values indicate bad.</returns>
        public async Task<string> Consensus() {
            return await Fetch<string>("consensus", new object[0]);
        }
    }
}
