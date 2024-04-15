using System;
using UnityEngine;

namespace SuperOthello
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIBase : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvas;
        
        public void SetUIActive(bool isActive)
        {
            _canvas.alpha = isActive ? 1 : 0;
            _canvas.blocksRaycasts = isActive;
        }
        
        #if UNITY_EDITOR
        private void Reset()
        {
            _canvas = GetComponent<CanvasGroup>();
        }
        #endif
    }
}