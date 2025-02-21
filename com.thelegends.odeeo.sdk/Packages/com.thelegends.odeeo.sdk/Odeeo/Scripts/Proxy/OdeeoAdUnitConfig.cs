using System;
using System.Collections.Generic;
using UnityEngine;

namespace Odeeo
{
    internal sealed class OdeeoAdUnitConfig
    {
        internal enum LogoPositionType
        {
            Anchor,
            Rect,
            Direct
        }
        
        internal const int AD_SIZE_LIMIT_DP_MIN = 70;
        internal const int AD_SIZE_LIMIT_DP_MAX = 120;
        internal const float FADE_VALUE = 0.1f;
        
        internal readonly OdeeoSdk.PlacementType PlacementType;
        internal readonly string PlacementId;
        
        internal OdeeoSdk.IconPosition Position;
        internal OdeeoSdk.IconPosition LinkedPosition;
        internal (int x, int y) Offset = (10, 10);
        internal (int x, int y) Size = (80, 80);
        
        // Rect transform positioning
        internal LogoPositionType LogoPosType;
        internal OdeeoSdk.AdSizingMethod SizingMethod;
        internal OdeeoIconAnchor LinkedAnchor;
        internal RectTransform LinkedRect;
        internal Canvas LinkedCanvas;
        
        // Popup settings
        internal OdeeoAdUnit.PopUpType PopupType;
        internal (int x, int y) PopupOffset;
        internal OdeeoSdk.IconPosition IconPopupPosition;
        internal OdeeoSdk.BannerPosition BannerPopupPosition;
        internal OdeeoAdUnit.ActionButtonPosition ActionButtonPosition;
        
        internal float SceneVolumeValue = 1f;
        internal string CustomTag;
        internal bool IsGameMuted;
        
        private static readonly Dictionary<OdeeoAdUnit.ErrorShowReason, string> _errorMessages = new Dictionary<OdeeoAdUnit.ErrorShowReason, string>
        {
            { OdeeoAdUnit.ErrorShowReason.NoInternetConnection, "Internet connection missing"},
            { OdeeoAdUnit.ErrorShowReason.AnotherAdPlaying, "Unable to simultaneously play two different ad units"},
            { OdeeoAdUnit.ErrorShowReason.CurrentAdPlaying, "Current ad already playing"},
            { OdeeoAdUnit.ErrorShowReason.NoAd, "No ad to play"},
            { OdeeoAdUnit.ErrorShowReason.SdkNotInitialized, "SDK not Initialized"},
            { OdeeoAdUnit.ErrorShowReason.GeneralError, "General error"},
            { OdeeoAdUnit.ErrorShowReason.RectTransformBlocked, "Ad show failed due to RectTransform size blocking"}
        };

        internal OdeeoAdUnitConfig(OdeeoSdk.PlacementType placementType, string placementId)
        {
            PlacementType = placementType;
            PlacementId = placementId;
            
            Position = OdeeoSdk.IconPosition.BottomCenter;
            LogoPosType = LogoPositionType.Direct;
            PopupOffset = Offset;

            bool isBannerPlacement = PlacementType == OdeeoSdk.PlacementType.RewardedAudioBannerAd ||
                                     PlacementType == OdeeoSdk.PlacementType.AudioBannerAd;

            if (isBannerPlacement)
                BannerPopupPosition = (OdeeoSdk.BannerPosition)Position;
            else
                IconPopupPosition = Position;

            PopupType = isBannerPlacement ? OdeeoAdUnit.PopUpType.BannerPopUp : OdeeoAdUnit.PopUpType.IconPopUp;
        }

        internal string ErrorMessageBy(OdeeoAdUnit.ErrorShowReason reason)
        {
            return _errorMessages.TryGetValue(reason, out var result) ? result : "Unknown...";
        }
    }
}