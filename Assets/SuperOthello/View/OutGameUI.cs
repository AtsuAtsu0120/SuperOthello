using MessagePipe;
using R3;
using TMPro;
using UnityEngine;
using VContainer;

namespace SuperOthello.View
{
    public class OutGameUI : UIBase
    {
        public Observable<Unit> OnNextGame => _onNextGame;
        
        [SerializeField] private TextMeshProUGUI _endText;
        private IPublisher<Unit> _nextGamePublisher;
        private Subject<Unit> _onNextGame = new();
        
        [Inject]
        public void Constructor(IPublisher<Unit> nextGamePublisher)
        {
            _nextGamePublisher = nextGamePublisher;
        }
        public void SetEndText(bool isBlackWin)
        {
            _endText.SetText($"{(isBlackWin ? "Black" : "White")} WIN");
        }

        public void NextGame()
        {
            _onNextGame.OnNext(Unit.Default);
            _nextGamePublisher.Publish(Unit.Default);
            
            SetUIActive(false);
        }
    }
}