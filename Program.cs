using System;
using System.Net;
using System.Threading.Tasks;
using Harkenel.Gdax;

namespace warbucks
{
    class Program
    {
        static void Main(string[] args)
        {
            Test().Wait();
        }

        private static async Task Test()
        {
            var vals = ApiKeys.Instance;
            
            var requestAuthenticator = new RequestAuthenticator(vals.ApiKey, vals.ApiPassphrase, vals.ApiSecret);
            var productClient = new ProductClient(vals.BaseUrl, requestAuthenticator);
            var productResponse = await productClient.GetProductTickerAsync("BTC-USD");

            if (productResponse.StatusCode == HttpStatusCode.OK)
            {
                var ticker = productResponse.Value;
                Console.WriteLine("Price: {0}", ticker.price);
            }
        }
    }
}
