using MessagePipe;
using TMPro;
using UnityEngine;
using VContainer;

namespace SuperOthello.View
{
    public class OthelloUI : UIBase
    {
        [SerializeField] private TextMeshProUGUI _blackCountText;
        [SerializeField] private TextMeshProUGUI _whiteCountText;

        [Inject]
        public void Constructor(ISubscriber<(int black, int white)> countSubscriber)
        {
            countSubscriber.Subscribe(data =>
            {
                SetBlackCount(data.black);
                SetWhiteCount(data.white);
            });
        }
        
        private void SetBlackCount(int count)
        {
            _blackCountText.SetText("Black : {0}", count);
        }
        private void SetWhiteCount(int count)
        {
            _whiteCountText.SetText("White : {0}", count);
        }
    }
}