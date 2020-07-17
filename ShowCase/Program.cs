using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;

[Serializable]
public struct TestStruct
{
    public int Value1 { get; set; }
    public int Value2 { get; set; }
    public string Value3 { get; set; }
    public double Value4 { get; set; }
}

[Serializable]
public struct ResponseError
{
    public int code { get; set; }
    public string message { get; set; }
}

[Serializable]
public struct Root<T>
{
    public string jsonrpc { get; set; }
    public T result { get; set; }
    public int id { get; set; }
    public ResponseError error { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    public static int id = 0;

    static HttpClient client = null;

    static string url = null;

    static async Task<T> fetch<T>(string method, object[] parameters)
    {
        var serializedParams = JsonSerializer.Serialize(parameters);

        var contentData = new StringContent($@"{{""jsonrpc"": ""2.0"", ""method"": ""consensus"", ""params"": {serializedParams}, ""id"": {id}}}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, contentData);
        var content = response.Content;
        var data = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Root<T>>(data).result;
    }

    static async Task MainAsync()
    {
        client = new HttpClient();
        url = "http://:@127.0.0.1:8648";

        try
        {
            Console.WriteLine(await fetch<string>("consensus", new object[0]));
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
}
