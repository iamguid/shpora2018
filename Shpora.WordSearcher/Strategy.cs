using System;
using System.Collections.Generic;
using static WordSearcher.AbstractClient;

namespace WordSearcher
{
    public enum StrategyState
    {
        FindSomething,
        DetectInitialBorders,
        ScanWordFromLeftToRight,
        ScanWordFromRightToLeft,
        WalkOutBorders,
        Finish, // Special state that indicates finish
    }

    public enum StrategyEvent
    {
        SomethingFinded,
        LeftBorderDetected,
        RightBorderDetected,
        SomethingWentWrong,
        WordFound,
        ReadyToNextWord,
        EwerythingIsFound,
    }

    public enum StrategyAction
    {
        MoveLeft,
        MoveUp,
        MoveRight,
        MoveDown,
        DoNothing,
    }

    public class StateTransition
    {
        public StrategyState from;
        public StrategyState to;
        public StrategyEvent when;
    }

    public class Strategy
    {
        public static StateTransition[] transitions = {
            new StateTransition { from = StrategyState.FindSomething, when = StrategyEvent.SomethingFinded, to = StrategyState.DetectInitialBorders },
            new StateTransition { from = StrategyState.DetectInitialBorders, when = StrategyEvent.RightBorderDetected, to = StrategyState.ScanWordFromRightToLeft },
            new StateTransition { from = StrategyState.DetectInitialBorders, when = StrategyEvent.LeftBorderDetected, to = StrategyState.ScanWordFromLeftToRight },
            new StateTransition { from = StrategyState.DetectInitialBorders, when = StrategyEvent.SomethingWentWrong, to = StrategyState.WalkOutBorders },
            new StateTransition { from = StrategyState.ScanWordFromLeftToRight, when = StrategyEvent.WordFound, to = StrategyState.WalkOutBorders },
            new StateTransition { from = StrategyState.ScanWordFromRightToLeft, when = StrategyEvent.WordFound, to = StrategyState.WalkOutBorders },
            new StateTransition { from = StrategyState.ScanWordFromLeftToRight, when = StrategyEvent.SomethingWentWrong, to = StrategyState.WalkOutBorders },
            new StateTransition { from = StrategyState.ScanWordFromRightToLeft, when = StrategyEvent.SomethingWentWrong, to = StrategyState.WalkOutBorders },
            new StateTransition { from = StrategyState.WalkOutBorders, when = StrategyEvent.ReadyToNextWord, to = StrategyState.FindSomething },
        };

        public static int bufferWidth = Alphabet.charSize * 200;
        public static int bufferHeight = Alphabet.charSize * 4;
        public static int wordLeftRightBorderWidth = 3;
        public static int wordTopBottomBorderWidth = 1;

        public StrategyState state;
        public StrategyEvent? transitionEvent;
        public StrategyAction? action;

        public AbstractClient client;
        public bool trackMovements;
        public bool moveVertical; // Move vertical or horizontal in some cases
        public int step;
        public int windowX;
        public int windowY;
        public bool?[][] buffer;
        public bool?[] xCollapsedBuff;
        public bool?[] yCollapsedBuff;
        public Utils.Rect contentRect;
        public Utils.Rect wordRect;
        public Utils.Rect charRect;
        public string currentWord;
        public HashSet<string> foundWords;

        // Flags that indicates is word borders detected or not
        public bool leftWordBorderDetected;
        public bool rightWordBorderDetected;
        public bool topWordBorderDetected;
        public bool bottomWordBorderDetected;

        public Strategy(AbstractClient client)
        {
            this.state = StrategyState.FindSomething;
            this.transitionEvent = null;
            this.action = null;
            this.trackMovements = false;
            this.moveVertical = false;
            this.step = 0;
            this.client = client;
            this.buffer = Utils.createTable(bufferWidth, bufferHeight, null);
            this.xCollapsedBuff = new bool?[bufferWidth];
            this.yCollapsedBuff = new bool?[bufferHeight];
            this.contentRect = new Utils.Rect();
            this.wordRect = new Utils.Rect();
            this.charRect = new Utils.Rect();
            this.currentWord = "";
            this.foundWords = new HashSet<string>();
            this.leftWordBorderDetected = false;
            this.rightWordBorderDetected = false;
            this.topWordBorderDetected = false;
            this.bottomWordBorderDetected = false;
        }

        public void nextStep()
        {
            // Exit if we already finished
            if (this.state == StrategyState.Finish) return;

            // Reset action
            this.action = null;

            // Loop until we doesn't have action
            while (!this.action.HasValue)
            {
                switch (this.nextState())
                {
                    case StrategyState.FindSomething: this.findSomething(); break;
                    case StrategyState.DetectInitialBorders: this.detectInitialBorders(); break;
                    case StrategyState.ScanWordFromLeftToRight: this.scanWordFromLeftToRight(); break;
                    case StrategyState.ScanWordFromRightToLeft: this.scanWordFromRightToLfeft(); break;
                    case StrategyState.WalkOutBorders: this.walkOut(); break;
                    case StrategyState.Finish: this.action = StrategyAction.DoNothing; break;
                }
            }

            // Do action
            switch (action)
            {
                case StrategyAction.MoveLeft:
                    this.client.moveWindow(MoveDirection.Left);
                    this.windowX--;
                    break;
                case StrategyAction.MoveUp:
                    this.client.moveWindow(MoveDirection.Up);
                    this.windowY--;
                    break;
                case StrategyAction.MoveRight:
                    this.client.moveWindow(MoveDirection.Right);
                    this.windowX++;
                    break;
                case StrategyAction.MoveDown:
                    this.client.moveWindow(MoveDirection.Down);
                    this.windowY++;
                    break;
                case StrategyAction.DoNothing:
                    break;
            }

            // Update buffer and some variables if needed
            if (this.trackMovements)
            {
                this.copyWindowToBuffer();
                this.updateBufferCollapsed();
                this.updateContentRect();
                this.updateWordRect();
                this.updateCharRect();
            }

            // Increase steps count and set moveVertical flag
            this.moveVertical = this.step++ % 2 == 0;
        }

        private StrategyState nextState()
        {
            if (this.transitionEvent.HasValue)
            {
                foreach (var transition in transitions)
                {
                    if ((transition.from == this.state) && (transition.when == this.transitionEvent))
                    {
                        this.state = transition.to;
                        this.action = null;
                        this.handleStateTransition(transition);
                        return this.state;
                    }
                }
            }

            return this.state;
        }

        private void handleStateTransition(StateTransition transition)
        {
            // Handle found word
            if (transition.when == StrategyEvent.WordFound)
            {
                var isRealyWordFound = this.leftWordBorderDetected && this.rightWordBorderDetected && this.currentWord.Length > 1;
                var isWordAlreadyFound = this.foundWords.Contains(this.currentWord);

                // We found all words, finish
                if (isWordAlreadyFound)
                {
                    // Redefine current state by force
                    this.state = StrategyState.Finish;
                    return;
                }

                // Add new word
                if (isRealyWordFound)
                {
                    this.foundWords.Add(this.currentWord);
                }

                // Reset current word
                this.currentWord = String.Empty;
            }

            // Enable movements track and update variables when something detected
            if (transition.to == StrategyState.DetectInitialBorders)
            {
                Utils.clearTable(ref this.buffer);
                this.windowX = bufferWidth / 2;
                this.windowY = bufferHeight / 2;
                this.copyWindowToBuffer();
                this.updateBufferCollapsed();
                this.updateContentRect();
                this.updateWordRect();
                this.updateCharRect();
                this.leftWordBorderDetected = false;
                this.rightWordBorderDetected = false;
                this.topWordBorderDetected = false;
                this.bottomWordBorderDetected = false;
                this.trackMovements = true;
            }

            // Disable movements track when we walk out, we enable it later when we detected something
            if (transition.to == StrategyState.WalkOutBorders)
            {
                this.trackMovements = false;
            }
        }

        private void findSomething()
        {
            if (this.hasSomethingInWindow())
            {
                this.transitionEvent = StrategyEvent.SomethingFinded;
            }
            else
            {
                this.action = this.moveVertical ? StrategyAction.MoveDown : StrategyAction.MoveRight;
            }
        }

        private void detectInitialBorders()
        {
            // Move closer to word
            if (this.wordRect.width < Alphabet.charSize && this.wordRect.height < AbstractClient.windowHeight)
            {
                // Indents between window borders and cells filled by true
                int windowTopIndents;
                int windowLeftIndents;
                int windowBottomIndents;
                int windowRightIndents;
                this.getWindowIndents(out windowTopIndents, out windowLeftIndents, out windowBottomIndents, out windowRightIndents);

                if (this.moveVertical)
                {
                    if (windowTopIndents > windowBottomIndents)
                    {
                        this.action = StrategyAction.MoveDown;
                        return;
                    }

                    this.action = StrategyAction.MoveUp;
                    return;
                }
                else
                {
                    if (windowRightIndents > windowLeftIndents)
                    {
                        this.action = StrategyAction.MoveLeft;
                        return;
                    }

                    this.action = StrategyAction.MoveRight;
                    return;
                }
            }

            // If current word rectangle height equals character size then we found top and bottom word borders
            if (this.wordRect.height == Alphabet.charSize)
            {
                this.topWordBorderDetected = this.bottomWordBorderDetected = true;
            }

            // If we found left initial borders
            if (this.leftWordBorderDetected && (this.topWordBorderDetected || this.bottomWordBorderDetected))
            {
                this.transitionEvent = StrategyEvent.LeftBorderDetected;
                return;
            }

            // If we found right initial borders
            if (this.rightWordBorderDetected && (this.topWordBorderDetected || this.bottomWordBorderDetected))
            {
                this.transitionEvent = StrategyEvent.RightBorderDetected;
                return;
            }

            // Top and bottom word border signatures
            int topBorderIndents;
            int bottomBorderIndents;
            bool topBorderSignature;
            bool bottomBorderSignature;
            this.getTopBorderSignature(out topBorderIndents, out topBorderSignature);
            this.getBottomBorderSignature(out bottomBorderIndents, out bottomBorderSignature);

            // Left and right word border signatures
            int leftBorderIndents;
            int rightBorderIndents;
            bool leftBorderSignature;
            bool rightBorderSignature;
            this.getLeftBorderSignature(out leftBorderIndents, out leftBorderSignature);
            this.getRightBorderSignature(out rightBorderIndents, out rightBorderSignature);

            // Detect top and bottom word borders
            if (!this.topWordBorderDetected && !this.bottomWordBorderDetected)
            {
                // See testCase5
                if (topBorderSignature && bottomBorderSignature)
                {
                    var leftBorderHasTrues = Utils.countTableRectValues(this.buffer, new Utils.Rect {
                        x = this.wordRect.x,
                        y = this.wordRect.y,
                        width = 1,
                        height = this.wordRect.height,
                    }, true) > 0;

                    var leftBorderHasNulls = Utils.countTableRectValues(this.buffer, new Utils.Rect {
                        x = this.wordRect.x - 1,
                        y = this.wordRect.y,
                        width = 1,
                        height = this.wordRect.height,
                    }, null) > 0;

                    var topBorderHasTrues = Utils.countTableRectValues(this.buffer, new Utils.Rect {
                        x = this.wordRect.x,
                        y = this.wordRect.y,
                        width = this.wordRect.width,
                        height = 1,
                    }, true) > 0;

                    var topBorderHasNulls = Utils.countTableRectValues(this.buffer, new Utils.Rect {
                        x = this.wordRect.x,
                        y = this.wordRect.y - 1,
                        width = this.wordRect.width,
                        height = 1,
                    }, null) > 0;

                    var rightBorderHasTrues = Utils.countTableRectValues(this.buffer, new Utils.Rect {
                        x = this.wordRect.x + this.wordRect.width - 1,
                        y = this.wordRect.y,
                        width = 1,
                        height = this.wordRect.height,
                    }, true) > 0;

                    var rightBorderHasNulls = Utils.countTableRectValues(this.buffer, new Utils.Rect {
                        x = this.wordRect.x + this.wordRect.width,
                        y = this.wordRect.y,
                        width = 1,
                        height = this.wordRect.height,
                    }, null) > 0;

                    var bottomBorderHasTrues = Utils.countTableRectValues(this.buffer, new Utils.Rect {
                        x = this.wordRect.x,
                        y = this.wordRect.y + this.wordRect.height - 1,
                        width = this.wordRect.width,
                        height = 1,
                    }, true) > 0;

                    var bottomBorderHasNulls = Utils.countTableRectValues(this.buffer, new Utils.Rect {
                        x = this.wordRect.x,
                        y = this.wordRect.y + this.wordRect.height,
                        width = this.wordRect.width,
                        height = 1,
                    }, null) > 0;

                    var moveToLeft = leftBorderHasNulls && leftBorderHasTrues && this.windowX > this.wordRect.x - 1;
                    var moveToTop = topBorderHasNulls && topBorderHasTrues && this.windowY > this.wordRect.x - 1;
                    var moveToRight = rightBorderHasNulls && rightBorderHasTrues && this.windowX + AbstractClient.windowWidth < this.wordRect.x + this.wordRect.width + 1;
                    var moveToBottom = bottomBorderHasNulls && bottomBorderHasTrues && this.windowY + AbstractClient.windowWidth < this.wordRect.y + this.wordRect.height + 1;

                    if (moveToRight)
                    {
                        this.action = StrategyAction.MoveRight;
                        return;
                    }

                    if (moveToTop)
                    {
                        this.action = StrategyAction.MoveUp;
                        return;
                    }

                    if (moveToLeft)
                    {
                        this.action = StrategyAction.MoveLeft;
                        return;
                    }

                    if (moveToBottom)
                    {
                        this.action = StrategyAction.MoveDown;
                        return;
                    }
                }

                if (topBorderIndents > bottomBorderIndents)
                {
                    this.action = StrategyAction.MoveDown;
                    return;
                }

                this.action = StrategyAction.MoveUp;
                return;
            }

            // Detect left or right border
            if (!this.leftWordBorderDetected && !this.rightWordBorderDetected)
            {
                if (leftBorderSignature)
                {
                    this.checkLeftBorder();
                    return;
                }

                if (rightBorderSignature)
                {
                    this.checkRightBorder();
                    return;
                }

                if (leftBorderIndents > rightBorderIndents)
                {
                    this.action = StrategyAction.MoveLeft;
                    return;
                }

                this.action = StrategyAction.MoveRight;
                return;
            }
        }

        private void scanWordFromLeftToRight()
        {
            // Right border detected
            if (this.rightWordBorderDetected)
            {
                this.transitionEvent = StrategyEvent.WordFound;
                return;
            }

            int rightBorderIndents;
            bool rightBorderSignature;
            this.getRightBorderSignature(out rightBorderIndents, out rightBorderSignature);

            // Trying detect right border if its look like border
            if (rightBorderSignature)
            {
                this.checkRightBorder();
                return;
            }

            this.updateCharRect();
            var similaryChars = this.getSimilaryChars();
            var isWindowOutsideChar = ((this.windowX + AbstractClient.windowWidth) - (this.charRect.x + this.charRect.width)) < 0;

            if (similaryChars.Count == 0)
            {
                if (isWindowOutsideChar)
                {
                    this.action = StrategyAction.MoveRight;
                    return;
                }
                else
                {
                    this.transitionEvent = StrategyEvent.SomethingWentWrong;
                    return;
                }
            }

            // Add character to word
            if (similaryChars.Count == 1)
            {
                this.currentWord += similaryChars[0];
                return;
            }

            // Clarify character
            if (similaryChars.Count > 1)
            {
                // Move window to right character border
                if (isWindowOutsideChar)
                {
                    this.action = StrategyAction.MoveRight;
                    return;
                }

                // Then explore unknown part of character
                var charNullsOnTop = Utils.countNullsOnRectTop(this.buffer, this.charRect, 2);
                var charNullsOnBottom = Utils.countNullsOnRectBottom(this.buffer, this.charRect, 2);

                if (charNullsOnTop > charNullsOnBottom)
                {
                    this.action = StrategyAction.MoveUp;
                    return;
                }
                else
                {
                    this.action = StrategyAction.MoveDown;
                    return;
                }
            }
        }

        private void scanWordFromRightToLfeft()
        {
            // Left border detected, word found
            if (this.leftWordBorderDetected)
            {
                this.transitionEvent = StrategyEvent.WordFound;
                return;
            }

            int leftBorderIndents;
            bool leftBorderSignature;
            this.getLeftBorderSignature(out leftBorderIndents, out leftBorderSignature);

            // Trying detect left border if its look like border
            if (leftBorderSignature)
            {
                this.checkLeftBorder();
                return;
            }

            this.updateCharRect();
            var similaryChars = this.getSimilaryChars();
            var isWindowOutsideChar = (this.charRect.x - this.windowX) < 0;

            if (similaryChars.Count == 0) {
                if (isWindowOutsideChar)
                {
                    this.action = StrategyAction.MoveLeft;
                    return;
                }
                else
                {
                    this.transitionEvent = StrategyEvent.SomethingWentWrong;
                    return;
                }
            }

            // Add character
            if (similaryChars.Count == 1)
            {
                this.currentWord = similaryChars[0] + this.currentWord;
                return;
            }

            // Clarify character
            if (similaryChars.Count > 1)
            {
                // Move window to left character border
                if (isWindowOutsideChar)
                {
                    this.action = StrategyAction.MoveLeft;
                    return;
                }

                // Then explore unknown part of character
                var charNullsOnTop = Utils.countNullsOnRectTop(this.buffer, this.charRect, 2);
                var charNullsOnBottom = Utils.countNullsOnRectBottom(this.buffer, this.charRect, 2);

                if (charNullsOnTop > charNullsOnBottom)
                {
                    this.action = StrategyAction.MoveUp;
                    return;
                }
                else
                {
                    this.action = StrategyAction.MoveDown;
                    return;
                }
            }
        }

        private void walkOut()
        {
            // Move window outside current word border
            // Our movements will not update last buffer state (see handleStateTransition method)
            if (this.windowY < this.wordRect.y + this.wordRect.height + wordTopBottomBorderWidth)
            {
                this.action = StrategyAction.MoveDown;
                return;
            }

            this.transitionEvent = StrategyEvent.ReadyToNextWord;
        }

        private void checkLeftBorder()
        {
            if (!this.leftWordBorderDetected)
            {
                // Move window to left word border if needed
                if (this.windowX > this.wordRect.x - wordLeftRightBorderWidth)
                {
                    this.action = StrategyAction.MoveLeft;
                    return;
                }

                // Check and explore nulls on left border
                var hasNullsOnTop = Utils.countTableRectValues(this.buffer, new Utils.Rect
                {
                    x = this.wordRect.x - wordLeftRightBorderWidth,
                    y = this.wordRect.y,
                    width = wordLeftRightBorderWidth,
                    height = 2
                }, null) > 0;

                var hasNullsOnBottom = Utils.countTableRectValues(this.buffer, new Utils.Rect
                {
                    x = this.wordRect.x - wordLeftRightBorderWidth,
                    y = this.wordRect.y + this.wordRect.height - 2,
                    width = wordLeftRightBorderWidth,
                    height = 2
                }, null) > 0;


                if (hasNullsOnTop)
                {
                    this.action = StrategyAction.MoveUp;
                    return;
                }

                if (hasNullsOnBottom)
                {
                    this.action = StrategyAction.MoveDown;
                    return;
                }

                // Change left border detected state
                this.leftWordBorderDetected = true;
            }
        }

        private void checkRightBorder()
        {
            if (!rightWordBorderDetected)
            {
                // Move window to right word border if needed
                if (this.windowX + AbstractClient.windowWidth < this.wordRect.x + this.wordRect.width + wordLeftRightBorderWidth)
                {
                    this.action = StrategyAction.MoveRight;
                    return;
                }

                // Check and explore nulls on right border
                var hasNullsOnTop = Utils.countTableRectValues(this.buffer, new Utils.Rect
                {
                    x = this.wordRect.x + this.wordRect.width,
                    y = this.wordRect.y,
                    width = wordLeftRightBorderWidth,
                    height = 2
                }, null) > 0;

                var hasNullsOnBottom = Utils.countTableRectValues(this.buffer, new Utils.Rect
                {
                    x = this.wordRect.x + this.wordRect.width,
                    y = this.wordRect.y + this.wordRect.height - 2,
                    width = wordLeftRightBorderWidth,
                    height = 2
                }, null) > 0;

                if (hasNullsOnTop)
                {
                    this.action = StrategyAction.MoveUp;
                    return;
                }

                if (hasNullsOnBottom)
                {
                    this.action = StrategyAction.MoveDown;
                    return;
                }

                // Change right border detected state
                this.rightWordBorderDetected = true;
            }
        }

        private bool hasSomethingInWindow()
        {
            for (var i = 0; i < AbstractClient.windowHeight; i++)
            {
                for (var j = 0; j < AbstractClient.windowWidth; j++)
                {
                    if (this.client.window[i][j] == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void updateBufferCollapsed()
        {
            this.xCollapsedBuff = new bool?[bufferWidth];
            this.yCollapsedBuff = new bool?[bufferHeight];

            for (var i = 0; i < bufferHeight; i++)
            {
                for (var j = 0; j < bufferWidth; j++)
                {
                    var bufferValue = this.buffer[i][j];
                    if (bufferValue == null) continue;

                    var oldYValue = this.yCollapsedBuff[i];
                    var oldXValue = this.xCollapsedBuff[j];

                    if (bufferValue == true)
                    {
                        if (oldXValue == null || oldXValue == false)
                        {
                            this.xCollapsedBuff[j] = true;
                        }

                        if (oldYValue == null || oldYValue == false)
                        {
                            this.yCollapsedBuff[i] = true;
                        }
                    }
                    else if (bufferValue == false)
                    {
                        if (oldXValue == null)
                        {
                            this.xCollapsedBuff[j] = false;
                        }

                        if (oldYValue == null)
                        {
                            this.yCollapsedBuff[i] = false;
                        }
                    }
                }
            }
        }

        private void updateContentRect()
        {
            this.contentRect.x = 0;
            for (var i = 0; i < bufferWidth; i++)
            {
                if (this.xCollapsedBuff[i] != null)
                {
                    this.contentRect.x = i;
                    break;
                }
            }

            this.contentRect.y = 0;
            for (var i = 0; i < bufferHeight; i++)
            {
                if (this.yCollapsedBuff[i] != null)
                {
                    this.contentRect.y = i;
                    break;
                }
            }

            this.contentRect.width = 0;
            for (var i = bufferWidth - 1; i >= 0; i--)
            {
                if (this.xCollapsedBuff[i] != null)
                {
                    this.contentRect.width = i + 1 - this.contentRect.x;
                    break;
                }
            }

            this.contentRect.height = 0;
            for (var i = bufferHeight - 1; i >= 0; i--)
            {
                if (this.yCollapsedBuff[i] != null)
                {
                    this.contentRect.height = i + 1 - this.contentRect.y;
                    break;
                }
            }
        }

        private void updateWordRect()
        {
            this.wordRect.x = this.contentRect.x;
            for (var i = this.contentRect.x; i < this.contentRect.x + this.contentRect.width; i++)
            {
                if (this.xCollapsedBuff[i] == true)
                {
                    this.wordRect.x = i;
                    break;
                }
            }

            this.wordRect.y = this.contentRect.y;
            for (var i = this.contentRect.y; i < this.wordRect.y + this.contentRect.height; i++)
            {
                if (this.yCollapsedBuff[i] == true)
                {
                    this.wordRect.y = i;
                    break;
                }
            }

            this.wordRect.width = this.contentRect.width;
            for (var i = this.contentRect.x + this.contentRect.width; i >= this.contentRect.x; i--)
            {
                if (this.xCollapsedBuff[i] == true)
                {
                    this.wordRect.width = i + 1 - this.wordRect.x;
                    break;
                }
            }

            this.wordRect.height = this.contentRect.height;
            for (var i = this.contentRect.y + this.contentRect.height; i >= this.contentRect.y; i--)
            {
                if (this.yCollapsedBuff[i] == true)
                {
                    this.wordRect.height = i + 1 - this.wordRect.y;
                    break;
                }
            }
        }

        private void updateCharRect()
        {
            var offset = Alphabet.charSize * this.currentWord.Length + this.currentWord.Length * 1;

            if (this.state == StrategyState.ScanWordFromRightToLeft)
            {
                this.charRect.x = this.wordRect.x + this.wordRect.width - offset - Alphabet.charSize;
                this.charRect.y = this.wordRect.y;
            }
            else
            {
                this.charRect.x = this.wordRect.x + offset;
                this.charRect.y = this.wordRect.y;
            }

            this.charRect.width = Alphabet.charSize;
            this.charRect.height = Alphabet.charSize;

        }

        private void getWindowIndents(out int top, out int left, out int bottom, out int right)
        {
            top = 0;
            left = 0;
            bottom = 0;
            right = 0;

            for (var i = this.windowY; i < this.windowY + AbstractClient.windowHeight; i++)
            {
                if (this.yCollapsedBuff[i] == true) break;
                top++;
            }

            for (var i = this.windowX; i < this.windowX + AbstractClient.windowWidth; i++)
            {
                if (this.xCollapsedBuff[i] == true) break;
                left++;
            }

            for (var i = this.windowY + AbstractClient.windowHeight - 1; i >= this.windowY; i--)
            {
                if (this.yCollapsedBuff[i] == true) break;
                bottom++;
            }

            for (var i = this.windowX + AbstractClient.windowWidth - 1; i >= this.windowX; i--)
            {
                if (this.xCollapsedBuff[i] == true) break;
                right++;
            }
        }

        private void getLeftBorderSignature(out int indents, out bool signature)
        {
            indents = (this.wordRect.x - this.contentRect.x);
            signature = indents >= wordLeftRightBorderWidth;
        }

        private void getRightBorderSignature(out int indents, out bool signature)
        {
            indents = (this.contentRect.x + this.contentRect.width) - (this.wordRect.x + this.wordRect.width);
            signature = indents >= wordLeftRightBorderWidth;
        }

        private void getTopBorderSignature(out int indents, out bool signature)
        {
            var hasHalfCharHeight = this.wordRect.height >= Alphabet.charSize / 2;
            indents = (this.wordRect.y - this.contentRect.y);
            signature = hasHalfCharHeight && indents >= wordTopBottomBorderWidth;
        }

        private void getBottomBorderSignature(out int indents, out bool signature)
        {
            var hasHalfCharHeight = this.wordRect.height >= Alphabet.charSize / 2;
            indents = (this.contentRect.y + this.contentRect.height) - (this.wordRect.y + this.wordRect.height);
            signature = hasHalfCharHeight && indents >= wordTopBottomBorderWidth;
        }

        private void copyWindowToBuffer()
        {
            Utils.copyTable(this.client.window, ref this.buffer, this.windowX, this.windowY);
        }

        private List<char> getSimilaryChars()
        {
            List<char> variants = new List<char>();

            foreach (var refChar in Alphabet.chars)
            {
                if (this.isCharSimilary(refChar.Value))
                {
                    variants.Add(refChar.Key);
                }
            }

            return variants;
        }

        private bool isCharSimilary(Alphabet.Char ch)
        {
            for (var i = 0; i < Alphabet.charSize; i++)
            {
                for (var j = 0; j < Alphabet.charSize; j++)
                {
                    var currentCharValue = this.buffer[this.charRect.y + i][this.charRect.x + j];
                    var refCharValue = ch.asBool[i][j];

                    if (currentCharValue == null) continue;

                    if (currentCharValue != refCharValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
