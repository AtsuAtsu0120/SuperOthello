using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using R3;
using SuperOthello.Model;
using UnityEngine;

namespace SuperOthello.View
{
    public class Cell : MonoBehaviour
    {
        public bool CanPut
        {
            get => _canPut.Value;
            set => _canPut.Value = value;
        } 
        [field: SerializeField] public CellPosition CellPosition { get; private set; }
        public CellState State 
        { 
            get => _state.Value;
            private set => _state.Value = value;
        }

        [SerializeField] private GameObject _piecePrefab;

        private GameObject _piece;
        private ReactiveProperty<CellState> _state = new();
        private readonly ReactiveProperty<bool> _canPut = new();

        private void Awake()
        {
            _canPut.Subscribe(value => transform.GetChild(0).gameObject.SetActive(value)).AddTo(this);
            
            _state.Pairwise()
                .Where(state => state.Current is CellState.Black or CellState.White && state.Previous is CellState.Empty)
                .Subscribe(state => OnPutNew(state.Current)).AddTo(this);
            _state.Pairwise()
                .Where(state => state.Current is CellState.Black or CellState.White && state.Previous is not CellState.Empty)
                .Subscribe(state => OnChangeColor(state.Current)).AddTo(this);
        }

        public async void Put(CellState state)
        {
            await UniTask.WaitUntil(() => didAwake);
            State = state;
        }

        private async void OnChangeColor(CellState state)
        {
            await LMotion.Create(_piece.transform.position.y, 10f, 3f)
                .WithEase(Ease.OutQuad).BindToPositionY(_piece.transform);
        }

        private void OnPutNew(CellState state)
        {
            _piece = Instantiate(_piecePrefab, transform);

            if (state is CellState.Black)
            {
                _piece.transform.Rotate(180, 0, 0);
            }
        }
    }
}
