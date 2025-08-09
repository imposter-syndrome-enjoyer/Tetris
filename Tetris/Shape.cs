using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    internal enum ShapeType
    {
        I,
        O,
        T,
        J,
        L,
        S,
        Z
    }

    internal enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    internal class Shape : IDisposable
    {
        static Random random = new Random();
        internal readonly ShapeType type;
        private (int x, int y)[] _coordinates;
        internal (int x, int y)[] Coordinates
        {
            set
            {
                if (AreCoordsValid(value))
                {
                    _coordinates = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Invalid Coordinates!");
                }
            }
            get => _coordinates;
        }
        internal Shape()
        {
            Array shapeTypes = Enum.GetValues(typeof(ShapeType));
            type = (ShapeType)shapeTypes.GetValue(random.Next(shapeTypes.Length))!;
            (int x, int y) = (PlayField.Width/2,  PlayField.Height - 1);  // shape's spawn point
            _coordinates = new (int, int)[4];
            switch (type)
            {
                case ShapeType.I:
                    Coordinates[0] = (x, y);
                    Coordinates[1] = (x + 1, y);
                    Coordinates[2] = (x - 1, y);
                    Coordinates[3] = (x - 2, y);
                    break;
                case ShapeType.O:
                    Coordinates[0] = (x, y);
                    Coordinates[1] = (x - 1, y);
                    Coordinates[2] = (x, y - 1);
                    Coordinates[3] = (x - 1, y - 1);
                    break;
                case ShapeType.T:
                    Coordinates[0] = (x, y);
                    Coordinates[1] = (x - 1, y);
                    Coordinates[2] = (x + 1, y);
                    Coordinates[3] = (x, y - 1);
                    break;
                case ShapeType.J:
                    Coordinates[0] = (x, y);
                    Coordinates[1] = (x, y - 1);
                    Coordinates[2] = (x - 1, y);
                    Coordinates[3] = (x - 2, y);
                    break;
                case ShapeType.L:
                    Coordinates[0] = (x - 1, y);
                    Coordinates[1] = (x - 1, y - 1);
                    Coordinates[2] = (x, y);
                    Coordinates[3] = (x + 1, y);
                    break;
                case ShapeType.S:
                    Coordinates[0] = (x, y);
                    Coordinates[1] = (x + 1, y);
                    Coordinates[2] = (x, y - 1);
                    Coordinates[3] = (x - 1, y - 1);
                    break;
                case ShapeType.Z:
                    Coordinates[0] = (x, y);
                    Coordinates[1] = (x - 1, y);
                    Coordinates[2] = (x, y - 1);
                    Coordinates[3] = (x + 1, y - 1);
                    break;
            }
        }
        internal bool Move(Direction direction)
        {
            (int, int)[] newCoords = ShiftSegments(this.Coordinates, direction);

            if (AreCoordsValid(newCoords) && !PlayField.IsShapeObstructed(newCoords))
            {
                this.Coordinates = newCoords;
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void Rotate()
        {
            (int x, int y)[] newCoords = ((int,int)[])_coordinates.Clone();
            (int x, int y) pivotPoint = newCoords[0];
            for (int i = 1; i < newCoords.Length; i++) // pivot point at [0] is not rotated, because its position doesn't change
            {
                (int a0, int b0) = newCoords[i];

                // for rotational matrix to work, coordinates need to be relative to pivot point:
                a0 -= pivotPoint.x;
                b0 -= pivotPoint.y;

                // simplified rotational matrix for angle of 90 degrees:
                int a = 0 - b0;
                int b = a0;

                // restoring original positions:
                a += pivotPoint.x;
                b += pivotPoint.y;
                newCoords[i] = (a, b);
            }

            // nudge the shape if it was rotated just outside the field border
            // at most, a shape can be 2 segments outside the line:

            for (int i = 0; i < newCoords.Length; i++)
            {
                if (newCoords[i].x < 0)
                {
                    newCoords = ShiftSegments(newCoords, Direction.Right);
                    i = -1;
                    continue;
                }
                if (newCoords[i].x >= PlayField.Width)
                {
                    newCoords = ShiftSegments(newCoords, Direction.Left);
                    i = -1;
                    continue;
                }
                if (newCoords[i].y >= PlayField.Height)
                {
                    newCoords = ShiftSegments(newCoords, Direction.Down);
                    i = -1;
                    continue;
                }
                if (newCoords[i].y < 0)
                {
                    newCoords = ShiftSegments(newCoords, Direction.Up);
                    i = -1;
                    continue;
                }
            }

            if (AreCoordsValid(newCoords) && !PlayField.IsShapeObstructed(newCoords))
            {
                this.Coordinates = newCoords;
            }
        }
        (int, int)[] ShiftSegments((int, int)[] coords, Direction direction)
        {
            (int x, int y)[] newCoords = ((int, int)[])coords.Clone();
            for (int i = 0; i < newCoords.Length; i++)
            {
                switch (direction)
                {
                    case Direction.Up:
                        newCoords[i].y += 1;
                        break;
                    case Direction.Down:
                        newCoords[i].y -= 1;
                        break;
                    case Direction.Left:
                        newCoords[i].x -= 1;
                        break;
                    case Direction.Right:
                        newCoords[i].x += 1;
                        break;
                }
            }
            return newCoords;
        }
        private bool AreCoordsValid((int, int)[] coords)
        {
            foreach ((int x, int y) in coords)
            {
                if (x < 0 || y < 0)
                {
                    return false;
                }
                if (x >= PlayField.Width || y >= PlayField.Height)
                {
                    return false;
                }
            }
            return true;
        }
        public void Dispose()
        {
            _coordinates = [];
        }
    }
}
