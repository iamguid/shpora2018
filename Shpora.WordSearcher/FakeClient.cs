using System;
using System.Collections.Generic;

namespace WordSearcher
{
    class FakeClient : AbstractClient
    {
        public int x;
        public int y;
        public bool?[][] table;

        public FakeClient(bool?[][] table, int x = 0, int y = 0) : base()
        {
            this.x = x;
            this.y = y;
            this.table = table;
            this.window = Utils.createTable(AbstractClient.windowWidth, AbstractClient.windowHeight, false);
            this.updateWindow();
        }

        public override void moveWindow(MoveDirection dir)
        {
            switch (dir)
            {
                case MoveDirection.Left: this.x--; break;
                case MoveDirection.Up: this.y--; break;
                case MoveDirection.Right: this.x++; break;
                case MoveDirection.Down: this.y++; break;
            }

            this.updateWindow();
        }

        private void updateWindow()
        {
            for (var i = 0; i < AbstractClient.windowHeight; i++)
            {
                for (var j = 0; j < AbstractClient.windowWidth; j++)
                {
                    this.window[i][j] = this.table[this.y + i][this.x + j];
                }
            }
        }
    }
}
