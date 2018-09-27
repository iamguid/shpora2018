using System.Collections.Generic;

namespace WordSearcher
{
    public abstract class AbstractClient
    {
        public enum MoveDirection
        {
            Left,
            Up,
            Right,
            Down
        }

        public static int windowWidth = 11;
        public static int windowHeight = 5;

        public bool?[][] window;

        public abstract void moveWindow(MoveDirection dir);

        public static int getScores(int l, int f)
        {
            return 20 * l * f;
        }
    }
}
