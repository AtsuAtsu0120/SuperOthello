using System;
using SuperOthello.Model;
using UnityEngine;

namespace SuperOthello
{
    public static class OthelloUtility
    {
        public static (int row, int column) GetPositionDifferenceByDirection(Direction direction)
        {
            return direction switch
            {
                Direction.TopLeft => (-1, -1),
                Direction.TopMiddle => (0, -1),
                Direction.TopRight => (1, -1),
                Direction.MiddleLeft => (-1, 0),
                Direction.MiddleRight => (1, 0),
                Direction.BottomLeft => (-1, 1),
                Direction.BottomMiddle => (0, 1),
                Direction.BottomRight => (1, 1),
                _ => throw new ArgumentException()
            };
        }
    }
}