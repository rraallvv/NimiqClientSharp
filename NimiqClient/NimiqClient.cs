using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System;
using System.Collections.Generic;

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

    /// <summaryType of a Nimiq account.</summary>
    [Serializable]
    public enum AccountType : int
    {
        /// <summaryNormal Nimiq account.</summary>
        basic = 0,
        /// <summaryVesting contract.</summary>
        vesting = 1,
        /// <summaryHashed Timelock Contract.</summary>
        htlc = 2,
    }

    /// <summary>Normal Nimiq account object returned by the server.</summary>
    public class Account
    {
        /// <summary>Hex-encoded 20 byte address.</summary>
        public string id { get; set; }
        /// <summary>User friendly address (NQ-address).</summary>
        public string address { get; set; }
        /// <summary>Balance of the account (in smallest unit).</summary>
        public int balance { get; set; }
        /// <summary>The account type associated with the account.</summary>
        public AccountType type { get; set; }
    }

    /// <summary>Vesting contract object returned by the server.</summary>
    public class VestingContract : Account
    {
        /// <summary>Hex-encoded 20 byte address of the owner of the vesting contract.</summary>
        public string owner { get; set; }
        /// <summary>User friendly address (NQ-address) of the owner of the vesting contract.</summary>
        public string ownerAddress { get; set; }
        /// <summary>The block that the vesting contracted commenced.</summary>
        public int vestingStart { get; set; }
        /// <summary>The number of blocks after which some part of the vested funds is released.</summary>
        public int vestingStepBlocks { get; set; }
        /// <summary>The amount (in smallest unit) released every vestingStepBlocks blocks.</summary>
        public int vestingStepAmount { get; set; }
        /// <summary>The total amount (in smallest unit) that was provided at the contract creation.</summary>
        public int vestingTotalAmount { get; set; }
    }

    /// <summary>Hashed Timelock Contract object returned by the server.
    public class HTLC : Account
    {
        /// <summary>Hex-encoded 20 byte address of the sender of the HTLC.</summary>
        public string sender { get; set; }
        /// <summary>User friendly address (NQ-address) of the sender of the HTLC.</summary>
        public string senderAddress { get; set; }
        /// <summary>Hex-encoded 20 byte address of the recipient of the HTLC.</summary>
        public string recipient { get; set; }
        /// <summary>User friendly address (NQ-address) of the recipient of the HTLC.</summary>
        public string recipientAddress { get; set; }
        /// <summary>Hex-encoded 32 byte hash root.</summary>
        public string hashRoot { get; set; }
        /// <summary>Hash algorithm.</summary>
        public int hashAlgorithm { get; set; }
        /// <summary>Number of hashes this HTLC is split into.</summary>
        public int hashCount { get; set; }
        /// <summary>Block after which the contract can only be used by the original sender to recover funds.</summary>
        public int timeout { get; set; }
        /// <summary>The total amount (in smallest unit) that was provided at the contract creation.</summary>
        public int totalAmount { get; set; }
    }

    /// <summary>Nimiq account returned by the server. The especific type can obtained with the cast operator.<summary>
    [Serializable]
    class RawAccount
    {

        public string id { get; set; }
        public string address { get; set; }
        public int balance { get; set; }
        public AccountType type { get; set; }
        public string owner { get; set; }
        public string ownerAddress { get; set; }
        public int vestingStart { get; set; }
        public int vestingStepBlocks { get; set; }
        public int vestingStepAmount { get; set; }
        public int vestingTotalAmount { get; set; }
        public string sender { get; set; }
        public string senderAddress { get; set; }
        public string recipient { get; set; }
        public string recipientAddress { get; set; }
        public string hashRoot { get; set; }
        public int hashAlgorithm { get; set; }
        public int hashCount { get; set; }
        public int timeout { get; set; }
        public int totalAmount { get; set; }

        /// <summary>Converts to normal account.<summary>
        /// <param name="account">Raw account type with all fields.</param>
        /// <returns>Normal account type.</returns>
        public static explicit operator Account(RawAccount account)
        {
            return new Account()
            {
                id = account.id,
                address = account.address,
                balance = account.balance,
                type = account.type,
            };
        }

        /// <summary>Converts to vesting contract account.<summary>
        /// <param name="account">Raw account type with all fields.</param>
        /// <returns>Vesting contract account type.</returns>
        public static explicit operator VestingContract(RawAccount account)
        {
            return new VestingContract()
            {
                id = account.id,
                address = account.address,
                balance = account.balance,
                type = account.type,
                owner = account.owner,
                ownerAddress = account.ownerAddress,
                vestingStart = account.vestingStart,
                vestingStepBlocks = account.vestingStepBlocks,
                vestingStepAmount = account.vestingStepAmount,
                vestingTotalAmount = account.vestingTotalAmount
            };
        }

        /// <summary>Converts to HTLC account.<summary>
        /// <param name="account">Raw account type with all fields.</param>
        /// <returns>HTLC account type.</returns>
        public static explicit operator HTLC(RawAccount account)
        {
            return new HTLC()
            {
                id = account.id,
                address = account.address,
                balance = account.balance,
                type = account.type,
                sender = account.sender,
                senderAddress = account.senderAddress,
                recipient = account.recipient,
                recipientAddress = account.recipientAddress,
                hashRoot = account.hashRoot,
                hashAlgorithm = account.hashAlgorithm,
                hashCount = account.hashCount,
                timeout = account.timeout,
                totalAmount = account.totalAmount
            };
        }
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

        /// <summary>Returns a list of addresses owned by client.</summary>
        /// <returns>Array of Accounts owned by the client.</returns>
        public async Task<object[]> Accounts() {
            var result = await Fetch<object[]>("accounts", new object[0]);
            var converted = new object[0];
            for (int i = 0; i < result.Length; i++) {
                RawAccount account = (RawAccount)result[i];
                switch (account.type)
                {
                    case AccountType.basic:
                        converted[i] = (Account)account;
                        break;
                    case AccountType.vesting:
                        converted[i] = (VestingContract)account;
                        break;
                    case AccountType.htlc:
                        converted[i] = (HTLC)account;
                        break;
                }
            }
            return converted;
        }
    }
}
