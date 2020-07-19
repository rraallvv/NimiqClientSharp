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
        public async Task TestPeerCount()
        {
            HttpMessageHandlerStub.testData = Fixtures.PeerCount();
    
            var result = await client.PeerCount();

            Assert.AreEqual("peerCount", HttpMessageHandlerStub.latestRequestMethod);

            Assert.AreEqual(6, result);
        }

		/*
        [TestMethod]
        public async Task TestSyncingStateWhenSyncing()
        {
            HttpMessageHandlerStub.testData = Fixtures.Syncing();

            const result = async client.Syncing();

            Assert.AreEqual("syncing", HttpMessageHandlerStub.latestRequestMethod);

            Assert.IsTrue(result is SyncStatus);
            const syncing = result as!SyncStatus
            Assert.AreEqual(578430, syncing.startingBlock);
            Assert.AreEqual(586493, syncing.currentBlock);
            Assert.AreEqual(586493, syncing.highestBlock);
        }

        [TestMethod]
        public async Task TestSyncingStateWhenNotSyncing()
        {
            HttpMessageHandlerStub.testData = Fixtures.SyncingNotSyncing();

            const result = async client.Syncing();

            Assert.AreEqual("syncing", HttpMessageHandlerStub.latestRequestMethod);

            Assert.IsTrue(result is Bool);
            const syncing = result as!Bool
            Assert.AreEqual(false, syncing);
        }


        [TestMethod]
        public async Task TestConsensusState()
        {
            HttpMessageHandlerStub.testData = Fixtures.ConsensusSyncing();

            object result = await client.Consensus();

            Assert.AreEqual("consensus", HttpMessageHandlerStub.latestRequestMethod);

            Assert.AreEqual("syncing", result);
        }
		*/

		[TestMethod]
        public async Task TestPeerListWithPeers()
        {
			HttpMessageHandlerStub.testData = Fixtures.PeerList();

			var result = await client.PeerList();

			Assert.AreEqual("peerList", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual(result.Length, 2);
			Assert.IsNotNull(result[0]);
			Assert.AreEqual("b99034c552e9c0fd34eb95c1cdf17f5e", result[0].id);
			Assert.AreEqual("wss://seed1.nimiq-testnet.com:8080/b99034c552e9c0fd34eb95c1cdf17f5e", result[0].address);
			Assert.AreEqual(PeerAddressState.established, result[0].addressState);
			Assert.AreEqual(PeerConnectionState.established, result[0].connectionState);

			Assert.IsNotNull(result[1]);
			Assert.AreEqual("e37dca72802c972d45b37735e9595cf0", result[1].id);
			Assert.AreEqual("wss://seed4.nimiq-testnet.com:8080/e37dca72802c972d45b37735e9595cf0", result[1].address);
			Assert.AreEqual(PeerAddressState.failed, result[1].addressState);
			Assert.AreEqual(null, result[1].connectionState);
		}

		[TestMethod]
        public async Task TestPeerListWhenEmpty()
        {
			HttpMessageHandlerStub.testData = Fixtures.PeerListEmpty();

			var result = await client.PeerList();

			Assert.AreEqual("peerList", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual(result.Length, 0);
		}

		/*
		[TestMethod]
        public async Task TestPeerNormal()
        {
			HttpMessageHandlerStub.testData = Fixtures.PeerStateNormal();

			var result = await client.PeerState("wss://seed1.nimiq-testnet.com:8080/b99034c552e9c0fd34eb95c1cdf17f5e");

			Assert.AreEqual("peerState", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("wss://seed1.nimiq-testnet.com:8080/b99034c552e9c0fd34eb95c1cdf17f5e", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.IsNotNull(result);
			Assert.AreEqual("b99034c552e9c0fd34eb95c1cdf17f5e", result.id);
			Assert.AreEqual("wss://seed1.nimiq-testnet.com:8080/b99034c552e9c0fd34eb95c1cdf17f5e", result.address);
			Assert.AreEqual(PeerAddressState.established, result.addressState);
			Assert.AreEqual(PeerConnectionState.established, result.connectionState);
		}

		[TestMethod]
        public async Task TestPeerFailed()
        {
			HttpMessageHandlerStub.testData = Fixtures.PeerStateFailed();

			var result = await client.PeerState("wss://seed4.nimiq-testnet.com:8080/e37dca72802c972d45b37735e9595cf0");

			Assert.AreEqual("peerState", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("wss://seed4.nimiq-testnet.com:8080/e37dca72802c972d45b37735e9595cf0", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.IsNotNull(result);
			Assert.AreEqual("e37dca72802c972d45b37735e9595cf0", result.id);
			Assert.AreEqual("wss://seed4.nimiq-testnet.com:8080/e37dca72802c972d45b37735e9595cf0", result.address);
			Assert.AreEqual(PeerAddressState.failed, result.addressState);
			Assert.AreEqual(null, result.connectionState);
		}

		[TestMethod]
        public async Task TestPeerError()
        {
			HttpMessageHandlerStub.testData = Fixtures.PeerStateError();

			Assert.IsTrueThrowsError(try client.PeerState("unknown"))
        { error in
				guard case Error.badMethodCall( _) = error else {
					return XCTFail();
				}
			}
		}

		[TestMethod]
        public async Task TestSetPeerNormal()
        {
			HttpMessageHandlerStub.testData = Fixtures.PeerStateNormal();

			var result = await client.PeerState("wss://seed1.nimiq-testnet.com:8080/b99034c552e9c0fd34eb95c1cdf17f5e", PeerStateCommand.connect);

			Assert.AreEqual("peerState", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("wss://seed1.nimiq-testnet.com:8080/b99034c552e9c0fd34eb95c1cdf17f5e", HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual("connect", HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.IsNotNull(result);
			Assert.AreEqual("b99034c552e9c0fd34eb95c1cdf17f5e", result.id);
			Assert.AreEqual("wss://seed1.nimiq-testnet.com:8080/b99034c552e9c0fd34eb95c1cdf17f5e", result.address);
			Assert.AreEqual(PeerAddressState.established, result.addressState);
			Assert.AreEqual(PeerConnectionState.established, result.connectionState);
		}

		[TestMethod]
        public async Task TestSendRawTransaction()
        {
			HttpMessageHandlerStub.testData = Fixtures.SendTransaction();

			var result = await client.SendRawTransaction("00c3c0d1af80b84c3b3de4e3d79d5c8cc950e044098c969953d68bf9cee68d7b53305dbaac7514a06dae935e40d599caf1bd8a243c00000000000000010000000000000001000dc2e201b5a1755aec80aa4227d5afc6b0de0fcfede8541f31b3c07b9a85449ea9926c1c958628d85a2b481556034ab3d67ff7de28772520813c84aaaf8108f6297c580c");

			Assert.AreEqual("sendRawTransaction", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("00c3c0d1af80b84c3b3de4e3d79d5c8cc950e044098c969953d68bf9cee68d7b53305dbaac7514a06dae935e40d599caf1bd8a243c00000000000000010000000000000001000dc2e201b5a1755aec80aa4227d5afc6b0de0fcfede8541f31b3c07b9a85449ea9926c1c958628d85a2b481556034ab3d67ff7de28772520813c84aaaf8108f6297c580c", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual("81cf3f07b6b0646bb16833d57cda801ad5957e264b64705edeef6191fea0ad63", result);
		}

		[TestMethod]
        public async Task TestCreateRawTransaction()
        {
			HttpMessageHandlerStub.testData = Fixtures.CreateRawTransactionBasic();

			var transaction = OutgoingTransaction(
				from: "NQ39 NY67 X0F0 UTQE 0YER 4JEU B67L UPP8 G0FM",
				fromType: AccountType.basic,
				to: "NQ16 61ET MB3M 2JG6 TBLK BR0D B6EA X6XQ L91U",
				toType: AccountType.basic,
				value: 100000,
				fee: 1
			);

			var result = await client.CreateRawTransaction(transaction);

			Assert.AreEqual("createRawTransaction", HttpMessageHandlerStub.latestRequestMethod);

			var param = HttpMessageHandlerStub.latestRequestParams[0] as! [String: Any]
			Assert.IsTrue(NSDictionary(dictionary: param).isEqual([
				"from": "NQ39 NY67 X0F0 UTQE 0YER 4JEU B67L UPP8 G0FM",
				"fromType": 0,
				"to": "NQ16 61ET MB3M 2JG6 TBLK BR0D B6EA X6XQ L91U",
				"toType": 0,
				"value": 100000,
				"fee": 1,
				"data": null
				] as [String: Any?] as [AnyHashable : Any]));

			Assert.AreEqual("00c3c0d1af80b84c3b3de4e3d79d5c8cc950e044098c969953d68bf9cee68d7b53305dbaac7514a06dae935e40d599caf1bd8a243c00000000000186a00000000000000001000af84c01239b16cee089836c2af5c7b1dbb22cdc0b4864349f7f3805909aa8cf24e4c1ff0461832e86f3624778a867d5f2ba318f92918ada7ae28d70d40c4ef1d6413802", result);
		}

		[TestMethod]
        public async Task TestSendTransaction()
        {
			HttpMessageHandlerStub.testData = Fixtures.SendTransaction();

			var transaction = OutgoingTransaction(
				from: "NQ39 NY67 X0F0 UTQE 0YER 4JEU B67L UPP8 G0FM",
				fromType: AccountType.basic,
				to: "NQ16 61ET MB3M 2JG6 TBLK BR0D B6EA X6XQ L91U",
				toType: AccountType.basic,
				value: 1,
				fee: 1
			);

			var result = await client.SendTransaction(transaction);

			Assert.AreEqual("sendTransaction", HttpMessageHandlerStub.latestRequestMethod);

			var param = HttpMessageHandlerStub.latestRequestParams[0] as! [String: Any]
			Assert.IsTrue(NSDictionary(dictionary: param).isEqual([
				"from": "NQ39 NY67 X0F0 UTQE 0YER 4JEU B67L UPP8 G0FM",
				"fromType": 0,
				"to": "NQ16 61ET MB3M 2JG6 TBLK BR0D B6EA X6XQ L91U",
				"toType": 0,
				"value": 1,
				"fee": 1,
				"data": null
				] as [String: Any?] as [AnyHashable : Any]));

			Assert.AreEqual("81cf3f07b6b0646bb16833d57cda801ad5957e264b64705edeef6191fea0ad63", result);
		}

		[TestMethod]
        public async Task TestGetRawTransactionInfo()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetRawTransactionInfoBasic();

			var result = await client.GetRawTransactionInfo("00c3c0d1af80b84c3b3de4e3d79d5c8cc950e044098c969953d68bf9cee68d7b53305dbaac7514a06dae935e40d599caf1bd8a243c00000000000186a00000000000000001000af84c01239b16cee089836c2af5c7b1dbb22cdc0b4864349f7f3805909aa8cf24e4c1ff0461832e86f3624778a867d5f2ba318f92918ada7ae28d70d40c4ef1d6413802");

			Assert.AreEqual("getRawTransactionInfo", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("00c3c0d1af80b84c3b3de4e3d79d5c8cc950e044098c969953d68bf9cee68d7b53305dbaac7514a06dae935e40d599caf1bd8a243c00000000000186a00000000000000001000af84c01239b16cee089836c2af5c7b1dbb22cdc0b4864349f7f3805909aa8cf24e4c1ff0461832e86f3624778a867d5f2ba318f92918ada7ae28d70d40c4ef1d6413802", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.IsNotNull(result);
			Assert.AreEqual("7784f2f6eaa076fa5cf0e4d06311ad204b2f485de622231785451181e8129091", result.hash);
			Assert.AreEqual("b7cc7f01e0e6f0e07dd9249dc598f4e5ee8801f5", result.from);
			Assert.AreEqual("NQ39 NY67 X0F0 UTQE 0YER 4JEU B67L UPP8 G0FM", result.fromAddress);
			Assert.AreEqual("305dbaac7514a06dae935e40d599caf1bd8a243c", result.to);
			Assert.AreEqual("NQ16 61ET MB3M 2JG6 TBLK BR0D B6EA X6XQ L91U", result.toAddress);
			Assert.AreEqual(100000, result.value);
			Assert.AreEqual(1, result.fee);
		}
		*/
		
		[TestMethod]
        public async Task TestGetTransactionByBlockHashAndIndex()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetTransactionFull();

			var result = await client.GetTransactionByBlockHashAndIndex("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", 0);

			Assert.AreEqual("getTransactionByBlockHashAndIndex", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual(0, HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.IsNotNull(result);
			Assert.AreEqual("78957b87ab5546e11e9540ce5a37ebbf93a0ebd73c0ce05f137288f30ee9f430", result.hash);
			Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", result.blockHash);
			Assert.AreEqual(0, result.transactionIndex);
			Assert.AreEqual("355b4fe2304a9c818b9f0c3c1aaaf4ad4f6a0279", result.from);
			Assert.AreEqual("NQ16 6MDL YQHG 9AE8 32UY 1GX1 MAPL MM7N L0KR", result.fromAddress);
			Assert.AreEqual("4f61c06feeb7971af6997125fe40d629c01af92f", result.to);
			Assert.AreEqual("NQ05 9VGU 0TYE NXBH MVLR E4JY UG6N 5701 MX9F", result.toAddress);
			Assert.AreEqual(2636710000, result.value);
			Assert.AreEqual(0, result.fee);
		}

		[TestMethod]
        public async Task TestGetTransactionByBlockHashAndIndexWhenNotFound()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetTransactionNotFound();

			var result = await client.GetTransactionByBlockHashAndIndex("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", 5);

			Assert.AreEqual("getTransactionByBlockHashAndIndex", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual(5, HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.IsNull(result);
		}

		[TestMethod]
        public async Task TestGetTransactionByBlockNumberAndIndex()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetTransactionFull();

			var result = await client.GetTransactionByBlockNumberAndIndex(11608, 0);

			Assert.AreEqual("getTransactionByBlockNumberAndIndex", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(11608, HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual(0, HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.IsNotNull(result);
			Assert.AreEqual("78957b87ab5546e11e9540ce5a37ebbf93a0ebd73c0ce05f137288f30ee9f430", result.hash);
			Assert.AreEqual(11608, result.blockNumber);
			Assert.AreEqual(0, result.transactionIndex);
			Assert.AreEqual("355b4fe2304a9c818b9f0c3c1aaaf4ad4f6a0279", result.from);
			Assert.AreEqual("NQ16 6MDL YQHG 9AE8 32UY 1GX1 MAPL MM7N L0KR", result.fromAddress);
			Assert.AreEqual("4f61c06feeb7971af6997125fe40d629c01af92f", result.to);
			Assert.AreEqual("NQ05 9VGU 0TYE NXBH MVLR E4JY UG6N 5701 MX9F", result.toAddress);
			Assert.AreEqual(2636710000, result.value);
			Assert.AreEqual(0, result.fee);
		}

		[TestMethod]
        public async Task TestGetTransactionByBlockNumberAndIndexWhenNotFound()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetTransactionNotFound();

			var result = await client.GetTransactionByBlockNumberAndIndex(11608, 0);

			Assert.AreEqual("getTransactionByBlockNumberAndIndex", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(11608, HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual(0, HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.IsNull(result);
		}

		[TestMethod]
        public async Task TestGetTransactionByHash()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetTransactionFull();

			var result = await client.GetTransactionByHash("78957b87ab5546e11e9540ce5a37ebbf93a0ebd73c0ce05f137288f30ee9f430");

			Assert.AreEqual("getTransactionByHash", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("78957b87ab5546e11e9540ce5a37ebbf93a0ebd73c0ce05f137288f30ee9f430", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.IsNotNull(result);
			Assert.AreEqual("78957b87ab5546e11e9540ce5a37ebbf93a0ebd73c0ce05f137288f30ee9f430", result.hash);
			Assert.AreEqual(11608, result.blockNumber);
			Assert.AreEqual("355b4fe2304a9c818b9f0c3c1aaaf4ad4f6a0279", result.from);
			Assert.AreEqual("NQ16 6MDL YQHG 9AE8 32UY 1GX1 MAPL MM7N L0KR", result.fromAddress);
			Assert.AreEqual("4f61c06feeb7971af6997125fe40d629c01af92f", result.to);
			Assert.AreEqual("NQ05 9VGU 0TYE NXBH MVLR E4JY UG6N 5701 MX9F", result.toAddress);
			Assert.AreEqual(2636710000, result.value);
			Assert.AreEqual(0, result.fee);
		}

		[TestMethod]
        public async Task TestGetTransactionByHashWhenNotFound()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetTransactionNotFound();

			var result = await client.GetTransactionByHash("78957b87ab5546e11e9540ce5a37ebbf93a0ebd73c0ce05f137288f30ee9f430");

			Assert.AreEqual("getTransactionByHash", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("78957b87ab5546e11e9540ce5a37ebbf93a0ebd73c0ce05f137288f30ee9f430", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.IsNull(result);
		}

		[TestMethod]
        public async Task TestGetTransactionByHashForContractCreation()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetTransactionContractCreation();

			var result = await client.GetTransactionByHash("539f6172b19f63be376ab7e962c368bb5f611deff6b159152c4cdf509f7daad2");

			Assert.AreEqual("getTransactionByHash", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("539f6172b19f63be376ab7e962c368bb5f611deff6b159152c4cdf509f7daad2", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.IsNotNull(result);
			Assert.AreEqual("539f6172b19f63be376ab7e962c368bb5f611deff6b159152c4cdf509f7daad2", result.hash);
			Assert.AreEqual("96fef80f517f0b2704476dee48da147049b591e8f034e5bf93f1f6935fd51b85", result.blockHash);
			Assert.AreEqual(1102500, result.blockNumber);
			Assert.AreEqual(1590148157, result.timestamp);
			Assert.AreEqual(7115, result.confirmations);
			Assert.AreEqual("d62d519b3478c63bdd729cf2ccb863178060c64a", result.from);
			Assert.AreEqual("NQ53 SQNM 36RL F333 PPBJ KKRC RE33 2X06 1HJA", result.fromAddress);
			Assert.AreEqual("a22eaf17848130c9b370e42ff7d345680df245e1", result.to);
			Assert.AreEqual("NQ87 L8PA X5U4 G4QC KCTG UGPY FLS5 D06Y 4HF1", result.toAddress);
			Assert.AreEqual(5000000000, result.value);
			Assert.AreEqual(0, result.fee);
			Assert.AreEqual("d62d519b3478c63bdd729cf2ccb863178060c64af5ad55071730d3b9f05989481eefbda7324a44f8030c63b9444960db429081543939166f05116cebc37bd6975ac9f9e3bb43a5ab0b010010d2de", result.data);
			Assert.AreEqual(1, result.flags);
		}

		/*
		[TestMethod]
        public async Task TestGetTransactionReceipt()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetTransactionReceiptFound();

			var result = await client.GetTransactionReceipt("fd8e46ae55c5b8cd7cb086cf8d6c81f941a516d6148021d55f912fb2ca75cc8e");

			Assert.AreEqual("getTransactionReceipt", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("fd8e46ae55c5b8cd7cb086cf8d6c81f941a516d6148021d55f912fb2ca75cc8e", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.IsNotNull(result);
			Assert.AreEqual("fd8e46ae55c5b8cd7cb086cf8d6c81f941a516d6148021d55f912fb2ca75cc8e", result.transactionHash);
			Assert.AreEqual(-1, result.transactionIndex);
			Assert.AreEqual(11608, result.blockNumber);
			Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", result.blockHash);
			Assert.AreEqual(1523412456, result.timestamp);
			Assert.AreEqual(718846, result.confirmations);
		}

		[TestMethod]
        public async Task TestGetTransactionReceiptWhenNotFound()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetTransactionReceiptNotFound();

			var result = await client.GetTransactionReceipt("unknown");

			Assert.AreEqual("getTransactionReceipt", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("unknown", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.IsNull(result);
		}
		*/

		[TestMethod]
        public async Task TestGetTransactionsByAddress()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetTransactionsFound();

			var result = await client.GetTransactionsByAddress("NQ05 9VGU 0TYE NXBH MVLR E4JY UG6N 5701 MX9F");

			Assert.AreEqual("getTransactionsByAddress", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("NQ05 9VGU 0TYE NXBH MVLR E4JY UG6N 5701 MX9F", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(3, result.Length);
			Assert.IsNotNull(result[0]);
			Assert.AreEqual("a514abb3ee4d3fbedf8a91156fb9ec4fdaf32f0d3d3da3c1dbc5fd1ee48db43e", result[0].hash);
			Assert.IsNotNull(result[1]);
			Assert.AreEqual("c8c0f586b11c7f39873c3de08610d63e8bec1ceaeba5e8a3bb13c709b2935f73", result[1].hash);
			Assert.IsNotNull(result[2]);
			Assert.AreEqual("fd8e46ae55c5b8cd7cb086cf8d6c81f941a516d6148021d55f912fb2ca75cc8e", result[2].hash);
		}

		[TestMethod]
        public async Task TestGetTransactionsByAddressWhenNoFound()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetTransactionsNotFound();

			var result = await client.GetTransactionsByAddress("NQ10 9VGU 0TYE NXBH MVLR E4JY UG6N 5701 MX9F");

			Assert.AreEqual("getTransactionsByAddress", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("NQ10 9VGU 0TYE NXBH MVLR E4JY UG6N 5701 MX9F", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(0, result.Length);
		}

		[TestMethod]
        public async Task TestMempoolContentHashesOnly()
        {
			HttpMessageHandlerStub.testData = Fixtures.MempoolContentHashesOnly();

			var result = await client.MempoolContent();

			Assert.AreEqual("mempoolContent", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(false, HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(3, result.Length);
			Assert.IsNotNull(result[0]);
			Assert.AreEqual("5bb722c2afe25c18ba33d453b3ac2c90ac278c595cc92f6188c8b699e8fb006a", result[0]);
			Assert.IsNotNull(result[1]);
			Assert.AreEqual("f59a30e0a7e3348ef569225db1f4c29026aeac4350f8c6e751f669eddce0c718", result[1]);
			Assert.IsNotNull(result[2]);
			Assert.AreEqual("9cd9c1d0ffcaebfcfe86bc2ae73b4e82a488de99c8e3faef92b05432bb94519c", result[2]);
		}

		[TestMethod]
        public async Task TestMempoolContentFullTransactions()
        {
			HttpMessageHandlerStub.testData = Fixtures.MempoolContentFullTransactions();

			var result = await client.MempoolContent(true);

			Assert.AreEqual("mempoolContent", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(true, HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(3, result.Length);
			Assert.IsNotNull(result[0]);
			Assert.AreEqual("5bb722c2afe25c18ba33d453b3ac2c90ac278c595cc92f6188c8b699e8fb006a", ((Transaction)result[0]).hash);
			Assert.IsNotNull(result[1]);
			Assert.AreEqual("f59a30e0a7e3348ef569225db1f4c29026aeac4350f8c6e751f669eddce0c718", ((Transaction)result[1]).hash);
			Assert.IsNotNull(result[2]);
			Assert.AreEqual("9cd9c1d0ffcaebfcfe86bc2ae73b4e82a488de99c8e3faef92b05432bb94519c", ((Transaction)result[2]).hash);
		}

		[TestMethod]
        public async Task TestMempoolWhenFull()
        {
			HttpMessageHandlerStub.testData = Fixtures.Mempool();

			var result = await client.Mempool();

			Assert.AreEqual("mempool", HttpMessageHandlerStub.latestRequestMethod);

			Assert.IsNotNull(result);
			Assert.AreEqual(3, result.total);
			CollectionAssert.AreEqual(new long[] { 1 }, result.buckets);
			Assert.AreEqual(3, result.transactionsPerBucket[1]);
		}

		[TestMethod]
        public async Task TestMempoolWhenEmpty()
        {
			HttpMessageHandlerStub.testData = Fixtures.MempoolEmpty();

			var result = await client.Mempool();

			Assert.AreEqual("mempool", HttpMessageHandlerStub.latestRequestMethod);

			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.total);
			CollectionAssert.AreEqual(new long[0], result.buckets);
			Assert.AreEqual(0, result.transactionsPerBucket.Count);
		}

		[TestMethod]
        public async Task TestMinFeePerByte()
        {
			HttpMessageHandlerStub.testData = Fixtures.MinFeePerByte();

			var result = await client.MinFeePerByte();

			Assert.AreEqual("minFeePerByte", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual(0, result);
		}

		[TestMethod]
        public async Task TestSetMinFeePerByte()
        {
			HttpMessageHandlerStub.testData = Fixtures.MinFeePerByte();

			var result = await client.MinFeePerByte(0);

			Assert.AreEqual("minFeePerByte", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(0, HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(0, result);
		}

		[TestMethod]
        public async Task TestMining()
        {
			HttpMessageHandlerStub.testData = Fixtures.MiningState();

			var result = await client.Mining();

			Assert.AreEqual("mining", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual(false, result);
		}

		[TestMethod]
        public async Task TestSetMining()
        {
			HttpMessageHandlerStub.testData = Fixtures.MiningState();

			var result = await client.Mining(false);

			Assert.AreEqual("mining", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(false, HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(false, result);
		}

		[TestMethod]
        public async Task TestHashrate()
        {
			HttpMessageHandlerStub.testData = Fixtures.Hashrate();

			var result = await client.Hashrate();

			Assert.AreEqual("hashrate", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual(52982.2731, result);
		}

		[TestMethod]
        public async Task TestMinerThreads()
        {
			HttpMessageHandlerStub.testData = Fixtures.MinerThreads();

			var result = await client.MinerThreads();

			Assert.AreEqual("minerThreads", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual(2, result);
		}

		[TestMethod]
        public async Task TestSetMinerThreads()
        {
			HttpMessageHandlerStub.testData = Fixtures.MinerThreads();

			var result = await client.MinerThreads(2);

			Assert.AreEqual("minerThreads", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(2, HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(2, result);
		}

		[TestMethod]
        public async Task TestMinerAddress()
        {
			HttpMessageHandlerStub.testData = Fixtures.MinerAddress();

			var result = await client.MinerAddress();

			Assert.AreEqual("minerAddress", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual("NQ39 NY67 X0F0 UTQE 0YER 4JEU B67L UPP8 G0FM", result);
		}

		/*
		[TestMethod]
        public async Task TestPool()
        {
			HttpMessageHandlerStub.testData = Fixtures.PoolSushipool();

			var result = await client.Pool();

			Assert.AreEqual("pool", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual("us.sushipool.com:443", result);
		}

		[TestMethod]
        public async Task TestSetPool()
        {
			HttpMessageHandlerStub.testData = Fixtures.PoolSushipool();

			var result = await client.Pool("us.sushipool.com:443");

			Assert.AreEqual("pool", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("us.sushipool.com:443", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual("us.sushipool.com:443", result);
		}

		[TestMethod]
        public async Task TestGetPoolWhenNoPool()
        {
			HttpMessageHandlerStub.testData = Fixtures.PoolNoPool();

			var result = await client.Pool();

			Assert.AreEqual("pool", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual(null, result);
		}

		[TestMethod]
        public async Task TestPoolConnectionState()
        {
			HttpMessageHandlerStub.testData = Fixtures.PoolConnectionState();

			var result = await client.PoolConnectionState();

			Assert.AreEqual("poolConnectionState", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual(PoolConnectionState.closed, result);
		}

		[TestMethod]
        public async Task TestPoolConfirmedBalance()
        {
			HttpMessageHandlerStub.testData = Fixtures.PoolConfirmedBalance();

			var result = await client.PoolConfirmedBalance();

			Assert.AreEqual("poolConfirmedBalance", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual(12000, result);
		}
		*/

		[TestMethod]
        public async Task TestGetWork()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetWork();

			var result = await client.GetWork();

			Assert.AreEqual("getWork", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual("00015a7d47ddf5152a7d06a14ea291831c3fc7af20b88240c5ae839683021bcee3e279877b3de0da8ce8878bf225f6782a2663eff9a03478c15ba839fde9f1dc3dd9e5f0cd4dbc96a30130de130eb52d8160e9197e2ccf435d8d24a09b518a5e05da87a8658ed8c02531f66a7d31757b08c88d283654ed477e5e2fec21a7ca8449241e00d620000dc2fa5e763bda00000000", result.data);
			Assert.AreEqual("11fad9806b8b4167517c162fa113c09606b44d24f8020804a0f756db085546ff585adfdedad9085d36527a8485b497728446c35b9b6c3db263c07dd0a1f487b1639aa37ff60ba3cf6ed8ab5146fee50a23ebd84ea37dca8c49b31e57d05c9e6c57f09a3b282b71ec2be66c1bc8268b5326bb222b11a0d0a4acd2a93c9e8a8713fe4383e9d5df3b1bf008c535281086b2bcc20e494393aea1475a5c3f13673de2cf7314d201b7cc7f01e0e6f0e07dd9249dc598f4e5ee8801f50000000000", result.suffix);
			Assert.AreEqual(503371296, result.target);
			Assert.AreEqual("nimiq-argon2", result.algorithm);
		}

		[TestMethod]
        public async Task TestGetWorkWithOverride()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetWork();

			var result = await client.GetWork("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET", "");

			Assert.AreEqual("getWork", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET", HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual("", HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.AreEqual("00015a7d47ddf5152a7d06a14ea291831c3fc7af20b88240c5ae839683021bcee3e279877b3de0da8ce8878bf225f6782a2663eff9a03478c15ba839fde9f1dc3dd9e5f0cd4dbc96a30130de130eb52d8160e9197e2ccf435d8d24a09b518a5e05da87a8658ed8c02531f66a7d31757b08c88d283654ed477e5e2fec21a7ca8449241e00d620000dc2fa5e763bda00000000", result.data);
			Assert.AreEqual("11fad9806b8b4167517c162fa113c09606b44d24f8020804a0f756db085546ff585adfdedad9085d36527a8485b497728446c35b9b6c3db263c07dd0a1f487b1639aa37ff60ba3cf6ed8ab5146fee50a23ebd84ea37dca8c49b31e57d05c9e6c57f09a3b282b71ec2be66c1bc8268b5326bb222b11a0d0a4acd2a93c9e8a8713fe4383e9d5df3b1bf008c535281086b2bcc20e494393aea1475a5c3f13673de2cf7314d201b7cc7f01e0e6f0e07dd9249dc598f4e5ee8801f50000000000", result.suffix);
			Assert.AreEqual(503371296, result.target);
			Assert.AreEqual("nimiq-argon2", result.algorithm);
		}

		[TestMethod]
        public async Task TestGetBlockTemplate()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetWorkBlockTemplate();

			var result = await client.GetBlockTemplate();

			Assert.AreEqual("getBlockTemplate", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual(901883, result.header.height);
			Assert.AreEqual(503371226, result.target);
			Assert.AreEqual("17e250f1977ae85bdbe09468efef83587885419ee1074ddae54d3fb5a96e1f54", result.body.hash);
		}

		[TestMethod]
        public async Task TestGetBlockTemplateWithOverride()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetWorkBlockTemplate();

			var result = await client.GetBlockTemplate("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET", "");

			Assert.AreEqual("getBlockTemplate", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET", HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual("", HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.AreEqual(901883, result.header.height);
			Assert.AreEqual(503371226, result.target);
			Assert.AreEqual("17e250f1977ae85bdbe09468efef83587885419ee1074ddae54d3fb5a96e1f54", result.body.hash);
		}

		/*
		[TestMethod]
        public async Task TestSubmitBlock()
        {
			HttpMessageHandlerStub.testData = Fixtures.SubmitBlock();

			var blockHex = "000100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000f6ba2bbf7e1478a209057000471d73fbdc28df0b717747d929cfde829c4120f62e02da3d162e20fa982029dbde9cc20f6b431ab05df1764f34af4c62a4f2b33f1f010000000000015ac3185f000134990001000000000000000000000000000000000000000007546573744e657400000000"

			try! client.SubmitBlock(blockHex);

			Assert.AreEqual("submitBlock", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(blockHex, HttpMessageHandlerStub.latestRequestParams[0]);
		}
		*/

		[TestMethod]
        public async Task TestAccounts()
		{
			HttpMessageHandlerStub.testData = Fixtures.Accounts();

			var result = await client.Accounts();

			Assert.AreEqual(HttpMessageHandlerStub.latestRequestMethod, "accounts");

			Assert.AreEqual(3, result.Length);

			Assert.IsNotNull(result[0]);
			var account = (Account)result[0];
			Assert.AreEqual("f925107376081be421f52d64bec775cc1fc20829", account.id);
			Assert.AreEqual("NQ33 Y4JH 0UTN 10DX 88FM 5MJB VHTM RGFU 4219", account.address);
			Assert.AreEqual(0, account.balance);
			Assert.AreEqual(AccountType.basic, account.type);

			Assert.IsNotNull(result[1]);
			var vesting = (VestingContract)result[1];
			Assert.AreEqual("ebcbf0de7dae6a42d1c12967db9b2287bf2f7f0f", vesting.id);
			Assert.AreEqual("NQ09 VF5Y 1PKV MRM4 5LE1 55KV P6R2 GXYJ XYQF", vesting.address);
			Assert.AreEqual(52500000000000, vesting.balance);
			Assert.AreEqual(AccountType.vesting, vesting.type);
			Assert.AreEqual("fd34ab7265a0e48c454ccbf4c9c61dfdf68f9a22", vesting.owner);
			Assert.AreEqual("NQ62 YLSA NUK5 L3J8 QHAC RFSC KHGV YPT8 Y6H2", vesting.ownerAddress);
			Assert.AreEqual(1, vesting.vestingStart);
			Assert.AreEqual(259200, vesting.vestingStepBlocks);
			Assert.AreEqual(2625000000000, vesting.vestingStepAmount);
			Assert.AreEqual(52500000000000, vesting.vestingTotalAmount);

			Assert.IsNotNull(result[2]);
			var htlc = (HTLC)result[2];
			Assert.AreEqual("4974636bd6d34d52b7d4a2ee4425dc2be72a2b4e", htlc.id);
			Assert.AreEqual("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET", htlc.address);
			Assert.AreEqual(1000000000, htlc.balance);
			Assert.AreEqual(AccountType.htlc, htlc.type);
			Assert.AreEqual("d62d519b3478c63bdd729cf2ccb863178060c64a", htlc.sender);
			Assert.AreEqual("NQ53 SQNM 36RL F333 PPBJ KKRC RE33 2X06 1HJA", htlc.senderAddress);
			Assert.AreEqual("f5ad55071730d3b9f05989481eefbda7324a44f8", htlc.recipient);
			Assert.AreEqual("NQ41 XNNM A1QP 639T KU2R H541 VTVV LUR4 LH7Q", htlc.recipientAddress);
			Assert.AreEqual("df331b3c8f8a889703092ea05503779058b7f44e71bc57176378adde424ce922", htlc.hashRoot);
			Assert.AreEqual(1, htlc.hashAlgorithm);
			Assert.AreEqual(1, htlc.hashCount);
			Assert.AreEqual(1105605, htlc.timeout);
			Assert.AreEqual(1000000000, htlc.totalAmount);
		}

		/*
		[TestMethod]
        public async Task TestCreateAccount()
        {
			HttpMessageHandlerStub.testData = Fixtures.CreateAccount();

			var result = await client.CreateAccount();

			Assert.AreEqual("createAccount", HttpMessageHandlerStub.latestRequestMethod);

			Assert.IsNotNull(result);
			Assert.AreEqual("b6edcc7924af5a05af6087959c7233ec2cf1a5db", result.id);
			Assert.AreEqual("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET", result.address);
			Assert.AreEqual("4f6d35cc47b77bf696b6cce72217e52edff972855bd17396b004a8453b020747", result.publicKey);
		}

		[TestMethod]
        public async Task TestGetBalance()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetBalance();

			var result = await client.GetBalance("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET");

			Assert.AreEqual("getBalance", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(1200000, result);
		}

		[TestMethod]
        public async Task TestGetAccount()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetAccountBasic();

			var result = await client.GetAccount("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET");

			Assert.AreEqual("getAccount", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.IsTrue(result is Account);
			var account = result as! Account
			Assert.AreEqual("b6edcc7924af5a05af6087959c7233ec2cf1a5db", account.id);
			Assert.AreEqual("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET", account.address);
			Assert.AreEqual(1200000, account.balance);
			Assert.AreEqual(AccountType.basic, account.type);
		}

		[TestMethod]
        public async Task TestGetAccountForVestingContract()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetAccountVesting();

			var result = await client.GetAccount("NQ09 VF5Y 1PKV MRM4 5LE1 55KV P6R2 GXYJ XYQF");

			Assert.AreEqual("getAccount", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("NQ09 VF5Y 1PKV MRM4 5LE1 55KV P6R2 GXYJ XYQF", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.IsTrue(result is VestingContract);
			var contract = result as! VestingContract
			Assert.AreEqual("ebcbf0de7dae6a42d1c12967db9b2287bf2f7f0f", contract.id);
			Assert.AreEqual("NQ09 VF5Y 1PKV MRM4 5LE1 55KV P6R2 GXYJ XYQF", contract.address);
			Assert.AreEqual(52500000000000, contract.balance);
			Assert.AreEqual(AccountType.vesting, contract.type);
			Assert.AreEqual("fd34ab7265a0e48c454ccbf4c9c61dfdf68f9a22", contract.owner);
			Assert.AreEqual("NQ62 YLSA NUK5 L3J8 QHAC RFSC KHGV YPT8 Y6H2", contract.ownerAddress);
			Assert.AreEqual(1, contract.vestingStart);
			Assert.AreEqual(259200, contract.vestingStepBlocks);
			Assert.AreEqual(2625000000000, contract.vestingStepAmount);
			Assert.AreEqual(52500000000000, contract.vestingTotalAmount);
		}

		[TestMethod]
        public async Task TestGetAccountForHashedTimeLockedContract()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetAccountVestingHtlc();

			var result = await client.GetAccount("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET");

			Assert.AreEqual("getAccount", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.IsTrue(result is HTLC);
			var contract = result as! HTLC
			Assert.AreEqual("4974636bd6d34d52b7d4a2ee4425dc2be72a2b4e", contract.id);
			Assert.AreEqual("NQ46 NTNU QX94 MVD0 BBT0 GXAR QUHK VGNF 39ET", contract.address);
			Assert.AreEqual(1000000000, contract.balance);
			Assert.AreEqual(AccountType.htlc, contract.type);
			Assert.AreEqual("d62d519b3478c63bdd729cf2ccb863178060c64a", contract.sender);
			Assert.AreEqual("NQ53 SQNM 36RL F333 PPBJ KKRC RE33 2X06 1HJA", contract.senderAddress);
			Assert.AreEqual("f5ad55071730d3b9f05989481eefbda7324a44f8", contract.recipient);
			Assert.AreEqual("NQ41 XNNM A1QP 639T KU2R H541 VTVV LUR4 LH7Q", contract.recipientAddress);
			Assert.AreEqual("df331b3c8f8a889703092ea05503779058b7f44e71bc57176378adde424ce922", contract.hashRoot);
			Assert.AreEqual(1, contract.hashAlgorithm);
			Assert.AreEqual(1, contract.hashCount);
			Assert.AreEqual(1105605, contract.timeout);
			Assert.AreEqual(1000000000, contract.totalAmount);
		}

		[TestMethod]
        public async Task TestBlockNumber()
        {
			HttpMessageHandlerStub.testData = Fixtures.BlockNumber();

			var result = await client.BlockNumber();

			Assert.AreEqual("blockNumber", HttpMessageHandlerStub.latestRequestMethod);

			Assert.AreEqual(748883, result);
		}
		*/

		[TestMethod]
        public async Task TestGetBlockTransactionCountByHash()
        {
			HttpMessageHandlerStub.testData = Fixtures.BlockTransactionCountFound();

			var result = await client.GetBlockTransactionCountByHash("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786");

			Assert.AreEqual("getBlockTransactionCountByHash", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(2, result);
		}

		[TestMethod]
        public async Task TestGetBlockTransactionCountByHashWhenNotFound()
        {
			HttpMessageHandlerStub.testData = Fixtures.BlockTransactionCountNotFound();

			var result = await client.GetBlockTransactionCountByHash("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786");

			Assert.AreEqual("getBlockTransactionCountByHash", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(null, result);
		}

		[TestMethod]
        public async Task TestGetBlockTransactionCountByNumber()
        {
			HttpMessageHandlerStub.testData = Fixtures.BlockTransactionCountFound();

			var result = await client.GetBlockTransactionCountByNumber(11608);

			Assert.AreEqual("getBlockTransactionCountByNumber", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(11608, HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(2, result);
		}

		[TestMethod]
        public async Task TestGetBlockTransactionCountByNumberWhenNotFound()
        {
			HttpMessageHandlerStub.testData = Fixtures.BlockTransactionCountNotFound();

			var result = await client.GetBlockTransactionCountByNumber(11608);

			Assert.AreEqual("getBlockTransactionCountByNumber", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(11608, HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(null, result);
		}

		[TestMethod]
        public async Task TestGetBlockByHash()
        {
            HttpMessageHandlerStub.testData = Fixtures.GetBlockFound();

            var result = await client.GetBlockByHash("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786");

            Assert.AreEqual("getBlockByHash", HttpMessageHandlerStub.latestRequestMethod);
            Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", (string)HttpMessageHandlerStub.latestRequestParams[0]);
            Assert.AreEqual(false, (bool)HttpMessageHandlerStub.latestRequestParams[1]);

            Assert.IsNotNull(result);
            Assert.AreEqual(11608, result.number);
            Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", result.hash);
            Assert.AreEqual(739224, result.confirmations);
            CollectionAssert.AreEqual(new string[] {
                "78957b87ab5546e11e9540ce5a37ebbf93a0ebd73c0ce05f137288f30ee9f430",
                "fd8e46ae55c5b8cd7cb086cf8d6c81f941a516d6148021d55f912fb2ca75cc8e",
            }, result.transactions);
        }

		/*
		[TestMethod]
        public async Task TestGetBlockByHashWithTransactions()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetBlockWithTransactions();

			var result = await client.GetBlockByHash("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", true);

			Assert.AreEqual("getBlockByHash", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual(true, HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.IsNotNull(result);
			Assert.AreEqual(11608, result.number);
			Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", result.hash);
			Assert.AreEqual(739501, result.confirmations);

			Assert.AreEqual(2, result.transactions.Length);
			Assert.IsTrue(result.transactions[0] is Transaction);
			Assert.IsTrue(result.transactions[1] is Transaction);
		}

		[TestMethod]
        public async Task TestGetBlockByHashNotFound()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetBlockNotFound();

			var result = await client.GetBlockByHash("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786");

			Assert.AreEqual("getBlockByHash", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual(false, HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.IsNull(result);
		}
		*/

		[TestMethod]
		public async Task TestGetBlockByNumber()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetBlockFound();

			var result = await client.GetBlockByNumber(11608);

			Assert.AreEqual("getBlockByNumber", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(11608, HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual(false, HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.IsNotNull(result);
			Assert.AreEqual(11608, result.number);
			Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", result.hash);
			Assert.AreEqual(739224, result.confirmations);
			CollectionAssert.AreEqual(new string[] {
				"78957b87ab5546e11e9540ce5a37ebbf93a0ebd73c0ce05f137288f30ee9f430",
				"fd8e46ae55c5b8cd7cb086cf8d6c81f941a516d6148021d55f912fb2ca75cc8e",
			}, result.transactions);
		}

		[TestMethod]
		public async Task TestGetBlockByNumberWithTransactions()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetBlockWithTransactions();

			var result = await client.GetBlockByNumber(11608, true);

			Assert.AreEqual("getBlockByNumber", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(11608, HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual(true, HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.IsNotNull(result);
			Assert.AreEqual(11608, result.number);
			Assert.AreEqual("bc3945d22c9f6441409a6e539728534a4fc97859bda87333071fad9dad942786", result.hash);
			Assert.AreEqual(739501, result.confirmations);

			Assert.AreEqual(2, result.transactions.Length);
			Assert.IsTrue(result.transactions[0] is Transaction);
			Assert.IsTrue(result.transactions[1] is Transaction);
		}

		[TestMethod]
        public async Task TestGetBlockByNumberNotFound()
        {
			HttpMessageHandlerStub.testData = Fixtures.GetBlockNotFound();

			var result = await client.GetBlockByNumber(11608);

			Assert.AreEqual("getBlockByNumber", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual(11608, HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual(false, HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.IsNull(result);
		}

		/*
		[TestMethod]
        public async Task TestConstant()
        {
			HttpMessageHandlerStub.testData = Fixtures.Constant();

			var result = await client.Constant("BaseConsensus.MAX_ATTEMPTS_TO_FETCH");

			Assert.AreEqual("constant", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("BaseConsensus.MAX_ATTEMPTS_TO_FETCH", HttpMessageHandlerStub.latestRequestParams[0]);

			Assert.AreEqual(5, result);
		}

		[TestMethod]
        public async Task TestSetConstant()
        {
			HttpMessageHandlerStub.testData = Fixtures.Constant();

			var result = await client.Constant("BaseConsensus.MAX_ATTEMPTS_TO_FETCH", 10);

			Assert.AreEqual("constant", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("BaseConsensus.MAX_ATTEMPTS_TO_FETCH", HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual(10, HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.AreEqual(5, result);
		}

		[TestMethod]
        public async Task TestResetConstant()
        {
			HttpMessageHandlerStub.testData = Fixtures.Constant();

			var result = await client.ResetConstant("BaseConsensus.MAX_ATTEMPTS_TO_FETCH");

			Assert.AreEqual("constant", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("BaseConsensus.MAX_ATTEMPTS_TO_FETCH", HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual("reset", HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.AreEqual(5, result);
		}
		*/

		[TestMethod]
        public async Task TestLog()
        {
			HttpMessageHandlerStub.testData = Fixtures.Log();

			var result = await client.Log("*", LogLevel.verbose);

			Assert.AreEqual("log", HttpMessageHandlerStub.latestRequestMethod);
			Assert.AreEqual("*", HttpMessageHandlerStub.latestRequestParams[0]);
			Assert.AreEqual("verbose", HttpMessageHandlerStub.latestRequestParams[1]);

			Assert.AreEqual(true, result);
		}		
    }
}
