using System;
using System.Net.Http;
using System.Threading.Tasks;
using Nimiq;

class Program
{
    static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    static async Task MainAsync()
    {
        var client = new NimiqClient(new Config());
        try
        {
            Console.WriteLine(await client.Consensus());
        }
        catch (Exception e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
}
