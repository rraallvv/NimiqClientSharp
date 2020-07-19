using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;

namespace Nimiq
{
    // JSONRPC Models

    /// <summary>Can be both a hexadecimal representation or a human readable address.</summary>
    using Address = String;

    /// <summary>Hexadecimal string containing a hash value.</summary>
    using Hash = String;

    /// <summary>Error returned in the response for the JSONRPC the server.</summary>
    [Serializable]
    public class ResponseError
    {
        public long code { get; set; }
        public string message { get; set; }
    }

    /// <summary>Used to decode the JSONRPC response returned by the server.</summary>
    [Serializable]
    public class Root<T>
    {
        public string jsonrpc { get; set; }
        public T result { get; set; }
        public long id { get; set; }
        public ResponseError error { get; set; }
    }

    /// <summaryType of a Nimiq account.</summary>
    [Serializable]
    public enum AccountType : long
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
        public long balance { get; set; }
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
        public long vestingStart { get; set; }
        /// <summary>The number of blocks after which some part of the vested funds is released.</summary>
        public long vestingStepBlocks { get; set; }
        /// <summary>The amount (in smallest unit) released every vestingStepBlocks blocks.</summary>
        public long vestingStepAmount { get; set; }
        /// <summary>The total amount (in smallest unit) that was provided at the contract creation.</summary>
        public long vestingTotalAmount { get; set; }
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
        public long hashAlgorithm { get; set; }
        /// <summary>Number of hashes this HTLC is split into.</summary>
        public long hashCount { get; set; }
        /// <summary>Block after which the contract can only be used by the original sender to recover funds.</summary>
        public long timeout { get; set; }
        /// <summary>The total amount (in smallest unit) that was provided at the contract creation.</summary>
        public long totalAmount { get; set; }
    }

    /// <summary>Nimiq account returned by the server. The especific type can obtained with the cast operator.<summary>
    [Serializable]
    class RawAccount
    {

        public string id { get; set; }
        public string address { get; set; }
        public long balance { get; set; }
        public AccountType type { get; set; }
        public string owner { get; set; }
        public string ownerAddress { get; set; }
        public long vestingStart { get; set; }
        public long vestingStepBlocks { get; set; }
        public long vestingStepAmount { get; set; }
        public long vestingTotalAmount { get; set; }
        public string sender { get; set; }
        public string senderAddress { get; set; }
        public string recipient { get; set; }
        public string recipientAddress { get; set; }
        public string hashRoot { get; set; }
        public long hashAlgorithm { get; set; }
        public long hashCount { get; set; }
        public long timeout { get; set; }
        public long totalAmount { get; set; }

        public object Account
        {
            get {
                switch (type)
                {
                    case AccountType.basic:
                        return new Account()
                        {
                            id = id,
                            address = address,
                            balance = balance,
                            type = type,
                        };
                    case AccountType.vesting:
                        return new VestingContract()
                        {
                            id = id,
                            address = address,
                            balance = balance,
                            type = type,
                            owner = owner,
                            ownerAddress = ownerAddress,
                            vestingStart = vestingStart,
                            vestingStepBlocks = vestingStepBlocks,
                            vestingStepAmount = vestingStepAmount,
                            vestingTotalAmount = vestingTotalAmount
                        };

                    case AccountType.htlc:
                        return new HTLC()
                        {
                            id = id,
                            address = address,
                            balance = balance,
                            type = type,
                            sender = sender,
                            senderAddress = senderAddress,
                            recipient = recipient,
                            recipientAddress = recipientAddress,
                            hashRoot = hashRoot,
                            hashAlgorithm = hashAlgorithm,
                            hashCount = hashCount,
                            timeout = timeout,
                            totalAmount = totalAmount
                        };
                }
                return null;
            }
        }
    }

    /// <summary>Consensus state returned by the server.</summary>
    [Serializable]
    [JsonConverter(typeof(ConsensusStateConverter))]
    public class ConsensusState
    {
        /// <summary>Connecting.</summary>
        public static ConsensusState trace { get { return new ConsensusState("trace"); } }
        /// <summary>Syncing blocks.</summary>
        public static ConsensusState verbose { get { return new ConsensusState("verbose"); } }
        /// <summary>Consensus established.</summary>
        public static ConsensusState debug { get { return new ConsensusState("debug"); } }

        private class ConsensusStateConverter : JsonConverter<ConsensusState>
        {
            public override ConsensusState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new ConsensusState(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, ConsensusState value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value);
            }
        }

        private ConsensusState(string value) { Value = value; }

        private string Value { get; set; }

        public static implicit operator string(ConsensusState level)
        {
            return level.Value;
        }

        public static explicit operator ConsensusState(string level)
        {
            return new ConsensusState(level);
        }
    }

    /// <summary>Nimiq wallet returned by the server.</summary>
    [Serializable]
    public class Wallet
    {
        /// <summary>Hex-encoded 20 byte address.</summary>
        public string id { get; set; }
        /// <summary>User friendly address (NQ-address).</summary>
        public string address { get; set; }
        /// <summary>Hex-encoded 32 byte Ed25519 public key.</summary>
        public string publicKey { get; set; }
        /// <summary>Hex-encoded 32 byte Ed25519 private key.</summary>
        public string privateKey { get; set; }
    }

    /// <summary>Used to pass the data to send transaccions.</summary>
    public class OutgoingTransaction
    {
        /// <summary>The address the transaction is send from.</summary>
        public Address from { get; set; }
        /// <summary>The account type at the given address.</summary>
        public AccountType fromType { get; set; } = AccountType.basic;
        /// <summary>The address the transaction is directed to.</summary>
        public Address to { get; set; }
        /// <summary>The account type at the given address.</summary>
        public AccountType toType { get; set; } = AccountType.basic;
        /// <summary>Integer of the value (in smallest unit) sent with this transaction.</summary>
        public long value { get; set; }
        /// <summary>Integer of the fee (in smallest unit) for this transaction.</summary>
        public long fee { get; set; }
        /// <summary>Hex-encoded contract parameters or a message.</summary>
        public string data { get; set; } = null;
    }

    /// <summary>Transaction returned by the server.
    [Serializable]
    public class Transaction
    {
        /// <summary>Hex-encoded hash of the transaction.</summary>
        public Hash hash { get; set; }
        /// <summary>Hex-encoded hash of the block containing the transaction.</summary>
        public Hash blockHash { get; set; }
        /// <summary>Height of the block containing the transaction.</summary>
        public long blockNumber { get; set; }
        /// <summary>UNIX timestamp of the block containing the transaction.</summary>
        public long timestamp { get; set; }
        /// <summary>Number of confirmations of the block containing the transaction.</summary>
        public long confirmations { get; set; } = 0;
        /// <summary>Index of the transaction in the block.</summary>
        public long transactionIndex { get; set; }
        /// <summary>Hex-encoded address of the sending account.</summary>
        public string from { get; set; }
        /// <summary>Nimiq user friendly address (NQ-address) of the sending account.</summary>
        public Address fromAddress { get; set; }
        /// <summary>Hex-encoded address of the recipient account.</summary>
        public string to { get; set; }
        /// <summary>Nimiq user friendly address (NQ-address) of the recipient account.</summary>
        public Address toAddress { get; set; }
        /// <summary>Integer of the value (in smallest unit) sent with this transaction.</summary>
        public long value { get; set; }
        /// <summary>Integer of the fee (in smallest unit) for this transaction.</summary>
        public long fee { get; set; }
        /// <summary>Hex-encoded contract parameters or a message.</summary>
        public string data { get; set; } = null;
        /// <summary>Bit-encoded transaction flags.</summary>
        public long flags { get; set; }
    }

    /// <summary>Block returned by the server.</summary>
    [Serializable]
    public class Block
    {
        /// <summary>Height of the block.</summary>
        public long number { get; set; }
        /// <summary>Hex-encoded 32-byte hash of the block.</summary>
        public Hash hash { get; set; }
        /// <summary>Hex-encoded 32-byte Proof-of-Work hash of the block.</summary>
        public Hash pow { get; set; }
        /// <summary>Hex-encoded 32-byte hash of the predecessor block.</summary>
        public Hash parentHash { get; set; }
        /// <summary>The nonce of the block used to fulfill the Proof-of-Work.</summary>
        public long nonce { get; set; }
        /// <summary>Hex-encoded 32-byte hash of the block body Merkle root.</summary>
        public Hash bodyHash { get; set; }
        /// <summary>Hex-encoded 32-byte hash of the accounts tree root.</summary>
        public Hash accountsHash { get; set; }
        /// <summary>Block difficulty, encoded as decimal number in string.</summary>
        public string difficulty { get; set; }
        /// <summary>UNIX timestamp of the block</summary>
        public long timestamp { get; set; }
        /// <summary>Number of confirmations for this transaction (number of blocks on top of the block where this transaction was in).</summary>
        public long confirmations { get; set; }
        /// <summary>Hex-encoded 20 byte address of the miner of the block.</summary>
        public string miner { get; set; }
        /// <summary>User friendly address (NQ-address) of the miner of the block.</summary>
        public Address minerAddress { get; set; }
        /// <summary>Hex-encoded value of the extra data field, maximum of 255 bytes.</summary>
        public string extraData { get; set; }
        /// <summary>Block size in byte.</summary>
        public long size { get; set; }
        /// <summary>Array of transactions. Either represented by the transaction hash or a Transaction object.</summary>
        [JsonConverter(typeof(HashOrTransactionConverter))]
        public object[] transactions { get; set; }

        private class HashOrTransactionConverter : JsonConverter<object[]>
        {
            public override object[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    return JsonSerializer.Deserialize<Transaction[]>(ref reader);
                }
                catch
                {
                    return JsonSerializer.Deserialize<string[]>(ref reader); ;
                }
            }

            public override void Write(Utf8JsonWriter writer, object[] value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>Block template header returned by the server.</summary>
    [Serializable]
    public class BlockTemplateHeader
    {
        /// <summary>Version in block header.</summary>
        public long version { get; set; }
        /// <summary>32-byte hex-encoded hash of the previous block.</summary>
        public Hash prevHash { get; set; }
        /// <summary>32-byte hex-encoded hash of the interlink.</summary>
        public Hash interlinkHash { get; set; }
        /// <summary>32-byte hex-encoded hash of the accounts tree.</summary>
        public Hash accountsHash { get; set; }
        /// <summary>Compact form of the hash target for this block.</summary>
        public long nBits { get; set; }
        /// <summary>Height of the block in the block chain (also known as block number).</summary>
        public long height { get; set; }
    }

    /// <summary>Block template body returned by the server.</summary>
    [Serializable]
    public class BlockTemplateBody
    {
        /// <summary>32-byte hex-encoded hash of the block body.</summary>
        public Hash hash { get; set; }
        /// <summary>20-byte hex-encoded miner address.</summary>
        public string minerAddr { get; set; }
        /// <summary>Hex-encoded value of the extra data field.</summary>
        public string extraData { get; set; }
        /// <summary>Array of hex-encoded transactions for this block.</summary>
        public string[] transactions { get; set; }
        /// <summary>Array of hex-encoded pruned accounts for this block.</summary>
        public string[] prunedAccounts { get; set; }
        /// <summary>Array of hex-encoded hashes that verify the path of the miner address in the merkle tree.
        /// This can be used to change the miner address easily.</summary>
        public Hash[] merkleHashes { get; set; }
    }

    /// <summary>Block template returned by the server.</summary>
    [Serializable]
    public class BlockTemplate
    {
        /// <summary>Block template header returned by the server.</summary>
        public BlockTemplateHeader header { get; set; }
        /// <summary>Hex-encoded interlink.</summary>
        public string interlink { get; set; }
        /// <summary>Block template body returned by the server.</summary>
        public BlockTemplateBody body { get; set; }
        /// <summary>Compact form of the hash target to submit a block to this client.</summary>
        public long target { get; set; }
    }

    /// <summary>Transaction receipt returned by the server.</summary>
    [Serializable]
    public class TransactionReceipt
    {
        /// <summary>Hex-encoded hash of the transaction.</summary>
        public Hash transactionHash { get; set; }
        /// <summary>Integer of the transactions index position in the block.</summary>
        public long transactionIndex { get; set; }
        /// <summary>Hex-encoded hash of the block where this transaction was in.</summary>
        public Hash blockHash { get; set; }
        /// <summary>Block number where this transaction was in.</summary>
        public long blockNumber { get; set; }
        /// <summary>Number of confirmations for this transaction (number of blocks on top of the block where this transaction was in).</summary>
        public long confirmations { get; set; }
        /// <summary>Timestamp of the block where this transaction was in.</summary>
        public long timestamp { get; set; }
    }

    /// <summary>Work instructions receipt returned by the server.</summary>
    [Serializable]
    public class WorkInstructions
    {
        /// <summary>Hex-encoded block header. This is what should be passed through the hash function.
        /// The last 4 bytes describe the nonce, the 4 bytes before are the current timestamp.
        /// Most implementations allow the miner to arbitrarily choose the nonce and to update the timestamp without requesting new work instructions.</summary>
        public string data { get; set; }
        /// <summary>Hex-encoded block without the header. When passing a mining result to submitBlock, append the suffix to the data string with selected nonce.</summary>
        public string suffix { get; set; }
        /// <summary>Compact form of the hash target to submit a block to this client.</summary>
        public long target { get; set; }
        /// <summary>Field to describe the algorithm used to mine the block. Always nimiq-argon2 for now.</summary>
        public string algorithm { get; set; }
    }

    /// <summary>Used to set the log level in the JSONRPC server.</summary>
    [Serializable]
    [JsonConverter(typeof(LogLevelConverter))]
    public class LogLevel
    {
        /// <summary>Trace level log.</summary>
        public static LogLevel trace { get { return new LogLevel("trace"); } }
        /// <summary>Verbose level log.</summary>
        public static LogLevel verbose { get { return new LogLevel("verbose"); } }
        /// <summary>Debugging level log.</summary>
        public static LogLevel debug { get { return new LogLevel("debug"); } }
        /// <summary>Info level log.</summary>
        public static LogLevel info { get { return new LogLevel("info"); } }
        /// <summary>Warning level log.</summary>
        public static LogLevel warn { get { return new LogLevel("warn"); } }
        /// <summary>Error level log.</summary>
        public static LogLevel error { get { return new LogLevel("error"); } }
        /// <summary>Assertions level log.</summary>
        public static LogLevel assert { get { return new LogLevel("assert"); } }

        private class LogLevelConverter : JsonConverter<LogLevel>
        {
            public override LogLevel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new LogLevel(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, LogLevel value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value);
            }
        }

        private LogLevel(string value) { Value = value; }

        private string Value { get; set; }

        public static implicit operator string(LogLevel level)
        {
            return level.Value;
        }

        public static explicit operator LogLevel(string level)
        {
            return new LogLevel(level);
        }
    }

    /// <summary>Mempool information returned by the server.</summary
    [Serializable]
    [JsonConverter(typeof(MempoolInfoConverter))]
    public class MempoolInfo
    {
        /// <summary>Total number of pending transactions in mempool.</summary
        public long total { get; set; }
        /// <summary>Array containing a subset of fee per byte buckets from <c>[10000, 5000, 2000, 1000, 500, 200, 100, 50, 20, 10, 5, 2, 1, 0]</c> that currently have more than one transaction.</summary
        public long[] buckets { get; set; }
        /// <summary>Number of transaction in the bucket. A transaction is assigned to the highest bucket of a value lower than its fee per byte value.</summary
        public Dictionary<long, long> transactionsPerBucket { get; set; }

        private class MempoolInfoConverter : JsonConverter<MempoolInfo>
        {
            public override MempoolInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                var result = new MempoolInfo();
                result.transactionsPerBucket = new Dictionary<long, long>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return result;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    string propertyName = reader.GetString();

                    if (long.TryParse(propertyName, out long key))
                    {
                        long value = JsonSerializer.Deserialize<long>(ref reader, options);
                        result.transactionsPerBucket.Add(key, value);
                    }
                    else if (propertyName == nameof(total))
                    {
                        result.total = JsonSerializer.Deserialize<long>(ref reader, options);
                    }
                    else if (propertyName == nameof(buckets))
                    {
                        result.buckets = JsonSerializer.Deserialize<long[]>(ref reader, options);
                    }
                    else
                    {
                        throw new JsonException($"Unable to convert \"{propertyName}\"");
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, MempoolInfo value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>Peer address state returned by the server.</summary>
    [Serializable]
    public enum PeerAddressState
    {
        /// <summary>New peer.</summary>
        @new = 1,
        /// <summary>Established peer.</summary>
        established = 2,
        /// <summary>Already tried peer.</summary>
        tried = 3,
        /// <summary>Peer failed.</summary>
        failed = 4,
        /// <summary>Balled peer.</summary>
        banned = 5
    }

    /// <summary>Peer connection state returned by the server.
    [Serializable]
    public enum PeerConnectionState
    {
        /// <summary>New connection.</summary>
        @new = 1,
        /// <summary>Connecting.</summary>
        connecting = 2,
        /// <summary>Connected.</summary>
        connected = 3,
        /// <summary>Negotiating connection.</summary>
        negotiating = 4,
        /// <summary>Connection established.</summary>
        established = 5,
        /// <summary>Connection closed.</summary>
        closed = 6
    }

    /// <summary>Peer information returned by the server.</summary>
    [Serializable]
    public class Peer
    {
        /// <summary>Peer id.</summary>
        public string id { get; set; }
        /// <summary>Peer address.</summary>
        public string address { get; set; }
        /// <summary>Peer address state.</summary>
        public PeerAddressState addressState { get; set; }
        /// <summary>Peer connection state.</summary>
        public PeerConnectionState? connectionState { get; set; }
        /// <summary>Node version the peer is running.</summary>
        public int? version { get; set; }
        /// <summary>Time offset with the peer (in miliseconds).</summary>
        public int? timeOffset { get; set; }
        /// <summary>Hash of the head block of the peer.</summary>
        public Hash headHash { get; set; }
        /// <summary>Latency to the peer.</summary>
        public int? latency { get; set; }
        /// <summary>Received bytes.</summary>
        public int? rx { get; set; }
        /// <summary>Sent bytes.</summary>
        public int? tx { get; set; }
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
        public long port;
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
        public long id = 0;

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
            if (config != null)
            {
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
        public NimiqClient(string scheme, string user, string password, string host, long port, HttpClient client = null)
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
        private void Init(string scheme, string user, string password, string host, long port, HttpClient client = null)
        {
            url = $@"{scheme}://{user}:{password}@{host}:{port}";
            if (client != null)
            {
                this.client = client;
            }
            else
            {
                this.client = new HttpClient();
            }
        }

        /// <summary>Used in all JSONRPC requests to fetch the data.</summary>
        /// <param name="method">JSONRPC method.</param>
        /// <param name="parameters">Parameters used by the request.</param>
        /// <returns>If succesfull, returns the model reperestation of the result, <c>null</c> otherwise.</returns>
        private async Task<T> Fetch<T>(string method, object[] parameters = null)
        {
            Root<T> responseObject = null;
            Exception clientError = null;
            try
            {
                // prepare the request
                var serializedParams = JsonSerializer.Serialize(parameters ?? new object[0]);
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

        /// <summary>Returns a list of addresses owned by client.</summary>
        /// <returns>Array of Accounts owned by the client.</returns>
        public async Task<object[]> Accounts()
        {
            var result = await Fetch<RawAccount[]>("accounts");
            return result.Select(o => o.Account).ToArray();
        }

        /// <summary>Returns the height of most recent block.</summary>
        /// <returns>The current block height the client is on.</returns>
        public async Task<long> BlockNumber()
        {
            return await Fetch<long>("blockNumber");
        }

        /// <summary>Returns information on the current consensus state.</summary>
        /// <returns>Consensus state. <c>"established"</c> is the value for a good state, other values indicate bad.</returns>
        public async Task<ConsensusState> Consensus()
        {
            return await Fetch<ConsensusState>("consensus");
        }

        /// <summary>Returns or overrides a constant value.
        /// When no parameter is given, it returns the value of the constant. When giving a value as parameter,
        /// it sets the constant to the given value. To reset the constant use <c>resetConstant()</c> instead.<summary>
        /// <param name="string">The class and name of the constant (format should be <c>"Class.CONSTANT"</c>).</parameter>
        /// <param name="value">The new value of the constant.</parameter>
        /// <returns>The value of the constant.</returns>
        public async Task<long> Constant(string constant, long? value = null)
        {
            var parameters = new List<object>() { constant };
            if (value != null)
            {
                parameters.Add(value.Value);
            }
            return await Fetch<long>("constant", parameters.ToArray());
        }

        /// <summary>Creates a new account and stores its private key in the client store.</summary>
        /// <returns>Information on the wallet that was created using the command.</returns>
        public async Task<Wallet> CreateAccount()
        {
            return await Fetch<Wallet>("createAccount");
        }

        /// <summary>Creates and signs a transaction without sending it.
        /// The transaction can then be send via <c>sendRawTransaction()</c> without accidentally replaying it.</summary>
        /// <param name="transaction">The transaction object.</parameter>
        /// <returns>Hex-encoded transaction.</returns>
        public async Task<string> CreateRawTransaction(OutgoingTransaction transaction)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "from", transaction.from },
                { "fromType", transaction.fromType },
                { "to", transaction.to },
                { "toType", transaction.toType },
                { "value", transaction.value },
                { "fee", transaction.fee },
                { "data", transaction.data }
            };
            return await Fetch<string>("createRawTransaction", new object[] { parameters });
        }

        /// <summary>Returns details for the account of given address.</summary>
        /// <param name="address">Address to get account details.</param>
        /// <returns>Details about the account. Returns the default empty basic account for non-existing accounts.</returns>
        public async Task<object> GetAccount(Address address)
        {
            var result = (RawAccount) await Fetch<object>("getAccount", new object[] { address });
            return result.Account;
        }

        /// <summary>Returns the balance of the account of given address.</summary>
        /// <param name="address">Address to check for balance.</param>
        /// <returns>The current balance at the specified address (in smalest unit).</returns>
        public async Task<long> GetBalance(Address address)
        {
            return await Fetch<long>("getBalance", new object[] { address });
        }

        /// <summary>Returns information about a block by hash.</summary>
        /// <param name="hash">Hash of the block to gather information on.</param>
        /// <param name="fullTransactions">If <c>true</c> it returns the full transaction objects, if <c>false</c> only the hashes of the transactions.</param>
        /// <returns>A block object or <c>null</c> when no block was found.</returns>
        public async Task<Block> GetBlockByHash(Hash hash, bool fullTransactions = false)
        {
            return await Fetch<Block>("getBlockByHash", new object[] { hash, fullTransactions });
        }

        /// <summary>Returns information about a block by block number.</summary>
        /// <param name="height">The height of the block to gather information on.</param>
        /// <param name="fullTransactions">If <c>true</c> it returns the full transaction objects, if <c>false</c> only the hashes of the transactions.</param>
        /// <returns>A block object or <c>null</c> when no block was found.</returns>
        public async Task<Block> GetBlockByNumber(int height, bool fullTransactions = false)
        {
            return await Fetch<Block>("getBlockByNumber", new object[] { height, fullTransactions });
        }

        /// <summary>Returns a template to build the next block for mining.
        /// This will consider pool instructions when connected to a pool.
        /// If <c>address</c> and <c>extraData</c> are provided the values are overriden.</summary>
        /// <param name="address">The address to use as a miner for this block. This overrides the address provided during startup or from the pool.</param>
        /// <param name="extraData">Hex-encoded value for the extra data field. This overrides the extra data provided during startup or from the pool.</param>
        /// <returns>A block template object.</returns>
        public async Task<BlockTemplate> GetBlockTemplate(Address address = null, string extraData = "")
        {
            var parameters = new List<object>();
            if (address != null)
            {
                parameters.Add(address);
                parameters.Add(extraData);
            }
            return await Fetch<BlockTemplate>("getBlockTemplate", parameters.ToArray());
        }

        /// <summary>Returns the number of transactions in a block from a block matching the given block hash.</summary>
        /// <param name="hash">Hash of the block.</param>
        /// <returns>Number of transactions in the block found, or <c>null</c>, when no block was found.</returns>
        public async Task<long?> GetBlockTransactionCountByHash(Hash hash)
        {
            return await Fetch<long?>("getBlockTransactionCountByHash", new object[] { hash });
        }

        /// <summary>Returns the number of transactions in a block matching the given block number.</summary>
        /// <param name="height">Height of the block.</param>
        /// <returns>Number of transactions in the block found, or <c>null</c>, when no block was found.</returns>
        public async Task<long?> GetBlockTransactionCountByNumber(long height)
        {
            return await Fetch<long?>("getBlockTransactionCountByNumber", new object[] { height });
        }

        /// <summary>Returns information about a transaction by block hash and transaction index position.</summary>
        /// <param name="hash">Hash of the block containing the transaction.</param>
        /// <param name="index">Index of the transaction in the block.</param>
        /// <returns>A transaction object or <c>null</c> when no transaction was found.<returns>
        public async Task<Transaction> GetTransactionByBlockHashAndIndex(Hash hash, long index)
        {
            return await Fetch<Transaction>("getTransactionByBlockHashAndIndex", new object[] { hash, index });
        }

        /// <summary>Returns information about a transaction by block number and transaction index position.</summary>
        /// <param name="height">Height of the block containing the transaction.</param>
        /// <param name="index">Index of the transaction in the block.</param>
        /// <returns>A transaction object or <c>null</c> when no transaction was found.<returns>
        public async Task<Transaction> GetTransactionByBlockNumberAndIndex(long height, long index)
        {
            return await Fetch<Transaction>("getTransactionByBlockNumberAndIndex", new object[] { height, index });
        }

        /// <summary>Returns the information about a transaction requested by transaction hash.</summary>
        /// <param name="hash">Hash of a transaction.</param>
        /// <returns>A transaction object or <c>null</c> when no transaction was found.</returns>
        public async Task<Transaction> GetTransactionByHash(Hash hash)
        {
            return await Fetch<Transaction>("getTransactionByHash", new object[] { hash });
        }

        /// <summary>Returns the receipt of a transaction by transaction hash.</summary>
        /// <param name="hash">Hash of a transaction.</param>
        /// <returns>A transaction receipt object, or <c>null</c> when no receipt was found.<returns>
        public async Task<TransactionReceipt> GetTransactionReceipt(Hash hash)
        {
            return await Fetch<TransactionReceipt>("getTransactionReceipt", new object[] { hash });
        }

        /// <summary>Returns the latest transactions successfully performed by or for an address.
        /// Note that this information might change when blocks are rewinded on the local state due to forks.</summary>
        /// <param name="address">Address of which transactions should be gathered.</param>
        /// <param name="numberOfTransactions">Number of transactions that shall be returned.</param>
        /// <returns>Array of transactions linked to the requested address.</returns>
        public async Task<Transaction[]> GetTransactionsByAddress(Address address, long numberOfTransactions = 1000)
        {
            return await Fetch<Transaction[]>("getTransactionsByAddress", new object[] { address, numberOfTransactions });
        }

        /// <summary>Returns instructions to mine the next block. This will consider pool instructions when connected to a pool.</summary>
        /// <param name="address">The address to use as a miner for this block. This overrides the address provided during startup or from the pool.</param>
        /// <param name="extraData">Hex-encoded value for the extra data field. This overrides the extra data provided during startup or from the pool.</param>
        /// <returns>Mining work instructions.</returns>
        public async Task<WorkInstructions> GetWork(Address address = null, string extraData = "")
        {
            var parameters = new List<object>();
            if (address != null)
            {
                parameters.Add(address);
                parameters.Add(extraData);
            }
            return await Fetch<WorkInstructions>("getWork", parameters.ToArray());
        }

        /// <summary>Returns the number of hashes per second that the node is mining with.</summary>
        /// <returns>Number of hashes per second.</returns>
        public async Task<double> Hashrate()
        {
            return await Fetch<double>("hashrate");
        }

        /// <summary>Sets the log level of the node.</summary>
        /// <param name="tag">Tag: If `"*"` the log level is set globally, otherwise the log level is applied only on this tag.</param>
        /// <param name="level">Minimum log level to display.</param>
        /// <returns><c>true</c> if the log level was changed, <c>false</c> otherwise.</returns>
        public async Task<bool> Log(string tag, LogLevel level)
        {
            return await Fetch<bool>("log", new object[] { tag, level });
        }

        /// <summary>Returns information on the current mempool situation. This will provide an overview of the number of transactions sorted into buckets based on their fee per byte (in smallest unit).</summary>
        /// <returns>Mempool information.</returns>
        public async Task<MempoolInfo> Mempool()
        {
            return await Fetch<MempoolInfo>("mempool");
        }

        /// <summary>Returns transactions that are currently in the mempool.</summary>
        /// <param name="fullTransactions">If <c>true</c> includes full transactions, if <c>false</c> includes only transaction hashes.</param>
        /// <returns>Array of transactions (either represented by the transaction hash or a transaction object).</returns>
        public async Task<object[]> MempoolContent(bool fullTransactions = false)
        {
            if (fullTransactions)
            {
                return await Fetch<Transaction[]>("mempoolContent", new object[] { fullTransactions });
            }
            else
            {
                return await Fetch<string[]>("mempoolContent", new object[] { fullTransactions });
            }
        }

        /// <summary>Returns the miner address.</summary>
        /// <returns>The miner address configured on the node.</returns>
        public async Task<string> MinerAddress()
        {
            return await Fetch<string>("minerAddress");
        }

        /// <summary>Returns or sets the number of CPU threads for the miner.
        /// When no parameter is given, it returns the current number of miner threads.
        /// When a value is given as parameter, it sets the number of miner threads to that value.</summary>
        /// <param name="threads">The number of threads to allocate for mining.</parameter>
        /// <returns>The number of threads allocated for mining.</returns>
        public async Task<int> MinerThreads(long? threads = null)
        {
            var parameters = new List<object>();
            if (threads != null)
            {
                parameters.Add(threads);
            }
            return await Fetch<int>("minerThreads", parameters.ToArray());
        }

        /// <summary>Returns or sets the minimum fee per byte.
        /// When no parameter is given, it returns the current minimum fee per byte.
        /// When a value is given as parameter, it sets the minimum fee per byte to that value.</summary>
        /// <param name="fee">The new minimum fee per byte.</param>
        /// <returns>The new minimum fee per byte.</returns>
        public async Task<int> MinFeePerByte(int? fee = null)
        {
            var parameters = new List<object>();
            if (fee != null)
            {
                parameters.Add(fee);
            }
            return await Fetch<int>("minFeePerByte", parameters.ToArray());
        }

        /// <summary>Returns true if client is actively mining new blocks.
        /// When no parameter is given, it returns the current state.
        /// When a value is given as parameter, it sets the current state to that value.</summary>
        /// <param name="state">The state to be set.</param>
        /// <returns><c>true</c> if the client is mining, otherwise <c>false</c>.</returns>
        public async Task<bool> Mining(bool? state = null)
        {
            var parameters = new List<object>();
            if (state != null)
            {
                parameters.Add(state);
            }
            return await Fetch<bool>("mining", parameters.ToArray());
        }

        /// <summary>Returns number of peers currently connected to the client.</summary>
        /// <returns>Number of connected peers.</returns>
        public async Task<int> PeerCount()
        {
            return await Fetch<int>("peerCount");
        }

        /// <summary>Returns list of peers known to the client.</summary>
        /// <returns>The list of peers.</returns>
        public async Task<Peer[]>PeerList()
        {
            return await Fetch<Peer[]>("peerList");
        }
    }
}
