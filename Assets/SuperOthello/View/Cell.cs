using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using R3;
using SuperOthello.Model;
using UnityEngine;
using UnityEngine.VFX;

namespace SuperOthello.View
{
    public class Cell : MonoBehaviour
    {
        public (bool canPut, bool isBlackTurn) CanPutInfo
        {
            get => _canPutInfo.Value;
            set => _canPutInfo.Value = value;
        }
        
        [field: SerializeField] public CellPosition CellPosition { get; private set; }
        public CellState State 
        { 
            get => _state.Value;
            private set => _state.Value = value;
        }

        [SerializeField] private GameObject _piecePrefab;
        [SerializeField] private VisualEffect _canputEffect;
        
        private GameObject _piece;
        private ReactiveProperty<CellState> _state = new();
        private readonly ReactiveProperty<(bool canPut, bool isBlackTurn)> _canPutInfo = new();
        
        private static readonly int VFXColorProperty = Shader.PropertyToID("IsBlackTurn");
        private const float AnimationTime = 0.2f;

        private void Awake()
        {
            _canPutInfo.Subscribe(value =>
            {
                _canputEffect.SetBool(VFXColorProperty, value.isBlackTurn);
                if (value.canPut)
                {
                    _canputEffect.Play();
                }
                else
                {
                    _canputEffect.Stop();
                }
            }).AddTo(this);
            
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
            var positionY = _piece.transform.position.y;
            await LMotion.Create(positionY, 2f, AnimationTime)
                .WithEase(Ease.OutQuad).BindToPositionY(_piece.transform);
            
            if (state is CellState.Black)
            {
                await LMotion.Create(0f, 180f, AnimationTime)
                    .BindToLocalEulerAnglesX(_piece.transform);
                
                // 謎にTweenの前にRotationが戻るときがあるので、強制的に変える
                _piece.transform.localEulerAngles = new (180f, 0f, 0f);
            }
            else
            {
                await LMotion.Create(180f, 0f, AnimationTime)
                    .BindToLocalEulerAnglesX(_piece.transform);
                
                // 謎にTweenの前にRotationが戻るときがあるので、強制的に変える
                _piece.transform.localEulerAngles = new (0f, 0f, 0f);
            }
            
            await LMotion.Create(_piece.transform.position.y, positionY, AnimationTime)
                .WithEase(Ease.OutQuad).BindToPositionY(_piece.transform);
        }

        private void OnPutNew(in CellState state)
        {
            _piece = Instantiate(_piecePrefab, transform);

            if (state is CellState.Black)
            {
                _piece.transform.Rotate(180, 0, 0);
            }
        }
    }
}
