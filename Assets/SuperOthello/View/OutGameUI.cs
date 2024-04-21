using MessagePipe;
using R3;
using TMPro;
using UnityEngine;
using VContainer;

namespace SuperOthello.View
{
    public class OutGameUI : UIBase
    {
        [SerializeField] private TextMeshProUGUI _endText;
        private IPublisher<Unit> _nextGamePublisher;
        
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
            _nextGamePublisher.Publish(Unit.Default);
            
            SetUIActive(false);
        }
    }
}