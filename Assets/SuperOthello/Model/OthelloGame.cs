using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MessagePipe;
using Unity.Collections;
using VContainer;
using VContainer.Unity;

namespace SuperOthello.Model
{
    public class OthelloGame : IInitializable, IDisposable
    {
        public const int RowLength = 8;
        public const int ColumnLength = 8;
        
        private readonly CellState[,] _board;
        private readonly IPublisher<CellState[,]> _boardPublisher;
        private readonly IPublisher<(IEnumerable<(int row, int column)> canPutList, bool isBlackTurn)> _canPutPublisher;
        private readonly IPublisher<(int black, int white)> _countPublisher;
        private readonly IPublisher<bool> _gameEndPublisher;

        private List<CellPosition> _turnableList = new();
        private bool _isBlackTurn;
        private bool _isGameEnd;

        private (int blackCount, int whiteCount) _pieceCount = (0, 0);

        private IDisposable _disposable;

        [Inject]
        public OthelloGame(ISubscriber<CellPosition> putSubscriber, IPublisher<CellState[,]> boardPublisher, IPublisher<(IEnumerable<(int row, int column)> canPutList, bool isBlackTurn)> canPutPublisher, IPublisher<(int black, int white)> countPublisher, IPublisher<bool> gameEndPublisher)
        {
            _boardPublisher = boardPublisher;
            _canPutPublisher = canPutPublisher;
            _countPublisher = countPublisher;
            _gameEndPublisher = gameEndPublisher;

            var d = DisposableBag.CreateBuilder();
            putSubscriber.Subscribe(Put).AddTo(d);
            _disposable = d.Build();
            
            _board = new CellState[RowLength, ColumnLength];
            _board[3, 3] = CellState.Black;
            _board[3, 4] = CellState.White;
            _board[4, 3] = CellState.White;
            _board[4, 4] = CellState.Black;
            
            CountPieces();
            _boardPublisher.Publish(_board);

            var canPutPositionList = GetEnablePutPosition();
            _canPutPublisher.Publish((canPutPositionList, _isBlackTurn));

            _turnableList.Capacity = 6;
        }
        public void Initialize()
        {
            
        }
        
        /// <summary>
        /// オセロのコマを置く
        /// </summary>
        /// <param name="position">置く場所</param>
        [BurstCompatible]
        private void Put(CellPosition position)
        {
            _board[position.Row, position.Column] = _isBlackTurn ? CellState.Black : CellState.White;

            var aroundList = GetAroundOpponentPieces(position);
            foreach (var (direction, row, column) in aroundList)
            {
                if (CanTurnOver(direction, row, column))
                {
                    foreach (var cellPosition in _turnableList)
                    {
                        _board[cellPosition.Row, cellPosition.Column] = _isBlackTurn ? CellState.Black : CellState.White;
                    }
                }
                _turnableList.Clear();
            }
            
            CountPieces();
            _boardPublisher.Publish(_board);

            NextTurn();
        }

        private void NextTurn()
        {
            _isBlackTurn = !_isBlackTurn;
            
            var canPutPositionList = GetEnablePutPosition();
            if (!canPutPositionList.Any())
            {
                // ゲームエンドフラグがたっていて、こっちも置けない場合は試合終了
                if (_isGameEnd)
                {
                    _gameEndPublisher.Publish(_pieceCount.blackCount > _pieceCount.whiteCount);
                    return;
                }
                _isGameEnd = true;
                NextTurn();
            }
            else
            {
                _isGameEnd = false;
            }
            _canPutPublisher.Publish((canPutPositionList, _isBlackTurn));
        }
        
        private IEnumerable<(int row, int column)> GetEnablePutPosition()
        {
            // 自分の色を取得
            var playerColorState = _isBlackTurn ? CellState.Black : CellState.White;
            // 自分の色のセルを取得
            var cellPositions = GetPlayerColorCellPosition(playerColorState);
            
            // 自分のセルの周りのセルを確認して、置ける位置を確認する。
            foreach (var position in cellPositions)
            {
                var aroundList = GetAroundOpponentPieces(position);
                foreach (var (direction, row, column) in aroundList)
                {
                    // 次が置けたら…
                    if (CanPutNext(direction, row, column, out var canPutPosition))
                    {
                        if (_board[canPutPosition.Row, canPutPosition.Column] is CellState.Empty)
                        {
                            yield return (canPutPosition.Row, canPutPosition.Column);
                        }
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

        private (CellState state, CellPosition position) GetNextCellState(Direction direction, int row, int column)
        {
            var positionDiff = OthelloUtility.GetPositionDifferenceByDirection(direction);
            
            var nextRow = row + positionDiff.row;
            var nextColumn = column + positionDiff.column;

            if (nextRow >= RowLength || nextColumn >= ColumnLength || nextRow < 0 || nextColumn < 0)
            {
                return (CellState.Error, default);
            }
            
            return (_board[nextRow, nextColumn], new CellPosition(nextRow, nextColumn));
        }

        private bool CanPutNext(Direction direction, int row, int column, out CellPosition canPutPosition)
        {
            var opponentColorState = _isBlackTurn ? CellState.White : CellState.Black;
            var (state, position) = GetNextCellState(direction, row, column);
            canPutPosition = position;
            
            if (state is CellState.Error)
            {
                return false;
            }
            if (state == opponentColorState)
            {
                if (CanPutNext(direction, position.Row, position.Column, out canPutPosition))
                {
                    return true;
                }
            }
            else if(state is CellState.Empty)
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

            _pieceCount = (black, white);
            _countPublisher.Publish((black, white));
        }

        private IEnumerable<(Direction direction, int row, int column)> GetAroundOpponentPieces(CellPosition position)
        {
            // 相手の色を取得
            var opponentColorState = _isBlackTurn ? CellState.White : CellState.Black;
            
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
                        
                    var checkRow = position.Row + row;
                    var checkColumn = position.Column + column;
                    if (checkRow >= RowLength || checkColumn >= ColumnLength || checkRow < 0 || checkColumn < 0)
                    {
                        direction++;
                        continue;
                    }
                    if (_board[checkRow, checkColumn] == opponentColorState)
                    {
                        yield return (direction, checkRow, checkColumn);
                    }

                    direction++;
                }
            }
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}