#if UNITY_EDITOR
using System;
using System.Collections;
using Odeeo.Data;
using Odeeo.Logging;
using Odeeo.Scripts.Proxy;
using Odeeo.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Odeeo.Proxy.AdUnit
{
    internal sealed class OdeeoAdUnitSimulation : MonoBehaviour, IOdeeoAdUnitCallbacks
    {
        public event Action<bool, OdeeoAdData> OnAvailabilityChanged;
        public event Action OnShow;
        public event Action<OdeeoAdUnit.CloseReason> OnClose;
        public event Action OnClick;
        public event Action<float> OnReward;
        public event Action<OdeeoImpressionData> OnImpression;
        public event Action OnRewardedPopupAppear;
        public event Action<OdeeoAdUnit.CloseReason> OnRewardedPopupClosed;
        public event Action<OdeeoAdUnit.StateChangeReason> OnPause;
        public event Action<OdeeoAdUnit.StateChangeReason> OnResume;
        public event Action<bool> OnMute;
        public event Action<string, OdeeoAdUnit.ErrorShowReason, string> OnShowFailed;
        
        private const string ADPopupPrefabFilename = "OdeeoPopup.prefab";

        [SerializeField] private RectTransform rect;
        [SerializeField] private Canvas canvas;
        [SerializeField] private int playLength = 8;
        
        [Space]
        [SerializeField] private Text timerText;
        [SerializeField] private Button buttonAd;
        [SerializeField] private Button buttonAction;

        [Space]
        [SerializeField] private RectTransform actionButtonCloseImage;

        private readonly IOdeeoLogging _logging = new OdeeoLogging();
        private OdeeoAdUnitConfig _config;
        
        private Coroutine _biddingRoutine;
        
        private RectTransform _buttonActionRect;
        private OdeeoAdUnitPopUpSimulation _popUpSimulation;

        private bool _isPaused;
        private bool _isAvailable;

        internal void Init(OdeeoAdUnitConfig config)
        {
            _config = config;
            
            buttonAd.onClick.AddListener(OnAdClicked);
            buttonAction.onClick.AddListener(OnActionClicked);

            _buttonActionRect = buttonAction.GetComponent<RectTransform>();

            if (OdeeoAdManager.IsBannerType(_config.PlacementType))
            {
                _config.Size = (320, 50);
                _config.Offset = (0, 0);
            }
            
            if (_config.PlacementType != OdeeoSdk.PlacementType.AudioIconAd)
                buttonAction.gameObject.SetActive(false);
            
            if (OdeeoAdManager.IsAdRewardedType(_config.PlacementType))
                CreatePopUp();

            RestartBidding();
        }

        internal void SetPause(bool isPaused, OdeeoAdUnit.StateChangeReason stateChangeReason)
        {
            _isPaused = isPaused;

            if (isPaused)
                OnPause?.Invoke(stateChangeReason);
            else
                OnResume?.Invoke(stateChangeReason);
        }

        internal void DestroyAd(OdeeoAdUnit.CloseReason reasonType)
        {
            if (_popUpSimulation)
            {
                OnRewardedPopupClosed?.Invoke(reasonType);
                
                if (_popUpSimulation.RewardAvailable)
                {
                    OnReward?.Invoke(Random.Range(0.1f, 100f));    
                }
                
                _popUpSimulation.gameObject.SetActive(false);
            }
            
            OnClose?.Invoke(reasonType);

            StopAllCoroutines();

            gameObject.SetActive(false);

            RestartBidding();
        }

        public void OnDestroy()
        {
            buttonAd.onClick.RemoveAllListeners();
            buttonAction.onClick.RemoveAllListeners();
        }
        
        public void ShowAd()
        {
            _logging.Info($"Showing the add... {_config.PlacementId}");
            gameObject.SetActive(true);
            _isAvailable = false;
            
            StartCoroutine(Timer());
            
            SetPosition();
            SetActionButton();
            
            OdeeoImpressionData impressionData = _config.PlacementType.FromEditor<OdeeoImpressionData>(_config.CustomTag);
            
            OnAvailabilityChanged?.Invoke(false, _config.PlacementType.FromEditor<OdeeoAdData>(_config.CustomTag));
            OnImpression?.Invoke(impressionData);
            OnShow?.Invoke();
            
            if (OdeeoAdManager.IsAdRewardedType(_config.PlacementType))
            {
                _popUpSimulation.ShowPopUp();
                OnRewardedPopupAppear?.Invoke();
            }
        }

        public void RemoveAd(OdeeoAdUnit.CloseReason reason)
        {
            DestroyAd(reason);
        }

        public bool IsAdAvailable()
        {
            return !gameObject.activeInHierarchy && _isAvailable;
        }

        public bool IsAdCached()
        {
            return OdeeoAdManager.TryGetCurrentAdBy(_config.PlacementId, out var unit);
        }

        public void SetBannerPosition(OdeeoSdk.BannerPosition position)
        {
            _logging.Info($"SetBannerPosition {position}");
        }

        public void SetPositionAndSize()
        {
            _logging.Info($"SetPositionAndSize");
        }

        public void SetRewardedPopupType(OdeeoAdUnit.PopUpType type)
        {
            _logging.Info($"SetRewardedPopupType {type}");
        }

        public void SetRewardedPopupBannerPosition(OdeeoSdk.BannerPosition position)
        {
            _logging.Info($"SetRewardedPopupBannerPosition {position}");
        }

        public void SetRewardedPopupIconPosition(OdeeoSdk.IconPosition position, int xOffset, int yOffset)
        {
            _logging.Info($"SetRewardedPopupIconPosition {position} {xOffset} {yOffset}");
        }

        public void SetProgressBarColor(Color progressBarColor)
        {
            _logging.Info($"SetProgressBarColor {progressBarColor}");
        }

        public void SetAudioOnlyBackgroundColor(Color color)
        {
            _logging.Info($"SetAudioOnlyBackgroundColor {color}");
        }

        public void SetAudioOnlyAnimationColor(Color color)
        {
            _logging.Info($"SetAudioOnlyAnimationColor {color}");
        }

        public void SetIconActionButtonPosition(OdeeoAdUnit.ActionButtonPosition position)
        {
            _logging.Info($"SetIconActionButtonPosition {position}");
        }

        public void SetCustomTag(string newTag)
        {
            _config.CustomTag = newTag;
            _logging.Info($"SetCustomTag {newTag}");
        }

        public void TrackRewardedOffer()
        {
            _logging.Info($"TrackRewardedOffer");
        }

        public void TrackAdShowBlocked()
        {
            _logging.Info($"TrackAdShowBlocked");
        }

        public void DispatchOnShowError(OdeeoAdUnit.ErrorShowReason reason, string customMessage)
        {
            string message = string.IsNullOrEmpty(customMessage) ? _config.ErrorMessageBy(reason) : customMessage;
            OnShowFailed?.Invoke(_config.PlacementId, reason, message);
        }
        
        private void OnAdClicked()
        {
            OnClick?.Invoke();
        }

        private void OnActionClicked()
        {
            DestroyAd(OdeeoAdUnit.CloseReason.UserClose);
        }

        private void SetPosition()
        {
            float deviceScale = OdeeoDpiResolution.GetDeviceScale();
            float canvasScaleFactor = canvas.scaleFactor;
            
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);

            float scaleMultiplier = 1f;

            Vector2 scaledSize = new Vector2(_config.Size.x * deviceScale, _config.Size.y * deviceScale);
            
            if (scaledSize.x > Screen.width)
                scaleMultiplier = Screen.width / scaledSize.x;
            if (scaledSize.y > Screen.height)
                scaleMultiplier = Screen.height / scaledSize.y;
            
            scaledSize *= scaleMultiplier;
            
            rect.sizeDelta = scaledSize / canvasScaleFactor;

            float xPos = _config.Offset.x * deviceScale;
            float yPos = _config.Offset.y * deviceScale;
            

            Rect canvasPixelRect = canvas.pixelRect;
            switch (_config.Position)
            {
                case OdeeoSdk.IconPosition.Centered:
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    yPos = canvasPixelRect.height * 0.5f - yPos;
                    xPos = canvasPixelRect.width * 0.5f + xPos;
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
            
            xPos = Mathf.Clamp(xPos, scaledSize.x * rect.pivot.x, Screen.width - scaledSize.x * (1f - rect.pivot.x));
            yPos = Mathf.Clamp(yPos, scaledSize.y * rect.pivot.y, Screen.height - scaledSize.y * (1f - rect.pivot.y));

            rect.position = new Vector3(xPos, yPos, 0);
        }

        private void SetActionButton()
        {
            float deviceScale = OdeeoDpiResolution.GetDeviceScale();
            float canvasScaleFactor = canvas.scaleFactor;
            
            float buttonSize = 20f * deviceScale / canvasScaleFactor;
            Vector2 sizeDelta = new Vector2(buttonSize, buttonSize);
            _buttonActionRect.sizeDelta = sizeDelta;
            actionButtonCloseImage.sizeDelta = sizeDelta * 0.9f;

            switch (_config.ActionButtonPosition)
            {
                case OdeeoAdUnit.ActionButtonPosition.TopLeft:
                    _buttonActionRect.anchorMin = _buttonActionRect.anchorMax = new Vector2(0f, 1f);
                    _buttonActionRect.pivot = new Vector2(0f, 1f);
                    _buttonActionRect.anchoredPosition = Vector2.zero;
                    break;
                case OdeeoAdUnit.ActionButtonPosition.TopRight:
                    _buttonActionRect.anchorMin = _buttonActionRect.anchorMax = new Vector2(1f, 1f);
                    _buttonActionRect.pivot = new Vector2(1f, 1f);
                    _buttonActionRect.anchoredPosition = Vector2.zero;
                    break;
            }
        }

        private void CreatePopUp()
        {
            string prefabPath = OdeeoEditorHelper.GetAssetBasedPath(ADPopupPrefabFilename);
            if (string.IsNullOrEmpty(prefabPath))
            {
                _logging.Error($"Can't find {ADPopupPrefabFilename} asset");
                return;
            }

            OdeeoAdUnitPopUpSimulation logoPrefab = AssetDatabase.LoadAssetAtPath<OdeeoAdUnitPopUpSimulation>(prefabPath);
            logoPrefab.gameObject.SetActive(false);
            _popUpSimulation = Instantiate(logoPrefab, Vector3.zero, Quaternion.identity);
            _popUpSimulation.gameObject.name = $"{logoPrefab.name}_{_config.PlacementId}";
            
            _popUpSimulation.Init(_config, this);
            
            DontDestroyOnLoad(_popUpSimulation.gameObject);
        }

        private void RestartBidding()
        {
            if (_biddingRoutine != null)
                OdeeoMainThreadDispatcher.Instance.StopCoroutine(_biddingRoutine);
            
            _biddingRoutine = OdeeoMainThreadDispatcher.Instance.StartCoroutine(AdBiddingTimer());
        }
        
        private IEnumerator Timer()
        {
            int time = playLength;
            while (time > 0)
            {
                timerText.text = time.ToString();
                yield return new WaitForSeconds(1f);
                
                if(!_isPaused)
                    time--;
            }

            if (OdeeoAdManager.IsAdRewardedType(_config.PlacementType))
            {
                _popUpSimulation.RewardAvailable = true;
            }

            DestroyAd(OdeeoAdUnit.CloseReason.AdCompleted);
        }
        
        private IEnumerator AdBiddingTimer()
        {
            _logging.Info("Bidding started...");
            
            yield return new WaitForSeconds(2);

            _logging.Info("Bidding completed. Notifying...");

            OdeeoAdData data = _config.PlacementType.FromEditor<OdeeoAdData>(_config.CustomTag);
            _isAvailable = true;
            
            OnAvailabilityChanged?.Invoke(true, data);
        }
    }
}
#endif