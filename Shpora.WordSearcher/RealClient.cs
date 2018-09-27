using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordSearcher
{
    public class RealClient : AbstractClient
    {
        public Api api;

        public RealClient(Api api, bool?[][] initialWindowValues)
        {
            this.api = api;
            this.window = Utils.createTable(AbstractClient.windowWidth, AbstractClient.windowHeight);
            Utils.copyTable(initialWindowValues, ref this.window);
        }

        public override void moveWindow(MoveDirection dir)
        {
            switch (dir)
            {
                case MoveDirection.Left:
                    this.api.moveLeft(out this.window);
                    break;
                case MoveDirection.Up:
                    this.api.moveUp(out this.window);
                    break;
                case MoveDirection.Right:
                    this.api.moveRight(out this.window);
                    break;
                case MoveDirection.Down:
                    this.api.moveDown(out this.window);
                    break;
            }
        }
    }
}
