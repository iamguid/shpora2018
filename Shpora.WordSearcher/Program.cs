using System;
using System.Linq;

namespace WordSearcher
{
    class Program
    {
        private static Uri defaultApiHost = new Uri("http://shpora.skbkontur.ru");
        private static string defaultApiToken = "75590799-d0ea-4929-b55d-63d8a9945315";

        static void Main(string[] args)
        {
            var apiHost = defaultApiHost;
            var apiToken = defaultApiToken;

            if (args.Length >= 2)
            {
                apiHost = new Uri(args[0]);
                apiToken = args[1];
            }

            var apiClient = new Api(apiHost, apiToken);

            bool?[][] initialRect = Utils.createTable(AbstractClient.windowWidth, AbstractClient.windowHeight);
            var authorizationSucceeded = false;

            while (!authorizationSucceeded)
            {
                var startResp = apiClient.start(out initialRect);

                if (startResp.StatusCode == System.Net.HttpStatusCode.Conflict || startResp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    apiClient.finish();
                }

                if (startResp.IsSuccessStatusCode)
                {
                    authorizationSucceeded = true;
                    Console.WriteLine("Authorization succeeded, lets rock");
                    break;
                }
                else
                {
                    Console.WriteLine("Error {0} {1}", startResp.StatusCode, startResp.ReasonPhrase);
                }

                Console.WriteLine("Reconnecting...");
                System.Threading.Thread.Sleep(1000);
            }

            var client = new RealClient(apiClient, initialRect);
            var strategy = new Strategy(client);

            while (strategy.state != StrategyState.Finish)
            {
                strategy.nextStep();

                if (strategy.state != StrategyState.FindSomething)
                {
                    Utils.printDebugInfo(client, strategy);
                }
            }

            var words = Enumerable.ToList(strategy.foundWords);
            words.Sort((string a, string b) => { return a.Length - b.Length; });

            foreach (var word in words)
            {
                string resultWords;
                apiClient.words(new string[1] { word }, out resultWords);
                Console.WriteLine($"Added word: {word} {resultWords}");

                string resultStats;
                apiClient.stats(out resultStats);
                Console.WriteLine($"Stats: {resultStats}");
            }

            apiClient.finish();

            Console.ReadKey();
            //Test.runCombinatoricTest();
        }
    }
}
