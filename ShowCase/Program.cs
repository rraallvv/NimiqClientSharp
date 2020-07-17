using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;
using Nimiq;

class Program
{
    static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    static async Task MainAsync()
    {
        var client = new NimiqClient();
        try
        {
            Console.WriteLine(await client.Consensus());
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
}
