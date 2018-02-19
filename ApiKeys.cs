using System;
using System.IO;

namespace warbucks
{
    public class ApiKeys
    {
        private ApiKeys()
        {
            var apiInfo = File.ReadAllText("ApiInfo.txt").Replace("\r", string.Empty).Split('\n');

            ApiKey = apiInfo[0];
            ApiSecret = apiInfo[1];
            ApiPassphrase = apiInfo[2];
        }
        
        private static ApiKeys _instance;

        public static ApiKeys Instance = _instance ?? (_instance = new ApiKeys());

        public string BaseUrl = "https://api.gdax.com";

        public string ApiKey;

        public string ApiSecret;
        
        public string ApiPassphrase;

    }
}