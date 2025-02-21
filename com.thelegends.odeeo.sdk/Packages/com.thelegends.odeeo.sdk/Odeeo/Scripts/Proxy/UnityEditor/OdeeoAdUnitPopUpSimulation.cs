#if UNITY_EDITOR
using Odeeo.Proxy.AdUnit;
using Odeeo.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Odeeo
{
    internal class OdeeoAdUnitPopUpSimulation : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Button buttonSkip;

        public bool RewardAvailable { get; set; }
        
        private OdeeoAdUnitConfig _config;
        private OdeeoAdUnitSimulation _adUnit;
        private Vector2 _size;

        public void Init(OdeeoAdUnitConfig data, OdeeoAdUnitSimulation adUnit)
        {
            _config = data;
            _adUnit = adUnit;
            
            buttonSkip.onClick.AddListener(OnCloseButtonPressed);

            if (_config.PopupType == OdeeoAdUnit.PopUpType.BannerPopUp)
                _size = new Vector2(320, 50);
            else
                _size = new Vector2(120, 120);
        }
        
        public void ShowPopUp()
        {
            gameObject.SetActive(true);

            SetPosition();
        }

        private void OnCloseButtonPressed()
        {
            _adUnit.RemoveAd(OdeeoAdUnit.CloseReason.UserClose);
        }

        private void SetPosition()
        {
            float deviceScale = OdeeoDpiResolution.GetDeviceScale();
            float canvasScaleFactor = canvas.scaleFactor;
            
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            
            float scaleMultiplier = 1f;
            
            Vector2 scaledSize = new Vector2(_size.x * deviceScale, _size.y * deviceScale);
            
            if (scaledSize.x > Screen.width)
                scaleMultiplier = Screen.width / scaledSize.x;
            if (scaledSize.y > Screen.height)
                scaleMultiplier = Screen.height / scaledSize.y;

            scaledSize *= scaleMultiplier;
            
            rect.sizeDelta = scaledSize / canvasScaleFactor;

            float xPos = _config.PopupOffset.x * deviceScale;
            float yPos = _config.PopupOffset.y * deviceScale;
            
            xPos = Mathf.Clamp(xPos, 0f, Screen.width - scaledSize.x);
            yPos = Mathf.Clamp(yPos, 0f, Screen.height - scaledSize.y);

            RectTransform buttonRect = buttonSkip.GetComponent<RectTransform>();
            if (buttonRect)
            {
                Vector2 baseSize = new Vector2(45f, 45f);
                buttonRect.sizeDelta = new Vector2(
                    baseSize.x * deviceScale / canvasScaleFactor,
                    baseSize.y * deviceScale / canvasScaleFactor);
            }

            Rect canvasPixelRect = canvas.pixelRect;

            int position = _config.PopupType == OdeeoAdUnit.PopUpType.BannerPopUp
                ? (int)_config.BannerPopupPosition
                : (int)_config.IconPopupPosition;
            
            // As both enums are identical, cast like this, but need to revisit it later
            OdeeoSdk.IconPosition popupPosition = (OdeeoSdk.IconPosition) position;
            
            switch (popupPosition)
            {
                case OdeeoSdk.IconPosition.Centered:
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    yPos = canvasPixelRect.height * 0.5f - yPos;
                    xPos = canvasPixelRect.width * 0.5f - xPos;
                    break;
                case OdeeoSdk.IconPosition.BottomLeft:
                    rect.pivot = Vector2.zero;
                    break;
                case OdeeoSdk.IconPosition.BottomRight:
                    rect.pivot = new Vector2(1f, 0f);
                    xPos = canvasPixelRect.width - xPos;
                    break;
                case OdeeoSdk.IconPosition.TopLeft:
                    rect.pivot = new Vector2(0f, 1f);
                    yPos = canvasPixelRect.height - yPos;
                    break;
                case OdeeoSdk.IconPosition.TopRight:
                    rect.pivot = new Vector2(1f, 1f);
                    yPos = canvasPixelRect.height - yPos;
                    xPos = canvasPixelRect.width - xPos;
                    break;
                case OdeeoSdk.IconPosition.CenterLeft:
                    rect.pivot = new Vector2(0f, 0.5f);
                    yPos = canvasPixelRect.height * 0.5f - yPos;
                    break;
                case OdeeoSdk.IconPosition.CenterRight:
                    rect.pivot = new Vector2(1f, 0.5f);
                    yPos = canvasPixelRect.height * 0.5f - yPos;
                    xPos = canvasPixelRect.width - xPos;
                    break;
                case OdeeoSdk.IconPosition.BottomCenter:
                    rect.pivot = new Vector2(0.5f, 0f);
                    xPos = canvasPixelRect.width * 0.5f - xPos;
                    break;
                case OdeeoSdk.IconPosition.TopCenter:
                    rect.pivot = new Vector2(0.5f, 1f);
                    xPos = canvasPixelRect.width * 0.5f - xPos;
                    yPos = canvasPixelRect.height - yPos;
                    break;
            }

            rect.position = new Vector3(xPos, yPos, 0);
        }
        
        private void OnDestroy()
        {
            buttonSkip.onClick.RemoveAllListeners();
        }
    }
}
#endif