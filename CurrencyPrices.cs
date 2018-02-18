using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Harkenel.Gdax;

namespace warbucks
{
    public class CurrencyPrices
    {
        public static string[] Currencies = new string[]
        {
            "BTC-USD",
            "ETH-USD",
            "BCH-USD",
            "LTC-USD"
        };

        private static List<Dictionary<string, ProductTicker>> PriceHistory = new List<Dictionary<string, ProductTicker>>();

        public static bool IsReady => PriceHistory.Count() > 59;

        public static async Task Run()
        {
            while (true)
            {
                var toAdd = new Dictionary<string, ProductTicker>();

                foreach (var currency in Currencies)
                {

                    var vals = ApiKeys.Instance;

                    var requestAuthenticator = new RequestAuthenticator(vals.ApiKey, vals.ApiPassphrase, vals.ApiSecret);
                    var productClient = new ProductClient(vals.BaseUrl, requestAuthenticator);
                    var productResponse = await productClient.GetProductTickerAsync("BTC-USD");

                    if (productResponse.StatusCode == HttpStatusCode.OK)
                    {
                        toAdd[currency] = productResponse.Value;
                    }
                };

                PriceHistory.Add(toAdd);

                // keep only 30 minute's worth of data
                if (PriceHistory.Count() > 60)
                {
                    PriceHistory = PriceHistory.GetRange(1, PriceHistory.Count() - 1);
                }
                else
                {
                    Console.WriteLine($"{PriceHistory.Count()} of 60 - prepopulating.");
                }

                await Task.Delay((int) TimeSpan.FromSeconds(30).TotalMilliseconds);
            }
        }

        public static decimal GetAverageBuy30MinsAgo(string currency)
        {
            // todo: make this more accurate somehow. Easiest would be to average the first N items, weighted towards the earliest.
            return PriceHistory.First()[currency].ask;
        }

        public static decimal GetCurrentBuy(string currency)
        {
            return PriceHistory.Last()[currency].ask;
        }
    }
}