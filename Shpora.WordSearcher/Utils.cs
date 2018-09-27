using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WordSearcher
{
    public static class Utils
    {
        public struct Rect
        {
            public int x;
            public int y;
            public int width;
            public int height;

            public override string ToString()
            {
                return $"X = {x}, Y = {y}, W = {width}, H = {height}";
            }
        }

        public static bool?[][] createTable(int w, int h, bool? fillByValue = null)
        {
            bool?[][] result = new bool?[h][];

            for (var i = 0; i < h; i++)
            {
                result[i] = new bool?[w];

                for (var j = 0; j < w; j++)
                {
                    result[i][j] = fillByValue;
                }
            }

            return result;
        }
        
        public static bool?[][] createTableFromLines(string[] lines)
        {
            var h = lines.Length;
            var w = lines[0].Length;

            bool?[][] result = new bool?[h][];

            for (var i = 0; i < h; i++)
            {
                result[i] = new bool?[w];

                for (var j = 0; j < w; j++)
                {
                    result[i][j] = (lines[i][j] == '1' ? true : false);
                }
            }

            return result;
        }

        public static void fillTableRect(ref bool?[][] table, Rect rect, bool? fillByValue)
        {
            for (var i = rect.y; i < rect.y + rect.height; i++)
            {
                for (var j = rect.x; j < rect.x + rect.width; j++)
                {
                    table[i][j] = fillByValue;
                }
            }
        }

        public static void clearTableRect(ref bool?[][] table, Rect rect)
        {
            fillTableRect(ref table, rect, null);
        }

        public static void clearTable(ref bool?[][] table)
        {
            Rect rect = new Rect
            {
                x = 0,
                y = 0,
                width = table[0].Length,
                height = table.Length
            };

            clearTableRect(ref table, rect);
        }

        public static void copyTable(bool?[][] from, ref bool?[][] to, int x = 0, int y = 0)
        {
            var h = from.Length;
            var w = from[0].Length;

            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    to[y + i][x + j] = from[i][j];
                }
            }
        }

        public static void printTableRect(bool?[][] table, Rect rect)
        {
            StringBuilder str = new StringBuilder();

            for (var i = rect.y; i < rect.y + rect.height; i++)
            {
                for (var j = rect.x; j < rect.x + rect.width; j++)
                {
                    var ch = "?";
                    if (table[i][j] == true) ch = "1";
                    if (table[i][j] == false) ch = "0";

                    str.Append(ch);
                }
                str.AppendLine();
            }

            Console.Write(str.ToString());
        }

        public static void printTable(bool?[][] table)
        {
            Rect rect = new Rect
            {
                x = 0,
                y = 0,
                width = table[0].Length,
                height = table.Length
            };

            printTableRect(table, rect);
        }

        public static int countTableRectValues(bool?[][] table, Rect rect, bool? countByValue)
        {
            var hits = 0;

            for (var i = rect.y; i < rect.y + rect.height; i++)
            {
                for (var j = rect.x; j < rect.x + rect.width; j++)
                {
                    if (table[i][j] == countByValue) hits++;
                }
            }

            return hits;
        }

        public static int countTableValues(bool?[][] table, bool? countByValue)
        {
            Rect rect = new Rect
            {
                x = 0,
                y = 0,
                width = table[0].Length,
                height = table.Length
            };

            return countTableRectValues(table, rect, countByValue);
        }

        public static int countNullsOnRectTop(bool?[][] table, Rect rect, int top)
        {
            return countTableRectValues(table, new Utils.Rect
            {
                x = rect.x,
                y = rect.y,
                width = rect.width,
                height = top
            }, null);
        }

        public static int countNullsOnRectBottom(bool?[][] table, Rect rect, int bottom)
        {
            return countTableRectValues(table, new Utils.Rect
            {
                x = rect.x,
                y = rect.y + rect.height - bottom,
                width = rect.width,
                height = bottom
            }, null);
        }

        public static void writeWord(ref bool?[][] table, string word, int x, int y, int space = 1)
        {
            for (var i = 0; i < word.Length; i++)
            {
                writeChar(ref table, word[i], x + (Alphabet.charSize + space) * i, y);
            }
        }

        public static void writeChar(ref bool?[][] table, char ch, int x, int y)
        {
            Alphabet.Char alphabetChar;
            Alphabet.chars.TryGetValue(ch, out alphabetChar);

            for (var i = 0; i < Alphabet.charSize; i++)
            {
                for (var j = 0; j < Alphabet.charSize; j++)
                {
                    table[y + i][x + j] = alphabetChar.asBool[i][j];
                }
            }
        }

        public static string getFormattedLogLine(string title, Dictionary<string, string> values)
        {
            var currentDate = String.Format("[{0:yyyy-MM-dd hh:mm:ss}]", DateTime.Now);
            var str = $"{currentDate} {title}: ";

            foreach (var value in values)
            {
                str += $"{value.Key}={value.Value} ";
            }

            return str;
        }

        public static void printDebugInfo(AbstractClient client, Strategy strategy)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Main: Step={strategy.step}, Word=\'{strategy.currentWord}\', State={strategy.state}");
            Console.WriteLine($"Window: X={strategy.windowX}, Y={strategy.windowY}");
            Utils.printTable(client.window);

            var hasSomethingInBuffer = strategy.contentRect.width > 0 || strategy.contentRect.height > 0;
            if (strategy.trackMovements && hasSomethingInBuffer)
            {
                Console.WriteLine($"Buffer rect: {strategy.contentRect}");
                Utils.printTableRect(strategy.buffer, strategy.contentRect);

                Console.WriteLine($"Word rect: {strategy.wordRect}");
                Utils.printTableRect(strategy.buffer, strategy.wordRect);

                Console.WriteLine($"Char rect: {strategy.charRect}");
                Utils.printTableRect(strategy.buffer, strategy.charRect);
            }
        }
    }
}
