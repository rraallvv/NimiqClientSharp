using System;
namespace NimiqClientTest
{
    public class Fixtures
    {
        public Fixtures()
        {
        }

        public static string ConsensusSyncing() {
        return @"
            {
                ""jsonrpc"": ""2.0"",
                ""result"": ""syncing"",
                ""id"": 1
            }";
        }

    }
}
