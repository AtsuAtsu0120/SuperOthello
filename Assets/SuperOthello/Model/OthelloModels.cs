﻿namespace SuperOthello.Model
{
    public readonly struct CellPosition
    {
        public int Row { get; }
        public int Column { get; }
        
        public CellPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }

    public enum CellState
    {
        Empty,
        Black,
        White
    }

    public enum Direction
    {
        TopLeft,
        TopMiddle,
        TopRight,
        MiddleLeft,
        Middle,
        MiddleRight,
        BottomLeft,
        BottomMiddle,
        BottomRight
    }
}