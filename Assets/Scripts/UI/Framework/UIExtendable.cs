using UnityEngine;
using DG.Tweening;

namespace UI.Framework
{
    public class UIExtendable : UIPanel
    {
        public ExtendableInfo EInfo;
        private bool _isExtended;
        
        [SerializeField] protected RectTransform panelHolder;

        protected UIExtendable()
        {
            panelName = "extendable";
            EInfo = new ExtendableInfo
            {
                OnX = true,
                OnY = true,
                NewWidth = Screen.width,
                OldWidth = 0,
                NewHeight = Screen.height,
                OldHeight = 0,
                Duration = 0.5f
            };
        }
        
        private void ToggleExtendable()
        {
            // Check if the panel is already extended
            float targetWidth = panelHolder.sizeDelta.x;
            float targetHeight = panelHolder.sizeDelta.y;
            
            if (EInfo.OnX)
            {
                targetWidth = _isExtended ? EInfo.OldWidth : EInfo.NewWidth;
            }
            if (EInfo.OnY)
            {
                targetHeight = _isExtended ? EInfo.OldHeight : EInfo.NewHeight;
                
            }
            switch (EInfo)
            {
                case { OnX: true, OnY: true}:
                    OverXY();
                    break;
                case { OnX: true }:
                    OverX();
                    break;
                case { OnY: true }:
                    OverY();
                    break;
            }
            
            _isExtended = !_isExtended;
            return;

            void OverXY()
            {
                // Set new target width and height to extend the panel fully in both directions
                panelHolder.DOSizeDelta(new Vector2(targetWidth, targetHeight), EInfo.Duration)
                    .SetEase(Ease.InOutSine);
            }
    
            void OverX()
            {
                // Set new target width to extend the panel fully horizontally
                panelHolder.DOSizeDelta(new Vector2(targetWidth, targetHeight), EInfo.Duration)
                    .SetEase(Ease.InOutSine);
            }
    
            void OverY()
            {
                // Set new target height to extend the panel fully vertically
                panelHolder.DOSizeDelta(new Vector2(targetWidth, targetHeight), EInfo.Duration)
                    .SetEase(Ease.InOutSine);
            }
        }

        protected override void Refresh()
        {
            // Override this method in child classes
        }
        
        public override void ToggleSelected(bool selected)
        {
            ToggleExtendable();
            
            base.ToggleSelected(selected);
        }

        public void OnRectTransformDimensionsChange()
        {
            EInfo.NewWidth = Screen.width;
            EInfo.NewHeight = Screen.height;
        }
    }
}

public struct ExtendableInfo
{
    internal bool OnX;
    internal bool OnY;
    
    internal float NewWidth;
    internal float OldWidth;
    internal float NewHeight;
    internal float OldHeight;
    
    internal float Duration;
}