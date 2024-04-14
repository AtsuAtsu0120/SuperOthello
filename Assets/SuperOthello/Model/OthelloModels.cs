using System;
using UnityEngine;

namespace SuperOthello.Model
{
    [Serializable]
    public struct CellPosition
    {
        [field:SerializeField] public int Row { get; private set; }
        [field:SerializeField] public int Column { get; private set; }
        
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