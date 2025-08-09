using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    internal static class PlayField
    {
        internal const int Width = 10;
        internal const int Height = 20;
        private static bool[,] _playField;
        internal delegate void RowsClearedHander(int rowsCleared);
        internal static event RowsClearedHander RowsClearedEvent;
        static PlayField()
        {
            _playField = new bool[Width, Height];
        }

        internal static bool[,] Field
        {
            get
            {
                return (bool[,]) _playField.Clone();
            }            
        }

        internal static bool IsShapeObstructed((int, int)[] coordinates)
        {
            foreach((int x, int y) coord in coordinates)
            {
                try
                {
                    if (_playField[coord.x, coord.y] == true)   // check if there are pile segments overlapping with current shape
                    {
                        return true;
                    }
                }
                catch (IndexOutOfRangeException)    // if the shape is outside of playfield bounds
                {
                    return true;
                }
            }
            return false;
        }

        internal static void AddToPile(Shape shape)
        {
            foreach((int x, int y) coord in shape.Coordinates)
            {
                _playField[coord.x, coord.y] = true;
            }

            shape.Dispose();

            //checking for any full rows:

            int rowsCleared = 0;
            for (int j = 0; j < Height; j++)
            {
                bool isRowFull = true;
                for (int i = 0; i < Width; i++)
                {
                    if (_playField[i, j] != true)
                    {
                        isRowFull = false;
                        break;
                    }
                }
                if (isRowFull)
                {
                    ClearRow(j);
                    rowsCleared++;
                    j = -1;
                }
            }
            if (rowsCleared > 0) RowsClearedEvent.Invoke(rowsCleared);
        }

        private static void ClearRow(int row)
        {
            for (int i = 0; i < Width; i++)
            {
                _playField[i,row] = false;
            }
            Game.RenderField();
            Thread.Sleep(200);
            for (int j = row; j < Height - 1; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    _playField[i, j] = _playField[i, j + 1];
                }    
            }
            for (int i = 0; i < Width; i++)
            {
                _playField[i, Height - 1] = false;
            }
            Game.RenderField();
            Thread.Sleep(200);
        }

        internal static void EraseField()
        {
            _playField = new bool[Width, Height];
        }
    }
}
