using System;
using System.Collections.Generic;
using System.IO;

namespace WordSearcher
{
    public static class Test
    {
        public static void runMultipleWordsTestCase1()
        {
            bool?[][] table = Utils.createTable(100, 50, false);
            Utils.writeWord(ref table, "мотор", 38, 5);
            Utils.writeWord(ref table, "рыба", 8, 15);
            Utils.writeWord(ref table, "чай", 10, 24);
            var client = new FakeClient(table, 8, 0);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        // Поподаем сверху на середину слова
        public static void runTestCase1()
        {
            bool?[][] table = Utils.createTable(70, 20, false);
            Utils.writeWord(ref table, "аллоха", 8, 5);
            var client = new FakeClient(table, 8, 0);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        // Поподаем на верхний левый угол слова
        public static void runTestCase2()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "мир", 12, 10);
            var client = new FakeClient(table, 0, 4);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        // Поподаем на верхний правый угол слова заканчивающегося на букву Р
        public static void runTestCase3()
        {
            bool?[][] table = Utils.createTable(70, 20, false);
            Utils.writeWord(ref table, "контур", 8, 8);
            var client = new FakeClient(table, 53, 3);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        // Поподаем на верхний правый угол слова заканчивающегося на букву С
        public static void runTestCase4()
        {
            bool?[][] table = Utils.createTable(70, 20, false);
            Utils.writeWord(ref table, "кокос", 8, 8);
            var client = new FakeClient(table, 45, 3);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        // Поподаем на верхний правый угол слова заканчивающегося на букву Ь
        public static void runTestCase5()
        {
            bool?[][] table = Utils.createTable(70, 20, false);
            Utils.writeWord(ref table, "кокоь", 8, 8);
            var client = new FakeClient(table, 42, 3);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        // Поподаем на нижний левый угол слова начинающегося на букву Т
        public static void runTestCase6()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "торус", 11, 3);
            var client = new FakeClient(table, 2, 8);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        // Сразу поподаем на середину слова
        public static void runTestCase7()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "колеса", 8, 6);
            var client = new FakeClient(table, 30, 7);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        // ЫЫЫ
        public static void runTestCase8()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "ыыы", 12, 6);
            var client = new FakeClient(table, 17, 1);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        // ООО
        public static void runTestCase9()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "ооо", 12, 6);
            var client = new FakeClient(table, 26, 1);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        public static void runTestCase10()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "стя", 12, 6);
            var client = new FakeClient(table, 1, 7);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        public static void runTestCase11()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "тяя", 12, 6);
            var client = new FakeClient(table, 2, 1);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        public static void runTestCase12()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "ддяя", 6, 6);
            var client = new FakeClient(table, 17, 1);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        public static void runTestCase13()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "ярц", 12, 6);
            var client = new FakeClient(table, 26, 1);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        public static void runTestCase14()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "яяя", 12, 6);
            var client = new FakeClient(table, 1, 7);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        public static void runTestCase15()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "ядд", 12, 6);
            var client = new FakeClient(table, 26, 1);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        public static void runTestCase16()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "яия", 12, 6);
            var client = new FakeClient(table, 26, 1);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        // [2018-09-27 02:21:30] So long: test='Left offset: 20' correct=ягц answer = steps = 100
        public static void runTestCase17()
        {
            bool?[][] table = Utils.createTable(60, 20, false);
            Utils.writeWord(ref table, "ягц", 12, 6);
            var client = new FakeClient(table, 20, 0);
            var strategy = new Strategy(client);
            loopTest(client, strategy);
        }

        public static void runCombinatoricTest()
        {
            string logFilePath = Path.Combine("./", String.Format("{0:yyyy-MM-dd-hhmmss}_{1}.txt", DateTime.Now, "combinatoric_test"));
            var charCombinations = new List<string>();
            var alphabet = new List<char>(Alphabet.chars.Keys);
            var counters = new int[3];

            while (true)
            {
                var word = "";
                for (var j = 0; j < 3; j++)
                {
                    word += alphabet[counters[j]];
                }
                charCombinations.Add(word);

                if (counters[2]++ >= alphabet.Count - 1)
                {
                    counters[2] = 0;
                    counters[1]++;
                }

                if (counters[1] >= alphabet.Count - 1)
                {
                    counters[1] = 0;
                    counters[0]++;
                }

                if (counters[0] >= alphabet.Count - 1)
                {
                    break;
                }
            }

            for (var i = 0; i < charCombinations.Count; i++)
            {
                var combination = charCombinations[i];
                bool?[][] table = Utils.createTable(60, 20, false);
                Utils.writeWord(ref table, combination, 12, 6);

                var logStr = Utils.getFormattedLogLine("", new Dictionary<string, string> {
                    { "word", combination },
                    { "total", charCombinations.Count.ToString() },
                    { "current", (i + 1).ToString() },
                });

                Console.WriteLine(logStr);

                using (StreamWriter stream = new StreamWriter(logFilePath, false))
                {
                    for (var leftOffset = 0; leftOffset <= 60 - AbstractClient.windowWidth; leftOffset++)
                    {
                        var client = new FakeClient(table, leftOffset, 0);
                        var strategy = new Strategy(client);
                        loopCombinatoricTest(stream, $"Left offset: {leftOffset}", combination, strategy);
                    }
                }
            }
        }

        private static void loopCombinatoricTest(StreamWriter errorsOut, string testName, string correctWord, Strategy strategy)
        {
            try
            {
                while (strategy.state != StrategyState.WalkOutBorders)
                {
                    strategy.nextStep();

                    if (strategy.step >= 100)
                    {
                        var str = Utils.getFormattedLogLine("So long", new Dictionary<string, string> {
                            { "test", $"\'{testName}\'" },
                            { "correct", correctWord },
                            { "answer", String.Join(",", strategy.foundWords) },
                            { "steps", strategy.step.ToString() },
                        });

                        Console.WriteLine(str);
                        errorsOut.WriteLine(str);
                        errorsOut.Flush();
                        return;
                    }
                }
            }
            catch
            {
                if (strategy.wordRect.width > 0)
                {
                    var str = Utils.getFormattedLogLine("Error", new Dictionary<string, string> {
                        { "test", $"\'{testName}\'" },
                        { "correct", correctWord },
                        { "answer", String.Join(",", strategy.foundWords) },
                        { "steps", strategy.step.ToString() },
                    });

                    Console.WriteLine(str);
                    errorsOut.WriteLine(str);
                    errorsOut.Flush();
                }
            }

            if (strategy.wordRect.width > 0 && !strategy.foundWords.Contains(correctWord))
            {
                var str = Utils.getFormattedLogLine("Wrong answer", new Dictionary<string, string> {
                    { "test", $"\'{testName}\'" },
                    { "correct", correctWord },
                    { "answer", String.Join(",", strategy.foundWords) },
                    { "steps", strategy.step.ToString() },
                });

                Console.WriteLine(str);
                errorsOut.WriteLine(str);
                errorsOut.Flush();
            }
        }

        private static void loopTest(FakeClient client, Strategy strategy)
        {
            Utils.printTable(client.table);

            while (strategy.state != StrategyState.WalkOutBorders)
            {
                strategy.nextStep();
                Utils.printDebugInfo(client, strategy);
            }

            Console.WriteLine();
            var foundWordsStr = String.Join(",", strategy.foundWords);
            Console.WriteLine($"Result: words={foundWordsStr} steps={strategy.step}");
            Console.ReadKey();
        }
    }
}
