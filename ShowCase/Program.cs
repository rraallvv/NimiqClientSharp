using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        HttpClient client = new HttpClient();
        string url = "http://:@127.0.0.1:8648";

        try
        {
            var contentData = new StringContent(@"{""jsonrpc"": ""2.0"", ""method"": ""consensus"", ""params"": [], ""id"": 1}", System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, contentData);
            var content = response.Content;
            var data = await content.ReadAsStringAsync();
            Console.WriteLine(data);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
}
