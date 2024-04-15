using System.Collections.Generic;
using MessagePipe;
using Unity.Collections;

namespace SuperOthello.Model
{
    public class OthelloGame
    {
        public const int RowLength = 8;
        public const int ColumnLength = 8;
        
        private readonly CellState[,] _board;
        private readonly IPublisher<CellState[,]> _boardPublisher;
        private readonly IPublisher<IEnumerable<(int row, int column)>> _canPutPublisher;
        private readonly IPublisher<(int black, int white)> _countPublisher;

        private List<CellPosition> _turnableList = new();
        private bool _isBlackTurn;

        public OthelloGame(IPublisher<CellState[,]> boardPublisher, IPublisher<IEnumerable<(int row, int column)>> canPutPublisher, IPublisher<(int black, int white)> countPublisher)
        {
            _boardPublisher = boardPublisher;
            _canPutPublisher = canPutPublisher;
            _countPublisher = countPublisher;
            
            _board = new CellState[RowLength, ColumnLength];
            _board[3, 3] = CellState.Black;
            _board[4, 3] = CellState.White;
            _board[3, 4] = CellState.White;
            _board[4, 4] = CellState.Black;
            
            CountPieces();
            _boardPublisher.Publish(_board);

            var canPutPositionList = GetEnablePutPosition();
            _canPutPublisher.Publish(canPutPositionList);

            _turnableList.Capacity = 6;
        }
        
        /// <summary>
        /// オセロのコマを置く
        /// </summary>
        /// <param name="position">置く場所</param>
        [BurstCompatible]
        public void Put(CellPosition position)
        {
            _board[position.Row, position.Column] = _isBlackTurn ? CellState.Black : CellState.White;
            
            // 相手の色を取得
            var opponentColorState = _isBlackTurn ? CellState.White : CellState.Black;
            
            // 自分のセルの周りのセルを確認して、ひっくり返す
            var direction = Direction.TopLeft;
            for (var column = -1; column < 2; column++)
            {
                for (var row = -1; row < 2; row++)
                {
                    _turnableList.Clear();
                    // どちらも0は自分を調べることになるので、なし
                    if (row is 0 && column is 0)
                    {
                        direction++;
                        continue;
                    }
                    
                    var checkRow = position.Row + row;
                    var checkColumn = position.Column + column;
                    if (checkRow >= RowLength || checkColumn >= ColumnLength || checkRow < 0 || checkColumn < 0)
                    {
                        continue;
                    }
                    if (_board[checkRow, checkColumn] == opponentColorState)
                    {
                        if (CanTurnOver(direction, checkRow, checkColumn))
                        {
                            foreach (var cellPosition in _turnableList)
                            {
                                _board[cellPosition.Row, cellPosition.Column] = _isBlackTurn ? CellState.Black : CellState.White;
                            }   
                        }
                    }

                    direction++;
                }
            }
            
            CountPieces();
            _boardPublisher.Publish(_board);
            _isBlackTurn = !_isBlackTurn;
            
            var canPutPositionList = GetEnablePutPosition();
            _canPutPublisher.Publish(canPutPositionList);
        }
        
        
        private IEnumerable<(int row, int column)> GetEnablePutPosition()
        {
            // 自分の色を取得
            var playerColorState = _isBlackTurn ? CellState.Black : CellState.White;
            // 自分の色のセルを取得
            var cellPositions = GetPlayerColorCellPosition(playerColorState);

            // 相手の色を取得
            var opponentColorState = _isBlackTurn ? CellState.White : CellState.Black;
            
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
                        if (checkRow >= RowLength || checkColumn >= ColumnLength || checkRow < 0 || checkColumn < 0)
                        {
                            continue;
                        }
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
                return true;
            }
            
            return false;
        }

        private bool CanTurnOver(Direction direction, int row, int column)
        {
            var newColor = _isBlackTurn ? CellState.Black : CellState.White;
            _turnableList.Add(new CellPosition(row, column));
            var diff = OthelloUtility.GetPositionDifferenceByDirection(direction);

            (int row, int column) nextPosition = (row + diff.row, column + diff.column);
            if (nextPosition.row >= RowLength || nextPosition.column >= ColumnLength 
                                              || nextPosition.row < 0 || nextPosition.column < 0)
            {
                return false;
            }
            var nextCellState = _board[nextPosition.row, nextPosition.column];
            if (nextCellState != newColor && nextCellState != CellState.Empty)
            {
                return CanTurnOver(direction, nextPosition.row, nextPosition.column);
            }

            if(nextCellState == newColor)
            {
                return true;
            }

            return false;
        }

        private void CountPieces()
        {
            var black = 0;
            var white = 0;
            foreach (var cellState in _board)
            {
                if (cellState == CellState.Black)
                {
                    black++;
                }
                else if (cellState == CellState.White)
                {
                    white++;
                }
            }
            
            _countPublisher.Publish((black, white));
        }
    }
}