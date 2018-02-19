using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Harkenel.Gdax;

namespace warbucks
{
    class Program
    {
        static void Main(string[] args)
        {
            StartProgram().Wait();
        }

        private static async Task StartProgram()
        {
            // begin grabbing prices
            CurrencyPrices.Run();

            // todo: use events!
            await Task.Delay(TimeSpan.FromMinutes(30));

            while (!CurrencyPrices.IsReady)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            while (true)
            {
                var vals = ApiKeys.Instance;

                var requestAuthenticator = new RequestAuthenticator(vals.ApiKey, vals.ApiPassphrase, vals.ApiSecret);
                var accountClient = new AccountClient(vals.BaseUrl, requestAuthenticator);
                var accountResponse = await accountClient.ListAccountsAsync();

                if (accountResponse.StatusCode == HttpStatusCode.OK)
                {
                    // find main account - one will have everything, others may have fractions of a cent
                    // don't worry about converting balances because it won't matter
                    var mainAccount = accountResponse.Value.OrderByDescending(x => x.balance).First();

                    var currentPercentageDiff = (CurrencyPrices.GetCurrentBuy(mainAccount.currency) -
                            CurrencyPrices.GetAverageBuy30MinsAgo(mainAccount.currency)) /
                        CurrencyPrices.GetAverageBuy30MinsAgo(mainAccount.currency);

                    var otherPercentageDiffs = new Dictionary<string, decimal>();

                    CurrencyPrices.Currencies.Where(x => x != mainAccount.currency).ToList().ForEach(otherCurrency =>
                    {
                        var otherDiff = (CurrencyPrices.GetCurrentBuy(mainAccount.currency) -
                                CurrencyPrices.GetAverageBuy30MinsAgo(mainAccount.currency)) /
                            CurrencyPrices.GetAverageBuy30MinsAgo(mainAccount.currency);

                        otherPercentageDiffs[otherCurrency] = otherDiff;
                    });

                    var currentCurrencyPrice = CurrencyPrices.GetCurrentBuy(mainAccount.currency);

                    Console.WriteLine($"We have {(mainAccount.balance * currentCurrencyPrice).ToString("C")} in {mainAccount.currency}.");
                    Console.WriteLine($"Percent change last 30 mins: {currentPercentageDiff.ToString("p2")}");
                    Console.WriteLine($"Dollar diff last 30 minutes: {(currentCurrencyPrice * currentPercentageDiff).ToString("C")}");
                    Console.WriteLine();

                    foreach (var otherCurrency in otherPercentageDiffs.Keys)
                    {
                        Console.WriteLine($"For {otherCurrency}:");
                        Console.WriteLine($"Percent change last 30 mins: {otherPercentageDiffs[otherCurrency].ToString("p2")}");
                        Console.WriteLine($"Dollar diff last 30 minutes: {(CurrencyPrices.GetCurrentBuy(otherCurrency) * otherPercentageDiffs[otherCurrency]).ToString("C")}");
                        Console.WriteLine();
                    }
                    Console.WriteLine("---------------------------");
                    Console.WriteLine("---------------------------");
                }

                await Task.Delay((int) TimeSpan.FromMinutes(1).TotalMilliseconds);
            }
        }
    }
}