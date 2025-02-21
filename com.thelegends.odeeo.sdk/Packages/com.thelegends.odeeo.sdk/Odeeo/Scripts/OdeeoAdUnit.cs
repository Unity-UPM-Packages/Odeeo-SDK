using System;
using Odeeo.Logging;
using Odeeo.Proxy;
using Odeeo.Scripts.Proxy;
using Odeeo.Utils;
using UnityEngine;

namespace Odeeo
{
    public class OdeeoAdUnit : IDisposable
    {
        public enum CloseReason
        {
            AdCompleted,
            AdExpired,
            UserClose,
            VolumeChanged,
            UserCancel,
            AdRemovedByDev,
            Other
        }
        
        public enum StateChangeReason
        {
            AdCovered,
            AdUncovered,
            RewardedVolumeMinimum,
            RewardedVolumeIncrease,
            ApplicationInBackground,
            ApplicationInForeground,
            AudioSessionInterruption,
            AudioSessionInterruptionEnd,
            OtherOdeeoPlacementStart,
            OtherOdeeoPlacementEnd
        }

        public enum ErrorShowReason
        {
            NoInternetConnection,
            AnotherAdPlaying,
            CurrentAdPlaying,
            NoAd,
            SdkNotInitialized,
            GeneralError,
            RectTransformBlocked
        }

        public enum ActionButtonPosition
        {
            TopRight,
            TopLeft
        }
        
        public enum PopUpType
        {
            IconPopUp,
            BannerPopUp
        }
        
        public IOdeeoAdUnitCallbacks AdCallbacks => _adProxy;

        public bool IsPlaying { get; private set; }

        private readonly IOdeeoLogging _logging;
        private readonly IOdeeoAdProxy _adProxy;
        private readonly OdeeoAdUnitConfig _odeeoAdUnitConfig;

        internal OdeeoAdUnit(OdeeoSdk.PlacementType adType, string placementID)
        {
            _logging = new OdeeoLogging();
            _odeeoAdUnitConfig = new OdeeoAdUnitConfig(adType, placementID);
            
            switch (Application.platform)
            {
                case RuntimePlatform.Android when !Application.isEditor:
#if UNITY_ANDROID
                    _adProxy = new OdeeoAdProxyAndroid(_odeeoAdUnitConfig, _logging);
#endif
                    break;
                case RuntimePlatform.IPhonePlayer when !Application.isEditor:
#if UNITY_IOS
                    _adProxy = new OdeeoAdProxyiOS(_odeeoAdUnitConfig, _logging);
#endif
                    break;
                default:
                {
                    if (Application.isEditor)
                    {
#if UNITY_EDITOR
                        _adProxy = new OdeeoAdProxyEditor(_odeeoAdUnitConfig, _logging);
#endif
                    }
                    else
                    {
                        _logging.Error("Unsupported platform. Only iOS and Android are supported at the moment.");
                    }

                    break;
                }
            }

            Debug.Assert(_adProxy != null, nameof(_adProxy) + " != null");
            
            _adProxy.OnClose += OnClose;
            _adProxy.OnShow += OnShow;
            _adProxy.OnMute += OnMute;
        }

        #region General

        internal void ShowAd()
        {
            RecalculatePositionAndSize();

            if (_adProxy.IsAdBlocked)
            {
                DispatchOnShowError(ErrorShowReason.RectTransformBlocked);
                TrackAdShowBlocked();
                _logging.Warning("Ad blocked. Rect transform size is smaller than the minimum ad size");
                return;
            }

            _adProxy.ShowAd();
        }

        internal void RemoveAd()
        {
            _adProxy.RemoveAd();
        }
        
        internal bool IsAdAvailable()
        {
            return !IsRectTransformBlocked() && _adProxy.IsAdAvailable();
        }

        internal bool IsAdCached()
        {
            return _adProxy.IsAdCached();
        }

        internal bool IsRectTransformBlocked()
        {
            if (_odeeoAdUnitConfig.LogoPosType != OdeeoAdUnitConfig.LogoPositionType.Rect) 
                return false;
            
            RecalculatePositionAndSize();
            return _adProxy.IsAdBlocked;
        }
        
        #endregion

        #region Positioning

        internal void SetBannerPosition(OdeeoSdk.BannerPosition position)
        {
            if (OdeeoAdManager.IsIconType(_adProxy.PlacementType))
            {
                _logging.Warning("SetBannerPosition can't be used with Icon ad type");
                return;
            }

            _odeeoAdUnitConfig.Position = (OdeeoSdk.IconPosition)position;
            _odeeoAdUnitConfig.BannerPopupPosition = position;
            
            _adProxy.SetBannerPosition(position);
        }
        
        internal void SetIconPosition(OdeeoSdk.IconPosition iconPosition, int xOffset, int yOffset)
        {
            if (OdeeoAdManager.IsBannerType(_adProxy.PlacementType))
            {
                _logging.Error("SetIconPosition can't be used with Banner ad type");
                return;
            }

            _odeeoAdUnitConfig.LogoPosType = OdeeoAdUnitConfig.LogoPositionType.Direct;

            _odeeoAdUnitConfig.Position = iconPosition;
            _odeeoAdUnitConfig.IconPopupPosition = iconPosition;
            _odeeoAdUnitConfig.Offset = (xOffset, yOffset);
        }
        
        internal void LinkIconToRectTransform(OdeeoSdk.IconPosition iconPosition, RectTransform rectTransform, Canvas canvas, OdeeoSdk.AdSizingMethod sizingMethod)
        {
            if (OdeeoAdManager.IsBannerType(_adProxy.PlacementType))
            {
                _logging.Warning("LinkIconToRectTransform can't be used with Banner ad type");
                return;
            }

            _odeeoAdUnitConfig.LogoPosType = OdeeoAdUnitConfig.LogoPositionType.Rect;
            _odeeoAdUnitConfig.LinkedRect = rectTransform;
            _odeeoAdUnitConfig.LinkedCanvas = canvas;
            _odeeoAdUnitConfig.LinkedPosition = iconPosition;
            _odeeoAdUnitConfig.SizingMethod = sizingMethod;
            
            RecalculatePositionAndSize();
        }

        private void CalculateLinkToRectTransform()
        {
            if (!_odeeoAdUnitConfig.LinkedRect)
            {
                _logging.Error("LinkIconToRectTransform function error. RectTransform is null");
                return;
            }

            if (!_odeeoAdUnitConfig.LinkedCanvas)
            {
                _logging.Error("LinkIconToRectTransform function error. Canvas is null");
                return;
            }

            Rect rect = OdeeoRectHelper.GetScreenRect(_odeeoAdUnitConfig.LinkedRect, _odeeoAdUnitConfig.LinkedCanvas);
            rect = OdeeoRectHelper.LimitRectToScreen(rect);
            
            float resolutionHeightFactor = (float)Display.main.systemHeight/(float)Screen.height;
            float resolutionWidthFactor = (float)Display.main.systemWidth/(float)Screen.width;
            
            Vector2 newPosition = rect.position;
            newPosition.x *= OdeeoDpiResolution.GetDpiMultiplier() * resolutionWidthFactor;
            newPosition.y *= OdeeoDpiResolution.GetDpiMultiplier() * resolutionHeightFactor;
            rect.position = newPosition;
            
            Vector2 newSize = rect.size;
            newSize.x *= OdeeoDpiResolution.GetDpiMultiplier() * resolutionWidthFactor;
            newSize.y *= OdeeoDpiResolution.GetDpiMultiplier() * resolutionHeightFactor;
            rect.size = newSize;
            
            _adProxy.IsAdBlocked = true;

            float bestSize = OdeeoAdUnitConfig.AD_SIZE_LIMIT_DP_MAX;
            Vector2 positinPx = Vector2.zero;
            int step = 5;
            for (int i = OdeeoAdUnitConfig.AD_SIZE_LIMIT_DP_MAX; i >= OdeeoAdUnitConfig.AD_SIZE_LIMIT_DP_MIN; i -= step)
            {
                bestSize = i;
                int sizeInPx = (int)(i * OdeeoDpiResolution.GetDeviceScale());
                positinPx = OdeeoRectHelper.ConvertRectToPosition(rect, _odeeoAdUnitConfig.LinkedPosition, sizeInPx);
                Rect adRect = new Rect(positinPx, new Vector2(sizeInPx, sizeInPx));
                if (OdeeoRectHelper.IsRectContainsRect(adRect, rect))
                {
                    _adProxy.IsAdBlocked = false;
                    break;
                }
            }

            switch (_odeeoAdUnitConfig.SizingMethod)
            {
                case OdeeoSdk.AdSizingMethod.Flexible:
                    _adProxy.IsAdBlocked = false; //using smallest icon, unblocking show
                    break;
                case OdeeoSdk.AdSizingMethod.Strict:
                    break;
            }

            Vector2 positionDp = OdeeoRectHelper.PixelPositionToDp(positinPx);

            _odeeoAdUnitConfig.Position = OdeeoSdk.IconPosition.BottomLeft;
            _odeeoAdUnitConfig.Offset = ((int)positionDp.x, (int)positionDp.y);
            _odeeoAdUnitConfig.Size =  ((int)bestSize, (int)bestSize);
        }
        
        internal void LinkToIconAnchor(OdeeoIconAnchor iconAnchor)
        {
            if (OdeeoAdManager.IsBannerType(_adProxy.PlacementType))
            {
                _logging.Warning("LinkToIconAnchor can't be used with Banner ad type");
                return;
            }

            _odeeoAdUnitConfig.LogoPosType = OdeeoAdUnitConfig.LogoPositionType.Anchor;
            _odeeoAdUnitConfig.LinkedAnchor = iconAnchor;

            RecalculatePositionAndSize();
        }

        private void CalculateLinkToPrefab()
        {
            if (!_odeeoAdUnitConfig.LinkedAnchor)
            {
                _logging.Error("LinkToIconAnchor function error. IconAnchor is NULL");
                return;
            }

            RectTransform rt = _odeeoAdUnitConfig.LinkedAnchor.RectTransform;
            Canvas canvas = _odeeoAdUnitConfig.LinkedAnchor.Canvas;

            if (canvas == null || rt == null)
            {
                _logging.Error("LinkToIconAnchor function error. IconAnchor Integrated incorrectly");
                return;
            }

            float multiplier = OdeeoDpiResolution.GetDpiMultiplier();
            float resolutionHeightFactor = (float)Display.main.systemHeight/(float)Screen.height;
            float resolutionWidthFactor = (float)Display.main.systemWidth/(float)Screen.width;
            
            Rect rect = OdeeoRectHelper.GetScreenRect(rt, canvas);
            float s = rt.sizeDelta.x * canvas.scaleFactor * multiplier * resolutionWidthFactor;
            
            Vector2 positionPx = OdeeoRectHelper.ConvertRectToPosition(rect, OdeeoSdk.IconPosition.BottomLeft, (int)s);
            positionPx.x *= multiplier * resolutionWidthFactor;
            positionPx.y *= multiplier * resolutionHeightFactor;
            
            Vector2 positionDp = OdeeoRectHelper.PixelPositionToDp(positionPx);
            
            _odeeoAdUnitConfig.Position = OdeeoSdk.IconPosition.BottomLeft;
            _odeeoAdUnitConfig.Offset = ((int)positionDp.x, (int)positionDp.y);
            
            int size = (int)(s / OdeeoDpiResolution.GetDeviceScale());
            _odeeoAdUnitConfig.Size = (size, size);
        }

        internal void SetIconSize(int size)
        {
            _odeeoAdUnitConfig.Size = (size, size);
        }

        private void RecalculatePositionAndSize()
        {
            _adProxy.IsAdBlocked = false;
            
            switch (_odeeoAdUnitConfig.LogoPosType)
            {
                case OdeeoAdUnitConfig.LogoPositionType.Direct:
                    break;
                case OdeeoAdUnitConfig.LogoPositionType.Anchor:
                    CalculateLinkToPrefab();
                    break;
                case OdeeoAdUnitConfig.LogoPositionType.Rect:
                    CalculateLinkToRectTransform();
                    break;
            }
        }

        #endregion

        #region RewardedPopUp

        internal void SetRewardedPopupType(PopUpType type)
        {
            _odeeoAdUnitConfig.PopupType = type;
            _adProxy.SetRewardedPopupType(type);
        }

        internal void SetRewardedPopupBannerPosition(OdeeoSdk.BannerPosition position)
        {
            _odeeoAdUnitConfig.BannerPopupPosition = position;
            _adProxy.SetRewardedPopupBannerPosition(position);
        }

        internal void SetRewardedPopupIconPosition(OdeeoSdk.IconPosition position, int xOffset, int yOffset)
        {
            _odeeoAdUnitConfig.IconPopupPosition = position;

            _odeeoAdUnitConfig.PopupOffset = (xOffset, yOffset);

            _adProxy.SetRewardedPopupIconPosition(position, xOffset, yOffset);
        }

        #endregion

        #region VisualSettings
        
        internal void SetProgressBarColor(Color progressBarColor)
        {
            _adProxy.SetProgressBarColor(progressBarColor);
        }
        
        internal void SetAudioOnlyBackgroundColor(Color color)
        {
            _adProxy.SetAudioOnlyBackgroundColor(color);
        }

        internal void SetAudioOnlyAnimationColor(Color color)
        {
            _adProxy.SetAudioOnlyAnimationColor(color);
        }

        #endregion

        #region ActionButton

        internal void SetIconActionButtonPosition(ActionButtonPosition position)
        {
            if (OdeeoAdManager.IsBannerType(_adProxy.PlacementType))
            {
                _logging.Warning("SetIconActionButtonPosition can't be used with Banner ad type");
                return;
            }

            if (OdeeoAdManager.IsAdRewardedType(_adProxy.PlacementType))
            {
                _logging.Warning("SetIconActionButtonPosition can't be used with Rewarded ad type");
                return;
            }

            _odeeoAdUnitConfig.ActionButtonPosition = position;
            
            _adProxy.SetIconActionButtonPosition(position);
        }

        #endregion

        #region Events
        
        private void OnShow()
        {
            IsPlaying = true;
            _odeeoAdUnitConfig.IsGameMuted = true;
            
            UpdateVolume();
            
            _logging.Info("Ad OnShow");
        }

        private void OnClose(CloseReason reason)
        {
            IsPlaying = false;
            _odeeoAdUnitConfig.IsGameMuted = false;
            
            UpdateVolume();
            
            _logging.Info("Ad OnClose");
        }

        private void OnMute(bool isMuted)
        {
            _odeeoAdUnitConfig.IsGameMuted = !isMuted;
            UpdateVolume();
        }
        
        #endregion

        internal void SetCustomTag(string tag)
        {
            _adProxy.SetCustomTag(tag);
        }
        
        internal void TrackRewardedOffer()
        {
            _adProxy.TrackRewardedOffer();
        }
        
        private void TrackAdShowBlocked()
        {
            _adProxy.TrackAdShowBlocked();
        }

        internal void DispatchOnShowError(ErrorShowReason reason, string customMessage = null)
        {
            _adProxy.DispatchOnShowError(reason, customMessage);
        }

        private void UpdateVolume()
        {
            if (_odeeoAdUnitConfig.IsGameMuted)
            {
                _odeeoAdUnitConfig.SceneVolumeValue = AudioListener.volume;
                AudioListener.volume = OdeeoAdUnitConfig.FADE_VALUE;
                return;
            }
            
            AudioListener.volume = _odeeoAdUnitConfig.SceneVolumeValue;
        }
        
        public void Dispose()
        {
            _adProxy.OnClose -= OnClose;
            _adProxy.OnShow -= OnShow;
            _adProxy.OnMute -= OnMute;
            
            _adProxy.Dispose();
        }
    }
}