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

    /// <summary>Transaction returned by the server. Can be of type Hash or Transaction.</summary>
    class HashOrTransactionConverter : JsonConverter<object[]>
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
        public async Task<string> Consensus()
        {
            return await Fetch<string>("consensus");
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
    }
}
