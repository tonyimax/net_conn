using System;
using System.Net.Http;
using System.Threading.Tasks;

using net_conn;

public class Http
{
    bool _isFromChina = false;

    public bool IsFromChina
    {
        get => _isFromChina;
        set => _isFromChina = value;
    }

    public async Task CheckIp(string ip)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://qifu-api.baidubce.com/ip/geo/v1/district?ip="+ip);
                response.EnsureSuccessStatusCode(); 
                string responseBody = await response.Content.ReadAsStringAsync();
                _isFromChina=responseBody.Contains("中国");
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
    }
}