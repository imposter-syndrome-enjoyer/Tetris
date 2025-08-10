using System;
using System.Text;

while (true) Tetris.Game.Play();

namespace Tetris
{
    public static class Game
    {
        
        private static readonly double _initialGameSpeed;
        private static readonly double _gameSpeedIncreasePercent;
        private static double _gameSpeed;
        private static int _score;        
        private static int _highScore;
        private static int _level;
        private static bool _isGameOver;
        internal static Shape currentShape;
        private static Shape _nextShape;        

        static Game()
        {
            Console.CursorVisible = false;
            _initialGameSpeed = 1000;            
            _gameSpeedIncreasePercent = 0.9;
            PlayField.RowsClearedEvent += RowsClearedEventHandler;
            currentShape = new Shape();
            _nextShape = new Shape();
        }

        public static void Play()
        {
            _isGameOver = false;
            _gameSpeed = _initialGameSpeed;
            _score = 0;
            _level = 0;
            Console.Clear();
            PlayField.EraseField();
            LoadInterface();
            UpdateInterface();
            while (!_isGameOver)
            {
                MainLoop();
            }
        }

        private static void MainLoop()
        {
            int elapsedTime = 0;
            int step = 25;
            bool isAbleToDrop = true;
            ClearInputBuffer();
            while (elapsedTime < _gameSpeed)
            {
                RenderField();
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.DownArrow:
                            isAbleToDrop = currentShape.Move(Direction.Down);
                            break;
                        case ConsoleKey.LeftArrow:
                            currentShape.Move(Direction.Left);
                            break;
                        case ConsoleKey.RightArrow:
                            currentShape.Move(Direction.Right);
                            break;
                        case ConsoleKey.Spacebar:
                            currentShape.Rotate();
                            break;
                        case ConsoleKey.Escape:
                            PauseGame();
                            break;
                    }
                }
                elapsedTime += step;
                Thread.Sleep(step);
                if (!isAbleToDrop)
                {
                    Thread.Sleep(100);
                    break;
                }
            }
            isAbleToDrop = currentShape.Move(Direction.Down);
            if (!isAbleToDrop)
            {
                PlayField.AddToPile(currentShape);
                currentShape = _nextShape;
                _nextShape = new Shape();
                UpdateInterface();
                if (PlayField.IsShapeObstructed(currentShape.Coordinates)) EndGame();
            }
        }
        internal static void RenderField()
        {
            bool[,] pileOnField = PlayField.Field;
            bool[,] shapeOnField = new bool[PlayField.Width, PlayField.Height];

            foreach ((int x, int y) coord in currentShape.Coordinates)
            {
                shapeOnField[coord.x, coord.y] = true;
            }
            int cursorPosX = 1;
            int cursorPosY = 1;
            Console.SetCursorPosition(cursorPosX, cursorPosY);

            for (int y = PlayField.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < PlayField.Width; x++)
                 {
                    if (pileOnField[x, y])
                    {
                        Console.Write("▒▒");
                        continue;
                    }
                    if (shapeOnField[x, y])
                    {
                        Console.Write("██");
                        continue;
                    }
                    else Console.Write("  ");  // special space (alt+255) that won't be aut-deleted by console
                }
                Console.SetCursorPosition(cursorPosX, ++cursorPosY);
            }
            Console.SetCursorPosition(1, 1);
        }

        private static void ClearInputBuffer()
        {
            while (Console.KeyAvailable)    // clearing keyboard buffer so next shape doesn't drop rapidly if arrow button is held down
            {
                Console.ReadKey(true);
            }
        }

        private static void LoadInterface()
        {
            string filePath = "ScreenTemplate.txt";
            string screenTemplate;
            try
            {
                using StreamReader streamReader = new(filePath);
                screenTemplate = streamReader.ReadToEnd();
            }
            catch (FileNotFoundException)
            {
                screenTemplate = CreateScreen();
                File.AppendAllText(filePath, screenTemplate);
            }
            StringBuilder highScoreString = new();
            string[] lines = screenTemplate.Split('\n');
            char[] symbols = lines[13].ToCharArray();
            for (int symbol = 23; symbol < 31; symbol++)
            {
                highScoreString.Append(symbols[symbol]);
            }
            _highScore = Int32.Parse(highScoreString.ToString());

            Console.WriteLine(screenTemplate);
        }
        internal static void UpdateInterface()
        {
            switch (_nextShape.type)
            {
                case ShapeType.I:
                    Console.SetCursorPosition(23, 2);
                    Console.Write("████████");
                    Console.SetCursorPosition(23, 3);
                    Console.Write("        ");
                    break;
                case ShapeType.O:
                    Console.SetCursorPosition(23, 2);
                    Console.Write("  ████  ");
                    Console.SetCursorPosition(23, 3);
                    Console.Write("  ████  ");
                    break;
                case ShapeType.T:
                    Console.SetCursorPosition(23, 2);
                    Console.Write("██████  ");
                    Console.SetCursorPosition(23, 3);                    
                    Console.Write("  ██    ");
                    break;
                case ShapeType.J:
                    Console.SetCursorPosition(23, 2);
                    Console.Write("██████  ");
                    Console.SetCursorPosition(23, 3);
                    Console.Write("    ██  ");
                    break;
                case ShapeType.L:
                    Console.SetCursorPosition(23, 2);
                    Console.Write("██████  ");
                    Console.SetCursorPosition(23, 3);
                    Console.Write("██      ");
                    break;
                case ShapeType.S:
                    Console.SetCursorPosition(23, 2);
                    Console.Write("  ████  ");
                    Console.SetCursorPosition(23, 3);
                    Console.Write("████    ");
                    break;
                case ShapeType.Z:
                    Console.SetCursorPosition(23, 2);
                    Console.Write("████    ");
                    Console.SetCursorPosition(23, 3);
                    Console.Write("  ████  ");
                    break;
            }
            Console.SetCursorPosition(23, 6);
            Console.Write(_level);
            Console.SetCursorPosition(23, 9);
            Console.Write(_score);
        }
        internal static void RowsClearedEventHandler(int rowsCleared)
        {
            _score += rowsCleared;
            _level = _score / 10;
            _gameSpeed = _initialGameSpeed * Math.Pow(_gameSpeedIncreasePercent,_level);  // game speed is accelerated by AccelerationRate for every level; a logarithmic increase
        }

        private static void PauseGame()
        {
            Console.SetCursorPosition(6, 8);
            Console.Write("┌┌─────────┐");  // for some reason, a single "┌ " doesn't render at all
            Console.SetCursorPosition(6, 9);
            Console.Write("│  PAUSE  │");
            Console.SetCursorPosition(6, 10);
            Console.Write("│PRESS ESC│");
            Console.SetCursorPosition(6, 11);
            Console.Write("└─────────┘");
            Console.SetCursorPosition(1, 20);
            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    return;
                }
            }
        }
        private static void EndGame()
        {
            if (_score <= _highScore)
            {
                Console.SetCursorPosition(5, 9);
                Console.Write("┌──────────┐");
                Console.SetCursorPosition(5, 10);
                Console.Write("│GAME OVER!│");
                Console.SetCursorPosition(5, 11);
                Console.Write("└──────────┘");
            }
            else
            {
                Console.SetCursorPosition(3, 8);
                Console.Write("┌───────────────┐");
                Console.SetCursorPosition(3, 9);
                Console.Write("│  GAME OVER!   │");
                Console.SetCursorPosition(3, 10);
                Console.Write("│NEW HIGH SCORE!│");
                Console.SetCursorPosition(3, 11);
                Console.Write("└───────────────┘");

                StringBuilder screenTemplate = new StringBuilder(CreateScreen());
                char[] scoreDigits = _score.ToString().ToCharArray();
                int index = 478;    // starting index for High Score field
                foreach (char digit in scoreDigits)
                {
                    screenTemplate[index] = digit;
                    index++;
                }
                while (index < 486)   // ending index for High Score field
                {
                    screenTemplate[index] = ' ';    // fill with spaces if there's still space left in field
                    index++;
                }
                using StreamWriter streamWriter = new StreamWriter("ScreenTemplate.txt");
                streamWriter.Write(screenTemplate.ToString());
            }

            _isGameOver = true;
            Thread.Sleep(1000);
            Console.ReadKey();
        }

        private static string CreateScreen()
        { 
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("┌────────────────────┬──────────┐");
            stringBuilder.AppendLine("│                    │Next:     │");
            stringBuilder.AppendLine("│                    │          │");
            stringBuilder.AppendLine("│                    │          │");
            stringBuilder.AppendLine("│                    ├──────────┤");
            stringBuilder.AppendLine("│                    │Level:    │");
            stringBuilder.AppendLine("│                    │          │");
            stringBuilder.AppendLine("│                    ├──────────┤");
            stringBuilder.AppendLine("│                    │Score:    │");
            stringBuilder.AppendLine("│                    │          │");
            stringBuilder.AppendLine("│                    ├──────────┤");
            stringBuilder.AppendLine("│                    │High      │");
            stringBuilder.AppendLine("│                    │Score:    │");
            stringBuilder.AppendLine("│                    │ 0        │");
            stringBuilder.AppendLine("│                    ├──────────┤");
            stringBuilder.AppendLine("│                    │Controls: │");
            stringBuilder.AppendLine("│                    │Move:     │");
            stringBuilder.AppendLine("│                    │    Arrows│");
            stringBuilder.AppendLine("│                    │Rotate:   │");
            stringBuilder.AppendLine("│                    │  Spacebar│");
            stringBuilder.AppendLine("│                    │Pause: Esc│");
            stringBuilder.AppendLine("└────────────────────┴──────────┘");
            return stringBuilder.ToString();
        }
    }

}

