using System;
using System.Collections.Generic;
using MessagePipe;
using R3;
using UnityEngine;

namespace SuperOthello.Model
{
    public class OthelloGame
    {
        private readonly CellState[,] _board;
        private readonly IPublisher<CellState[,]> _boardPublisher;

        private const int RowLength = 8;
        private const int ColumnLength = 8;

        public OthelloGame(IPublisher<CellState[,]> boardPublisher)
        {
            _boardPublisher = boardPublisher;
            
            _board = new CellState[RowLength, ColumnLength];
            _board[3, 3] = CellState.Black;
            _board[4, 3] = CellState.White;
            _board[3, 4] = CellState.White;
            _board[4, 4] = CellState.Black;
            
            _boardPublisher.Publish(_board);

            var list = GetEnablePutPosition(false);
            foreach (var place in list)
            {
                Debug.Log($"{place.row}:{place.column}");
            }
        }

        private IEnumerable<(int row, int column)> GetEnablePutPosition(bool isBlackTurn)
        {
            // 自分の色を取得
            var playerColorState = isBlackTurn ? CellState.Black : CellState.White;
            // 自分の色のセルを取得
            var cellPositions = GetPlayerColorCellPosition(playerColorState);

            // 相手の色を取得
            var opponentColorState = isBlackTurn ? CellState.White : CellState.Black;
            
            // 自分のセルの周りのセルを確認して、置ける位置を確認する。
            foreach (var cell in cellPositions)
            {
                var direction = Direction.TopLeft;
                for (var column = -1; column < 2; column++)
                {
                    for (var row = -1; row < 2; row++)
                    {
                        // どちらも0は自分を調べることになるので、なし
                        if (row is 0 && column is 0)
                        {
                            direction++;
                            continue;
                        }
                        
                        var checkRow = cell.Row + row;
                        var checkColumn = cell.Column + column;
                        if (_board[checkRow, checkColumn] == opponentColorState)
                        {
                            if (CanPut(opponentColorState, direction, checkRow, checkColumn))
                            {
                                var positionDiff = OthelloUtility.GetPositionDifferenceByDirection(direction);
                                yield return (checkRow + positionDiff.row, checkColumn + positionDiff.column);
                            }
                        }

                        direction++;
                    }
                }
            }
        }

        private IEnumerable<CellPosition> GetPlayerColorCellPosition(CellState playerColorState)
        {
            for (var row = 0; row < RowLength; row++)
            {
                for (var column = 0; column < ColumnLength; column++)
                {
                    if (_board[row, column] == playerColorState)
                    {
                        yield return new CellPosition(row, column);
                    }
                }
            }
        }

        private CellState GetNextCellState(Direction direction, int row, int column)
        {
            var positionDiff = OthelloUtility.GetPositionDifferenceByDirection(direction);
            return _board[row - positionDiff.row, column - positionDiff.column];
        }

        private bool CanPut(CellState opponentColorState, Direction direction, int row, int column)
        {
            var state = GetNextCellState(direction, row, column);
            if (state == opponentColorState)
            {
                if (CanPut(opponentColorState, direction, row, column))
                {
                    return true;
                }
            }
            else if(state != CellState.Empty)
            {
                // このとき自分の色なはず。
                return true;
            }
            
            return false;
        }
    }
}