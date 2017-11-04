using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CoinLib.Tests.Controllers
{
    public static class HttpClientExtensions
    {
        public static async Task<T> GetAsync<T>(this HttpClient client, string requestUrl)
        {
            var response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}